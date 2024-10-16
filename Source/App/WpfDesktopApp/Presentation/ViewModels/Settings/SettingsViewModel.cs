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

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;

public class SettingsViewModel : PresenterViewModel, ICanManageDialogs
{
    private readonly IWindowManager _windowManager;
    private readonly IConfigStoreService _configStoreService;
    private readonly ISettingsParser _settingsParser;
    private readonly IErrorManager _errorManager;
    private readonly IAutoUpdaterService _autoUpdater;
    private readonly IUpdateMigratorService _migratorService;
    private ProviderViewModel? _selectedProvider;
    private bool _isAddNewSourceWorkflowEnabled;
    private SourceViewModel? _selectedSource;
    private DialogManager? _dialogManager;
    private bool _hasChackedForUpdate = false;
    public override string Name => "Queue Settings";

    public SettingsViewModel(
        IWindowManager windowManager,
        IConfigStoreService configStoreService,
        ISettingsParser settingsParser,
        ISourceReader sourceReader,
        IErrorManager errorManager,
        IAutoUpdaterService autoUpdater,
        IUpdateMigratorService migratorService)
    : base(errorManager)
    {
        _windowManager = windowManager;
        _configStoreService = configStoreService;
        _settingsParser = settingsParser;
        _errorManager = errorManager;
        _autoUpdater = autoUpdater;
        _migratorService = migratorService;

        if (AvailableProviders.Any())
        {
            SelectedProvider = AvailableProviders.First();
        }

        Sources = sourceReader.ReadSources(availableProvidersCollection).ToObservableCollection();

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

        MenuViewModel = new MenuViewModel(configStoreService, autoUpdater, migratorService);
    }


    public RelayCommand ConnectToSourceCommand { get; }
    public RelayCommand CreateNewSourceCommand { get; }
    public RelayCommand AddNewSourceCommand { get; }
    public RelayCommand DuplicateSourceCommand { get; }
    public RelayCommand RemoveSourceCommand { get; }

    public MenuViewModel MenuViewModel { get; }

    public DialogManager? DialogManager
    {
        get => _dialogManager;
        set
        {
            _dialogManager = value;
            MenuViewModel.SetDialogManager(value);
        }
    }

    public ObservableCollection<SourceViewModel> Sources { get; private set; }

    public List<ProviderViewModel> AvailableProviders { get; }

    public ProviderViewModel? SelectedProvider
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
        var freshProvider = ;
        SelectedSource.UpdateSettings(freshProvider.Settings);
        var vm = new QueueInspectorViewModel(freshProvider, _errorManager, _windowManager);
        _windowManager.Create(vm);
    }

    private void CreateSource()
    {
        if (SelectedProvider?.SelectedInstance is null)
        {
            return;
        }

        var settings = _settingsParser.ParseMembers(SelectedProvider.SelectedInstance);
        var source = new SourceViewModel(Guid.NewGuid(), SelectedProvider.SelectedInstance.Name, SelectedProvider.SelectedInstance, settings.ToArray());
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

        var source = new SourceViewModel(Guid.NewGuid(), SelectedSource.Name, provider.SelectedInstance, SelectedSource.Settings.Copy());
        Sources.Add(source);
        SelectedSource = source;
        _configStoreService.StoreSources(Sources.ToArray());
    }
}