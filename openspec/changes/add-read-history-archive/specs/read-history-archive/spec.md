# read-history-archive Specification

## ADDED Requirements

### Requirement: Open and close the archive with B

The system SHALL open the reading archive when the player presses B during active gameplay and no tutorial, death, pause, settings, cutscene, or other archive UI is active. The system SHALL close the archive from its list view when the player presses B or Escape.

#### Scenario: Open during gameplay
- **WHEN** gameplay is active and the player presses B
- **THEN** the archive opens on a category list and gameplay input is suspended

#### Scenario: Reject a conflicting modal state
- **WHEN** a tutorial popup, death screen, pause menu, settings panel, or cutscene owns the UI state
- **THEN** pressing B does not open the archive

#### Scenario: Close from the list
- **WHEN** the archive list is visible and the player presses B or Escape
- **THEN** the archive closes and restores the gameplay state that existed before it opened

### Requirement: Preserve and restore pause presentation state

The system SHALL pause gameplay, expose the cursor, and hide the gameplay HUD while the archive is open. It SHALL restore the previously captured time scale, cursor visibility, cursor lock mode, and HUD active state exactly once when the archive closes or is destroyed.

#### Scenario: Normal close restores state
- **WHEN** the player closes an archive opened from active gameplay
- **THEN** time, cursor, HUD, camera, and player controls return to their pre-open state

#### Scenario: Scene teardown while open
- **WHEN** the owning UI is destroyed while the archive is open
- **THEN** its saved time and cursor state are restored without leaving gameplay paused

### Requirement: Separate story and control categories

The archive SHALL provide distinct Story and Controls categories and SHALL list only entries discovered for the currently controlled role in the selected category.

#### Scenario: Browse discovered story
- **WHEN** the human selects Story after reading two human story entries
- **THEN** the list shows those two story titles in catalog order and no undiscovered title

#### Scenario: Empty category
- **WHEN** the controlled role has no discovered entry in the selected category
- **THEN** the archive displays an empty-state message instead of entry buttons

#### Scenario: Role-specific discovery
- **WHEN** a story has been read only by the dog
- **THEN** it appears while the dog is controlled and does not appear while the human is controlled

### Requirement: Persist discovered archive entries

The system SHALL store each discovered `(stable content ID, role)` pair in a versioned persistent collection, de-duplicate repeated discovery, and ignore stale IDs that are absent from the current catalog.

#### Scenario: Discovery survives restart
- **WHEN** an actor reads an entry and the application is restarted
- **THEN** that role's entry remains available in the archive

#### Scenario: Re-reading an entry
- **WHEN** an already discovered entry is opened again in the world
- **THEN** the persistent collection contains only one copy of that discovery

#### Scenario: Catalog no longer contains an ID
- **WHEN** saved progress contains an ID absent from the current catalog
- **THEN** archive loading continues and omits the stale entry

### Requirement: Replay discovered pages across scenes

The system SHALL resolve archive pages from a persistent central catalog rather than from currently loaded NoticeBoard objects. Selecting a discovered entry SHALL display its ordered pages inside a dedicated reader region of the still-visible archive panel, alongside the scrollable entry navigation.

#### Scenario: Replay after source scene unloads
- **WHEN** the player leaves the level containing a discovered notice and selects it in the archive
- **THEN** all pages for the current role appear in authored order inside the archive reader region

#### Scenario: Navigate pages inside the archive
- **WHEN** a selected entry contains multiple pages
- **THEN** the reader shows its title, current page number, fitted page image, and boundary-aware previous/next controls without hiding the scrollable entry list

#### Scenario: Close while reading
- **WHEN** the player presses B or Escape while a page is visible in the embedded reader
- **THEN** the entire archive closes and gameplay state is restored

#### Scenario: Change category while reading
- **WHEN** the player changes archive category while an entry is selected
- **THEN** the old page is cleared and the reader asks the player to select an entry from the new category

### Requirement: Discover world notices on successful reading

Each formal NoticeBoard SHALL have a stable archive content ID. The system SHALL discover the current role's version only after at least one valid page successfully opens.

#### Scenario: Successful notice read
- **WHEN** the human presses F at a configured notice and its page opens
- **THEN** the human version of that stable content ID is persisted as discovered

#### Scenario: Missing role pages
- **WHEN** the dog tries to read a notice without valid dog pages
- **THEN** no popup opens and no dog discovery is persisted

### Requirement: Discover tutorial content without changing trigger semantics

The opening, checkpoint, and Level_04B tutorials SHALL have stable Controls IDs and SHALL discover the displayed role version when they successfully open. Archive replay SHALL not mark a natural tutorial trigger as completed and SHALL not suppress a future natural trigger.

#### Scenario: Natural tutorial unlock
- **WHEN** the dog's checkpoint tutorial successfully opens
- **THEN** `controls.checkpoint` is discovered for the dog

#### Scenario: Replay has no trigger side effect
- **WHEN** a discovered tutorial is replayed from the archive
- **THEN** its existing tutorial PlayerPrefs completion keys are not changed

#### Scenario: Existing tutorial save migration
- **WHEN** a legacy tutorial completion key is set and the new discovery collection has no matching entry
- **THEN** the corresponding available Human tutorial version is discovered once

### Requirement: Validate archive authoring

The project SHALL provide an editor validation workflow that detects empty or duplicate stable IDs, missing titles, null pages, duplicate catalog role records, missing tutorial records, and incomplete Persistent UI references.

#### Scenario: Duplicate stable ID
- **WHEN** two world notice sources are assigned the same stable archive ID
- **THEN** validation fails and identifies both sources

#### Scenario: Valid migrated project
- **WHEN** all 15 formal notice sources, three tutorial groups, catalog records, and Persistent UI references are complete
- **THEN** validation succeeds without changing project assets
