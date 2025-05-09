﻿using Pulsar.Client.Api;
using Pulsar.Client.Common;
using Rachkov.InspectaQueue.Abstractions.Messaging.Interfaces;
using Rachkov.InspectaQueue.Abstractions.Messaging.Models;
using Rachkov.InspectaQueue.Abstractions.Notifications.Errors;
using Rachkov.InspectaQueue.Abstractions.Notifications.ProgressStatus;
using Rachkov.InspectaQueue.Providers.Pulsar.Extensions;
using System.Diagnostics;
using System.Text;

namespace Rachkov.InspectaQueue.Providers.Pulsar;

public class PulsarConsumer : IQueueProvider, ICanPublish, ICanAcknowledge
{
    private readonly IErrorReporter _errorReporter;
    private Task? _readerTask;
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly PulsarConsumerSettings _settings;
    private PulsarClient? _client;
    private IConsumer<byte[]>? _consumer;
    private IProducer<byte[]>? _publisher;

    private IMessageProvider? _messagesForPublishingProvider;
    private IProgressNotificationService? _publisherProgressNotificationService;
    private long _lastPublishedMessage;

    public PulsarConsumer(IErrorReporter errorReporter)
    {
        _errorReporter = errorReporter;
        Debug.WriteLine($"==========> Constructing: {InstanceId}");
        _settings = new PulsarConsumerSettings();
    }

    ~PulsarConsumer()
    {
        Debug.WriteLine($"==========> Destructing: {InstanceId}");
    }

    public Guid InstanceId { get; } = Guid.NewGuid();

    public IProviderDetails Details { get; } = new ProviderDetails
    {
        Name = "Pulsar Consumer",
        Description = "Consumer with subscription name, cursor and publishing capabilities.",
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

                var messageString = Encoding.UTF8.GetString(message.Data);
                var frame = new InboundMessageFrame
                {
                    Content = messageString,
                    JsonRepresentation = messageString,
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

            await _consumer.NegativeAcknowledge(messageObject.MessageId);
            inboundMessage.AcknowledgedStatus = AcknowledgeStatus.NegativeAcknowledged;
        }

        return !hasUnacknowledged;
    }

    #endregion

    #region Publisher

    public async Task ConnectPublisher(IMessageProvider messageProvider, IProgressNotificationService progressNotificationService)
    {
        _publisherProgressNotificationService = progressNotificationService;
        _messagesForPublishingProvider = messageProvider;

        if (_client is null)
        {
            await _publisherProgressNotificationService.SendProgressUpdateNotification(new ProgressNotification(Constants.StatusMessage.Failed, Status.Failed));
            return;
        }

        await _publisherProgressNotificationService.SendProgressUpdateNotification(new ProgressNotification(Constants.StatusMessage.Connecting, Status.InProgress));

        try
        {
            if (string.IsNullOrWhiteSpace(_settings.ProducerName))
            {
                _publisher = await _client.NewProducer()
                    .Topic(_settings.TopicName)
                    .CreateAsync();
            }
            else
            {
                _publisher = await _client.NewProducer()
                    .Topic(_settings.TopicName)
                    .ProducerName(_settings.ProducerName)
                    .CreateAsync();
            }
        }
        catch (Exception e)
        {
            if (e is not OperationCanceledException)
            {
                _errorReporter.RaiseError(new()
                {
                    Text = "Error while initializing Pulsar publisher",
                    Source = this,
                    Exception = e
                });
            }

            await _publisherProgressNotificationService.SendProgressUpdateNotification(new ProgressNotification(Constants.StatusMessage.Failed, Status.Failed));

            await DisconnectPublisher();

            return;
        }

        messageProvider.MessageDispatched += PublishMessage;

        await _publisherProgressNotificationService.SendProgressUpdateNotification(new ProgressNotification(Constants.StatusMessage.Connected, Status.Ok));
    }

    private void PublishMessage(object? sender, IMessage e)
    {
        if (_publisher is null || _publisherProgressNotificationService is null)
        {
            return;
        }

        var data = Encoding.UTF8.GetBytes(e.Content);
        var message = _publisher.NewMessage(data, e.Key);

        _publisher.SendAsync(message).Wait();

        _lastPublishedMessage++;
        _publisherProgressNotificationService.SendProgressUpdateNotification(
            new ProgressNotification(_lastPublishedMessage, Constants.StatusMessage.Connected, Status.Ok))
            .Wait();
    }

    public async Task DisconnectPublisher()
    {
        if (_messagesForPublishingProvider is null)
        {
            return;
        }

        _messagesForPublishingProvider.MessageDispatched -= PublishMessage;
        await DisposePublisher();
    }

    #endregion

    public async ValueTask DisposeAsync()
    {
        Debug.WriteLine($"==========> Disposing: {InstanceId}");
        await DisposePublisher();
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

    private async Task DisposePublisher()
    {
        if (_publisher is not null)
        {
            await _publisher.DisposeAsync();
        }
    }
}