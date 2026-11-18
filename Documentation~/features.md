# Features

[← Back to README](../README.md)

- [Capture](#capture)
- [Find](#find)
- [Read](#read)
- [Act](#act)
- [Performance](#performance)

---

## Capture

- **Every log, from every thread.** Logs are read through `Application.logMessageReceivedThreaded`. Logs from background threads are queued and shown on the next Editor update, with the time they were written.
- **Compiler errors and warnings** come straight from the compilation pipeline, so they appear even when Unity doesn't send them as regular logs. Each assembly's results replace its previous ones, and a message that also arrives as a regular log is listed once.
- **Logs survive domain reloads** while the window is open: after a recompile and when entering Play Mode. Up to 10,000 logs are kept; the oldest are dropped in chunks of 1,000.
- **Context objects.** A log written with a context, such as `Debug.Log(message, gameObject)`, remembers that object.

## Find

- **Search** for plain text in messages and stack traces, ignoring case. The first match in each visible message is highlighted, except in messages with rich text tags.
- **Log level toggles** for info, warnings and errors, each with its count. Exceptions and failed asserts count as errors.
- **Sections** group logs by system and hide the ones you don't need:
  - A section matches by name, ignoring case and spaces, or by a regex.
  - Choose whether sections search the message, the stack trace or both, so a namespace or folder can be matched without tagging every log.
  - Sections are saved per project in a ScriptableObject, each with a color. While section filtering is on, matching rows show that color as a bar.
  - A popup turns sections on and off, with **Enable All** and **Disable All**. The toolbar shows how many are on, such as `SL (3/5)`.
  - An invalid regex logs a warning once and falls back to the section name.

  See [Section Filtering](section-filtering.md).
- **Collapse** groups logs with the same message and type and shows how many times each was logged. Optionally, the stack trace must match too.

## Read

- **Time column** showing time since startup (`mm:ss.mmm`), the local wall-clock time (`HH:mm:ss.fff`) or the frame number (`#1234`).
- **Display options:** 1 to 10 lines per log, a monospace font for messages and stack traces, and rich text on or off.
- **Stack trace panel** for the selected log, resizable by dragging. Lines with a file you can open are links.
- **Autoscroll** follows new logs while the list is at the bottom. Scrolling up pauses it, and scrolling back to the bottom resumes it.

## Act

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

## Performance

- The filtered list is cached, and new logs are filtered as they arrive instead of refiltering everything.
- Collapse groups logs with a lookup table, so turning it on with 10,000 logs doesn't freeze the Editor.
- Each log remembers which section it matched, and regexes are parsed once.
- Only the visible rows and stack trace lines are drawn.
- While logs arrive, the window repaints at most 30 times per second, and not at all when nothing changed.
