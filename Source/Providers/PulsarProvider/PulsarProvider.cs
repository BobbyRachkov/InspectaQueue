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

public class PulsarProvider : IQueueProvider
{
    private readonly IErrorReporter _errorReporter;
    private Task? _readerTask;
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly PulsarSettings _settings;
    private PulsarClient? _client;
    private IConsumer<byte[]>? _consumer;

    public PulsarProvider(IErrorReporter errorReporter)
    {
        _errorReporter = errorReporter;
        Debug.WriteLine($"==========> Constructing: {InstanceId}");
        _settings = new PulsarSettings();
    }

    ~PulsarProvider()
    {
        Debug.WriteLine($"==========> Destructing: {InstanceId}");
    }

    public Guid InstanceId { get; } = Guid.NewGuid();

    public IProviderDetails Details { get; } = new ProviderDetails
    {
        Name = "Pulsar Consumer",
        Description = "Consumer with subscription name and cursor",
        Type = QueueType.Pulsar,
        PackageVendorName = "InspectaQueue"
    };

    public IQueueProviderSettings Settings => _settings;

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
        try
        {
            await progressNotificationService.SendProgressUpdateNotification(new ProgressNotification("Connecting...", Status.InProgress));

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

            await progressNotificationService.SendProgressUpdateNotification(new ProgressNotification("Failed", Status.Failed));

            await DisposeConsumerAndClient();

            return;
        }

        await progressNotificationService.SendProgressUpdateNotification(new ProgressNotification("Connected", Status.Ok));

        var filterByKeyEnabled = !string.IsNullOrEmpty(_settings.FilterByKey);
        long messagesReceived = 0, messagesProcessed = 0;
        string receivingMessage = "Receiving";

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
                        receivingMessage,
                        Status.Ok));

                    if (_settings.AcknowledgeOnReceive)
                    {
                        await _consumer.AcknowledgeAsync(message.MessageId);
                    }

                    continue;
                }

                var messageString = Encoding.UTF8.GetString(message.Data);
                var frame = new InboundMessageFrame
                {
                    Content = messageString,
                    JsonRepresentation = messageString,
                    Message = message,
                    Key = message.Key,
                    Id = message.MessageId.EntryId.ToString()
                };

                await messageReceiver.SendMessageAsync(frame);
                messagesProcessed++;

                if (_settings.AcknowledgeOnReceive)
                {
                    await _consumer.AcknowledgeAsync(message.MessageId);
                    frame.IsAcknowledged = true;
                }

                await progressNotificationService.SendProgressUpdateNotification(new ProgressNotification(
                    messagesReceived,
                    messagesProcessed,
                    receivingMessage,
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
                        "Failed",
                        Status.Failed));
                }
            }
        }

        await progressNotificationService.SendProgressUpdateNotification(new ProgressNotification(
            messagesReceived,
            messagesProcessed,
            "Disconnecting",
            Status.InProgress));

        await DisposeConsumerAndClient();

        await progressNotificationService.SendProgressUpdateNotification(new ProgressNotification(
            messagesReceived,
            messagesProcessed,
            "Disconnected",
            Status.Ok));
    }

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