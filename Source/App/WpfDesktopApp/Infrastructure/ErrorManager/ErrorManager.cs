using System.Threading.Channels;
using Rachkov.InspectaQueue.Abstractions.Notifications.Errors;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure.ErrorManager;

public class ErrorManager : IErrorManager
{
    private readonly List<Error> _errors;

    public ErrorManager()
    {
        _errors = new();
    }

    public void RaiseError(Error error)
    {
        lock (_errors)
        {
            _errors.Add(error);
        }

        ErrorRaised?.Invoke(error.Source, error);
    }

    public Error[] GetErrors()
    {
        lock (_errors)
        {
            return _errors.ToArray();
        }
    }

    public void ClearErrors()
    {
        lock (_errors)
        {
            _errors.Clear();
        }

        ErrorsCleared?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler<Error>? ErrorRaised;
    public event EventHandler? ErrorsCleared;
}