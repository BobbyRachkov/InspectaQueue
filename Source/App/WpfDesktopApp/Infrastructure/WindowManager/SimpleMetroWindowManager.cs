using System.Windows;
using MahApps.Metro.Controls;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure.WindowManager;

public class SimpleMetroWindowManager : IWindowManager
{
    private readonly IEnumerable<IPresenterViewModel> _presenterViewModels;
    private readonly List<Window> _windows;

    public SimpleMetroWindowManager()
    {
        _windows = new List<Window>();
    }

    public void Create(IPresenterViewModel viewModel)
    {
        var window = new MetroWindow()
        {
            Content = viewModel,
            Title = viewModel.Name
        };

        viewModel.Show += OnShowInvoked;
        viewModel.Hide += OnHideInvoked;
        window.Closing += (_,args)=>viewModel.RaiseClosing(args);
        
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

        var window = _windows.FirstOrDefault(x => x.Content == viewModel);
        return window;
    }
}