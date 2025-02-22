using Nuke.Common.IO;
using Rachkov.InspectaQueue.AutoUpdater.Abstractions.Migrations.Interfaces;
using System.Diagnostics;

namespace Rachkov.InspectaQueue.AutoUpdater.Core.Services.Migrations;

public class WindowsInstallerRunner : IInstallerRunner
{
    private readonly InstallerDownloader _installerDownloader;

    public WindowsInstallerRunner(InstallerDownloader installerDownloader)
    {
        _installerDownloader = installerDownloader;
    }

    public async Task<bool> TryInstallPrerequisiteIfNeeded(IPrerequisite prerequisite, AbsolutePath tempDirectory, CancellationToken cancellationToken = default)
    {
        if (prerequisite.WindowsProcedure.HasToBePerformed())
        {
            return true;
        }

        var installerPath = tempDirectory / $"{Guid.NewGuid()}.exe";

        try
        {
            var downloadSuccess = await _installerDownloader.DownloadInstaller(prerequisite.WindowsProcedure.UrlOfInstaller, installerPath, cancellationToken);
            if (!downloadSuccess)
            {
                return false;
            }

            var psi = new ProcessStartInfo
            {
                FileName = installerPath,
                Arguments = prerequisite.WindowsProcedure.InstallerArgs ?? string.Empty,
                UseShellExecute = true,
                CreateNoWindow = false,
                Verb = "runas"
            };

            using var process = Process.Start(psi);
            if (process is null)
            {
                return false;
            }

            await process.WaitForExitAsync(cancellationToken);
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
        finally
        {
            installerPath.DeleteFile();
        }
    }
}
