namespace Rachkov.InspectaQueue.Abstractions.Notifications.ProgressStatus;

public record ProgressNotification : IProgressNotification
{
    public long Received { get; init; }
    public long Processed { get; init; }
    public string? ConnectionStatusMessage { get; init; }
    public Status Status { get; init; }

    public ProgressNotification(Status status)
    {
        Status = status;
    }

    public ProgressNotification(string connectionStatusMessage, Status status) : this(status)
    {
        ConnectionStatusMessage = connectionStatusMessage;
    }

    public ProgressNotification(long received, long processed, string connectionStatusMessage, Status status) : this(connectionStatusMessage, status)
    {
        Received = received;
        Processed = processed;
    }
}