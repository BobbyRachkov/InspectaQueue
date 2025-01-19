using Rachkov.InspectaQueue.AutoUpdater.Core;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure.DialogManager;
using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings.Models;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.Config;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;

public class MenuViewModel : ViewModel
{
    private readonly IConfigStoreService _configService;
    private readonly IAutoUpdaterService _autoUpdater;
    private readonly IUpdateMigratorService _migratorService;
    private readonly IApplicationPathsConfiguration _applicationPathsConfiguration;
    private bool _isAutoupdaterEnabled;
    private bool _isBetaReleaseChannel;
    private bool _hasCheckedForUpdate;

    public DialogManager? DialogManager { get; private set; }

    public MenuViewModel(IConfigStoreService configService,
        IAutoUpdaterService autoUpdater,
        IUpdateMigratorService migratorService,
        IApplicationPathsConfiguration applicationPathsConfiguration)
    {
        _configService = configService;
        _autoUpdater = autoUpdater;
        _migratorService = migratorService;
        _applicationPathsConfiguration = applicationPathsConfiguration;
        _isAutoupdaterEnabled = configService.GetSettings().IsAutoUpdaterEnabled;
        _isBetaReleaseChannel = configService.GetSettings().IsAutoUpdaterBetaReleaseChannel;

        CheckForUpdatesCommand = new RelayCommand(CheckForUpdatesManually);
        ShowAboutDialogCommand = new RelayCommand(ShowAboutDialog);
        LaunchAutoUpdater = new RelayCommand(LaunchInstaller);
    }

    public bool IsAutoupdaterEnabled
    {
        get => _isAutoupdaterEnabled;
        set
        {
            _isAutoupdaterEnabled = value;
            _configService.UpdateAndStore(s => s.IsAutoUpdaterEnabled = value);
        }
    }

    public bool IsBetaReleaseChannel
    {
        get => _isBetaReleaseChannel;
        set
        {
            _isBetaReleaseChannel = value;
            _configService.UpdateAndStore(s => s.IsAutoUpdaterBetaReleaseChannel = value);
        }
    }

    public ICommand CheckForUpdatesCommand { get; }
    public ICommand LaunchAutoUpdater { get; }
    public ICommand ShowAboutDialogCommand { get; }

    public void SetDialogManager(DialogManager? dialogManager)
    {
        DialogManager = dialogManager;
        CheckForUpdatesAutomatically();
    }

    private void CheckForUpdatesManually()
    {
        TryCheckForUpdates()
            .ContinueWith(updateResult =>
            {
                if (updateResult.Result is UpdateResult.UpToDate)
                {
                    DialogManager?.ShowVersionUpToDateDialog(_autoUpdater.GetExecutingAppVersion().ToString());
                }
            });
    }

    private void CheckForUpdatesAutomatically()
    {
        if (_hasCheckedForUpdate
            || !_configService.GetSettings().IsAutoUpdaterEnabled
            || DialogManager is null)
        {
            return;
        }

        _hasCheckedForUpdate = true;
        _ = TryCheckForUpdates();
    }

    private void ShowAboutDialog()
    {
        DialogManager?.ShowVersionInfoDialog(_autoUpdater.GetExecutingAppVersion().ToString());
    }

    private async Task<UpdateResult> TryCheckForUpdates()
    {
        if (!await _autoUpdater.EnsureInstallerUpToDate())
        {
            return UpdateResult.MissingInstaller;
        }

        var updateChannel = _configService.GetSettings().IsAutoUpdaterBetaReleaseChannel
            ? ReleaseType.Prerelease
            : ReleaseType.Official;

        var releaseInfo = await _autoUpdater.GetReleaseInfo();

        if (releaseInfo is null || DialogManager is null)
        {
            return UpdateResult.UnsuccessfulRequest;
        }

        var latestRelease = updateChannel == ReleaseType.Official
            ? releaseInfo.Latest
            : releaseInfo.Prerelease;

        if (latestRelease is null
            || latestRelease.WindowsAppZip?.Version is null)
        {
            return UpdateResult.UnsuccessfulRequest;
        }

        var currentVersion = _autoUpdater.GetExecutingAppVersion();
        var latestVersion = latestRelease.WindowsAppZip.Version;

        if (!(latestVersion > currentVersion))
        {
            return UpdateResult.UpToDate;
        }

        if (latestRelease.WindowsAppZip.DownloadUrl is null)
        {
            MessageBox.Show("There is new version, but the download url is corrupted. Contact application author.");
            return UpdateResult.SystemError;
        }

        UpdateDialogResult? promptResult = null;

        OnUiThread(() =>
        {
            promptResult = DialogManager.ShowNewUpdateDialog(currentVersion.ToString(), latestVersion.ToString());
        });

        if (promptResult is UpdateDialogResult.NotNow)
        {
            return UpdateResult.Rejected;
        }

        var args = $"{(promptResult == UpdateDialogResult.InstallOnClose ? Constants.StartupArgs.QuietUpdateArg : Constants.StartupArgs.ForceUpdateArg)} " +
                   $"{(updateChannel == ReleaseType.Prerelease ? Constants.StartupArgs.PrereleaseVersionArg : string.Empty)}";

        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = _applicationPathsConfiguration.InstallerPath,
            WorkingDirectory = _applicationPathsConfiguration.IqBaseDirectory,
            Arguments = args,
            UseShellExecute = false,
            CreateNoWindow = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            RedirectStandardInput = false,
            WindowStyle = ProcessWindowStyle.Normal
        };

        Process.Start(psi);

        if (promptResult != UpdateDialogResult.InstallOnClose)
        {
            Environment.Exit(0);
        }

        return UpdateResult.Updated;
    }

    private void LaunchInstaller()
    {
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = _applicationPathsConfiguration.InstallerPath,
            WorkingDirectory = _applicationPathsConfiguration.IqBaseDirectory,
            Arguments = "",
            UseShellExecute = false,
            CreateNoWindow = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            RedirectStandardInput = false,
            WindowStyle = ProcessWindowStyle.Normal
        };

        Process.Start(psi);
    }
}