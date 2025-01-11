namespace Rachkov.InspectaQueue.Abstractions.Models;

public sealed record ReleaseInfo
{
    public required Release Latest { get; init; }
    public required Release? Prerelease { get; init; }
}