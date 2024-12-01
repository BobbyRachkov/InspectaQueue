using System.Reflection;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager.Models;

public interface ISettingPack
{
    PropertyInfo ReflectedProperty { get; set; }
    string PropertyName { get; set; }
    string Name { get; set; }
    string? ToolTip { get; set; }
    Modifiers.Modifiers Modifiers { get; init; }
    Type Type { get; set; }
    object? Value { get; set; }
}