using Autofac;
using Nuke.Common.IO;
using Rachkov.InspectaQueue.WpfDesktopApp.Extensions;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure.WindowManager;
using Rachkov.InspectaQueue.WpfDesktopApp.Presentation.ViewModels.Settings;
using System.Reflection;
using System.Windows;

namespace Rachkov.InspectaQueue.WpfDesktopApp;
public class AppBootstrapper
{
    private static IContainer _container = null!;

    public static void OnStartup(StartupEventArgs _)
    {
        MoveConfigAndProviders();

        var builder = new ContainerBuilder();
        builder
            .RegisterQueueProviders()
            .RegisterPresenterViewModels()
            .RegisterWindowManager()
            .RegisterConfigStore()
            .RegisterManagers()
            .RegisterSourceReader()
            .RegisterErrorManager()
            .RegisterHttpClientFactory()
            .RegisterAutoUpdater()
            .RegisterMapper()
            .RegisterImportExportService()
            .RegisterJsonService();

        _container = builder.Build();

        _container
            .InitializeWindowManager();

        var settingsViewModel = _container.Resolve<SettingsViewModel>();
        var windowManager = _container.Resolve<IWindowManager>();
        windowManager.Create(settingsViewModel);
    }

    private static void MoveConfigAndProviders()
    {
        var executingPath = ((AbsolutePath)Assembly.GetExecutingAssembly().Location).Parent;
        var oldConfigPath = executingPath / "config.json";
        var newConfigPath = executingPath / "..\\config.json";

        if (oldConfigPath.FileExists())
        {
            oldConfigPath.Move(newConfigPath, ExistsPolicy.FileOverwrite);
        }

        var oldProvidersPath = executingPath / "Providers";
        var newProvidersPath = executingPath / "..";

        if (oldProvidersPath.DirectoryExists())
        {
            oldProvidersPath.MoveToDirectory(newProvidersPath, ExistsPolicy.MergeAndOverwrite);
        }
    }


    public static void OnShutdown(ExitEventArgs e)
    {
        _container.DisposeAsync();
    }
}