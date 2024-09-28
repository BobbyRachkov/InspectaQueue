# Define variables
$repoOwner = "BobbyRachkov"
$repoName = "InspectaQueue"
$zipName = "release.zip"
$targetDir = "$env:LOCALAPPDATA\InspectaQueue"
$appDir = "$env:LOCALAPPDATA\InspectaQueue\App"
$githubApi = "https://api.github.com/repos/$repoOwner/$repoName/releases/latest"
$exeName = "InspectaQueue.exe"  # Change this to the name of the executable

# Make target directory if it doesn't exist
if (-not (Test-Path -Path $targetDir)) {
    Write-Host "Creating target directory: $targetDir"
    New-Item -ItemType Directory -Path $targetDir
} else {
    Write-Host "Target directory exists: $targetDir"
}

# Fetch the latest release URL from GitHub API
Write-Host "Fetching the latest release..."
$latestRelease = Invoke-RestMethod -Uri $githubApi

# Parse the download URL for the zip file
$downloadUrl = $latestRelease.assets | Where-Object { $_.name -like "*.zip" } | Select-Object -ExpandProperty browser_download_url

if (-not $downloadUrl) {
    Write-Host "Failed to find download URL. Exiting."
    exit 1
}

Write-Host "Download URL is: $downloadUrl"

# Download the release zip file
Write-Host "Downloading release..."
Invoke-WebRequest -Uri $downloadUrl -OutFile $zipName

if (-not (Test-Path -Path $zipName)) {
    Write-Host "Failed to download the zip file. Exiting."
    exit 1
}

# Unzip the file
Write-Host "Extracting zip to $targetDir..."
Expand-Archive -Path $zipName -DestinationPath $targetDir -Force

if (-not (Test-Path -Path "$appDir\$exeName")) {
    Write-Host "Failed to extract or locate the executable. Exiting."
    exit 1
}

# Clean up
Remove-Item $zipName

#Create desktop shortcut
Write-Host "Creating desktop shortcut..."
$WshShell = New-Object -COMObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut("$Home\Desktop\InspectaQueue.lnk")
$Shortcut.TargetPath = "$appDir\$exeName"
$Shortcut.WorkingDirectory = "$appDir"
$Shortcut.Save()

# Run the executable
Write-Host "Running the executable..."
Start-Process "$appDir\$exeName" -WorkingDirectory "$appDir"

Write-Host "Done."