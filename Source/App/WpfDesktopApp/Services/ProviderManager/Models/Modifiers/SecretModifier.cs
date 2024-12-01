namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager.Models.Modifiers;

public class SecretModifier
{
    public bool CanBeRevealed { get; init; } = false;
    public char PasswordChar { get; init; } = '\u25cf';
}