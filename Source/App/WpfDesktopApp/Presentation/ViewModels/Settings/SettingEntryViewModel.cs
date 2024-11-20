using Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager.Models;
using System.Reflection;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;

public class SettingEntryViewModel(ISettingPack settingsInstance)
{
    public ISettingPack SettingsInstance { get; } = settingsInstance;

    public PropertyInfo ReflectedProperty => SettingsInstance.ReflectedProperty;
    public string PropertyName => SettingsInstance.PropertyName;
    public string Name => SettingsInstance.Name;
    public string? ToolTip => SettingsInstance.ToolTip;
    public Type Type => SettingsInstance.Type;
    public object? Value
    {
        get => SettingsInstance.Value;
        set => SettingsInstance.Value = value;
    }
}