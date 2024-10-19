using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager.Models;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.Config;

public class SourceReader : ISourceReader
{
    private readonly IConfigStoreService _configStore;
    private readonly IProviderManager _providerManager;
    private readonly ISettingsManager _settingsManager;

    public SourceReader(
        IConfigStoreService configStore,
        IProviderManager providerManager,
        ISettingsManager settingsManager)
    {
        _configStore = configStore;
        _providerManager = providerManager;
        _settingsManager = settingsManager;
    }

    public IEnumerable<SourceViewModel> ReadSources()
    {
        var storedSources = _configStore.GetSettings().Sources;
        var activeProvidersArray = _providerManager.GetAllProviderVersions().ToArray();


        foreach (var storedSource in storedSources)
        {
            var provider = activeProvidersArray.FirstOrDefault(x =>
                ProviderTypeConverter.GetProviderStringRepresentation(x.GetType()) == storedSource.ProviderType);

            if (provider is null)
            {
                continue;
            }

            var settings = _settingsManager.ExtractSettings(provider);

            var mergedSettings =
                _settingsManager.MergePacks(
                    settings,
                    storedSource.Settings.Select(x => new SettingDetachedPack
                    {
                        PropertyName = x.PropertyName,
                        Value = x.Value
                    }));

            yield return new SourceViewModel(
                storedSource.Id,
                storedSource.Name,
                provider,
                mergedSettings.Select(x => new SettingEntryViewModel(x)).ToArray());
        }
    }
}