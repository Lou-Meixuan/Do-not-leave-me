## Context

See `proposal.md` for motivation. Level 04B already owns the special long-corridor music, formal human and dog actors, a checkpoint, a crate-door trigger, and the physical exit into Level 05. `PlayerControl` and `PlayerActor` currently provide camera-relative Rigidbody locomotion and jumping; `CameraFollow` owns the normal camera; `DeathScreen` owns caught/anxiety death; the dog has locomotion and footstep audio but no identified bark event in the current Wwise work unit.

The parkour must therefore temporarily take ownership of existing actors and camera without creating a second player stack or changing the formal route catalog. The route must be authorable with placeholder geometry now and replaceable art later. Unity is 2022.3 and the project does not currently rely on Unity Splines, so the design avoids adding that dependency.

## Goals / Non-Goals

**Goals:**

- Represent the entire chase as deterministic authored data that can be tuned in the Inspector.
- Keep human input responsive while preventing Rigidbody drift away from the three-lane route.
- Give the dog its own safe trajectory and keep dog movement cosmetic to parkour success.
- Make the final left/right fiction work without maintaining two copies of the existing long corridor.
- Restore every temporarily overridden formal system at success, death, reset, and component disable.
- Provide primitive placeholder construction and validation so level design can iterate before final art.

**Non-Goals:**

- Slide input, slide animation, crouched collision bounds, or slide-only obstacles.
- Procedural or endless obstacle generation.
- Navmesh/A* pursuit during the parkour phase.
- Dog failure, dog lane controls, or dog collision with obstacles.
- Replacement of the existing Level 04B corridor, checkpoint, door, crate, music, or Level 05 transition.

## Decisions

### D1. Use a phase controller with explicit ownership

A Level 04B parkour controller owns a small state machine: `Inactive`, `Chase`, `FinalTurn`, `Walkout`, `Tutorial`, `Released`, and `Failed`. On entry it disables normal player input effects, actor switching, interactions, dog orbit following, and mouse camera control through explicit public ownership APIs. On every exit path it releases those systems idempotently.

Alternative considered: disabling the existing components directly. Rejected because disabling `PlayerControl`/`CameraFollow` hides restoration rules and makes death/restart ordering fragile.

### D2. Author route segments as local frames and waypoint chains

Each human segment provides a centerline start/end frame, three lane offsets, speed, decision-window distance, and outgoing turn metadata. Straight progress is a scalar distance; the human target is the segment frame plus the current interpolated lane offset. Turns use short authored waypoint/arcs and rotate the route frame and camera toward the next segment.

The dog uses a parallel list of authored waypoints per segment. Its mover advances by distance and drives the existing external dog locomotion state. Dog waypoints are placed on flat, visually clear ground and may pass beneath future slide clearances because the dog never changes pose or participates in parkour collision.

Alternative considered: physics steering plus NavMesh/A* for both actors. Rejected because fixed turns, close walls, additive scene timing, and decorative obstacle changes would make the chase nondeterministic and could reproduce current monster-navigation coupling.

### D3. Keep vertical physics, own horizontal placement

During chase, the controller owns human X/Z route placement and facing while `PlayerActor.Jump()` and Rigidbody gravity continue to own Y. Horizontal velocity is cleared before route placement so the no-friction player material cannot accumulate drift. Lane transitions use configurable easing rather than teleporting.

Alternative considered: full kinematic movement. Rejected because it would require duplicating grounded and jumping behavior and could diverge from the existing actor animation state.

### D4. Separate obstacle lethality from near-miss timing

A replaceable obstacle root exposes a lethal trigger and an upstream near-miss gate. The obstacle tracks only the formal human actor. A direct lethal entry fails immediately. A near miss is emitted only if the human leaves the evaluation zone alive and the last safe action occurred inside a configured late interval; merely passing close to decorative bounds is insufficient.

Primitive cubes provide the initial obstacle visuals/colliders. Final art can replace children while retaining the semantic volumes and obstacle component.

Alternative considered: calculating distance from renderer bounds. Rejected because art replacement would change difficulty and produce unclear near-miss judgments.

### D5. Model the monster as chase presentation, not navigation

The runner maintains normalized proximity in a bounded range. Near misses add pressure; clean time decays it toward baseline. A monster presentation anchor maps proximity to a point behind the human route and controls visible/audio intensity. Reaching the lethal limit is not itself death in this first version; direct impacts and failed turns remain the unambiguous death causes.

The dog owns an Inspector-assigned Wwise bark event reference. Crossing the warning threshold starts cooldown-limited bark requests. Because no bark event is currently present in the inspected Wwise work unit, code must tolerate an unassigned event with one diagnostic warning; audio authoring/assignment is required before the bark acceptance criterion can be signed off.

Alternative considered: running `MonsterPatrol` through the parkour geometry. Rejected because its room bounds, line of sight, and A* pathing are designed for free-roaming gameplay rather than a cinematic lane chase.

### D6. Merge the final choice before the existing corridor

The final turn accepts either direction. It plays a direction-specific short human/dog/camera path and sends the monster presentation toward the opposite branch. Both short paths terminate at one merge anchor immediately before the existing long corridor. At the merge, actors are normalized to the same transforms; no duplicate corridor or additive scene is required.

Alternative considered: two physical corridors joined later. Rejected because it doubles art/collider maintenance while producing no downstream gameplay difference.

### D7. Use an authored walkout and tutorial handoff

After merge, the controller changes actor speed/animation intent from running to walking, follows a short walkout waypoint chain, and blends the camera to a calm framing. At the final marker it requests a one-page tutorial message containing `怪物似乎追丢了`. Once the tutorial handoff completes, the controller restores normal `PlayerControl`, dog following, and `CameraFollow`, activates the post-parkour checkpoint if configured, and becomes inert.

Alternative considered: releasing controls immediately at the merge. Rejected because players could turn back into the branch illusion and would not receive a clear chase-ending beat.

### D8. Scene authoring remains data-driven and self-validating

Level 04B receives one `L04B_ParkourRoot` containing segment markers, dog routes, branch/merge/walkout anchors, monster presentation, placeholder obstacles, and the controller. An editor validator reports missing references, non-contiguous segment endpoints, invalid lane widths, obstacles without semantic volumes, dog paths crossing lethal bounds, and a handoff point that is not aligned with the existing corridor.

## Risks / Trade-offs

- [Horizontal route placement fights Rigidbody collision resolution] -> Clear horizontal velocity, use one movement owner, move in the physics loop, and keep architecture colliders outside the valid lane envelope.
- [Existing scripts continue accepting input during chase] -> Add explicit parkour ownership/lock APIs and cover acquisition/release with play-mode tests.
- [Dog route clips after final art replacement] -> Keep dog paths visible as gizmos and validate them against semantic obstacle volumes, not render meshes.
- [Branch merge produces a visible snap] -> Place direction-specific merge approach markers with identical final position/facing and blend camera before normalization.
- [Near-miss tuning feels arbitrary] -> Expose timing/distance values per obstacle and visualize lethal/evaluation volumes in scene gizmos.
- [Bark cannot be heard until Wwise content exists] -> Keep runtime integration assignment-driven, warn once when missing, and track Wwise authoring/assignment as an explicit verification task.
- [Death or scene unload leaves controls locked] -> Centralize cleanup in idempotent release logic invoked by success, failure, reset, disable, and scene lifecycle paths.

## Migration Plan

1. Add runtime ownership APIs and parkour components without placing them in a production scene.
2. Add edit/play-mode coverage for lane bounds, corner decisions, obstacle filtering, pressure recovery, final merge, and cleanup.
3. Author `L04B_ParkourRoot` with primitives and align its merge/walkout path to the current Level 04B corridor entrance.
4. Wire the dog route, bark event slot, monster presentation, tutorial message, death flow, and reset lifecycle.
5. Validate the full route from Level 04 entry through the existing Level 04B door/crate exit.

Rollback removes or disables `L04B_ParkourRoot` and its new runtime components; because the existing corridor and route catalog are preserved, Level 04B can revert to its current spawn and traversal behavior without asset migration.

## Open Questions

- The exact Wwise dog warning-bark event name and audio asset remain an audio-authoring choice; the programming contract is an Inspector-assigned event with cooldown and missing-assignment diagnostics.
