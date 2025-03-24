using Rachkov.InspectaQueue.Abstractions.Messaging.Interfaces;

namespace Rachkov.InspectaQueue.Abstractions.Messaging.Models;

public class OutboundMessageFrame : IMessage
{
    public string? Key { get; init; }

    public required string Content { get; init; }
}