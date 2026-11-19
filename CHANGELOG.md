# Changelog

All notable changes to this package are documented in this file.

## [Unreleased]

## [0.3.6] - 2026-11-19

### Fixed

- Clicking a stack trace line for a script in a package opens the real file. Project paths go through `AssetDatabase.OpenAsset`, like the Project window, and other paths are resolved to their location on disk. VS Code used to open an empty, unsaved buffer instead of the script.

## [0.3.5] - 2026-11-18

### Documentation

- The README's description is plain text instead of a quote, and short enough for two lines.

## [0.3.4] - 2026-11-18

### Documentation

- The README is now a short overview with installation, a quickstart and links. Its content moved into guides in `Documentation~/`: Getting Started, Features, Section Filtering, Toolbar & Shortcuts, How It Works, Comparison, Troubleshooting & Support, FAQ and Project Status.
- Installation instructions no longer pin a release tag.

### Removed

- The welcome dialog shown once per project after the package is imported.

## [0.3.3] - 2026-11-18

### Documentation

- The README describes every feature in detail, grouped by capture, finding, reading, acting on logs and performance.
- Added a Support section: where to report bugs, ask questions and report security problems.
- Corrected the Troubleshooting rows for clickable stack trace lines and invalid regexes, and added one for logs that don't open.
- The contributing guide links the C# standards the package follows, and adds a manual test pass and GitHub Release steps to the release checklist.
- `package.json` links the changelog and license, so Package Manager shows them.

### Changed

- The section menu turns sections on and off directly. Before, it tracked them by name, so two sections with the same name changed together.
- The last long methods are split, in the section menu, the section config inspector, the Setup command and message truncation. Behavior is unchanged.

## [0.3.2] - 2026-11-18

### Added

- The time column can show time since startup, wall-clock time or frame number. Choose with the arrow beside **Time** or the window's ⋮ menu. Logs now store their wall-clock time and frame.
- **Error Pause** pauses Play Mode when an error, exception or failed assert is logged.
- Display options in the window's ⋮ menu: 1 to 10 lines per log, a monospace font, and turning rich text off.
- Keyboard navigation: Page Up/Down and Home/End move the selection, Shift extends it, and the selection stays in view. Enter or Return opens the selected log's source. Ctrl+F or Cmd+F focuses the search field.
- Delete (Backspace on a Mac) or Edit > Delete removes the selected logs from Enhanced Console and clears the selection. In Collapse mode it removes every log in the selected groups.
- Autoscroll: while the list is scrolled to the bottom, it follows new logs. Scrolling up stops it, and scrolling back down resumes it.
- The first search match in each log is highlighted, except in messages with rich text tags.
- Logs from background threads, through `Application.logMessageReceivedThreaded`. They keep the time they were written.
- Compiler errors and warnings are read from the compilation pipeline, so they appear even if they never reach the log callback. Like Unity's Console, **Clear** keeps compiler errors until their assembly compiles.

### Documentation

- Rechecked the Unity's Console comparison against the Unity 6 manual. Unity's Console has timestamps, lines per log, a monospace font and Error Pause, and keeps compiler errors after Clear.
- Added a comparison with Editor Console Pro, Debug.Log Extensions, Rider's Unity log view and runtime consoles.

### Changed

- A section with an invalid regex now logs a warning once, instead of silently matching its name as plain text.
- **Force Recompile** logs one line instead of three.
- `EnhancedConsoleWindow` is split into small classes by responsibility: preferences, log capture, toolbar, log list, input, selection and details panel. `LogEntryManager` and `SectionedLogOrganizer` are split the same way. Behavior is unchanged.
- Code cleanup to follow the C# standards in [unity-architecture-agents](https://github.com/sgonzalezrozo/unity-architecture-agents): naming, spaces instead of tabs, 120-character lines, no lambdas, cached toolbar content and named sizes. Renamed serialized fields keep their data through `[FormerlySerializedAs]`.
- Faster with many logs. The filtered list is cached and only new logs are filtered, instead of copying and filtering every log several times per repaint. Collapse uses a lookup table instead of a linear search, so turning it on with 10,000 logs no longer freezes the Editor.
- Section matching no longer allocates for every log on every repaint. Regexes are parsed once, and each log remembers which section it matched.
- An invalid section regex is detected once, instead of throwing an exception for every log on every repaint.
- The window repaints at most 30 times per second while logs arrive, instead of once per log plus a queued repaint, and no longer repaints every 0.1 seconds in Play Mode when nothing changed.
- The stack trace panel parses the selected stack trace once and draws only the visible lines.
- Clicking a log finds its context object directly, instead of searching every loaded scene. Logs whose context is an asset, such as a ScriptableObject, now ping that asset.
- When logs are pruned at 10,000, the oldest 1,000 are removed at once.
- Resizing the stack trace panel saves its height when you release the mouse, not on every drag event.

### Fixed

- Double-click and Enter didn't open logs from local packages (`file:` dependencies), whose stack traces have absolute paths. Absolute paths now open, mapped to `Packages/` when possible, and Windows paths with a drive letter are read correctly.
- Enter or Return didn't open the selected log when the search field had keyboard focus, for example after searching. The search field took the key to start editing.
- A section with an empty name matched every log.
- The selection jumped to a different log when a filter changed, Collapse was toggled or old logs were pruned. The selected log is found again instead, and multi-selection is cleared.
- `Debug.Log(message, context)` from a background thread threw an exception, because the context handler read the context object off the main thread. The context is now read only for main-thread logs.
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
