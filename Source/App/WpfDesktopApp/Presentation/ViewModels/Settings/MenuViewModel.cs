using Rachkov.InspectaQueue.Abstractions;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure.DialogManager;
using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings.Models;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.Config;
using System.Diagnostics;
using System.Windows;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;

public class MenuViewModel : ViewModel
{
    private readonly IConfigStoreService _configService;
    private readonly IAutoUpdaterService _autoUpdater;
    private readonly IUpdateMigratorService _migratorService;
    private bool _isAutoupdaterEnabled;
    private bool _isBetaReleaseChannel;
    private bool _hasCheckedForUpdate;

    public DialogManager? DialogManager { get; private set; }

    public MenuViewModel(IConfigStoreService configService,
        IAutoUpdaterService autoUpdater,
        IUpdateMigratorService migratorService)
    {
        _configService = configService;
        _autoUpdater = autoUpdater;
        _migratorService = migratorService;
        _isAutoupdaterEnabled = configService.GetSettings().IsAutoUpdaterEnabled;
        _isBetaReleaseChannel = configService.GetSettings().IsAutoUpdaterBetaReleaseChannel;

        CheckForUpdatesCommand = new(CheckForUpdatesManually);
        ShowAboutDialogCommand = new(ShowAboutDialog);
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

    public RelayCommand CheckForUpdatesCommand { get; }
    public RelayCommand ShowAboutDialogCommand { get; }

    public void SetDialogManager(DialogManager? dialogManager)
    {
        DialogManager = dialogManager;
        CheckForUpdatesAutomatically();
    }

    private async Task<UpdateResult> TryCheckForUpdates()
    {
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

        bool? promptResult = null;

        OnUiThread(() =>
        {
            promptResult = DialogManager.ShowNewUpdateDialog(currentVersion.ToString(), latestVersion.ToString());
        });

        if (promptResult is not true)
        {
            return UpdateResult.Rejected;
        }



        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            WorkingDirectory = "..\\",
            Arguments = $"{Constants.StartupArgs.ForceUpdateArg} {(updateChannel == ReleaseType.Prerelease ? Constants.StartupArgs.PrereleaseVersionArg : string.Empty)}",
            UseShellExecute = false,
            CreateNoWindow = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            RedirectStandardInput = false,
            WindowStyle = ProcessWindowStyle.Normal
        };

        Process? cmdProcess = Process.Start(psi);
        Environment.Exit(0);

        //OnUiThread(async () =>
        //{
        //    await DialogManager.ShowProgressDialog("Update in progress...", "The program will restart soon...", false, true);
        //});

        return UpdateResult.Updated;
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
}