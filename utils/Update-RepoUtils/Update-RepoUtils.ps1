#requires -Version 7.0
#requires -PSEdition Core

<#
.SYNOPSIS
    Refreshes a local maksit-repoutils copy from GitHub.

.DESCRIPTION
    This script clones the configured repository into a temporary directory,
    removes the current working directory contents, preserves an existing
    scriptsettings.json file, and copies the cloned src contents into the
    current working directory.

    All configuration is stored in scriptsettings.json.

.EXAMPLE
    pwsh -File .\Update-RepoUtils.ps1

.NOTES
    CONFIGURATION (scriptsettings.json):
    - repository.url: Git repository to clone
    - repository.sourceSubdirectory: Folder copied into the target directory
    - repository.preserveFileName: Existing file in the target directory to keep
    - repository.cloneDepth: Depth used for git clone
#>

[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Get the directory of the current script (for loading settings and relative paths)
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$utilsDir = Split-Path $scriptDir -Parent

# The target is the current working directory, not the script directory.
$targetDirectory = (Get-Location).Path

#region Import Modules

$scriptConfigModulePath = Join-Path $utilsDir "ScriptConfig.psm1"
if (-not (Test-Path $scriptConfigModulePath)) {
    Write-Error "ScriptConfig module not found at: $scriptConfigModulePath"
    exit 1
}

$loggingModulePath = Join-Path $utilsDir "Logging.psm1"
if (-not (Test-Path $loggingModulePath)) {
    Write-Error "Logging module not found at: $loggingModulePath"
    exit 1
}

Import-Module $scriptConfigModulePath -Force
Import-Module $loggingModulePath -Force

#endregion

#region Load Settings

$settings = Get-ScriptSettings -ScriptDir $scriptDir

#endregion

#region Configuration

$repositoryUrl = $settings.repository.url
$sourceSubdirectory = if ($settings.repository.sourceSubdirectory) { $settings.repository.sourceSubdirectory } else { 'src' }
$preserveFileName = if ($settings.repository.preserveFileName) { $settings.repository.preserveFileName } else { 'scriptsettings.json' }
$cloneDepth = if ($settings.repository.cloneDepth) { [int]$settings.repository.cloneDepth } else { 1 }
$currentScriptName = Split-Path -Leaf $MyInvocation.MyCommand.Path

#endregion

#region Validate CLI Dependencies

Assert-Command git

if ([string]::IsNullOrWhiteSpace($repositoryUrl)) {
    Write-Error "repository.url is required in scriptsettings.json."
    exit 1
}

#endregion

#region Main

Write-Log -Level "INFO" -Message "========================================"
Write-Log -Level "INFO" -Message "Update RepoUtils Script"
Write-Log -Level "INFO" -Message "========================================"
Write-Log -Level "INFO" -Message "Target directory: $targetDirectory"

$temporaryRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("maksit-repoutils-update-" + [System.Guid]::NewGuid().ToString('N'))

try {
    Write-LogStep "Cloning latest repository snapshot..."
    & git clone --depth $cloneDepth $repositoryUrl $temporaryRoot
    if ($LASTEXITCODE -ne 0) {
        throw "git clone failed with exit code $LASTEXITCODE."
    }
    Write-Log -Level "OK" -Message "Repository cloned"

    $clonedSourceDirectory = Join-Path $temporaryRoot $sourceSubdirectory
    if (-not (Test-Path -Path $clonedSourceDirectory -PathType Container)) {
        throw "The cloned repository does not contain the expected source directory: $clonedSourceDirectory"
    }

    $existingPreservedFile = Join-Path $targetDirectory $preserveFileName
    $preservedFileBackup = $null
    if (Test-Path -Path $existingPreservedFile -PathType Leaf) {
        $preservedFileBackup = Join-Path $temporaryRoot ("preserved-" + $preserveFileName)
        Copy-Item -Path $existingPreservedFile -Destination $preservedFileBackup -Force
        Write-Log -Level "OK" -Message "Preserved existing $preserveFileName"
    }
    else {
        Write-Log -Level "WARN" -Message "No existing $preserveFileName found in target directory"
    }

    Write-LogStep "Cleaning target directory..."
    $itemsToRemove = Get-ChildItem -Path $targetDirectory -Force |
        Where-Object {
            $_.Name -ne $preserveFileName -and
            $_.Name -ne $currentScriptName
        }

    foreach ($item in $itemsToRemove) {
        Remove-Item -Path $item.FullName -Recurse -Force
    }
    Write-Log -Level "OK" -Message "Target directory cleaned"

    Write-LogStep "Copying refreshed source files..."
    Get-ChildItem -Path $clonedSourceDirectory -Force | ForEach-Object {
        Copy-Item -Path $_.FullName -Destination $targetDirectory -Recurse -Force
    }
    Write-Log -Level "OK" -Message "Source files copied"

    if ($preservedFileBackup -and (Test-Path -Path $preservedFileBackup -PathType Leaf)) {
        Copy-Item -Path $preservedFileBackup -Destination $existingPreservedFile -Force
        Write-Log -Level "OK" -Message "$preserveFileName restored"
    }

    Write-Log -Level "OK" -Message "========================================"
    Write-Log -Level "OK" -Message "Update completed successfully!"
    Write-Log -Level "OK" -Message "========================================"
}
finally {
    if (Test-Path -Path $temporaryRoot) {
        Remove-Item -Path $temporaryRoot -Recurse -Force -ErrorAction SilentlyContinue
    }
}

#endregion
