using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;
using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings.Translators;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.ImportExport;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager.Models;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.Config;

public class SourceReader : ISourceReader
{
    private readonly IConfigStoreService _configStore;
    private readonly IProviderManager _providerManager;
    private readonly ISettingsManager _settingsManager;
    private readonly ISettingImportExportService _settingImportExportService;

    public SourceReader(
        IConfigStoreService configStore,
        IProviderManager providerManager,
        ISettingsManager settingsManager,
        ISettingImportExportService settingImportExportService)
    {
        _configStore = configStore;
        _providerManager = providerManager;
        _settingsManager = settingsManager;
        _settingImportExportService = settingImportExportService;
    }

    public IEnumerable<SourceViewModel> ReadSources(Action saveSourcesCallback)
    {
        var storedSources = _configStore.GetSettings().Sources;
        var activeProvidersArray = _providerManager.GetAllProvidersVersions().ToArray();


        foreach (var storedSource in storedSources)
        {
            var provider = activeProvidersArray.FirstOrDefault(x =>
                ProviderTypeConverter.GetProviderStringRepresentation(x.GetType()) == storedSource.ProviderType);

            if (provider is null)
            {
                var possibleProviderVersions = activeProvidersArray.Where(x =>
                    storedSource.ProviderType.Contains(ProviderTypeConverter.GetProviderStringRepresentationWithoutVersion(x.GetType())));

                provider = possibleProviderVersions
                    .Select(x =>
                    {
                        var versionString = x.GetType().Assembly.GetName().Version?.ToString();
                        versionString ??= "0.0.0.0";
                        return new
                        {
                            instance = x,
                            version = new Version(versionString)
                        };
                    })
                    .OrderByDescending(x => x.version)
                    .FirstOrDefault()
                    ?.instance;

            }

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
                    }))
                    .EnsureCorrectTypes(_settingsManager)
                    .ToArray();

            yield return new SourceViewModel(
                storedSource.Id,
                storedSource.Name,
                _settingsManager,
                provider,
                _providerManager.GetProviderByInstance(provider).Versions,
                mergedSettings.Select(x => x.ToViewModel()).ToArray(),
                saveSourcesCallback);
        }
    }
}