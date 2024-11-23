using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;
using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings.Translators;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.Config;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Extensions;

internal static class ConfigStoreExtensions
{
    public static void StoreSources(this IConfigStoreService configStoreService, IEnumerable<SourceViewModel> sources)
    {
        configStoreService.StoreSources(sources.Select(x => x.ToDto()).ToArray());
    }
}