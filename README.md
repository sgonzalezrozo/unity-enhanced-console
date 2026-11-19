<div align="center">

# Unity Enhanced Console

<sub>DESIGNED & BUILT BY</sub><br>
**✦ [`SANTIAGO GONZÁLEZ ROZO (@sgonzalezrozo)`](https://github.com/sgonzalezrozo) ✦**

A Unity Editor console for faster log triage: hide logs by system, search messages and stack traces, see when each log happened and copy log ranges. Editor-only, no logging API needed.

[![Unity](https://img.shields.io/badge/Unity-6000.3%20tested-black?style=for-the-badge&logo=unity)](Documentation~/getting-started.md#requirements)
[![Version](https://img.shields.io/badge/version-0.3.8-blue?style=for-the-badge)](CHANGELOG.md)
[![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)](LICENSE)

[**Installation**](#installation) • [**Key Features**](#key-features) • [**Quickstart**](#quickstart) • [**Section Filtering**](Documentation~/section-filtering.md) • [**Toolbar & Shortcuts**](Documentation~/toolbar-and-shortcuts.md) • [**Comparison**](Documentation~/comparison.md)

<sub>[Getting Started](Documentation~/getting-started.md) • [Features](Documentation~/features.md) • [How It Works](Documentation~/core-concepts.md) • [Troubleshooting](Documentation~/troubleshooting.md) • [FAQ](Documentation~/faq.md) • [Project Status](Documentation~/project-status.md)</sub>

</div>

---

## Installation

1. Open **Window > Package Manager** in Unity.
2. Click **`+`** in the top-left corner and select **Add package from git URL...**
3. Paste the URL below and click **Add**:

```text
https://github.com/sgonzalezrozo/unity-enhanced-console.git
```

It has no dependencies. Requirements: [Getting Started](Documentation~/getting-started.md#requirements).

---

## Key Features

* **[Sections](Documentation~/section-filtering.md)**: group logs by system and hide the ones you don't need, by name or by regex.
  Sections can match the message, the stack trace or both, so a namespace or folder is filtered without tagging every log.
* **[Search everywhere](Documentation~/features.md#find)**: plain text in messages and stack traces, with the match highlighted, plus log level toggles and Collapse.
* **[A time on each log](Documentation~/features.md#read)**: time since startup, wall-clock time or frame number.
* **[Act on logs fast](Documentation~/features.md#act)**: open the source from any stack trace line, select a log's context object, copy a range with or without stack traces, and delete logs you're done with.
* **[Stays fast with 10,000 logs](Documentation~/features.md#performance)**: new logs are filtered as they arrive, and only visible rows are drawn.

| Everything, all at once | With the `Networking` section hidden |
|---|---|
| `[Networking] Packet 1042 received` | |
| `[Networking] Packet 1043 received` | |
| `[Shop] Purchase failed: not enough coins` | `[Shop] Purchase failed: not enough coins` |
| `[Networking] Packet 1044 received` | |

---

## Quickstart

1. Open **Window > Enhanced Console** and keep it docked: logs written before it opens aren't shown.
2. Enter Play Mode or trigger some logs. Search, filter and collapse from the toolbar.
3. Double-click a log, or click a linked stack trace line, to open the source.

To hide logs by system, tag them, run **Tools > Enhanced Console > Setup**, and turn on **SL**:

```csharp
Debug.Log("[Networking] Packet 1042 received"); // matches a section named "Networking"
```

<details>
<summary><strong>View full quickstart</strong></summary>

1. Open **Window > Enhanced Console**.
2. Enter Play Mode or trigger some logs.
3. Use the toolbar to search, filter and collapse entries.
4. Turn on **Stack Trace** and select a log to see its stack trace.
5. Double-click a log, or click a linked stack trace line, to open the source file.
6. Click a log written with a context object, such as `Debug.Log(message, gameObject)`, to select that GameObject. A Component selects its GameObject, and an asset is pinged in the Project window.
7. Run **Tools > Enhanced Console > Setup** to create the section config, turn on **SL**, and choose **Sections...** from the arrow beside it to hide a section's logs.

```csharp
using UnityEngine;

public class ShopDebug : MonoBehaviour
{
    private void Start()
    {
        // Matched by message: a section named "Shop".
        Debug.Log("[Shop] Opened");

        // Also matched with no tag, by stack trace: a section named "ShopDebug" (this class)
        // with filter mode Only Stack or Stack And Content.
        Debug.LogWarning("Price list is empty");

        // Clicking this log selects the GameObject.
        Debug.Log("[Shop] Purchase failed: not enough coins", gameObject);
    }
}
```

</details>

The full walkthrough is in [Getting Started](Documentation~/getting-started.md).
