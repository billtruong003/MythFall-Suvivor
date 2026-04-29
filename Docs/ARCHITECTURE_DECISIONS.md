# 🏛️ Architecture Decisions Log — Mythfall: Survivors

> Track non-trivial architectural decisions, framework modifications, and the reasoning behind them.
> Append-only log. Most recent at the top.

---

## 2026-04-29 — Enemy player-reference resolution: lazy-fetch over event subscribe (Sprint 1 Day 3)

**Context:** GameplaySpawner is async (waits on Bill ready before spawning the player), and `WaveSpawner` is also async (waits on Bill.Pool). Their relative ordering inside GameplayScene is undefined — either can fire first. Swarmers spawned before the player would `FindGameObjectWithTag("Player")` in `OnSpawn` and cache `null`, then idle forever even after the player appears.

**Considered:** pure event-driven — every spawned `EnemyBase` subscribes to `CharacterSpawnedEvent` to set `playerTransform`.

**Rejected because:** an enemy spawning *after* `CharacterSpawnedEvent` already fired misses the notification, leaving `playerTransform` permanently null. Re-firing the event for late subscribers requires a "sticky event" mechanism Bill.Events doesn't have. Per-enemy subscribe also needs cleanup on pool return to avoid leaks.

**Chosen:** hybrid lazy-fetch — `EnemyBase.ResolvePlayerTransform()` does `FindGameObjectWithTag` on demand from the subclass `Update` tick, caches the result, returns null while no player exists.
- Self-contained: works whether enemy or player spawns first.
- Cached after first hit: O(1) hot path, no per-frame Find churn.
- No subscribe/unsubscribe lifetime management on transient pooled enemies.

**Where event-driven IS correct:** long-lived UI subscribers like `HudPanel` use `CharacterSpawnedEvent` because they exist from frame 0 (registered with `MythfallPanelRegistry` before any state.Enter), and the event fires after they're listening. HUD also keeps `SyncFromActivePlayer()` (FindGameObjectWithTag fallback) for the case where the panel shows late and the player already exists.

**Architecture Rule 2 (Bill.Events for communication) reading:** the rule prohibits tight-coupling component references for communication. Self-discovery of a singleton scene entity (the player) by transient pooled objects via tag is not communication; it's identity resolution. Event-driven would actually couple us *more* tightly to a specific spawn ordering.

---

## 2026-04-29 — GameplaySpawner uses Instantiate, not Bill.Pool (Sprint 1 Day 3)

**Context:** Day 3 added `GameplaySpawner` to spawn the player into GameplayScene from `InventoryService.Data.currentCharacterId` → `CharacterDataSO.characterPrefab`. CLAUDE.md Rule 3 says "no Instantiate/Destroy — use Bill.Pool".

**Decision:** `GameplaySpawner` uses `Object.Instantiate(charData.characterPrefab, ...)` directly. The player is **not** registered in Bill.Pool.

**Why:** Rule 3 targets repeated spawn/destroy churn (enemies, projectiles, VFX). The player spawns exactly once per gameplay scene load, dies once, then the entire scene reloads on Retry. There is no churn.

**Cost of pooling the player would be:**
- Add `OnSpawn()` re-init logic to `PlayerBase`: reset `PlayerHealth.CurrentHP`/`MaxHP`, re-create `RuntimeCharacterStats`, clear iFrame timer, reset `Animator` state, zero `CharacterLocomotion.PlayerVelocity`, re-arm `PlayerCombatBase.attackTimer`. Pool reuse hands you back a GameObject in whatever state the previous run left it in — Awake/Start do not re-fire.
- Manage pool warm size and retain GO across scene unloads (DontDestroyOnLoad scope creep).

**Cost of Instantiate:** one prefab instantiation per scene load. Negligible.

**Pattern going forward:**
- `Bill.Pool` for repeated transient spawns: enemies, projectiles, damage numbers, VFX, XP gems.
- `Object.Instantiate` for singleton scene-bound entities: the player. Lifetime tied to scene; reload destroys + recreates → guaranteed clean state.

If a future sprint adds player respawn *without* scene reload (e.g. checkpoint revive mid-run), revisit this — that'd be the trigger to either pool the player or implement an explicit `PlayerBase.RestoreToFullState()` reset path.

---

## 2026-04-29 — Sprint2Setup prefab generation tool fixes (Sprint 1 Day 3)

**Context:** While preparing Sprint 1 Day 3 (UI panels + state flow + pool-spawned character flow via `CharacterSelectState`), we discovered `Sprint2Setup.cs` had two bugs sharing the same anti-pattern: serialized config was applied to the **scene instance** instead of being baked into the **prefab asset** before `SaveAsPrefabAsset`.

**Bug 1 — `groundLayer` lost on prefab assets**
- `ConfigureKaiGroundLayer(kaiInstance)` ran on the scene instance only. Lyra was never configured.
- Result: both player prefab YAMLs had `groundLayer.m_Bits = 0` → `CharacterLocomotion.IsGrounded()` returned false forever for any pool-spawned copy.
- Symptom masked previously: scene-dragged Kai inherited the override `m_Bits = 1` from a manual editor edit. Day 3's `Bill.Pool.Spawn` would have hit the unmasked prefab default.

**Bug 2 — Swarmer Kinematic Rigidbody not regenerated**
- Day 2's verified fix added a Kinematic Rigidbody on the Swarmer prefab manually in the Editor.
- `Sprint2Setup.BuildSwarmerPrefab` never added one — re-running the tool would silently regenerate a Swarmer without the Rigidbody and break combat (push back, no trigger events) per the 2026-04-29 entry below.

**Decision:** All serialized config for prefab assets must be set inside the build function on the in-memory root object **before** `SaveAsPrefabAsset`. Never rely on scene-instance modifications surviving onto the prefab asset.

**Fix applied to `Sprint2Setup.cs`:**
1. Renamed `ConfigureKaiGroundLayer(GameObject)` → `ConfigurePlayerGroundLayer(GameObject)` (generic for any `CharacterLocomotion`-bearing root).
2. Called `ConfigurePlayerGroundLayer(root)` inside `BuildPlayerPrefab` immediately after `AddComponent<CharacterLocomotion>()` — runs for both Kai and Lyra at prefab build time.
3. Removed the now-redundant scene-instance call after `InstantiatePrefab`.
4. `BuildSwarmerPrefab` now adds `Rigidbody` with `isKinematic = true` and `useGravity = false` before save. No `FreezeRotation` constraints — match the Day 2 verified spec exactly (kinematic body ignores physics torque, and `SwarmerEnemy.Update` writes `transform.rotation` directly, so constraints are redundant).

**Pattern lesson for future setup tools:** treat `Sprint*Setup` as a regeneration tool, not a one-time scaffold. Anything that has to survive a regen must be set on the in-memory root before `SaveAsPrefabAsset`. Anything applied via `SerializedObject` on a scene instance after instantiation is a one-shot manual touch — it does NOT propagate to the asset.

**Companion patches (applied 2026-04-29 to existing prefab YAMLs):** `Kai.prefab` line 282 and `Lyra.prefab` line 93: `groundLayer.m_Bits 0 → 1`. These match what the fixed `Sprint2Setup` would now generate, so a future regen produces a clean diff.

---

## 2026-04-29 — Enemies require a Kinematic Rigidbody (Sprint 1 Day 2 verification)

**Context:** Sprint 1 Day 2 in-editor smoke test surfaced two coupled bugs that traced to the same root cause:
1. Swarmers physically pushed Kai's CharacterController off the ground plane.
2. Kai's `MeleeCombat.Execute()` fired correctly, but no damage ever landed on Swarmers.

**Root cause:** the Swarmer prefab had a solid `CapsuleCollider` and **no Rigidbody**.
- Swarmers move via `transform.position += direction * speed * Time.deltaTime` ([SwarmerEnemy.cs:65-71](../Assets/Mythfall/Scripts/Enemy/SwarmerEnemy.cs#L65-L71)). Without a Rigidbody, the physics engine treats them as moving static colliders, and resolves any overlap with a CharacterController by shoving the CC outward — that's the push.
- Unity trigger callbacks (`OnTriggerEnter`) fire only when **at least one of the two overlapping colliders has a Rigidbody (or a CharacterController on the same GameObject)**. Kai's hitbox SphereCollider sits on a child `Hitbox` GameObject, separate from the root CC, so the CC didn't count for that pair. With no Rigidbody on either side, no trigger event ever fired — Kai swung through Swarmers harmlessly.

**Decision:** every enemy prefab in Mythfall MUST have a **Kinematic Rigidbody (`isKinematic = true`, `useGravity = false`)** on the same GameObject as its primary collider.
- Kinematic = Rigidbody is moved by `transform`/scripts, not by physics forces. This is exactly what the existing `transform.position +=` movement assumes.
- Kinematic Rigidbody **does not resolve overlaps against CharacterControllers** the way a static collider does → fixes the push.
- Kinematic Rigidbody **does enable trigger callbacks** for both directions of an overlap → fixes damage detection.

**Applies to:** SwarmerEnemy (fixed 2026-04-29), and all future enemies — BruteEnemy, ShooterEnemy, BossEnemy in Sprint 2. When `Sprint2Setup.cs` (or any future enemy-prefab generator) creates a new enemy, it must add the Kinematic Rigidbody automatically.

**Why not the alternative — collision matrix tweak:** disabling Player↔Enemy layer collision via `Physics.IgnoreLayerCollision` would solve the push but not the trigger-event issue, and would also disable the hitbox→enemy detection we *do* want. Kinematic Rigidbody is the single fix that addresses both directions cleanly.

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
