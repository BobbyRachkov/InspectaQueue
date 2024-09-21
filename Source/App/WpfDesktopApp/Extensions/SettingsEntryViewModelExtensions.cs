using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Extensions;

public static class SettingsEntryViewModelExtensions
{
    public static SettingEntryViewModel[] Copy(this SettingEntryViewModel[] source)
    {
        var result = new SettingEntryViewModel[source.Length];

        for (var i = 0; i < source.Length; i++)
        {
            result[i] = source[i].Copy();
        }

        return result;
    }
    public static SettingEntryViewModel Copy(this SettingEntryViewModel source)
    {
        return new SettingEntryViewModel
        {
            Name = source.Name,
            ToolTip = source.ToolTip,
            PropertyName = source.PropertyName,
            ReflectedProperty = source.ReflectedProperty,
            Type = source.Type,
            Value = source.Value
        };
    }
}