using System.Security.AccessControl;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure;

public interface IPresenterViewModel
{
    event EventHandler Show;
    event EventHandler Hide;

    bool IsVisible { get; }

    string Name { get; }

    void SetVisibility(bool isVisible);
}