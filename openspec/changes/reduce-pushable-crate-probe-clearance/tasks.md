## 1. Update probe calculation

- [x] 1.1 Use the BoxCollider's world center for the BoxCast origin.
- [x] 1.2 Replace projected-half-extent sweep distance with next-fixed-step displacement plus non-negative safety skin.
- [x] 1.3 Preserve inset cast shape and attached-actor/self/trigger exclusions.

## 2. Update shared configuration

- [x] 2.1 Set the shared and embedded Level 1 crate configurations from 0.12 m to 0.03 m.
- [x] 2.2 Confirm no scene or prefab override retains the old value.

## 3. Verify

- [x] 3.1 Compile cleanly and check the Unity Console.
- [ ] 3.2 Push straight and diagonally into walls and props; confirm centimetre-scale clearance without tunneling.
- [ ] 3.3 Confirm attached actors do not falsely block the cast and normal push/pull still works.
