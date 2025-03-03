namespace Rachkov.InspectaQueue.AutoUpdater.Core;

public class Constants
{
    public class Url
    {
        public const string RepositoryApi = "https://api.github.com/repos/BobbyRachkov/InspectaQueue/";
        public const string ReleasesPath = "releases";
    }

    public class StartupArgs
    {
        public const string ForceUpdateArg = "update";
        public const string QuietUpdateArg = "quiet-update";
        public const string PrereleaseVersionArg = "prerelease";
    }
}