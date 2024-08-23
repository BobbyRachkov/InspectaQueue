using System.Collections.ObjectModel;
using Autofac;
using Rachkov.InspectaQueue.Abstractions;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure.WindowManager;
using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.QueueInspector;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.Config;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.SettingsParser;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;

public class SettingsViewModel : PresenterViewModel
{
    private readonly IWindowManager _windowManager;
    private readonly IConfigStoreService _configStoreService;
    private readonly ISettingsParser _settingsParser;
    private IQueueProvider? _selectedProvider;
    private bool _isAddNewSourceWorkflowEnabled;
    private SourceViewModel? _selectedSource;
    public override string Name => "Queue Settings";

    public SettingsViewModel(
        IWindowManager windowManager,
        IEnumerable<IQueueProvider> availableProviders,
        IConfigStoreService configStoreService,
        ISettingsParser settingsParser)
    {
        _windowManager = windowManager;
        _configStoreService = configStoreService;
        _settingsParser = settingsParser;
        AvailableProviders = availableProviders.ToArray();

        if (AvailableProviders.Any())
        {
            SelectedProvider = AvailableProviders.First();
        }

        Sources = new();
        Connect = new(ConnectToQueue);
        Create = new(CreateSource, () => SelectedProvider is not null);
        AddNewSource = new(() => IsAddNewSourceWorkflowEnabled = true);
    }

    public RelayCommand Connect { get; }
    public RelayCommand Create { get; }
    public RelayCommand AddNewSource { get; }

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
        _configStoreService.StoreSources(Sources.ToArray());
        //var vm = new QueueInspectorViewModel();
        //_windowManager.Create(vm);
    }

    private void CreateSource()
    {
        var source = new SourceViewModel(SelectedProvider!.Name, SelectedProvider, _settingsParser);
        Sources.Add(source);
        SelectedSource = source;
        _configStoreService.StoreSources(Sources.ToArray());
    }
}