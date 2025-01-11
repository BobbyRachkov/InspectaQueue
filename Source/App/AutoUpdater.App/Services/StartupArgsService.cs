using System;

namespace AutoUpdater.App.Services;

public class StartupArgsService
{
    public static StartupArgsService? Instance { get; private set; }

    private StartupArgsService(bool isForceUpdate, bool isQuietUpdate)
    {
        IsForceUpdate = isForceUpdate;
        IsQuietUpdate = isQuietUpdate;
    }

    public bool IsForceUpdate { get; }
    public bool IsQuietUpdate { get; }

    public static void Init(bool isForceUpdate, bool isQuietUpdate)
    {
        if (Instance is not null)
        {
            throw new InvalidOperationException("Cannot initialize Startup args service second time.");
        }

        Instance = new StartupArgsService(isForceUpdate, isQuietUpdate);
    }
}