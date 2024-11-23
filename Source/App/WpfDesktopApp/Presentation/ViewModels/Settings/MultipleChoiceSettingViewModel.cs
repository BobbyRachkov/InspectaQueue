using System.Reflection;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;

public class MultipleChoiceSettingViewModel : ISettingViewModel
{
    public required PropertyInfo ReflectedProperty { get; init; }
    public required string PropertyName { get; init; }
    public required string Name { get; init; }
    public string? ToolTip { get; init; }
    public required Type Type { get; init; }
    public object? Value { get; set; }

    public bool IsMultiSelectEnabled { get; init; } = false;
    public DropdownOptionViewModel[] Options { get; init; } = [];

    public ISettingViewModel Clone()
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