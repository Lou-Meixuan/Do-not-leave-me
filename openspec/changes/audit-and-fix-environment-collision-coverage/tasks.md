## 1. Establish collision ownership and baseline

- [ ] 1.1 Enumerate enabled Formal gameplay/shared-art scenes and the additive scene combinations used at runtime.
- [ ] 1.2 Run the existing ground and MeshCollider-sync audits and record what they cover and omit.
- [ ] 1.3 Produce a baseline list of active renderers without enabled non-trigger collision coverage, including scene/prefab ownership data.

## 2. Add missing-coverage audit

- [ ] 2.1 Implement coverage discovery across selected hierarchies and loaded Formal scenes without mutating assets.
- [ ] 2.2 Classify covered, disabled, trigger-only, missing-candidate, and review-required results.
- [ ] 2.3 Exclude player characters, particles/VFX, UI, decals, authoritative ground visuals, and other explicit non-blocking categories.
- [ ] 2.4 Include scene path, hierarchy path, prefab source, layer, bounds, Rigidbody state, and coverage source in reports.
- [ ] 2.5 Add durable reviewed exceptions with reasons and stale-entry reporting.

## 3. Add safe selected-object repair

- [ ] 3.1 Add an Undo-aware command that fits a `BoxCollider` to explicitly selected simple static render bounds.
- [ ] 3.2 Reject or route dynamic, irregular, trigger/mechanism, floor, door-frame, and ambiguous nested-prefab targets to manual review.
- [ ] 3.3 Record prefab instance overrides correctly and leave scene saving to the user.
- [ ] 3.4 Assign the appropriate static obstacle layer and no-friction physics material according to existing project rules.

## 4. Review and repair Formal routes

- [ ] 4.1 Audit each Level 01–05/04A/04B gameplay scene with its required shared-art companions loaded.
- [ ] 4.2 Review every missing candidate and choose prefab repair, scene proxy, compound/mesh review, or intentional exception.
- [ ] 4.3 Repair approved blocking walls, architecture, large furniture, and substantial props without modifying visual-only clutter.
- [ ] 4.4 Re-run coverage and MeshCollider-sync audits until no unreviewed missing candidates remain.

## 5. Verify

- [ ] 5.1 Compile cleanly and check the Unity Console for new errors or physics warnings.
- [ ] 5.2 Walk both actors through representative routes, walls, corners, doors, furniture, and shared-scene seams.
- [ ] 5.3 Confirm there are no new invisible barriers, blocked mechanisms, floor seams, or collision jitter.
- [ ] 5.4 Save a durable final audit report listing repairs and intentional exceptions.
