namespace Rachkov.InspectaQueue.AutoUpdater.Migrations.Contracts.Config.v1;

internal class SettingsDto
{
    public bool IsAutoUpdaterEnabled { get; set; } = true;

    public bool IsAutoUpdaterBetaReleaseChannel { get; set; } = false;

    public int SelectedActionIndex { get; set; } = 0;

    public string AppVersion { get; set; } = "0.0.0";

    public SourceDto[] Sources { get; set; } = [];
}