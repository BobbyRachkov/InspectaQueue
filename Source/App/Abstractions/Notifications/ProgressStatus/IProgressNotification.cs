namespace Rachkov.InspectaQueue.Abstractions.Notifications.ProgressStatus;

public interface IProgressNotification
{
    /// <summary>
    /// Count of the messages that this service has received from external source (queue or user)
    /// </summary>
    public long? Received { get; }

    /// <summary>
    /// Count of messages are passed down after filtering/processing
    /// </summary>
    public long? Processed { get; }

    /// <summary>
    /// Connection status text that will be displayed in the UI
    /// </summary>
    public string? ConnectionStatusMessage { get; }

    /// <summary>
    /// Enum of the status, used for progress bar/spinner, whether to show green tick, loading bar or failed status
    /// </summary>
    public Status Status { get; }
}