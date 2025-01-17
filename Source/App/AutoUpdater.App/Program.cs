using AutoUpdater.App.Services;
using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.Linq;
using Rachkov.InspectaQueue.AutoUpdater.Core;

namespace AutoUpdater.App
{
    internal sealed class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            ParseArgs(args);

            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }

        private static void ParseArgs(string[] args)
        {
            var normalizedArgs = args.Select(x => x.ToLower().TrimStart('-')).ToArray();

            var isForceUpdate = normalizedArgs.Any(x => x == Constants.StartupArgs.ForceUpdateArg);
            var isQuietUpdate = normalizedArgs.Any(x => x == Constants.StartupArgs.QuietUpdateArg);
            var isPrerelease = normalizedArgs.Any(x => x == Constants.StartupArgs.PrereleaseVersionArg);

            StartupArgsService.Init(isForceUpdate, isQuietUpdate, isPrerelease);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace()
                .UseReactiveUI();
    }
}
