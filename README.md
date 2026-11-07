# Enhanced Console

**A Unity Editor console for faster log triage in large projects.**

[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Tested on Unity 6000.3](https://img.shields.io/badge/Unity-6000.3%20tested-black?logo=unity)](#compatibility)
[![Version 0.3.0](https://img.shields.io/badge/version-0.3.0-blue.svg)](CHANGELOG.md)

Enhanced Console is a free, open-source Unity Editor console window for Unity 6, for cutting through noisy logs. It adds section-based and regex log filtering, search across messages and stack traces, log timestamps, and copying a range of logs with or without stack traces. Click a stack trace line to open the source. It runs beside Unity's Console, adds nothing to player builds, and needs no special logging API.

| Everything, all at once | With the `Networking` section hidden |
|---|---|
| `[Networking] Packet 1042 received` | |
| `[Networking] Packet 1043 received` | |
| `[Shop] Purchase failed: not enough coins` | `[Shop] Purchase failed: not enough coins` |
| `[Networking] Packet 1044 received` | |

<!-- Demo GIF: Documentation~/images/demo.gif -->

## Why I built it

I started this console in part to copy a range of logs without their stack traces. Then I realized I also needed to filter logs in more ways than a search bar allows.

So I added a selective filter system next to the traditional search. Each project sets up its own filters in a ScriptableObject, and they can match the log, the stack trace or both.

I also thought it would be great to show a time on each log, as a simple measurement for developers while playing in the Editor.

I built and refined it on my own time in personal projects. It isn't used in production yet. In November 2026 I extracted it into this standalone package and documented it. Earlier history lives in a private repository.

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
- Multi-select, copy and optional log times
- Clear on Play, Clear on Build and Clear on Recompile options
- Logs preserved across domain reloads while the window is open
- Recompile and Recompile + Asset Refresh buttons

## Compared with Unity's Console

Enhanced Console runs beside Unity's Console and doesn't replace it.

| | Unity's Console | Enhanced Console |
|---|---|---|
| Search and log level toggles | Yes | Yes |
| Collapse | Yes | Yes, with an optional exact stack trace match |
| Clear on Play, Build and Recompile | Yes | Yes |
| Hide logs by system | No | Yes, with sections |
| Regex matching | No | Yes, per section |
| Copy a range of logs | Depends on the Unity version | Yes, with or without stack traces |
| Pause on error | Yes | No |
| Logs from before the window opened | Yes | No |
| Logs from background threads | Yes | No |
| Logs from device builds | Yes, by connecting to the player | No |

## Compatibility

| Requirement | Supported |
|---|---|
| Unity | Tested on Unity 6 (`6000.3.16f1`). The package declares `2022.3` as its minimum, but older versions aren't tested yet. |
| Editor platforms | Not verified yet |
| Editor themes | Light and dark are both handled in code, but neither is verified yet |
| IDEs | Uses your External Script Editor setting. Tested IDEs aren't published yet. |
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
https://github.com/sgonzalezrozo/unity-enhanced-console.git#v0.3.0
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

### Section recipes

Sections are checked in order: the five default sections first, then your own. The first match decides whether a log is shown.

**Match a namespace.** Name the section after the namespace, then set the filter mode to **Only Stack**. Unity's stack traces include the full type name, so this matches every log written from that code, whatever the message says:

| Section name | Matches |
|---|---|
| `MyGame.Networking` | Logs from any class in that namespace |

**Match a tag at the start of a message.** Turn on **Regex** and use **Only Content**:

```text
^\[(Net|Socket)\]
```

**Match several folders.** Turn on **Regex** and use **Only Stack**:

```text
Assets/Features/(Shop|Inventory)/
```

**Match a whole word.** Plain matching finds `Net` inside `Network` and `Planet`. A regex avoids that:

```text
\bNet\b
```

If a regex is invalid, the section falls back to matching its name as plain text.

## Using the toolbar

| Control | Purpose |
|---|---|
| Clear | Clear this window and Unity's Console |
| Clear arrow | Set Clear on Play, Clear on Build and Clear on Recompile |
| Collapse | Group repeated logs with the same message and type |
| Collapse arrow | Also require an exact stack trace match |
| Stack Trace | Show or hide the resizable stack trace panel |
| Time | Show or hide each log's time, in `mm:ss.mmm`, from `Time.realtimeSinceStartup` |
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
| Ctrl+C or Cmd+C | Copy selected logs. Stack traces are included only while **Stack Trace** is on. |
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


Enhanced Console only shows what reaches `Application.logMessageReceived`. It doesn't read compiler results directly, and whether compiler errors always appear hasn't been verified yet. If your scripts stop compiling, check Unity's Console.

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

## FAQ

### How do I filter Debug.Log messages by system in Unity?

Create sections in Enhanced Console, named after a tag or namespace, or with a regex, then hide the sections you don't need. Logs need no special API. See [Section filtering](#section-filtering).

### Enhanced Console vs Unity's Console?

It runs beside Unity's Console and adds sections, regex, timestamps and range copying. Unity's Console still covers background threads, device builds and pause on error. See [Compared with Unity's Console](#compared-with-unitys-console).

### Can I copy several logs without their stack traces?

Yes. Select a range, turn off **Stack Trace**, and press Ctrl+C or Cmd+C.

### Does it show logs from device builds?

No. It only runs in the Unity Editor.

### Does it add anything to my build?

No. The package is Editor-only.

### Does it work with Unity 6?

Yes. It's tested on Unity `6000.3`. The package declares `2022.3` as its minimum, but older versions aren't tested yet.

### Does it work with AI coding agents?

Not directly. Agents read Unity's Console through Unity MCP or batch-mode logs, so Enhanced Console doesn't change what they see. Plain `[Tag]` prefixes in messages make sections match, and they also make logs easier for agents to search.

### Is there a better console for Unity?

There are several Editor console replacements. Enhanced Console is a free one that adds sections, regex, timestamps and range copying beside Unity's own Console, rather than replacing it. See [Is it right for your project?](#is-it-right-for-your-project).

### How do I hide noisy logs in the Unity Console?

Put a tag in the message, such as `[Networking]`, create a section with that name, and clear it in the **SL** menu. Everything else stays visible. See [Section filtering](#section-filtering).

### How do I install it?

In Unity, open **Window > Package Manager**, select **+ > Add package from git URL** and enter `https://github.com/sgonzalezrozo/unity-enhanced-console.git`. See [Installation](#installation).

### Is it production-ready? Does anyone use it?

Not yet. Enhanced Console is a pre-release package (`0.x`) by a single author. It isn't used in a shipped game, and it has no other users yet, so expect rough edges and API changes before `1.0.0`. Bug reports are very welcome. See [Project status](#project-status).

### Is it free for commercial use?

Yes. It's MIT licensed.

## Troubleshooting

| Problem | Try this |
|---|---|
| The **SL** button shows `--/--` | Run **Tools > Enhanced Console > Setup** |
| A log isn't in a section | Check that the section name or regex matches the log, and check the filter mode |
| Older logs are missing | Logs are only captured while the window is open, so keep it docked |
| Copied logs have no stack traces | Turn on **Stack Trace** before copying |
| A regex section matches by name instead | Check the pattern, because an invalid regex falls back to the section name |
| Clicking a log doesn't select a GameObject | Pass a GameObject or Component as the log's context, and check that the object is still loaded |
| A log opens the wrong file | Check whether a custom logger wrapper is the first project frame in the stack trace |
| A stack trace line isn't clickable | Only paths starting with `Assets/` or `Packages/` are clickable |
| A log from a background thread is missing | Background-thread logs aren't captured |

## Limitations

- Logs aren't kept after closing the window or restarting Unity.
- The log handler wrapper adds a `LogContextHandler` frame to stack traces, so double-clicking a log in Unity's own Console can open that file instead of your code.
- Logs aren't captured from background threads or device builds.

## Project status

Enhanced Console is a pre-release package at version `0.3.0`. Its API and behavior may change before `1.0.0`. Not yet used in a shipped game.

Until `1.0.0`, minor versions (`0.x.0`) can include breaking changes, and patch versions (`0.x.y`) don't. Breaking changes are marked **Breaking** in the [changelog](CHANGELOG.md).

There's no public roadmap yet. Known gaps are listed under [Limitations](#limitations), and planned work is tracked in [issues](https://github.com/sgonzalezrozo/unity-enhanced-console/issues).

## Contributing

Bug reports and pull requests are welcome. Please open an issue before starting a large change. See the [contributing guide](.github/CONTRIBUTING.md) and [security policy](.github/SECURITY.md).

## Related packages

| Package | Purpose |
|---|---|
| [Fast IoC](https://github.com/sgonzalezrozo/unity-fast-ioc) | Code-generated dependency injection |
| [Screen System](https://github.com/sgonzalezrozo/unity-screen-system) | Declarative UI navigation |
| [UI Tweens](https://github.com/sgonzalezrozo/unity-ui-tweens) | ECS-based UI animation |
| [Recyclable Scroller](https://github.com/sgonzalezrozo/unity-recyclable-scroller) | Virtualized lists |

Enhanced Console has no dependencies, so it doesn't need [Tools Core](https://github.com/sgonzalezrozo/unity-tools-core).

## License

Released under the [MIT License](LICENSE).
