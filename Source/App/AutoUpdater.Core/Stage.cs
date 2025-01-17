namespace Rachkov.InspectaQueue.Abstractions;

public enum Stage
{
    DownloadingRelease,
    VerifyingInstaller,
    DownloadingInstaller,
    Unzipping,
    CopyingFiles,
    CleaningUp,
    WaitingAppToClose,
    LaunchApp
}