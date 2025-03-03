namespace Rachkov.InspectaQueue.WpfDesktopApp.Services.Config.Models;

public class SettingsDto
{
    public bool IsAutoUpdaterEnabled { get; set; } = true;

    public bool IsAutoUpdaterBetaReleaseChannel { get; set; } = false;

    public int SelectedActionIndex { get; set; } = 0;

    public string AppVersion { get; set; } = "0.0.0";

    public SourceDto[] Sources { get; set; } = [];
}