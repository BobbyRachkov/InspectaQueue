using Nuke.Common.IO;

namespace Rachkov.InspectaQueue.AutoUpdater.Core;

public interface IApplicationPathsConfiguration
{
    AbsolutePath IqBaseDirectory { get; }
    AbsolutePath IqAppDirectory { get; }
    AbsolutePath IqAppExecutablePath { get; }
    AbsolutePath? InstallerPath { get; }
    AbsolutePath IqUpdateZipPath { get; }
    AbsolutePath IqExtractedZipDirectory { get; }
    AbsolutePath IqExtractedAppDirectory { get; }
    public AbsolutePath ConfigFilePath { get; }
    public AbsolutePath OldConfigFilePath { get; }
    public AbsolutePath ProvidersDirectory { get; }
    public AbsolutePath OldProvidersDirectory { get; }
    AbsolutePath GetInstallerPath(Version? version);
    Version? GetInstallerVersion();
    bool IsIqInstalled();
}