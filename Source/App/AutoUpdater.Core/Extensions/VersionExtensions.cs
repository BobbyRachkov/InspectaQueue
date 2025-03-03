namespace Rachkov.InspectaQueue.AutoUpdater.Core.Extensions;

public static class VersionExtensions
{
    public static Version ToVersion(this (int major, int minor, int patch) version)
    {
        return new Version(version.major, version.minor, version.patch);
    }
}