using Rachkov.InspectaQueue.WpfDesktopApp.Services.Config;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.Config.Models;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings.Translators;

internal static class SourceTranslator
{
    public static SourceDto ToDto(this SourceViewModel sourceViewModel)
    {
        return new SourceDto
        {
            Id = sourceViewModel.Id,
            Name = sourceViewModel.Name,
            ProviderType = ProviderTypeConverter.GetProviderStringRepresentation(sourceViewModel.ProviderType),
            Settings = sourceViewModel.Settings.Select(x => x.ToDto()).ToArray()
        };
    }
}