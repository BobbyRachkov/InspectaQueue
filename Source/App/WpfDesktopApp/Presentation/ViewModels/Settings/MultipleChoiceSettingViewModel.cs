namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;

public class MultipleChoiceSettingViewModel : BasicSettingViewModel
{
    public bool IsMultiSelectEnabled { get; init; } = false;
    public DropdownOptionViewModel[] Options { get; init; } = [];
    public object? SelectedItem
    {
        get => Options.FirstOrDefault(x => x.BackingValue.ToString() == Value?.ToString());
        set => Value = (value as DropdownOptionViewModel)?.BackingValue;
    }

    public override ISettingViewModel Clone()
    {
        return new MultipleChoiceSettingViewModel()
        {
            ReflectedProperty = ReflectedProperty,
            PropertyName = PropertyName,
            Name = Name,
            ToolTip = ToolTip,
            Type = Type,
            Value = Value,
            IsMultiSelectEnabled = IsMultiSelectEnabled,
            Options = Options.Select(x => x.Clone()).ToArray()
        };
    }
}