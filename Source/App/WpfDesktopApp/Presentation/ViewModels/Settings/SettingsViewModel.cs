using System.Collections.ObjectModel;
using Autofac;
using Rachkov.InspectaQueue.Abstractions;
using Rachkov.InspectaQueue.WpfDesktopApp.Extensions;
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
    private readonly ILifetimeScope _lifetimeScope;
    private IQueueProvider? _selectedProvider;
    private bool _isAddNewSourceWorkflowEnabled;
    private SourceViewModel? _selectedSource;
    public override string Name => "Queue Settings";

    public SettingsViewModel(
        IWindowManager windowManager,
        IEnumerable<IQueueProvider> availableProviders,
        IConfigStoreService configStoreService,
        ISettingsParser settingsParser,
        ISourceReader sourceReader,
        ILifetimeScope lifetimeScope)
    {
        _windowManager = windowManager;
        _configStoreService = configStoreService;
        _settingsParser = settingsParser;
        _lifetimeScope = lifetimeScope;
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
    }

    public RelayCommand ConnectToSourceCommand { get; }
    public RelayCommand CreateNewSourceCommand { get; }
    public RelayCommand AddNewSourceCommand { get; }
    public RelayCommand DuplicateSourceCommand { get; }
    public RelayCommand RemoveSourceCommand { get; }

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
        var vm = new QueueInspectorViewModel(freshProvider);
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
}