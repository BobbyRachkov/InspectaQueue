namespace Rachkov.InspectaQueue.Abstractions.EventArgs;

public sealed record StageStatusChangedEventArgs
{
    public required Stage Stage { get; init; }
    public required StageStatus Status { get; init; }
}