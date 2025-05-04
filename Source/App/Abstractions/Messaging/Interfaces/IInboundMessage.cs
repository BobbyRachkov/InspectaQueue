using Rachkov.InspectaQueue.Abstractions.Messaging.Models;

namespace Rachkov.InspectaQueue.Abstractions.Messaging.Interfaces;

public interface IInboundMessage : IMessage
{
    string? Id { get; init; }

    object? Message { get; init; }

    string? JsonRepresentation { get; init; }

    AcknowledgeStatus AcknowledgedStatus { get; set; }
}