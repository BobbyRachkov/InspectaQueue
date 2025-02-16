using System.Diagnostics;

namespace AutoUpdater.Migrations.Helpers;

public static class DotNetHelper
{
    /// <summary>
    /// Checks if a specific .NET runtime version is installed.
    /// </summary>
    /// <param name="version">Version string in format "Major.Minor" (e.g., "6.0", "7.0") or "Major.Minor.Patch" (e.g., "6.0.16", "7.0.5").
    /// Note: Patch version is optional. If not specified, any patch version will match.</param>
    /// <returns>True if the specified version is installed, false otherwise.</returns>
    /// <example>
    /// Example outputs from dotnet --list-runtimes:
    /// Microsoft.NETCore.App 6.0.16 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
    /// Microsoft.NETCore.App 7.0.5 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]
    /// Microsoft.WindowsDesktop.App 6.0.16 [C:\Program Files\dotnet\shared\Microsoft.WindowsDesktop.App]
    /// 
    /// Example usage:
    /// IsDotNetVersionInstalled("6.0")     // Checks for any 6.0.x version
    /// IsDotNetVersionInstalled("6.0.16")  // Checks for specific patch version
    /// IsDotNetVersionInstalled("7.0.5")   // Checks for specific patch version
    /// </example>
    public static bool IsDotNetVersionInstalled(string version)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "--list-runtimes",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            using var process = Process.Start(psi);

            if (process is not null)
            {
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                return output.Contains($"Microsoft.NETCore.App {version}") ||
                       output.Contains($"Microsoft.WindowsDesktop.App {version}");
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}