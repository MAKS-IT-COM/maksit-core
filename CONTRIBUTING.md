# Contributing to MaksIT.Core

Thank you for your interest in contributing to MaksIT.Core! This document provides guidelines for contributing to the project.

## Getting Started

1. Fork the repository
2. Clone your fork locally
3. Create a new branch for your changes
4. Make your changes
5. Submit a pull request

## Development Setup

### Prerequisites

- .NET10 SDK or later
- Git

### Building the Project

```bash
cd src
dotnet build MaksIT.Core.sln
```

### Running Tests

```bash
cd src
dotnet test MaksIT.Core.Tests
```

## Commit Message Format

This project uses the following commit message format:

```
(type): description
```

### Commit Types

| Type | Description |
|------|-------------|
| `(feature):` | New feature or enhancement |
| `(bugfix):` | Bug fix |
| `(refactor):` | Code refactoring without functional changes |
| `(chore):` | Maintenance tasks (dependencies, CI, documentation) |

### Examples

```
(feature): add support for custom JWT claims
(bugfix): fix multithreading issue in file logger
(refactor): simplify expression extension methods
(chore): update copyright year to 2026
```

### Guidelines

- Use lowercase for the description
- Keep the description concise but descriptive
- No period at the end of the description

## Code Style

- Follow standard C# naming conventions
- Use XML documentation comments for public APIs
- Keep methods focused and single-purpose
- Write unit tests for new functionality

## Pull Request Process

1. Ensure all tests pass
2. Update documentation if needed
3. Update CHANGELOG.md with your changes under the appropriate version section
4. Submit your pull request against the `main` branch

## Versioning

This project follows [Semantic Versioning](https://semver.org/):

- **MAJOR** - Breaking changes
- **MINOR** - New features (backward compatible)
- **PATCH** - Bug fixes (backward compatible)

## Release Process

The release process is automated via PowerShell scripts in the `src/scripts/` directory.

### Prerequisites

- Docker Desktop running (for Linux tests)
- GitHub CLI (`gh`) installed
- Environment variables configured:
  - `NUGET_MAKS_IT` - NuGet.org API key
  - `GITHUB_MAKS_IT_COM` - GitHub Personal Access Token (needs `repo` scope)

### Release Scripts Overview

| Script | Purpose |
|--------|---------|
| `Generate-Changelog.ps1` | AI-assisted changelog generation and license year update |
| `Release-NuGetPackage.ps1` | Build, test, and publish to NuGet.org and GitHub |
| `Force-AmendTaggedCommit.ps1` | Fix mistakes in tagged commits |

### Release Workflow

1. **Update version** in `MaksIT.Core/MaksIT.Core.csproj`

2. **Generate changelog** (uses AI with Ollama if available):
   ```powershell
   cd src/scripts
   .\Generate-Changelog.ps1          # Updates CHANGELOG.md and LICENSE.md year
   .\Generate-Changelog.ps1 -DryRun  # Preview without changes
   ```

3. **Review and commit** all changes:
   ```bash
   git add -A
   git commit -m "(chore): release v1.x.x"
   ```

4. **Create version tag**:
   ```bash
   git tag v1.x.x
   ```

5. **Run release script**:
   ```powershell
   cd src/scripts
   .\Release-NuGetPackage.ps1          # Full release
   .\Release-NuGetPackage.ps1 -DryRun  # Test without publishing
   ```

---

### Generate-Changelog.ps1

AI-assisted changelog generation using a 3-pass LLM pipeline with Ollama.

**What it does:**
1. Analyzes uncommitted changes and converts them to changelog items
2. Consolidates similar items and removes duplicates
3. Formats output in Keep a Changelog format
4. Updates LICENSE.md copyright year if needed

**Usage:**
```powershell
.\Generate-Changelog.ps1          # Generate and update files
.\Generate-Changelog.ps1 -DryRun  # Preview without changes
```

**Configuration:** `changelogsettings.json`
- LLM models for each pass
- Prompt templates
- File paths

---

### Release-NuGetPackage.ps1

Builds, tests, and publishes the package to NuGet.org and GitHub.

**The script is IDEMPOTENT** - you can safely re-run it if any step fails. It will skip already-completed steps and only create what's missing.

**What it does:**
1. Validates prerequisites and environment
2. Checks if already released on NuGet.org (skips if exists)
3. Checks if GitHub release exists (skips if exists)
4. Runs security vulnerability scan
5. Builds and tests on Windows
6. Builds and tests on Linux (via Docker)
7. Analyzes code coverage
8. Creates NuGet package (.nupkg and .snupkg)
9. Pushes to NuGet.org
10. Creates GitHub release with assets

**Usage:**
```powershell
.\Release-NuGetPackage.ps1          # Full release
.\Release-NuGetPackage.ps1 -DryRun  # Test without publishing
```

**Idempotent behavior:**
| Target | Already Released | Action |
|--------|-----------------|--------|
| NuGet.org | Yes | Skip |
| NuGet.org | No | Publish |
| GitHub | Yes | Skip |
| GitHub | No | Create release |

**Validation checks:**
- Version source: Reads latest version from `CHANGELOG.md`
- Tag required: Must have a tag matching the changelog version (e.g., `v1.2.3`)
- Branch validation: Tag must be on configured branch (default: `main`)
- Clean working directory: No uncommitted changes allowed

**Configuration:** `scriptsettings.json`

---

### Force-AmendTaggedCommit.ps1

Fixes mistakes in the last tagged commit by amending it and force-pushing.

**When to use:**
- You noticed an error after committing and tagging
- Need to add forgotten files to the release commit
- Need to fix a typo in the release

**What it does:**
1. Verifies the last commit has an associated tag
2. Stages all pending changes
3. Amends the latest commit (keeps existing message)
4. Deletes and recreates the tag on the amended commit
5. Force pushes the branch and tag to origin

**Usage:**
```powershell
.\Force-AmendTaggedCommit.ps1          # Amend and force push
.\Force-AmendTaggedCommit.ps1 -DryRun  # Preview without changes
```

**Warning:** This rewrites history. Only use on commits that haven't been pulled by others.

---

### Fixing a Failed Release

If the release partially failed (e.g., NuGet succeeded but GitHub failed):

1. **Just re-run the release script:**
   ```powershell
   .\Release-NuGetPackage.ps1
   ```
   The script will skip NuGet (already released) and create the GitHub release.

2. **If you need to fix the commit content:**
   ```powershell
   # Make your fixes, then:
   .\Force-AmendTaggedCommit.ps1
   .\Release-NuGetPackage.ps1
   ```

## License

By contributing, you agree that your contributions will be licensed under the MIT License.
