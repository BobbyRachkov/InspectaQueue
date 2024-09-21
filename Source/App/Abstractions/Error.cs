namespace Rachkov.InspectaQueue.Abstractions;

public class Error
{
    public required string Text { get; init; }

    public required object Source { get; init; }

    public Exception? Exception { get; init; }
}