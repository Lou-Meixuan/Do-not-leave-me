## Purpose

Defines a dedicated Level 04A parkour scene that transitions naturally from Level 04 through a concealed dark corridor to a configurable downstream room.

## ADDED Requirements

### Requirement: Level 04 advances into Level 04A

The formal route SHALL load Level 04A after the player uses Level 04's exit and SHALL place the persistent human and dog at Level 04A's configured entry anchors.

#### Scenario: Player leaves Level 04
- **WHEN** the Level 04 exit transition completes
- **THEN** Level 04A becomes the active gameplay scene
- **AND** the parkour chase starts from its configured spawn

### Requirement: Level 04A owns the existing parkour chase

Level 04A SHALL contain the working three-lane chase, obstacle, dog, pressure, and final-choice behavior previously generated in Level 04B.

#### Scenario: Either final direction succeeds
- **WHEN** the player chooses left or right at the final junction
- **THEN** that choice enters its own dark approach
- **AND** neither choice is treated as failure

### Requirement: Level 04A reuses the Level 04B corridor language

Each main parkour straight SHALL use the Level 04B corridor model as its visual shell and SHALL be approximately twice the length of the initial Level 04A prototype straight.

#### Scenario: Builder regenerates Level 04A
- **WHEN** the Level 04A parkour builder runs
- **THEN** every main straight receives an aligned Level 04B corridor visual instance
- **AND** lane, obstacle, and dog-path gameplay data remain authored independently from that visual model

### Requirement: Darkness conceals shared-corridor normalization

Both final approaches SHALL become visually unreadable before the actors are normalized to one shared exit corridor, and the authored map SHALL NOT expose a physical Y-shaped merge.

#### Scenario: Player reaches darkness from either branch
- **WHEN** the human reaches the concealment point on either final approach
- **THEN** the human and dog are placed at the same dark exit-corridor entry
- **AND** the spatial normalization is not visible to the player

### Requirement: Parkour releases before the exit door

After normalization the system SHALL perform the guided walk, display `怪物似乎追丢了`, and restore normal controls before the player reaches the route exit door.

#### Scenario: Chase decompression completes
- **WHEN** the guided dark-corridor walk ends
- **THEN** the completion message is shown
- **AND** normal camera and movement control are restored

### Requirement: Level 04A exit is configurable

The Level 04A physical exit SHALL advance through the formal route system to a serialized successor scene, initially Level 04B.

#### Scenario: Player uses the exit door
- **WHEN** the exit door's route transition condition is satisfied
- **THEN** the configured successor room is loaded
- **AND** changing that successor does not require parkour-code changes

### Requirement: Level 04B remains independent

Level 04B SHALL retain its pre-existing corridor, checkpoint, crate, door, music, and Level 05 transition and SHALL no longer contain the generated parkour root.

#### Scenario: Player arrives in Level 04B
- **WHEN** Level 04B is the configured Level 04A successor
- **THEN** the player enters Level 04B's ordinary corridor flow
- **AND** no Level 04A chase restarts there
