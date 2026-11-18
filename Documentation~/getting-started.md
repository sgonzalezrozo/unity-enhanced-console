# Getting Started

[← Back to README](../README.md)

- [Requirements](#requirements)
- [Installation](#installation)
- [Quick start](#quick-start)
- [Next steps](#next-steps)

---

## Requirements

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

## Quick start

1. Open **Window > Enhanced Console**.
2. Enter Play Mode or trigger some logs.
3. Use the toolbar to search, filter and collapse entries.
4. Turn on **Stack Trace** and select a log to see its stack trace.
5. Double-click a log, or click a linked stack trace line, to open the source file.
6. Click a log written with a context object, such as `Debug.Log(message, gameObject)`, to select that GameObject. A Component selects its GameObject, and an asset is pinged in the Project window.

Logs written before the window opens are not shown, so keep it docked.

## Next steps

- [Section Filtering](section-filtering.md): hide logs by system
- [Toolbar & Shortcuts](toolbar-and-shortcuts.md): every control, display option and key
- [Features](features.md): everything the window does, in detail
