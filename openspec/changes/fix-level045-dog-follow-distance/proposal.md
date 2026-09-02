## Why

Level 4.5 forces the dog to follow the human, but the current follower continuously chases a moving orbit point and only stops within an extremely small movement threshold. As a result, the dog keeps pushing or circling after reaching the human instead of waiting nearby. The forced-follow state must also end cleanly when Level 5 begins.

## What Changes

- Change forced dog follow in Level 4.5 to stop when the dog reaches a configurable near distance from the human.
- Resume forced follow only after the human moves beyond a larger configurable follow distance, preventing rapid start/stop jitter.
- Keep the dog stationary and idle while it is inside the near-distance band, including when the actors touch or collide.
- Keep forced follow active while only one actor is standing on the Level 5 checkpoint carpet.
- Commit Level 5 only after both the human and dog are simultaneously on the L05_Checkpoint carpet; then cancel forced follow, release any parkour ownership, restore ordinary two-character control, close the transition door, and unload retained predecessor scenes without moving either actor.

## Capabilities

### New Capabilities

- None.

### Modified Capabilities

- `formal-dog-locomotion`: Define distance-gated forced-follow movement and idle behavior.
- `formal-level05-dog-control`: Require the Level 5 handoff to cancel any active forced follower in addition to restoring switching.

## Impact

- `DogOrbitFollower`, `LevelCheckpoint`, `GameFlowController`, the L05_Checkpoint carpet configuration, and focused Unity tests.
- Existing Level 4.5 parkour ownership and Level 5 actor positions remain unchanged.
- No scene layout or art asset changes are expected.
