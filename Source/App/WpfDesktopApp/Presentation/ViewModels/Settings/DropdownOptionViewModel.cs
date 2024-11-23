namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;

public class DropdownOptionViewModel(string displayName, object backingValue)
{
    public string DisplayName { get; } = displayName;
    public object BackingValue { get; } = backingValue;

    public DropdownOptionViewModel Clone()
    {
        return new DropdownOptionViewModel(displayName, backingValue);
    }
}