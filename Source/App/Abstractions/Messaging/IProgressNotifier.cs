namespace Rachkov.InspectaQueue.Abstractions.Messaging;

public interface IProgressNotifier
{
    public long Received { get; }
    public long Processed { get; }
    public string? ConnectionStatus { get; }
    public Status Status { get; }
}