## Why

The Formal human and dog have dynamic rigidbodies and non-trigger capsule colliders, but `PlayerActors.Awake` explicitly disables contact between the pair. The forced dog follower also writes `transform.position` directly, bypassing normal rigidbody collision resolution. Separately, both capsules live under differently scaled `Body` children (human `0.7`, dog `1.5`), which makes serialized capsule numbers misleading.

## What Changes

- Audit the effective world-space human and dog capsule dimensions against their loaded render bounds.
- Tune the capsule shape and center in the shared `PlayerActors.prefab` so each volume follows the actor's gameplay silhouette and remains grounded.
- Preserve solid human/dog contact: neither actor may pass through or visibly overlap the other during ordinary movement.
- Stop globally ignoring the human/dog collider pair and route forced dog following through rigidbody movement.
- Extend the existing actor body-fit audit so it reports enough world-space capsule data to catch regressions caused by `Body` scaling.
- Verify the shared prefab in representative gameplay without editing individual level scenes.

## Capabilities

### New Capabilities

<!-- None. -->

### Modified Capabilities

- `formal-actor-foot-pivot`: Tightens the existing body-scale/collider contract with explicit silhouette fit and human/dog contact requirements.

## Impact

- `Do not leave me/Assets/DoNotLeaveMe/Levels/Prefabs/PlayerActors.prefab`
- `Do not leave me/Assets/DoNotLeaveMe/Scripts/Editor/ActorBodyAudit.cs`
- `Do not leave me/Assets/DoNotLeaveMe/Scripts/LevelRuntime/PlayerActors.cs`
- `Do not leave me/Assets/DoNotLeaveMe/Scripts/LevelRuntime/DogOrbitFollower.cs`
- No level-scene edits, movement-speed changes, animation changes, or project-wide collision-matrix changes are intended.
- Missing colliders on environment objects are explicitly deferred to a second change after this one is verified.
