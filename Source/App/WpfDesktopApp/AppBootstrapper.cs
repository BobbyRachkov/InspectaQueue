using Autofac;
using Rachkov.InspectaQueue.WpfDesktopApp.Extensions;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure.WindowManager;
using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;
using System.Windows;

namespace Rachkov.InspectaQueue.WpfDesktopApp;
public class AppBootstrapper
{
    private static IContainer _container = null!;

    public static void OnStartup(StartupEventArgs _)
    {
        var builder = new ContainerBuilder();
        builder
            .RegisterQueueProviders()
            .RegisterPresenterViewModels()
            .RegisterWindowManager()
            .RegisterConfigStore()
            .RegisterSettingsParser()
            .RegisterSourceReader()
            .RegisterErrorManager()
            .RegisterHttpClientFactory()
            .RegisterAutoUpdater();

        _container = builder.Build();

        _container
            .InitializeWindowManager();

        var settingsViewModel = _container.Resolve<SettingsViewModel>();
        var windowManager = _container.Resolve<IWindowManager>();
        windowManager.Create(settingsViewModel);
    }


    public static void OnShutdown(ExitEventArgs e)
    {
        _container.DisposeAsync();
    }
}