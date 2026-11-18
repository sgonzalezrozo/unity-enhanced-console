# How It Works

[← Back to README](../README.md)

- [How log capture works](#how-log-capture-works)
- [Architecture](#architecture)

---

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
