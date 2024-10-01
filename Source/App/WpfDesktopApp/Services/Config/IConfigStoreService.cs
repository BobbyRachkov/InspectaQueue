using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.Config.Models;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.Config;

public interface IConfigStoreService
{
    void StoreSources(SourceViewModel[] sources);
    SettingsDto GetSettings();
    void StoreSettings(SettingsDto settings);
    void UpdateAndStore(Action<SettingsDto> update);
}