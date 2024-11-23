using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.Config.Models;
using System.IO;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.Config;

public class JsonFileConfigStoreService : IConfigStoreService
{
    private const string StorageFileName = "config.json";
    private readonly JsonSerializerSettings _serializerSettings = new()
    {
        Converters = new List<JsonConverter>()
        {
            new StringEnumConverter()
        },
    };

    private SettingsDto? _settingsCache;
    private readonly object _settingsWriteLock = new();

    public void StoreSources(SourceDto[] sources)
    {
        var settings = GetSettings();
        settings.Sources = sources;
        StoreSettings(settings);
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
        lock (_settingsWriteLock)
        {
            _settingsCache = null;
            var text = JsonConvert.SerializeObject(settings, Formatting.Indented, _serializerSettings);
            File.WriteAllText(StorageFileName, text);
            _settingsCache = settings;
        }
    }

    public void UpdateAndStore(Action<SettingsDto> update)
    {
        var settings = GetSettings();
        update(settings);
        StoreSettings(settings);
    }
}