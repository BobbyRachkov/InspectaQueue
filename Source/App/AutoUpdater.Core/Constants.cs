namespace Rachkov.InspectaQueue.Abstractions;

internal class Constants
{
    public class Url
    {
        public const string RepositoryApi = "https://api.github.com/repos/BobbyRachkov/InspectaQueue/";
        public const string ReleasesPath = "releases";
    }

    public class Path
    {
        public const string Config = "config.json";
        public const string MigratedConfig = "..\\config.json";
        public const string ProvidersFolder = "Providers";
        public const string MigratedProvidersFolder = "..\\Providers";
    }
}