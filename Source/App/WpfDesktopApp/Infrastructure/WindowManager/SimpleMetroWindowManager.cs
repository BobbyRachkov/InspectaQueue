using System.Windows;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure.WindowManager;

public class SimpleMetroWindowManager : IWindowManager
{
    private readonly List<Window> _windows = new();

    public void Create(IPresenterViewModel viewModel)
    {
        var window = new MainWindow()
        {
            Content = viewModel,
            DataContext = viewModel
        };

        viewModel.Show += OnShowInvoked;
        viewModel.Hide += OnHideInvoked;
        window.Closing += (_, args) => viewModel.RaiseClosing(args);

        _windows.Add(window);

        if (viewModel.IsVisible)
        {
            viewModel.SetVisibility(true);
        }
    }

    public void Show(IPresenterViewModel viewModel)
    {
        viewModel.SetVisibility(true);
    }

    public void Hide(IPresenterViewModel viewModel)
    {
        viewModel.SetVisibility(false);
    }

    public void Close(IPresenterViewModel viewModel)
    {
        var window = GetWindow(viewModel);

        if (window is null)
        {
            return;
        }

        _windows.Remove(window);
        window.Close();
    }

    private void OnShowInvoked(object? sender, EventArgs e)
    {
        var window = GetWindow(sender);
        window?.Show();
    }

    private void OnHideInvoked(object? sender, EventArgs e)
    {
        var window = GetWindow(sender);
        window?.Hide();
    }

    private Window? GetWindow(object? sender)
    {
        var viewModel = sender as IPresenterViewModel;

        if (viewModel is null)
        {
            return null;
        }

        var window = _windows.FirstOrDefault(x => x.DataContext == viewModel);
        return window;
    }
}