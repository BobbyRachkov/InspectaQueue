namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager.Models;

public class MultipleChoiceSettingPack : BasicSettingPack
{
    public required MultipleChoiceEntry[] Options { get; init; }
    public bool MultipleSelectionEnabled { get; init; } = false;

    public override ISettingPack Clone()
    {
        return new MultipleChoiceSettingPack
        {
            ReflectedProperty = ReflectedProperty,
            PropertyName = PropertyName,
            Name = Name,
            ToolTip = ToolTip,
            Type = Type,
            Value = Value,
            MultipleSelectionEnabled = MultipleSelectionEnabled,
            Options = Options.Select(x => new MultipleChoiceEntry()
            {
                DisplayName = x.DisplayName,
                Value = x.Value
            }).ToArray()
        };
    }
}