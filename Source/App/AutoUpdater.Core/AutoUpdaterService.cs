using Rachkov.InspectaQueue.Abstractions.EventArgs;
using Rachkov.InspectaQueue.Abstractions.Models;
using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;

namespace Rachkov.InspectaQueue.Abstractions;

public sealed class AutoUpdaterService : IAutoUpdaterService
{
    private readonly IDownloadService _downloadService;
    private readonly IApplicationPathsConfiguration _applicationPathsConfiguration;
    private ReleaseInfo? _releaseInfo;

    public AutoUpdaterService(
        IDownloadService downloadService,
        IApplicationPathsConfiguration applicationPathsConfiguration)
    {
        _downloadService = downloadService;
        _applicationPathsConfiguration = applicationPathsConfiguration;
    }

    public event EventHandler<JobStatusChangedEventArgs>? JobStatusChanged;
    public event EventHandler<StageStatusChangedEventArgs>? StageStatusChanged;

    public async Task<ReleaseInfo?> GetReleaseInfo()
    {
        if (_releaseInfo is null)
        {
            await UpdateReleaseInfo();
        }

        return _releaseInfo;
    }

    public Version GetExecutingAppVersion()
    {
        Assembly assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
        return new Version(fvi.FileVersion ?? assembly.GetName().Version?.ToString() ?? "0.0.0");
    }

    public async Task<bool> DownloadRelease(bool prerelease)
    {
        try
        {
            RaiseStageStatusChanged(Stage.DownloadingRelease, StageStatus.InProgress);

            var releaseInfo = await _downloadService.FetchReleaseInfoAsync();

            if (releaseInfo is null)
            {
                return FailStage(Stage.DownloadingRelease);
            }

            if (releaseInfo.Latest.WindowsAppZip is null)
            {
                return FailStage(Stage.DownloadingRelease);
            }

            var assetForDownloading = prerelease && releaseInfo.Prerelease?.WindowsAppZip is not null
                ? releaseInfo.Prerelease.WindowsAppZip
                : releaseInfo.Latest.WindowsAppZip;

            var downloadResult = await _downloadService.TryDownloadAssetAsync(assetForDownloading, _applicationPathsConfiguration.IqUpdateZipPath);

            if (!downloadResult)
            {
                return FailStage(Stage.DownloadingRelease);
            }

            return PassStage(Stage.DownloadingRelease);
        }
        catch
        {
            return FailStage(Stage.DownloadingRelease);
        }
    }

    public Task<bool> Unzip()
    {
        try
        {
            RaiseStageStatusChanged(Stage.Unzipping, StageStatus.InProgress);

            ZipFile.ExtractToDirectory(
                _applicationPathsConfiguration.IqUpdateZipPath,
                _applicationPathsConfiguration.IqExtractedZipFolderPath);

            return Task.FromResult(PassStage(Stage.Unzipping));
        }
        catch
        {
            return Task.FromResult(FailStage(Stage.Unzipping));
        }
    }

    //public void RunFinalCopyScript()
    //{
    //    var scriptPath = "..\\restore.bat";
    //    var scriptFullPath = Path.GetFullPath(scriptPath);

    //    File.WriteAllText(scriptPath, Constants.Script.Finalize2);
    //    ProcessStartInfo psi = new ProcessStartInfo
    //    {
    //        FileName = "cmd.exe",
    //        WorkingDirectory = Path.GetDirectoryName(scriptFullPath),
    //        Arguments = "/c " + Path.GetFileName(scriptFullPath),
    //        UseShellExecute = false,
    //        CreateNoWindow = false, //
    //        RedirectStandardOutput = false,
    //        RedirectStandardError = false,
    //        RedirectStandardInput = false,
    //        WindowStyle = ProcessWindowStyle.Normal
    //    };

    //    Process cmdProcess = Process.Start(psi);
    //    cmdProcess.Dispose();
    //}

    private async Task UpdateReleaseInfo()
    {
        _releaseInfo = await _downloadService.FetchReleaseInfoAsync();
    }

    private void RaiseJobStatusChanged(bool isJobRunning, Stage[] stages)
    {
        JobStatusChanged?.Invoke(this, new JobStatusChangedEventArgs
        {
            IsJobRunning = isJobRunning,
            Stages = stages
        });
    }

    private void RaiseStageStatusChanged(Stage stage, StageStatus status)
    {
        StageStatusChanged?.Invoke(this, new StageStatusChangedEventArgs
        {
            Stage = stage,
            Status = status
        });
    }

    private bool FailStage(Stage stage)
    {
        RaiseStageStatusChanged(stage, StageStatus.Failed);
        return false;
    }

    private bool PassStage(Stage stage)
    {
        RaiseStageStatusChanged(stage, StageStatus.Done);
        return true;
    }
}