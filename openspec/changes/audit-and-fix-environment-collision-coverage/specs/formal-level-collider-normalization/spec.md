## ADDED Requirements

### Requirement: Missing blocking coverage audit

The system SHALL provide a report-only editor audit that identifies active Formal environment renderers without enabled non-trigger physical coverage on the rendered object or its approved collision-owning hierarchy. The audit SHALL distinguish a missing blocking Collider from an existing MeshCollider whose mesh reference is missing or divergent.

#### Scenario: Substantial static obstacle has no Collider

- **WHEN** an active wall, architectural object, large furniture item, or other substantial static obstacle is rendered without approved blocking coverage
- **THEN** the audit reports it as a missing candidate with its scene, hierarchy path, prefab source, layer, world bounds, and Rigidbody state

#### Scenario: Existing MeshCollider has no mesh

- **WHEN** a rendered node already has a MeshCollider whose `sharedMesh` is null
- **THEN** the audit reports a mesh-reference synchronization failure rather than conflating it with absent physical coverage

#### Scenario: Trigger does not provide blocking coverage

- **WHEN** a rendered mechanism or object has only trigger Colliders
- **THEN** the audit reports it as trigger-only and does not count it as physically blocking

### Requirement: Collision intent classification

The collision coverage audit SHALL classify candidates according to physical role and SHALL NOT require blocking collision on visual-only decoration, VFX, UI, decals, authoritative ground visuals, door frames, or other reviewed non-blocking content.

#### Scenario: Visual-only small prop

- **WHEN** a small decorative prop is reviewed as intentionally non-blocking
- **THEN** it is recorded as an intentional exception with a durable reason and is not repeatedly reported as an unresolved error

#### Scenario: Ambiguous or irregular object

- **WHEN** an object's shape, dynamic state, nested prefab ownership, or gameplay role makes automatic collision unsafe
- **THEN** the audit classifies it as review-required and does not mutate it

### Requirement: Explicit selected-object repair

The system SHALL provide an Undo-aware repair command that adds bounds-aligned, enabled, non-trigger BoxCollider coverage only to explicitly selected and approved simple static obstacles. The command SHALL NOT automatically save scenes or modify unselected objects.

#### Scenario: Repair simple rectangular obstacle

- **WHEN** the user explicitly selects an approved simple static wall or rectangular prop and invokes repair
- **THEN** the object receives a bounds-aligned BoxCollider using the project static-obstacle layer and physics material conventions
- **AND** prefab instance changes are recorded as explicit overrides

#### Scenario: Reject unsafe automatic repair

- **WHEN** the selected target is dynamic, irregular/concave, a floor, a trigger mechanism, a door frame, or has ambiguous collision ownership
- **THEN** automatic repair refuses the target and reports the reason for manual review

### Requirement: Formal route collision review completion

Every enabled Formal gameplay scene SHALL be audited with its required shared-art companion scenes loaded, and every missing candidate SHALL resolve to an approved Collider repair or a documented intentional exception.

#### Scenario: Route audit completes

- **WHEN** the final collision-coverage audit is run across the Formal route
- **THEN** no unreviewed missing candidates remain
- **AND** the durable report lists all repairs, manual-review decisions, and intentional exceptions
