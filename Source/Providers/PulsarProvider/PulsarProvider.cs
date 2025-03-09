using Pulsar.Client.Api;
using Pulsar.Client.Common;
using Rachkov.InspectaQueue.Abstractions.Messaging;
using Rachkov.InspectaQueue.Abstractions.Notifications.Errors;
using Rachkov.InspectaQueue.Providers.Pulsar.Extensions;
using System.Diagnostics;
using System.Text;
using System.Threading.Channels;

namespace Rachkov.InspectaQueue.Providers.Pulsar;

public class PulsarProvider : IQueueProvider, IAsyncDisposable
{
    private readonly IErrorReporter _errorReporter;
    private readonly Channel<MessageFrame> _messagesChannel;
    private Task? _readerTask;
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly PulsarSettings _settings;
    private PulsarClient? _client;
    private IConsumer<byte[]>? _consumer;
    private Guid _id = Guid.NewGuid();

    public PulsarProvider(IErrorReporter errorReporter)
    {
        _errorReporter = errorReporter;
        Debug.WriteLine($"==========> Constructing: {_id}");
        _settings = new PulsarSettings();
        _messagesChannel = Channel.CreateUnbounded<MessageFrame>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = true
        });
    }

    ~PulsarProvider()
    {
        Debug.WriteLine($"==========> Destructing: {_id}");
    }

    public string Name => "Apache Pulsar";
    public string PackageVendorName => "InspectaQueue";

    public IQueueProviderSettings Settings => _settings;

    public Task UpdateSettings(IQueueProviderSettings settings)
    {
        throw new NotImplementedException();
    }

    public ChannelReader<MessageFrame> Messages => _messagesChannel.Reader;

    public Task Connect()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _readerTask = Task.Run(() => ReadAsync(_cancellationTokenSource.Token));

        return Task.CompletedTask;
    }

    public async Task Disconnect()
    {
        if (_cancellationTokenSource is not null)
        {
            await _cancellationTokenSource.CancelAsync();
        }
    }

    public async Task<bool> TryAcknowledge(MessageFrame frame)
    {
        var message = frame.Message as Message<byte[]>;
        if (message is null || _consumer is null)
        {
            return false;
        }

        await _consumer.AcknowledgeAsync(message.MessageId);
        frame.IsAcknowledged = true;
        return true;
    }

    private async Task ReadAsync(CancellationToken cancellationToken)
    {
        try
        {
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

            await DisposeConsumerAndClient();

            return;
        }

        var filterByKeyEnabled = !string.IsNullOrEmpty(_settings.FilterByKey);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var message = await _consumer.ReceiveAsync(cancellationToken);

                if (filterByKeyEnabled && message.Key.Contains(_settings.FilterByKey))
                {
                    if (_settings.AcknowledgeOnReceive)
                    {
                        await _consumer.AcknowledgeAsync(message.MessageId);
                    }

                    continue;
                }

                var messageString = Encoding.UTF8.GetString(message.Data);
                var frame = new MessageFrame
                {
                    Content = messageString,
                    JsonRepresentation = messageString,
                    Message = message,
                    Key = message.Key,
                    Id = message.MessageId.EntryId
                };

                _messagesChannel.Writer.TryWrite(frame);


                if (_settings.AcknowledgeOnReceive)
                {
                    await _consumer.AcknowledgeAsync(message.MessageId);
                    frame.IsAcknowledged = true;
                }
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
                }
            }
        }

        await DisposeConsumerAndClient();
    }

    public async ValueTask DisposeAsync()
    {
        Debug.WriteLine($"==========> Disposing: {_id}");
        await Disconnect();
    }

    private async Task DisposeConsumerAndClient()
    {
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