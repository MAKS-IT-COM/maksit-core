<#
.SYNOPSIS
    Amends the latest commit, recreates its associated tag, and force pushes both to remote.

.DESCRIPTION
    This script performs the following operations:
    1. Gets the last commit and verifies it has an associated tag
    2. Stages all pending changes
    3. Amends the latest commit (keeps existing message)
    4. Deletes and recreates the tag on the amended commit
    5. Force pushes the branch and tag to origin

.PARAMETER DryRun
    If specified, shows what would be done without making changes.

.EXAMPLE
    .\Force-AmendTaggedCommit.ps1
    
.EXAMPLE
    .\Force-AmendTaggedCommit.ps1 -DryRun
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

function Write-Step {
    param([string]$Text)
    Write-Host "`n>> $Text" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Text)
    Write-Host "   $Text" -ForegroundColor Green
}

function Write-Info {
    param([string]$Text)
    Write-Host "   $Text" -ForegroundColor Gray
}

function Write-Warn {
    param([string]$Text)
    Write-Host "   $Text" -ForegroundColor Yellow
}

function Invoke-Git {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments,
        
        [Parameter(Mandatory = $false)]
        [switch]$CaptureOutput,
        
        [Parameter(Mandatory = $false)]
        [string]$ErrorMessage = "Git command failed"
    )
    
    if ($CaptureOutput) {
        $output = & git @Arguments 2>&1
        $exitCode = $LASTEXITCODE
        if ($exitCode -ne 0) {
            throw "$ErrorMessage (exit code: $exitCode)"
        }
        return $output
    } else {
        & git @Arguments
        $exitCode = $LASTEXITCODE
        if ($exitCode -ne 0) {
            throw "$ErrorMessage (exit code: $exitCode)"
        }
    }
}

try {
    Write-Host "`n========================================" -ForegroundColor Magenta
    Write-Host "  Force Amend Tagged Commit Script" -ForegroundColor Magenta
    Write-Host "========================================`n" -ForegroundColor Magenta

    if ($DryRun) {
        Write-Warn "*** DRY RUN MODE - No changes will be made ***`n"
    }

    # Get current branch
    Write-Step "Getting current branch..."
    $Branch = Invoke-Git -Arguments @("rev-parse", "--abbrev-ref", "HEAD") -CaptureOutput -ErrorMessage "Failed to get current branch"
    Write-Info "Branch: $Branch"

    # Get last commit info
    Write-Step "Getting last commit..."
    $null = Invoke-Git -Arguments @("rev-parse", "HEAD") -CaptureOutput -ErrorMessage "Failed to get HEAD commit"
    $CommitMessage = Invoke-Git -Arguments @("log", "-1", "--format=%s") -CaptureOutput
    $CommitHash = Invoke-Git -Arguments @("log", "-1", "--format=%h") -CaptureOutput
    Write-Info "Commit: $CommitHash - $CommitMessage"

    # Find tag pointing to HEAD
    Write-Step "Finding tag on last commit..."
    $Tags = & git tag --points-at HEAD 2>&1
    
    if (-not $Tags -or [string]::IsNullOrWhiteSpace("$Tags")) {
        throw "No tag found on the last commit ($CommitHash). This script requires the last commit to have an associated tag."
    }

    # If multiple tags, use the first one
    $TagName = ("$Tags" -split "`n")[0].Trim()
    Write-Success "Found tag: $TagName"

    # Show current status
    Write-Step "Checking pending changes..."
    $Status = & git status --short 2>&1
    if ($Status -and -not [string]::IsNullOrWhiteSpace("$Status")) {
        Write-Info "Pending changes:"
        "$Status" -split "`n" | ForEach-Object { Write-Info "  $_" }
    } else {
        Write-Warn "No pending changes found"
        $confirm = Read-Host "`n   No changes to amend. Continue to recreate tag and force push? (y/N)"
        if ($confirm -ne 'y' -and $confirm -ne 'Y') {
            Write-Host "`nAborted by user" -ForegroundColor Yellow
            exit 0
        }
    }

    # Confirm operation
    Write-Host "`n----------------------------------------" -ForegroundColor White
    Write-Host "  Summary of operations:" -ForegroundColor White
    Write-Host "----------------------------------------" -ForegroundColor White
    Write-Host "  Branch: $Branch" -ForegroundColor White
    Write-Host "  Commit: $CommitHash" -ForegroundColor White
    Write-Host "  Tag:    $TagName" -ForegroundColor White
    Write-Host "  Remote: origin" -ForegroundColor White
    Write-Host "----------------------------------------`n" -ForegroundColor White

    if (-not $DryRun) {
        $confirm = Read-Host "   Proceed with amend and force push? (y/N)"
        if ($confirm -ne 'y' -and $confirm -ne 'Y') {
            Write-Host "`nAborted by user" -ForegroundColor Yellow
            exit 0
        }
    }

    # Stage all changes
    Write-Step "Staging all changes..."
    if (-not $DryRun) {
        Invoke-Git -Arguments @("add", "-A") -ErrorMessage "Failed to stage changes"
    }
    Write-Success "All changes staged"

    # Amend commit
    Write-Step "Amending commit..."
    if (-not $DryRun) {
        Invoke-Git -Arguments @("commit", "--amend", "--no-edit") -ErrorMessage "Failed to amend commit"
    }
    Write-Success "Commit amended"

    # Delete local tag
    Write-Step "Deleting local tag '$TagName'..."
    if (-not $DryRun) {
        Invoke-Git -Arguments @("tag", "-d", $TagName) -ErrorMessage "Failed to delete local tag"
    }
    Write-Success "Local tag deleted"

    # Recreate tag on new commit
    Write-Step "Recreating tag '$TagName' on amended commit..."
    if (-not $DryRun) {
        Invoke-Git -Arguments @("tag", $TagName) -ErrorMessage "Failed to create tag"
    }
    Write-Success "Tag recreated"

    # Force push branch
    Write-Step "Force pushing branch '$Branch' to origin..."
    if (-not $DryRun) {
        Invoke-Git -Arguments @("push", "--force", "origin", $Branch) -ErrorMessage "Failed to force push branch"
    }
    Write-Success "Branch force pushed"

    # Force push tag
    Write-Step "Force pushing tag '$TagName' to origin..."
    if (-not $DryRun) {
        Invoke-Git -Arguments @("push", "--force", "origin", $TagName) -ErrorMessage "Failed to force push tag"
    }
    Write-Success "Tag force pushed"

    Write-Host "`n========================================" -ForegroundColor Green
    Write-Host "  Operation completed successfully!" -ForegroundColor Green
    Write-Host "========================================`n" -ForegroundColor Green

    # Show final state
    Write-Host "Final state:" -ForegroundColor White
    & git log -1 --oneline
    Write-Host ""

} catch {
    Write-Host "`n========================================" -ForegroundColor Red
    Write-Host "  ERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "========================================`n" -ForegroundColor Red
    exit 1
}
