# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.6.8] - 2026-06-27

### Changed
- Updated NuGet dependencies: Microsoft.Extensions.* and System.Threading.RateLimiting 10.0.8 → 10.0.9, Microsoft.AspNetCore.* 2.3.10 → 2.3.11, Microsoft.IdentityModel.Tokens and System.IdentityModel.Tokens.Jwt 8.19.x → 8.19.1, Microsoft.NET.Test.Sdk 18.6.0 → 18.7.0

## [1.6.7] - 2026-06-02

### Fixed
- Missing dependencies update

## [1.6.6] - 2026-06-02

### Changed
- Reorganized `utils/` into engine, plugin, and module layout (`engines/`, `plugins/`, `modules/`, `tools/`) with root entry scripts (`Invoke-TestEngine.bat`, `Invoke-ReleasePackage.bat`, and related launchers).
- **TestRunner**: resolves `.csproj` paths explicitly, builds once, then runs `dotnet test --no-build` to reduce file-lock failures on Windows.

### Fixed
- Test engine `scriptSettings.json` paths corrected for the `utils/engines/test` layout.
- **EnvVarTests**: user-level environment variable tests no longer hang the xUnit test host on Windows (non-parallel collection and timeout-safe skip).
- **.gitignore**: added override rules so the `utils/` folder is tracked despite broader ignore patterns.

## [1.6.5] - 2026-03-02

### Changed
- Replaced explicit `ArgumentNullException` throws with `ArgumentNullException.ThrowIfNull` in `ExpressionExtensions`, `NetworkConnection`, `Base32Encoder`, `StringExtensions.CSVToDataTable`, `FileLoggerProvider`, and `JsonFileLoggerProvider`.
- **Base32Encoder**: empty input now throws `ArgumentException` instead of `ArgumentNullException` for clearer semantics.
- **StringExtensions.CSVToDataTable**: null file path throws via `ThrowIfNull`; empty/whitespace path throws `ArgumentException`.
- **ObjectExtensions**: `DeepClone` returns `default` for null input; `DeepEqual` explicitly treats (null, null) as true and (null, non-null) as false. Replaced obsolete `FormatterServices.GetUninitializedObject` with `RuntimeHelpers.GetUninitializedObject`. Fixed nullability in `ReferenceEqualityComparer` to match `IEqualityComparer<object>`.
- **TotpGenerator**: recovery code generation uses range syntax (`code[..4]`, `code[4..8]`) instead of `Substring`.

### Fixed
- **ExceptionExtensions.ExtractMessages**: null check added to avoid `NullReferenceException` when passed null.
- **BaseFileLogger.RemoveExpiredLogFiles**: guard added before `Substring(4)` so malformed log file names do not throw.

## [1.6.4] - 2026-02-21

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
