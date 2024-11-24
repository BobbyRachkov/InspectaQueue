using Rachkov.InspectaQueue.Abstractions;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure;
using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings.Translators;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager.Models;
using System.Windows;
using System.Windows.Input;
using Version = Rachkov.InspectaQueue.Abstractions.Version;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;

public class SourceViewModel : ViewModel
{
    private IQueueProvider _provider;
    private readonly Action _saveSourcesCallback;
    private string _name;
    private readonly ISettingsManager _settingsManager;
    private string _providerDisplayVersion;
    private ISettingViewModel[] _settings;
    private bool _isNewerVersionAvailable;

    public SourceViewModel(
        Guid id,
        string name,
        ISettingsManager settingsManager,
        IQueueProvider provider,
        IReadOnlyDictionary<string, IQueueProvider> availableProviderVersions,
        ISettingViewModel[] settings,
        Action saveSourcesCallback)
    {
        Id = id;
        _name = name;
        _settingsManager = settingsManager;
        _provider = provider;
        Settings = settings;
        _saveSourcesCallback = saveSourcesCallback;
        AvailableProviderVersions = availableProviderVersions
            .OrderByDescending(x => new Version(x.Key))
            .ToDictionary();

        SelectedProviderVersion = AvailableProviderVersions.First(x => x.Value == ProviderInstance);
        ProviderDisplayVersion = SelectedProviderVersion.Key;

        ChangeVersionCommand = new RelayCommand(ChangeProviderVersion, () => _provider != SelectedProviderVersion.Value);
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
    public ICommand ChangeVersionCommand { get; }
    public Type ProviderType => _provider.GetType();
    public IQueueProvider ProviderInstance => _provider;

    public string ProviderDisplayName => $"{ProviderType.FullName}";

    public string ProviderDisplayVersion
    {
        get => _providerDisplayVersion;
        private set
        {
            _providerDisplayVersion = $"v{value}";
            OnPropertyChanged();
        }
    }

    public IReadOnlyDictionary<string, IQueueProvider> AvailableProviderVersions { get; }
    public KeyValuePair<string, IQueueProvider> SelectedProviderVersion { get; set; }
    public bool IsNewerVersionAvailable => SelectedProviderVersion.Key != AvailableProviderVersions.First().Key;

    public ISettingViewModel[] Settings
    {
        get => _settings;
        private set
        {
            _settings = value;
            OnPropertyChanged();
        }
    }

    public void ChangeProviderVersion()
    {
        if (_provider == SelectedProviderVersion.Value)
        {
            return;
        }

        _provider = SelectedProviderVersion.Value;
        ProviderDisplayVersion = SelectedProviderVersion.Key;

        var newSettings = _settingsManager.ExtractSettings(_provider);
        var mergedSettings = _settingsManager.MergePacks(newSettings, Settings.Select(x => new SettingDetachedPack
        {
            PropertyName = x.PropertyName,
            Value = x.Value
        }));
        Settings = mergedSettings.Select(x => x.ToViewModel()).ToArray();


        _saveSourcesCallback();

        OnPropertyChanged(nameof(ProviderType));
        OnPropertyChanged(nameof(ProviderInstance));
        OnPropertyChanged(nameof(ProviderDisplayName));
        OnPropertyChanged(nameof(ProviderDisplayVersion));
        OnPropertyChanged(nameof(IsNewerVersionAvailable));
    }

    public void UpdateSettings(SettingDetachedPack[] newSettings)
    {
        //todo implememnt this
        MessageBox.Show("ipd");
    }
}