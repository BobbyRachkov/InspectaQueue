using Rachkov.InspectaQueue.Abstractions.Messaging.Interfaces;
using Rachkov.InspectaQueue.Abstractions.Notifications.ProgressStatus;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.DataChannel;
using System.Threading.Channels;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.ProgressNotification;

public class ProgressNotificationService : IProgressNotificationService
{
    private readonly IAsyncCommunicationService<IProgressNotification> _asyncCommunicationService;

    public event EventHandler<IProgressNotification>? MessageDispatched;

    public ProgressNotificationService()
    {
        _asyncCommunicationService =
            new BoundedChannelAsyncCommunicationService<IProgressNotification>(2, BoundedChannelFullMode.DropOldest);
        _asyncCommunicationService.ItemDispatched += ItemDispatched;
    }

    private void ItemDispatched(object? sender, IProgressNotification e)
    {
        MessageDispatched?.Invoke(sender, e);
    }

    public async Task SendProgressUpdateNotification(IProgressNotification notification)
    {
        await _asyncCommunicationService.SendAsync(null, notification);
    }
}