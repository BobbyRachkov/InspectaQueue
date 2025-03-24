namespace Rachkov.InspectaQueue.Abstractions.Notifications.Errors;

public interface IErrorReporter
{
    void RaiseError(Error error);
}