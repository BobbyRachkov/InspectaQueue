namespace Rachkov.InspectaQueue.Abstractions.Messaging.Interfaces;

public interface IMessageProvider
{
    event EventHandler<IMessage> MessageDispatched;
}