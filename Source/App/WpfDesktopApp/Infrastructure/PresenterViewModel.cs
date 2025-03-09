using Rachkov.InspectaQueue.Abstractions.Notifications.Errors;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure.ErrorManager;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure;

public abstract class PresenterViewModel : ViewModel, IPresenterViewModel
{
    protected readonly IErrorManager ErrorManager;
    public event EventHandler? Show;

    public event EventHandler? Hide;

    public event EventHandler<CancelEventArgs>? OnClosing;

    private bool _isErrorsFlyoutOpen;

    protected PresenterViewModel(IErrorManager errorManager)
    {
        ErrorManager = errorManager;
        ErrorManager.ErrorRaised += OnErrorRaised;
        ErrorManager.ErrorsCleared += OnErrorsCleared;

        var existingErrors = ErrorManager.GetErrors().Select(ConvertError).ToArray();
        Errors = new(existingErrors);

        ClearErrorsCommand = new RelayCommand(ErrorManager.ClearErrors, Errors.Any);
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
        ErrorManager.ErrorRaised -= OnErrorRaised;
        ErrorManager.ErrorsCleared -= OnErrorsCleared;
    }

    private void OnErrorRaised(object? sender, Error e)
    {
        OnUiThread(() =>
        {
            Errors.Insert(0, ConvertError(e));
            IsErrorsFlyoutOpen = true;
        });
    }

    private static ErrorViewModel ConvertError(Error e)
    {
        string sourceAsString;

        if (e.Source is string str)
        {
            sourceAsString = str;
        }
        else
        {
            sourceAsString = e.Source.GetType().FullName ?? e.Source.GetType().Name;
        }

        return new ErrorViewModel
        {
            Text = e.Text,
            Source = sourceAsString,
            ExceptionHeader = e.Exception?.GetType().Name,
            ExceptionText = e.Exception?.ToString()
        };
    }
}