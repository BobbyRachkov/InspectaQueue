using Rachkov.InspectaQueue.Abstractions.Notifications.Errors;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure.ErrorManager;

public interface IErrorManager : IErrorReporter
{
    Error[] GetErrors();

    void ClearErrors();

    event EventHandler<Error> ErrorRaised;

    event EventHandler ErrorsCleared;
}