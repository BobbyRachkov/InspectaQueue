using Rachkov.InspectaQueue.Abstractions.Notifications.ProgressStatus;

namespace Rachkov.InspectaQueue.Abstractions.Messaging.Interfaces;

public interface IProgressNotificationService
{
    Task SendProgressUpdateNotification(IProgressNotification notification);
}