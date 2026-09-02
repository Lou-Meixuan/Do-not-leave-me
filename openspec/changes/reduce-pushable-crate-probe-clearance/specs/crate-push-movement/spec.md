## ADDED Requirements

### Requirement: Obstacle probe uses bounded safety clearance

The pushable crate SHALL sweep its inset collision volume only across the distance it can move during the next fixed physics step plus a configurable non-negative safety skin. The default safety skin SHALL be 0.03 metres. The probe SHALL NOT add the crate half extent to the sweep distance because the BoxCast shape already represents the crate volume.

#### Scenario: Crate approaches a wall

- **WHEN** the crate moves toward a wall at configured movement speed
- **THEN** the forward probe stops the crate before physical penetration
- **AND** the visible clearance is centimetre-scale rather than approximately one crate half-width

#### Scenario: Fixed timestep or speed changes

- **WHEN** configured movement speed or Unity fixed timestep changes
- **THEN** the probe sweep distance changes proportionally to the next intended physics-step displacement
- **AND** retains the configured 0.03 m safety skin

#### Scenario: Invalid negative configuration

- **WHEN** movement speed or safety skin is negative
- **THEN** the probe calculation treats the negative contribution as zero
