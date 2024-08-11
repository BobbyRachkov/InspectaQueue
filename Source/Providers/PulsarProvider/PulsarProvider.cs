using System.Diagnostics;
using System.Text;
using System.Threading.Channels;
using Pulsar.Client.Api;
using Pulsar.Client.Common;
using Rachkov.InspectaQueue.Abstractions;

namespace Rachkov.InspectaQueue.Providers.Pulsar;

public class PulsarProvider : IQueueProvider, IAsyncDisposable
{
    private readonly Channel<MessageFrame> _messagesChannel;
    private Task? _readerTask;
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly PulsarSettings _settings;
    private PulsarClient? _client;
    private IConsumer<byte[]>? _consumer;
    private Guid _id = Guid.NewGuid();

    public PulsarProvider()
    {
        Debug.WriteLine($"==========> Constructing: {_id}");
        Console.WriteLine($"==========> Constructing: {_id}");
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
        Console.WriteLine($"==========> Destructing: {_id}");
    }

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

    public Task Disconnect()
    {
        _cancellationTokenSource?.Cancel();
        return Task.CompletedTask;
    }

    public async Task<bool> TryAcknowledge(MessageFrame frame)
    {
        var message = frame.Message as Message<byte[]>;
        if (message is null || _consumer is null)
        {
            return false;
        }

        await _consumer.AcknowledgeAsync(message.MessageId);
        return true;
    }

    private async Task ReadAsync(CancellationToken cancellationToken)
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
            .SubscribeAsync();

        while (!cancellationToken.IsCancellationRequested)
        {
            var message = await _consumer.ReceiveAsync(cancellationToken);

            var frame = new MessageFrame
            {
                Content = Encoding.UTF8.GetString(message.Data),
                Message = message
            };

            _messagesChannel.Writer.TryWrite(frame);

            if (_settings.AcknowledgeOnReceive)
            {
                await _consumer.AcknowledgeAsync(message.MessageId);
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        Debug.WriteLine($"==========> Disposing: {_id}");
        Console.WriteLine($"==========> Disposing: {_id}");
        if (_cancellationTokenSource is not null)
        {
            await _cancellationTokenSource.CancelAsync();
            _cancellationTokenSource?.Dispose();
        }

        _readerTask?.Dispose();
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