using Rachkov.InspectaQueue.WpfDesktopApp.Services.ImportExport.Models;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.JsonService;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager.Models;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.ImportExport;

public class SettingImportExportService : ISettingImportExportService
{
    private readonly IJsonService _jsonService;
    private readonly ICypherService _cypherService;

    public SettingImportExportService(IJsonService jsonService, ICypherService cypherService)
    {
        _jsonService = jsonService;
        _cypherService = cypherService;
    }

    public string PrepareForExport(IEnumerable<SettingDetachedPack> settings)
    {
        var settingsForExport = ConvertForExport(settings).ToArray();
        var settingsJson = _jsonService.Serialize(settingsForExport);
        return _cypherService.Encode(settingsJson);
    }

    public IEnumerable<SettingDetachedPack>? ConvertFromImport(string settingsCypher)
    {
        try
        {
            var decodedJson = _cypherService.Decode(settingsCypher);
            var parsedSettings = _jsonService.Deserialize<SettingDto[]>(decodedJson);
            if (parsedSettings is null)
            {
                return null;
            }

            return ConvertForImport(parsedSettings);
        }
        catch
        {
            return null;
        }
    }

    private IEnumerable<SettingDto> ConvertForExport(IEnumerable<SettingDetachedPack> settings)
    {
        foreach (var settingDetachedPack in settings)
        {
            if (settingDetachedPack.Value is null
                || (settingDetachedPack.Value is string str
                    && string.IsNullOrWhiteSpace(str)))
            {
                continue;
            }

            yield return new SettingDto
            {
                PropertyName = settingDetachedPack.PropertyName,
                Value = settingDetachedPack.Value
            };
        }
    }

    private IEnumerable<SettingDetachedPack> ConvertForImport(IEnumerable<SettingDto> settings)
    {
        foreach (var settingDetachedPack in settings)
        {
            yield return new SettingDetachedPack
            {
                PropertyName = settingDetachedPack.PropertyName,
                Value = settingDetachedPack.Value
            };
        }
    }
}