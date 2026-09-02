## 1. Distance-gated forced follow

- [x] 1.1 Add configurable stop and resume distances to `DogOrbitFollower`, enforcing a valid hysteresis gap.
- [x] 1.2 Stop movement, clear stale path state, and set the dog idle when it reaches the near distance.
- [x] 1.3 Resume orbit/path following only after horizontal separation exceeds the resume distance.
- [x] 1.4 Preserve the existing follow-speed multiplier, collision-aware Rigidbody movement, A* fallback behavior, and parkour ownership flow.

## 2. Level 5 handoff

- [x] 2.1 Add opt-in dual-actor occupancy tracking to `LevelCheckpoint`, including removal on trigger exit before completion.
- [x] 2.2 Enable dual-actor completion on L05_Checkpoint and prevent the broader Level 5 entry seal from committing or closing the transition first.
- [x] 2.3 When both actors are simultaneously on L05_Checkpoint, save and commit Level 5, then cancel forced follow, force-release outstanding parkour ownership, and restore normal switching before closing the transition door and unloading predecessor scenes.
- [x] 2.4 Keep checkpoint and handoff cleanup idempotent so later controller `OnDisable` cleanup cannot re-enable follow or corrupt ownership counts.
- [x] 2.5 When Level 4.5 parkour temporarily owns dog movement, retain the forced-follow request and start it immediately after parkour releases ownership.

## 3. Verification

- [x] 3.1 Add focused tests for stopping near/touching the human, remaining idle inside the hysteresis band, and resuming beyond the follow distance.
- [x] 3.2 Add focused coverage that one actor on L05_Checkpoint, or one actor leaving before the partner arrives, does not cancel follow, close the door, or commit Level 5.
- [x] 3.3 Add focused coverage that both actors on L05_Checkpoint cancels follow, releases parkour control, restores dog switching, preserves actor positions, and only then closes the door.
- [ ] 3.4 Run relevant Unity tests, check compilation/console errors, and Play Mode verify the Level 4.5-to-Level 5 route.
