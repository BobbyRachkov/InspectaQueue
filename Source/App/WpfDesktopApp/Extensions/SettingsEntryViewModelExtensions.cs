using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Extensions;

public static class SettingsEntryViewModelExtensions
{
    public static SettingEntryViewModel[] Clone(this SettingEntryViewModel[] source)
    {
        var result = new SettingEntryViewModel[source.Length];

        for (var i = 0; i < source.Length; i++)
        {
            result[i] = new SettingEntryViewModel(source[i].SettingsInstance.Clone());
        }

        return result;
    }
}