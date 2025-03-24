using System.Threading.Channels;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.DataChannel;

public class BoundedChannelAsyncCommunicationService<T> : IAsyncCommunicationService<T>
{
    private readonly Channel<(object? Sender, T Payload)> _channel;

    public event EventHandler<T>? ItemDispatched;

    public BoundedChannelAsyncCommunicationService(int maxItems, BoundedChannelFullMode fullMode, CancellationToken cancellationToken = default)
    {
        _channel = Channel.CreateBounded<(object?, T)>(new BoundedChannelOptions(maxItems)
        {
            FullMode = fullMode,
            SingleReader = true,
            SingleWriter = true
        });

        Task.Factory.StartNew(() => Receive(cancellationToken), cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    public Task<bool> SendAsync(object? sender, T item)
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