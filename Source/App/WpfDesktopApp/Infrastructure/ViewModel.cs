using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure;

public class ViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected SynchronizationContext? ViewContext = SynchronizationContext.Current;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected void OnUiThread(Action action)
    {
        ViewContext?.Send(_ => action(), null);
    }

    protected void OnUiThread(Func<Task> action)
    {
        ViewContext?.Send(_ => action(), null);
    }
}