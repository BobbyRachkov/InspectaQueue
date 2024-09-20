namespace Rachkov.InspectaQueue.Abstractions;

public interface IErrorReporter
{
    void RaiseError(Error error);
}