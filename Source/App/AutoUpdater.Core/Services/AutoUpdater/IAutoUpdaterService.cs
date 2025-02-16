using Rachkov.InspectaQueue.AutoUpdater.Core.EventArgs;
using Rachkov.InspectaQueue.AutoUpdater.Core.Models;

namespace Rachkov.InspectaQueue.AutoUpdater.Core.Services.AutoUpdater;

public interface IAutoUpdaterService
{
    event EventHandler<JobStatusChangedEventArgs>? JobStatusChanged;
    event EventHandler<StageStatusChangedEventArgs>? StageStatusChanged;

    Task<ReleaseInfo?> GetReleaseInfo(CancellationToken cancellationToken = default);
    Version GetExecutingAppVersion();
    Task<bool> EnsureInstallerUpToDate(CancellationToken cancellationToken = default);
    Task<bool> FreshInstall(CancellationToken cancellationToken = default);
    Task<bool> Update(bool prerelease = false, CancellationToken cancellationToken = default);
    Task<bool> SilentUpdate(bool prerelease = false, CancellationToken cancellationToken = default);
    Task<bool> Uninstall(bool removeConfig = false, CancellationToken cancellationToken = default);
}