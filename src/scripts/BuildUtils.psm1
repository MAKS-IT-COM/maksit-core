<#
.SYNOPSIS
    Build utilities module for PowerShell scripts.

.DESCRIPTION
    Provides reusable functions for build/release scripts:
    - Step timing and progress tracking
    - Prerequisite/command validation
    - Git status utilities
    - Console output helpers

.USAGE
    Import-Module .\BuildUtils.psm1
    
    Initialize-StepTimer
    Start-Step "Building project"
    # ... do work ...
    Complete-Step "OK"
    Show-TimingSummary
#>

# ==============================================================================
# MODULE STATE
# ==============================================================================

$script:StepTimerState = @{
    TotalStopwatch = $null
    CurrentStep = $null
    StepTimings = @()
}

# ==============================================================================
# STEP TIMING FUNCTIONS
# ==============================================================================

function Initialize-StepTimer {
    <#
    .SYNOPSIS
        Initialize the step timer. Call at the start of your script.
    #>
    $script:StepTimerState.TotalStopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    $script:StepTimerState.StepTimings = @()
    $script:StepTimerState.CurrentStep = $null
}

function Start-Step {
    <#
    .SYNOPSIS
        Start timing a new step with console output.
    .PARAMETER Name
        Name/description of the step.
    #>
    param([Parameter(Mandatory)][string]$Name)
    
    $script:StepTimerState.CurrentStep = @{
        Name = $Name
        Stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    }
    Write-Host ""
    Write-Host "[$([DateTime]::Now.ToString('HH:mm:ss'))] $Name..." -ForegroundColor Cyan
}

function Complete-Step {
    <#
    .SYNOPSIS
        Complete the current step and record timing.
    .PARAMETER Status
        Status of the step: OK, SKIP, FAIL, WARN
    #>
    param([string]$Status = "OK")
    
    $step = $script:StepTimerState.CurrentStep
    if ($step) {
        $step.Stopwatch.Stop()
        $elapsed = $step.Stopwatch.Elapsed
        $script:StepTimerState.StepTimings += @{
            Name = $step.Name
            Duration = $elapsed
            Status = $Status
        }
        
        $timeStr = "{0:mm\:ss\.fff}" -f $elapsed
        $color = switch ($Status) {
            "OK"   { "Green" }
            "SKIP" { "Yellow" }
            "WARN" { "Yellow" }
            default { "Red" }
        }
        
        if ($Status -eq "SKIP") {
            Write-Host "  Skipped" -ForegroundColor $color
        }
        else {
            $prefix = if ($Status -eq "OK") { "Completed" } else { "Failed" }
            Write-Host "  $prefix in $timeStr" -ForegroundColor $color
        }
    }
}

function Get-StepTimings {
    <#
    .SYNOPSIS
        Get the recorded step timings.
    .OUTPUTS
        Array of step timing objects.
    #>
    return $script:StepTimerState.StepTimings
}

function Show-TimingSummary {
    <#
    .SYNOPSIS
        Display a summary of all step timings.
    #>
    Write-Host ""
    Write-Host "=========================================="
    Write-Host "TIMING SUMMARY"
    Write-Host "=========================================="
    
    foreach ($step in $script:StepTimerState.StepTimings) {
        $timeStr = "{0:mm\:ss\.fff}" -f $step.Duration
        $status = $step.Status
        $color = switch ($status) {
            "OK"   { "Green" }
            "SKIP" { "Yellow" }
            "WARN" { "Yellow" }
            default { "Red" }
        }
        Write-Host ("  [{0,-4}] {1,-40} {2}" -f $status, $step.Name, $timeStr) -ForegroundColor $color
    }
    
    if ($script:StepTimerState.TotalStopwatch) {
        $script:StepTimerState.TotalStopwatch.Stop()
        $totalTime = "{0:mm\:ss\.fff}" -f $script:StepTimerState.TotalStopwatch.Elapsed
        Write-Host "----------------------------------------"
        Write-Host "  Total: $totalTime" -ForegroundColor Cyan
    }
}

# ==============================================================================
# PREREQUISITE FUNCTIONS
# ==============================================================================

function Test-CommandExists {
    <#
    .SYNOPSIS
        Check if a command exists.
    .PARAMETER Command
        Command name to check.
    .OUTPUTS
        Boolean indicating if command exists.
    #>
    param([Parameter(Mandatory)][string]$Command)
    return [bool](Get-Command $Command -ErrorAction SilentlyContinue)
}

function Assert-Command {
    <#
    .SYNOPSIS
        Assert that a command exists, exit if not.
    .PARAMETER Command
        Command name to check.
    .PARAMETER ExitCode
        Exit code to use if command is missing (default: 1).
    #>
    param(
        [Parameter(Mandatory)][string]$Command,
        [int]$ExitCode = 1
    )
    
    if (-not (Test-CommandExists $Command)) {
        Write-Error "Required command '$Command' is not available. Aborting."
        exit $ExitCode
    }
}

function Assert-Commands {
    <#
    .SYNOPSIS
        Assert that multiple commands exist.
    .PARAMETER Commands
        Array of command names to check.
    #>
    param([Parameter(Mandatory)][string[]]$Commands)
    
    foreach ($cmd in $Commands) {
        Assert-Command $cmd
    }
}

function Test-EnvironmentVariable {
    <#
    .SYNOPSIS
        Check if an environment variable is set.
    .PARAMETER Name
        Environment variable name.
    .OUTPUTS
        Boolean indicating if variable is set and not empty.
    #>
    param([Parameter(Mandatory)][string]$Name)
    
    $value = [Environment]::GetEnvironmentVariable($Name)
    return -not [string]::IsNullOrWhiteSpace($value)
}

function Assert-EnvironmentVariable {
    <#
    .SYNOPSIS
        Assert that an environment variable is set, exit if not.
    .PARAMETER Name
        Environment variable name.
    .PARAMETER ExitCode
        Exit code to use if variable is missing (default: 1).
    #>
    param(
        [Parameter(Mandatory)][string]$Name,
        [int]$ExitCode = 1
    )
    
    if (-not (Test-EnvironmentVariable $Name)) {
        Write-Error "Required environment variable '$Name' is not set. Aborting."
        exit $ExitCode
    }
}

# ==============================================================================
# GIT UTILITIES
# ==============================================================================

function Get-GitStatus {
    <#
    .SYNOPSIS
        Get git status as structured object.
    .OUTPUTS
        Object with Staged, Modified, Untracked arrays and IsClean boolean.
    #>
    $status = @{
        Staged = @()
        Modified = @()
        Untracked = @()
        Deleted = @()
        IsClean = $true
    }
    
    $statusLines = git status --porcelain 2>$null
    if (-not $statusLines) { return $status }
    
    $status.IsClean = $false
    
    foreach ($line in ($statusLines -split "`n")) {
        if ([string]::IsNullOrWhiteSpace($line)) { continue }
        
        $index = $line.Substring(0, 1)
        $workTree = $line.Substring(1, 1)
        $file = $line.Substring(3)
        
        # Staged changes
        if ($index -match '[MADRC]') {
            $status.Staged += $file
        }
        # Unstaged modifications
        if ($workTree -eq 'M') {
            $status.Modified += $file
        }
        # Deleted files
        if ($index -eq 'D' -or $workTree -eq 'D') {
            $status.Deleted += $file
        }
        # Untracked files
        if ($index -eq '?' -and $workTree -eq '?') {
            $status.Untracked += $file
        }
    }
    
    return $status
}

function Show-GitStatus {
    <#
    .SYNOPSIS
        Display git status in a formatted, colored output.
    .PARAMETER Status
        Git status object from Get-GitStatus (optional, will fetch if not provided).
    #>
    param([hashtable]$Status)
    
    if (-not $Status) {
        $Status = Get-GitStatus
    }
    
    if ($Status.IsClean) {
        Write-Host "  Working directory is clean" -ForegroundColor Green
        return
    }
    
    if ($Status.Staged.Count -gt 0) {
        Write-Host "  Staged ($($Status.Staged.Count)):" -ForegroundColor Green
        $Status.Staged | ForEach-Object { Write-Host "    + $_" -ForegroundColor Green }
    }
    if ($Status.Modified.Count -gt 0) {
        Write-Host "  Modified ($($Status.Modified.Count)):" -ForegroundColor Yellow
        $Status.Modified | ForEach-Object { Write-Host "    M $_" -ForegroundColor Yellow }
    }
    if ($Status.Deleted.Count -gt 0) {
        Write-Host "  Deleted ($($Status.Deleted.Count)):" -ForegroundColor Red
        $Status.Deleted | ForEach-Object { Write-Host "    D $_" -ForegroundColor Red }
    }
    if ($Status.Untracked.Count -gt 0) {
        Write-Host "  Untracked ($($Status.Untracked.Count)):" -ForegroundColor Cyan
        $Status.Untracked | ForEach-Object { Write-Host "    ? $_" -ForegroundColor Cyan }
    }
}

function Get-CurrentBranch {
    <#
    .SYNOPSIS
        Get the current git branch name.
    .OUTPUTS
        Branch name string or $null if not in a git repo.
    #>
    try {
        $branch = git rev-parse --abbrev-ref HEAD 2>$null
        if ($LASTEXITCODE -eq 0) { return $branch }
    } catch { }
    return $null
}

function Get-LastTag {
    <#
    .SYNOPSIS
        Get the most recent git tag.
    .OUTPUTS
        Tag name string or $null if no tags exist.
    #>
    try {
        $tag = git describe --tags --abbrev=0 2>$null
        if ($LASTEXITCODE -eq 0) { return $tag }
    } catch { }
    return $null
}

function Get-CommitsSinceTag {
    <#
    .SYNOPSIS
        Get commits since a specific tag (or all commits if no tag).
    .PARAMETER Tag
        Tag to start from (optional, uses last tag if not specified).
    .PARAMETER Format
        Output format: oneline, full, hash (default: oneline).
    .OUTPUTS
        Array of commit strings.
    #>
    param(
        [string]$Tag,
        [ValidateSet("oneline", "full", "hash")]
        [string]$Format = "oneline"
    )
    
    if (-not $Tag) {
        $Tag = Get-LastTag
    }
    
    $formatArg = switch ($Format) {
        "oneline" { "--oneline" }
        "full" { "--format=full" }
        "hash" { "--format=%H" }
    }
    
    try {
        if ($Tag) {
            $commits = git log "$Tag..HEAD" $formatArg --no-merges 2>$null
        }
        else {
            $commits = git log -50 $formatArg --no-merges 2>$null
        }
        
        if ($commits) {
            return $commits -split "`n" | Where-Object { $_.Trim() -ne "" }
        }
    } catch { }
    
    return @()
}

function Get-VersionBumpCommit {
    <#
    .SYNOPSIS
        Find the commit where a version string was introduced in a file.
    .PARAMETER Version
        Version string to search for (e.g., "1.6.1").
    .PARAMETER FilePath
        File path to search in (e.g., "*.csproj").
    .OUTPUTS
        Commit hash where version first appeared, or $null.
    #>
    param(
        [Parameter(Mandatory)][string]$Version,
        [string]$FilePath = "*.csproj"
    )
    
    try {
        # Find commit that introduced this version string
        $commit = git log -S "<Version>$Version</Version>" --format="%H" --reverse -- $FilePath 2>$null | Select-Object -First 1
        if ($commit) { return $commit.Trim() }
        
        # Try alternative format (without tags)
        $commit = git log -S "$Version" --format="%H" --reverse -- $FilePath 2>$null | Select-Object -First 1
        if ($commit) { return $commit.Trim() }
    } catch { }
    
    return $null
}

function Get-CommitsForVersion {
    <#
    .SYNOPSIS
        Get commits for a specific version (from previous version to HEAD).
        Designed for pre-commit workflow: version is bumped locally but not yet committed.
    .PARAMETER Version
        Version string (e.g., "1.6.1") - the NEW version being prepared.
    .PARAMETER CsprojPath
        Path to csproj file (absolute or relative).
    .PARAMETER Format
        Output format: oneline, full, hash, detailed (default: oneline).
        "detailed" includes commit message + changed files.
    .OUTPUTS
        Array of commit strings for this version.
    #>
    param(
        [Parameter(Mandatory)][string]$Version,
        [string]$CsprojPath = "*.csproj",
        [ValidateSet("oneline", "full", "hash", "detailed")]
        [string]$Format = "oneline"
    )
    
    # Get git repo root for path conversion
    $gitRoot = (git rev-parse --show-toplevel 2>$null)
    if ($gitRoot) { $gitRoot = $gitRoot.Trim() }
    
    # Get path relative to git root using git itself (handles drive letter issues)
    function ConvertTo-GitPath {
        param([string]$Path)
        if (-not $Path) { return $null }
        
        # If it's a relative path, use it directly
        if (-not [System.IO.Path]::IsPathRooted($Path)) {
            # Get path relative to repo root by combining with current dir offset
            $cwdRelative = git rev-parse --show-prefix 2>$null
            if ($cwdRelative) {
                $cwdRelative = $cwdRelative.Trim().TrimEnd('/')
                if ($cwdRelative) {
                    return "$cwdRelative/$($Path -replace '\\', '/')"
                }
            }
            return $Path -replace '\\', '/'
        }
        
        # For absolute paths, try to make relative using git
        Push-Location (Split-Path $Path -Parent) -ErrorAction SilentlyContinue
        try {
            $prefix = git rev-parse --show-prefix 2>$null
            if ($prefix) {
                $prefix = $prefix.Trim().TrimEnd('/')
                $filename = Split-Path $Path -Leaf
                if ($prefix) {
                    return "$prefix/$filename"
                }
                return $filename
            }
        }
        finally {
            Pop-Location -ErrorAction SilentlyContinue
        }
        
        # Fallback: normalize to forward slashes
        return $Path -replace '\\', '/'
    }
    
    # Find actual csproj file if glob pattern
    $actualCsprojPath = $CsprojPath
    if ($CsprojPath -match '\*') {
        $found = Get-ChildItem -Path $CsprojPath -Recurse -ErrorAction SilentlyContinue | 
            Where-Object { $_.Name -match '\.csproj$' } | 
            Select-Object -First 1
        if ($found) { $actualCsprojPath = $found.FullName }
    }
    
    $gitCsprojPath = ConvertTo-GitPath $actualCsprojPath
    
    # Determine commit range
    $range = ""
    try {
        # Check if this version is already committed
        $versionCommit = Get-VersionBumpCommit -Version $Version -FilePath $CsprojPath
        
        if ($versionCommit) {
            # Version already in git history - get commits from that point
            $range = "$versionCommit^..HEAD"
        }
        else {
            # Version NOT committed yet (normal pre-commit workflow)
            # Find the PREVIOUS version from the committed csproj
            if ($gitCsprojPath) {
                $committedContent = git show "HEAD:$gitCsprojPath" 2>$null
                if ($committedContent) {
                    $prevVersionMatch = [regex]::Match(($committedContent -join "`n"), '<Version>([^<]+)</Version>')
                    if ($prevVersionMatch.Success) {
                        $prevVersion = $prevVersionMatch.Groups[1].Value
                        # Find when previous version was introduced
                        $prevCommit = Get-VersionBumpCommit -Version $prevVersion -FilePath $CsprojPath
                        if ($prevCommit) {
                            # Get commits AFTER previous version was set (these are unreleased)
                            $range = "$prevCommit..HEAD"
                        }
                    }
                }
            }
            
            # Fallback to last tag if still no range
            if (-not $range) {
                $lastTag = Get-LastTag
                if ($lastTag) {
                    $range = "$lastTag..HEAD"
                }
            }
        }
    } catch { }
    
    
    # For detailed format, get commit + files changed
    if ($Format -eq "detailed") {
        return Get-DetailedCommits -Range $range
    }
    
    $formatArg = switch ($Format) {
        "oneline" { "--oneline" }
        "full" { "--format=full" }
        "hash" { "--format=%H" }
    }
    
    try {
        if ($range) {
            $commits = git log $range $formatArg --no-merges 2>$null
        }
        else {
            $commits = git log -30 $formatArg --no-merges 2>$null
        }
        
        if ($commits) {
            return $commits -split "`n" | Where-Object { $_.Trim() -ne "" }
        }
    } catch { }
    
    return @()
}

function Get-DetailedCommits {
    <#
    .SYNOPSIS
        Get detailed commit info including changed files.
    .PARAMETER Range
        Git commit range (e.g., "v1.0.0..HEAD").
    .PARAMETER MaxCommits
        Maximum commits to return (default: 50).
    .OUTPUTS
        Array of formatted strings: "hash message [files: file1, file2, ...]"
    #>
    param(
        [string]$Range,
        [int]$MaxCommits = 50
    )
    
    $results = @()
    
    try {
        # Get commit hashes
        if ($Range) {
            $hashes = git log $Range --format="%H" --no-merges -n $MaxCommits 2>$null
        }
        else {
            $hashes = git log --format="%H" --no-merges -n $MaxCommits 2>$null
        }
        
        if (-not $hashes) { return @() }
        
        $hashArray = $hashes -split "`n" | Where-Object { $_.Trim() -ne "" }
        
        foreach ($hash in $hashArray) {
            $hash = $hash.Trim()
            if (-not $hash) { continue }
            
            # Get commit message (first line)
            $message = git log -1 --format="%s" $hash 2>$null
            if (-not $message) { continue }
            
            # Get changed files (source files only)
            $files = git diff-tree --no-commit-id --name-only -r $hash 2>$null
            $sourceFiles = @()
            if ($files) {
                $sourceFiles = ($files -split "`n" | Where-Object { 
                    $_.Trim() -ne "" -and ($_ -match '\.(cs|fs|vb|ts|js|py|java|go|rs|cpp|c|h)$')
                }) | Select-Object -First 5  # Limit to 5 files per commit
            }
            
            # Format output
            $shortHash = $hash.Substring(0, 7)
            if ($sourceFiles.Count -gt 0) {
                $fileList = $sourceFiles -join ", "
                $results += "$shortHash $message [files: $fileList]"
            }
            else {
                $results += "$shortHash $message"
            }
        }
    } catch { }
    
    return $results
}

function Get-UncommittedChanges {
    <#
    .SYNOPSIS
        Get summary of uncommitted changes (staged, unstaged, untracked) with meaningful descriptions.
    .PARAMETER IncludeContent
        If true, includes file content diffs (for AI analysis).
    .PARAMETER FileFilter
        File extension filter (default: .cs for C# files).
    .OUTPUTS
        Object with Staged, Modified, Untracked arrays and Summary with change descriptions.
    #>
    param(
        [switch]$IncludeContent,
        [string]$FileFilter = ".cs"
    )
    
    $result = @{
        Staged = @()
        Modified = @()
        Untracked = @()
        Deleted = @()
        Summary = @()
    }
    
    try {
        # Get current directory prefix relative to repo root
        $cwdPrefix = git rev-parse --show-prefix 2>$null
        if ($cwdPrefix) { $cwdPrefix = $cwdPrefix.Trim().TrimEnd('/') }
        
        # Get all changes using git status porcelain
        $status = git status --porcelain 2>$null
        if (-not $status) { return $result }
        
        $statusLines = $status -split "`n" | Where-Object { $_.Trim() -ne "" }
        
        foreach ($line in $statusLines) {
            if ($line.Length -lt 3) { continue }
            $statusCode = $line.Substring(0, 2)
            $filePath = $line.Substring(3).Trim().Trim('"')
            
            # Filter by extension
            if ($FileFilter -and -not $filePath.EndsWith($FileFilter)) { continue }
            
            # Convert repo-relative path to cwd-relative path for git diff
            $diffPath = $filePath
            if ($cwdPrefix -and $filePath.StartsWith("$cwdPrefix/")) {
                $diffPath = $filePath.Substring($cwdPrefix.Length + 1)
            }
            
            # Categorize by status (store both paths)
            $pathInfo = @{ Full = $filePath; Diff = $diffPath }
            if ($statusCode -match '^\?\?') {
                $result.Untracked += $pathInfo
            } elseif ($statusCode -match '^D' -or $statusCode -match '^.D') {
                # Deleted files (staged or unstaged)
                $result.Deleted += $pathInfo
            } elseif ($statusCode -match '^[MARC]') {
                $result.Staged += $pathInfo
            } elseif ($statusCode -match '^.[MARC]') {
                $result.Modified += $pathInfo
            }
        }
        
        # Process modified/staged files (get diff)
        $allModified = @($result.Staged) + @($result.Modified)
        foreach ($fileInfo in $allModified) {
            if (-not $fileInfo) { continue }
            
            $diffPath = $fileInfo.Diff
            $fullPath = $fileInfo.Full
            
            $diff = git diff -- $diffPath 2>$null
            if (-not $diff) { $diff = git diff --cached -- $diffPath 2>$null }
            
            $fileName = Split-Path $fullPath -Leaf
            $className = $fileName -replace '\.cs$', ''
            
            if ($diff) {
                $changes = @()
                $diffLines = $diff -split "`n"
                
                foreach ($line in $diffLines) {
                    # Added method/class/property
                    if ($line -match '^\+\s*(public|private|protected|internal)\s+static\s+\w+\s+(\w+)\s*\(') {
                        $changes += "Added static method $($Matches[2])"
                    }
                    elseif ($line -match '^\+\s*(public|private|protected|internal)\s+\w+\s+(\w+)\s*\([^)]*\)\s*\{?') {
                        $methodName = $Matches[2]
                        if ($methodName -notmatch '^(get|set|if|for|while|switch|new|return)$') {
                            $changes += "Added method $methodName"
                        }
                    }
                    elseif ($line -match '^\+\s*(public|private|protected|internal)\s+(class|interface|struct|enum)\s+(\w+)') {
                        $changes += "Added $($Matches[2]) $($Matches[3])"
                    }
                    # Detect try/catch blocks, error handling
                    elseif ($line -match '^\+.*catch\s*\(') {
                        $changes += "Added exception handling"
                    }
                }
                
                $changes = $changes | Select-Object -Unique | Select-Object -First 4
                
                if ($changes.Count -gt 0) {
                    $result.Summary += "(uncommitted) $className`: $($changes -join ', ')"
                }
                else {
                    # Fallback to line count
                    $addCount = ($diffLines | Where-Object { $_ -match '^\+[^+]' }).Count
                    $delCount = ($diffLines | Where-Object { $_ -match '^-[^-]' }).Count
                    $result.Summary += "(uncommitted) $className`: Modified (+$addCount/-$delCount lines)"
                }
            }
            else {
                $result.Summary += "(uncommitted) $className`: Modified"
            }
        }
        
        # Process untracked files (new files)
        foreach ($fileInfo in $result.Untracked) {
            if (-not $fileInfo) { continue }
            
            $diffPath = $fileInfo.Diff
            $fullPath = $fileInfo.Full
            
            $fileName = Split-Path $fullPath -Leaf
            $className = $fileName -replace '\.cs$', ''
            
            # Read file to understand what it contains
            $content = Get-Content $diffPath -Raw -ErrorAction SilentlyContinue
            if ($content) {
                $features = @()
                
                if ($content -match 'class\s+(\w+)') { $features += "class $($Matches[1])" }
                if ($content -match 'interface\s+(\w+)') { $features += "interface $($Matches[1])" }
                if ($content -match 'enum\s+(\w+)') { $features += "enum $($Matches[1])" }
                if ($content -match '\[Fact\]|\[Theory\]') { $features += "unit tests" }
                
                if ($features.Count -gt 0) {
                    $result.Summary += "(new file) $className`: Added $($features -join ', ')"
                }
                else {
                    $result.Summary += "(new file) $className`: New file"
                }
            }
            else {
                $result.Summary += "(new file) $className`: New file"
            }
        }
        
        # Process deleted files
        foreach ($fileInfo in $result.Deleted) {
            if (-not $fileInfo) { continue }
            
            $fullPath = $fileInfo.Full
            $fileName = Split-Path $fullPath -Leaf
            $className = $fileName -replace '\.cs$', ''
            
            $result.Summary += "(deleted) $className`: Removed"
        }
        
        if ($IncludeContent) {
            $result.DiffContent = git diff --cached 2>$null
            if (-not $result.DiffContent) { $result.DiffContent = git diff 2>$null }
        }
    } catch { }
    
    return $result
}

function Get-CommitChangesAnalysis {
    <#
    .SYNOPSIS
        Analyze commits in a range and extract meaningful changes from diffs.
    .PARAMETER Version
        Version string to find commits for.
    .PARAMETER CsprojPath
        Path to csproj file.
    .PARAMETER FileFilter
        File extension filter (default: .cs).
    .OUTPUTS
        Array of change summary strings (like Get-UncommittedChanges).
    #>
    param(
        [Parameter(Mandatory)][string]$Version,
        [string]$CsprojPath = "*.csproj",
        [string]$FileFilter = ".cs"
    )
    
    $summaries = @()
    
    try {
        # Get commit range for this version
        $range = ""
        
        # Find csproj
        $actualCsprojPath = $CsprojPath
        if ($CsprojPath -match '\*') {
            $found = Get-ChildItem -Path $CsprojPath -Recurse -ErrorAction SilentlyContinue | 
                Where-Object { $_.Name -match '\.csproj$' } | 
                Select-Object -First 1
            if ($found) { $actualCsprojPath = $found.FullName }
        }
        
        # Get git path
        $cwdPrefix = git rev-parse --show-prefix 2>$null
        if ($cwdPrefix) { $cwdPrefix = $cwdPrefix.Trim().TrimEnd('/') }
        
        $gitCsprojPath = $actualCsprojPath -replace '\\', '/'
        if ([System.IO.Path]::IsPathRooted($actualCsprojPath)) {
            Push-Location (Split-Path $actualCsprojPath -Parent) -ErrorAction SilentlyContinue
            try {
                $prefix = git rev-parse --show-prefix 2>$null
                if ($prefix) {
                    $prefix = $prefix.Trim().TrimEnd('/')
                    $filename = Split-Path $actualCsprojPath -Leaf
                    $gitCsprojPath = if ($prefix) { "$prefix/$filename" } else { $filename }
                }
            } finally { Pop-Location -ErrorAction SilentlyContinue }
        }
        
        # Determine commit range
        $versionCommit = Get-VersionBumpCommit -Version $Version -FilePath $CsprojPath
        
        if ($versionCommit) {
            $range = "$versionCommit^..HEAD"
        }
        else {
            # Version not committed - find previous version
            $committedContent = git show "HEAD:$gitCsprojPath" 2>$null
            if ($committedContent) {
                $prevVersionMatch = [regex]::Match(($committedContent -join "`n"), '<Version>([^<]+)</Version>')
                if ($prevVersionMatch.Success) {
                    $prevVersion = $prevVersionMatch.Groups[1].Value
                    $prevCommit = Get-VersionBumpCommit -Version $prevVersion -FilePath $CsprojPath
                    if ($prevCommit) {
                        $range = "$prevCommit..HEAD"
                    }
                }
            }
            
            if (-not $range) {
                $lastTag = Get-LastTag
                if ($lastTag) { $range = "$lastTag..HEAD" }
            }
        }
        
        if (-not $range) { return @() }
        
        # Get commits
        $hashes = git log $range --format="%H" --no-merges -n 30 2>$null
        if (-not $hashes) { return @() }
        
        $hashArray = $hashes -split "`n" | Where-Object { $_.Trim() -ne "" }
        
        foreach ($hash in $hashArray) {
            $hash = $hash.Trim()
            if (-not $hash) { continue }
            
            $message = git log -1 --format="%s" $hash 2>$null
            $shortHash = $hash.Substring(0, 7)
            
            # Get files changed in this commit
            $files = git diff-tree --no-commit-id --name-only -r $hash 2>$null
            if (-not $files) { continue }
            
            $sourceFiles = $files -split "`n" | Where-Object { 
                $_.Trim() -ne "" -and $_.EndsWith($FileFilter)
            }
            
            foreach ($file in $sourceFiles) {
                $file = $file.Trim()
                if (-not $file) { continue }
                
                $fileName = Split-Path $file -Leaf
                $className = $fileName -replace '\.cs$', ''
                
                # Get diff for this file in this commit
                $diff = git show $hash --format="" -- $file 2>$null
                
                if ($diff) {
                    $changes = @()
                    $diffLines = $diff -split "`n"
                    
                    foreach ($line in $diffLines) {
                        # Added method/class/property
                        if ($line -match '^\+\s*(public|private|protected|internal)\s+static\s+\w+\s+(\w+)\s*\(') {
                            $changes += "Added static method $($Matches[2])"
                        }
                        elseif ($line -match '^\+\s*(public|private|protected|internal)\s+\w+\s+(\w+)\s*\([^)]*\)\s*\{?') {
                            $methodName = $Matches[2]
                            if ($methodName -notmatch '^(get|set|if|for|while|switch|new|return)$') {
                                $changes += "Added method $methodName"
                            }
                        }
                        elseif ($line -match '^\+\s*(public|private|protected|internal)\s+(class|interface|struct|enum)\s+(\w+)') {
                            $changes += "Added $($Matches[2]) $($Matches[3])"
                        }
                        # Removed
                        elseif ($line -match '^-\s*(public|private|protected|internal)\s+(class|interface|struct|enum)\s+(\w+)') {
                            $changes += "Removed $($Matches[2]) $($Matches[3])"
                        }
                        elseif ($line -match '^-\s*(public|private|protected|internal)\s+\w+\s+(\w+)\s*\([^)]*\)\s*\{?') {
                            $methodName = $Matches[2]
                            if ($methodName -notmatch '^(get|set|if|for|while|switch|new|return)$') {
                                $changes += "Removed method $methodName"
                            }
                        }
                        # Exception handling
                        elseif ($line -match '^\+.*catch\s*\(') {
                            $changes += "Added exception handling"
                        }
                    }
                    
                    $changes = $changes | Select-Object -Unique | Select-Object -First 4
                    
                    if ($changes.Count -gt 0) {
                        $summaries += "(commit $shortHash) $className`: $($changes -join ', ')"
                    }
                }
            }
            
            # Also add commit message context if no detailed changes found for any file
            if (-not ($summaries | Where-Object { $_ -match $shortHash })) {
                $summaries += "(commit $shortHash) $message"
            }
        }
    } catch { }
    
    return $summaries | Select-Object -Unique
}

# ==============================================================================
# CONSOLE OUTPUT HELPERS
# ==============================================================================

function Write-Banner {
    <#
    .SYNOPSIS
        Write a banner/header to console.
    .PARAMETER Title
        Banner title text.
    .PARAMETER Width
        Banner width (default: 50).
    .PARAMETER Color
        Text color (default: Cyan).
    #>
    param(
        [Parameter(Mandatory)][string]$Title,
        [int]$Width = 50,
        [string]$Color = "Cyan"
    )
    
    $border = "=" * $Width
    Write-Host ""
    Write-Host $border -ForegroundColor $Color
    Write-Host $Title -ForegroundColor $Color
    Write-Host $border -ForegroundColor $Color
    Write-Host ""
}

function Write-Success {
    <#
    .SYNOPSIS
        Write a success message.
    #>
    param([Parameter(Mandatory)][string]$Message)
    Write-Host $Message -ForegroundColor Green
}

function Write-Warning {
    <#
    .SYNOPSIS
        Write a warning message.
    #>
    param([Parameter(Mandatory)][string]$Message)
    Write-Host "WARNING: $Message" -ForegroundColor Yellow
}

function Write-Failure {
    <#
    .SYNOPSIS
        Write a failure/error message.
    #>
    param([Parameter(Mandatory)][string]$Message)
    Write-Host "ERROR: $Message" -ForegroundColor Red
}

function Write-Info {
    <#
    .SYNOPSIS
        Write an info message.
    #>
    param([Parameter(Mandatory)][string]$Message)
    Write-Host $Message -ForegroundColor Gray
}

# ==============================================================================
# MODULE EXPORTS
# ==============================================================================

Export-ModuleMember -Function @(
    # Step Timing
    'Initialize-StepTimer'
    'Start-Step'
    'Complete-Step'
    'Get-StepTimings'
    'Show-TimingSummary'
    
    # Prerequisites
    'Test-CommandExists'
    'Assert-Command'
    'Assert-Commands'
    'Test-EnvironmentVariable'
    'Assert-EnvironmentVariable'
    
    # Git Utilities
    'Get-GitStatus'
    'Show-GitStatus'
    'Get-CurrentBranch'
    'Get-LastTag'
    'Get-CommitsSinceTag'
    'Get-VersionBumpCommit'
    'Get-CommitsForVersion'
    'Get-DetailedCommits'
    'Get-UncommittedChanges'
    'Get-CommitChangesAnalysis'
    
    # Console Output
    'Write-Banner'
    'Write-Success'
    'Write-Warning'
    'Write-Failure'
    'Write-Info'
)
