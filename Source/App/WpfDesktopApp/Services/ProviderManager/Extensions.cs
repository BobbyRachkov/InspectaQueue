using Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager.Models;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager;

public static class Extensions
{
    public static IEnumerable<ISettingPack> EnsureCorrectTypes(this IEnumerable<ISettingPack> settings, ISettingsManager settingsManager)
    {
        return settingsManager.EnsureCorrectTypes(settings);
    }
}