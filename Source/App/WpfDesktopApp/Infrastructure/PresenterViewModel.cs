using System.ComponentModel;
using System.Windows.Threading;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure;

public abstract class PresenterViewModel : ViewModel, IPresenterViewModel
{
    public event EventHandler? Show;
    public event EventHandler? Hide;
    public event EventHandler<CancelEventArgs>? OnClosing;
    protected SynchronizationContext? ViewContext;

    public abstract string Name { get; }
    public bool IsVisible { get; set; } = true;

    protected PresenterViewModel()
    {
        ViewContext = SynchronizationContext.Current;
    }

    public void SetVisibility(bool isVisible)
    {
        IsVisible = isVisible;

        if (isVisible)
        {
            Show?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Hide?.Invoke(this, EventArgs.Empty);
        }
    }

    public void RaiseClosing(CancelEventArgs args)
    {
        OnClosing?.Invoke(this, args);
    }

    protected void OnUiThread(Action action)
    {
        ViewContext?.Send(_ => action(), null);
    }
}