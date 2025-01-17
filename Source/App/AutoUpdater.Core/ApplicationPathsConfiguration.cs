using Nuke.Common.IO;

namespace Rachkov.InspectaQueue.AutoUpdater.Core;

public class ApplicationPathsConfiguration : IApplicationPathsConfiguration
{
    public ApplicationPathsConfiguration()
    {
        LocalAppDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        //If Windows
        IqBaseDirectory = LocalAppDataDirectory / "InspectaQueue";

        IqAppDirectory = IqBaseDirectory / "App";
        IqAppExecutablePath = IqAppDirectory / "InspectaQueue.exe";
        IqUpdateZipPath = IqBaseDirectory / "release.zip";
        IqExtractedZipDirectory = IqBaseDirectory / "Release";

        ConfigFilePath = IqAppDirectory / "config.json";
        OldConfigFilePath = IqBaseDirectory / "config.json";
        ProvidersDirectory = IqAppDirectory / "Providers";
        OldProvidersDirectory = IqBaseDirectory / "Providers";

        IqAppDirectory.CreateDirectory();
    }

    private AbsolutePath LocalAppDataDirectory { get; }
    public AbsolutePath IqBaseDirectory { get; }
    public AbsolutePath IqAppDirectory { get; }
    public AbsolutePath ConfigFilePath { get; }
    public AbsolutePath OldConfigFilePath { get; }
    public AbsolutePath ProvidersDirectory { get; }
    public AbsolutePath OldProvidersDirectory { get; }
    public AbsolutePath IqAppExecutablePath { get; }
    public AbsolutePath? InstallerPath => GetInstallerPath();
    public AbsolutePath IqUpdateZipPath { get; }
    public AbsolutePath IqExtractedZipDirectory { get; }

    private AbsolutePath? GetInstallerPath()
    {
        var files = Directory.GetFiles(IqBaseDirectory);
        return files.FirstOrDefault(x => x.StartsWith("Installer") && x.EndsWith(".exe"));
    }

    public Version? GetInstallerVersion()
    {
        var installerPath = GetInstallerPath();

        if (installerPath is null)
        {
            return null;
        }

        var versionString = installerPath.NameWithoutExtension.Substring(installerPath.NameWithoutExtension.IndexOf("_", StringComparison.Ordinal));
        return new Version(versionString);
    }

    public AbsolutePath GetInstallerPath(Version? version)
    {
        if (version is null)
        {
            return IqBaseDirectory / "Installer_0.0.0.exe";
        }

        return IqBaseDirectory / $"Installer_{version}.exe";
    }

    public bool IsIqInstalled()
    {
        return IqAppExecutablePath.FileExists();
    }
}