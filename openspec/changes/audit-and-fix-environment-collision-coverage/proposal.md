## Why

Some visible environment objects in Formal scenes have renderers but no blocking Collider, allowing the human and dog to pass through them. Textures and materials do not own physics; collision must be authored on the rendered GameObject, a parent proxy, or a separate scene-owned volume. The existing `MeshColliderSyncAuditor` only inspects nodes that already contain both a `MeshFilter` and a `MeshCollider`, so it cannot detect a truly missing Collider. Ground tooling covers floor support but does not validate walls, furniture, or other substantial obstacles.

## What Changes

- Add a report-only collision-coverage audit for loaded Formal scene hierarchies and selected prefab/model hierarchies.
- Report rendered objects with no enabled non-trigger Collider on themselves, their relevant parent proxy, or their child collision hierarchy.
- Classify candidates so visual-only decoration, trigger-only mechanisms, doors, floors, dynamic objects, and substantial static obstacles are not treated identically.
- Provide an explicit selection-based repair workflow for approved static obstacles:
  - bounds-aligned `BoxCollider` for simple rectangular objects;
  - no automatic conversion for irregular/concave objects, which require a reviewed compound collider or approved static `MeshCollider`;
  - preserve existing prefab ownership and record prefab overrides correctly.
- Validate every enabled Build Settings gameplay/shared-art scene and produce a durable report of fixed items and intentional exceptions.

## Capabilities

### New Capabilities

<!-- None. -->

### Modified Capabilities

- `formal-level-collider-normalization`: Extend collision normalization with missing-coverage discovery, classification, safe selected-object repair, and route-level validation.

## Impact

- `Do not leave me/Assets/DoNotLeaveMe/Scripts/Editor/MeshColliderSyncAuditor.cs` or a focused companion audit tool.
- Formal gameplay and shared-art scenes listed in `ProjectSettings/EditorBuildSettings.asset`.
- Approved shared model prefabs under `Assets/DoNotLeaveMe/Levels/Prefabs/SharedModels/` when collision ownership belongs to the prefab.
- No blanket FBX import-setting changes and no automatic Collider addition to every renderer.
