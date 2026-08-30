## Purpose

Defines the playable Level 04B escape sequence and its reliable handoff into the existing long-corridor door-and-crate gameplay without making final art a prerequisite.

## ADDED Requirements

### Requirement: Parkour phase owns forward locomotion
While the Level 04B parkour phase is active, the system SHALL move the human forward automatically along an authored route, constrain the human to three discrete lateral lanes, and interpret `A` and `D` as one-lane left and right changes except inside a turn decision window.

#### Scenario: Player changes one lane
- **WHEN** the human is running on a straight segment and the player presses `A` or `D`
- **THEN** the human transitions to the adjacent lane in that direction without leaving the authored route
- **AND** an input that would move beyond the outer lane is ignored

#### Scenario: Normal control is suppressed during parkour
- **WHEN** the parkour phase is active
- **THEN** free camera-relative movement, actor switching, sprint toggling, and mover interaction do not displace or redirect the human

### Requirement: Player can jump parkour obstacles
The system SHALL accept the existing jump input during the parkour phase and preserve forward route progress while the human is airborne.

#### Scenario: Player jumps from a valid grounded state
- **WHEN** the parkour phase is active, the human is grounded, and the player presses `W` or `Space`
- **THEN** the human performs a jump while continuing along the current route and lane

#### Scenario: Slide behavior is not exposed
- **WHEN** this change is active
- **THEN** the parkour route contains no obstacle that requires a slide action to survive

### Requirement: Authored corners require directional decisions
Each non-final corner SHALL declare a valid left or right direction and SHALL accept the matching `A` or `D` input during a clearly signalled decision window.

#### Scenario: Correct corner input
- **WHEN** the player supplies the authored direction inside the corner decision window
- **THEN** the human and camera complete the turn and continue on the next straight segment

#### Scenario: Missed or incorrect corner input
- **WHEN** the player supplies the wrong direction or reaches the corner without a valid direction
- **THEN** the chase ends in the existing caught-death flow

### Requirement: Obstacles provide hit and near-miss outcomes
Parkour obstacles SHALL distinguish a direct human impact from a late successful avoidance, SHALL ignore the dog, and SHALL be replaceable without changing route logic.

#### Scenario: Human directly hits an obstacle
- **WHEN** the human enters an obstacle's lethal volume during parkour
- **THEN** forward movement stops and the existing caught-death flow is shown

#### Scenario: Human narrowly avoids an obstacle
- **WHEN** the human exits an obstacle's near-miss window without entering its lethal volume and only became safe during the configured late interval
- **THEN** the player remains alive
- **AND** the monster temporarily closes distance

#### Scenario: Dog overlaps an obstacle
- **WHEN** the dog overlaps any parkour obstacle volume
- **THEN** no hit, near miss, or death outcome is produced

### Requirement: Chase pressure is recoverable and readable
The chase SHALL maintain a bounded monster-proximity state that affects the pursuer's presentation, increases after a near miss, and recovers toward its normal value after sustained clean running.

#### Scenario: Monster closes after a near miss
- **WHEN** a near miss is recorded
- **THEN** the monster visibly and audibly moves closer for a configured interval

#### Scenario: Clean running restores separation
- **WHEN** the player continues without another near miss or direct hit for the configured recovery interval
- **THEN** monster proximity returns toward its normal chase distance without exceeding its safe bounds

### Requirement: Dog follows a safe authored route
During parkour the dog SHALL run continuously along an authored companion route that favors flat ground and avoids obstacle and slide-clearance volumes, without accepting direct player control.

#### Scenario: Dog advances through a parkour segment
- **WHEN** the parkour phase advances along a straight or turning segment
- **THEN** the dog follows the corresponding safe companion markers in a running state without affecting human collision outcomes

#### Scenario: Monster reaches warning proximity
- **WHEN** monster proximity crosses the configured warning threshold
- **THEN** the dog emits a warning bark with a configured cooldown while the threshold remains active

### Requirement: Final branch converges on the existing corridor
The final junction SHALL accept either `A` or `D`, present the monster as taking the unchosen route, and converge both player choices on the same existing Level 04B long-corridor continuation.

#### Scenario: Player chooses either final direction
- **WHEN** the player presses `A` or `D` inside the final junction decision window
- **THEN** the chosen turn is presented to the player
- **AND** the human and dog arrive at the same authored merge point without duplicating the downstream corridor

### Requirement: Parkour ends with a guided decompression
After the final merge, the system SHALL remove immediate chase pressure, guide the human and dog through a short walk under controlled camera framing, display `怪物似乎追丢了`, and then restore the existing Level 04B controls.

#### Scenario: Guided walk completes
- **WHEN** the human and dog reach the end of the post-chase walk path
- **THEN** the tutorial message `怪物似乎追丢了` is displayed
- **AND** normal camera and free-movement control are restored after the message handoff

#### Scenario: Existing corridor gameplay resumes
- **WHEN** the parkour handoff is complete
- **THEN** the player can continue through the existing Level 04B long corridor toward its current door, crate, checkpoint, and exit behavior

### Requirement: Parkour restart is deterministic
The Level 04B reset flow SHALL restore parkour actors, route progress, lane state, obstacles, chase proximity, camera ownership, and tutorial state to a consistent configured restart point.

#### Scenario: Player retries after a parkour death
- **WHEN** the player restarts Level 04B after a parkour impact or failed turn
- **THEN** the parkour begins from its configured restart point with the human, dog, monster, and camera in their initial chase states

