namespace Rachkov.InspectaQueue.Abstractions;

public interface IAutoUpdaterService
{
    Task<(Version version, string? downloadUrl)?> GetLatestVersion(ReleaseType releaseType);
    Task DownloadVersion(string downloadPath, string downloadUrl);
    Version GetAppVersion();
    void RunFinalCopyScript();
}