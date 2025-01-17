namespace Rachkov.InspectaQueue.Abstractions.EventArgs;

public record JobStatusChangedEventArgs
{
    public required Stage[]? Stages { get; init; }
    public required bool IsJobRunning { get; init; }
}