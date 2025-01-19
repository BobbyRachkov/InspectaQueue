namespace Rachkov.InspectaQueue.AutoUpdater.Core.EventArgs;

public sealed record StageStatusChangedEventArgs
{
    public required Stage Stage { get; init; }
    public required StageStatus Status { get; init; }
}