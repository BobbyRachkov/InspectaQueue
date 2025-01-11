namespace Rachkov.InspectaQueue.Abstractions;

public class Constants
{
    public class Url
    {
        public const string RepositoryApi = "https://api.github.com/repos/BobbyRachkov/InspectaQueue/";
        public const string ReleasesPath = "releases";
    }

    public class StartupArgs
    {
        public const string ForceUpdateArg = "update";
        public const string QuietUpdateArg = "quiet-update";
    }

    public class Path
    {
        public const string Config = "config.json";
        public const string MigratedConfig = "..\\config.json";
        public const string ProvidersFolder = "Providers";
        public const string MigratedProvidersFolder = "..\\Providers";
        public const string DownloadPath = "..\\release.zip";
    }

    public class Script
    {
        public const string Finalize = @$"timeout /t 1 /nobreak >nul && if exist ""App"" ( rd /s /q ""App"") &&
                robocopy ""{Path.Config}"" ""App\\{Path.Config}"" /s /xo /r:0\r\n && pause";

        public const string Finalize2 = @$"@echo off
:: Wait for 1 second
timeout /t 1 /nobreak >nul

:: Delete a directory (recursively if it exists)
set ""dirToDelete=App""
if exist ""%dirToDelete%"" (
    rd /s /q ""%dirToDelete%""
    echo Deleted directory: %dirToDelete%
) else (
    echo Directory does not exist: %dirToDelete%
)

:: Unzip a file
set ""zipFile=release.zip""
set ""unzipDestination=temp""
if exist ""%zipFile%"" (
    powershell -command ""Expand-Archive -Path '%zipFile%' -DestinationPath '%unzipDestination%' -Force""
    echo Unzipped file: %zipFile% to %unzipDestination%
) else (
    echo Zip file does not exist: %zipFile%
)

:: Copy a directory recursively
set ""dirToCopy=temp\App""
set ""dirCopyDestination=App""
if exist ""%dirToCopy%"" (
    robocopy ""%dirToCopy%"" ""%dirCopyDestination%"" /e /xo
    echo Copied directory: %dirToCopy% to %dirCopyDestination%
) else (
    echo Directory does not exist: %dirToCopy%
)

:: Copy a file
set ""fileToCopy=config.json""
set ""fileCopyDestination=App\config.json""
if exist ""%fileToCopy%"" (
    copy /y ""%fileToCopy%"" ""%fileCopyDestination%""
    echo Copied file: %fileToCopy% to %fileCopyDestination%
) else (
    echo File does not exist: %fileToCopy%
)

:: Copy a directory recursively
set ""dirToCopy=Providers""
set ""dirCopyDestination=App\Providers""
if exist ""%dirToCopy%"" (
    robocopy ""%dirToCopy%"" ""%dirCopyDestination%"" /e /xo
    echo Copied directory: %dirToCopy% to %dirCopyDestination%
) else (
    echo Directory does not exist: %dirToCopy%
)

:: Cleanup
echo Cleaning up...
del /f /q ""release.zip""
del /f /q ""config.json""
rd /s /q ""temp""
rd /s /q ""Providers""

cd /d App && start InspectaQueue.exe
:: End of script
echo Done!
del /f /q ""restore.bat""";
    }
}