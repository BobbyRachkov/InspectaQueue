using Rachkov.InspectaQueue.Abstractions;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure.ErrorManager;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure;

public abstract class PresenterViewModel : ViewModel, IPresenterViewModel
{
    private readonly IErrorManager _errorManager;
    public event EventHandler? Show;

    public event EventHandler? Hide;

    public event EventHandler<CancelEventArgs>? OnClosing;

    private bool _isErrorsFlyoutOpen;

    protected PresenterViewModel(IErrorManager errorManager)
    {
        _errorManager = errorManager;
        _errorManager.ErrorRaised += OnErrorRaised;
        _errorManager.ErrorsCleared += OnErrorsCleared;

        var existingErrors = _errorManager.GetErrors().Select(ConvertError).ToArray();
        Errors = new(existingErrors);

        ClearErrorsCommand = new RelayCommand(_errorManager.ClearErrors, Errors.Any);
        OpenErrorsFlyoutCommand = new RelayCommand(() => IsErrorsFlyoutOpen = true);
    }

    private void OnErrorsCleared(object? sender, EventArgs e)
    {
        Errors.Clear();
        IsErrorsFlyoutOpen = false;
    }

    public abstract string Name { get; }

    public bool IsVisible { get; set; } = true;

    public RelayCommand ClearErrorsCommand { get; }

    public RelayCommand OpenErrorsFlyoutCommand { get; }

    public ObservableCollection<ErrorViewModel> Errors { get; }

    public bool IsErrorsFlyoutOpen
    {
        get => _isErrorsFlyoutOpen;
        set
        {
            _isErrorsFlyoutOpen = value;
            OnPropertyChanged();
        }
    }

    public void SetVisibility(bool isVisible)
    {
        IsVisible = isVisible;

        if (isVisible)
        {
            Show?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Hide?.Invoke(this, EventArgs.Empty);
        }
    }

    public void RaiseClosing(CancelEventArgs args)
    {
        OnClosing?.Invoke(this, args);
        _errorManager.ErrorRaised -= OnErrorRaised;
        _errorManager.ErrorsCleared -= OnErrorsCleared;
    }

    private void OnErrorRaised(object? sender, Abstractions.Error e)
    {
        OnUiThread(() =>
        {
            Errors.Insert(0, ConvertError(e));
            IsErrorsFlyoutOpen = true;
        });
    }

    private static ErrorViewModel ConvertError(Error e)
    {
        return new ErrorViewModel
        {
            Text = e.Text,
            Source = e.Source.GetType().FullName ?? e.Source.GetType().Name,
            ExceptionHeader = e.Exception?.GetType().Name,
            ExceptionText = e.Exception?.ToString()
        };
    }
}