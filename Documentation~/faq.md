# FAQ

[← Back to README](../README.md)

**About the project**
- [Is it right for my project?](#is-it-right-for-my-project)
- [Is it production-ready? Does anyone use it?](#is-it-production-ready-does-anyone-use-it)
- [Why was it built?](#why-was-it-built)
- [Is it free for commercial use?](#is-it-free-for-commercial-use)

**Usage**
- [How do I filter Debug.Log messages by system in Unity?](#how-do-i-filter-debuglog-messages-by-system-in-unity)
- [How do I hide noisy logs in the Unity Console?](#how-do-i-hide-noisy-logs-in-the-unity-console)
- [Can I copy several logs without their stack traces?](#can-i-copy-several-logs-without-their-stack-traces)
- [Enhanced Console vs Unity's Console?](#enhanced-console-vs-unitys-console)
- [Is there a better console for Unity?](#is-there-a-better-console-for-unity)

**Compatibility**
- [Does it show logs from device builds?](#does-it-show-logs-from-device-builds)
- [Does it add anything to my build?](#does-it-add-anything-to-my-build)
- [Does it work with Unity 6?](#does-it-work-with-unity-6)
- [Does it work with AI coding agents?](#does-it-work-with-ai-coding-agents)

---

## About the project

### Is it right for my project?

**Use Enhanced Console if you:**

- Work in a large codebase with noisy logs
- Want to group logs by system
- Spend a lot of time reading stack traces

**Consider something else if you:**

- Have a small project and Unity's Console is enough
- Need to view logs from device builds. Enhanced Console only runs in the Unity Editor.

### Is it production-ready? Does anyone use it?

Not yet. Enhanced Console is a pre-release package (`0.x`) by a single author. It isn't used in a shipped game, and it has no other users yet, so expect rough edges and API changes before `1.0.0`. Bug reports are very welcome. See [Project Status](project-status.md).

### Why was it built?

I started this console in part to copy a range of logs without their stack traces. Then I realized I also needed to filter logs in more ways than a search bar allows.

So I added a selective filter system next to the traditional search. Each project sets up its own filters in a ScriptableObject, and they can match the log, the stack trace or both.

I also thought it would be great to show a time on each log, as a simple measurement for developers while playing in the Editor.

I built and refined it on my own time in personal projects. In November 2026 I extracted it into this standalone package and documented it. Earlier history lives in a private repository.

### Is it free for commercial use?

Yes. It's MIT licensed.

## Usage

### How do I filter Debug.Log messages by system in Unity?

Create sections in Enhanced Console, named after a tag or namespace, or with a regex, then hide the sections you don't need. Logs need no special API. See [Section Filtering](section-filtering.md).

### How do I hide noisy logs in the Unity Console?

Put a tag in the message, such as `[Networking]`, create a section with that name, and clear it in the **SL** menu. Everything else stays visible. See [Section Filtering](section-filtering.md).

### Can I copy several logs without their stack traces?

Yes. Select a range, turn off **Stack Trace**, and press Ctrl+C or Cmd+C.

### Enhanced Console vs Unity's Console?

It runs beside Unity's Console and adds sections, regex, search in stack traces, time since startup or frame number, and range copying. Unity's Console still covers device builds and logs from before the window opened. See [Compared with Unity's Console](comparison.md#compared-with-unitys-console).

### Is there a better console for Unity?

There are several Editor console replacements. Enhanced Console is a free one that adds sections, regex, search in stack traces, time since startup or frame number, and range copying beside Unity's own Console, rather than replacing it. See [Compared with other tools](comparison.md#compared-with-other-tools).

## Compatibility

### Does it show logs from device builds?

No. It only runs in the Unity Editor.

### Does it add anything to my build?

No. The package is Editor-only.

### Does it work with Unity 6?

Yes. It's tested on Unity `6000.3`. The package declares `2022.3` as its minimum, but older versions aren't tested yet.

### Does it work with AI coding agents?

Not directly. Agents read Unity's Console through Unity MCP or batch-mode logs, so Enhanced Console doesn't change what they see. Plain `[Tag]` prefixes in messages make sections match, and they also make logs easier for agents to search.
