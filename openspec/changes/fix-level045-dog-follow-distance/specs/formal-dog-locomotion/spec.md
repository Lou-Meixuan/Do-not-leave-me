## ADDED Requirements

### Requirement: Forced follow waits near the human
During forced follow, the system SHALL stop moving the dog when its horizontal distance from the human is at or below a configurable stop distance. The stopped dog SHALL remain stationary and play Idle, including when the dog and human touch or their colliders contact.

#### Scenario: Dog reaches the human
- **WHEN** forced follow is active and the dog's horizontal distance from the human reaches the configured stop distance
- **THEN** the dog stops applying follow movement and plays Idle

#### Scenario: Actors remain close or touching
- **WHEN** the dog is waiting and the actors remain within the configured resume distance, including physical contact
- **THEN** the dog remains stationary instead of pushing, circling, or repeatedly restarting

### Requirement: Forced follow resumes after separation
The system SHALL resume forced-follow movement only when the dog's horizontal distance from the human exceeds a configurable resume distance. The resume distance SHALL be greater than the stop distance so small distance changes do not cause repeated start/stop transitions.

#### Scenario: Human leaves the waiting dog
- **WHEN** the dog is waiting and the human moves beyond the configured resume distance
- **THEN** the dog resumes pathing toward and following the human

#### Scenario: Distance remains within the hysteresis band
- **WHEN** the dog is waiting and its distance from the human is greater than the stop distance but not greater than the resume distance
- **THEN** the dog remains waiting and Idle
