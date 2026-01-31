<#
.SYNOPSIS
    AI-assisted changelog generation and license year update.

.DESCRIPTION
    Generates changelog entries from uncommitted changes using a 3-pass LLM pipeline:
    1. Analyze: Convert changes to changelog items
    2. Consolidate: Merge similar items, remove duplicates
    3. Format: Structure as Keep a Changelog format

    Also updates LICENSE.md copyright year if needed.
    Optional RAG pre-processing clusters related changes using embeddings.
    All configuration is in changelogsettings.json.

.PARAMETER DryRun
    Show what would be generated without making changes.
    Enables debug output showing intermediate LLM results.
    Does not modify CHANGELOG.md or LICENSE.md.

.USAGE
    Generate changelog and update license:
        .\Generate-Changelog.ps1

    Dry run (preview without changes):
        .\Generate-Changelog.ps1 -DryRun

.NOTES
    Requires:
    - Ollama running locally (configured in changelogsettings.json)
    - OllamaClient.psm1 and BuildUtils.psm1 modules
    
    Configuration (changelogsettings.json):
    - csprojPath: Path to .csproj file for version
    - outputFile: Path to CHANGELOG.md
    - licensePath: Path to LICENSE.md
    - debug: Enable debug output
    - models: LLM models for each pass
    - prompts: Prompt templates
#>

param(
    [switch]$DryRun
)

# ==============================================================================
# PATH CONFIGURATION
# ==============================================================================

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = git rev-parse --show-toplevel 2>$null
if (-not $repoRoot) {
    # Fallback if not in git repo - go up two levels (scripts -> src -> repo root)
    $repoRoot = Split-Path -Parent (Split-Path -Parent $scriptDir)
}

$repoRoot = $repoRoot.Trim()

# Solution directory is one level up from scripts
$solutionDir = Split-Path -Parent $scriptDir

# ==============================================================================
# LOAD SETTINGS
# ==============================================================================

$settingsPath = Join-Path $scriptDir "changelogsettings.json"
if (-not (Test-Path $settingsPath)) {
    Write-Error "Settings file not found: $settingsPath"
    exit 1
}

$settings = Get-Content $settingsPath -Raw | ConvertFrom-Json
Write-Host "Loaded settings from changelogsettings.json" -ForegroundColor Gray

# Resolve paths relative to script location
$CsprojPath = if ($settings.changelog.csprojPath) {
    [System.IO.Path]::GetFullPath((Join-Path $scriptDir $settings.changelog.csprojPath))
}
else {
    Join-Path $solutionDir "MaksIT.Core\MaksIT.Core.csproj"
}

$OutputFile = if ($settings.changelog.outputFile) {
    [System.IO.Path]::GetFullPath((Join-Path $scriptDir $settings.changelog.outputFile))
}
else {
    $null
}

$LicensePath = if ($settings.changelog.licensePath) {
    [System.IO.Path]::GetFullPath((Join-Path $scriptDir $settings.changelog.licensePath))
}
else {
    $null
}

# ==============================================================================
# LICENSE YEAR UPDATE
# ==============================================================================

if ($LicensePath -and (Test-Path $LicensePath)) {
    Write-Host "Checking LICENSE.md copyright year..." -ForegroundColor Gray
    $currentYear = (Get-Date).Year
    $licenseContent = Get-Content $LicensePath -Raw
    
    # Match pattern: "Copyright (c) YYYY - YYYY" and update end year
    $licensePattern = "(Copyright \(c\) \d{4}\s*-\s*)(\d{4})"
    
    if ($licenseContent -match $licensePattern) {
        $existingEndYear = [int]$Matches[2]
        
        if ($existingEndYear -lt $currentYear) {
            if ($DryRun) {
                Write-Host "[DryRun] LICENSE.md needs update: $existingEndYear -> $currentYear" -ForegroundColor Yellow
            }
            else {
                Write-Host "Updating LICENSE.md copyright year: $existingEndYear -> $currentYear" -ForegroundColor Cyan
                $updatedContent = $licenseContent -replace $licensePattern, "`${1}$currentYear"
                Set-Content -Path $LicensePath -Value $updatedContent -NoNewline
                Write-Host "LICENSE.md updated." -ForegroundColor Green
            }
        }
        else {
            Write-Host "LICENSE.md copyright year is current ($existingEndYear)." -ForegroundColor Gray
        }
    }
}

# ==============================================================================
# IMPORT MODULES
# ==============================================================================

# Import build utilities
$buildUtilsPath = Join-Path $scriptDir "BuildUtils.psm1"
if (Test-Path $buildUtilsPath) {
    Import-Module $buildUtilsPath -Force
}
else {
    Write-Error "BuildUtils.psm1 not found: $buildUtilsPath"
    exit 1
}

# Import Ollama client
$ollamaModulePath = Join-Path $scriptDir "OllamaClient.psm1"
if (-not $settings.ollama.enabled) {
    Write-Error "Ollama is disabled in changelogsettings.json"
    exit 1
}

if (-not (Test-Path $ollamaModulePath)) {
    Write-Error "OllamaClient.psm1 not found: $ollamaModulePath"
    exit 1
}

Import-Module $ollamaModulePath -Force
Set-OllamaConfig -ApiUrl $settings.ollama.apiUrl `
                 -DefaultContextWindow $settings.ollama.defaultContextWindow `
                 -DefaultTimeout $settings.ollama.defaultTimeout

# ==============================================================================
# CHANGELOG CONFIGURATION
# ==============================================================================

$clSettings = $settings.changelog
$changelogConfig = @{
    Debug = if ($DryRun) { $true } else { $clSettings.debug }
    EnableRAG = $clSettings.enableRAG
    SimilarityThreshold = $clSettings.similarityThreshold
    FileExtension = $clSettings.fileExtension
    ExcludePatterns = if ($clSettings.excludePatterns) { @($clSettings.excludePatterns) } else { @() }
    Models = @{
        Analyze = @{ 
            Name = $clSettings.models.analyze.name
            Context = $clSettings.models.analyze.context
            MaxTokens = if ($null -ne $clSettings.models.analyze.maxTokens) { $clSettings.models.analyze.maxTokens } else { 0 }
        }
        Reason = @{ 
            Name = $clSettings.models.reason.name
            Context = $clSettings.models.reason.context
            MaxTokens = if ($null -ne $clSettings.models.reason.maxTokens) { $clSettings.models.reason.maxTokens } else { 0 }
            Temperature = if ($clSettings.models.reason.temperature) { $clSettings.models.reason.temperature } else { 0.1 }
        }
        Write = @{ 
            Name = $clSettings.models.write.name
            Context = $clSettings.models.write.context
            MaxTokens = if ($null -ne $clSettings.models.write.maxTokens) { $clSettings.models.write.maxTokens } else { 0 }
        }
        Embed = @{ Name = $clSettings.models.embed.name }
    }
    Prompts = @{
        Analyze = if ($clSettings.prompts.analyze) { 
            if ($clSettings.prompts.analyze -is [array]) { $clSettings.prompts.analyze -join "`n" } else { $clSettings.prompts.analyze }
        } else { "Convert changes to changelog: {{changes}}" }
        Reason = if ($clSettings.prompts.reason) { 
            if ($clSettings.prompts.reason -is [array]) { $clSettings.prompts.reason -join "`n" } else { $clSettings.prompts.reason }
        } else { "Consolidate: {{input}}" }
        Format = if ($clSettings.prompts.format) { 
            if ($clSettings.prompts.format -is [array]) { $clSettings.prompts.format -join "`n" } else { $clSettings.prompts.format }
        } else { "Format as changelog: {{items}}" }
    }
}

# ==============================================================================
# AI CHANGELOG GENERATION FUNCTION
# ==============================================================================

function Get-AIChangelogSuggestion {
    param(
        [Parameter(Mandatory)][string]$Changes,
        [Parameter(Mandatory)][string]$Version
    )
    
    $cfg = $script:changelogConfig
    $debug = $cfg.Debug
    
    # === RAG PRE-PROCESSING ===
    $processedChanges = $Changes
    
    if ($cfg.EnableRAG) {
        Write-Host "  RAG Pre-processing ($($cfg.Models.Embed.Name))..." -ForegroundColor Cyan
        $changeArray = $Changes -split "`n" | Where-Object { $_.Trim() -ne "" }
        
        if ($changeArray.Length -gt 3) {
            Write-Host "  RAG: Embedding $($changeArray.Length) changes..." -ForegroundColor Gray
            $clusters = Group-TextsByEmbedding -Model $cfg.Models.Embed.Name -Texts $changeArray -SimilarityThreshold $cfg.SimilarityThreshold
            Write-Host "  RAG: Reduced to $($clusters.Length) groups" -ForegroundColor Green
            
            # Format clusters
            $grouped = @()
            foreach ($cluster in $clusters) {
                if ($cluster.Length -eq 1) {
                    $grouped += $cluster[0]
                }
                else {
                    $grouped += "[RELATED CHANGES]`n" + ($cluster -join "`n") + "`n[/RELATED CHANGES]"
                }
            }
            $processedChanges = $grouped -join "`n"
            
            if ($debug) {
                Write-Host "`n  [DEBUG] RAG grouped changes:" -ForegroundColor Magenta
                Write-Host $processedChanges -ForegroundColor DarkGray
                Write-Host ""
            }
        }
    }
    
    # === PASS 1: Analyze changes ===
    $m1 = $cfg.Models.Analyze
    Write-Host "  Pass 1/3: Analyzing ($($m1.Name), ctx:$($m1.Context))..." -ForegroundColor Gray
    
    $prompt1 = $cfg.Prompts.Analyze -replace '\{\{changes\}\}', $processedChanges
    $pass1 = Invoke-OllamaPrompt -Model $m1.Name -ContextWindow $m1.Context -MaxTokens $m1.MaxTokens -Prompt $prompt1
    
    if (-not $pass1) { return $null }
    if ($debug) { Write-Host "`n  [DEBUG] Pass 1 output:" -ForegroundColor Magenta; Write-Host $pass1 -ForegroundColor DarkGray; Write-Host "" }
    
    # === PASS 2: Consolidate ===
    $m2 = $cfg.Models.Reason
    Write-Host "  Pass 2/3: Consolidating ($($m2.Name), ctx:$($m2.Context))..." -ForegroundColor Gray
    
    $prompt2 = $cfg.Prompts.Reason -replace '\{\{input\}\}', $pass1
    $pass2 = Invoke-OllamaPrompt -Model $m2.Name -ContextWindow $m2.Context -MaxTokens $m2.MaxTokens -Temperature $m2.Temperature -Prompt $prompt2
    
    if (-not $pass2) { return $pass1 }
    if ($pass2 -match "</think>") { $pass2 = ($pass2 -split "</think>")[-1].Trim() }
    if ($debug) { Write-Host "`n  [DEBUG] Pass 2 output:" -ForegroundColor Magenta; Write-Host $pass2 -ForegroundColor DarkGray; Write-Host "" }
    
    # === PASS 3: Format ===
    $m3 = $cfg.Models.Write
    Write-Host "  Pass 3/3: Formatting ($($m3.Name), ctx:$($m3.Context))..." -ForegroundColor Gray
    
    $prompt3 = $cfg.Prompts.Format -replace '\{\{items\}\}', $pass2
    $pass3 = Invoke-OllamaPrompt -Model $m3.Name -ContextWindow $m3.Context -MaxTokens $m3.MaxTokens -Prompt $prompt3
    
    if (-not $pass3) { return $pass2 }
    if ($debug) { Write-Host "`n  [DEBUG] Pass 3 output:" -ForegroundColor Magenta; Write-Host $pass3 -ForegroundColor DarkGray; Write-Host "" }
    
    # Clean up preamble
    if ($pass3 -match "(### Added|### Changed|### Fixed|### Removed)") {
        $pass3 = $pass3.Substring($pass3.IndexOf($Matches[0]))
    }
    
    # Clean up headers - remove any extra text after "### Added" etc.
    $pass3 = $pass3 -replace '(### Added)[^\n]*', '### Added'
    $pass3 = $pass3 -replace '(### Changed)[^\n]*', '### Changed'
    $pass3 = $pass3 -replace '(### Fixed)[^\n]*', '### Fixed'
    $pass3 = $pass3 -replace '(### Removed)[^\n]*', '### Removed'
    
    # Clean up formatting: remove extra blank lines, normalize line endings
    $pass3 = $pass3 -replace "`r`n", "`n"                    # Normalize to LF
    $pass3 = $pass3 -replace "(\n\s*){3,}", "`n`n"           # Max 1 blank line
    $pass3 = $pass3 -replace "- (.+)\n\n- ", "- `$1`n- "     # No blank between items
    $pass3 = $pass3 -replace "\n{2,}(### )", "`n`n`$1"       # One blank before headers
    
    # Remove empty sections (e.g., "### Fixed\n- (No items)" or "### Removed\n\n###")
    $pass3 = $pass3 -replace "### \w+\s*\n-\s*\(No items\)\s*\n?", ""
    $pass3 = $pass3 -replace "### \w+\s*\n\s*\n(?=###|$)", ""
    $pass3 = $pass3.Trim()
    
    return $pass3
}

# ==============================================================================
# MAIN EXECUTION
# ==============================================================================

Write-Host ""
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host "AI CHANGELOG GENERATOR" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan
Write-Host ""

# Check Ollama availability
if (-not (Test-OllamaAvailable)) {
    Write-Error "Ollama is not available. Start Ollama and try again."
    exit 1
}

Write-Host "Ollama connected: $($settings.ollama.apiUrl)" -ForegroundColor Green
Write-Host "Models: $($changelogConfig.Models.Analyze.Name) | $($changelogConfig.Models.Reason.Name) | $($changelogConfig.Models.Embed.Name)" -ForegroundColor Gray
Write-Host ""

# Get version from csproj
if (-not (Test-Path $CsprojPath)) {
    Write-Error "Csproj file not found: $CsprojPath"
    exit 1
}

[xml]$csproj = Get-Content $CsprojPath
$Version = $csproj.Project.PropertyGroup.Version | Where-Object { $_ } | Select-Object -First 1

Write-Host "Version: $Version" -ForegroundColor White

# Filter function for excluding test files
$excludePatterns = $changelogConfig.ExcludePatterns
function Test-Excluded {
    param([string]$Item)
    foreach ($pattern in $excludePatterns) {
        if ($Item -match [regex]::Escape($pattern)) { return $true }
    }
    return $false
}

# Get committed changes for this version (analyzed diffs)
$committedChanges = Get-CommitChangesAnalysis -Version $Version -CsprojPath $CsprojPath -FileFilter $changelogConfig.FileExtension
$filteredCommitted = $committedChanges | Where-Object { -not (Test-Excluded $_) }

# Get uncommitted changes (staged, modified, new, deleted)
$uncommitted = Get-UncommittedChanges -FileFilter $changelogConfig.FileExtension
$filteredUncommitted = $uncommitted.Summary | Where-Object { -not (Test-Excluded $_) }

# Combine all changes
$allChanges = @()
if ($filteredCommitted.Count -gt 0) { $allChanges += $filteredCommitted }
if ($filteredUncommitted.Count -gt 0) { $allChanges += $filteredUncommitted }

if ($allChanges.Count -eq 0) {
    Write-Host "No changes found for version $Version (excluding tests)" -ForegroundColor Yellow
    exit 0
}

$changeLog = $allChanges -join "`n"

Write-Host "Found $($filteredCommitted.Count) committed changes" -ForegroundColor Gray
Write-Host "Found $($filteredUncommitted.Count) uncommitted changes" -ForegroundColor Gray
Write-Host ""

# Generate changelog from uncommitted changes
$suggestion = Get-AIChangelogSuggestion -Changes $changeLog -Version $Version

if ($suggestion) {
    $fullEntry = "## v$Version`n`n$suggestion"
    
    Write-Host ""
    Write-Host "==========================================" -ForegroundColor Green
    Write-Host "AI SUGGESTED CHANGELOG ENTRY" -ForegroundColor Green
    Write-Host "==========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host $fullEntry -ForegroundColor White
    Write-Host ""
    Write-Host "==========================================" -ForegroundColor Green
    
    # Update changelog file if specified and not in DryRun mode
    if ($OutputFile -and -not $DryRun) {
        if (Test-Path $OutputFile) {
            # Read existing content
            $existingContent = Get-Content $OutputFile -Raw
            
            # Check if this version already exists
            if ($existingContent -match "## v$Version\b") {
                Write-Host ""
                Write-Host "WARNING: Version $Version already exists in $OutputFile" -ForegroundColor Yellow
                Write-Host "Skipping file update. Review and update manually if needed." -ForegroundColor Yellow
            }
            else {
                # Find insertion point (after header, before first version entry)
                # Header typically ends before first "## v" line
                if ($existingContent -match '(?s)(^.*?)(\r?\n)(## v)') {
                    $header = $Matches[1]
                    $newline = $Matches[2]
                    $rest = $existingContent.Substring($header.Length + $newline.Length)
                    $newContent = $header + "`n`n" + $fullEntry + "`n`n" + $rest
                }
                else {
                    # No existing version entries - append after content
                    $newContent = $existingContent.TrimEnd() + "`n`n" + $fullEntry + "`n"
                }
                
                # Normalize multiple blank lines to max 2
                $newContent = $newContent -replace "(\r?\n){3,}", "`n`n"
                
                $newContent | Out-File -FilePath $OutputFile -Encoding utf8 -NoNewline
                Write-Host ""
                Write-Host "Updated: $OutputFile" -ForegroundColor Cyan
            }
        }
        else {
            # Create new file with header
            $newContent = @"
# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

$fullEntry
"@
            $newContent | Out-File -FilePath $OutputFile -Encoding utf8
            Write-Host ""
            Write-Host "Created: $OutputFile" -ForegroundColor Cyan
        }
    }
    elseif ($OutputFile -and $DryRun) {
        Write-Host ""
        Write-Host "[DryRun] Would update: $OutputFile" -ForegroundColor Yellow
    }
    
    Write-Host ""
    if ($DryRun) {
        Write-Host "DryRun complete. No files were modified." -ForegroundColor Yellow
    }
    else {
        Write-Host "Review the changelog entry, then commit." -ForegroundColor Yellow
    }
}
else {
    Write-Error "AI changelog generation failed"
    exit 1
}

Write-Host ""
