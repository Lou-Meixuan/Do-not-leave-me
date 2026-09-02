## Context

`DogOrbitFollower` currently computes a continuously rotating destination at `orbitRadius` from the human and drives the dog toward it every physics step. Its `movementThreshold` only detects arrival at that moving destination, not arrival near the human. `PlayerControl.ForceHumanOnly(false)` calls `StopOrbit`, and the Level 5 checkpoint currently uses that path to restore normal switching.

## Goals / Non-Goals

**Goals:**

- Make the forced follower wait near the human instead of continuously orbiting or pushing into them.
- Resume following after the human creates meaningful separation.
- Guarantee that entering Level 5 clears forced-follow movement and speed overrides.
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

### Make Level 5 cancellation explicit and idempotent

The Level 5 checkpoint handoff will continue restoring ordinary player control and will explicitly leave `DogOrbitFollower` inactive, with external movement, speed multiplier, and velocity reset. Repeated cancellation remains safe and does not reposition either actor.

## Risks / Trade-offs

- [Thresholds feel too close or too far] -> expose both values as serialized configuration and cover their ordering in validation/tests.
- [Rapid boundary oscillation causes animation flicker] -> use separate stop and resume distances.
- [A stale path moves the dog after waiting] -> clear the path and movement target whenever waiting begins or follow is cancelled.
- [Parkour temporarily owns dog motion] -> retain the existing stop/restart ownership protocol in `Level04BParkourController`.

## Migration Plan

1. Add distance-gated waiting/following state to `DogOrbitFollower` with safe serialized defaults.
2. Harden the Level 5 handoff so cancellation is observable and idempotent.
3. Add focused automated coverage and perform a Play Mode route check from Level 4.5 into Level 5.

Rollback requires reverting the follower state gate and Level 5 cancellation hardening; no serialized scene migration is required.
