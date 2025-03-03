namespace Rachkov.InspectaQueue.AutoUpdater.Core;

public enum Stage
{
    DownloadingRelease,
    VerifyingInstaller,
    DownloadingInstaller,
    Unzipping,
    CopyingFiles,
    CleaningUp,
    WaitingAppToClose,
    LaunchApp,
    Uninstalling,
    FinalizingSetup,
    FinalizingRemoval,
    InstallPrerequisites
}