# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## v1.6.2 - 2026-02-13

### Added
- `BaseFileLogger` idempotent log folder creation and tests

### Changed
- Improved `BaseFileLogger` to ensure log folder is recreated if deleted during runtime (idempotent folder creation).
- Added comprehensive tests verifying log folder recreation and robustness against folder deletion scenarios.
- Removed AI assisted CHANGELOG.md generation as it's weak and not worth the effort.

## v1.6.1 - 2026-31-01

### Added
- Added `CreateMutex` method to `BaseFileLogger`
- Added `ResolveFolderPath` and `SanitizeForPath` methods to `FileLoggerProvider`
- Added `ResolveFolderPath` and `SanitizeForPath` methods to `JsonFileLoggerProvider`
- Added `LoggerPrefix` class for managing logger prefixes
- AI assisted CHANGELOG.md generation

### Changed
- Improved error handling in `BaseFileLogger`

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
