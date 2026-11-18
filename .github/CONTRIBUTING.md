# Contributing to Enhanced Console

Thanks for helping out. Bug reports, documentation fixes and pull requests are welcome.

## Before you start

- Search the [existing issues](https://github.com/sgonzalezrozo/unity-enhanced-console/issues) first.
- Open an issue before starting a large change, so we can agree on the approach.
- Keep each pull request focused on one change.

## Set up

1. Create a Unity 2022.3 or newer project.
2. Clone this repository.
3. In **Window > Package Manager**, select **+ > Add package from disk** and choose this repository's `package.json`.

## Make a change

- Follow the [C# standards](https://github.com/sgonzalezrozo/unity-architecture-agents/blob/main/architecture/CSHARP_STANDARDS.md) this package uses. In short:
  - `camelCase` fields without an underscore, `ALL_CAPS` constants, explicit types instead of `var`.
  - Methods of 15 lines at most, classes of 200 lines at most, and 3 parameters at most.
  - No LINQ or lambdas, and `for` loops over lists.
  - A short `///` summary on each class, and no `///` on members. Other comments only for what the code can't say.
- Put new code in the folder for its area. The [Architecture](../Documentation~/core-concepts.md#architecture) section in How It Works lists them.
- Don't leave internal notes in comments, such as planning phases or old file locations.
- Update the matching guide in `Documentation~/` when behavior changes, and `README.md` if the change affects installation, the quickstart or the feature list.
- Add an entry to `CHANGELOG.md`, and mark breaking changes with **Breaking**.
- Unity needs a `.meta` file for every new file and folder in the package, so commit those too. Folders ending in `~` and folders starting with `.` don't need them.

## Test your change

This package has no automated tests yet. Check your change in the light and dark Editor themes, and across a script recompile.

Before a release, run through the whole window:

1. Log from a script and from a background thread, then check search, the log level toggles and Collapse.
2. Run **Tools > Enhanced Console > Setup**, turn on **SL** and switch sections on and off in **Sections...**.
3. Select logs with a click, Shift-click, Ctrl-click or Cmd-click, and the arrow keys, then copy with and without **Stack Trace**.
4. Open a log with a double-click and with Enter, from `Assets/` and from a local package.
5. Delete a log, then delete a collapsed group.
6. Break a script, press **Clear** and check that the compiler error stays.
7. Try each option in the window's ⋮ menu, and resize the stack trace panel.

In your pull request, list the Unity version, scripting backend and platform you tested.

## Release checklist

1. Update `version` in `package.json`.
2. Update the version in the README badge and in `Documentation~/project-status.md`.
3. Move the changelog entries under the new version.
4. Check that `llms.txt` still describes the features.
5. Create the matching tag, such as `v0.3.0`, and push it.
6. Create a GitHub Release from the tag, using the version's changelog entries as its notes.
