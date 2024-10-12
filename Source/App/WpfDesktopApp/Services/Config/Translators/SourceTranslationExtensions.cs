using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.Config.Models;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.Config.Translators;

public static class SourceTranslationExtensions
{
    public static SourceDto ToSourceDto(this SourceViewModel sourceViewModel)
    {
        return new SourceDto
        {
            Id = sourceViewModel.Id,
            Name = sourceViewModel.Name,
            ProviderType = ProviderTypeConverter.GetProviderStringRepresentation(sourceViewModel.ProviderType),
            Settings = sourceViewModel.Settings.ToSourceSettingEntries()
        };
    }
}