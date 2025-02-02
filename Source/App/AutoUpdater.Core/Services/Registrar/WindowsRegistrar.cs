using Nuke.Common.IO;
using System.Diagnostics;
#pragma warning disable CA1416

namespace Rachkov.InspectaQueue.AutoUpdater.Core.Services.Registrar;

public class WindowsRegistrar : IRegistrar
{
    private readonly IApplicationPathsConfiguration _pathsConfiguration;
    private readonly AbsolutePath _startMenuPath = (AbsolutePath)Environment.GetFolderPath(Environment.SpecialFolder.StartMenu) / "Programs" / "InspectaQueue.lnk";

    public WindowsRegistrar(IApplicationPathsConfiguration pathsConfiguration)
    {
        _pathsConfiguration = pathsConfiguration;
    }

    public async Task<bool> CreateDesktopShortcut(CancellationToken cancellationToken = default)
    {
        var shortcutPath = (AbsolutePath)Environment.GetFolderPath(Environment.SpecialFolder.Desktop) / "InspectaQueue.lnk";
        return await CreateShortcut(shortcutPath, cancellationToken);
    }

    public async Task<bool> CreateOsSearchIndex(CancellationToken cancellationToken = default)
    {
        if (_startMenuPath.FileExists())
        {
            return true;
        }

        return await CreateShortcut(_startMenuPath, cancellationToken);
    }

    public async Task<bool> RegisterAppInProgramUninstallList(Version? appVersion = null, CancellationToken cancellationToken = default)
    {
        var args = new Dictionary<string, string>()
        {
            {"DisplayName","InspectaQueue"},
            {"Publisher","Bobi Rachkov"},
            {"DisplayIcon",_pathsConfiguration.IqAppExecutablePath},
            {"URLInfoAbout","https://github.com/BobbyRachkov/InspectaQueue"},
            {"HelpLink","https://github.com/BobbyRachkov/InspectaQueue"},
            {"URLUpdateInfo","https://github.com/BobbyRachkov/InspectaQueue/releases/latest"},
            {"UninstallString",_pathsConfiguration.InstallerProxy},
            {"ModifyPath",_pathsConfiguration.InstallerProxy}
        };

        if (appVersion is not null)
        {
            args.Add("DisplayVersion", appVersion.ToString());
        }

        if (await IsRegistryUpToDate(args, cancellationToken))
        {
            return true;
        }

        var psScript =
            "$RegistryPath = 'HKLM:\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\InspectaQueue'; " +
            "if (-not (Test-Path $RegistryPath)) { New-Item -Path $RegistryPath -Force | Out-Null }; " +
            "if (-not (Get-ItemProperty -Path $RegistryPath -Name 'InstallDate' -ErrorAction SilentlyContinue)) { Set-ItemProperty -Path $RegistryPath -Name 'InstallDate' -Value (Get-Date -Format 'yyyyMMdd') -Type String }; ";

        foreach (var arg in args)
        {
            psScript += $"Set-ItemProperty -Path $RegistryPath -Name '{arg.Key}' -Value '{arg.Value}' -Type String; ";
        }

        var psi = GetAdminPowershellProcessStartInfo($"-NoProfile -ExecutionPolicy Bypass -Command \"{psScript}\"");

        var process = Process.Start(psi);

        if (process is null)
        {
            return false;
        }

        await process.WaitForExitAsync(cancellationToken);

        return true;
    }

    public async Task<bool> CreateOrUpdateInstallerProxy(CancellationToken cancellationToken = default)
    {
        try
        {
            _pathsConfiguration.InstallerProxy.DeleteFile();

            var powershellCommand = "-ExecutionPolicy Bypass -NoLogo -NonInteractive -NoProfile " +
                                    $"$ws = New-Object -ComObject WScript.Shell; " +
                                    $"$s = $ws.CreateShortcut('{_pathsConfiguration.InstallerProxy}'); " +
                                    $"$S.TargetPath = '{_pathsConfiguration.InstallerPath}'; " +
                                    $"$S.WorkingDirectory = '{_pathsConfiguration.IqBaseDirectory}'; " +
                                    $"$S.Description = 'InspectaQueue Uninstaller'; " +
                                    "$S.Save(); ";

            var psi = GetPowershellProcessStartInfo(powershellCommand);

            var process = Process.Start(psi);

            if (process is null)
            {
                return false;
            }

            await process.WaitForExitAsync(cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UnregisterAppFromSystem(CancellationToken cancellationToken = default)
    {
        try
        {
            // Remove start menu shortcut
            _startMenuPath.DeleteFile();

            // Remove registry entry
            var command = new System.Text.StringBuilder();
            command.AppendLine("$RegistryPath = 'HKLM:\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\InspectaQueue';");
            command.AppendLine("if (Test-Path $RegistryPath) {");
            command.AppendLine("    Remove-Item -Path $RegistryPath -Force;");
            command.AppendLine("    if (Test-Path $RegistryPath) { exit 1 }");
            command.AppendLine("}");
            command.AppendLine("exit 0");

            var psi = GetAdminPowershellProcessStartInfo($"-NoProfile -NonInteractive -ExecutionPolicy Bypass -Command \"{command}\"");

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
    }

    private async Task<bool> IsRegistryUpToDate(Dictionary<string, string> expectedValues, CancellationToken cancellationToken = default)
    {
        var command = new System.Text.StringBuilder();
        command.AppendLine("$RegistryPath = 'HKLM:\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\InspectaQueue';");
        command.AppendLine("if (-not (Test-Path $RegistryPath)) { exit 1 }");
        command.AppendLine("$properties = Get-ItemProperty -Path $RegistryPath;");

        foreach (var kvp in expectedValues)
        {
            command.AppendLine($"if ($properties.{kvp.Key} -ne '{kvp.Value}') {{ exit 1 }}");
        }

        command.AppendLine("exit 0");

        var psi = GetPowershellProcessStartInfo($"-NoProfile -NonInteractive -ExecutionPolicy Bypass -Command \"{command}\"");

        try
        {
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
    }

    private async Task<bool> CreateShortcut(AbsolutePath shortcutPath, CancellationToken cancellationToken = default)
    {
        var powershellCommand = "-ExecutionPolicy Bypass -NoLogo -NonInteractive -NoProfile " +
                                $"$ws = New-Object -ComObject WScript.Shell; " +
                                $"$s = $ws.CreateShortcut('{shortcutPath}'); " +
                                $"$S.TargetPath = '{_pathsConfiguration.IqAppExecutablePath}'; " +
                                $"$S.WorkingDirectory = '{_pathsConfiguration.IqAppDirectory}'; " +
                                $"$S.Description = 'InspectaQueue - Message Queue Inspector'; " +
                                "$S.Save(); ";

        var psi = GetPowershellProcessStartInfo(powershellCommand);
        var process = Process.Start(psi);

        if (process is null)
        {
            return false;
        }

        await process.WaitForExitAsync(cancellationToken);

        return true;
    }

    private ProcessStartInfo GetAdminPowershellProcessStartInfo(string arguments)
    {
        return new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = arguments,
            Verb = "runas",
            UseShellExecute = true,
            CreateNoWindow = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            RedirectStandardInput = false,
            WindowStyle = ProcessWindowStyle.Hidden
        };
    }

    private ProcessStartInfo GetPowershellProcessStartInfo(string arguments)
    {
        return new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            RedirectStandardInput = false,
            WindowStyle = ProcessWindowStyle.Hidden
        };
    }
}