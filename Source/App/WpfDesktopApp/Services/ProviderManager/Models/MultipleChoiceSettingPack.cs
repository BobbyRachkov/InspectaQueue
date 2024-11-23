namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager.Models;

public class MultipleChoiceSettingPack : BasicSettingPack
{
    public required MultipleChoiceEntry[] Options { get; init; }
    public bool MultipleSelectionEnabled { get; init; } = false;
}