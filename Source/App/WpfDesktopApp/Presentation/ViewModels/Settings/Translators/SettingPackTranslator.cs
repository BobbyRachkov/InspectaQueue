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
            Options = settingPack.Options.Select(x => new DropdownOptionViewModel(x.DisplayName, x.Value)).ToArray(),
            Modifiers = settingPack.Modifiers
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
            Value = settingPack.Value,
            Modifiers = settingPack.Modifiers
        };
    }

    public static ISettingPack ToModel(this ISettingViewModel settingPack)
    {
        return settingPack switch
        {
            MultipleChoiceSettingViewModel multipleChoiceSettingPack => HandleMultipleChoice(multipleChoiceSettingPack),
            _ => HandleBasicPack(settingPack)
        };
    }

    private static ISettingPack HandleMultipleChoice(MultipleChoiceSettingViewModel settingPack)
    {
        return new MultipleChoiceSettingPack()
        {
            Name = settingPack.Name,
            ToolTip = settingPack.ToolTip,
            PropertyName = settingPack.PropertyName,
            ReflectedProperty = settingPack.ReflectedProperty,
            Type = settingPack.Type,
            Value = settingPack.Value,
            MultipleSelectionEnabled = settingPack.IsMultiSelectEnabled,
            Options = settingPack.Options.Select(x => new MultipleChoiceEntry
            {
                DisplayName = x.DisplayName,
                Value = x.BackingValue
            }).ToArray(),
            Modifiers = settingPack.Modifiers
        };
    }

    private static ISettingPack HandleBasicPack(ISettingViewModel settingPack)
    {
        return new BasicSettingPack()
        {
            Name = settingPack.Name,
            ToolTip = settingPack.ToolTip,
            PropertyName = settingPack.PropertyName,
            ReflectedProperty = settingPack.ReflectedProperty,
            Type = settingPack.Type,
            Value = settingPack.Value,
            Modifiers = settingPack.Modifiers
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