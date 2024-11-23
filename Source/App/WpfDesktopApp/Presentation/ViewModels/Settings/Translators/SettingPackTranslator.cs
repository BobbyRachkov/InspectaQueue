using Rachkov.InspectaQueue.WpfDesktopApp.Services.Config.Models;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager.Models;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings.Translators;

internal static class SettingPackTranslator
{
    public static ISettingViewModel ToViewModel(this ISettingPack settingPack)
    {
        return settingPack switch
        {
            MultipleChoiceSettingPack multipleChoiceSettingPack => HandleMultipleChoice(multipleChoiceSettingPack),
            _ => HandleBasicPack(settingPack)
        };
    }

    private static ISettingViewModel HandleMultipleChoice(MultipleChoiceSettingPack settingPack)
    {
        return new MultipleChoiceSettingViewModel()
        {
            Name = settingPack.Name,
            ToolTip = settingPack.ToolTip,
            PropertyName = settingPack.PropertyName,
            ReflectedProperty = settingPack.ReflectedProperty,
            Type = settingPack.Type,
            Value = settingPack.Value,
            IsMultiSelectEnabled = settingPack.MultipleSelectionEnabled,
            Options = settingPack.Options.Select(x => new DropdownOptionViewModel(x.DisplayName, x.Value)).ToArray()
        };
    }

    private static ISettingViewModel HandleBasicPack(ISettingPack settingPack)
    {
        return new BasicSettingViewModel
        {
            Name = settingPack.Name,
            ToolTip = settingPack.ToolTip,
            PropertyName = settingPack.PropertyName,
            ReflectedProperty = settingPack.ReflectedProperty,
            Type = settingPack.Type,
            Value = settingPack.Value
        };
    }

    public static ISettingPack ToModel(this ISettingViewModel settingPack)
    {
        return new BasicSettingPack
        {
            Name = settingPack.Name,
            ToolTip = settingPack.ToolTip,
            PropertyName = settingPack.PropertyName,
            ReflectedProperty = settingPack.ReflectedProperty,
            Type = settingPack.Type,
            Value = settingPack.Value
        };
    }

    public static SourceSettingEntryDto ToDto(this ISettingViewModel settingPack)
    {
        return new SourceSettingEntryDto
        {
            PropertyName = settingPack.PropertyName,
            Value = settingPack.Value
        };
    }
}