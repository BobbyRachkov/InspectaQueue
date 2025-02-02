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

        var oldInstallerName = _applicationPathsConfiguration.InstallerPath;
        var temporaryLocation = _applicationPathsConfiguration.IqBaseDirectory / $"{Guid.NewGuid()}.exe";
        var finalLocation = _applicationPathsConfiguration.GetInstallerPath(releaseInfo.Latest.Installer.Version);
        var result = await DownloadInstaller(temporaryLocation, cancellationToken);

        if (!result)
        {
            RaiseStageStatusChanged(Stage.DownloadingInstaller, StageStatus.Failed);
            RaiseJobStatusChanged(false);
            return false;
        }

        oldInstallerName.DeleteFile();
        temporaryLocation.Move(finalLocation);

        await _registrar.CreateOrUpdateInstallerProxy(cancellationToken);

        RaiseStageStatusChanged(Stage.DownloadingInstaller, StageStatus.Done);
        RaiseJobStatusChanged(false);
        return true;
    }

    public async Task<bool> FreshInstall(CancellationToken cancellationToken = default)
    {
        var stages = new[]
        {
            Stage.DownloadingRelease,
            Stage.Unzipping,
            Stage.CopyingFiles,
            Stage.CleaningUp,
            Stage.FinalizingSetup,
            Stage.LaunchApp
        };

        RaiseJobStatusChanged(true, stages);

        if (!await DownloadRelease(cancellationToken: cancellationToken))
        {
            return FailJob(stages, Stage.Unzipping);
        }

        if (!await Unzip(cancellationToken))
        {
            return FailJob(stages, Stage.CopyingFiles);
        }

        if (!await CopyFiles(cancellationToken))
        {
            return FailJob(stages, Stage.CleaningUp);
        }

        if (!await CleanUp(cancellationToken))
        {
            return FailJob(stages, Stage.FinalizingSetup);
        }

        if (!await FinalizeSetup(true, _releaseInfo?.Latest.WindowsAppZip?.Version, cancellationToken))
        {
            return FailJob(stages, Stage.LaunchApp);
        }

        if (!LaunchInspectaQueue())
        {
            return FailJob();
        }

        RaiseJobStatusChanged(false);
        return true;
    }

    public async Task<bool> Update(bool prerelease = false, CancellationToken cancellationToken = default)
    {
        return await UpdateInternal(prerelease, silent: false, cancellationToken);
    }

    public async Task<bool> SilentUpdate(bool prerelease = false, CancellationToken cancellationToken = default)
    {
        return await UpdateInternal(prerelease, silent: true, cancellationToken);
    }

    public async Task<bool> Uninstall(bool removeConfig = false, CancellationToken cancellationToken = default)
    {
        var stages = new[] { Stage.Uninstalling };
        RaiseJobStatusChanged(true, stages);

        await Task.Yield();

        if (!await UninstallInternal(cancellationToken: cancellationToken))
        {
            return FailJob();
        }

        RaiseJobStatusChanged(false);
        return true;
    }

    private async Task<bool> UpdateInternal(bool prerelease, bool silent, CancellationToken cancellationToken = default)
    {
        var stages = new[]
        {
            Stage.DownloadingRelease,
            Stage.WaitingAppToClose,
            Stage.Unzipping,
            Stage.CopyingFiles,
            Stage.CleaningUp,
            Stage.FinalizingSetup,
        };

        if (!silent)
        {
            stages = stages.Append(Stage.LaunchApp).ToArray();
        }

        RaiseJobStatusChanged(true, stages);

        if (!await DownloadRelease(prerelease, cancellationToken))
        {
            return FailJob(stages, Stage.WaitingAppToClose);
        }

        if (!await WaitInspectaQueueToExit(cancellationToken))
        {
            return FailJob(stages, Stage.Unzipping);
        }

        if (!await Unzip(cancellationToken))
        {
            return FailJob(stages, Stage.CopyingFiles);
        }

        if (!await CopyFiles(cancellationToken))
        {
            return FailJob(stages, Stage.CleaningUp);
        }

        if (!await CleanUp(cancellationToken))
        {
            return FailJob(stages, Stage.FinalizingSetup);
        }

        var version = prerelease ? _releaseInfo?.Prerelease?.WindowsAppZip?.Version : _releaseInfo?.Latest.WindowsAppZip?.Version;
        if (!await FinalizeSetup(false, version, cancellationToken))
        {
            return FailJob(stages, silent ? Stage.FinalizingSetup : Stage.LaunchApp);
        }

        if (!silent && !LaunchInspectaQueue())
        {
            return FailJob();
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

    private async Task<bool> DownloadInstaller(AbsolutePath downloadPath, CancellationToken cancellationToken = default)
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
                downloadPath,
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
            _applicationPathsConfiguration.InstallerProxy.DeleteFile();

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

    private async Task<bool> FinalizeSetup(bool createDesktopShortcut, Version? appVersion = null, CancellationToken cancellationToken = default)
    {
        RaiseStageStatusChanged(Stage.FinalizingSetup, StageStatus.InProgress);

        try
        {
            if (createDesktopShortcut && !await _registrar.CreateDesktopShortcut(cancellationToken))
            {
                return FailStage(Stage.FinalizingSetup);
            }

            if (!await _registrar.CreateOsSearchIndex(cancellationToken)
                || !await _registrar.RegisterAppInProgramUninstallList(appVersion, cancellationToken)
                || !await _registrar.CreateOrUpdateInstallerProxy(cancellationToken))
            {
                return FailStage(Stage.FinalizingSetup);
            }

            return PassStage(Stage.FinalizingSetup);
        }
        catch
        {
            return FailStage(Stage.FinalizingSetup);
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

    private bool FailJob(Stage[] stages, Stage skipFromStage)
    {

        foreach (var stage in stages.SkipWhile(x => x != skipFromStage))
        {
            RaiseStageStatusChanged(stage, StageStatus.Skipped);
        }

        RaiseJobStatusChanged(false);
        return false;
    }

    private bool FailJob()
    {
        RaiseJobStatusChanged(false);
        return false;
    }
}