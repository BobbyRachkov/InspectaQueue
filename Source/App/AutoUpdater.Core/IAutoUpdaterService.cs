using Rachkov.InspectaQueue.Abstractions.EventArgs;
using Rachkov.InspectaQueue.Abstractions.Models;

namespace Rachkov.InspectaQueue.Abstractions;

public interface IAutoUpdaterService
{
    event EventHandler<JobStatusChangedEventArgs>? JobStatusChanged;
    event EventHandler<StageStatusChangedEventArgs>? StageStatusChanged;

    Task<ReleaseInfo?> GetReleaseInfo();
    Version GetExecutingAppVersion();
    Task<bool> EnsureInstallerUpToDate();
    Task<bool> FreshInstall();
    Task<bool> Update(bool prerelease = false);
    Task<bool> SilentUpdate(bool prerelease = false);
}