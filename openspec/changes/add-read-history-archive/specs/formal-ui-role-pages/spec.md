# formal-ui-role-pages Specification

## ADDED Requirements

### Requirement: Tutorial archive identity

The opening, checkpoint, and level-introduction tutorial groups SHALL expose stable archive content IDs for their independent human and dog page collections. A successful natural display SHALL discover only the displayed role version.

#### Scenario: Role tutorial becomes reviewable
- **WHEN** the dog opening tutorial successfully displays its configured pages
- **THEN** the dog version of the opening tutorial becomes available in the Controls archive

### Requirement: Archive replay preserves tutorial progress

Replaying tutorial pages from the reading archive SHALL preserve their supplied page collection and SHALL not write or otherwise change the natural tutorial completion keys.

#### Scenario: Replay before another natural trigger
- **WHEN** the player replays a discovered tutorial while another tutorial group has not yet triggered naturally
- **THEN** the untriggered group's completion state remains unchanged

