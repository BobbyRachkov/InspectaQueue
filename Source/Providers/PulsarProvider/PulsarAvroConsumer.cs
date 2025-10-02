using Chr.Avro.Representation;
using Chr.Avro.Serialization;
using Newtonsoft.Json;
using Pulsar.Client.Api;
using Pulsar.Client.Common;
using Rachkov.InspectaQueue.Abstractions.Messaging.Interfaces;
using Rachkov.InspectaQueue.Abstractions.Messaging.Models;
using Rachkov.InspectaQueue.Abstractions.Notifications.Errors;
using Rachkov.InspectaQueue.Abstractions.Notifications.ProgressStatus;
using Rachkov.InspectaQueue.Providers.Pulsar.Extensions;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;
using BinaryReader = Chr.Avro.Serialization.BinaryReader;

namespace Rachkov.InspectaQueue.Providers.Pulsar;

public class PulsarAvroConsumer : IQueueProvider, ICanAcknowledge
{
    private readonly IErrorReporter _errorReporter;
    private Task? _readerTask;
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly PulsarAvroConsumerSettings _settings;
    private PulsarClient? _client;
    private IConsumer<byte[]>? _consumer;

    private static readonly HttpClient HttpClient = new();
    private static readonly ConcurrentDictionary<string, SchemaEntry> SchemaCache = new();

    public PulsarAvroConsumer(IErrorReporter errorReporter)
    {
        _errorReporter = errorReporter;
        Debug.WriteLine($"==========> Constructing: {InstanceId}");
        _settings = new PulsarAvroConsumerSettings();
    }

    ~PulsarAvroConsumer()
    {
        Debug.WriteLine($"==========> Destructing: {InstanceId}");
    }

    public Guid InstanceId { get; } = Guid.NewGuid();

    public IProviderDetails Details { get; } = new ProviderDetails
    {
        Name = "Pulsar Consumer (Avro)",
        Description = "Consumer with subscription name, cursor, publishing and AVRO deserialization capabilities.",
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
    }

    private async Task ReadAsync(
        IMessageReceiver messageReceiver,
        IProgressNotificationService progressNotificationService,
        CancellationToken cancellationToken)
    {
        try
        {
            await progressNotificationService.SendProgressUpdateNotification(new ProgressNotification(Constants.StatusMessage.Connecting, Status.InProgress));

            _client = await new PulsarClientBuilder()
                .ServiceUrl(_settings.ServiceUrl)
                .Authentication(AuthenticationFactoryOAuth2.ClientCredentials(
                    new Uri(_settings.IssuerUrl),
                    _settings.Audience,
                    new Uri(_settings.FilePath)))
                .BuildAsync();

            _consumer = await _client.NewConsumer()
                .Topic(_settings.TopicName)
                .SubscriptionName(_settings.SubscriptionName)
                .SubscriptionType(_settings.SubscriptionType.ToPulsarEnum())
                .SubscriptionInitialPosition(_settings.SubscriptionInitialPosition.ToPulsarEnum())
                .SubscribeAsync();

            cancellationToken.ThrowIfCancellationRequested();
        }
        catch (Exception e)
        {
            if (e is not OperationCanceledException)
            {
                _errorReporter.RaiseError(new()
                {
                    Text = "Error while initializing Pulsar client",
                    Source = this,
                    Exception = e
                });
            }

            await progressNotificationService.SendProgressUpdateNotification(new ProgressNotification(Constants.StatusMessage.Failed, Status.Failed));

            await DisposeConsumerAndClient();

            return;
        }

        await progressNotificationService.SendProgressUpdateNotification(new ProgressNotification(Constants.StatusMessage.Connected, Status.Ok));

        var filterByKeyEnabled = !string.IsNullOrEmpty(_settings.FilterByKey);
        long messagesReceived = 0, messagesProcessed = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var message = await _consumer.ReceiveAsync(cancellationToken);
                messagesReceived++;

                if (filterByKeyEnabled && !message.Key.Contains(_settings.FilterByKey))
                {
                    await progressNotificationService.SendProgressUpdateNotification(new ProgressNotification(
                        messagesReceived,
                        messagesProcessed,
                        Constants.StatusMessage.Connected,
                        Status.Ok));

                    if (_settings.AcknowledgeOnReceive)
                    {
                        await _consumer.AcknowledgeAsync(message.MessageId);
                    }

                    continue;
                }

                var (messageString, jsonRepresentation) = await GetMessageContent(message);

                var frame = new InboundMessageFrame
                {
                    Content = messageString,
                    JsonRepresentation = jsonRepresentation,
                    Message = message,
                    Key = message.Key,
                    Id = message.MessageId.EntryId.ToString(),
                };

                await messageReceiver.SendMessageAsync(frame);
                messagesProcessed++;

                if (_settings.AcknowledgeOnReceive)
                {
                    await _consumer.AcknowledgeAsync(message.MessageId);
                    frame.AcknowledgedStatus = AcknowledgeStatus.Acknowledged;
                }

                await progressNotificationService.SendProgressUpdateNotification(new ProgressNotification(
                    messagesReceived,
                    messagesProcessed,
                    Constants.StatusMessage.Connected,
                    Status.Ok));
            }
            catch (Exception e)
            {
                if (e is not OperationCanceledException)
                {
                    _errorReporter.RaiseError(new()
                    {
                        Text = "Error while reading message",
                        Source = this,
                        Exception = e
                    });

                    await progressNotificationService.SendProgressUpdateNotification(new ProgressNotification(
                        messagesReceived,
                        messagesProcessed,
                        Constants.StatusMessage.Failed,
                        Status.Failed));
                }
            }
        }

        await progressNotificationService.SendProgressUpdateNotification(new ProgressNotification(
            messagesReceived,
            messagesProcessed,
            Constants.StatusMessage.Disconnecting,
            Status.InProgress));

        await DisposeConsumerAndClient();

        await progressNotificationService.SendProgressUpdateNotification(new ProgressNotification(
            messagesReceived,
            messagesProcessed,
            Constants.StatusMessage.Disconnected,
            Status.Ok));
    }

    private async Task<(string message, string json)> GetMessageContent(Message<byte[]> message)
    {
        if (message.SchemaVersion is null || message.SchemaVersion.Length == 0)
        {
            var plainText = Encoding.UTF8.GetString(message.Data);
            return (plainText, plainText);
        }

        try
        {
            if (!SchemaCache.TryGetValue(_settings.SchemaCacheKey, out var schemaEntry))
            {
                var schemaInfo = await FetchSchema(message);
                if (schemaInfo is null) return (Encoding.UTF8.GetString(message.Data), Encoding.UTF8.GetString(message.Data));

                var schema = new JsonSchemaReader().Read(schemaInfo.Data.SchemaDefinition);
                var deserializeDelegate = new BinaryDeserializerBuilder().BuildDelegate<object>(schema);
                schemaEntry = new SchemaEntry
                {
                    Schema = schema,
                    Deserializer = (data) =>
                    {
                        var reader = new BinaryReader(data.AsSpan());
                        return deserializeDelegate(ref reader);
                    }
                };
                SchemaCache[_settings.SchemaCacheKey] = schemaEntry;
            }

            // Strip schema version from payload before deserializing
            var payload = message.Data.AsSpan().Slice(10).ToArray();
            var genericObject = schemaEntry.Deserializer(payload);
            var json = JsonConvert.SerializeObject(genericObject, Formatting.Indented);
            return (json, json);
        }
        catch (Exception ex)
        {
            _errorReporter.RaiseError(new() { Text = "Failed to deserialize Avro message", Source = this, Exception = ex });
            return ("<Could not deserialize Avro message>", "{\"error\": \"Could not deserialize Avro message\"}");
        }
    }

    private async Task<PulsarSchemaResponse?> FetchSchema(Message<byte[]> message)
    {
        var topic = _settings.TopicName;
        var schemaVersion = BitConverter.ToInt64(message.SchemaVersion, 0);
        var url = _settings.SchemaRegistryUrl;

        try
        {
            return await HttpClient.GetFromJsonAsync<PulsarSchemaResponse>(url);
        }
        catch (Exception ex)
        {
            _errorReporter.RaiseError(new() { Text = $"Failed to fetch Avro schema from {url}", Source = this, Exception = ex });
            return null;
        }
    }

    public async Task<bool> TryAcknowledge(IEnumerable<IInboundMessage> messages)
    {
        var hasUnacknowledged = false;

        foreach (var inboundMessage in messages)
        {
            if (inboundMessage.Message is not Message<byte[]> messageObject
                || _consumer is null)
            {
                hasUnacknowledged = true;
                continue;
            }

            await _consumer.AcknowledgeAsync(messageObject.MessageId);
            inboundMessage.AcknowledgedStatus = AcknowledgeStatus.Acknowledged;
        }

        return !hasUnacknowledged;
    }

    public async Task<bool> TryNegativeAcknowledge(IEnumerable<IInboundMessage> messages)
    {
        var hasUnacknowledged = false;

        foreach (var inboundMessage in messages)
        {
            if (inboundMessage.Message is not Message<byte[]> messageObject
                || _consumer is null)
            {
                continue;
            }

            _consumer.NegativeAcknowledge(messageObject.MessageId);
            inboundMessage.AcknowledgedStatus = AcknowledgeStatus.NegativeAcknowledged;
        }

        return !hasUnacknowledged;
    }

    private sealed class PulsarSchemaResponse
    {
        [JsonProperty("data")]
        public PulsarSchemaData Data { get; set; } = new();
    }

    private sealed class PulsarSchemaData
    {
        [JsonProperty("schema")]
        public string SchemaDefinition { get; set; } = string.Empty;
    }

    private sealed class SchemaEntry
    {
        public Chr.Avro.Abstract.Schema Schema { get; set; } = null!;
        public Func<byte[], object> Deserializer { get; set; } = null!;
    }

    #endregion

    public async ValueTask DisposeAsync()
    {
        Debug.WriteLine($"==========> Disposing: {InstanceId}");
        await DisconnectSubscriber();
    }

    private async Task DisposeConsumerAndClient()
    {
        if (_readerTask?.IsCompleted == true)
        {
            _readerTask?.Dispose();
        }

        if (_consumer is not null)
        {
            await _consumer.DisposeAsync();
        }

        if (_client is not null)
        {
            await _client.CloseAsync();
        }
    }
}
