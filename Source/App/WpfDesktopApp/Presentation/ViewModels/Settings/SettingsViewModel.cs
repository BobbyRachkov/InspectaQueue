﻿using Rachkov.InspectaQueue.Abstractions;
using Rachkov.InspectaQueue.WpfDesktopApp.Extensions;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure.DialogManager;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure.ErrorManager;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure.WindowManager;
using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.QueueInspector;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.Config;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.ProviderManager;
using System.Collections.ObjectModel;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;

public class SettingsViewModel : PresenterViewModel, ICanManageDialogs
{
    private readonly IWindowManager _windowManager;
    private readonly IConfigStoreService _configStoreService;
    private readonly IProviderManager _providerManager;
    private readonly ISettingsManager _settingsManager;
    private readonly IErrorManager _errorManager;
    private readonly IAutoUpdaterService _autoUpdater;
    private readonly IUpdateMigratorService _migratorService;
    private ProviderViewModel? _selectedProvider;
    private ProviderVersionViewModel? _selectedVersion;
    private bool _isAddNewSourceWorkflowEnabled;
    private SourceViewModel? _selectedSource;
    private DialogManager? _dialogManager;
    private bool _hasChackedForUpdate = false;
    public override string Name => "Queue Settings";

    public SettingsViewModel(
        IWindowManager windowManager,
        IConfigStoreService configStoreService,
        IProviderManager providerManager,
        ISettingsManager settingsManager,
        ISourceReader sourceReader,
        IErrorManager errorManager,
        IAutoUpdaterService autoUpdater,
        IUpdateMigratorService migratorService)
    : base(errorManager)
    {
        _windowManager = windowManager;
        _configStoreService = configStoreService;
        _providerManager = providerManager;
        _errorManager = errorManager;
        _autoUpdater = autoUpdater;
        _migratorService = migratorService;
        _settingsManager = settingsManager;

        AvailableProviders = providerManager.GetProviders().Select(x => new ProviderViewModel(x)).ToArray();

        if (AvailableProviders.Any())
        {
            SelectedProvider = AvailableProviders.First();
        }

        Sources = sourceReader.ReadSources(StoreSources).ToObservableCollection();

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

    public ProviderViewModel[] AvailableProviders { get; }

    public ProviderViewModel? SelectedProvider
    {
        get => _selectedProvider;
        set
        {
            _selectedProvider = value;
            OnPropertyChanged();

            if (value is not null)
            {
                AvailableVersions =
                    value.AssociatedProvider
                        .Versions
                        .Select(x => new ProviderVersionViewModel(x))
                        .OrderByDescending(x => x.Name)
                        .ToArray();
                SelectedVersion = AvailableVersions.First();
            }
        }
    }

    public ProviderVersionViewModel[] AvailableVersions { get; set; }

    public ProviderVersionViewModel? SelectedVersion
    {
        get => _selectedVersion;
        set
        {
            _selectedVersion = value;
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

        var freshProvider =
            _providerManager.GetNewInstance(SelectedSource.ProviderType, SelectedSource.Settings.Select(x => x.SettingsInstance));

        var vm = new QueueInspectorViewModel(SelectedSource.Name, freshProvider, _errorManager, _windowManager);
        _windowManager.Create(vm);
    }

    private void CreateSource()
    {
        if (SelectedProvider is null
            || SelectedVersion is null)
        {
            return;
        }

        var settings = _settingsManager.ExtractSettings(SelectedVersion.Instance);
        var source = new SourceViewModel(
            Guid.NewGuid(),
            SelectedVersion.Instance.Name,
            _settingsManager,
            SelectedVersion.Instance,
            SelectedProvider.AssociatedProvider.Versions,
            settings.Select(x => new SettingEntryViewModel(x)).ToArray(),
            StoreSources);

        Sources.Add(source);
        SelectedSource = source;
        StoreSources();
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

        var provider = _providerManager.GetProviderByInstance(SelectedSource.ProviderInstance);

        var source = new SourceViewModel(
            Guid.NewGuid(),
            SelectedSource.Name,
            _settingsManager,
            SelectedSource.ProviderInstance,
            provider.Versions,
            SettingsEntryViewModelExtensions.Clone(SelectedSource.Settings),
            StoreSources);

        Sources.Add(source);
        SelectedSource = source;
        StoreSources();
    }

    private void StoreSources()
    {
        _configStoreService.StoreSources(Sources.ToArray());
    }
}