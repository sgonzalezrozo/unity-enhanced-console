# Changelog

All notable changes to this package are documented in this file.

## [0.3.0] - 2026-11-03

### Documentation

- Added a before and after example, a Unity Console comparison and section recipes.
- Documented how copying and the time column work.
- Added contributing, security and issue template files, and a Quick Start sample.
- Removed an out-of-date internal note from `EnhancedConsoleWindow`.

## [0.2.0] - Unreleased

### Added

- **Sections...** menu for turning sections on and off from the console.
- Logs now survive domain reloads while the window is open.
- The Collapse setting is remembered.

### Changed

- **Breaking:** Renamed the namespace from `Tools.EnhanceConsole.Editor` to `EnhancedConsole.Editor`.
- **Breaking:** Renamed the assembly to `EnhancedConsole.Editor`.
- Changed the Setup menu to `Tools/Enhanced Console/Setup`.
- Moved the section config to `Assets/EnhancedConsole`. Setup moves an existing config.
- Clear on Build now uses `IPreprocessBuildWithReport`.
- Clear on Recompile no longer clears logs when entering Play Mode.

### Removed

- Unused classes and error tracking code.

## [0.1.0] - Unreleased

First pre-release for private testing.

### Added

- Enhanced Console window with live log capture
- Search across messages and stack traces
- Log level toggles and collapse modes
- Section-based filtering with custom sections and regex patterns
- Clickable stack traces
- GameObject selection for logs written with a context object
- Multi-select and copy
- Clear on Play, Build and Recompile options
- Recompile and asset refresh shortcuts
