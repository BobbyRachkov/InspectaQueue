using AutoUpdater.App.Services;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Rachkov.InspectaQueue.AutoUpdater.Core;
using ReactiveUI;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AutoUpdater.App.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private bool _isBusy;
        private readonly FileService _fileService;
        private readonly IAutoUpdaterService _autoUpdater;

        public MainWindowViewModel()
        {
            CloseCommand = ReactiveCommand.Create(() => Environment.Exit(0));
            UpdateCommand = ReactiveCommand.Create(NextEffect);
            _fileService = new FileService();

            _autoUpdater = new AutoUpdaterService(
                new DownloadService(new HttpClientFactory()),
                new ApplicationPathsConfiguration());

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
        public bool IsInstallButtonVisible => !IsBusy && !_fileService.IsIqInstalled() && !IsForceUpdate;
        public bool IsUpdateButtonVisible => !IsBusy && _fileService.IsIqInstalled() && !IsForceUpdate;
        public bool IsUninstallButtonVisible => !IsBusy && _fileService.IsIqInstalled() && !IsForceUpdate;
        public bool IsCancelButtonVisible => IsBusy;
        public bool IsCloseButtonVisible => !IsBusy;


        public string Text => StartupArgsService.Instance?.IsForceUpdate.ToString() ?? "";

        public ICommand CloseCommand { get; }

        public ICommand UpdateCommand { get; }

        private async Task NextEffect()
        {
            IsBusy = !IsBusy;
        }

        private void RaiseButtonsVisibilityUpdated()
        {
            this.RaisePropertyChanged(nameof(IsInstallButtonVisible));
            this.RaisePropertyChanged(nameof(IsUpdateButtonVisible));
            this.RaisePropertyChanged(nameof(IsUninstallButtonVisible));
            this.RaisePropertyChanged(nameof(IsCancelButtonVisible));
            this.RaisePropertyChanged(nameof(IsCloseButtonVisible));
        }
    }
}
