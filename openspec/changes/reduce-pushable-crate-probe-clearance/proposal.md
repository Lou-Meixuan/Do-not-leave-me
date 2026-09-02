## Why

The pushable crate stops roughly a crate half-width before visible obstacles. `ProbeBlocked` casts an almost crate-sized box and also sweeps it by `projectedHalfExtent + blockProbeSkin`; because the cast shape already occupies the crate volume, adding the half extent a second time creates an approximately one-metre invisible clearance for the current 2.2 m crate. Reducing only the serialized skin from 0.12 m to 0.03 m would leave most of that false clearance intact.

## What Changes

- Calculate the forward BoxCast distance from the crate's next fixed-step displacement plus a 0.03 m safety skin.
- Set the shared crate prefab's `blockProbeSkin` to 0.03 m.
- Keep the cast volume slightly inset and continue excluding triggers, the crate hierarchy, and attached actors.
- Verify the crate stops close to walls and props without tunneling through them.

## Capabilities

### New Capabilities

<!-- None. -->

### Modified Capabilities

- `crate-push-movement`: Define a bounded obstacle-probe clearance that prevents both tunneling and visible air-wall stopping.

## Impact

- `Do not leave me/Assets/DoNotLeaveMe/Scripts/Level01/PushableCrate.cs`
- `Do not leave me/Assets/DoNotLeaveMe/Levels/Prefabs/L01_MovableStep_WoodenCrate.prefab`
- `Do not leave me/Assets/DoNotLeaveMe/Levels/Prefabs/L01_Content.prefab` (embedded Level 1 crate configuration)
- No changes to crate dimensions, movement speed, interaction points, wall Colliders, or unrelated scene files.
