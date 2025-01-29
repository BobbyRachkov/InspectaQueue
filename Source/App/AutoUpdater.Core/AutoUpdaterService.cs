using Nuke.Common.IO;
using Rachkov.InspectaQueue.AutoUpdater.Core.EventArgs;
using Rachkov.InspectaQueue.AutoUpdater.Core.Models;
using Rachkov.InspectaQueue.AutoUpdater.Core.Services.Registrar;
using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;

namespace Rachkov.InspectaQueue.AutoUpdater.Core;

public sealed class AutoUpdaterService : IAutoUpdaterService
{
    private readonly IDownloadService _downloadService;
    private readonly IApplicationPathsConfiguration _applicationPathsConfiguration;
    private readonly IRegistrar _registrar;
    private ReleaseInfo? _releaseInfo;
    private readonly TimeSpan _consistentDelay = TimeSpan.FromMilliseconds(600);

    public AutoUpdaterService(
        IDownloadService downloadService,
        IApplicationPathsConfiguration applicationPathsConfiguration,
        IRegistrar registrar)
    {
        _downloadService = downloadService;
        _applicationPathsConfiguration = applicationPathsConfiguration;
        _registrar = registrar;
    }

    public event EventHandler<JobStatusChangedEventArgs>? JobStatusChanged;
    public event EventHandler<StageStatusChangedEventArgs>? StageStatusChanged;

    public async Task<ReleaseInfo?> GetReleaseInfo(CancellationToken cancellationToken = default)
    {
        if (_releaseInfo is null)
        {
            await UpdateReleaseInfo(cancellationToken);
        }

        return _releaseInfo;
    }

    public Version GetExecutingAppVersion()
    {
        Assembly assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
        return new Version(fvi.FileVersion ?? assembly.GetName().Version?.ToString() ?? "0.0.0");
    }

    #region Jobs

    public async Task<bool> EnsureInstallerUpToDate(CancellationToken cancellationToken = default)
    {
        RaiseJobStatusChanged(true, [Stage.VerifyingInstaller, Stage.DownloadingInstaller]);
        RaiseStageStatusChanged(Stage.VerifyingInstaller, StageStatus.InProgress);

        var releaseInfo = await GetReleaseInfo(cancellationToken);

        if (releaseInfo?.Latest.Installer?.Version is null)
        {
            RaiseJobStatusChanged(false);
            return FailStage(Stage.VerifyingInstaller);
        }

        var localInstallerVersion = _applicationPathsConfiguration.GetInstallerVersion();

        if (localInstallerVersion is not null
            && localInstallerVersion >= releaseInfo.Latest.Installer.Version)
        {
            RaiseStageStatusChanged(Stage.VerifyingInstaller, StageStatus.Done);
            RaiseStageStatusChanged(Stage.DownloadingInstaller, StageStatus.Skipped);
            RaiseJobStatusChanged(false);
            return true;
        }

        RaiseStageStatusChanged(Stage.VerifyingInstaller, StageStatus.Done);
        RaiseStageStatusChanged(Stage.DownloadingInstaller, StageStatus.InProgress);

        _applicationPathsConfiguration.InstallerPath.DeleteFile();
        var result = await DownloadInstaller(cancellationToken);

        if (!result)
        {
            RaiseStageStatusChanged(Stage.DownloadingInstaller, StageStatus.Failed);
            RaiseJobStatusChanged(false);
            return false;
        }

        _registrar.RegisterAppInProgramUninstallList();

        RaiseStageStatusChanged(Stage.DownloadingInstaller, StageStatus.Done);
        RaiseJobStatusChanged(false);
        return true;
    }

    public async Task<bool> FreshInstall(CancellationToken cancellationToken = default)
    {
        RaiseJobStatusChanged(true, [Stage.DownloadingRelease, Stage.Unzipping, Stage.CopyingFiles, Stage.CleaningUp, Stage.CreateDesktopShortcut, Stage.LaunchApp]);

        if (!await DownloadRelease(cancellationToken: cancellationToken))
        {
            return FailJob(Stage.Unzipping, Stage.CopyingFiles, Stage.CleaningUp, Stage.CreateDesktopShortcut, Stage.LaunchApp);
        }

        if (!await Unzip(cancellationToken))
        {
            return FailJob(Stage.CopyingFiles, Stage.CleaningUp, Stage.CreateDesktopShortcut, Stage.LaunchApp);
        }

        if (!await CopyFiles(cancellationToken))
        {
            return FailJob(Stage.CleaningUp, Stage.CreateDesktopShortcut, Stage.LaunchApp);
        }

        if (!await CleanUp(cancellationToken))
        {
            return FailJob(Stage.CreateDesktopShortcut, Stage.LaunchApp);
        }

        if (!await CreateDesktopShortcut(cancellationToken))
        {
            return FailJob(Stage.LaunchApp);
        }

        _registrar.RegisterAppInProgramUninstallList(_releaseInfo?.Latest.WindowsAppZip?.Version);

        if (!LaunchInspectaQueue())
        {
            return FailJob();
        }

        RaiseJobStatusChanged(false);
        return true;
    }

    public async Task<bool> Update(bool prerelease = false, CancellationToken cancellationToken = default)
    {
        RaiseJobStatusChanged(true, [Stage.DownloadingRelease, Stage.WaitingAppToClose, Stage.Unzipping, Stage.CopyingFiles, Stage.CleaningUp, Stage.LaunchApp]);

        _registrar.RegisterAppInProgramUninstallList(_releaseInfo?.Latest.WindowsAppZip?.Version);

        ;
        if (!await DownloadRelease(prerelease, cancellationToken))
        {
            return FailJob(Stage.Unzipping, Stage.CopyingFiles, Stage.CleaningUp, Stage.LaunchApp);
        }

        if (!await WaitInspectaQueueToExit(cancellationToken))
        {
            return FailJob(Stage.Unzipping, Stage.CopyingFiles, Stage.CleaningUp, Stage.LaunchApp);
        }

        if (!await Unzip(cancellationToken))
        {
            return FailJob(Stage.CopyingFiles, Stage.CleaningUp, Stage.LaunchApp);
        }

        if (!await CopyFiles(cancellationToken))
        {
            return FailJob(Stage.CleaningUp, Stage.LaunchApp);
        }

        if (!await CleanUp(cancellationToken))
        {
            return FailJob(Stage.LaunchApp);
        }

        if (prerelease)
        {
            _registrar.RegisterAppInProgramUninstallList(_releaseInfo?.Latest.WindowsAppZip?.Version);
        }
        else
        {
            _registrar.RegisterAppInProgramUninstallList(_releaseInfo?.Latest.WindowsAppZip?.Version);
        }

        if (!LaunchInspectaQueue())
        {
            return FailJob();
        }


        RaiseJobStatusChanged(false);
        return true;
    }

    public async Task<bool> SilentUpdate(bool prerelease = false, CancellationToken cancellationToken = default)
    {
        RaiseJobStatusChanged(true, [Stage.DownloadingRelease, Stage.WaitingAppToClose, Stage.Unzipping, Stage.CopyingFiles, Stage.CleaningUp]);

        if (!await DownloadRelease(prerelease, cancellationToken))
        {
            return FailJob(Stage.WaitingAppToClose, Stage.Unzipping, Stage.CopyingFiles, Stage.CleaningUp, Stage.LaunchApp);
        }

        if (!await WaitInspectaQueueToExit(cancellationToken))
        {
            return FailJob(Stage.Unzipping, Stage.CopyingFiles, Stage.CleaningUp, Stage.LaunchApp);
        }

        if (!await Unzip(cancellationToken))
        {
            return FailJob(Stage.CopyingFiles, Stage.CleaningUp, Stage.LaunchApp);
        }

        if (!await CopyFiles(cancellationToken))
        {
            return FailJob(Stage.CleaningUp, Stage.LaunchApp);
        }

        if (prerelease)
        {
            _registrar.RegisterAppInProgramUninstallList(_releaseInfo?.Latest.WindowsAppZip?.Version);
        }
        else
        {
            _registrar.RegisterAppInProgramUninstallList(_releaseInfo?.Latest.WindowsAppZip?.Version);
        }

        if (!await CleanUp(cancellationToken))
        {
            return FailJob(Stage.LaunchApp);
        }

        RaiseJobStatusChanged(false);
        return true;
    }

    public async Task<bool> Uninstall(bool removeConfig = false, CancellationToken cancellationToken = default)
    {
        RaiseJobStatusChanged(true, [Stage.Uninstalling]);

        await Task.Yield();

        if (!await UninstallInternal(cancellationToken: cancellationToken))
        {
            return FailJob(Stage.Uninstalling);
        }

        RaiseJobStatusChanged(false);
        return true;
    }

    #endregion

    #region Stages

    private async Task<bool> DownloadRelease(bool prerelease = false, CancellationToken cancellationToken = default)
    {
        try
        {
            RaiseStageStatusChanged(Stage.DownloadingRelease, StageStatus.InProgress);

            var releaseInfo = await GetReleaseInfo(cancellationToken);

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

            var downloadResult = await _downloadService.TryDownloadAssetAsync(
                assetForDownloading,
                _applicationPathsConfiguration.IqUpdateZipPath,
                cancellationToken);

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

    private async Task<bool> DownloadInstaller(CancellationToken cancellationToken = default)
    {
        try
        {
            RaiseStageStatusChanged(Stage.DownloadingInstaller, StageStatus.InProgress);
            await Task.Delay(_consistentDelay, cancellationToken);

            var releaseInfo = await GetReleaseInfo(cancellationToken);

            if (releaseInfo is null)
            {
                return FailStage(Stage.DownloadingInstaller);
            }

            if (releaseInfo.Latest.Installer is null)
            {
                return FailStage(Stage.DownloadingInstaller);
            }

            var downloadResult = await _downloadService.TryDownloadAssetAsync(
                releaseInfo.Latest.Installer,
                _applicationPathsConfiguration.GetInstallerPath(releaseInfo.Latest.Installer.Version),
                cancellationToken);

            if (!downloadResult)
            {
                return FailStage(Stage.DownloadingInstaller);
            }

            return PassStage(Stage.DownloadingInstaller);
        }
        catch
        {
            return FailStage(Stage.DownloadingInstaller);
        }
    }

    private async Task<bool> Unzip(CancellationToken cancellationToken = default)
    {
        try
        {
            RaiseStageStatusChanged(Stage.Unzipping, StageStatus.InProgress);
            await Task.Delay(_consistentDelay, cancellationToken);

            _applicationPathsConfiguration.IqExtractedZipDirectory.DeleteDirectory();
            ZipFile.ExtractToDirectory(
                _applicationPathsConfiguration.IqUpdateZipPath,
                _applicationPathsConfiguration.IqExtractedZipDirectory);

            return PassStage(Stage.Unzipping);
        }
        catch
        {
            return FailStage(Stage.Unzipping);
        }
    }

    private async Task<bool> CopyFiles(CancellationToken cancellationToken = default)
    {
        try
        {
            RaiseStageStatusChanged(Stage.CopyingFiles, StageStatus.InProgress);
            await Task.Delay(_consistentDelay, cancellationToken);

            if (_applicationPathsConfiguration.OldConfigFilePath.FileExists())
            {
                _applicationPathsConfiguration.OldConfigFilePath.Copy(_applicationPathsConfiguration.ConfigFilePath, ExistsPolicy.FileOverwrite);
            }

            if (_applicationPathsConfiguration.OldProvidersDirectory.DirectoryExists())
            {
                _applicationPathsConfiguration.OldProvidersDirectory.CopyToDirectory(_applicationPathsConfiguration.IqBaseDirectory, ExistsPolicy.MergeAndOverwrite);
            }

            _applicationPathsConfiguration.IqAppDirectory.DeleteDirectory();

            (_applicationPathsConfiguration.IqExtractedZipDirectory / "App").CopyToDirectory(
                _applicationPathsConfiguration.IqBaseDirectory);


            if (_applicationPathsConfiguration.IqExtractedProvidersDirectory.DirectoryExists())
            {
                _applicationPathsConfiguration.IqExtractedProvidersDirectory.CopyToDirectory(_applicationPathsConfiguration.IqBaseDirectory, ExistsPolicy.MergeAndOverwrite);
            }

            return PassStage(Stage.CopyingFiles);
        }
        catch
        {
            return FailStage(Stage.CopyingFiles);
        }
    }

    private async Task<bool> CleanUp(CancellationToken cancellationToken = default)
    {
        try
        {
            RaiseStageStatusChanged(Stage.CleaningUp, StageStatus.InProgress);
            await Task.Delay(_consistentDelay, cancellationToken);

            _applicationPathsConfiguration.IqExtractedZipDirectory.DeleteDirectory();
            _applicationPathsConfiguration.IqUpdateZipPath.DeleteFile();

            return PassStage(Stage.CleaningUp);
        }
        catch
        {
            return FailStage(Stage.CleaningUp);
        }
    }

    private async Task<bool> CreateDesktopShortcut(CancellationToken cancellationToken = default)
    {
        try
        {
            RaiseStageStatusChanged(Stage.CreateDesktopShortcut, StageStatus.InProgress);
            await Task.Delay(_consistentDelay, cancellationToken);

            if (!await _registrar.CreateDesktopShortcut(cancellationToken))
            {
                return FailStage(Stage.CreateDesktopShortcut);
            }

            return PassStage(Stage.CreateDesktopShortcut);
        }
        catch
        {
            return FailStage(Stage.CreateDesktopShortcut);
        }
    }

    private async Task<bool> UninstallInternal(bool deleteConfig = true, CancellationToken cancellationToken = default)
    {
        try
        {
            RaiseStageStatusChanged(Stage.Uninstalling, StageStatus.InProgress);
            await Task.Delay(_consistentDelay, cancellationToken);

            _applicationPathsConfiguration.IqAppDirectory.DeleteDirectory();
            _applicationPathsConfiguration.ConfigFilePath.DeleteFile();
            _applicationPathsConfiguration.ProvidersDirectory.DeleteDirectory();
            _applicationPathsConfiguration.IqExtractedZipDirectory.DeleteDirectory();
            _applicationPathsConfiguration.IqUpdateZipPath.DeleteFile();

            return PassStage(Stage.Uninstalling);
        }
        catch
        {
            return FailStage(Stage.Uninstalling);
        }
    }

    private async Task<bool> WaitInspectaQueueToExit(CancellationToken cancellationToken = default)
    {
        try
        {
            RaiseStageStatusChanged(Stage.WaitingAppToClose, StageStatus.InProgress);
            await Task.Delay(_consistentDelay, cancellationToken);

            var processes = Process.GetProcessesByName("InspectaQueue");

            if (processes.Length == 0)
            {
                return PassStage(Stage.WaitingAppToClose);
            }

            await processes[0].WaitForExitAsync(cancellationToken);

            return PassStage(Stage.WaitingAppToClose);
        }
        catch
        {
            return FailStage(Stage.WaitingAppToClose);
        }
    }

    private bool LaunchInspectaQueue()
    {
        try
        {
            RaiseStageStatusChanged(Stage.LaunchApp, StageStatus.InProgress);

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = _applicationPathsConfiguration.IqAppExecutablePath,
                WorkingDirectory = _applicationPathsConfiguration.IqAppDirectory,
                Arguments = "",
                UseShellExecute = false,
                CreateNoWindow = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                RedirectStandardInput = false,
                WindowStyle = ProcessWindowStyle.Normal
            };

            Process.Start(psi);

            return PassStage(Stage.LaunchApp);
        }
        catch
        {
            return FailStage(Stage.LaunchApp);
        }
    }

    #endregion

    private async Task UpdateReleaseInfo(CancellationToken cancellationToken = default)
    {
        _releaseInfo = await _downloadService.FetchReleaseInfoAsync(cancellationToken);
    }

    private void RaiseJobStatusChanged(bool isJobRunning, Stage[]? stages = null)
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

    private bool FailJob(params Stage[] failStages)
    {
        foreach (var stage in failStages)
        {
            RaiseStageStatusChanged(stage, StageStatus.Skipped);
        }

        RaiseJobStatusChanged(false);
        return false;
    }
}