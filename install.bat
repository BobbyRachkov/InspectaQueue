@echo off
setlocal

:: Define variables
set REPO_OWNER=BobbyRachkov
set REPO_NAME=InspectaQueue
set ZIP_NAME=release.zip
set TARGET_DIR=%LOCALAPPDATA%\InspectaQueue
set DESTINATION_DIR=%LOCALAPPDATA%\InspectaQueue\App
set APP_NAME=InspectaQueue.exe
set GITHUB_API=https://api.github.com/repos/%REPO_OWNER%/%REPO_NAME%/releases/latest

:: Make target directory if it doesn't exist
if not exist "%TARGET_DIR%" mkdir "%TARGET_DIR%"

:: Fetch the latest release URL from GitHub API
echo Fetching the latest release...
curl -s %GITHUB_API% > latest_release.json

:: Parse the download URL for the zip file (assumes it contains "browser_download_url")
for /f "tokens=*" %%i in ('type latest_release.json ^| findstr /i "browser_download_url"') do set DOWNLOAD_URL=%%i
set DOWNLOAD_URL=%DOWNLOAD_URL:browser_download_url"=%
set DOWNLOAD_URL=%DOWNLOAD_URL:"=%
set DOWNLOAD_URL=%DOWNLOAD_URL::=%
set DOWNLOAD_URL=%DOWNLOAD_URL: =%
set DOWNLOAD_URL=%DOWNLOAD_URL:https=https:%


:: Download the release zip file
echo Downloading release...
curl -L "%DOWNLOAD_URL%" -o "%ZIP_NAME%"

:: Unzip the file (using PowerShell if tar is not available)
echo Extracting zip to %TARGET_DIR%...
tar -xf %ZIP_NAME% -C "%TARGET_DIR%" 2>nul || powershell -command "Expand-Archive -Path '%CD%\%ZIP_NAME%' -DestinationPath '%TARGET_DIR%'"

:: Clean up
del latest_release.json
del %ZIP_NAME%

echo Trying to create desktop shortcut
set SHORTCUT='%userprofile%\Desktop\InspectaQueue.lnk'
set PWS=powershell.exe -ExecutionPolicy Bypass -NoLogo -NonInteractive -NoProfile
%PWS% -Command "$ws = New-Object -ComObject WScript.Shell; $s = $ws.CreateShortcut(%SHORTCUT%); $S.TargetPath = '%DESTINATION_DIR%\%APP_NAME%'; $S.WorkingDirectory = '%DESTINATION_DIR%'; $S.Save()"

echo Done.
echo You can now close this window
cd /d %DESTINATION_DIR% && start %APP_NAME%
pause

