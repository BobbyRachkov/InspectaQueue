using Rachkov.InspectaQueue.Abstractions.Models;

namespace Rachkov.InspectaQueue.Abstractions;

public interface IDownloadService
{
    Task<ReleaseInfo?> FetchReleaseInfoAsync();
    Task<bool> TryDownloadAssetAsync(Asset asset, string downloadPath);
}