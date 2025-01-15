using Nuke.Common.IO;

namespace Rachkov.InspectaQueue.Abstractions;

public interface IApplicationPathsConfiguration
{
    AbsolutePath IqBaseFolderPath { get; }
    AbsolutePath IqAppFolderPath { get; }
    AbsolutePath IqAppExecutablePath { get; }
    AbsolutePath? InstallerPath { get; }
    AbsolutePath IqUpdateZipPath { get; }
    AbsolutePath IqExtractedZipFolderPath { get; }
    AbsolutePath? GetInstallerPath(Version version);
    bool IsIqInstalled();
}