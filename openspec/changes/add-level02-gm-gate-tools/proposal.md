## Why

Level 2's cooperative door must be tested quickly and its missing prerequisite must be visible when it does not open. Testers also need a temporary way to move the dog through the level more quickly.

## What Changes

- Add an optional runtime cheat panel that reports the current route level, dog-speed multiplier, and Level 2 door-gate status.
- While cheats are enabled, use keypad 8/2 to move to the next/previous route level, keypad 1 to toggle dog speed, and keypad 3 to show or hide the panel.
- Keep the commands scoped to runtime testing and avoid changing the intended cooperative door rules.

## Capabilities

### New Capabilities

- `formal-level02-gm-gate-tools`: Provides an optional runtime cheat panel, route navigation, L2 gate visibility, and a dog-speed testing toggle.

### Modified Capabilities

- None.

## Impact

- Formal route flow, player movement runtime, and Level 2 cooperative door interaction diagnostics.
