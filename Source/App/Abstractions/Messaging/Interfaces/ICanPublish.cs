namespace Rachkov.InspectaQueue.Abstractions.Messaging.Interfaces;

public interface ICanPublish
{
    Task ConnectPublisher(IMessageProvider messageProvider, IProgressNotificationService progressNotificationService);

    Task DisconnectPublisher();
}