## Why

The parkour chase currently lives inside Level 04B and must visibly normalize two final branches into one corridor, making the transition feel artificial. A dedicated Level 04A scene can contain the chase and conceal branch normalization inside an unlit exit corridor while preserving Level 04 and Level 04B as independent rooms.

## What Changes

- Add `Level_04A` between `Level_04` and the configurable downstream room.
- Move the existing three-lane parkour assembly from Level 04B into Level 04A without rewriting its working movement, obstacle, dog, or branch controls.
- Make both final choices enter separate dark branch approaches, then invisibly normalize the actors into one unlit exit corridor.
- End parkour ownership before a physical exit door and allow that door to advance to a configurable successor, initially `Level_04B`.
- Change Level 04's exit target to Level 04A and add Level 04A to build settings and the formal route catalog.
- Remove the generated parkour root from Level 04B while preserving its existing corridor, crate, door, music, checkpoint, and Level 05 exit.

## Capabilities

### New Capabilities

- `formal-level04a-parkour-transition`: Dedicated chase scene, dark branch concealment, reusable exit corridor, and configurable door transition.

### Modified Capabilities

- `formal-level045-parkour-chase`: Relocate the existing chase implementation from Level 04B to Level 04A without changing its established control contract.

## Impact

- Adds `Assets/DoNotLeaveMe/Levels/Level_04A.unity` and its `.meta` file.
- Updates Level 04's serialized successor, `Persistent.unity` route catalog, build settings, and the parkour editor builder.
- Reuses the existing persistent actors, route loading, physical door transition, and parkour runtime components.
- Defaults Level 04A's successor to Level 04B but keeps it authorable for a future replacement room.
