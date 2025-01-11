namespace Rachkov.InspectaQueue.Abstractions;

public enum Stage
{
    DownloadingRelease,
    DownloadingInstaller,
    Unzipping,
    CopyingFiles,
    CleaningUp,
    WaitingAppToClose
}