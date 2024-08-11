using Autofac;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure.WindowManager;
using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.QueueInspector;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels;

public class SettingsViewModel : PresenterViewModel
{
    private readonly IWindowManager _windowManager;
    public override string Name => "Queue Settings";

    public SettingsViewModel(
        IWindowManager windowManager)
    {
        _windowManager = windowManager;
        Connect = new(ConnectToQueue);
    }

    public RelayCommand Connect { get; }

    private void ConnectToQueue()
    {
        var vm = new QueueInspectorViewModel();
        _windowManager.Create(vm);
    }
}