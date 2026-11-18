# Toolbar & Shortcuts

[← Back to README](../README.md)

- [Toolbar](#toolbar)
- [Display options](#display-options)
- [Keyboard and mouse](#keyboard-and-mouse)

---

## Toolbar

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
| SL | Turn section filtering on or off. See [Section Filtering](section-filtering.md). |

## Display options

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
