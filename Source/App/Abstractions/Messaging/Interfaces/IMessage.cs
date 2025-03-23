namespace Rachkov.InspectaQueue.Abstractions.Messaging.Interfaces;

public interface IMessage
{
    public string? Key { get; init; }

    public string Content { get; init; }
}