using System.IO;
using Newtonsoft.Json;
using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.Config.Models;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.Config.Translators;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.Config;

public class ConfigStoreService : IConfigStoreService
{
    private const string StorageFileName = "config.json";

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

        var text = File.ReadAllText(StorageFileName);
        return JsonConvert.DeserializeObject<SettingsDto>(text) ?? new SettingsDto();
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
        var text = JsonConvert.SerializeObject(settings, Formatting.Indented);
        File.WriteAllText(StorageFileName, text);
    }

    private void UpdateSources(SourceDto[] sources)
    {
        var settings = GetSettings();
        settings.Sources = sources;
        StoreSettings(settings);
    }
}