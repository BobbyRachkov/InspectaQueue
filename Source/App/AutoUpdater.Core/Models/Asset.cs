namespace Rachkov.InspectaQueue.Abstractions.Models;

public sealed record Asset
{
    public required string Name { get; init; }
    public required int DownloadCount { get; init; }
    public required string? DownloadUrl { get; init; }
    public required string Url { get; init; }

    public Version? Version { get; init; }
}