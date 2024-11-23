using Rachkov.InspectaQueue.WpfDesktopApp.Services.Config.Models;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.Config;

public interface IConfigStoreService
{
    void StoreSources(SourceDto[] sources);
    SettingsDto GetSettings();
    void StoreSettings(SettingsDto settings);
    void UpdateAndStore(Action<SettingsDto> update);
}