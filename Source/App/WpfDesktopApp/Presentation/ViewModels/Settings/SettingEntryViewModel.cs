using System.Reflection;
using Rachkov.InspectaQueue.Abstractions.Attributes;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;

public class SettingEntryViewModel
{
    public required PropertyInfo ReflectedProperty { get; set; }
    public required string PropertyName { get; set; }
    public required string Name { get; set; }
    public string? ToolTip { get; set; }
    public required Type Type { get; set; }
    public object? Value { get; set; }
}