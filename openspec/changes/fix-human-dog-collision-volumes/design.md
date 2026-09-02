## Context

`PlayerActors.prefab` is the shared source for `FormalHumanActor` and `FormalDogActor`. Each actor has a root `Rigidbody`, while its `CapsuleCollider` and visual prefab are children of a uniformly scaled `Body`. Current serialized values therefore are not world-space values: the human capsule (`h=1.7`, `r=0.35`) is scaled by `0.7`, and the dog capsule (`h=1.07`, `r=0.37`, Z direction) is scaled by `1.5`. Both actor roots and collider objects are on `Default`, and the global collision matrix permits their contact, but `PlayerActors.Awake` overrides the matrix with a pair-specific `Physics.IgnoreCollision(..., true)` call.

## Goals / Non-Goals

**Goals:**

- Make each effective world-space capsule closely follow the loaded actor silhouette.
- Keep each capsule bottom aligned with the foot-pivot ground plane.
- Ensure human and dog resolve solid contact without visible body overlap or a conspicuous invisible gap.
- Keep one shared prefab authoritative across all Formal levels.

**Non-Goals:**

- Adding or repairing colliders on environment art.
- Reworking movement, follow behavior, navigation, animation, or physics layers.
- Replacing the capsule approach with per-bone, compound, or mesh colliders.

## Decisions

### Retain one capsule per actor

Capsules are stable for moving rigidbodies, inexpensive, and compatible with the existing grounding query. The human remains Y-oriented; the quadruped dog remains Z-oriented. Compound or animated colliders would add contact jitter and make gameplay depend on animation pose.

### Tune in effective world space

The acceptance target is the collider's `bounds`, not its raw Inspector values, because the `Body` scale changes the result. Tuning will use renderer bounds plus Scene-view/play-mode observation. Raw radius, height, and center will then be stored on the child collider so the desired world-space volume survives prefab reuse.

The capsule bottom must remain at the actor root's ground plane within a small tolerance. Lateral fit should cover the torso/core silhouette rather than extremities such as hands, tail, or animated legs; this avoids snagging while still preventing obvious actor overlap.

### Restore solid collision routing

No new actor layers or collision-matrix edits are required. `PlayerActors.Awake` explicitly restores pair collision rather than disabling it. Both colliders remain non-trigger and both actors remain dynamic rigidbodies with continuous collision detection set by `PlayerActor.Awake`.

### Route forced following through physics

`DogOrbitFollower` may continue calculating orbit/path targets in `Update`, but applies movement during `FixedUpdate` through `PlayerActor.Move`. This preserves its speed multiplier while allowing the rigidbody solver to resolve contact with the human and environment. Direct Transform position writes are prohibited for ordinary follow movement.

### Give dynamics only to controlled actors

During ordinary player switching, the active actor remains a dynamic Rigidbody and the inactive partner becomes kinematic. The kinematic partner retains its non-trigger collider and therefore blocks overlap, but cannot receive momentum and slide away. `PlayerControl` owns this state transition so Tab switching cannot leave both actors in the wrong mode.

Scripted phases that intentionally drive both actors (`ForceHumanOnly` dog following and parkour control) keep both rigidbodies dynamic. Returning to ordinary control reapplies the single-active ownership rule. `PlayerActor` avoids velocity writes while kinematic so checkpoint placement and stop calls do not generate physics warnings.

### Improve the audit before final tuning

`ActorBodyAudit` will report capsule orientation, effective bounds size/center, ground-bottom error, and model-versus-capsule gaps. This makes the scaled-child setup inspectable and gives later changes a repeatable regression check.

## Risks / Trade-offs

- Renderer bounds vary with animation pose; final values must be checked in more than one normal locomotion pose.
- A capsule fitted to every limb would snag on geometry, while a capsule fitted only to the torso permits limited limb overlap. The target is readable gameplay contact, not pixel-perfect physical anatomy.
- Tuning the shared prefab affects every Formal level, so verification must cover spawn/grounding and at least one constrained space in addition to direct human/dog contact.
- A moving dynamic actor can still be blocked by the inactive kinematic partner; this is intentional. Level design must leave enough room for the active actor to move around the partner.

## Verification

- Run `Tools/DoNotLeaveMe/Actor/Audit Body Fit` with both actors loaded and record effective bounds.
- In play mode, drive the human into the idle and moving dog from front, side, and rear; confirm stable separation without pass-through, launch, or persistent jitter.
- Repeat with the dog moving into the human and while both move toward each other.
- Confirm both remain grounded, can separate naturally, and still traverse representative doors/corridors.
- Confirm Unity compiles cleanly and the Console has no new errors.
