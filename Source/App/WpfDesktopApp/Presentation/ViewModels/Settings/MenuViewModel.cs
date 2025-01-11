using Rachkov.InspectaQueue.Abstractions;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure.DialogManager;
using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings.Models;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.Config;
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
        var latestVersion = await _autoUpdater.GetLatestVersion(updateChannel);

        if (latestVersion is null || DialogManager is null)
        {
            return UpdateResult.UnsuccessfulRequest;
        }

        var (newVersion, downloadUrl) = latestVersion.Value;
        var currentVersion = _autoUpdater.GetAppVersion();

        if (!(newVersion > currentVersion))
        {
            return UpdateResult.UpToDate;
        }

        if (downloadUrl is null)
        {
            MessageBox.Show("There is new version, but the download url is corrupted. Contact application author.");
            return UpdateResult.SystemError;
        }

        bool? promptResult = null;

        OnUiThread(() =>
        {
            promptResult = DialogManager.ShowNewUpdateDialog(currentVersion.ToString(), newVersion.ToString());
        });

        if (promptResult is not true)
        {
            return UpdateResult.Rejected;
        }

        _migratorService.MigrateConfig();
        _migratorService.MigrateProviders();

        Task downloadTask = _autoUpdater.DownloadVersion(TODO, downloadUrl).ContinueWith(_ =>
        {
            _autoUpdater.RunFinalCopyScript();
            Environment.Exit(0);
        });

        OnUiThread(async () =>
        {
            await DialogManager.ShowProgressDialog("Update in progress...", "The program will restart soon...", false, true);
        });

        await downloadTask;

        return UpdateResult.Updated;
    }

    private void CheckForUpdatesManually()
    {
        TryCheckForUpdates()
            .ContinueWith(updateResult =>
            {
                if (updateResult.Result is UpdateResult.UpToDate)
                {
                    DialogManager?.ShowVersionUpToDateDialog(_autoUpdater.GetAppVersion().ToString());
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
        DialogManager?.ShowVersionInfoDialog(_autoUpdater.GetAppVersion().ToString());
    }
}