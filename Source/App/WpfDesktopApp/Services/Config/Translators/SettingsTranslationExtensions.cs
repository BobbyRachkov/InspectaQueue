using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.Config.Models;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.Config.Translators;

public static class SettingsTranslationExtensions
{
    public static SourceSettingEntryDto ToSourceSettingEntry(this SettingEntryViewModel settingEntryEntry)
    {
        return new SourceSettingEntryDto
        {
            PropertyName = settingEntryEntry.PropertyName,
            Type = settingEntryEntry.Type,
            Value = settingEntryEntry.Value
        };
    }

    public static SourceSettingEntryDto[] ToSourceSettingEntries(this SettingEntryViewModel[] settings)
    {
        return settings.Select(x => x.ToSourceSettingEntry()).ToArray();
    }
}