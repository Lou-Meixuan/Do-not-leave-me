## 1. Runtime Ownership Foundation

- [ ] 1.1 Add explicit parkour movement/input ownership to `PlayerControl` and verify a play-mode test shows normal movement, actor switching, and interactions are suppressed only while ownership is held.
- [ ] 1.2 Add parkour-safe horizontal placement and run/walk intent APIs to `PlayerActor` while preserving Rigidbody Y/gravity behavior; verify jump height and landing still match baseline locomotion.
- [ ] 1.3 Add explicit scripted-camera ownership/blending to `CameraFollow` and verify mouse rotation resumes with its prior settings after release, disable, death, and reset.

## 2. Route and Chase Core

- [ ] 2.1 Implement serializable human route segments with three lane offsets, straight progress, authored turn direction, decision windows, and gizmos; verify edit-mode tests cover lane bounds and contiguous segment validation.
- [ ] 2.2 Implement the Level 04B parkour phase state machine and automatic human run with eased one-lane `A`/`D` changes; verify play-mode tests cover outer-lane rejection and ownership cleanup.
- [ ] 2.3 Route `W` and `Space` to the existing grounded jump during chase while preserving automatic forward progress; verify a play-mode test covers grounded acceptance and airborne rejection.
- [ ] 2.4 Implement correct, incorrect, and missed non-final corner decisions with camera/route turning; verify correct input advances and both failure cases invoke the existing caught-death lifecycle.

## 3. Obstacles and Pressure

- [ ] 3.1 Implement replaceable parkour obstacle roots with separate lethal and late-evaluation volumes that resolve formal human identity and ignore the dog; verify edit/play-mode tests cover hit, near-miss, early-safe, and dog-overlap cases.
- [ ] 3.2 Implement bounded monster proximity, near-miss pressure gain, clean-run recovery, and a route-relative monster presentation anchor; verify deterministic tests cover clamping, recovery timing, and presentation distance.
- [ ] 3.3 Integrate parkour failure with `DeathScreen.DeathCause.Caught` and formal reset registration; verify retry restores lane, segment, actors, obstacles, proximity, camera ownership, and tutorial state.

## 4. Dog Companion and Final Branch

- [ ] 4.1 Implement an authored dog waypoint runner that maintains run locomotion, ignores parkour obstacles, and exposes scene gizmos; verify the dog completes straight/turn paths without influencing human outcomes.
- [ ] 4.2 Add proximity-threshold dog bark requests with cooldown and one-time missing-event diagnostics; verify the runtime test uses a bark-event seam and record that final audible verification remains pending until a Wwise bark event is authored and assigned.
- [ ] 4.3 Implement the final junction so both `A` and `D` play direction-specific branch approaches, send the monster presentation down the opposite branch, and normalize at one merge anchor; verify both choices end at identical human/dog positions and facing.

## 5. Walkout and Level 04B Scene Assembly

- [ ] 5.1 Implement the post-merge guided walk, calm camera framing, tutorial message `怪物似乎追丢了`, and idempotent restoration of free Level 04B control; verify the message precedes release and the existing corridor remains traversable afterward.
- [x] 5.2 Add `L04B_ParkourRoot` to Level 04B with four-to-six turns, human/dog/monster markers, final branch/merge, walkout path, and primitive placeholder jump/lane obstacles; verify scene validation passes and no slide-only obstacle is present.
- [ ] 5.3 Align the parkour merge with the current long corridor and preserve the existing checkpoint, crate-door trigger, physical exit binding, corridor music, and Level 05 transition references; verify serialized references and formal scene validators remain valid.

## 6. End-to-End Verification

- [ ] 6.1 Run edit-mode and play-mode parkour tests plus the existing formal route/traversal suite, and record any unrelated baseline failures separately.
- [ ] 6.2 Play through both final-branch choices from Level 04B chase start to the existing door-and-crate section, verifying hit death/retry, near-miss approach/recovery, dog route and bark request, guided walk/tutorial, restored camera/control, checkpoint, and downstream exit.
