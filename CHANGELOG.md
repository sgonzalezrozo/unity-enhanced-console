# Changelog

All notable changes to this package are documented in this file.

## [Unreleased]

### Changed

- Faster with many logs. The filtered list is cached and only new logs are filtered, instead of copying and filtering every log several times per repaint. Collapse uses a lookup table instead of a linear search, so turning it on with 10,000 logs no longer freezes the Editor.
- Section matching no longer allocates for every log on every repaint. Regexes are parsed once, and each log remembers which section it matched.
- An invalid section regex is detected once, instead of throwing an exception for every log on every repaint.
- The window repaints at most 30 times per second while logs arrive, instead of once per log plus a queued repaint, and no longer repaints every 0.1 seconds in Play Mode when nothing changed.
- The stack trace panel parses the selected stack trace once and draws only the visible lines.
- Clicking a log finds its context object directly, instead of searching every loaded scene. Logs whose context is an asset, such as a ScriptableObject, now ping that asset.
- When logs are pruned at 10,000, the oldest 1,000 are removed at once.
- Resizing the stack trace panel saves its height when you release the mouse, not on every drag event.

### Fixed

- A section with an empty name matched every log.
- The selection jumped to a different log when a filter changed, Collapse was toggled or old logs were pruned. The selected log is found again instead, and multi-selection is cleared.
- `Debug.Log(message, component)` from a background thread threw an exception, because the context handler read `component.gameObject` off the main thread.
- A context logged on a background thread could attach to the next main-thread log.
- Errors in the window's GUI were drawn as a help box that broke the layout and repeated on every repaint. They're now logged once.
- The **Time** toggle saved preferences whenever any control changed, such as while typing a search.
- The section config inspector didn't refresh its serialized data before drawing, and removing with nothing selected raised an error.

### Removed

- `ConsoleViewRenderer`, which was unused and created three textures each time the window was enabled without destroying them.

## [0.3.1] - 2026-11-17

### Added

- After the package is imported, a dialog links to the documentation, once per project. **Tools > Enhanced Console > Documentation** opens it at any time.
- `documentationUrl` in `package.json`, so the Package Manager links to the documentation.

### Documentation

- An FAQ, `llms.txt` and more package keywords.
- Said that the package isn't used in production or in a shipped game yet.

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
