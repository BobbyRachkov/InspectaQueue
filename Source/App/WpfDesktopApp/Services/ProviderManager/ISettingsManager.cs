using Rachkov.InspectaQueue.Abstractions;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager.Models;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager;

public interface ISettingsManager
{
    IEnumerable<SettingPack> ExtractSettings(IQueueProvider queueProvider);

    IEnumerable<SettingPack> MergePacks(
        IEnumerable<SettingPack> @base,
        IEnumerable<SettingPack> overriding);

    IEnumerable<SettingPack> MergePacks(
        IEnumerable<SettingPack> @base,
        IEnumerable<SettingDetachedPack> overriding);
}