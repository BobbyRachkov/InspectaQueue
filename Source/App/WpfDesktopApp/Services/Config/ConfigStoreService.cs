using Newtonsoft.Json;
using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.Config.Models;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.Config.Translators;
using System.IO;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.Config;

public class ConfigStoreService : IConfigStoreService
{
    private const string StorageFileName = "config.json";

    private SettingsDto? _settingsCache;

    public void StoreSources(SourceViewModel[] sources)
    {
        var sourceDtos = new SourceDto[sources.Length];

        for (int i = 0; i < sources.Length; i++)
        {
            sourceDtos[i] = sources[i].ToSourceDto();
        }

        UpdateSources(sourceDtos);
    }

    public SettingsDto GetSettings()
    {
        EnsureFileExists();

        if (_settingsCache is not null)
        {
            return _settingsCache;
        }

        var text = File.ReadAllText(StorageFileName);
        _settingsCache = JsonConvert.DeserializeObject<SettingsDto>(text) ?? new SettingsDto();
        return _settingsCache;
    }

    private void EnsureFileExists()
    {
        if (!File.Exists(StorageFileName))
        {
            StoreSettings(new SettingsDto());
        }
    }

    public void StoreSettings(SettingsDto settings)
    {
        _settingsCache = null;
        var text = JsonConvert.SerializeObject(settings, Formatting.Indented);
        File.WriteAllText(StorageFileName, text);
        _settingsCache = settings;
    }

    private void UpdateSources(SourceDto[] sources)
    {
        var settings = GetSettings();
        settings.Sources = sources;
        StoreSettings(settings);
    }

    public void UpdateAndStore(Action<SettingsDto> update)
    {
        var settings = GetSettings();
        update(settings);
        StoreSettings(settings);
    }
}