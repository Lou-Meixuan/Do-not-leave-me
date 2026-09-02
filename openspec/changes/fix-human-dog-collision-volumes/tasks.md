## 1. Establish the baseline

- [ ] 1.1 Open the shared `PlayerActors.prefab` in Unity and run the existing body-fit audit with both visuals loaded.
- [ ] 1.2 Record effective world-space human/dog capsule bounds and reproduce the visible overlap or excessive separation from multiple approach directions.
- [ ] 1.3 Confirm at runtime that both actor colliders are enabled, non-trigger, and mutually colliding through the active layer matrix.

## 2. Restore physics contact

- [x] 2.1 Remove the startup behavior that ignores the human/dog collider pair and explicitly restore their contact.
- [x] 2.2 Route forced dog orbit movement through `PlayerActor.Move` during fixed physics updates instead of writing `transform.position`.
- [x] 2.3 Restore the dog's configured runtime speed multiplier and stop its rigidbody when forced following ends.
- [x] 2.4 Add explicit dynamic/kinematic physics ownership to `PlayerActor` without disabling its collider.
- [x] 2.5 Transfer physics ownership on startup, Tab switching, forced-human mode, and parkour control transitions.
- [x] 2.6 Avoid velocity writes while an actor is kinematic.

## 3. Make collision fit inspectable

- [x] 3.1 Extend `ActorBodyAudit` to report capsule orientation, world-space size/center, bottom-to-root error, and lateral/vertical gaps versus renderer bounds.
- [x] 3.2 Verify the audit handles missing visuals or colliders without throwing and clearly identifies the affected actor.

## 4. Tune the shared prefab

- [ ] 4.1 Adjust the human capsule raw radius, height, and center for an appropriate effective world-space silhouette while keeping its bottom on the foot-pivot plane.
- [ ] 4.2 Adjust the dog Z-oriented capsule raw radius, height, and center using the same criteria.
- [x] 4.3 Apply changes only to `PlayerActors.prefab`; do not add per-scene overrides.

## 5. Verify

- [x] 5.1 Compile cleanly and check the Unity Console for new errors or physics warnings.
- [ ] 5.2 Test human-to-dog, dog-to-human, and both-moving contact from front, side, and rear; confirm no pass-through, obvious overlap, launch, or persistent jitter.
- [ ] 5.3 Confirm both actors remain grounded and can separate naturally after contact.
- [ ] 5.3a Confirm the inactive partner blocks the active actor without sliding, and Tab switching transfers which actor can be pushed/moved.
- [ ] 5.4 Smoke-test representative doors/corridors and existing spawn/checkpoint placement for clearance regressions.
- [ ] 5.5 Re-run the audit and record the final effective dimensions and fit observations.
