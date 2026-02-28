# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## v1.6.4 - 2026-02-21

### Added
- New shared utility modules under `utils/`:
- `Logging.psm1` for timestamped, aligned log output.
- `ScriptConfig.psm1` for shared settings loading and command assertions.
- `GitTools.psm1` for reusable git operations.
- `TestRunner.psm1` for shared test/coverage execution.
- New `Generate-CoverageBadges` utility script and settings to generate SVG coverage badges.

### Changed
- Refactored release/amend/badges scripts to a modular structure with shared modules.
- Standardized script structure with regions and clearer comments.
- Switched script output to centralized logging format with timestamps (where logging module is available).
- Updated release settings comments (`_comments`) for clarity and accuracy.
- Updated `README.md` to show coverage badges.

### Removed
- Removed legacy scripts from `src/scripts/` in favor of the `utils/`-based toolchain.
- Removed unused helper logic (including obsolete step-wrapper usage and unused csproj helper).

### Fixed
- Fixed NuGet packing metadata by explicitly packing `LICENSE.md`, `README.md`, and `CHANGELOG.md` into the package.
- Fixed release pipeline packaging flow to create and resolve the `.nupkg` before `dotnet nuget push`.
- Added `/staging` to `.gitignore` to avoid committing temporary release artifacts.

### Security
- Kept release-time git checks and branch/tag validation in shared release flow to reduce accidental publish risk.

<!-- 
Template for new releases:

## v1.x.x

### Added
- New features

### Changed
- Changes in existing functionality

### Deprecated
- Soon-to-be removed features

### Removed
- Removed features

### Fixed
- Bug fixes

### Security
- Security improvements
-->
