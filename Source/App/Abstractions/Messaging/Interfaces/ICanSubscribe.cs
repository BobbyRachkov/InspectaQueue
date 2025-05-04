namespace Rachkov.InspectaQueue.Abstractions.Messaging.Interfaces;

public interface ICanSubscribe
{
    Task Connect(IMessageReceiver messageReceiver, IProgressNotificationService progressNotificationService);

    Task DisconnectSubscriber();
}