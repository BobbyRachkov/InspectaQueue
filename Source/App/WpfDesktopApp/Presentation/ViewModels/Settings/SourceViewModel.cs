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
        string name,
        IQueueProvider provider,
        ISettingsParser parser)
    {
        _name = name;
        Settings = new();
        _provider = provider;
        Settings = parser.ParseMembers(provider).ToList();
    }

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

    public List<SettingEntryViewModel> Settings { get; }

    public IQueueProviderSettings UpdateSettings(IQueueProviderSettings settingsObjectToUpdate)
    {
        foreach (var setting in Settings)
        {
            setting.ReflectedProperty.SetValue(settingsObjectToUpdate, setting.Value);
        }

        return settingsObjectToUpdate;
    }

}