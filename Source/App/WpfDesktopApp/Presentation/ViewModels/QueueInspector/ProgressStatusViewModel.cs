using Rachkov.InspectaQueue.Abstractions.Notifications.ProgressStatus;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.QueueInspector;

public class ProgressStatusViewModel : ViewModel
{
    private long _dispatchedMessages;
    private long _receivedMessages;
    private string _statusMessage = "Initializing...";
    private Status _status = Status.InProgress;

    public long DispatchedMessages
    {
        get => _dispatchedMessages;
        set
        {
            if (value == _dispatchedMessages) return;
            _dispatchedMessages = value;
            OnPropertyChanged();
        }
    }

    public long ReceivedMessages
    {
        get => _receivedMessages;
        set
        {
            if (value == _receivedMessages) return;
            _receivedMessages = value;
            OnPropertyChanged();
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (value == _statusMessage) return;
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public Status Status
    {
        get => _status;
        set
        {
            if (value == _status) return;
            _status = value;

            OnPropertyChanged();
            OnPropertyChanged(nameof(IsOk));
            OnPropertyChanged(nameof(IsInProgress));
            OnPropertyChanged(nameof(IsFailed));
        }
    }

    public bool IsOk => _status == Status.Ok;
    public bool IsInProgress => _status == Status.InProgress;
    public bool IsFailed => _status == Status.Failed;


    public void Update(IProgressNotification notification)
    {
        if (notification.Processed is not null)
        {
            DispatchedMessages = notification.Processed.Value;
        }

        if (notification.Received is not null)
        {
            ReceivedMessages = notification.Received.Value;
        }

        StatusMessage = notification.ConnectionStatusMessage ?? string.Empty;
        Status = notification.Status;
    }
}