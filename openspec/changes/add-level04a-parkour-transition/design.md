## Context

Level 04 and Level 04B are already separate formal route entries. The working parkour is generated as `L04B_ParkourRoot` in Level 04B and temporarily owns the persistent human, dog, and camera. Both final choices currently reuse a common long-corridor handoff, but the visible spatial normalization feels artificial.

## Goals / Non-Goals

**Goals:**

- Preserve the working three-lane movement and both valid final choices.
- Give the chase a dedicated Level 04A scene.
- Hide shared-corridor normalization through darkness and occlusion.
- Use a real exit door and the existing route system for the following room.
- Keep the successor configurable, with Level 04B as the initial value.

**Non-Goals:**

- Rebuild Level 04 or Level 04B art.
- Add sliding or a slide animation.
- Duplicate the downstream room for left and right choices.
- Rename Level 04B or change its Level 05 exit.

## Decisions

### D1. Add a dedicated formal Level 04A route entry

The route becomes `Level_04 -> Level_04A -> Level_04B -> Level_05`. Level 04A receives its own scene, build-settings entry, route-catalog entry, respawn anchors, and exit binding. This keeps checkpoint and restart ownership aligned with scene boundaries.

### D2. Migrate generated chase content, not the downstream corridor

The builder targets Level 04A and creates a renamed `L04A_ParkourRoot`. Existing runtime components remain reusable. Level 04B's generated parkour root is removed, while all pre-existing Level 04B content remains untouched.

### D3. Conceal branch normalization in darkness

Left and right choices each lead into a short independent, unlit approach. Once the player is fully inside darkness and the camera has no readable landmarks, the controller normalizes human and dog transforms to one shared dark exit corridor. No Y-shaped physical merge is authored or visible.

### D4. Separate chase completion from route advancement

After normalization, the chase releases into a short guided walk and displays `怪物似乎追丢了`. Normal control resumes before the exit door. Opening/passing the door uses the existing route transition system; the parkour controller does not directly load the successor.

### D5. Keep the successor serialized

Level 04A initially targets `Level_04B`. The exit door/trigger stores the target as serialized scene data so a future room can replace Level 04B without changing parkour code.

## Risks / Trade-offs

- Additive route loading may expose a lighting flash -> keep the entire normalization zone unlit and hold the black presentation until actor placement is complete.
- Moving the builder could accidentally overwrite Level 04B -> use distinct scene/root constants and explicitly remove only the known generated root from Level 04B.
- A new route entry affects retained shared-art logic -> give Level 04A only the shared scenes it actually needs and validate predecessor/successor loading.
- The exit door asset may not suit final art -> use an existing physical-door contract with placeholder geometry that can be replaced later.

## Migration Plan

1. Create Level 04A and add it to build settings and the route catalog.
2. Retarget Level 04's successor to Level 04A.
3. Retarget and rename the parkour builder and generated root for Level 04A.
4. Add dark branch approaches, shared dark corridor, guided walk, and configurable door exit.
5. Remove only `L04B_ParkourRoot` from Level 04B.
6. Validate both final choices, death/retry, Level 04 entry, and downstream transition.

Rollback restores Level 04's successor and route catalog, removes Level 04A from build settings, and regenerates the previous Level 04B parkour root.
