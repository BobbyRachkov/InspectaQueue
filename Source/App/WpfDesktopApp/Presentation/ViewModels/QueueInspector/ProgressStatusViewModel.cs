using Rachkov.InspectaQueue.Abstractions.Notifications.ProgressStatus;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.QueueInspector;

public class ProgressStatusViewModel : ViewModel
{
    private long _dispatchedMessages;
    private long _receivedMessages;
    private string _statusMessage = "Initializing...";
    private Status _receivingStatus = Status.InProgress;
    private bool _isMasterLoadingIndicatorOn = true;

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

            if (value is Status.Failed)
            {
                IsMasterLoadingIndicatorOn = false;
            }

            OnPropertyChanged();
            OnPropertyChanged(nameof(IsReceivingOk));
            OnPropertyChanged(nameof(IsReceivingInProgress));
            OnPropertyChanged(nameof(IsReceivingFailed));
        }
    }

    public bool IsReceivingOk => _receivingStatus == Status.Ok;
    public bool IsReceivingInProgress => _receivingStatus == Status.InProgress;
    public bool IsReceivingFailed => _receivingStatus == Status.Failed;

    public bool IsMasterLoadingIndicatorOn
    {
        get => _isMasterLoadingIndicatorOn;
        set
        {
            if (value == _isMasterLoadingIndicatorOn) return;
            _isMasterLoadingIndicatorOn = value;
            OnPropertyChanged();
        }
    }

    public void UpdateReceiving(IProgressNotification notification)
    {
        DispatchedMessages = notification.Processed;
        ReceivedMessages = notification.Received;
        StatusMessage = notification.ConnectionStatusMessage ?? string.Empty;
        ReceivingStatus = notification.Status;
    }
}