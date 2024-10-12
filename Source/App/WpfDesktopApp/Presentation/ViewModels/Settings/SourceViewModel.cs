using System.Reflection;
using Rachkov.InspectaQueue.Abstractions;
using Rachkov.InspectaQueue.Abstractions.Attributes;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.SettingsParser;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;

public class SourceViewModel : ViewModel
{
    private readonly IQueueProvider _provider;
    private string _name;

    public SourceViewModel(
        Guid id,
        string name,
        IQueueProvider provider,
        SettingEntryViewModel[] settings)
    {
        Id = id;
        _provider = provider;
        _name = name;
        Settings = settings;
    }

    public Guid Id { get; }

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();
        }
    }

    public Type ProviderType => _provider.GetType();

    public SettingEntryViewModel[] Settings { get; }

    public IQueueProviderSettings UpdateSettings(IQueueProviderSettings settingsObjectToUpdate)
    {
        foreach (var setting in Settings)
        {
            setting.ReflectedProperty.SetValue(settingsObjectToUpdate, EnsureProperValueType(setting));
        }

        return settingsObjectToUpdate;
    }

    private object? EnsureProperValueType(SettingEntryViewModel setting)
    {
        if (setting.Value is not null
            && setting.Type == typeof(int)
            && setting.Value.GetType() != typeof(int))
        {
            return Convert.ToInt32(setting.Value);
        }

        return setting.Value;
    }

}