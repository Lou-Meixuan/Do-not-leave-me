# formal-notice-board-pages Specification

## ADDED Requirements

### Requirement: Archive identity and discovery

Each formal notice board SHALL reference a stable archive content ID whose role-specific pages are present in the central reading catalog. A role SHALL be marked as having discovered that content only after its configured pages successfully open.

#### Scenario: Notice opens from catalog content
- **WHEN** a human reads a notice whose catalog record contains ordered human pages
- **THEN** the popup displays those pages in catalog order and persists the human discovery

#### Scenario: Notice content cannot open
- **WHEN** the current role has no valid catalog pages for the notice ID
- **THEN** no discovery is recorded and the existing missing-content diagnosis is reported

