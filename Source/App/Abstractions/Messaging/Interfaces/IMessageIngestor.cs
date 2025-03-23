namespace Rachkov.InspectaQueue.Abstractions.Messaging.Interfaces;

public interface IMessageReceiver
{
    Task<bool> SendMessageAsync(IInboundMessage message);
}