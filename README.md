# Do Not Leave Me

> A 3D top-down two-character puzzle game · Unity 2022.3 (URP) · Wwise audio

*"Together you are safe — but you have to separate to move forward."*

A human and a dog are trapped in an abandoned containment hospital. **Together**, they are safe but lose their individual abilities. **Apart**, they can solve the mechanisms — but anxiety builds, and with it come darkness and hallucinations. The entire game is built on that tension: safety or capability, never both.

---

## Contents

- [Core Mechanics](#core-mechanics)
- [Controls](#controls)
- [Requirements](#requirements)
- [Getting Started](#getting-started)
- [Repository Layout](#repository-layout)
- [Levels & Scenes](#levels--scenes)
- [Systems Overview](#systems-overview)
- [Documentation](#documentation)
- [Development Conventions](#development-conventions)
- [License](#license)

---

## Core Mechanics

| Mechanic | Description |
|----------|-------------|
| **Proximity** | Both characters inside the *safety radius* count as **together**; outside it they are **separated** |
| **Together** | No anxiety growth; enables cooperative actions (pushing heavy crates, two-person pressure plates, heavy mechanism doors), but individual abilities are locked |
| **Separated** | Individual abilities unlock and puzzles become solvable, but anxiety rises continuously |
| **Human abilities** | Operate switches and valves; pick up and place small items |
| **Dog abilities** | Sprint; visualize footprints on the floor to predict monster patrol routes |
| **Anxiety** | As it rises the vignette closes in, scene lights die out, and auditory/visual hallucinations appear. A full bar resets the current level |
| **Monsters** | Present only in patrol rooms. Contact means capture — capture resets that room segment, not the whole level |

The loop: **solve (separated) → anxiety builds → reunite to recover → advance.** Players must plan their separation routes and budget how long they stay apart, finishing each puzzle before the anxiety bar fills.

Full design in [`02-核心玩法.md`](Do%20not%20leave%20me/Assets/DoNotLeaveMe/Docs/02-核心玩法.md).

---

## Controls

| Key | Action |
|-----|--------|
| `W A S D` / Left stick | Move |
| `Space` | Jump |
| `Tab` | Switch active character (Human ⇄ Dog) |
| `Q` | Enter / exit linked (two-person) mode |
| `E` | Pick up / drop item (Human only) |
| `F` | Trigger switch, hold valve; attach crate in linked mode |
| `Left Shift` | Sprint (Dog only, while separated) |
| `Esc` | Pause menu |
| `F3` | Developer info panel |

---

## Requirements

| Item | Version / Notes |
|------|-----------------|
| Unity | **2022.3.62f3c1** — use this exact version to avoid asset reserialization |
| Render pipeline | Universal RP 14.0.12 |
| Audio middleware | **Audiokinetic Wwise** — project at `Do not leave me/UnityProject_WwiseProject/` |
| Key packages | ProBuilder 5.2.4, TextMeshPro 3.0.9, Timeline 1.7.7, Test Framework 1.1.33 |
| Platform | Windows (PC standalone) |

> **On Wwise:** the SoundBanks under `Assets/StreamingAssets/Audio/` are generated from the Wwise project. If audio is missing, regenerate SoundBanks in Wwise Authoring rather than editing the generated output by hand.

---

## Getting Started

```bash
git clone https://github.com/Lou-Meixuan/Do-not-leave-me.git
cd Do-not-leave-me
```

1. Open `Do not leave me/` with **Unity Hub** using 2022.3.62f3c1. Note the spaces in the folder name — quote it in any shell command.
2. The first import takes a while to build `Library/`. Let it finish.
3. Open `Assets/DoNotLeaveMe/Levels/Start.unity` and press Play, or open any `Level_XX.unity` to debug a level in isolation.
4. If there is no sound, verify the Wwise integration is initialized and the SoundBanks have been generated.

---

## Repository Layout

```
Do-not-leave-me/
├─ AGENTS.md                       # Guide for agents and collaborators — read before changing anything
├─ openspec/                       # OpenSpec specifications and change proposals
│  ├─ specs/                       # Merged capability specs
│  └─ changes/                     # In-flight and archived changes
├─ .agents/skills/                 # Tool-agnostic collaboration skills
└─ Do not leave me/                # Unity project root
   ├─ Assets/
   │  ├─ DoNotLeaveMe/             # Gameplay logic
   │  │  ├─ Levels/                # Scenes, animations, baked meshes
   │  │  ├─ Scripts/               # Runtime and editor scripts
   │  │  ├─ Prefabs/  Materials/  Shaders/  UI/  Rendering/
   │  │  └─ Docs/                  # Design and implementation docs (authoritative)
   │  ├─ Art/                      # Art library: characters, environment, lighting, VFX, UI, video
   │  ├─ Resources/                # Runtime assets loaded by name
   │  └─ StreamingAssets/Audio/    # Wwise SoundBanks
   ├─ UnityProject_WwiseProject/   # Wwise project (historical name; not a Unity project)
   ├─ Packages/manifest.json
   ├─ ProjectSettings/
   └─ _ArchivedAssets/             # Retired content, outside Assets/ so Unity does not import it
```

---

## Levels & Scenes

The build list (`ProjectSettings/EditorBuildSettings.asset`) follows the play order:

```
Start → Cutscene_Intro → Persistent
      → Level_01 → Level_02 → Level_03 → Level_04 → Level_04A → Level_04B → Level_05
      → Cutscene_End
```

The `SharedArt_*` scenes (`L01_L02`, `L02_L03`, `L03_L04`, `L04_L04B`, `L04B_L05`) hold art shared between adjacent levels and are loaded additively, so transition areas are built once rather than duplicated.

Room-by-room breakdowns live in [`04-场景清单.md`](Do%20not%20leave%20me/Assets/DoNotLeaveMe/Docs/04-场景清单.md).

---

## Systems Overview

| Directory | Responsibility |
|-----------|----------------|
| `Scripts/Core/` | `GameManager` (anxiety value, level reset, flow control), `PlayerManager` (character switching and linked mode), `CameraFollow`, `SceneLoader`, `CutscenePlayer`, anxiety post-processing |
| `Scripts/Player/` | `PlayerController` — movement, jump, sprint, pickup and switch interaction |
| `Scripts/Puzzle/` | `PuzzleSwitch`, `HoldSwitch`, `PressurePlate`, `PushableBox`, `GateController`, `SequenceGateController`, `PickupItem`, `Checkpoint` |
| `Scripts/Enemy/` | Monster patrol and pursuit |
| `Scripts/Anxiety/` | Anxiety state data |
| `Scripts/Audio/` | Wwise routing for music, death, and ambience |
| `Scripts/UI/` | HUD, pause menu, settings, death screen, tutorial popups, reading-archive system |
| `Scripts/Editor/` | Level-building and asset-audit tools (door generation, mechanism audits, mesh-collider sync checks, and more) |

Anxiety implementation: while separated, the bar fills in roughly 10 seconds. Past the 70% threshold the scene lights go out and lighting switches to a spotlight above each character, producing real 3D shadows.

---

## Documentation

`Do not leave me/Assets/DoNotLeaveMe/Docs/` is the authoritative source for design and implementation. The documents are written in Chinese.

| Document | Contents |
|----------|----------|
| `00-目录结构.md` | **Read first** — actual directory structure, naming rules, the four places to update by hand when renaming, archiving rules |
| `01-项目规范.md` | Naming conventions and registries (sections 2–4 are prototype-era and obsolete) |
| `02-核心玩法.md` | Game loop, proximity, abilities, monsters, anxiety |
| `02-新场景搭建指南.md` | Building a new scene |
| `03-关卡结构.md` | Room-stitching model and level types |
| `03-关卡参考-*.md` | Room-level design for the upper treatment ward and the underground containment block |
| `04-场景清单.md` | Implemented scenes, ability coverage matrix, mechanism-to-script mapping |
| `05-脚本说明与道具制作指南.md` | Script reference and prop authoring guide |
| `06-音效需求清单.md` · `08-音乐系统.md` | Audio requirements and the music system |
| `09-摄像头与提示镜头.md` | Camera and hint-shot rules |

---

## Development Conventions

- **Unity assets.** Always move, add, and delete `.meta` files alongside their assets. Avoid unrelated scene or asset reserialization. Close the Unity Editor before moving or archiving assets from a shell.
- **Renaming.** References use GUIDs, but these work by name: scene paths in `EditorBuildSettings.asset`, scene-name strings serialized inside scenes, C# string constants (`SceneManager.LoadScene`, `Resources.Load`, lookups by object name), and paths under `Resources/`. Read section 5 of `00-目录结构.md` before renaming anything.
- **Archive, don't delete.** Retired content moves to `_ArchivedAssets/`.
- **OpenSpec workflow.** Create and review an OpenSpec proposal before implementing a feature or behavior change. The `openspec` CLI is not installed on the current machine; use the manual fallback each skill in `.agents/skills/` documents. Spec files use CRLF line endings — preserve them.
- See [`AGENTS.md`](AGENTS.md) for the full guide.

---

## License

No license has been specified. Unless stated otherwise, all rights are reserved.

---

<sub>Do Not Leave Me · a game jam project · maintained at <a href="https://github.com/Lou-Meixuan/Do-not-leave-me">Lou-Meixuan/Do-not-leave-me</a></sub>
