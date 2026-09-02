## Context

See proposal.md. The Level 2 door now requires a dog-operated pedal, two occupants in the safe zone, and E from the human. Its components have independent runtime state, while the dog stores its normal movement speed on the actor component.

## Goals / Non-Goals

**Goals:**

- Expose an optional in-game panel for the current route level, dog speed, and exact Level 2 gate failure.
- Toggle only the dog's runtime speed multiplier with keypad 1.
- Navigate the authored route with keypad 8/2 without duplicating scene names.

**Non-Goals:**

- Change L2 puzzle conditions, save the GM state, or affect human movement speed.
- Add production UI or persist cheat state between runs.

## Decisions

### Use an optional runtime panel

An Inspector toggle enables the cheat controls. Keypad 3 shows or hides the panel; the panel reads the actual route controller and L2 pedal/safe-zone/door-interaction components rather than inferring readiness from route state. Disabled cheats ignore every cheat key and draw no panel.

### Reuse the authored route for keypad navigation

Keypad 8 calls the existing next-level debug transition and keypad 2 calls the existing previous-level transition. Both therefore use the serialized route catalog and the established hard-cut test path.

### Preserve configured dog speed as the toggle baseline

The dog actor retains a separate runtime multiplier, so keypad 1 applies 5x without overwriting its serialized walking speed. A second press resets the multiplier to 1x.

## Risks / Trade-offs

- [Missing L2 references] → Report the missing component explicitly instead of throwing.
- [GM keys are pressed outside L2] → The panel reports that L2 is inactive; route navigation and dog-speed toggle remain deliberate test-only controls.

## Migration Plan

1. Add the diagnostic and speed-toggle hooks with safe missing-reference reporting.
2. Enable cheats, use keypad 3 to inspect the panel, and use keypad 8/2 across route boundaries.
3. Toggle keypad 1 twice and verify the dog returns to normal speed.
