# Enhanced Console

**A Unity Editor console for faster log triage in large projects.**

[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Tested on Unity 6000.3](https://img.shields.io/badge/Unity-6000.3%20tested-black?logo=unity)](#compatibility)
[![Version 0.3.3](https://img.shields.io/badge/version-0.3.3-blue.svg)](CHANGELOG.md)

Enhanced Console is a free, open-source Unity Editor console window for Unity 6, for cutting through noisy logs. It adds section-based and regex log filtering, search across messages and stack traces, log times by time since startup, wall clock or frame, and copying a range of logs with or without stack traces. Click a stack trace line to open the source. It runs beside Unity's Console, adds nothing to player builds, and needs no special logging API.

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

## Features

### Capture

- **Every log, from every thread.** Logs are read through `Application.logMessageReceivedThreaded`. Logs from background threads are queued and shown on the next Editor update, with the time they were written.
- **Compiler errors and warnings** come straight from the compilation pipeline, so they appear even when Unity doesn't send them as regular logs. Each assembly's results replace its previous ones, and a message that also arrives as a regular log is listed once.
- **Logs survive domain reloads** while the window is open: after a recompile and when entering Play Mode. Up to 10,000 logs are kept; the oldest are dropped in chunks of 1,000.
- **Context objects.** A log written with a context, such as `Debug.Log(message, gameObject)`, remembers that object.

### Find

- **Search** for plain text in messages and stack traces, ignoring case. The first match in each visible message is highlighted, except in messages with rich text tags.
- **Log level toggles** for info, warnings and errors, each with its count. Exceptions and failed asserts count as errors.
- **Sections** group logs by system and hide the ones you don't need:
  - A section matches by name, ignoring case and spaces, or by a regex.
  - Choose whether sections search the message, the stack trace or both, so a namespace or folder can be matched without tagging every log.
  - Sections are saved per project in a ScriptableObject, each with a color. While section filtering is on, matching rows show that color as a bar.
  - A popup turns sections on and off, with **Enable All** and **Disable All**. The toolbar shows how many are on, such as `SL (3/5)`.
  - An invalid regex logs a warning once and falls back to the section name.
- **Collapse** groups logs with the same message and type and shows how many times each was logged. Optionally, the stack trace must match too.

### Read

- **Time column** showing time since startup (`mm:ss.mmm`), the local wall-clock time (`HH:mm:ss.fff`) or the frame number (`#1234`).
- **Display options:** 1 to 10 lines per log, a monospace font for messages and stack traces, and rich text on or off.
- **Stack trace panel** for the selected log, resizable by dragging. Lines with a file you can open are links.
- **Autoscroll** follows new logs while the list is at the bottom. Scrolling up pauses it, and scrolling back to the bottom resumes it.

### Act

- **Open the source** with a double-click, Enter or Return, or a click on a stack trace link:
  - It opens the first frame you can open, skipping Unity's logging frames.
  - Project paths (`Assets/`, `Packages/`) work, and so do absolute paths, such as code in a local package. Windows paths with a drive letter work too.
  - A compiler message opens its file at the reported line.
- **Select the context object** by clicking a log. A Component selects its GameObject, and an asset is pinged in the Project window.
- **Copy** the selected logs with Ctrl+C or Cmd+C. Stack traces are included only while **Stack Trace** is on, so you can copy a range of messages without them.
- **Select** with Shift-click for a range, Ctrl-click or Cmd-click to add or remove one log, and Ctrl+A or Cmd+A for all visible logs.
- **Delete** the selected logs with Delete (Backspace on a Mac) or **Edit > Delete**. In Collapse mode, the whole group is removed. Unity's Console keeps them.
- **Keyboard navigation:** arrows, Page Up and Page Down, Home and End, with Shift to extend the selection. The selection stays in view.
- **Error Pause** pauses Play Mode when an error, exception or failed assert is logged.
- **Clear** this window and Unity's Console together. Compiler errors stay until their code compiles, like in Unity's Console. Optionally clear on Play, on Build and on Recompile.
- **Recompile** all scripts from the toolbar, optionally after an asset refresh.

### Performance

- The filtered list is cached, and new logs are filtered as they arrive instead of refiltering everything.
- Collapse groups logs with a lookup table, so turning it on with 10,000 logs doesn't freeze the Editor.
- Each log remembers which section it matched, and regexes are parsed once.
- Only the visible rows and stack trace lines are drawn.
- While logs arrive, the window repaints at most 30 times per second, and not at all when nothing changed.

## Compared with Unity's Console

Enhanced Console runs beside Unity's Console and doesn't replace it. Unity's column reflects the [Unity 6 Console manual](https://docs.unity3d.com/Manual/Console.html), checked in November 2026.

| | Unity's Console | Enhanced Console |
|---|---|---|
| Log level toggles with counts | Yes | Yes |
| Search | One exact substring at a time | Plain text in messages and stack traces, ignoring case |
| Highlight the search match | Yes | Yes, unless the message has rich text tags |
| Hide logs by system | No | Yes, with sections |
| Regex matching | No | Yes, per section |
| Collapse | Yes | Yes, with an optional exact stack trace match |
| Clear on Play, Build and Recompile | Yes | Yes |
| Compiler errors kept after Clear | Yes | Yes |
| Time on each log | Wall-clock time | Time since startup, wall-clock time or frame number |
| Lines per log and monospace font | Yes | Yes |
| Copy a range of logs | Not documented | Yes, with or without stack traces |
| Keyboard: Page Up/Down, Home/End, Enter to open | Not documented | Yes |
| Delete individual logs | No, only Clear | Yes, with the Delete key |
| Pause on error | Yes | Yes |
| Hide `[HideInCallstack]` frames | Yes | No |
| Open the Editor and Player log files | Yes | No |
| Logs from before the window opened | Yes | No |
| Logs from background threads | Yes | Yes |
| Logs from device builds | Yes, by connecting to the player | No |

## Compared with other tools

Features as listed by each tool's publisher or documentation in November 2026. Check their pages for the current details.

| Tool | Where it's stronger | Where Enhanced Console differs |
|---|---|---|
| [Editor Console Pro](https://assetstore.unity.com/packages/tools/utilities/editor-console-pro-11889) (paid) | Search by file, class or GameObject name, with regex. Ignore specific logs. Export to a text file. Remote logs from standalone and mobile builds. Years of releases and reviews. | Free and open source. Sections are saved per project and can match the message, the stack trace or both. Runs beside Unity's Console. |
| [Debug.Log Extensions](https://assetstore.unity.com/packages/tools/utilities/debug-log-extensions-162798) (paid) | Channels from `[Tag]` prefixes, plus extra logging calls such as `Debug.LogIf` and `Debug.LogToFile`, and stripping logs from release builds. | Sections can match stack traces and regexes, so a namespace or folder can be filtered without tags. No logging API to adopt. |
| [Rider's Unity log view](https://www.jetbrains.com/help/rider/Unity_window.html) | Opens any file, class or method named in a log. Edit Mode and Play Mode toggles. | Works without Rider. Sections, regex, range copying, a stack trace panel and Error Pause. |
| Runtime consoles such as [In-game Debug Console](https://openupm.com/packages/com.yasirkula.ingamedebugconsole/), [SRDebugger](https://assetstore.unity.com/packages/tools/gui/srdebugger-console-tools-on-device-27688) and [Quantum Console](https://assetstore.unity.com/packages/tools/utilities/quantum-console-211046) | Logs in player builds and on devices, and commands typed at runtime. SRDebugger adds an options panel and bug reporter. | Built for working through Editor logs: sections, stack trace links and opening the source. Nothing is added to builds. |

## Compatibility

| Requirement | Supported |
|---|---|
| Unity | Tested on Unity 6 (`6000.3.16f1`). The package declares `2022.3` as its minimum, but older versions aren't tested yet. |
| Editor platforms | Used day to day on macOS. Windows and Linux aren't verified yet. |
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
https://github.com/sgonzalezrozo/unity-enhanced-console.git#v0.3.3
```

## Quick start

1. Open **Window > Enhanced Console**.
2. Enter Play Mode or trigger some logs.
3. Use the toolbar to search, filter and collapse entries.
4. Turn on **Stack Trace** and select a log to see its stack trace.
5. Double-click a log, or click a linked stack trace line, to open the source file.
6. Click a log written with a context object, such as `Debug.Log(message, gameObject)`, to select that GameObject. A Component selects its GameObject, and an asset is pinged in the Project window.

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

If a regex is invalid, Enhanced Console logs a warning once and the section falls back to matching its name as plain text.

## Using the toolbar

| Control | Purpose |
|---|---|
| Clear | Clear this window and Unity's Console |
| Clear arrow | Set Clear on Play, Clear on Build and Clear on Recompile |
| Collapse | Group repeated logs with the same message and type |
| Collapse arrow | Also require an exact stack trace match |
| Error Pause | Pause Play Mode when an error, exception or failed assert is logged |
| Stack Trace | Show or hide the resizable stack trace panel |
| Time | Show or hide the time column |
| Time arrow | Show **Since Startup** (`mm:ss.mmm` from `Time.realtimeSinceStartup`), **Wall Clock** (local `HH:mm:ss.fff`) or **Frame** (`#` and `Time.frameCount`) |
| Recompile | Request a script recompile, available outside Play Mode |
| Recompile arrow | Choose Recompile Only or Recompile and Asset Refresh |
| Search | Find plain text in messages and stack traces, ignoring case |
| Info, warning, error | Show or hide each log level, with counts |
| SL | Turn section filtering on or off |

The window's ⋮ menu has display options:

| Option | Purpose |
|---|---|
| Log Entry | Show 1 to 10 lines of each message in the list |
| Time | The same choices as the Time arrow |
| Use Monospace Font | Use a fixed-width font for messages and stack traces |
| Rich Text | Render tags such as `<color>` and `<b>`. Turn it off to see the tags as text. |

All toolbar and display settings are saved per user in EditorPrefs.

## Keyboard and mouse

| Action | Result |
|---|---|
| Click | Select a log and its context object, if it has one |
| Up and Down arrows | Move the selection, keeping it in view |
| Page Up and Page Down | Move the selection by a page |
| Home and End | Select the first or last log |
| Shift with any of the keys above | Extend the selection |
| Enter or Return | Open the selected log's source file |
| Delete (Backspace on a Mac) | Remove the selected logs and clear the selection. In Collapse mode, a row's whole group is removed. Unity's Console keeps them. |
| Ctrl+F or Cmd+F | Focus the search field |
| Shift-click | Select a range |
| Ctrl-click or Cmd-click | Add or remove a log from the selection |
| Ctrl+A or Cmd+A | Select all visible logs |
| Ctrl+C or Cmd+C | Copy selected logs. Stack traces are included only while **Stack Trace** is on. |
| Double-click | Open the log's source file |

## How log capture works

- Enhanced Console uses `Application.logMessageReceivedThreaded`, so it captures logs from every thread. Logs from background threads are queued and added on the next Editor update, with the time they were written.
- Compiler errors and warnings come from `CompilationPipeline.assemblyCompilationFinished`. Each assembly's results replace its previous ones, and the same message arriving as a regular log isn't listed twice.
- **Clear** keeps compiler errors, like Unity's Console. They're removed when their assembly compiles.
- The window stores up to 10,000 entries in memory.
- Logs are kept across domain reloads while the window is open, including after a recompile or entering Play Mode.
- **Clear on Recompile** clears logs when scripts recompile.
- Closing the window or restarting Unity clears stored logs.
- While the window is open, Enhanced Console wraps `Debug.unityLogger.logHandler` in the Editor to read each log's context object. Every log is forwarded to the original handler, and the wrapper stays in place until the next domain reload. Unity only allows reading an object on the main thread, so logs from background threads have no context object.
- Double-click and Enter open the first frame in the stack trace whose file exists, skipping Unity's logging frames. Absolute paths, such as code in a local package, are mapped to `Packages/` when possible. Custom logger wrappers aren't skipped, so a wrapper's own line may open instead of its caller.
- **Clear** also clears Unity's Console through the internal `UnityEditor.LogEntries` API, which a future Unity version could change.


## Architecture

For contributors. Each folder under `Editor/` holds one area:

| Folder or type | Purpose |
|---|---|
| `EnhancedConsoleWindow` | Creates the window's parts, connects them to Editor events and draws them |
| `Window/ConsoleContext` | Shared state and services for one window, and commands such as Clear |
| `Window/Toolbar/` | Toolbar controls: Clear, Collapse, Error Pause, Stack Trace, Time, Recompile, search, log type toggles, section filter |
| `Window/LogList/` | The scrolling list, row drawing, search highlight, mouse and keyboard input, copy and delete |
| `Window/Details/` | The stack trace panel and its resize handle |
| `Window/LogSelection` | Which logs are selected, and how the selection follows its logs when the view is rebuilt |
| `LogCapture/` | Receives logs from every thread and compiler messages from the compilation pipeline |
| `LogManagement/` | Stores logs and keeps the collapsed and filtered views in step with them |
| `Sections/` | Compiles section names and regexes into matchers |
| `Preferences/` | Settings kept in EditorPrefs |
| `LogFilterManager` | Combines log level, search and section filters |
| `SectionedLogOrganizer` | Applies section rules and filter modes |
| `StackTraceParser` | Extracts file paths and line numbers |
| `LogContextHelper` | Captures the context object passed with a log |

## FAQ

### How do I filter Debug.Log messages by system in Unity?

Create sections in Enhanced Console, named after a tag or namespace, or with a regex, then hide the sections you don't need. Logs need no special API. See [Section filtering](#section-filtering).

### Enhanced Console vs Unity's Console?

It runs beside Unity's Console and adds sections, regex, search in stack traces, time since startup or frame number, and range copying. Unity's Console still covers device builds and logs from before the window opened. See [Compared with Unity's Console](#compared-with-unitys-console).

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

There are several Editor console replacements. Enhanced Console is a free one that adds sections, regex, search in stack traces, time since startup or frame number, and range copying beside Unity's own Console, rather than replacing it. See [Compared with other tools](#compared-with-other-tools).

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
| A regex section matches by name instead | Look for an `[EnhancedConsole]` warning about the pattern. An invalid regex falls back to the section name. |
| Clicking a log doesn't select a GameObject | Pass a GameObject or Component as the log's context, and check that the object is still loaded |
| A log opens the wrong file | Check whether a custom logger wrapper is the first frame with a file in the stack trace |
| Double-click or Enter does nothing | The stack trace has no file that exists on this machine, for example a log from a build machine, or a log without a stack trace |
| A stack trace line isn't clickable | The file must exist. Project paths (`Assets/`, `Packages/`) and absolute paths, such as code in a local package, are clickable. Unity's own source paths aren't. |
| Clicking a background-thread log doesn't select its context object | Context objects can only be read on the main thread |

## Limitations

- Logs aren't kept after closing the window or restarting Unity.
- The log handler wrapper adds a `LogContextHandler` frame to stack traces, so double-clicking a log in Unity's own Console can open that file instead of your code.
- Logs aren't captured from device builds.

## Project status

Enhanced Console is a pre-release package at version `0.3.3`. Its API and behavior may change before `1.0.0`. Not yet used in a shipped game.

Until `1.0.0`, minor versions (`0.x.0`) can include breaking changes, and patch versions (`0.x.y`) don't. Breaking changes are marked **Breaking** in the [changelog](CHANGELOG.md).

There's no public roadmap yet. Known gaps are listed under [Limitations](#limitations), and planned work is tracked in [issues](https://github.com/sgonzalezrozo/unity-enhanced-console/issues).

## Support

Enhanced Console is maintained by one person in their spare time, so replies are best effort.

| Need | Where |
|---|---|
| A bug | Open a [bug report](https://github.com/sgonzalezrozo/unity-enhanced-console/issues/new?template=bug_report.yml) with your Unity version, Editor platform and steps to reproduce |
| A feature idea | Open a [feature request](https://github.com/sgonzalezrozo/unity-enhanced-console/issues/new?template=feature_request.yml) |
| A question | Open an [issue](https://github.com/sgonzalezrozo/unity-enhanced-console/issues) and describe what you're trying to do |
| A security problem | Follow the [security policy](.github/SECURITY.md). Don't open a public issue. |

When something looks wrong, check the Console for messages starting with `[EnhancedConsole]`, and include them in your report.

## Contributing

Bug reports and pull requests are welcome. Please open an issue before starting a large change. See the [contributing guide](.github/CONTRIBUTING.md) and [security policy](.github/SECURITY.md).

## Related packages

| Package | Purpose |
|---|---|
| [Fast IoC](https://github.com/sgonzalezrozo/unity-fast-ioc) | Code-generated dependency injection |
| [Screen System](https://github.com/sgonzalezrozo/unity-screen-system) | Screens built from reusable layers |
| [UI Tweens](https://github.com/sgonzalezrozo/unity-ui-tweens) | ECS-based UI animation |
| [UI Scroller](https://github.com/sgonzalezrozo/unity-ui-scroller) | Virtualized lists |

Enhanced Console has no dependencies, so it doesn't need [Tools Core](https://github.com/sgonzalezrozo/unity-tools-core).

## License

Released under the [MIT License](LICENSE).
