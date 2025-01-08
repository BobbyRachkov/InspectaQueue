using Nuke.Common.IO;
using System;

namespace AutoUpdater.App.Services;

public class FileService
{
    public FileService()
    {
        LocalAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        IqFolderBasePath = LocalAppDataPath / "InspectaQueue";
        IqFolderAppPath = IqFolderBasePath / "App";
        IqFolderInstallerPath = IqFolderBasePath / "InspectaQueue_Installer.exe";
        IqFolderNewZipPath = IqFolderBasePath / "release.zip";
        IqFolderExtractedZipPath = IqFolderBasePath / "ReleaseExtracted";
    }

    public AbsolutePath LocalAppDataPath { get; }
    public AbsolutePath IqFolderBasePath { get; }
    public AbsolutePath IqFolderAppPath { get; }
    public AbsolutePath IqFolderInstallerPath { get; }
    public AbsolutePath IqFolderNewZipPath { get; }
    public AbsolutePath IqFolderExtractedZipPath { get; }

    public bool IsIqInstalled()
    {
        return !string.IsNullOrWhiteSpace(IqFolderBasePath.ExistingDirectory());
    }
}