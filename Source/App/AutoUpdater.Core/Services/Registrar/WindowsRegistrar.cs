using Microsoft.Win32;
using Nuke.Common.IO;
using System.Diagnostics;
#pragma warning disable CA1416

namespace Rachkov.InspectaQueue.AutoUpdater.Core.Services.Registrar;

public class WindowsRegistrar : IRegistrar
{
    private readonly IApplicationPathsConfiguration _pathsConfiguration;

    public WindowsRegistrar(IApplicationPathsConfiguration pathsConfiguration)
    {
        _pathsConfiguration = pathsConfiguration;
    }

    public async Task<bool> CreateDesktopShortcut(CancellationToken cancellationToken = default)
    {
        var shortcutPath = (AbsolutePath)Environment.GetFolderPath(Environment.SpecialFolder.Desktop) / "InspectaQueue.lnk";

        var powershellCommand = "-ExecutionPolicy Bypass -NoLogo -NonInteractive -NoProfile " +
                                $"$ws = New-Object -ComObject WScript.Shell; " +
                                $"$s = $ws.CreateShortcut('{shortcutPath}'); " +
                                $"$S.TargetPath = '{_pathsConfiguration.IqAppExecutablePath}'; " +
                                $"$S.WorkingDirectory = '{_pathsConfiguration.IqAppDirectory}'; " +
                                $"$S.Save(); " +
                                $"Wait-Debugger";

        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = powershellCommand,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            RedirectStandardInput = false,
            WindowStyle = ProcessWindowStyle.Hidden
        };

        var process = Process.Start(psi);

        if (process is null)
        {
            return false;
        }

        await process.WaitForExitAsync(cancellationToken);

        return true;
    }

    public bool RegisterAppInProgramsList(Version? appVersion = null)
    {
        using var parent = Registry.LocalMachine.OpenSubKey(
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", true);

        if (parent is null)
        {
            return false;
        }

        RegistryKey? key = null;

        try
        {
            var registryParentName = "InspectaQueue";
            key = parent.OpenSubKey(registryParentName, true) ??
                  parent.CreateSubKey(registryParentName);

            if (key is null)
            {
                return false;
            }

            if (appVersion is not null)
            {
                key.SetValue("ApplicationVersion", appVersion.ToString());
            }

            key.SetValue("DisplayName", "InspectaQueue");
            key.SetValue("Publisher", "Bobi Rachkov");
            key.SetValue("DisplayIcon", _pathsConfiguration.IqAppExecutablePath);
            key.SetValue("DisplayVersion", appVersion.ToString());
            key.SetValue("URLInfoAbout", "https://github.com/BobbyRachkov/InspectaQueue");
            key.SetValue("InstallDate", DateTime.Now.ToString("yyyyMMdd"));
            key.SetValue("UninstallString", _pathsConfiguration.InstallerPath ?? _pathsConfiguration.IqBaseDirectory / "Installer.exe");

            return true;
        }
        finally
        {
            if (key is not null)
            {
                key.Close();
            }
        }

        return true;
    }
}