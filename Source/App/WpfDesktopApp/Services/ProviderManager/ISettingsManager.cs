using Rachkov.InspectaQueue.Abstractions.Messaging;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager.Models;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager;

public interface ISettingsManager
{
    IEnumerable<ISettingPack> ExtractSettings(IQueueProvider queueProvider);

    IEnumerable<ISettingPack> MergePacks(
        IEnumerable<ISettingPack> @base,
        IEnumerable<ISettingPack> overriding);

    IEnumerable<ISettingPack> MergePacks(
        IEnumerable<ISettingPack> @base,
        IEnumerable<SettingDetachedPack> overriding);

    IEnumerable<ISettingPack> EnsureCorrectTypes(IEnumerable<ISettingPack> settings);
}