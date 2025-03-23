using System.Threading.Channels;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.DataChannel;

public class UnboundedChannelAsyncCommunicationService<T> : IAsyncCommunicationService<T>
{
    private readonly Channel<(object Sender, T Payload)> _channel;

    public event EventHandler<T>? ItemDispatched;

    public UnboundedChannelAsyncCommunicationService(int maxItems, BoundedChannelFullMode fullMode, CancellationToken cancellationToken = default)
    {
        _channel = Channel.CreateUnbounded<(object, T)>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = true
        });

        Task.Run(() => Receive(cancellationToken), cancellationToken);
    }

    public Task<bool> SendAsync(object sender, T item)
    {
        return Task.FromResult(_channel.Writer.TryWrite((sender, item)));
    }

    private async Task Receive(CancellationToken cancellationToken)
    {
        while (await _channel.Reader.WaitToReadAsync(cancellationToken))
        {
            while (_channel.Reader.TryRead(out var item))
            {
                cancellationToken.ThrowIfCancellationRequested();
                ItemDispatched?.Invoke(item.Sender, item.Payload);
            }
        }
    }
}