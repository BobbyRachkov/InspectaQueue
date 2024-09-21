using System.ComponentModel;
using System.Security.AccessControl;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure;

public interface IPresenterViewModel
{
    event EventHandler Show;
    event EventHandler Hide;
    event EventHandler<CancelEventArgs> OnClosing;

    bool IsVisible { get; }

    string Name { get; }

    void SetVisibility(bool isVisible);

    void RaiseClosing(CancelEventArgs args);
}