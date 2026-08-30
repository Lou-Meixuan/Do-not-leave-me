## 1. Formal route and scene

- [x] 1.1 Create `Level_04A.unity` with formal level metadata, human/dog respawn anchors, and placeholder entry/exit structure.
- [x] 1.2 Add Level 04A to build settings and insert it between Level 04 and Level 04B in the serialized and fallback route catalogs.
- [x] 1.3 Retarget Level 04's successor from Level 04B to Level 04A and verify Level 04B still targets Level 05.

## 2. Parkour migration

- [x] 2.1 Retarget the parkour builder to Level 04A and rename its generated root and editor menu without changing established movement controls.
- [x] 2.2 Generate the four-to-six-turn chase, obstacles, dog path, camera, and final left/right choice in Level 04A.
- [x] 2.3 Remove only the generated parkour root from Level 04B and verify its original serialized objects remain intact.
- [x] 2.4 Tile Level 04B floor/wall visual modules with disabled visual colliders along every doubled-length Level 04A straight.

## 3. Dark transition corridor

- [x] 3.1 Author independent left/right dark approaches with no visible physical merge.
- [x] 3.2 Add a concealment-controlled normalization to the shared dark exit corridor for human, dog, camera, and monster presentation.
- [x] 3.3 Complete the guided walk, `怪物似乎追丢了` message, and normal-control restoration inside the exit corridor.
- [x] 3.4 Preserve the earlier chase camera, then snap behind the human only when either black branch emerges into the shared exit corridor.

## 4. Configurable exit

- [x] 4.1 Add a placeholder physical exit door and route trigger to Level 04A with serialized successor `Level_04B`.
- [x] 4.2 Verify the successor can be changed to another valid future route scene without modifying parkour runtime code.

## 5. Verification

- [ ] 5.1 Compile runtime/editor assemblies and run route/scene validation.
- [ ] 5.2 Play through Level 04 into Level 04A using both final choices, confirming the normalization is hidden and controls restore before the exit door.
- [ ] 5.3 Continue through the Level 04A door into Level 04B and verify Level 04B's checkpoint, crate, music, exit, and Level 05 transition remain functional.
