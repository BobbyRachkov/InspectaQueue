using Nuke.Common.IO;

namespace Rachkov.InspectaQueue.Abstractions;

public static class ApplicationPaths
{
    static ApplicationPaths()
    {
        LocalAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        //If Windows
        IqBaseFolderPath = LocalAppDataPath / "InspectaQueue";

        IqAppFolderPath = IqBaseFolderPath / "App";
        IqAppExecutablePath = IqAppFolderPath / "InspectaQueue.exe";
        IqUpdateZipPath = IqBaseFolderPath / "release.zip";
        IqExtractedZipFolderPath = IqBaseFolderPath / "ReleaseExtracted";

        IqAppFolderPath.CreateDirectory();
    }

    private static AbsolutePath LocalAppDataPath { get; }
    public static AbsolutePath IqBaseFolderPath { get; }
    public static AbsolutePath IqAppFolderPath { get; }
    public static AbsolutePath IqAppExecutablePath { get; }
    public static AbsolutePath? InstallerPath => GetInstallerPath();
    public static AbsolutePath IqUpdateZipPath { get; }
    public static AbsolutePath IqExtractedZipFolderPath { get; }

    private static AbsolutePath? GetInstallerPath()
    {
        var files = Directory.GetFiles(IqBaseFolderPath);
        return files.FirstOrDefault(x => x.StartsWith("Installer") && x.EndsWith(".exe"));
    }

    public static AbsolutePath? GetInstallerPath(Version version)
    {

        return IqBaseFolderPath / $"Installer_{version}.exe";
    }

    public static bool IsIqInstalled()
    {
        return IqAppExecutablePath.FileExists();
    }
}