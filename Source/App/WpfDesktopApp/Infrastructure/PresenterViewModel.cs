namespace Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure;

public abstract class PresenterViewModel : ViewModel, IPresenterViewModel
{
    public event EventHandler? Show;
    public event EventHandler? Hide;

    public abstract string Name { get; }
    public bool IsVisible { get; set; } = true;

    protected PresenterViewModel()
    {
        
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
}