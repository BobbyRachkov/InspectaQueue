namespace Rachkov.InspectaQueue.Abstractions;

public record MessageFrame
{
    public required string Content { get; init; }
    public object? Message { get; init; }
}