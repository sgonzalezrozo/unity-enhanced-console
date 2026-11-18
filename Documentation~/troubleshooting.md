# Troubleshooting & Support

[← Back to README](../README.md)

- [Troubleshooting](#troubleshooting)
- [Limitations](#limitations)
- [Support](#support)

---

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

There's no public roadmap yet. Planned work is tracked in [issues](https://github.com/sgonzalezrozo/unity-enhanced-console/issues).

## Support

Enhanced Console is maintained by one person in their spare time, so replies are best effort.

| Need | Where |
|---|---|
| A bug | Open a [bug report](https://github.com/sgonzalezrozo/unity-enhanced-console/issues/new?template=bug_report.yml) with your Unity version, Editor platform and steps to reproduce |
| A feature idea | Open a [feature request](https://github.com/sgonzalezrozo/unity-enhanced-console/issues/new?template=feature_request.yml) |
| A question | Open an [issue](https://github.com/sgonzalezrozo/unity-enhanced-console/issues) and describe what you're trying to do |
| A security problem | Follow the [security policy](../.github/SECURITY.md). Don't open a public issue. |

When something looks wrong, check the Console for messages starting with `[EnhancedConsole]`, and include them in your report.
