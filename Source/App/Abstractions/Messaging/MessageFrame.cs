namespace Rachkov.InspectaQueue.Abstractions.Messaging;

public record MessageFrame
{
    public long? Id { get; init; }

    public string? Key { get; init; }

    public required string Content { get; init; }

    public object? Message { get; init; }

    public string? JsonRepresentation { get; init; }


    public bool IsAcknowledged { get; set; }
}