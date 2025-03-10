using Nuke.Common.IO;

namespace Rachkov.InspectaQueue.AutoUpdater.Core.Services.Paths;

public interface IApplicationPathsConfiguration
{
    AbsolutePath IqBaseDirectory { get; }
    AbsolutePath IqAppDirectory { get; }
    AbsolutePath IqAppExecutablePath { get; }
    AbsolutePath? InstallerPath { get; }
    AbsolutePath IqUpdateZipPath { get; }
    AbsolutePath IqExtractedZipDirectory { get; }
    AbsolutePath MigrationsDllPath { get; }
    AbsolutePath MigrationsAbstractionsDllPath { get; }
    AbsolutePath IqExtractedAppDirectory { get; }
    AbsolutePath IqExtractedProvidersDirectory { get; }
    AbsolutePath InstallerProxy { get; }
    AbsolutePath ConfigFilePath { get; }
    AbsolutePath ConfigBackupFilePath { get; }
    AbsolutePath OldConfigFilePath { get; }
    AbsolutePath ProvidersDirectory { get; }
    AbsolutePath OldProvidersDirectory { get; }
    AbsolutePath GetInstallerPath(Version? version);
    Version? GetInstallerVersion();
    bool IsIqInstalled();
}