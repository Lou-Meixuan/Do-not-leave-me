## Purpose

Provides an optional in-game cheat panel for route navigation, diagnosing the Level 2 cooperative door, and accelerating dog traversal during focused testing.

## ADDED Requirements

### Requirement: Optional cheat controls

The runtime SHALL ignore cheat keys and draw no cheat panel when cheats are disabled. When cheats are enabled, keypad 3 SHALL toggle the cheat panel.

#### Scenario: Cheats are disabled

- **WHEN** the cheat option is disabled and a cheat key is pressed
- **THEN** route, dog speed, and panel visibility remain unchanged

#### Scenario: Show current panel

- **WHEN** cheats are enabled and the tester presses keypad 3
- **THEN** the panel becomes visible and identifies the current route level and dog-speed multiplier

### Requirement: Cheat route navigation

The runtime SHALL use the authored route catalog to move forward with keypad 8 and backward with keypad 2 while cheats are enabled.

#### Scenario: Move to next level

- **WHEN** the tester presses keypad 8 and a successor exists
- **THEN** the existing debug level-load path loads the next route level and places both actors at its spawn

#### Scenario: Move to previous level

- **WHEN** the tester presses keypad 2 and a predecessor exists
- **THEN** the existing debug level-load path loads the previous route level and places both actors at its spawn

### Requirement: Level 2 gate status panel

The runtime SHALL show the Level 2 cooperative door's current readiness and each unmet condition in the cheat panel.

#### Scenario: Gate is incomplete

- **WHEN** the panel is visible while the Level 2 cooperative door cannot open
- **THEN** the panel identifies whether the pedal, two-player safe-zone condition, E-interaction occupancy, or target door resolution is missing

#### Scenario: Gate is ready

- **WHEN** the panel is visible after every Level 2 cooperative prerequisite is satisfied
- **THEN** the panel reports that the E interaction is ready to open the L2-to-L3 door

### Requirement: Five-times dog speed GM toggle

The runtime SHALL toggle the dog's normal movement speed between its configured value and five times that value when cheats are enabled and the tester presses keypad 1.

#### Scenario: Enable accelerated dog speed

- **WHEN** the tester presses keypad 1 while the dog is available
- **THEN** the dog's movement speed becomes five times its configured normal speed and the Console reports the active multiplier

#### Scenario: Restore dog speed

- **WHEN** the tester presses keypad 1 again while acceleration is active
- **THEN** the dog's movement speed returns to its configured normal speed and the Console reports restoration
