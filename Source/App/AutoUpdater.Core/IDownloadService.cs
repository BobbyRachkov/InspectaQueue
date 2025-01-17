using Rachkov.InspectaQueue.AutoUpdater.Core.Models;

namespace Rachkov.InspectaQueue.AutoUpdater.Core;

public interface IDownloadService
{
    Task<ReleaseInfo?> FetchReleaseInfoAsync(CancellationToken cancellationToken = default);
    Task<bool> TryDownloadAssetAsync(Asset asset, string downloadPath, CancellationToken cancellationToken = default);
}