using Autofac;
using AutoMapper.Contrib.Autofac.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Rachkov.InspectaQueue.Abstractions;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure.ErrorManager;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure.MapperProfiles;
using Rachkov.InspectaQueue.WpfDesktopApp.Infrastructure.WindowManager;
using Rachkov.InspectaQueue.WpfDesktopApp.Services.Config;
using System.IO;
using System.Net.Http;
using System.Reflection;

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
        var providersFolder = ".\\Providers\\";
        Directory.CreateDirectory(providersFolder);

        var files = Directory
            .EnumerateFiles(providersFolder, "*", SearchOption.AllDirectories)
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
        builder.RegisterType<JsonFileConfigStoreService>()
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

    public static ContainerBuilder RegisterHttpClientFactory(this ContainerBuilder builder)
    {
        builder.Register<IHttpClientFactory>(_ =>
        {
            var services = new ServiceCollection();
            services.AddHttpClient();
            var provider = services.BuildServiceProvider();
            return provider.GetRequiredService<IHttpClientFactory>();
        });

        return builder;
    }

    public static ContainerBuilder RegisterAutoUpdater(this ContainerBuilder builder)
    {
        builder.RegisterType<AutoUpdaterService>()
            .AsImplementedInterfaces()
            .SingleInstance();

        builder.RegisterType<UpdateMigratorService>()
            .AsImplementedInterfaces()
            .SingleInstance();

        return builder;
    }

    public static ContainerBuilder RegisterMapper(this ContainerBuilder builder)
    {
        builder.RegisterAutoMapper(cfg => cfg.AddProfiles(
            [
                new SettingsProfile()
            ]));

        return builder;
    }
}