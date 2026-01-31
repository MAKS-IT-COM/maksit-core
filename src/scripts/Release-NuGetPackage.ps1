<#
.SYNOPSIS
    Release script for MaksIT.Core NuGet package and GitHub release.

.DESCRIPTION
    This script automates the release process for MaksIT.Core library.
    The script is IDEMPOTENT - you can safely re-run it if any step fails.
    It will skip already-completed steps (NuGet and GitHub) and only create what's missing.
    
    Features:
    - Validates environment and prerequisites
    - Checks if version already exists on NuGet.org (skips if released)
    - Checks if GitHub release exists (skips if released)
    - Scans for vulnerable packages (security check)
    - Builds and tests the project (Windows + Linux via Docker)
    - Collects code coverage with Coverlet (threshold enforcement optional)
    - Generates test result artifacts (TRX format) and coverage reports
    - Displays test results with pass/fail counts and coverage percentage
    - Publishes to NuGet.org
    - Creates a GitHub release with changelog and package assets
    - Shows timing summary for all steps

.REQUIREMENTS
    Environment Variables:
    - NUGET_MAKS_IT        : NuGet.org API key for publishing packages
    - GITHUB_MAKS_IT_COM   : GitHub Personal Access Token (needs 'repo' scope)

    Tools (Required):
    - dotnet CLI           : For building, testing, and packing
    - git                  : For version control operations
    - gh (GitHub CLI)      : For creating GitHub releases
    - docker               : For cross-platform Linux testing

.WORKFLOW
    1. VALIDATION PHASE
       - Check required environment variables (NuGet key, GitHub token)
       - Check required tools are installed (dotnet, git, gh, docker)
       - Verify no uncommitted changes in working directory
       - Authenticate GitHub CLI

    2. VERSION & RELEASE CHECK PHASE (Idempotent)
       - Read latest version from CHANGELOG.md
       - Find commit with matching version tag
       - Validate tag is on configured release branch (from scriptsettings.json)
       - Check if already released on NuGet.org (mark for skip if yes)
       - Check if GitHub release exists (mark for skip if yes)
       - Read target framework from MaksIT.Core.csproj
       - Extract release notes from CHANGELOG.md for current version

    3. SECURITY SCAN
       - Check for vulnerable packages (dotnet list package --vulnerable)
       - Fail or warn based on $failOnVulnerabilities setting

    4. BUILD & TEST PHASE
       - Clean previous builds (delete bin/obj folders)
       - Restore NuGet packages
       - Windows: Build main project -> Build test project -> Run tests with coverage
       - Analyze code coverage (fail if below threshold when configured)
       - Linux (Docker): Build main project -> Build test project -> Run tests (TRX report)
       - Rebuild for Windows (Docker may overwrite bin/obj)
       - Create NuGet package (.nupkg) and symbols (.snupkg)
       - All steps are timed for performance tracking

    5. CONFIRMATION PHASE
       - Display release summary
       - If -DryRun: Show summary and exit (no changes made)
       - Prompt user for confirmation before proceeding

    6. NUGET RELEASE PHASE (Idempotent)
       - Skip if version already exists on NuGet.org
       - Otherwise, push package to NuGet.org

    7. GITHUB RELEASE PHASE (Idempotent)
       - Skip if release already exists
       - Push tag to remote if not already there
       - Create GitHub release with:
         * Release notes from CHANGELOG.md
         * .nupkg and .snupkg as downloadable assets

    8. COMPLETION PHASE
       - Display timing summary for all steps
       - Display test results summary
       - Display success summary with links
       - Open NuGet and GitHub release pages in browser
       - TODO: Email notification (template provided)
       - TODO: Package signing (template provided)

.PARAMETER DryRun
    If specified, runs build and tests without publishing.
    - Bypasses branch check (warns instead)
    - No changes are made to NuGet, GitHub, or git tags

.USAGE
    Before running:
    1. Ensure Docker Desktop is running (for Linux tests)
    2. Update version in MaksIT.Core.csproj
    3. Run .\Generate-Changelog.ps1 to update CHANGELOG.md and LICENSE.md
    4. Review and commit all changes
    5. Create version tag: git tag v1.x.x
    6. Run: .\Release-NuGetPackage.ps1
    
    Note: The script finds the commit with the tag matching CHANGELOG.md version.
    You can run it from any branch/commit - it releases the tagged commit.

    Dry run (test without publishing):
        .\Release-NuGetPackage.ps1 -DryRun

    Re-run release (idempotent - skips NuGet/GitHub if already released):
        .\Release-NuGetPackage.ps1

    Generate changelog and update LICENSE year:
        .\Generate-Changelog.ps1
        .\Generate-Changelog.ps1 -DryRun

.CONFIGURATION
    All settings are stored in scriptsettings.json:
    - qualityGates: Coverage threshold, vulnerability checks
    - packageSigning: Code signing certificate configuration
    - emailNotification: SMTP settings for release notifications

.NOTES
    Author: Maksym Sadovnychyy (MAKS-IT)
    Repository: https://github.com/MAKS-IT-COM/maksit-core
#>

param(
    [switch]$DryRun
)

# ==============================================================================
# PATH CONFIGURATION
# ==============================================================================

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$solutionDir = Split-Path -Parent $scriptDir
$repoRoot = Split-Path -Parent $solutionDir
$projectDir = "$solutionDir\MaksIT.Core"
$outputDir = "$projectDir\bin\Release"
$testProjectDir = "$solutionDir\MaksIT.Core.Tests"
$csprojPath = "$projectDir\MaksIT.Core.csproj"
$testResultsDir = "$repoRoot\TestResults"

# ==============================================================================
# IMPORT MODULES
# ==============================================================================

# Import build utilities module
$buildUtilsPath = Join-Path $scriptDir "BuildUtils.psm1"
if (Test-Path $buildUtilsPath) {
    Import-Module $buildUtilsPath -Force
}
else {
    Write-Error "BuildUtils.psm1 not found at $buildUtilsPath"
    exit 1
}

# Initialize step timer
Initialize-StepTimer

# ==============================================================================
# CONFIGURATION
# ==============================================================================

if ($TestChangelog) {
    Write-Banner "TEST CHANGELOG MODE - AI generation only"
}
elseif ($DryRun) {
    Write-Banner "DRY RUN MODE - No changes will be made"
}

# NuGet source
$nugetSource = "https://api.nuget.org/v3/index.json"

# ==============================================================================
# LOAD SETTINGS FROM JSON
# ==============================================================================

$settingsPath = Join-Path $scriptDir "scriptsettings.json"
if (Test-Path $settingsPath) {
    $settings = Get-Content $settingsPath -Raw | ConvertFrom-Json
    Write-Host "Loaded settings from scriptsettings.json"
}
else {
    Write-Error "Settings file not found: $settingsPath"
    exit 1
}

# Resolve paths from settings (relative to script location)
$changelogPath = if ($settings.paths.changelogPath) {
    [System.IO.Path]::GetFullPath((Join-Path $scriptDir $settings.paths.changelogPath))
}
else {
    "$repoRoot\CHANGELOG.md"
}

# Release branch setting
$releaseBranch = if ($settings.release.branch) { $settings.release.branch } else { "main" }

# ==============================================================================
# SECRETS FROM ENVIRONMENT VARIABLES
# ==============================================================================

# Get env var names from settings (allows customization)
$envVars = $settings.environmentVariables

# NuGet API key
$nugetApiKey = [Environment]::GetEnvironmentVariable($envVars.nugetApiKey)
if (-not $nugetApiKey) {
    Write-Error "Error: API key not found in environment variable $($envVars.nugetApiKey)."
    exit 1
}

# GitHub token (set for gh CLI)
$env:GH_TOKEN = [Environment]::GetEnvironmentVariable($envVars.githubToken)

# Package signing password (optional)
$packageSigningCertPassword = [Environment]::GetEnvironmentVariable($envVars.signingCertPassword)

# SMTP password (optional)
$smtpPassword = [Environment]::GetEnvironmentVariable($envVars.smtpPassword)

# ==============================================================================
# NON-SECRET SETTINGS
# ==============================================================================

# Quality gates
$coverageThreshold = $settings.qualityGates.coverageThreshold
$failOnVulnerabilities = $settings.qualityGates.failOnVulnerabilities

# Package signing (non-secret parts)
$packageSigningEnabled = $settings.packageSigning.enabled
$packageSigningCertPath = $settings.packageSigning.certificatePath
$packageSigningTimestamper = $settings.packageSigning.timestampServer

# Email notification (non-secret parts)
$emailEnabled = $settings.emailNotification.enabled
$emailSmtpServer = $settings.emailNotification.smtpServer
$emailSmtpPort = $settings.emailNotification.smtpPort
$emailUseSsl = $settings.emailNotification.useSsl
$emailFrom = $settings.emailNotification.from
$emailTo = $settings.emailNotification.to

# ==============================================================================
# PREREQUISITE CHECKS
# ==============================================================================

Assert-Commands @("dotnet", "git", "gh", "docker")

# ==============================================================================
# GIT STATUS VALIDATION
# ==============================================================================

# Check for uncommitted changes (always block)
Write-Host "Checking for uncommitted changes..."
$gitStatus = Get-GitStatus

if (-not $gitStatus.IsClean) {
    $fileCount = $gitStatus.Staged.Count + $gitStatus.Modified.Count + $gitStatus.Untracked.Count + $gitStatus.Deleted.Count
    Write-Host "ERROR: You have $fileCount uncommitted file(s). Commit or stash them first." -ForegroundColor Red
    Show-GitStatus $gitStatus
    exit 1
}

Write-Host "Working directory is clean."

# ==============================================================================
# VERSION & TAG DISCOVERY
# ==============================================================================

# Read latest version from CHANGELOG.md
Write-Host "Reading version from CHANGELOG.md..."
if (-not (Test-Path $changelogPath)) {
    Write-Error "CHANGELOG.md not found at $changelogPath"
    exit 1
}

$changelogContent = Get-Content $changelogPath -Raw
$versionMatch = [regex]::Match($changelogContent, '##\s+v(\d+\.\d+\.\d+)')

if (-not $versionMatch.Success) {
    Write-Error "No version found in CHANGELOG.md (expected format: ## v1.2.3)"
    exit 1
}

$version = $versionMatch.Groups[1].Value
$tag = "v$version"
Write-Host "Latest changelog version: $version"

# Find commit with this tag
$tagCommit = git rev-parse "$tag^{commit}" 2>$null
if ($LASTEXITCODE -ne 0 -or -not $tagCommit) {
    Write-Host ""
    Write-Host "ERROR: Tag $tag not found." -ForegroundColor Red
    Write-Host "The release process requires a tag matching the changelog version." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "To fix, run:" -ForegroundColor Cyan
    Write-Host "  git tag $tag <commit-hash>" -ForegroundColor Cyan
    Write-Host "  git push origin $tag" -ForegroundColor Cyan
    exit 1
}

$shortCommit = $tagCommit.Substring(0, 7)
Write-Host "Found tag $tag -> commit $shortCommit"

# Validate tag commit is on release branch
if ($releaseBranch) {
    $branchContains = git branch --contains $tagCommit --list $releaseBranch 2>$null
    if (-not $branchContains) {
        Write-Host ""
        Write-Host "ERROR: Tag $tag (commit $shortCommit) is not on branch '$releaseBranch'." -ForegroundColor Red
        Write-Host "Release is only allowed from the configured branch." -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Either:" -ForegroundColor Cyan
        Write-Host "  1. Merge the tagged commit to '$releaseBranch'" -ForegroundColor Cyan
        Write-Host "  2. Change release.branch in scriptsettings.json" -ForegroundColor Cyan
        exit 1
    }
    Write-Host "Tag is on branch '$releaseBranch'" -ForegroundColor Green
}

# Extract target framework from csproj (needed for Docker image)
[xml]$csproj = Get-Content $csprojPath
$targetFramework = ($csproj.Project.PropertyGroup |
                    Where-Object { $_.TargetFramework } |
                    Select-Object -First 1).TargetFramework

if (-not $targetFramework) {
    # Try TargetFrameworks (plural) for multi-target projects, take first one
    $targetFrameworks = ($csproj.Project.PropertyGroup |
                         Where-Object { $_.TargetFrameworks } |
                         Select-Object -First 1).TargetFrameworks
    if ($targetFrameworks) {
        $targetFramework = ($targetFrameworks -split ';')[0]
    }
}

if (-not $targetFramework) {
    Write-Error "TargetFramework not found in $csprojPath"
    exit 1
}

# Convert "net8.0" to "8.0" for Docker image tag
$dotnetVersion = $targetFramework -replace '^net', ''
Write-Host "Target framework: $targetFramework (Docker SDK: $dotnetVersion)"

# ==============================================================================
# CHANGELOG VALIDATION
# ==============================================================================

$tag = "v$version"
$releaseName = "Release $version"

Start-Step "Validating CHANGELOG.md"

if (-not (Test-Path $changelogPath)) {
    Complete-Step "FAIL"
    Write-Error "CHANGELOG.md not found. Run .\Generate-Changelog.ps1 first."
    exit 1
}

$changelog = Get-Content $changelogPath -Raw
$pattern = "(?ms)^##\s+v$([regex]::Escape($version))\b.*?(?=^##\s+v\d+\.\d+|\Z)"
$match = [regex]::Match($changelog, $pattern)

if (-not $match.Success) {
    Complete-Step "FAIL"
    Write-Host ""
    Write-Host "No CHANGELOG entry for v$version" -ForegroundColor Red
    Write-Host "Run: .\Generate-Changelog.ps1" -ForegroundColor Yellow
    exit 1
}

$releaseNotes = $match.Value.Trim()
Complete-Step "OK"
Write-Host ""
Write-Host "Release notes (v$version):" -ForegroundColor Gray
Write-Host $releaseNotes

# ==============================================================================
# NUGET VERSION CHECK
# ==============================================================================

Start-Step "Checking NuGet.org release status"
$packageName = "MaksIT.Core"
$nugetCheckUrl = "https://api.nuget.org/v3-flatcontainer/$($packageName.ToLower())/index.json"
$script:nugetAlreadyReleased = $false

try {
    $existingVersions = (Invoke-RestMethod -Uri $nugetCheckUrl -ErrorAction Stop).versions
    if ($existingVersions -contains $version) {
        $script:nugetAlreadyReleased = $true
        Write-Host "  Version $version already on NuGet.org - will skip NuGet publish" -ForegroundColor Yellow
        Complete-Step "SKIP"
    }
    else {
        Write-Host "  Version $version not yet on NuGet.org - will release" -ForegroundColor Green
        Complete-Step "OK"
    }
}
catch {
    Write-Host "  Could not check NuGet (will attempt publish): $_" -ForegroundColor Yellow
    Complete-Step "SKIP"
}

# ==============================================================================
# GITHUB CONFIGURATION
# ==============================================================================

# Read GitHub settings from config
$gitHubConfig = $settings.gitHub
$gitHubEnabled = if ($null -ne $gitHubConfig.enabled) { $gitHubConfig.enabled } else { $true }
$gitHubRepo = $null

if ($gitHubEnabled) {
    # Get remote URL to check if it's GitHub or another host
    $remoteUrl = git config --get remote.origin.url
    if ($LASTEXITCODE -ne 0 -or -not $remoteUrl) {
        Write-Error "Could not determine git remote origin URL."
        exit 1
    }

    # Check if remote is GitHub (supports github.com in HTTPS or SSH format)
    $isGitHubRemote = $remoteUrl -match "github\.com[:/]"

    if ($isGitHubRemote) {
        # Auto-detect owner/repo from GitHub remote URL
        if ($remoteUrl -match "[:/](?<owner>[^/]+)/(?<repo>[^/.]+)(\.git)?$") {
            $owner = $matches['owner']
            $repoName = $matches['repo']
            $gitHubRepo = "$owner/$repoName"
            Write-Host "GitHub repository (auto-detected): $gitHubRepo"
        }
        else {
            Write-Error "Could not parse GitHub repo from remote URL: $remoteUrl"
            exit 1
        }
    }
    else {
        # Remote is not GitHub (e.g., Gitea, GitLab, etc.) - use fallback from config
        if ($gitHubConfig.repository -and $gitHubConfig.repository.Trim() -ne "") {
            $gitHubRepo = $gitHubConfig.repository.Trim()
            Write-Host "GitHub repository (from config, remote is not GitHub): $gitHubRepo"
        }
        else {
            Write-Error "Remote origin is not GitHub ($remoteUrl) and no fallback repository configured in scriptsettings.json (gitHub.repository)."
            exit 1
        }
    }

    # Ensure GH_TOKEN is set
    if (-not $env:GH_TOKEN) {
        Write-Error "GitHub token not found. Set environment variable: $($envVars.githubToken)"
        exit 1
    }

    # Test GitHub CLI authentication
    Write-Host "Authenticating GitHub CLI using GH_TOKEN..."
    $authTest = gh api user 2>$null

    if ($LASTEXITCODE -ne 0 -or -not $authTest) {
        Write-Error "GitHub CLI authentication failed. GH_TOKEN may be invalid or missing repo scope."
        exit 1
    }

    Write-Host "GitHub CLI authenticated successfully via GH_TOKEN."

    # Check if GitHub release already exists
    Start-Step "Checking GitHub release status"
    $script:githubAlreadyReleased = $false
    $existingGitHubRelease = gh release view $tag --repo $gitHubRepo 2>$null
    if ($LASTEXITCODE -eq 0 -and $existingGitHubRelease) {
        $script:githubAlreadyReleased = $true
        Write-Host "  GitHub release $tag already exists - will skip" -ForegroundColor Yellow
        Complete-Step "SKIP"
    }
    else {
        Write-Host "  GitHub release $tag not found - will create" -ForegroundColor Green
        Complete-Step "OK"
    }
}
else {
    Write-Host "GitHub releases: DISABLED (skipping GitHub authentication)" -ForegroundColor Yellow
    $script:githubAlreadyReleased = $false
}

# ==============================================================================
# BUILD & TEST PHASE
# ==============================================================================

Start-Step "Cleaning previous builds"
# Use direct folder deletion instead of dotnet clean (avoids package resolution issues)
$foldersToClean = @(
    "$projectDir\bin",
    "$projectDir\obj",
    "$testProjectDir\bin",
    "$testProjectDir\obj"
)
foreach ($folder in $foldersToClean) {
    if (Test-Path $folder) {
        Remove-Item -Path $folder -Recurse -Force -ErrorAction SilentlyContinue
        Write-Host "  Removed: $folder"
    }
}

Complete-Step "OK"

Start-Step "Restoring NuGet packages"
dotnet restore $solutionDir\MaksIT.Core.sln --nologo -v q

if ($LASTEXITCODE -ne 0) {
    Complete-Step "FAIL"
    Write-Error "NuGet restore failed. Check your internet connection or run 'dotnet nuget locals all --clear' and try again."
    exit 1
}

Complete-Step "OK"

# ==============================================================================
# SECURITY SCAN
# ==============================================================================

Start-Step "Scanning for vulnerable packages"
$vulnerabilityOutput = dotnet list $solutionDir\MaksIT.Core.sln package --vulnerable 2>&1 | Out-String

# Check if vulnerabilities were found
$hasVulnerabilities = $vulnerabilityOutput -match "has the following vulnerable packages"

if ($hasVulnerabilities) {
    Write-Host $vulnerabilityOutput -ForegroundColor Yellow
    if ($failOnVulnerabilities -and -not $DryRun) {
        Complete-Step "FAIL"
        Write-Error "Vulnerable packages detected. Update packages or set `$failOnVulnerabilities = `$false to bypass."
        exit 1
    }
    else {
        Write-Host "  WARNING: Vulnerable packages found (bypassed)" -ForegroundColor Yellow
        Complete-Step "WARN"
    }
}
else {
    Write-Host "  No known vulnerabilities found" -ForegroundColor Green
    Complete-Step "OK"
}

# ==============================================================================
# WINDOWS BUILD & TEST
# ==============================================================================

Start-Step "Building main project (Windows)"
dotnet build $projectDir -c Release --nologo -v q --no-restore

if ($LASTEXITCODE -ne 0) {
    Complete-Step "FAIL"
    Write-Error "Main project build failed."
    exit 1
}

Complete-Step "OK"

Start-Step "Building test project (Windows)"
dotnet build $testProjectDir -c Release --nologo -v q --no-restore

if ($LASTEXITCODE -ne 0) {
    Complete-Step "FAIL"
    Write-Error "Test project build failed."
    exit 1
}

Complete-Step "OK"

Start-Step "Running Windows tests with coverage"

# Create test results directory
if (-not (Test-Path $testResultsDir)) {
    New-Item -ItemType Directory -Path $testResultsDir -Force | Out-Null
}

# Run tests with TRX logger and coverage collection
$windowsTestResultFile = "$testResultsDir\Windows-TestResults.trx"
$testOutput = dotnet test $testProjectDir -c Release --nologo -v q --no-build `
    --logger "trx;LogFileName=$windowsTestResultFile" `
    --collect:"XPlat Code Coverage" `
    --results-directory "$testResultsDir" 2>&1 | Out-String

if ($LASTEXITCODE -ne 0) {
    Complete-Step "FAIL"
    Write-Host $testOutput
    Write-Error "Windows tests failed. Aborting release process."
    exit 1
}

# Parse test results
if ($testOutput -match "Passed:\s*(\d+)") { $script:windowsTestsPassed = [int]$Matches[1] } else { $script:windowsTestsPassed = 0 }
if ($testOutput -match "Failed:\s*(\d+)") { $script:windowsTestsFailed = [int]$Matches[1] } else { $script:windowsTestsFailed = 0 }
if ($testOutput -match "Skipped:\s*(\d+)") { $script:windowsTestsSkipped = [int]$Matches[1] } else { $script:windowsTestsSkipped = 0 }
$script:windowsTestsTotal = $script:windowsTestsPassed + $script:windowsTestsFailed + $script:windowsTestsSkipped

Write-Host "  Tests: $script:windowsTestsPassed passed, $script:windowsTestsFailed failed, $script:windowsTestsSkipped skipped" -ForegroundColor Green
Write-Host "  Results: $windowsTestResultFile" -ForegroundColor Gray
Complete-Step "OK"

# ==============================================================================
# CODE COVERAGE CHECK
# ==============================================================================

Start-Step "Analyzing code coverage"

# Find the coverage file (Coverlet creates it in a GUID folder)
$coverageFile = Get-ChildItem -Path $testResultsDir -Filter "coverage.cobertura.xml" -Recurse | 
    Sort-Object LastWriteTime -Descending | 
    Select-Object -First 1

if ($coverageFile) {
    # Parse coverage from Cobertura XML
    [xml]$coverageXml = Get-Content $coverageFile.FullName
    $lineRate = [double]$coverageXml.coverage.'line-rate' * 100
    $branchRate = [double]$coverageXml.coverage.'branch-rate' * 100
    $script:codeCoverage = [math]::Round($lineRate, 2)
    $script:branchCoverage = [math]::Round($branchRate, 2)
    
    Write-Host "  Line coverage:   $script:codeCoverage%" -ForegroundColor $(if ($script:codeCoverage -ge $coverageThreshold) { "Green" } else { "Yellow" })
    Write-Host "  Branch coverage: $script:branchCoverage%" -ForegroundColor Gray
    Write-Host "  Report: $($coverageFile.FullName)" -ForegroundColor Gray
    
    # Check threshold
    if ($coverageThreshold -gt 0 -and $script:codeCoverage -lt $coverageThreshold) {
        Complete-Step "FAIL"
        Write-Error "Code coverage ($script:codeCoverage%) is below threshold ($coverageThreshold%)."
        exit 1
    }
    Complete-Step "OK"
}
else {
    Write-Host "  Coverage file not found (coverlet may not be installed)" -ForegroundColor Yellow
    $script:codeCoverage = 0
    $script:branchCoverage = 0
    Complete-Step "SKIP"
}

# ==============================================================================
# LINUX BUILD & TEST (Docker)
# ==============================================================================

Start-Step "Checking Docker availability"
docker info 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    Complete-Step "FAIL"
    Write-Error "Docker is not running. Start Docker Desktop and try again."
    exit 1
}

# Extract Docker version info
$dockerVersion = docker version --format '{{.Server.Version}}' 2>$null
$dockerOS = docker version --format '{{.Server.Os}}' 2>$null
Write-Host "  Docker: $dockerVersion ($dockerOS)"
Complete-Step "OK"

# Convert Windows path to Docker-compatible path
$dockerRepoPath = $repoRoot -replace '\\', '/' -replace '^([A-Za-z]):', '/$1'

# Build Docker image name from target framework
$dockerImage = "mcr.microsoft.com/dotnet/sdk:$dotnetVersion"

Start-Step "Building & testing in Linux ($dockerImage)"
# Build main project, then test project, then run tests with TRX logger - all in one container run
$linuxTestResultFile = "TestResults/Linux-TestResults.trx"
$dockerTestOutput = docker run --rm -v "${dockerRepoPath}:/src" -w /src $dockerImage `
    sh -c "dotnet build src/MaksIT.Core -c Release --nologo -v q && dotnet build src/MaksIT.Core.Tests -c Release --nologo -v q && dotnet test src/MaksIT.Core.Tests -c Release --nologo -v q --no-build --logger 'trx;LogFileName=/src/$linuxTestResultFile'" 2>&1 | Out-String

if ($LASTEXITCODE -ne 0) {
    Complete-Step "FAIL"
    Write-Host $dockerTestOutput
    Write-Error "Linux build/tests failed. Aborting release process."
    exit 1
}

# Parse Docker test results
if ($dockerTestOutput -match "Passed:\s*(\d+)") { $script:linuxTestsPassed = [int]$Matches[1] } else { $script:linuxTestsPassed = 0 }
if ($dockerTestOutput -match "Failed:\s*(\d+)") { $script:linuxTestsFailed = [int]$Matches[1] } else { $script:linuxTestsFailed = 0 }
if ($dockerTestOutput -match "Skipped:\s*(\d+)") { $script:linuxTestsSkipped = [int]$Matches[1] } else { $script:linuxTestsSkipped = 0 }
$script:linuxTestsTotal = $script:linuxTestsPassed + $script:linuxTestsFailed + $script:linuxTestsSkipped

Write-Host "  Tests: $script:linuxTestsPassed passed, $script:linuxTestsFailed failed, $script:linuxTestsSkipped skipped" -ForegroundColor Green
Complete-Step "OK"

# Clean up test results directory
if (Test-Path $testResultsDir) {
    Remove-Item -Path $testResultsDir -Recurse -Force -ErrorAction SilentlyContinue
}

# ==============================================================================
# PACK (rebuild for Windows after Docker overwrote bin/obj)
# ==============================================================================

Start-Step "Rebuilding for package (Windows)"
# Docker tests may have overwritten bin/obj with Linux artifacts, rebuild for Windows
dotnet build $projectDir -c Release --nologo -v q

if ($LASTEXITCODE -ne 0) {
    Complete-Step "FAIL"
    Write-Error "Rebuild for packaging failed."
    exit 1
}

Complete-Step "OK"

Start-Step "Creating NuGet package"
dotnet pack $projectDir -c Release --no-build --nologo -v q

if ($LASTEXITCODE -ne 0) {
    Complete-Step "FAIL"
    Write-Error "dotnet pack failed."
    exit 1
}

# Look for the .nupkg and .snupkg files
$packageFile = Get-ChildItem -Path $outputDir -Filter "*.nupkg" -Recurse | Sort-Object LastWriteTime -Descending | Select-Object -First 1
$symbolsFile = Get-ChildItem -Path $outputDir -Filter "*.snupkg" -Recurse | Sort-Object LastWriteTime -Descending | Select-Object -First 1

if (-not $packageFile) {
    Complete-Step "FAIL"
    Write-Error "Package creation failed. No .nupkg file found."
    exit 1
}

# Get package size
$packageSize = "{0:N2} KB" -f ($packageFile.Length / 1KB)
Write-Host "  Package: $($packageFile.Name) ($packageSize)"
if ($symbolsFile) {
    $symbolsSize = "{0:N2} KB" -f ($symbolsFile.Length / 1KB)
    Write-Host "  Symbols: $($symbolsFile.Name) ($symbolsSize)"
}

Complete-Step "OK"

# ==============================================================================
# DRY RUN SUMMARY / CONFIRMATION PROMPT
# ==============================================================================

if ($DryRun) {
    # Show timing summary
    Show-TimingSummary
    
    Write-Host ""
    Write-Host "=========================================="
    Write-Host "DRY RUN COMPLETE - v$version"
    Write-Host "=========================================="
    Write-Host ""
    Write-Host "Validation Results:"
    Write-Host "  [OK] Prerequisites (dotnet, git, gh, docker)"
    Write-Host "  [OK] Working directory clean" -ForegroundColor Green
    Write-Host "  [OK] Tag $tag on branch '$releaseBranch'" -ForegroundColor Green
    if ($gitHubEnabled) {
        Write-Host "  [OK] GitHub CLI authenticated" -ForegroundColor Green
    }
    else {
        Write-Host "  [--] GitHub releases disabled" -ForegroundColor DarkGray
    }
    if ($hasVulnerabilities) {
        Write-Host "  [WARN] Vulnerable packages found (review recommended)" -ForegroundColor Yellow
    }
    else {
        Write-Host "  [OK] No vulnerable packages" -ForegroundColor Green
    }
    Write-Host ""
    Write-Host "Build Information:"
    Write-Host "  Target framework: $targetFramework"
    Write-Host "  Package: $($packageFile.Name) ($packageSize)"
    Write-Host "  Release commit: $shortCommit (tag $tag)" -ForegroundColor Green
    Write-Host ""
    Write-Host "Test Results:"
    Write-Host "  Windows: $script:windowsTestsPassed passed, $script:windowsTestsFailed failed, $script:windowsTestsSkipped skipped" -ForegroundColor Green
    Write-Host "  Linux:   $script:linuxTestsPassed passed, $script:linuxTestsFailed failed, $script:linuxTestsSkipped skipped" -ForegroundColor Green
    $totalTests = $script:windowsTestsTotal + $script:linuxTestsTotal
    Write-Host "  Total:   $totalTests tests across 2 platforms" -ForegroundColor Cyan
    if ($script:codeCoverage -gt 0) {
        $coverageColor = if ($coverageThreshold -gt 0 -and $script:codeCoverage -ge $coverageThreshold) { "Green" } elseif ($coverageThreshold -gt 0) { "Yellow" } else { "Cyan" }
        Write-Host "  Coverage: $script:codeCoverage% line, $script:branchCoverage% branch" -ForegroundColor $coverageColor
        if ($coverageThreshold -gt 0) {
            Write-Host "  Threshold: $coverageThreshold% ($(if ($script:codeCoverage -ge $coverageThreshold) { 'PASSED' } else { 'FAILED' }))" -ForegroundColor $coverageColor
        }
    }
    Write-Host ""
    Write-Host "Pending Features:"
    if ($packageSigningEnabled -and $packageSigningCertPath -and (Test-Path $packageSigningCertPath)) {
        Write-Host "  [READY] Package Signing - Certificate configured" -ForegroundColor Green
    }
    else {
        Write-Host "  [TODO] Package Signing - Enable in scriptsettings.json" -ForegroundColor DarkGray
    }
    if ($emailEnabled -and $emailSmtpServer -and $emailFrom -and $emailTo) {
        Write-Host "  [READY] Email Notification - SMTP configured" -ForegroundColor Green
    }
    else {
        Write-Host "  [TODO] Email Notification - Enable in scriptsettings.json" -ForegroundColor DarkGray
    }
    if ($coverageThreshold -gt 0) {
        Write-Host "  [ACTIVE] Code Coverage - Threshold: $coverageThreshold%" -ForegroundColor Green
    }
    else {
        Write-Host "  [INFO] Code Coverage - Collected but no threshold set" -ForegroundColor DarkGray
    }
    Write-Host ""
    Write-Host "If this were a real release, it would:"
    $itemNum = 1
    if (-not $script:nugetAlreadyReleased) {
        Write-Host "  $itemNum. Push $($packageFile.Name) to NuGet.org"
        $itemNum++
    }
    if ($gitHubEnabled -and -not $script:githubAlreadyReleased) {
        Write-Host "  $itemNum. Push tag $tag to remote (if not there)"
        $itemNum++
        Write-Host "  $itemNum. Create GitHub release with assets"
    }
    
    # Show what would be skipped
    if ($script:nugetAlreadyReleased -or ($gitHubEnabled -and $script:githubAlreadyReleased)) {
        Write-Host ""
        Write-Host "Would be skipped (already released):" -ForegroundColor DarkGray
        if ($script:nugetAlreadyReleased) {
            Write-Host "  - NuGet.org (version already exists)" -ForegroundColor DarkGray
        }
        if ($gitHubEnabled -and $script:githubAlreadyReleased) {
            Write-Host "  - GitHub (release already exists)" -ForegroundColor DarkGray
        }
    }
    
    Write-Host ""
    Write-Host "Run without -DryRun to perform the actual release." -ForegroundColor Green
    exit 0
}

# Check if there's anything to do
$hasWorkToDo = $false
$workItems = @()

if (-not $script:nugetAlreadyReleased) {
    $hasWorkToDo = $true
    $workItems += "Push package to NuGet.org"
}

if ($gitHubEnabled -and -not $script:githubAlreadyReleased) {
    $hasWorkToDo = $true
    $workItems += "Create GitHub release with tag v$version"
}

if (-not $hasWorkToDo) {
    Write-Host ""
    Write-Host "=========================================="
    Write-Host "NOTHING TO RELEASE" -ForegroundColor Yellow
    Write-Host "=========================================="
    Write-Host ""
    Write-Host "Version $version is already released on:" -ForegroundColor Yellow
    if ($script:nugetAlreadyReleased) {
        Write-Host "  - NuGet.org" -ForegroundColor Green
    }
    if ($gitHubEnabled -and $script:githubAlreadyReleased) {
        Write-Host "  - GitHub" -ForegroundColor Green
    }
    Write-Host ""
    Write-Host "To release a new version:" -ForegroundColor Gray
    Write-Host "  1. Update version in csproj" -ForegroundColor Gray
    Write-Host "  2. Run Generate-Changelog.ps1" -ForegroundColor Gray
    Write-Host "  3. Commit and tag: git tag v{new-version}" -ForegroundColor Gray
    Write-Host "  4. Run this script again" -ForegroundColor Gray
    exit 0
}

Write-Host ""
Write-Host "=========================================="
Write-Host "Ready to release v$version"
Write-Host "=========================================="
Write-Host "This will:"
$itemNum = 1
foreach ($item in $workItems) {
    Write-Host "  $itemNum. $item"
    $itemNum++
}

# Show what will be skipped
if ($script:nugetAlreadyReleased -or ($gitHubEnabled -and $script:githubAlreadyReleased)) {
    Write-Host ""
    Write-Host "Skipping (already released):" -ForegroundColor DarkGray
    if ($script:nugetAlreadyReleased) {
        Write-Host "  - NuGet.org (version already exists)" -ForegroundColor DarkGray
    }
    if ($gitHubEnabled -and $script:githubAlreadyReleased) {
        Write-Host "  - GitHub (release already exists)" -ForegroundColor DarkGray
    }
}

Write-Host ""
$confirm = Read-Host "Proceed with release? (y/n)"
if ($confirm -ne 'y' -and $confirm -ne 'Y') {
    Write-Host "Release cancelled."
    exit 0
}

# ==============================================================================
# NUGET PUBLISH
# ==============================================================================

if ($script:nugetAlreadyReleased) {
    Write-Host ""
    Write-Host "Skipping NuGet publish (version $version already exists)" -ForegroundColor Yellow
}
else {
    Start-Step "Pushing to NuGet.org"
    dotnet nuget push $packageFile.FullName -k $nugetApiKey -s $nugetSource --skip-duplicate

    if ($LASTEXITCODE -ne 0) {
        Complete-Step "FAIL"
        Write-Error "Failed to push the package to NuGet."
        exit 1
    }

    Complete-Step "OK"
}

# ==============================================================================
# GITHUB RELEASE
# ==============================================================================

if ($gitHubEnabled) {
    if ($script:githubAlreadyReleased) {
        Write-Host ""
        Write-Host "Skipping GitHub release (release $tag already exists)" -ForegroundColor Yellow
    }
    else {
        Start-Step "Creating GitHub release"
        Write-Host "  Tag: $tag -> $shortCommit"

        # Push tag to remote if not already there
        $remoteTag = git ls-remote --tags origin $tag 2>$null
        if (-not $remoteTag) {
            Write-Host "  Pushing tag to remote..."
            git push origin $tag
            if ($LASTEXITCODE -ne 0) {
                Write-Error "Failed to push git tag."
                exit 1
            }
        }
        else {
            Write-Host "  Tag already on remote"
        }

        # Build release assets list
        $releaseAssets = @($packageFile.FullName)
        if ($symbolsFile) {
            $releaseAssets += $symbolsFile.FullName
        }

        # Create GitHub release with assets
        Write-Host "Creating GitHub release: $releaseName"

        gh release create $tag @releaseAssets `
            --repo $gitHubRepo `
            --title "$releaseName" `
            --notes "$releaseNotes"

        if ($LASTEXITCODE -ne 0) {
            Complete-Step "FAIL"
            Write-Error "Failed to create GitHub release for tag $tag."
            exit 1
        }

        Complete-Step "OK"
    }
}
else {
    Write-Host ""
    Write-Host "Skipping GitHub release (disabled in settings)" -ForegroundColor Yellow
}

# ==============================================================================
# COMPLETION
# ==============================================================================

# Show timing summary
Show-TimingSummary

Write-Host ""
Write-Host "=========================================="
Write-Host "RELEASE COMPLETED SUCCESSFULLY!" -ForegroundColor Green
Write-Host "=========================================="
Write-Host ""
Write-Host "Version: $version"
Write-Host "Package: $($packageFile.Name) ($packageSize)"
Write-Host ""
Write-Host "Test Results:"
Write-Host "  Windows: $script:windowsTestsPassed passed" -ForegroundColor Green
Write-Host "  Linux:   $script:linuxTestsPassed passed" -ForegroundColor Green
if ($script:codeCoverage -gt 0) {
    Write-Host "  Coverage: $script:codeCoverage% line, $script:branchCoverage% branch" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "Links:"
Write-Host "  NuGet:  https://www.nuget.org/packages/MaksIT.Core/$version"
if ($gitHubEnabled) {
    Write-Host "  GitHub: https://github.com/$gitHubRepo/releases/tag/$tag"
}
Write-Host ""

# ==============================================================================
# PACKAGE SIGNING (TODO)
# ==============================================================================

if ($packageSigningEnabled -and $packageSigningCertPath -and (Test-Path $packageSigningCertPath)) {
    Start-Step "Signing NuGet package"
    try {
        $signArgs = @(
            "nuget", "sign", $packageFile.FullName,
            "--certificate-path", $packageSigningCertPath,
            "--timestamper", $packageSigningTimestamper
        )
        if ($packageSigningCertPassword) {
            $signArgs += "--certificate-password"
            $signArgs += $packageSigningCertPassword
        }
        & dotnet @signArgs
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  Package signed successfully" -ForegroundColor Green
            Complete-Step "OK"
        }
        else {
            Write-Host "  Package signing failed (continuing without signature)" -ForegroundColor Yellow
            Complete-Step "WARN"
        }
    }
    catch {
        Write-Host "  Package signing error: $_" -ForegroundColor Yellow
        Complete-Step "WARN"
    }
}
else {
    Write-Host ""
    Write-Host "[TODO] Package Signing - Not configured" -ForegroundColor DarkGray
    Write-Host "       Set packageSigning.enabled = true in scriptsettings.json" -ForegroundColor DarkGray
}

# ==============================================================================
# EMAIL NOTIFICATION (TODO)
# ==============================================================================

if ($emailEnabled -and $emailSmtpServer -and $emailFrom -and $emailTo) {
    Start-Step "Sending email notification"
    try {
        $gitHubLine = if ($gitHubEnabled) { "GitHub: https://github.com/$gitHubRepo/releases/tag/$tag`n" } else { "" }
        $emailBody = @"
MaksIT.Core Release $version completed successfully.

Package: $($packageFile.Name)
NuGet: https://www.nuget.org/packages/MaksIT.Core/$version
$gitHubLine
Test Results:
- Windows: $script:windowsTestsPassed passed
- Linux: $script:linuxTestsPassed passed
"@
        $emailParams = @{
            From = $emailFrom
            To = $emailTo
            Subject = "MaksIT.Core v$version Released"
            Body = $emailBody
            SmtpServer = $emailSmtpServer
            Port = $emailSmtpPort
            UseSsl = $emailUseSsl
        }
        
        # Add credentials if SMTP password is set
        if ($smtpPassword) {
            $securePassword = ConvertTo-SecureString $smtpPassword -AsPlainText -Force
            $credential = New-Object System.Management.Automation.PSCredential($emailFrom, $securePassword)
            $emailParams.Credential = $credential
        }
        
        Send-MailMessage @emailParams
        Write-Host "  Email sent to $emailTo" -ForegroundColor Green
        Complete-Step "OK"
    }
    catch {
        Write-Host "  Email sending failed: $_" -ForegroundColor Yellow
        Complete-Step "WARN"
    }
}
else {
    Write-Host ""
    Write-Host "[TODO] Email Notification - Not configured" -ForegroundColor DarkGray
    Write-Host "       Set emailNotification.enabled = true in scriptsettings.json" -ForegroundColor DarkGray
}

# ==============================================================================
# CODE COVERAGE STATUS
# ==============================================================================

Write-Host ""
if ($script:codeCoverage -gt 0) {
    if ($coverageThreshold -gt 0) {
        Write-Host "[ACTIVE] Code Coverage: $script:codeCoverage% (threshold: $coverageThreshold%)" -ForegroundColor Green
    }
    else {
        Write-Host "[INFO] Code Coverage: $script:codeCoverage% (no threshold enforced)" -ForegroundColor Cyan
        Write-Host "       Set `$coverageThreshold > 0 to enforce minimum coverage" -ForegroundColor DarkGray
    }
}
else {
    Write-Host "[SKIP] Code Coverage - Not collected" -ForegroundColor DarkGray
    Write-Host "       Ensure coverlet.collector is installed in test project" -ForegroundColor DarkGray
}

Write-Host ""

# Open release pages in browser
Write-Host "Opening release pages in browser..."
Start-Process "https://www.nuget.org/packages/MaksIT.Core/$version"
if ($gitHubEnabled) {
    Start-Process "https://github.com/$gitHubRepo/releases/tag/$tag"
}
