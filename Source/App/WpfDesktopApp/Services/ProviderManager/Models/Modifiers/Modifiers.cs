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
            FilePath = FilePath is null ? null : new FilePathModifier
            {
                Title = FilePath!.Title,
                Filter = FilePath.Filter
            },
            Secret = Secret is null ? null : new SecretModifier
            {
                CanBeRevealed = Secret.CanBeRevealed,
                PasswordChar = Secret.PasswordChar
            }
        };
    }
}