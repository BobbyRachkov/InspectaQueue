namespace Rachkov.InspectaQueue.AutoUpdater.Core.Models;

public sealed record Release
{
    public required string Name { get; init; }
    public required string Tag { get; init; }
    public required bool IsLatest { get; init; }
    public required bool IsPrerelease { get; init; }

    public required Asset[] Assets { get; init; }

    public Asset? Installer { get; init; }
    public Asset? WindowsAppZip { get; init; }
    public Asset? VersionInfo { get; init; }
}