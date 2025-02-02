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
        return await CreateShortcut(shortcutPath, cancellationToken);
    }

    public async Task<bool> CreateOsSearchIndex(CancellationToken cancellationToken = default)
    {
        var startMenuPath = (AbsolutePath)Environment.GetFolderPath(Environment.SpecialFolder.StartMenu) / "Programs" / "InspectaQueue.lnk";

        if (startMenuPath.FileExists())
        {
            return true;
        }

        return await CreateShortcut(startMenuPath, cancellationToken);
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

        string psScript =
            "$RegistryPath = 'HKLM:\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\InspectaQueue'; " +
            "if (-not (Test-Path $RegistryPath)) { New-Item -Path $RegistryPath -Force | Out-Null }; " +
            "if (-not (Get-ItemProperty -Path $RegistryPath -Name 'InstallDate' -ErrorAction SilentlyContinue)) { Set-ItemProperty -Path $RegistryPath -Name 'InstallDate' -Value (Get-Date -Format 'yyyyMMdd') -Type String }; ";

        foreach (var arg in args)
        {
            psScript += $"Set-ItemProperty -Path $RegistryPath -Name '{arg.Key}' -Value '{arg.Value}' -Type String; ";
        }

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
        catch
        {
            return false;
        }
    }

    private async Task<bool> IsRegistryUpToDate(Dictionary<string, string> expectedValues, CancellationToken cancellationToken = default)
    {
        var command = new System.Text.StringBuilder();
        command.AppendLine("$RegistryPath = 'HKLM:\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\InspectaQueue';");
        command.AppendLine("if (-not (Test-Path $RegistryPath)) { Write-Output 'False'; exit; }");
        command.AppendLine("$properties = Get-ItemProperty -Path $RegistryPath;");
        command.AppendLine("Write-Output $properties;");

        foreach (var kvp in expectedValues)
        {
            command.AppendLine($"if ($properties.{kvp.Key} -ne '{kvp.Value}') {{ Write-Output 'False'; exit; }}");
        }

        command.AppendLine("Write-Output 'True';");

        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -NonInteractive -ExecutionPolicy Bypass -Command \"{command}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        };

        try
        {
            using var process = Process.Start(psi);
            if (process is null)
            {
                return false;
            }

            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            return output.Trim().Equals("True", StringComparison.OrdinalIgnoreCase);
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
}