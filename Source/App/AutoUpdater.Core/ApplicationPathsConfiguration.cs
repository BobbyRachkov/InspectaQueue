using Nuke.Common.IO;

namespace Rachkov.InspectaQueue.Abstractions;

public class ApplicationPathsConfiguration : IApplicationPathsConfiguration
{
    public ApplicationPathsConfiguration()
    {
        LocalAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        //If Windows
        IqBaseFolderPath = LocalAppDataPath / "InspectaQueue";

        IqAppFolderPath = IqBaseFolderPath / "App";
        IqAppExecutablePath = IqAppFolderPath / "InspectaQueue.exe";
        IqUpdateZipPath = IqBaseFolderPath / "release.zip";
        IqExtractedZipFolderPath = IqBaseFolderPath / "Release";

        IqAppFolderPath.CreateDirectory();
    }

    private AbsolutePath LocalAppDataPath { get; }
    public AbsolutePath IqBaseFolderPath { get; }
    public AbsolutePath IqAppFolderPath { get; }
    public AbsolutePath IqAppExecutablePath { get; }
    public AbsolutePath? InstallerPath => GetInstallerPath();
    public AbsolutePath IqUpdateZipPath { get; }
    public AbsolutePath IqExtractedZipFolderPath { get; }

    private AbsolutePath? GetInstallerPath()
    {
        var files = Directory.GetFiles(IqBaseFolderPath);
        return files.FirstOrDefault(x => x.StartsWith("Installer") && x.EndsWith(".exe"));
    }

    public AbsolutePath? GetInstallerPath(Version version)
    {

        return IqBaseFolderPath / $"Installer_{version}.exe";
    }

    public bool IsIqInstalled()
    {
        return IqAppExecutablePath.FileExists();
    }
}