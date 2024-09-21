using Autofac;
using Rachkov.InspectaQueue.Abstractions;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure.ErrorManager;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure.WindowManager;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.Config;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.SettingsParser;

namespace Rachkov.InspectaQueue.WpfDesktopApp.Extensions;

public static class ContainerBuilderExtensions
{
    public static ContainerBuilder RegisterQueueProviders(this ContainerBuilder builder)
    {
        foreach (var assembly in LoadProviderModules())
        {
            builder.RegisterAssemblyTypes(assembly)
                .AssignableTo<IQueueProvider>()
                .AsImplementedInterfaces()
                .AsSelf();
        }

        return builder;
    }

    private static IEnumerable<Assembly> LoadProviderModules()
    {
        var files = Directory
            .EnumerateFiles(".\\Providers\\", "*", SearchOption.AllDirectories)
            .Where(f => f.EndsWith(".dll"))
            .Select(Path.GetFullPath);

        foreach (var file in files)
        {
            yield return Assembly.LoadFile(file);
        }
    }

    public static ContainerBuilder RegisterPresenterViewModels(this ContainerBuilder builder)
    {
        builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
            .AssignableTo<IPresenterViewModel>()
            .AsImplementedInterfaces()
            .AsSelf();

        return builder;
    }

    public static ContainerBuilder RegisterWindowManager(this ContainerBuilder builder)
    {
        builder.RegisterType<SimpleMetroWindowManager>()
            .AsImplementedInterfaces()
            .SingleInstance();

        return builder;
    }

    public static ContainerBuilder RegisterConfigStore(this ContainerBuilder builder)
    {
        builder.RegisterType<ConfigStoreService>()
            .AsImplementedInterfaces()
            .SingleInstance();

        return builder;
    }

    public static ContainerBuilder RegisterSettingsParser(this ContainerBuilder builder)
    {
        builder.RegisterType<ReflectionParser>()
            .AsImplementedInterfaces()
            .SingleInstance();

        return builder;
    }

    public static ContainerBuilder RegisterSourceReader(this ContainerBuilder builder)
    {
        builder.RegisterType<SourceReader>()
            .AsImplementedInterfaces()
            .SingleInstance();

        return builder;
    }

    public static ContainerBuilder RegisterErrorManager(this ContainerBuilder builder)
    {
        builder.RegisterType<ErrorManager>()
            .AsImplementedInterfaces()
            .SingleInstance();

        return builder;
    }
}