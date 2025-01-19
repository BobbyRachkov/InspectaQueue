namespace Rachkov.InspectaQueue.AutoUpdater.Core.Config.v1;

internal class SettingsDto
{
    public bool IsAutoUpdaterEnabled { get; set; } = true;
    public bool IsAutoUpdaterAlphaReleaseChannel { get; set; } = false;

    public string ConfigVersion { get; set; } = "v1";

    public SourceDto[] Sources { get; set; } = [];
}