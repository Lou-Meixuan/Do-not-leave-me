## Context

The project already defines collision rules in `formal-level-collider-normalization`: visual-only small props should have no Collider, simple blocking models should use `BoxCollider`, irregular obstacles require reviewed compound or approved mesh collision, door frames should not block, and mechanisms separate trigger detection from physical blocking. Existing tools cover ground volumes and MeshCollider mesh-reference synchronization, but there is no audit for renderers that have no physical coverage at all.

The current sync auditor contains an early exit when `MeshCollider` is absent. Its `Missing` state means an existing MeshCollider has a null `sharedMesh`; it does not mean the rendered object lacks a Collider component. This naming gap explains why the current tool appears to check missing collision while still overlooking pass-through objects.

## Goals / Non-Goals

**Goals:**

- Find substantial visible objects that plausibly require collision but have none.
- Distinguish definite errors from candidates requiring designer judgment.
- Repair only explicitly approved objects and preserve prefab/scene ownership.
- Prefer stable, inexpensive primitive/compound collision over unnecessary MeshColliders.
- Verify that formal routes no longer pass through approved blocking geometry.

**Non-Goals:**

- Giving collision to textures, materials, decals, particles, VFX, or every MeshRenderer.
- Automatically converting all imported FBX files to `Generate Colliders`.
- Replacing the authoritative `GroundVolume` system with per-floor mesh collision.
- Adding blocking collision to door frames, trigger-only mechanisms, small clutter, or intentionally traversable decoration.

## Decisions

### Audit physical coverage, not component presence alone

For each active renderer candidate, the audit checks enabled, non-trigger Colliders on the rendered node, within its collision-owning prefab/scene hierarchy, and on an explicitly associated parent proxy. A trigger alone does not count as blocking coverage. Disabled Colliders are reported separately.

The report includes scene, hierarchy path, prefab asset path, layer, renderer bounds, Rigidbody presence, and coverage source. Results are classified as:

- `Covered`: valid blocking coverage exists;
- `IntentionalVisual`: excluded by an explicit rule or reviewed exception;
- `TriggerOnly`: detection exists but no blocking volume;
- `MissingCandidate`: substantial static rendered object with no blocking coverage;
- `ReviewRequired`: irregular, dynamic, nested-prefab, door/mechanism, or ambiguous ownership.

### Require explicit selection for mutation

The full-scene command is report-only. Repair commands operate only on current selection, show the number and paths of targets, use Undo, and never save scenes automatically. This prevents hundreds of accidental scene/prefab overrides.

### Use collision shape by physical role

- Simple rectangular static wall, cabinet, desk, or large prop: bounds-fitted non-trigger `BoxCollider`.
- Complex but static obstacle whose traversable silhouette matters: reviewed compound boxes or an approved non-convex `MeshCollider` with no Rigidbody.
- Dynamic Rigidbody object: primitive/compound convex collision; never an automatic non-convex MeshCollider.
- Floor: existing `GroundVolume` remains authoritative.
- Door: blocking belongs to the door leaf; frame/jamb remains non-blocking per the existing spec.
- Small decoration and VFX: no Collider.

### Preserve ownership

If every use of a shared model needs identical collision, repair the shared prefab. If collision is level-specific or belongs to architecture/layout, author it in the scene as a named proxy. Nested prefab changes use recorded overrides and never silently modify the source asset.

### Keep reviewed exceptions durable

The audit must support a project-owned exception record keyed by stable asset/scene path rather than instance ID. Each exception includes a reason such as visual-only, handled-by-parent-proxy, door-frame, ground-volume-owned, or intentionally traversable. Reports list stale exceptions whose targets no longer exist.

## Risks / Trade-offs

- Renderer bounds can overestimate rotated or concave geometry; automatic boxes are restricted to explicitly selected simple shapes.
- MeshCollider overuse increases physics cost and can snag actors; it remains a reviewed exception, not the default.
- Scene additive loading can split visual and collision ownership across scenes; the audit must inspect the complete loaded route set or report when required companion scenes are absent.
- Existing names are inconsistent, so name matching may help classification but cannot be the sole basis for mutation.

## Verification

- Run the report across every enabled Formal gameplay/shared-art scene pairing.
- Review every `MissingCandidate` and record either a Collider repair or an intentional exception.
- Confirm repaired simple obstacles use fitted boxes and irregular objects were not automatically boxed.
- Play-walk representative routes with both actors, including walls, large furniture, doors, corners, and shared-scene seams.
- Confirm no new invisible barriers, blocked doors, floor seams, collider jitter, or Console errors.
