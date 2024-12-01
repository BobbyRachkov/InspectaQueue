namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager.Models.Modifiers;

public class FilePathModifier
{
    public string Filter { get; init; } = "*.*";
    public required string Title { get; init; }
}