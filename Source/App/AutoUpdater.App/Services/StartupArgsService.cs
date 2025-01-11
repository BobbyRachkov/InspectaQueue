using System;

namespace AutoUpdater.App.Services;

public class StartupArgsService
{
    public static StartupArgsService? Instance { get; private set; }

    private StartupArgsService(bool isForceUpdate, bool isQuietUpdate, bool isPrerelease)
    {
        IsForceUpdate = isForceUpdate;
        IsQuietUpdate = isQuietUpdate;
        IsPrerelease = isPrerelease;
    }

    public bool IsForceUpdate { get; }
    public bool IsQuietUpdate { get; }
    public bool IsPrerelease { get; }

    public static void Init(bool isForceUpdate, bool isQuietUpdate, bool isPrerelease)
    {
        if (Instance is not null)
        {
            throw new InvalidOperationException("Cannot initialize Startup args service second time.");
        }

        Instance = new StartupArgsService(isForceUpdate, isQuietUpdate, isPrerelease);
    }
}