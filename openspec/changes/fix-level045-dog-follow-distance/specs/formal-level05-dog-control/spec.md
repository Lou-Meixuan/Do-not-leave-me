## MODIFIED Requirements

### Requirement: Level 5 restores dog switching and cancels forced follow
The system SHALL restore normal human/dog character switching when L05_Checkpoint establishes Level 5, SHALL cancel any active forced dog-follow behavior, and SHALL not move either player actor as part of the handoff.

#### Scenario: Level 5 checkpoint is reached after pursuit
- **WHEN** L05_Checkpoint activates after the Level 4.5-to-Level 5 physical traversal
- **THEN** forced dog follow is inactive, the player can switch control back to the dog, and both actors retain their current world positions

#### Scenario: Cancelled follower receives later updates
- **WHEN** Level 5 has been established and the human moves away from the dog
- **THEN** the dog does not automatically resume following

#### Scenario: Level 4.5 pursuit remains human-controlled
- **WHEN** the player has not yet reached L05_Checkpoint during the Level 4.5 pursuit segment
- **THEN** distance-gated forced dog follow remains enabled and normal dog switching remains unavailable
