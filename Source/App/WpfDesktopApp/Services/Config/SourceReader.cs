using Rachkov.InspectaQueue.Abstractions;
using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.Config.Models;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.SettingsParser;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.Config;

public class SourceReader : ISourceReader
{
    private readonly IConfigStoreService _configStore;
    private readonly ISettingsParser _settingsParser;

    public SourceReader(IConfigStoreService configStore, ISettingsParser settingsParser)
    {
        _configStore = configStore;
        _settingsParser = settingsParser;
    }

    public IEnumerable<SourceViewModel> ReadSources(IEnumerable<IQueueProvider> activeProviders)
    {
        var storedSources = _configStore.GetSettings().Sources;
        var activeProvidersArray = activeProviders.ToArray();


        foreach (var storedSource in storedSources)
        {
            var provider = activeProvidersArray.FirstOrDefault(x =>
                x.GetType().FullName == storedSource.ProviderType.FullName);

            if (provider is null)
            {
                continue;
            }

            var liveSettings = _settingsParser.ParseMembers(provider).ToArray();
            FillSettings(liveSettings, storedSource.Settings);
            yield return new SourceViewModel(storedSource.Name, provider, liveSettings);
        }
    }

    private void FillSettings(SettingEntryViewModel[] liveSettings, SourceSettingEntryDto[] storedSourceSettings)
    {
        foreach (var storedSetting in storedSourceSettings)
        {
            var correspondingLiveSetting = liveSettings.FirstOrDefault(x =>
                x.PropertyName == storedSetting.PropertyName
                && x.Type == storedSetting.Type);

            if (correspondingLiveSetting is null)
            {
                continue;
            }

            correspondingLiveSetting.Value = storedSetting.Value;
        }
    }
}