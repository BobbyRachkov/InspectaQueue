namespace Rachkov.InspectaQueue.AutoUpdater.Core.EventArgs;

public record JobStatusChangedEventArgs
{
    public required Stage[]? Stages { get; init; }
    public required bool IsJobRunning { get; init; }
}