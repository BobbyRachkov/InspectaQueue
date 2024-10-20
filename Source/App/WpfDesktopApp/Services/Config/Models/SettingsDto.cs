namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.Config.Models;

public class SettingsDto
{
    public bool IsAutoUpdaterEnabled { get; set; } = true;
    public bool IsAutoUpdaterBetaReleaseChannel { get; set; } = false;

    public string ConfigVersion { get; set; } = "v1";

    public SourceDto[] Sources { get; set; } = [];
}