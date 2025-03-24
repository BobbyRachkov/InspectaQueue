using Rachkov.InspectaQueue.Abstractions.Messaging.Interfaces;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.DataChannel;
using System.Threading.Channels;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.Messaging;

public class MessageReceiver : IMessageReceiver
{
    private readonly IAsyncCommunicationService<IInboundMessage> _asyncCommunicationService;

    public event EventHandler<IInboundMessage>? MessageDispatched;

    public MessageReceiver(int maxMessages, CancellationToken cancellationToken)
    {
        _asyncCommunicationService = new BoundedChannelAsyncCommunicationService<IInboundMessage>(maxMessages, BoundedChannelFullMode.DropOldest, cancellationToken);
        _asyncCommunicationService.ItemDispatched += ItemDispatched;
    }

    private void ItemDispatched(object? sender, IInboundMessage e)
    {
        MessageDispatched?.Invoke(sender, e);
    }

    public async Task<bool> SendMessageAsync(IInboundMessage message)
    {
        return await _asyncCommunicationService.SendAsync(null, message);
    }
}