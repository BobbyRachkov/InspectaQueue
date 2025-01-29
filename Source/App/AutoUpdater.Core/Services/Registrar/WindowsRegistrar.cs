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

    public async Task<bool> RegisterAppInProgramUninstallList(Version? appVersion = null, CancellationToken cancellationToken = default)
    {
        var installerPath = _pathsConfiguration.InstallerPath ?? _pathsConfiguration.IqBaseDirectory / "Installer.exe";
        var args = new Dictionary<string, string>()
        {
            {"DisplayName","InspectaQueue"},
            {"Publisher","Bobi Rachkov"},
            {"DisplayIcon",_pathsConfiguration.IqAppExecutablePath},
            {"URLInfoAbout","https://github.com/BobbyRachkov/InspectaQueue"},
            {"HelpLink","https://github.com/BobbyRachkov/InspectaQueue"},
            {"URLUpdateInfo","https://github.com/BobbyRachkov/InspectaQueue/releases/latest"},
            {"UninstallString",installerPath},
            {"ModifyPath",installerPath}
        };

        if (appVersion is not null)
        {
            args.Add("DisplayVersion", appVersion.ToString());
        }

        string psScript =
            "$RegistryPath = 'HKLM:\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\InspectaQueue'; " +
            "if (-not (Test-Path $RegistryPath)) { New-Item -Path $RegistryPath -Force | Out-Null }; " +
            "if (-not (Get-ItemProperty -Path $RegistryPath -Name 'InstallDate' -ErrorAction SilentlyContinue)) { Set-ItemProperty -Path $RegistryPath -Name 'InstallDate' -Value (Get-Date -Format 'yyyyMMdd') -Type String }; ";// +
                                                                                                                                                                                                                                  //"$AppRegistryPath = 'HKLM:\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\InspectaQueue.exe'; " +
                                                                                                                                                                                                                                  //"if (-not (Test-Path $AppRegistryPath)) { New-Item -Path $AppRegistryPath -Force | Out-Null }; " +
                                                                                                                                                                                                                                  //$"Set-ItemProperty -Path $AppRegistryPath -Name '(Default)' -Value '{_pathsConfiguration.IqAppExecutablePath}' -Type String; " +
                                                                                                                                                                                                                                  //$"Set-ItemProperty -Path $AppRegistryPath -Name 'Path' -Value '{_pathsConfiguration.IqAppDirectory}' -Type String; " +
                                                                                                                                                                                                                                  //$"Restart-Service WSearch; ";

        foreach (var arg in args)
        {
            psScript += $"Set-ItemProperty -Path $RegistryPath -Name '{arg.Key}' -Value '{arg.Value}' -Type String; ";
        }

        psScript += "pause;";
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{psScript}\"",
            Verb = "runas",
            UseShellExecute = true,
            CreateNoWindow = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            RedirectStandardInput = false,
            WindowStyle = ProcessWindowStyle.Normal
        };

        var process = Process.Start(psi);

        if (process is null)
        {
            return false;
        }

        await process.WaitForExitAsync(cancellationToken);

        return true;
    }
}