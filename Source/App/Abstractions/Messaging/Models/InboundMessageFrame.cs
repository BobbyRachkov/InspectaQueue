using Rachkov.InspectaQueue.Abstractions.Messaging.Interfaces;

namespace Rachkov.InspectaQueue.Abstractions.Messaging.Models;

public record InboundMessageFrame : IInboundMessage
{
    public string? Id { get; init; }

    public string? Key { get; init; }

    public required string Content { get; init; }

    public object? Message { get; init; }

    public string? JsonRepresentation { get; init; }

    public AcknowledgeStatus AcknowledgedStatus { get; set; } = AcknowledgeStatus.None;
}