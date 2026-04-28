# 🏛️ Architecture Decisions Log — Mythfall: Survivors

> Track non-trivial architectural decisions, framework modifications, and the reasoning behind them.
> Append-only log. Most recent at the top.

---

## 2026-04-28 — CharacterLocomotion: ExternalRotationControl pattern (Sprint 1)

**Context:** Sprint 1 introduces top-down combat where the player faces the nearest enemy via `PlayerFacing` while moving freely via joystick. Movement direction and facing direction must be independent (CLAUDE.md Architecture Rule 4).

The shipped `ModularTopDown.Locomotion.CharacterLocomotion` auto-rotated the character toward `targetMoveVector` inside `HandleGroundedMovement` and toward `moveDirection` inside `HandleAirborneMovement`, fighting any external rotation source.

**Decision:** Modify `CharacterLocomotion.cs` with five **additive, non-breaking** changes (default behavior unchanged when `ExternalRotationControl == false`).

**Changes (all marked `// MYTHFALL ADDITIVE` in source):**

1. New property: `public bool ExternalRotationControl { get; set; }` — defaults `false`.
2. New getter: `public Vector3 HorizontalVelocity` — returns `(playerVelocity.x, 0, playerVelocity.z)`.
3. New getter: `public float RunSpeed` — exposes the private `runSpeed` serialized field.
4. Null-safe `characterAnimator` access in `HandleGroundedMovement` (silent fallback — Mythfall players use Unity Animator directly with `Speed` parameter, not the framework's `CharacterAnimator` middleman).
5. Rotation guard: `HandleRotation(...)` calls in `HandleGroundedMovement` and `HandleAirborneMovement` are now skipped when `ExternalRotationControl == true`.

**Why additive instead of subclass/wrapper:**
- Subclassing requires overriding two methods + duplicating private state — maintenance burden.
- The flag pattern is the intended design per CLAUDE.md Rule 4; the omission was an oversight, not deliberate immutability.
- Default-`false` flag means every existing consumer (DashState, JumpState, future framework users) keeps the original behavior.

**Player wiring (Mythfall side):**
```csharp
// PlayerBase.Awake
locomotion.ExternalRotationControl = true;

// PlayerBase.Update (after locomotion.HandleGroundedMovement / HandleAirborneMovement)
float normalizedSpeed = locomotion.HorizontalVelocity.magnitude / locomotion.RunSpeed;
animator.SetFloat("Speed", normalizedSpeed);
// PlayerFacing rotates the transform toward TargetSelector.CurrentTarget independently.
```

The framework's `CharacterAnimator` field on player prefabs is intentionally left **unassigned** — Mythfall drives the Animator directly so that animation events (Attack_1, Death, Skill_Active_1) fire via `DynamicAnimationEventHub`.

**Reverted-by-accident risk:** the in-source comment block at the top of the additive section explains the reasoning so future maintainers don't undo these lines under a misapplied "don't touch the framework" rule. Search `MYTHFALL ADDITIVE` to find all five touch points.

---

## 2026-04-28 — BillBootstrapConfig.defaultGameScene = "" (Sprint 0)

**Context:** Sprint 0 doc Task 0.5 set `defaultGameScene = "MenuScene"` AND Task 0.6 had `GameBootstrap.Initialize()` calling `Bill.Scene.Load("MenuScene")` — that's a double-load.

**Decision:** Leave `BillBootstrapConfig.defaultGameScene` empty. `GameBootstrap` (with `BillStartup`) owns the first scene transition so it happens AFTER our services are registered (otherwise services would race the scene load).

**Why:** Bill's auto-bootstrap fires `Bill.Scene.Load(_cfg.defaultGameScene)` immediately after `MarkInitialized` + `GameReadyEvent`. If our services aren't registered before that, MenuScene would load with `LocalizationService` missing.

---

## 2026-04-28 — BillGameCore namespace correction (Sprint 0)

**Context:** Sprint 0 doc + `Docs/LOCALIZATION_GUIDE.md` reference `BillTheDev.GameCore` and `BillTheDev.GameCore.Bootstrap` namespaces. Actual code uses a single `BillGameCore` namespace.

**Decision:** Mythfall code uses the actual namespace (`BillGameCore`). The two docs should be updated separately when convenient.

---

*Append new entries above this line.*
