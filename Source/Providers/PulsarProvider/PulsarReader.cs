using Pulsar.Client.Api;
using Pulsar.Client.Common;
using Rachkov.InspectaQueue.Abstractions.Messaging.Interfaces;
using Rachkov.InspectaQueue.Abstractions.Messaging.Models;
using Rachkov.InspectaQueue.Abstractions.Notifications.Errors;
using Rachkov.InspectaQueue.Abstractions.Notifications.ProgressStatus;
using System.Diagnostics;
using System.Text;

namespace Rachkov.InspectaQueue.Providers.Pulsar;

public class PulsarReader : IQueueProvider
{
    private readonly IErrorReporter _errorReporter;
    private Task? _readerTask;
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly PulsarReaderSettings _settings;
    private PulsarClient? _client;
    private IReader<byte[]>? _reader;

    public PulsarReader(IErrorReporter errorReporter)
    {
        _errorReporter = errorReporter;
        Debug.WriteLine($"==========> Constructing Reader: {InstanceId}");
        _settings = new PulsarReaderSettings();
    }


    public Guid InstanceId { get; } = Guid.NewGuid();

    public IProviderDetails Details { get; } = new ProviderDetails
    {
        Name = "Pulsar Reader",
        Description = "Reader that reads from the latest message, no subscription tracking.",
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

    public Task<bool> TryAcknowledge(IInboundMessage message)
    {
        return Task.FromResult(true);
    }

    private async Task ReadAsync(
        IMessageReceiver messageReceiver,
        IProgressNotificationService progressNotificationService,
        CancellationToken cancellationToken)
    {
        long messagesReceived = 0;
        long messagesProcessed = 0;

        try
        {
            await progressNotificationService.SendProgressUpdateNotification(
                new ProgressNotification(Constants.StatusMessage.Connecting, Status.InProgress));

            _client = await new PulsarClientBuilder()
                .ServiceUrl(_settings.ServiceUrl)
                .Authentication(AuthenticationFactoryOAuth2.ClientCredentials(
                    new Uri(_settings.IssuerUrl),
                    _settings.Audience,
                    new Uri(_settings.FilePath)))
                .BuildAsync();

            _reader = await _client.NewReader()
                .Topic(_settings.TopicName)
                .StartMessageId(MessageId.Latest)
                .CreateAsync();

            cancellationToken.ThrowIfCancellationRequested();
        }
        catch (Exception e)
        {
            if (e is not OperationCanceledException)
            {
                _errorReporter.RaiseError(new()
                {
                    Text = "Error while initializing Pulsar reader",
                    Source = this,
                    Exception = e
                });
            }

            await progressNotificationService.SendProgressUpdateNotification(
                new ProgressNotification(Constants.StatusMessage.Failed, Status.Failed));
            return;
        }

        await progressNotificationService.SendProgressUpdateNotification(
            new ProgressNotification(Constants.StatusMessage.Connected, Status.Ok));

        var filterByKeyEnabled = !string.IsNullOrEmpty(_settings.FilterByKey);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var message = await _reader.ReadNextAsync(cancellationToken);
                messagesReceived++;

                if (filterByKeyEnabled
                    && message.Key is not null
                    && !message.Key.Contains(_settings.FilterByKey))
                {
                    await progressNotificationService.SendProgressUpdateNotification(new ProgressNotification(
                        messagesReceived,
                        messagesProcessed,
                        Constants.StatusMessage.Connected,
                        Status.Ok));
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
                    IsAcknowledged = false
                };

                await messageReceiver.SendMessageAsync(frame);
                messagesProcessed++;

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

                if (e is OperationCanceledException || !await _reader.IsConnected())
                    break;
            }
        }

        await progressNotificationService.SendProgressUpdateNotification(new ProgressNotification(
            messagesReceived,
            messagesProcessed,
            Constants.StatusMessage.Disconnected,
            Status.Failed));

        await DisposeReaderAndClient();

        if (_cancellationTokenSource is not null)
        {
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }
    }

    private async Task DisposeReaderAndClient()
    {
        if (_reader is not null)
        {
            try
            {
                await _reader.DisposeAsync();
                _reader = null;
            }
            catch (Exception ex)
            {
                _errorReporter.RaiseError(new() { Text = "Error disposing Pulsar reader", Source = this, Exception = ex });
            }
        }

        if (_client is not null)
        {
            try
            {
                await _client.CloseAsync();
                _client = null;
            }
            catch (Exception ex)
            {
                _errorReporter.RaiseError(new() { Text = "Error disposing Pulsar client", Source = this, Exception = ex });
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeReaderAndClient();

        if (_readerTask is not null)
        {
            await CastAndDispose(_readerTask);
        }

        if (_cancellationTokenSource is not null)
        {
            await CastAndDispose(_cancellationTokenSource);
        }

        if (_reader is not null)
        {
            await _reader.DisposeAsync();
        }

        return;

        static async ValueTask CastAndDispose(IDisposable resource)
        {
            if (resource is IAsyncDisposable resourceAsyncDisposable)
            {
                await resourceAsyncDisposable.DisposeAsync();
            }
            else
            {
                resource.Dispose();
            }
        }
    }
}
