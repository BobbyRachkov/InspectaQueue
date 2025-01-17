using Rachkov.InspectaQueue.AutoUpdater.Core.EventArgs;
using Rachkov.InspectaQueue.AutoUpdater.Core.Models;

namespace Rachkov.InspectaQueue.AutoUpdater.Core;

public interface IAutoUpdaterService
{
    event EventHandler<JobStatusChangedEventArgs>? JobStatusChanged;
    event EventHandler<StageStatusChangedEventArgs>? StageStatusChanged;

    Task<ReleaseInfo?> GetReleaseInfo(CancellationToken cancellationToken = default);
    Version GetExecutingAppVersion();
    Task<bool> EnsureInstallerUpToDate(CancellationToken cancellationToken = bad);
    Task<bool> FreshInstall(CancellationToken cancellationToken = bad);
    Task<bool> Update(bool prerelease = false, CancellationToken cancellationToken = bad);
    Task<bool> SilentUpdate(bool prerelease = false, CancellationToken cancellationToken = bad);
}