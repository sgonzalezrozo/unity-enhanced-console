# Enhanced Console

**A Unity Editor console for faster log triage in large projects.**

[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

Enhanced Console runs beside Unity's Console and helps you cut through noisy logs with search, collapse modes and section-based filtering. Select a log to read its stack trace, then click through to the source line.

> **TODO:** Add a screenshot or GIF showing section filtering and a click from a stack trace to source code.

## Why I built it

In a large live project, the log you need is often buried in hundreds of others. Unity's Console is enough for many projects, but I wanted to group logs by system and get from a failing log to its code faster.

> **TODO:** Replace this with your own story: which debugging problem made you build it, and what Unity's Console could not do.

## Is it right for your project?

**Use Enhanced Console if you:**

- Work in a large codebase with noisy logs
- Want to group logs by system
- Spend a lot of time reading stack traces

**Consider something else if you:**

- Have a small project and Unity's Console is enough
- Need to view logs from device builds. Enhanced Console only runs in the Unity Editor.
- Need logs from background threads

## Features

- Live log capture while the window is open
- Case-insensitive search across messages and stack traces
- Toggles and counts for info, warning and error logs
- Collapsing by message and type, with an optional exact stack trace match
- Section filtering by name or regex, with custom sections, colors and quick toggles
- Stack trace panel with clickable file links
- GameObject selection for logs written with a context object
- Multi-select, copy and optional timestamps
- Clear on Play, Clear on Build and Clear on Recompile options
- Logs preserved across domain reloads while the window is open
- Recompile and Recompile + Asset Refresh buttons

## Compatibility

| Requirement | Supported |
|---|---|
| Unity | 2022.3 or newer. **TODO:** add tested versions. |
| Editor platforms | **TODO:** list tested platforms |
| Editor themes | **TODO:** confirm light and dark |
| IDEs | Uses your External Script Editor setting. **TODO:** list tested IDEs. |
| Player builds | The package is Editor-only and adds nothing to player builds. |
| Dependencies | None |

## Installation

Git must be installed on your machine.

1. In Unity, open **Window > Package Manager**.
2. Select **+ > Add package from git URL**.
3. Enter:

```text
https://github.com/sgonzalezrozo/unity-enhanced-console.git
```

To pin a release, add its tag:

```text
https://github.com/sgonzalezrozo/unity-enhanced-console.git#v0.1.0
```

## Quick start

1. Open **Window > Enhanced Console**.
2. Enter Play Mode or trigger some logs.
3. Use the toolbar to search, filter and collapse entries.
4. Turn on **Stack Trace** and select a log to see its stack trace.
5. Double-click a log, or click a linked stack trace line, to open the source file.
6. Click a log written with a context object, such as `Debug.Log(message, gameObject)`, to select that GameObject.

Logs written before the window opens are not shown, so keep it docked.

## Section filtering

Section filtering is optional. It lets you hide logs from selected systems while everything else stays visible.

### Set it up

1. Run **Tools > Enhanced Console > Setup**. This creates `Assets/EnhancedConsole/SelectiveLogsConfig.asset` with five default sections, or moves an existing config there.
2. Select the asset to rename sections, change colors, add your own or turn on regex.
3. Turn on the **SL** button in the console toolbar.
4. Select the arrow beside **SL**, choose **Sections...**, then clear a section to hide its logs.

### How logs match a section

A log matches when the section name appears in its message or stack trace. Matching ignores case and spaces, so you don't need a special logging API:

```csharp
// Matches the default "ScreenSystem" section by message.
Debug.Log("[ScreenSystem] Opened Settings");
```

A log also matches if `ScreenSystem` appears in its stack trace, such as in a namespace or class name.

For more control, turn on **Regex** for a section and enter a pattern. Regex matching also ignores case.

The first matching section wins, and logs that match no section stay visible.

### Filter modes

Select the arrow beside the **SL** button, then choose a mode:

| Mode | Searches |
|---|---|
| Stack And Content | The message and stack trace |
| Only Stack | The stack trace only |
| Only Content | The message only |

## Using the toolbar

| Control | Purpose |
|---|---|
| Clear | Clear this window and Unity's Console |
| Clear arrow | Set Clear on Play, Clear on Build and Clear on Recompile |
| Collapse | Group repeated logs with the same message and type |
| Collapse arrow | Also require an exact stack trace match |
| Stack Trace | Show or hide the resizable stack trace panel |
| Time | Show or hide timestamps |
| Recompile | Request a script recompile, available outside Play Mode |
| Recompile arrow | Choose Recompile Only or Recompile and Asset Refresh |
| Search | Find plain text in messages and stack traces, ignoring case |
| Info, warning, error | Show or hide each log level, with counts |
| SL | Turn section filtering on or off |

## Keyboard and mouse

| Action | Result |
|---|---|
| Click | Select a log and its context GameObject, if it has one |
| Up and Down arrows | Move the selection |
| Shift-click | Select a range |
| Ctrl-click or Cmd-click | Add or remove a log from the selection |
| Ctrl+A or Cmd+A | Select all visible logs |
| Ctrl+C or Cmd+C | Copy selected logs |
| Double-click | Open the log's source file |

## How log capture works

- Enhanced Console uses `Application.logMessageReceived`, so it captures main-thread logs only.
- The window stores up to 10,000 entries in memory.
- Logs are kept across domain reloads while the window is open, including after a recompile or entering Play Mode.
- **Clear on Recompile** clears logs when scripts recompile.
- Closing the window or restarting Unity clears stored logs.
- While the window is open, Enhanced Console wraps `Debug.unityLogger.logHandler` in the Editor to read each log's context object. Every log is forwarded to the original handler, and the wrapper stays in place until the next domain reload.
- Double-click opens the first project frame in the stack trace. Custom logger wrappers aren't skipped, so a wrapper's own line may open instead of its caller.
- **Clear** also clears Unity's Console through the internal `UnityEditor.LogEntries` API, which a future Unity version could change.

> **TODO:** Test whether compiler errors appear reliably, then document the result.

## Architecture

For contributors:

| Type | Purpose |
|---|---|
| `EnhancedConsoleWindow` | Coordinates the UI and editor integration |
| `ConsoleViewRenderer` | Renders console styles and views |
| `LogEntryManager` | Stores, groups and rebuilds log entries |
| `LogFilterManager` | Combines log level, search and section filters |
| `ConsoleSearchEngine` | Handles message and stack trace search |
| `SectionedLogOrganizer` | Applies section rules and filter modes |
| `StackTraceParser` | Extracts file paths and line numbers |
| `LogContextHelper` | Captures the context object passed with a log |

## Troubleshooting

| Problem | Try this |
|---|---|
| The **SL** button shows `--/--` | Run **Tools > Enhanced Console > Setup** |
| A log isn't in a section | Check that the section name or regex matches the log, and check the filter mode |
| Older logs are missing | Logs are only captured while the window is open, so keep it docked |
| Clicking a log doesn't select a GameObject | Pass a GameObject or Component as the log's context, and check that the object is still loaded |
| A log opens the wrong file | Check whether a custom logger wrapper is the first project frame in the stack trace |
| A stack trace line isn't clickable | Only paths starting with `Assets/` or `Packages/` are clickable |
| A log from a background thread is missing | Background-thread logs aren't captured |

## Limitations

- Logs aren't kept after closing the window or restarting Unity.
- The log handler wrapper adds a `LogContextHandler` frame to stack traces, so double-clicking a log in Unity's own Console can open that file instead of your code.
- Logs aren't captured from background threads or device builds.

## Project status

Enhanced Console is a pre-release package at version `0.1.0`. Its API and behavior may change before `1.0.0`.

> **TODO:** Add a roadmap link and breaking-change policy.

## Contributing

Bug reports and pull requests are welcome. Please open an issue before starting a large change.

## Related packages

| Package | Purpose |
|---|---|
| [Fast IoC](https://github.com/sgonzalezrozo/unity-fast-ioc) | Code-generated dependency injection |
| [Screen System](https://github.com/sgonzalezrozo/unity-screen-system) | Declarative UI navigation |
| [UI Tweens](https://github.com/sgonzalezrozo/unity-ui-tweens) | ECS-based UI animation |
| [Recyclable Scroller](https://github.com/sgonzalezrozo/unity-recyclable-scroller) | Virtualized lists |

## License

Released under the [MIT License](LICENSE).
