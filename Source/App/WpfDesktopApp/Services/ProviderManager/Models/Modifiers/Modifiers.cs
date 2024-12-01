namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager.Models.Modifiers;

public class Modifiers
{
    public bool IsFilePath => FilePath is not null;
    public FilePathModifier? FilePath { get; init; }

    public bool IsSecret => Secret is not null;
    public SecretModifier? Secret { get; init; }

    public Modifiers Clone()
    {
        return new Modifiers
        {
            FilePath = !IsFilePath ? null : new FilePathModifier(),
            Secret = !IsSecret ? null : new SecretModifier()
        };
    }
}