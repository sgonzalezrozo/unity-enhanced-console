# Comparison

[← Back to README](../README.md)

- [Compared with Unity's Console](#compared-with-unitys-console)
- [Compared with other tools](#compared-with-other-tools)

---

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
