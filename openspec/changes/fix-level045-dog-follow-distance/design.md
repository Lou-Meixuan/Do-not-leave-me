## Context

`DogOrbitFollower` currently computes a continuously rotating destination at `orbitRadius` from the human and drives the dog toward it every physics step. Its `movementThreshold` only detects arrival at that moving destination, not arrival near the human. The desired transition boundary is the visible L05_Checkpoint carpet, but the current checkpoint completes when either actor touches it and the broader Level 5 entry seal can commit before the dog is safely on the carpet. That ordering can close the dog outside the door. Temporary Level 4.5 parkour ownership must also be released at the same dual-actor carpet commit or Tab switching can remain blocked.

## Goals / Non-Goals

**Goals:**

- Make the forced follower wait near the human instead of continuously orbiting or pushing into them.
- Resume following after the human creates meaningful separation.
- Keep forced follow active until both actors are simultaneously standing on the L05_Checkpoint carpet.
- Guarantee that the dual-actor checkpoint handoff clears forced-follow movement, speed overrides, and parkour-control ownership before restoring switching and closing the door.
- Preserve collision-aware Rigidbody movement and A* pathing while travel is required.

**Non-Goals:**

- Change manual dog controls, dog walk speed, parkour choreography, monster pursuit targeting, or actor collision configuration.
- Teleport either actor at the Level 5 handoff.
- Rebalance Level 4.5 encounter timing.

## Decisions

### Gate movement by horizontal human-dog distance

The follower will use horizontal distance to the human as the authoritative follow state. At or below a configurable stop distance, it will clear active movement, stop the dog, and report idle. This naturally covers physical contact without depending on collision callbacks, which can vary with collider shape and Rigidbody timing.

### Use hysteresis between stop and resume

The dog will resume travel only when distance exceeds a configurable resume distance that is greater than the stop distance. While distance lies between the two thresholds, the prior waiting/following state is retained. Serialized defaults will provide useful behavior without requiring scene edits, and invalid inspector values will be clamped so the resume distance cannot fall below the stop distance.

### Preserve orbit/path selection only while following

The existing orbit target and A* repathing remain available while the dog is outside the resume boundary. While waiting, the follower will not request paths or apply movement. Any stale path or target state will be cleared before a later resume so the next path is based on current positions.

### Commit Level 5 at the dual-actor checkpoint carpet

`LevelCheckpoint` will support an opt-in dual-actor occupancy requirement. L05_Checkpoint will enable it and track human and dog trigger occupancy, including exits before completion. A single actor on the carpet will not complete the checkpoint or commit the physical transition. Only when both roles are simultaneously inside the carpet collider will it save the checkpoint and notify the flow to commit Level 5. The broader Level 5 `LevelEntrySeal` will not be allowed to commit this transition ahead of the carpet.

### Release every temporary controller before restoring switching

When the dual-actor checkpoint confirms Level 5, the flow will first cancel `DogOrbitFollower`, force-release any remaining Level 4.5 parkour ownership, and clear human-only mode. Only after that cleanup will it close the transition door and unload predecessors. The operations will be idempotent and will not reposition either actor. This prevents both the dog being sealed outside and a stale `parkourControlOwners` count keeping `PlayerControl.Update` in its early-return path.

## Risks / Trade-offs

- [Thresholds feel too close or too far] -> expose both values as serialized configuration and cover their ordering in validation/tests.
- [Rapid boundary oscillation causes animation flicker] -> use separate stop and resume distances.
- [A stale path moves the dog after waiting] -> clear the path and movement target whenever waiting begins or follow is cancelled.
- [Parkour still owns input when Level 5 commits] -> force-release parkour ownership at the dual-actor handoff before restoring switching; later `OnDisable` cleanup remains idempotent.
- [One actor leaves the carpet before the partner arrives] -> remove that actor from checkpoint occupancy and continue forced follow without committing.
- [The broader entry seal sees both actors before the carpet does] -> configure the Level 5 entry seal so it cannot commit or close the door; L05_Checkpoint is the sole commit boundary.

## Migration Plan

1. Add distance-gated waiting/following state to `DogOrbitFollower` with safe serialized defaults.
2. Add opt-in dual-actor occupancy to `LevelCheckpoint` and enable it on L05_Checkpoint.
3. Prevent the broader Level 5 entry seal from committing before the checkpoint carpet.
4. At dual-actor checkpoint confirmation, cancel follow, force-release parkour ownership, restore switching, then close the door and unload predecessors.
5. Add focused automated coverage and perform a Play Mode route check from Level 4.5 into Level 5.

Rollback requires reverting the follower state gate and Level 5 cancellation hardening; no serialized scene migration is required.
