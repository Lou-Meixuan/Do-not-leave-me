## Why

Level 04B currently begins as an ordinary traversable corridor and does not deliver the intended escape climax between Level 04 and the existing door-and-crate ending. The level needs a deterministic, prototype-friendly parkour sequence that gives the programmer a complete playable flow while allowing final art, obstacle models, and a slide animation to be added later.

## What Changes

- Add a human-controlled three-lane chase at the start of Level 04B with automatic forward running, discrete `A`/`D` lane changes, jumping, and context-sensitive left/right corner choices.
- Add authored straight segments and four-to-six authored turns using replaceable placeholder obstacles; sliding and slide-only obstacles are explicitly deferred.
- Resolve direct obstacle impacts as death and late-but-successful avoidance as a near miss that temporarily brings the pursuing monster closer.
- Keep the dog running on an authored safe companion route, exclude it from obstacle failure checks, and trigger dog warning barks as the monster closes in.
- Make the final junction accept either left or right while both inputs converge on the same existing Level 04B long corridor; fictionally, the monster takes the unchosen branch and loses the players.
- Transition from parkour control into a short camera-controlled walking sequence, show the tutorial message `怪物似乎追丢了`, then restore the current Level 04B free-movement flow leading to the door and crate.
- Preserve the existing Level 04B checkpoint, exit binding, crate trigger, corridor music lifecycle, and downstream transition.

## Capabilities

### New Capabilities

- `formal-level045-parkour-chase`: Defines the three-lane chase flow, authored turns and obstacles, near-miss pressure, dog companion behavior, converging final choice, and handoff into the existing Level 04B corridor.

### Modified Capabilities

None. The existing Level 04B art, traversal, pursuit, music, exit-door, and route-transition contracts remain valid outside the new parkour phase.

## Impact

- Adds Level 04B runtime components under `Assets/DoNotLeaveMe/Scripts/LevelRuntime/` and parkour prefabs or scene-authored roots under `Assets/DoNotLeaveMe/Levels/`.
- Temporarily overrides formal player movement, camera control, and dog following while the parkour phase is active, then restores the existing systems.
- Reuses the existing `DeathScreen` lifecycle for impacts and the existing Wwise integration for dog vocal feedback where an appropriate event is available.
- Extends `Level_04B.unity` and/or `L04B_Content.prefab` with replaceable primitive obstacle placeholders and authored human/dog route markers.
- Does not require a new scene, change the formal route catalog, or implement a slide animation in this change.
