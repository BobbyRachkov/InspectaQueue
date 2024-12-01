using Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager.Models.Modifiers;
using System.Reflection;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;

public class BasicSettingViewModel : ISettingViewModel
{
    public required PropertyInfo ReflectedProperty { get; init; }
    public required string PropertyName { get; init; }
    public required string Name { get; init; }
    public string? ToolTip { get; init; }
    public Modifiers Modifiers { get; init; } = new();
    public required Type Type { get; init; }
    public object? Value { get; set; }

    public virtual ISettingViewModel Clone()
    {
        return new BasicSettingViewModel()
        {
            ReflectedProperty = ReflectedProperty,
            PropertyName = PropertyName,
            Name = Name,
            ToolTip = ToolTip,
            Type = Type,
            Value = Value,
            Modifiers = Modifiers.Clone()
        };
    }
}