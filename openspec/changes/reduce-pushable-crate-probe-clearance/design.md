## Context

The crate uses a 2.2 m BoxCollider and moves at 2 m/s. `ProbeBlocked` currently supplies an approximately 90%-sized crate box to `Physics.BoxCast`, then sweeps it by the crate projected half extent plus a serialized 0.12 m skin. For an axis-aligned push this is about 1.22 m of sweep beyond an already volumetric cast shape. Unity's global contact offset is only 0.01 m and is not the source of the large visual gap.

## Decision

Use the maximum distance the crate intends to travel during the next fixed physics step, plus the requested 0.03 m probe skin:

`probeDistance = movementSpeed * Time.fixedDeltaTime + blockProbeSkin`

At 2 m/s and the usual 0.02 s fixed step, the cast distance becomes approximately 0.07 m. The slightly inset cast half-extents remain to avoid immediate self/contact-edge false positives. The origin should represent the BoxCollider center in world space rather than assuming the Transform pivot is the collider center.

The 0.03 m value is serialized on the shared prefab so designers can see and tune it. Code clamps negative speed/skin values to zero.

## Risks / Trade-offs

- A very large fixed timestep or externally increased crate speed increases the required sweep; deriving distance from both values keeps coverage proportional.
- An oversized wall/prop Collider can still create an apparent gap; the collision-coverage work audits those separately.
- BoxCast reports contact against physics geometry, so a remaining small gap around 0.03 m plus contact offset is expected and intentional.

## Verification

- Push the crate straight and diagonally into a wall and substantial prop.
- Measure the visible stop gap and confirm it is centimetre-scale rather than near one metre.
- Confirm no tunneling at configured movement speed and fixed timestep.
- Confirm attached human/dog Colliders do not create a false blocked result.
- Compile cleanly and check the Console.
