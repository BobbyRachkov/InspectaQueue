using Rachkov.InspectaQueue.Abstractions.Notifications.ProgressStatus;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.QueueInspector;

public class ProgressStatusViewModel : ViewModel
{
    private long _shownMessages;
    private long _dispatchedMessages;
    private long _receivedMessages;
    private string _statusMessage = string.Empty;
    private Status _receivingStatus;

    public long ShownMessages
    {
        get => _shownMessages;
        set
        {
            if (value == _shownMessages) return;
            _shownMessages = value;
            OnPropertyChanged();
        }
    }

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

    public Status ReceivingStatus
    {
        get => _receivingStatus;
        set
        {
            if (value == _receivingStatus) return;
            _receivingStatus = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsReceivingOk));
            OnPropertyChanged(nameof(IsReceivingInProgress));
            OnPropertyChanged(nameof(IsReceivingFailed));
        }
    }

    public bool IsReceivingOk => _receivingStatus == Status.Ok;
    public bool IsReceivingInProgress => _receivingStatus == Status.InProgress;
    public bool IsReceivingFailed => _receivingStatus == Status.Failed;

    public void UpdateReceiving(IProgressNotification notification)
    {
        DispatchedMessages = notification.Processed;
        ReceivedMessages = notification.Received;
        StatusMessage = notification.ConnectionStatusMessage ?? string.Empty;
        ReceivingStatus = notification.Status;
    }
}