using Autofac;
using Rachkov.InspectaQueue.Abstractions;
using Rachkov.InspectaQueue.WpfDesktopApp.Extensions;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure.DialogManager;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure.ErrorManager;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure.WindowManager;
using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.QueueInspector;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.Config;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.SettingsParser;
using System.Collections.ObjectModel;
using System.Windows;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;

public class SettingsViewModel : PresenterViewModel, ICanManageDialogs
{
    private readonly IWindowManager _windowManager;
    private readonly IConfigStoreService _configStoreService;
    private readonly ISettingsParser _settingsParser;
    private readonly ILifetimeScope _lifetimeScope;
    private readonly IErrorManager _errorManager;
    private readonly IAutoUpdaterService _autoUpdater;
    private readonly IUpdateMigratorService _migratorService;
    private IQueueProvider? _selectedProvider;
    private bool _isAddNewSourceWorkflowEnabled;
    private SourceViewModel? _selectedSource;
    private DialogManager? _dialogManager;
    private bool _hasChackedForUpdate = false;
    public override string Name => "Queue Settings";

    public SettingsViewModel(
        IWindowManager windowManager,
        IEnumerable<IQueueProvider> availableProviders,
        IConfigStoreService configStoreService,
        ISettingsParser settingsParser,
        ISourceReader sourceReader,
        ILifetimeScope lifetimeScope,
        IErrorManager errorManager,
        IAutoUpdaterService autoUpdater,
        IUpdateMigratorService migratorService)
    : base(errorManager)
    {
        _windowManager = windowManager;
        _configStoreService = configStoreService;
        _settingsParser = settingsParser;
        _lifetimeScope = lifetimeScope;
        _errorManager = errorManager;
        _autoUpdater = autoUpdater;
        _migratorService = migratorService;
        AvailableProviders = availableProviders.ToArray();

        if (AvailableProviders.Any())
        {
            SelectedProvider = AvailableProviders.First();
        }

        Sources = sourceReader.ReadSources(AvailableProviders).ToObservableCollection();

        if (Sources.Any())
        {
            SelectedSource = Sources.First();
        }

        ConnectToSourceCommand = new(ConnectToQueue);
        CreateNewSourceCommand = new(CreateSource, () => SelectedProvider is not null);
        AddNewSourceCommand = new(() =>
        {
            SelectedSource = null;
            IsAddNewSourceWorkflowEnabled = true;
        });
        DuplicateSourceCommand = new(DuplicateSource, () => SelectedSource is not null);
        RemoveSourceCommand = new(DeleteSource, () => SelectedSource is not null);
        CheckForUpdatesCommand = new(CheckForUpdatesManually);


    }

    public RelayCommand ConnectToSourceCommand { get; }
    public RelayCommand CreateNewSourceCommand { get; }
    public RelayCommand AddNewSourceCommand { get; }
    public RelayCommand DuplicateSourceCommand { get; }
    public RelayCommand RemoveSourceCommand { get; }
    public RelayCommand CheckForUpdatesCommand { get; }

    public DialogManager? DialogManager
    {
        get => _dialogManager;
        set
        {
            _dialogManager = value;
            if (!_hasChackedForUpdate
                && _configStoreService.GetSettings().IsAutoUpdaterEnabled)
            {
                _hasChackedForUpdate = true;
                CheckForUpdates();
            }
        }
    }

    public ObservableCollection<SourceViewModel> Sources { get; private set; }

    public IQueueProvider[] AvailableProviders { get; }

    public IQueueProvider? SelectedProvider
    {
        get => _selectedProvider;
        set
        {
            _selectedProvider = value;
            OnPropertyChanged();
        }
    }

    public SourceViewModel? SelectedSource
    {
        get => _selectedSource;
        set
        {
            _selectedSource = value;
            IsAddNewSourceWorkflowEnabled = false;
            OnPropertyChanged();
        }
    }

    public bool IsAddNewSourceWorkflowEnabled
    {
        get => _isAddNewSourceWorkflowEnabled;
        set
        {
            _isAddNewSourceWorkflowEnabled = value;
            OnPropertyChanged();
        }
    }

    private void ConnectToQueue()
    {
        if (SelectedSource is null)
        {
            return;
        }

        _configStoreService.StoreSources(Sources.ToArray());
        var freshProvider = (IQueueProvider)_lifetimeScope.Resolve(SelectedSource.ProviderType);
        SelectedSource.UpdateSettings(freshProvider.Settings);
        var vm = new QueueInspectorViewModel(freshProvider, _errorManager, _windowManager);
        _windowManager.Create(vm);
    }

    private void CreateSource()
    {
        if (SelectedProvider is null)
        {
            return;
        }

        var settings = _settingsParser.ParseMembers(SelectedProvider);
        var source = new SourceViewModel(SelectedProvider.Name, SelectedProvider, settings.ToArray());
        Sources.Add(source);
        SelectedSource = source;
        _configStoreService.StoreSources(Sources.ToArray());
    }

    private void DeleteSource()
    {
        if (SelectedSource is null)
        {
            return;
        }

        Sources.Remove(SelectedSource);
        SelectedSource = Sources.FirstOrDefault();
        _configStoreService.StoreSources(Sources.ToArray());
    }

    private void DuplicateSource()
    {
        if (SelectedSource is null)
        {
            return;
        }

        var provider = AvailableProviders.FirstOrDefault(x => x.GetType().Name == SelectedSource.ProviderType.Name);

        if (provider is null)
        {
            return;
        }

        var source = new SourceViewModel(SelectedSource.Name, provider, SelectedSource.Settings.Copy());
        Sources.Add(source);
        SelectedSource = source;
        _configStoreService.StoreSources(Sources.ToArray());
    }

    private void CheckForUpdates()
    {
        var updateChannel = _configStoreService.GetSettings().IsAutoUpdaterBetaReleaseChannel
            ? ReleaseType.Prerelease
            : ReleaseType.Official;
        _autoUpdater.GetLatestVersion(updateChannel).ContinueWith(t =>
        {
            if (t.Result is null || DialogManager is null)
            {
                return;
            }

            var (newVersion, downloadUrl) = t.Result.Value;
            var currentVersion = _autoUpdater.GetAppVersion();

            if (!(newVersion > currentVersion))
            {
                return;
            }

            if (downloadUrl is null)
            {
                MessageBox.Show("There is new version, but the download url is corrupted. Contact application author.");
                return;
            }

            bool? promptResult = null;

            OnUiThread(() =>
            {
                promptResult = DialogManager.ShowNewUpdateDialog(currentVersion.ToString(), newVersion.ToString());
            });

            if (promptResult is not true)
            {
                return;
            }

            _migratorService.MigrateConfig();
            _migratorService.MigrateProviders();
            _autoUpdater.DownloadVersion(downloadUrl).ContinueWith(t2 =>
            {
                _autoUpdater.RunFinalCopyScript();
                Environment.Exit(0);
            });

            OnUiThread(async () =>
            {
                await DialogManager.ShowProgressDialog("Update in progress...", "The program will restart soon...", false, true);
            });
        });
    }

    private void CheckForUpdatesManually()
    {

    }
}