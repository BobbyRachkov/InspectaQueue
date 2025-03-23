namespace Rachkov.InspectaQueue.Abstractions.Messaging.Interfaces;

public interface IInboundMessage : IMessage
{
    string? Id { get; init; }

    object? Message { get; init; }

    string? JsonRepresentation { get; init; }

    bool IsAcknowledged { get; set; }
}