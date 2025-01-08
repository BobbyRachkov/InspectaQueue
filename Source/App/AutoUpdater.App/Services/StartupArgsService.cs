using System;

namespace AutoUpdater.App.Services;

public class StartupArgsService
{
    public static StartupArgsService? Instance { get; private set; }

    private StartupArgsService(bool isForceUpdate)
    {
        IsForceUpdate = isForceUpdate;
    }

    public bool IsForceUpdate { get; }

    public static void Init(bool isForceUpdate)
    {
        if (Instance is not null)
        {
            throw new InvalidOperationException("Cannot initialize Startup args service second time.");
        }

        Instance = new StartupArgsService(isForceUpdate);
    }
}