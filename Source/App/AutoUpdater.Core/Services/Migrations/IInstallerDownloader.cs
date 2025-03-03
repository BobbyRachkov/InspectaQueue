using Nuke.Common.IO;

namespace Rachkov.InspectaQueue.AutoUpdater.Core.Services.Migrations;

public interface IInstallerDownloader
{
    Task<bool> DownloadInstaller(string url, AbsolutePath downloadPath, CancellationToken cancellationToken = default);
}