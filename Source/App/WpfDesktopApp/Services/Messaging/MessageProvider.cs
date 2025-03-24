using Rachkov.InspectaQueue.Abstractions.Messaging.Interfaces;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.DataChannel;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.Messaging;

public class MessageProvider : IMessageProvider
{
    private readonly IAsyncCommunicationService<IMessage> _asyncCommunicationService;

    public event EventHandler<IMessage>? MessageDispatched;

    public MessageProvider(CancellationToken cancellationToken = default)
    {
        _asyncCommunicationService = new UnboundedChannelAsyncCommunicationService<IMessage>(cancellationToken);
        _asyncCommunicationService.ItemDispatched += ItemDispatched;
    }

    private void ItemDispatched(object? sender, IMessage e)
    {
        MessageDispatched?.Invoke(sender, e);
    }

    public void Send(IMessage message, object? sender = null)
    {
        _asyncCommunicationService.SendAsync(sender, message);
    }
}