using Nuke.Common.IO;

namespace Rachkov.InspectaQueue.AutoUpdater.Core.Services.Paths;

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
        MigrationsDllPath = IqExtractedZipDirectory / "Migration" / "Migrations.dll";
        MigrationsAbstractionsDllPath = IqExtractedZipDirectory / "Migration" / "Abstractions.dll";
        IqExtractedAppDirectory = IqExtractedZipDirectory / "App";
        IqExtractedProvidersDirectory = IqExtractedZipDirectory / "Providers";
        InstallerProxy = IqBaseDirectory / "Uninstaller.lnk";

        ConfigFilePath = IqBaseDirectory / "config.json";
        ConfigBackupFilePath = IqBaseDirectory / "config-backup.json";
        OldConfigFilePath = IqAppDirectory / "config.json";
        ProvidersDirectory = IqBaseDirectory / "Providers";
        OldProvidersDirectory = IqAppDirectory / "Providers";

        IqAppDirectory.CreateDirectory();
    }

    private AbsolutePath LocalAppDataDirectory { get; }
    public AbsolutePath IqBaseDirectory { get; }
    public AbsolutePath IqAppDirectory { get; }
    public AbsolutePath IqExtractedAppDirectory { get; }
    public AbsolutePath IqExtractedProvidersDirectory { get; }
    public AbsolutePath ConfigFilePath { get; }
    public AbsolutePath ConfigBackupFilePath { get; }
    public AbsolutePath OldConfigFilePath { get; }
    public AbsolutePath ProvidersDirectory { get; }
    public AbsolutePath OldProvidersDirectory { get; }
    public AbsolutePath IqAppExecutablePath { get; }
    public AbsolutePath? InstallerPath => GetInstallerPath();
    public AbsolutePath InstallerProxy { get; }
    public AbsolutePath IqUpdateZipPath { get; }
    public AbsolutePath IqExtractedZipDirectory { get; }
    public AbsolutePath MigrationsDllPath { get; }
    public AbsolutePath MigrationsAbstractionsDllPath { get; }

    private AbsolutePath? GetInstallerPath()
    {
        var files = Directory.GetFiles(IqBaseDirectory);
        return files.FirstOrDefault(x =>
        {
            var file = (AbsolutePath)x;
            return file.Name.StartsWith("Installer") && file.Extension == ".exe";
        });
    }

    public Version? GetInstallerVersion()
    {
        var installerPath = GetInstallerPath();

        if (installerPath is null)
        {
            return null;
        }

        var versionString = installerPath.NameWithoutExtension.Substring(installerPath.NameWithoutExtension.IndexOf("_", StringComparison.Ordinal) + 1);
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