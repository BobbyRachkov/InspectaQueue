using Confluent.SchemaRegistry; 
using Confluent.SchemaRegistry.Serdes; // Required for Avro Deserializer
using Newtonsoft.Json; // Required for converting Avro object to JSON
using Pulsar.Client.Api;
using Pulsar.Client.Common;
using Rachkov.InspectaQueue.Abstractions.Messaging.Interfaces;
using Rachkov.InspectaQueue.Abstractions.Messaging.Models;
using Rachkov.InspectaQueue.Abstractions.Notifications.Errors;
using Rachkov.InspectaQueue.Abstractions.Notifications.ProgressStatus;
using Rachkov.InspectaQueue.Providers.Pulsar.Extensions;
using System.Diagnostics;
using System.Text;

namespace Rachkov.InspectaQueue.Providers.Pulsar;

public class PulsarAvroConsumer : IQueueProvider
{
    private readonly IErrorReporter _errorReporter;
    private Task? _readerTask;
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly PulsarAvroConsumerSettings _settings;
    private PulsarClient? _client;
    private IConsumer<byte[]>? _consumer;
    private ISchemaRegistryClient? _schemaRegistryClient;
    private AvroDeserializer<object>? _avroDeserializer; // Use 'object' for generic deserialization

    public PulsarAvroConsumer(IErrorReporter errorReporter)
    {
        _errorReporter = errorReporter;
        Debug.WriteLine($"==========> Constructing Avro Consumer: {InstanceId}");
        _settings = new PulsarAvroConsumerSettings();
    }

    ~PulsarAvroConsumer()
    {
        Debug.WriteLine($"==========> Destructing Avro Consumer: {InstanceId}");
        // Ensure cleanup happens
        DisconnectSubscriber().Wait(); 
    }

    public Guid InstanceId { get; } = Guid.NewGuid();

    public IProviderDetails Details { get; } = new ProviderDetails
    {
        Name = "Pulsar Consumer (Avro)",
        Description = "Consumer with subscription name and Avro deserialization.",
        Type = QueueType.Pulsar,
        PackageVendorName = "InspectaQueue"
    };

    public IQueueProviderSettings Settings => _settings;

    #region Consumer

    public Task Connect(IMessageReceiver messageReceiver, IProgressNotificationService progressNotificationService)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _readerTask = Task.Run(() => ReadAsync(messageReceiver, progressNotificationService, _cancellationTokenSource.Token));

        return Task.CompletedTask;
    }

    public async Task DisconnectSubscriber()
    {
        if (_cancellationTokenSource is not null)
        {
            await _cancellationTokenSource.CancelAsync();
        }
        // Reading task completion and disposal is handled in ReadAsync finally block
    }

    public async Task<bool> TryAcknowledge(IInboundMessage message)
    {
        if (message.Message is not Message<byte[]> messageObject
            || _consumer is null)
        {
            return false;
        }

        await _consumer.AcknowledgeAsync(messageObject.MessageId);
        message.IsAcknowledged = true;
        return true;
    }

    private async Task ReadAsync(
        IMessageReceiver messageReceiver,
        IProgressNotificationService progressNotificationService,
        CancellationToken cancellationToken)
    {
        long messagesReceived = 0, messagesProcessed = 0;
        try
        {
            await progressNotificationService.SendProgressUpdateNotification(new ProgressNotification(Constants.StatusMessage.Connecting, Status.InProgress));

            _client = await new PulsarClientBuilder()
                .ServiceUrl(_settings.ServiceUrl)
                // Add authentication if needed, same as PulsarConsumer
                .Authentication(AuthenticationFactoryOAuth2.ClientCredentials(
                    new Uri(_settings.IssuerUrl),
                    _settings.Audience,
                    new Uri(_settings.FilePath)))
                .BuildAsync();

            _consumer = await _client.NewConsumer() // Still consume byte[]
                .Topic(_settings.TopicName)
                .SubscriptionName(_settings.SubscriptionName)
                .SubscriptionType(_settings.SubscriptionType.ToPulsarEnum())
                .SubscriptionInitialPosition(_settings.SubscriptionInitialPosition.ToPulsarEnum())
                .SubscribeAsync();

            // Initialize Schema Registry Client and Deserializer if URL is provided
            if (!string.IsNullOrEmpty(_settings.SchemaRegistryUrl))
            {
                var schemaRegistryConfig = new SchemaRegistryConfig { Url = _settings.SchemaRegistryUrl };
                // Add authentication to schema registry if needed here
                _schemaRegistryClient = new CachedSchemaRegistryClient(schemaRegistryConfig);
                _avroDeserializer = new AvroDeserializer<object>(_schemaRegistryClient);
            }
            else
            {
                 await progressNotificationService.SendProgressUpdateNotification(new ProgressNotification("Schema Registry URL not configured. Will display raw bytes.", Status.Warning));
            }

            cancellationToken.ThrowIfCancellationRequested();
        }
        catch (Exception e)
        {
            if (e is not OperationCanceledException)
            {
                _errorReporter.RaiseError(new() { Text = "Error while initializing Pulsar Avro client/consumer", Source = this, Exception = e });
            }
            await progressNotificationService.SendProgressUpdateNotification(new ProgressNotification(Constants.StatusMessage.Failed, Status.Failed));
            await DisposeConsumerAndClient();
            return;
        }

        await progressNotificationService.SendProgressUpdateNotification(new ProgressNotification(Constants.StatusMessage.Connected, Status.Ok));

        var filterByKeyEnabled = !string.IsNullOrEmpty(_settings.FilterByKey);

        while (!cancellationToken.IsCancellationRequested)
        {
            string messageContent = string.Empty;
            string jsonRepresentation = string.Empty;
            Message<byte[]>? message = null;
            bool deserializationError = false;

            try
            {
                message = await _consumer.ReceiveAsync(cancellationToken);
                messagesReceived++;

                if (filterByKeyEnabled && message.Key is not null && !message.Key.Contains(_settings.FilterByKey))
                {
                    // Skip message and update progress
                    await progressNotificationService.SendProgressUpdateNotification(new ProgressNotification(messagesReceived, messagesProcessed, Constants.StatusMessage.Connected, Status.Ok));
                    if (_settings.AcknowledgeOnReceive)
                    {
                         await _consumer.AcknowledgeAsync(message.MessageId);
                    }
                    continue;
                }

                // Attempt Avro Deserialization if configured
                if (_avroDeserializer is not null)
                {
                    try
                    {
                        var deserializationContext = new DeserializationContext(MessageComponentType.Value, _settings.TopicName); // Assuming schema is based on topic name convention
                        var avroObject = await _avroDeserializer.DeserializeAsync(message.Data, message.Data is null, deserializationContext);
                        // Convert Avro object to JSON for display
                        messageContent = JsonConvert.SerializeObject(avroObject, Formatting.Indented);
                        jsonRepresentation = messageContent; 
                    }
                    catch (Exception avroEx)
                    {
                         _errorReporter.RaiseError(new() { Text = $"Avro deserialization failed for message {message.MessageId}: {avroEx.Message}", Source = this, Exception = avroEx });
                         messageContent = "<< AVRO DESERIALIZATION FAILED >>\n" + Convert.ToBase64String(message.Data);
                         jsonRepresentation = "{\"error\": \"Avro deserialization failed\"}";
                         deserializationError = true;
                         // Decide if you want to stop processing or just flag the message
                    }
                }
                else
                {
                    // Fallback to UTF8 string or Base64 if not Avro or not configured
                     try { 
                        messageContent = Encoding.UTF8.GetString(message.Data);
                        jsonRepresentation = messageContent; // Assume it might be JSON
                     } catch { 
                        messageContent = Convert.ToBase64String(message.Data); // If not valid UTF8
                        jsonRepresentation = "{\"data_base64\": \"" + messageContent + "\"}";
                     } 
                }

                var frame = new InboundMessageFrame
                {
                    Content = messageContent,
                    JsonRepresentation = jsonRepresentation,
                    Message = message,
                    Key = message.Key,
                    Id = message.MessageId.EntryId.ToString(),
                    IsAcknowledged = false // Will be set if auto-ack is on
                };

                await messageReceiver.SendMessageAsync(frame);
                messagesProcessed++;

                if (_settings.AcknowledgeOnReceive && !deserializationError) // Optionally avoid ack on error
                {
                    await _consumer.AcknowledgeAsync(message.MessageId);
                    frame.IsAcknowledged = true;
                }

                await progressNotificationService.SendProgressUpdateNotification(new ProgressNotification(messagesReceived, messagesProcessed, Constants.StatusMessage.Connected, Status.Ok));
            }
            catch (Exception e)
            {
                if (e is not OperationCanceledException)
                {
                    _errorReporter.RaiseError(new() { Text = "Error while reading message", Source = this, Exception = e });
                    await progressNotificationService.SendProgressUpdateNotification(new ProgressNotification(messagesReceived, messagesProcessed, Constants.StatusMessage.Failed, Status.Failed));
                }
                 // Consider breaking the loop on critical errors
                 if (e is OperationCanceledException) break;
            }
        }

        await progressNotificationService.SendProgressUpdateNotification(new ProgressNotification(messagesReceived, messagesProcessed, Constants.StatusMessage.Disconnected, Status.Ok));
        await DisposeConsumerAndClient();

         if (_cancellationTokenSource is not null)
         {
             _cancellationTokenSource.Dispose();
             _cancellationTokenSource = null;
         }
    }

    private async Task DisposeConsumerAndClient()
    {
        if (_consumer is not null)
        {
            try
            {
                await _consumer.DisposeAsync();
                _consumer = null;
            }
            catch (Exception ex)
            {
                _errorReporter.RaiseError(new() { Text = "Error disposing Pulsar consumer", Source = this, Exception = ex });
            }
        }

        if (_client is not null)
        {
            try
            {
                await _client.DisposeAsync();
                _client = null;
            }
            catch (Exception ex)
            {
                _errorReporter.RaiseError(new() { Text = "Error disposing Pulsar client", Source = this, Exception = ex });
            }
        }

         // Dispose Schema Registry Client if it was created
        (_schemaRegistryClient as IDisposable)?.Dispose();
         _schemaRegistryClient = null;
         _avroDeserializer = null; // No explicit dispose needed for deserializer itself typically
    }

    #endregion
}
