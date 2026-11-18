# Section Filtering

[← Back to README](../README.md)

Section filtering is optional. It lets you hide logs from selected systems while everything else stays visible.

- [Set it up](#set-it-up)
- [How logs match a section](#how-logs-match-a-section)
- [Filter modes](#filter-modes)
- [Section recipes](#section-recipes)

---

## Set it up

1. Run **Tools > Enhanced Console > Setup**. This creates `Assets/EnhancedConsole/SelectiveLogsConfig.asset` with five default sections, or moves an existing config there.
2. Select the asset to rename sections, change colors, add your own or turn on regex.
3. Turn on the **SL** button in the console toolbar.
4. Select the arrow beside **SL**, choose **Sections...**, then clear a section to hide its logs.

## How logs match a section

A log matches when the section name appears in its message or stack trace. Matching ignores case and spaces, so you don't need a special logging API:

```csharp
// Matches the default "ScreenSystem" section by message.
Debug.Log("[ScreenSystem] Opened Settings");
```

A log also matches if `ScreenSystem` appears in its stack trace, such as in a namespace or class name.

For more control, turn on **Regex** for a section and enter a pattern. Regex matching also ignores case.

The first matching section wins, and logs that match no section stay visible.

## Filter modes

Select the arrow beside the **SL** button, then choose a mode:

| Mode | Searches |
|---|---|
| Stack And Content | The message and stack trace |
| Only Stack | The stack trace only |
| Only Content | The message only |

## Section recipes

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
