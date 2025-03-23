namespace Rachkov.InspectaQueue.Abstractions.Notifications.ProgressStatus;

public class ProgressNotification : IProgressNotification
{
    public required long Received { get; init; }
    public required long Processed { get; init; }
    public string? ConnectionStatusMessage { get; init; }
    public required Status Status { get; init; }
}