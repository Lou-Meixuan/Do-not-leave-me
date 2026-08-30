# Agent Guide

> Last verified 2026-08-30. Paths in this file were checked against the working tree on that date.

## Project Layout

- The repository root contains `AGENTS.md`, `openspec/`, `.agents/`, and the Unity project folder.
- `Do not leave me/` is the Unity project root. **Note the spaces in the folder name** — quote it in every shell command.
- Runtime scripts, scenes, prefabs, and assets are under `Do not leave me/Assets/`.
- Unity package dependencies are defined in `Do not leave me/Packages/manifest.json`.
- Unity editor configuration is under `Do not leave me/ProjectSettings/`. Unity version is 2022.3.62f3c1.
- The Wwise project is `Do not leave me/UnityProject_WwiseProject/` (the folder name is historical; it is not a Unity project).
- Do not manually edit generated Unity directories such as `Do not leave me/Library/`, `Temp/`, `Logs/`, or `UserSettings/`.

**For anything more detailed than the above, read `Do not leave me/Assets/DoNotLeaveMe/Docs/00-目录结构.md` first.**
That document is the authority on the `Assets/` layout, naming rules, the four places that must be updated by hand when
an asset is renamed, and the archiving rules. It is kept current; this file is not a substitute for it.

The short version of what that document says:

- `Assets/DoNotLeaveMe/` — gameplay logic: levels, scripts, prefabs, rendering settings, docs.
- `Assets/Art/` — the art library: models, textures, UI images, lighting, VFX, video.
- `Do not leave me/_ArchivedAssets/` — retired content. It sits **outside** `Assets/` so Unity does not import it. Never delete;
  move things here instead.

## Two Git Repositories — Read Before Committing

There are two nested Git repositories tracking overlapping files:

- The **repository root** (`Do-not-leave-me/`) is the real one. It has `origin`
  (`https://github.com/Lou-Meixuan/Do-not-leave-me.git`) and tracks the Unity project, `openspec/`, and `.agents/`.
- `Do not leave me/.git` is a **redundant duplicate** created during the 2026-08-30 cleanup. It has no remote.

Run `git rev-parse --show-toplevel` before committing and make sure you are in the repository root, not the inner one.
Committing to the inner repository publishes nothing. Consolidating the two is an open task.

## OpenSpec Workflow

- OpenSpec configuration and change artifacts live in `openspec/`; merged capability specs live in `openspec/specs/`.
- Shared OpenSpec agent skills live in `.agents/skills/`; keep them tool-agnostic.
- For a feature or behavior change, create and review an OpenSpec proposal before implementation.
- Keep implementation tasks and acceptance criteria in the relevant OpenSpec change artifacts.
- **The `openspec` CLI is not installed on this machine** (`openspec: command not found`). The skills in `.agents/skills/`
  describe CLI commands; when a command is unavailable, follow the manual fallback each skill documents. For archiving that
  means: merge the change's delta specs into `openspec/specs/<capability>/spec.md`, then
  `mv openspec/changes/<name> openspec/changes/archive/YYYY-MM-DD-<name>`.
- Delta specs use `## ADDED Requirements`; merged specs use `## Requirements` under a `# <capability> Specification` title.
  Spec files use CRLF line endings — preserve them.
- Archived change directories keep their original names, including outdated `formal-` and `superbreadman-` prefixes. Do not
  rename them; they are historical records that must match the commits that produced them.

## Unity Changes

- Preserve Unity `.meta` files alongside moved, added, or removed assets.
- Avoid unrelated scene or asset reserialization.
- Asset references use GUIDs, so renaming does not break them — but scene paths in
  `ProjectSettings/EditorBuildSettings.asset`, scene-name strings serialized inside scenes, string constants in C#
  (`SceneManager.LoadScene`, `Resources.Load`, lookups by object name), and paths under `Resources/` all work by name.
  See section 5 of `00-目录结构.md` before renaming anything.
- Verify C# changes in the Unity Editor or with the project's supported Unity test workflow when available.
- Close the Unity Editor before moving or archiving assets from a shell.
