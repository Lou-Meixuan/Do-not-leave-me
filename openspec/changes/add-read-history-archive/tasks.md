# Tasks: add-read-history-archive

## 1. Data model and persistence

- [x] 1.1 Add `ReadingArchiveCatalog` and serializable entry/category/role models with stable-ID lookup and ordered role-specific Sprite pages
- [x] 1.2 Add `ReadingArchiveProgress` with versioned PlayerPrefs JSON, discovery de-duplication, stale-ID tolerance, role isolation, and legacy tutorial-key migration
- [x] 1.3 Add EditMode tests for catalog lookup, progress serialization, duplicate discovery, stale IDs, role isolation, and legacy migration

## 2. Runtime discovery and replay integration

- [x] 2.1 Extend `NoticeBoard` with stable archive ID lookup; open pages from the catalog and persist discovery only after a successful role-specific open
- [x] 2.2 Extend `TutorialPopup` with fixed tutorial archive IDs, successful-open discovery, and a replay mode that does not mutate natural tutorial PlayerPrefs or own the archive pause session
- [x] 2.3 Add explicit modal-state integration so PauseMenu, TutorialPopup, DeathScreen, player interaction, and the archive cannot respond simultaneously
- [x] 2.4 Compile and confirm no Console errors before scene migration

## 3. Authoring migration and validation

- [x] 3.1 Add `Tools/DoNotLeaveMe/阅读档案/构建与迁移` to create/update the catalog, assign deterministic IDs to all 15 formal NoticeBoards, collect role pages, and save only changed scenes
- [x] 3.2 Add a read-only validation command for unique IDs, titles, valid pages, tutorial records, catalog/source agreement, and Persistent UI references
- [x] 3.3 Run migration, review all generated Chinese titles and category assignments, and run validation successfully

## 4. Persistent archive UI

- [x] 4.1 Add `ReadingArchiveController` with B/ESC handling, modal guards, exact time/cursor/HUD capture and restore, category state, current-role filtering, and empty-state handling
- [x] 4.2 Revise the FormalUI archive panel into one integrated view: scrollable entry navigation plus an always-in-panel reader with title, fitted page image, page count, previous/next controls, empty/selection states, and close button
- [x] 4.3 Update the HUD controls text to advertise `B 阅读档案` for both human and dog
- [x] 4.4 Save Persistent without unrelated scene reserialization and run archive validation again

## 5. Verification

- [ ] 5.1 Play from a clean discovery save: B opens/closes only during gameplay, pauses correctly, restores HUD/cursor/time, and empty categories render correctly
- [ ] 5.2 Read notices as human and dog across at least two levels; verify role-specific unlock, persistence after restart, and replay after source scenes unload
- [ ] 5.3 Trigger all three tutorial groups and verify Controls unlocks; replay them without changing remaining natural tutorial triggers
- [ ] 5.4 Verify B/Escape behavior against TutorialPopup, page replay, PauseMenu/settings, DeathScreen, cutscenes, scene reset, and UI destruction
- [ ] 5.5 Run EditMode tests, archive validation, representative route Play checks, and confirm the Console has no errors
- [ ] 5.6 Verify selected content renders in front within the B panel reader, the list remains scrollable, multi-page navigation stays inside the panel, and no TutorialPopup layer opens during archive reading
