## ADDED Requirements

### Requirement: Actor collision silhouette fit

Each Formal player actor SHALL use one non-trigger capsule collider whose effective world-space bounds follow the actor's core gameplay silhouette. The capsule SHALL exclude highly animated extremities and SHALL remain aligned with the actor root's foot-pivot ground plane.

#### Scenario: Human body fit

- **WHEN** the Formal human visual is loaded at its configured Body scale
- **THEN** the effective Y-oriented capsule covers the human's core standing silhouette without a conspicuous invisible lateral gap
- **AND** its bottom aligns with the actor root's ground plane within the project's body-fit tolerance

#### Scenario: Dog body fit

- **WHEN** the Formal dog visual is loaded at its configured Body scale
- **THEN** the effective Z-oriented capsule covers the dog's core standing silhouette without treating animated legs or tail as rigid volume
- **AND** its bottom aligns with the actor root's ground plane within the project's body-fit tolerance

### Requirement: Human and dog solid contact

The Formal human and dog SHALL resolve solid physics contact during ordinary gameplay movement and SHALL NOT pass through one another or remain visibly interpenetrated.

The shared actor setup SHALL NOT globally ignore the human/dog collider pair. Automated dog following SHALL apply movement through the dog's Rigidbody physics path rather than directly writing its Transform position.

During ordinary single-actor control, the inactive partner SHALL retain solid collision but SHALL NOT receive collision momentum that makes it slide away. Switching the active actor SHALL transfer dynamic physics control to the newly active actor. Scripted phases that move both actors MAY temporarily keep both actors dynamic.

#### Scenario: One actor approaches a stationary partner

- **WHEN** either actor moves into the stationary other actor from the front, side, or rear at configured gameplay speed
- **THEN** their colliders maintain stable separation
- **AND** neither actor passes through, launches, or enters persistent contact jitter

#### Scenario: Both actors approach

- **WHEN** the human and dog move toward each other at configured gameplay speeds
- **THEN** Unity resolves their contact without visible core-body overlap
- **AND** both actors can move apart normally afterward

#### Scenario: Active actor contacts inactive partner

- **WHEN** the player moves the active actor into the inactive partner during ordinary control
- **THEN** the inactive partner blocks overlap without sliding away from collision momentum

#### Scenario: Switch active actor

- **WHEN** the player switches control from one actor to the other
- **THEN** the newly active actor becomes dynamically movable
- **AND** the previously active actor becomes a solid non-pushable partner

### Requirement: Effective collider audit

The actor body-fit audit SHALL report collider measurements in effective world space so that child Body scaling cannot hide a collision-volume regression.

#### Scenario: Audit scaled actor bodies

- **WHEN** the audit runs for a loaded Formal actor
- **THEN** it reports capsule orientation, world-space bounds size and center, bottom-to-root error, and capsule-to-renderer fit gaps

#### Scenario: Audit incomplete actor

- **WHEN** an audited actor lacks a loaded renderer or capsule collider
- **THEN** the audit identifies the missing dependency without throwing an exception
