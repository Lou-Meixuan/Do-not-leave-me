## 1. Distance-gated forced follow

- [x] 1.1 Add configurable stop and resume distances to `DogOrbitFollower`, enforcing a valid hysteresis gap.
- [x] 1.2 Stop movement, clear stale path state, and set the dog idle when it reaches the near distance.
- [x] 1.3 Resume orbit/path following only after horizontal separation exceeds the resume distance.
- [x] 1.4 Preserve the existing follow-speed multiplier, collision-aware Rigidbody movement, A* fallback behavior, and parkour ownership flow.

## 2. Level 5 handoff

- [x] 2.1 Ensure `L05_Checkpoint` explicitly and idempotently cancels active forced follow while restoring normal human/dog switching.
- [x] 2.2 Confirm cancellation resets external movement animation, runtime speed override, velocity, path, and target state without repositioning either actor.

## 3. Verification

- [x] 3.1 Add focused tests for stopping near/touching the human, remaining idle inside the hysteresis band, and resuming beyond the follow distance.
- [x] 3.2 Add focused coverage that the Level 5 checkpoint leaves forced follow inactive and normal dog switching available without changing actor positions.
- [ ] 3.3 Run relevant Unity tests, check compilation/console errors, and Play Mode verify the Level 4.5-to-Level 5 route.
