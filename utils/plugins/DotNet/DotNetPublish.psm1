#requires -Version 7.0
#requires -PSEdition Core

<#
.SYNOPSIS
    .NET publish plugin for producing application release artifacts.

.DESCRIPTION
    This plugin publishes configured .NET projects into the artifacts directory
    and appends those publish folders to shared archive inputs so later plugins
    can zip them next to any earlier pack outputs. Existing NuGet package facts
    (packageFile) are left unchanged.
#>

if (-not (Get-Command Import-PluginDependency -ErrorAction SilentlyContinue)) {
    $srcDir = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
        $pluginSupportModulePath = Join-Path $srcDir "modules/Engine/PluginSupport.psm1"
    if (Test-Path $pluginSupportModulePath -PathType Leaf) {
        Import-Module $pluginSupportModulePath -Force -Global -ErrorAction Stop
    }
}

function Invoke-Plugin {
    param(
        [Parameter(Mandatory = $true)]
        $Settings
    )

    Import-PluginDependency -ModuleName "Logging" -RequiredCommand "Write-Log"
    Import-PluginDependency -ModuleName "ScriptConfig" -RequiredCommand "Assert-Command"
    Import-PluginDependency -ModuleName "EngineContext" -RequiredCommand "Set-EngineFact"

    $pluginSettings = $Settings
    $sharedSettings = $Settings.context
    $scriptDir = $sharedSettings.scriptDir
    $projectFiles = @()

    Assert-Command dotnet

    if ($pluginSettings.PSObject.Properties['projectFiles'] -and $null -ne $pluginSettings.projectFiles) {
        $projectFiles = @(Resolve-RelativePaths -Value $pluginSettings.projectFiles -BasePath $scriptDir)
    }
    else {
        $fromFact = Get-EngineFact -Context $sharedSettings -Namespace 'dotnet' -Name 'projectFiles' -LegacyProperty @('projectFiles')
        if ($null -ne $fromFact) {
            $projectFiles = @($fromFact)
        }
        elseif ($sharedSettings.PSObject.Properties['projectFiles'] -and $null -ne $sharedSettings.projectFiles) {
            $projectFiles = @($sharedSettings.projectFiles)
        }
    }

    if ($projectFiles.Count -eq 0) {
        throw "DotNetPublish plugin requires projectFiles in plugin settings or projectFiles on shared context."
    }

    if ($pluginSettings.PSObject.Properties['artifactsDir'] -and -not [string]::IsNullOrWhiteSpace([string]$pluginSettings.artifactsDir)) {
        $artifactsDirectory = [System.IO.Path]::GetFullPath((Join-Path $scriptDir ([string]$pluginSettings.artifactsDir)))
        Set-EngineState -Context $sharedSettings -Name 'artifactsDirectory' -Value $artifactsDirectory
        Set-EngineState -Context $sharedSettings -Name 'releaseDir' -Value $artifactsDirectory
    }
    else {
        $artifactsDirectory = $sharedSettings.artifactsDirectory
    }

    if ([string]::IsNullOrWhiteSpace([string]$artifactsDirectory)) {
        throw "DotNetPublish plugin requires artifactsDir in plugin settings or artifactsDirectory on shared context."
    }

    if (!(Test-Path $artifactsDirectory)) {
        New-Item -ItemType Directory -Path $artifactsDirectory | Out-Null
    }

    $existing = Get-EngineFact -Context $sharedSettings -Namespace 'release' -Name 'archiveInputs' -LegacyProperty @('releaseArchiveInputs')
    $archiveInputs = [System.Collections.Generic.List[object]]::new()
    if ($null -ne $existing) {
        foreach ($item in @($existing)) {
            if ($null -ne $item) {
                $archiveInputs.Add($item)
            }
        }
    }

    foreach ($publishProjectPath in $projectFiles) {
        $publishDir = Join-Path $artifactsDirectory ([System.IO.Path]::GetFileNameWithoutExtension($publishProjectPath))

        if (Test-Path $publishDir) {
            Remove-Item -Path $publishDir -Recurse -Force
        }

        Write-Log -Level "STEP" -Message "Publishing release artifact..."
        $dotnetPublishArguments = @(
            'publish', $publishProjectPath, '-c', 'Release', '-o', $publishDir, '--nologo'
        )
        & dotnet @dotnetPublishArguments
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet publish failed for $publishProjectPath."
        }

        $publishedItems = @(Get-ChildItem -Path $publishDir -Force -ErrorAction SilentlyContinue)
        if ($publishedItems.Count -eq 0) {
            throw "dotnet publish completed, but no files were produced in: $publishDir"
        }

        Write-Log -Level "OK" -Message "  Published artifact ready: $publishDir"
        $archiveInputs.Add($publishDir)
    }

    Set-EngineFact -Context $sharedSettings -Namespace 'release' -Name 'archiveInputs' -Value @($archiveInputs) -Overwrite Replace -LegacyProperty 'releaseArchiveInputs'
}

Export-ModuleMember -Function Invoke-Plugin
