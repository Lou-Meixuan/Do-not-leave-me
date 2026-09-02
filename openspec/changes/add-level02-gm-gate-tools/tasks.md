## 1. GM Runtime Hooks

- [x] 1.1 Add a runtime-only movement multiplier to the dog actor that preserves its configured walk speed as the baseline.
- [x] 1.2 Add keypad 1 handling to toggle the available dog's multiplier between 1x and 5x while cheats are enabled.

## 2. Level 2 Gate Diagnostics

- [x] 2.1 Expose read-only gate state from the existing L2 pedal, safe-zone interaction, and target-door components without changing the cooperative rules.
- [x] 2.2 Show the L2 gate's pedal, two-player safe-zone, E-interaction, and target-door resolution state in the panel, including safe handling of missing references.

## 3. Cheat Panel and Route Navigation

- [x] 3.1 Add an Inspector option that disables all cheat input and panel rendering when off.
- [x] 3.2 Add keypad 3 panel visibility toggling and display the current route level and dog-speed multiplier.
- [x] 3.3 Add keypad 8/2 navigation through the existing authored route transition methods.

## 4. Verification

- [ ] 4.1 Add or update focused editor tests for dog speed toggling and panel state reporting where practical.
- [x] 4.2 Build the runtime and editor assemblies and validate the OpenSpec change.
- [ ] 4.3 Run the direct Play Mode checklist: disabled cheats ignore input; keypad 3 toggles the panel; keypad 8/2 navigates the route; keypad 1 toggles dog speed; and the L2 gate state is accurate.
