using Nuke.Common.IO;
using Rachkov.InspectaQueue.AutoUpdater.Core.Models;

namespace Rachkov.InspectaQueue.AutoUpdater.Core.Services.Download;

public interface IDownloadService
{
    Task<ReleaseInfo?> FetchReleaseInfoAsync(CancellationToken cancellationToken = default);
    Task<bool> TryDownloadAssetAsync(Asset asset, AbsolutePath downloadPath, CancellationToken cancellationToken = default);
}