using System.Reflection;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager.Models;

public class SettingPack
{
    public required PropertyInfo ReflectedProperty { get; set; }
    public required string PropertyName { get; set; }
    public required string Name { get; set; }
    public string? ToolTip { get; set; }
    public required Type Type { get; set; }
    public object? Value { get; set; }

    public SettingPack Clone()
    {
        return new SettingPack
        {
            ReflectedProperty = ReflectedProperty,
            PropertyName = PropertyName,
            Name = Name,
            ToolTip = ToolTip,
            Type = Type,
            Value = Value
        };
    }
}