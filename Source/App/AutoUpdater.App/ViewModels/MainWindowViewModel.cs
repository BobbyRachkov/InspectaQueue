using AutoUpdater.App.Services;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using ReactiveUI;
using System;
using System.Windows.Input;

namespace AutoUpdater.App.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private bool _isActive;
        private readonly FileService _fileService;

        public MainWindowViewModel()
        {
            CloseCommand = ReactiveCommand.Create(() => Environment.Exit(0));
            UpdateCommand = ReactiveCommand.Create(NextEffect);
            _fileService = new FileService();
        }

        public IImmutableSolidColorBrush Background => new ImmutableSolidColorBrush(new Color(255, 25, 25, 25));
        public IImmutableSolidColorBrush AccentColor => new ImmutableSolidColorBrush(new Color(255, 65, 55, 75));


        public bool IsActive
        {
            get => _isActive;
            set => this.RaiseAndSetIfChanged(ref _isActive, value);
        }

        public string Text => _fileService.IsIqInstalled().ToString();

        public ICommand CloseCommand { get; }
        public ICommand UpdateCommand { get; }

        private void NextEffect()
        {
            IsActive = !IsActive;
        }
    }
}
