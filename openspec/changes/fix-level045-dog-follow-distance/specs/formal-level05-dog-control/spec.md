## MODIFIED Requirements

### Requirement: Level 5 commits only after both actors occupy the checkpoint carpet
The system SHALL commit the physical transition into Level 5 only after both the human and dog are simultaneously inside the L05_Checkpoint carpet trigger. A single actor on the carpet SHALL NOT complete the checkpoint, close the transition door, unload predecessor scenes, cancel forced follow, or establish Level 5 by itself.

#### Scenario: Human reaches the checkpoint before the dog
- **WHEN** the human stands on L05_Checkpoint while the dog is not on the carpet
- **THEN** the transition door remains open, forced dog follow remains active, and Level 4.5 control restrictions remain in effect

#### Scenario: First actor leaves before the partner arrives
- **WHEN** one actor leaves the L05_Checkpoint carpet before the other actor enters it
- **THEN** the checkpoint remains incomplete and the physical transition is not committed

#### Scenario: Both actors occupy the checkpoint carpet
- **WHEN** both the human and dog are simultaneously inside L05_Checkpoint
- **THEN** the system completes the checkpoint and begins the ordered Level 5 handoff

### Requirement: Dual-actor handoff restores dog switching and cancels temporary control
When the dual-actor checkpoint commits Level 5, the system SHALL first cancel active forced dog follow, release any outstanding Level 4.5 parkour-control ownership, and restore normal human/dog character switching while retaining both actors at their current world positions. The system SHALL close the transition door and unload predecessor scenes only after that control cleanup.

#### Scenario: Level 5 entry is committed after pursuit
- **WHEN** both actors satisfy L05_Checkpoint after the Level 4.5 traversal
- **THEN** forced follow and parkour control are inactive, the player can switch control to the dog, and both actors retain their current world positions

#### Scenario: Transition door closes after control cleanup
- **WHEN** the dual-actor checkpoint commits Level 5
- **THEN** forced follow and temporary control ownership are cleared before the transition door closes

#### Scenario: Cancelled follower receives later updates
- **WHEN** the dual-actor checkpoint handoff has established Level 5 and the human moves away from the dog
- **THEN** the dog does not automatically resume following

#### Scenario: Level 4.5 pursuit remains human-controlled
- **WHEN** both actors are not simultaneously on L05_Checkpoint during the Level 4.5 pursuit segment
- **THEN** distance-gated forced dog follow remains enabled and normal dog switching remains unavailable
