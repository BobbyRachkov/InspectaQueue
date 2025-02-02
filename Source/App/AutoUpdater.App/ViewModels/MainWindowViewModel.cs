using AutoUpdater.App.Services;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Rachkov.InspectaQueue.AutoUpdater.Core;
using Rachkov.InspectaQueue.AutoUpdater.Core.Services.Registrar;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AutoUpdater.App.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private bool _isBusy;
        private readonly FileService _fileService;
        private readonly IAutoUpdaterService _autoUpdater;
        private bool _finished = false;

        public MainWindowViewModel()
        {
            CloseCommand = ReactiveCommand.Create(() => Environment.Exit(0));
            UpdateCommand = ReactiveCommand.Create(Update);
            InstallCommand = ReactiveCommand.Create(Install);
            UninstallCommand = ReactiveCommand.Create(Uninstall);
            _fileService = new FileService();

            var paths = new ApplicationPathsConfiguration();
            _autoUpdater = new AutoUpdaterService(
                new DownloadService(new HttpClientFactory()),
                paths,
                new WindowsRegistrar(paths));

            _autoUpdater.JobStatusChanged += OnJobStatusChanged;
            _autoUpdater.StageStatusChanged += OnStageStatusChanged;

            _ = RunStartupProcedures();
        }

        private void OnStageStatusChanged(object? sender, Rachkov.InspectaQueue.AutoUpdater.Core.EventArgs.StageStatusChangedEventArgs e)
        {
            var stage = Stages.FirstOrDefault(x => x.Stage == e.Stage);

            if (stage is not null)
            {
                stage.Status = e.Status;
            }
        }

        private void OnJobStatusChanged(object? sender, Rachkov.InspectaQueue.AutoUpdater.Core.EventArgs.JobStatusChangedEventArgs e)
        {
            if (!e.IsJobRunning)
            {
                _finished = true;
            }

            IsBusy = e.IsJobRunning;

            if (e.Stages is null)
            {
                return;
            }

            Stages.Clear();
            foreach (var stage in e.Stages)
            {
                Stages.Add(new StageViewModel
                {
                    Stage = stage,
                    Status = StageStatus.Pending
                });
            }
        }

        public IImmutableSolidColorBrush Background => new ImmutableSolidColorBrush(new Color(255, 25, 25, 25));

        public IImmutableSolidColorBrush AccentColor => new ImmutableSolidColorBrush(new Color(255, 65, 55, 75));


        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                this.RaiseAndSetIfChanged(ref _isBusy, value);
                RaiseButtonsVisibilityUpdated();
            }
        }

        public bool IsForceUpdate => StartupArgsService.Instance?.IsForceUpdate ?? false;

        public bool IsInstallButtonVisible => !IsBusy && !_fileService.IsIqInstalled() && !IsForceUpdate && !_finished;

        public bool IsUpdateButtonVisible => !IsBusy && _fileService.IsIqInstalled() && !IsForceUpdate && !_finished;

        public bool IsUninstallButtonVisible => !IsBusy && _fileService.IsIqInstalled() && !IsForceUpdate && !_finished;

        public bool IsCancelButtonVisible => IsBusy;

        public bool IsCloseButtonVisible => !IsBusy;

        public ObservableCollection<StageViewModel> Stages { get; } = new();

        public ICommand CloseCommand { get; }

        public ICommand UpdateCommand { get; }

        public ICommand InstallCommand { get; }

        public ICommand UninstallCommand { get; }

        private async Task Update()
        {
            await _autoUpdater.Update();
        }

        private async Task Install()
        {
            await _autoUpdater.FreshInstall();
        }

        private async Task Uninstall()
        {
            await _autoUpdater.Uninstall();
        }

        private void RaiseButtonsVisibilityUpdated()
        {
            this.RaisePropertyChanged(nameof(IsInstallButtonVisible));
            this.RaisePropertyChanged(nameof(IsUpdateButtonVisible));
            this.RaisePropertyChanged(nameof(IsUninstallButtonVisible));
            this.RaisePropertyChanged(nameof(IsCancelButtonVisible));
            this.RaisePropertyChanged(nameof(IsCloseButtonVisible));
        }

        private async Task RunStartupProcedures()
        {
            var prerelease = StartupArgsService.Instance?.IsPrerelease ?? false;

            if (StartupArgsService.Instance?.IsForceUpdate == true)
            {
                await _autoUpdater.Update(prerelease);
            }

            if (StartupArgsService.Instance?.IsQuietUpdate == true)
            {
                await _autoUpdater.SilentUpdate(prerelease);
            }
        }
    }
}
