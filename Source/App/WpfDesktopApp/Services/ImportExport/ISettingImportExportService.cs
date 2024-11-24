using Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager.Models;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.ImportExport;

public interface ISettingImportExportService
{
    string PrepareForExport(IEnumerable<SettingDetachedPack> settings);
    IEnumerable<SettingDetachedPack> ConvertFromImport(string settingsCypher);
}