# 🛏️ HANDOFF — 2026-05-12 (end of Sprint 2 Day 1 code session)

> Snapshot at end of session so future-you (or the next agent) can resume cold.
> Focus is "what to do next time you sit down", not full history.

---

## ✅ WHERE WE ARE

| Sprint | Status | Notes |
|---|---|---|
| **S0 — Setup + Bootstrap + Localization** | 🟢 Done | Bill ready, VN/EN localization. |
| **S1 — Player + Single Enemy + Scene Loop** | 🟢 Done | Verified end-to-end Editor 2026-05-12. |
| **S2 Day 1 — Enemy Variety + AI State Machine** | 🟢 Done (code-side) | Awaiting user Editor build + smoke test. **DO THIS NEXT.** |
| S2 Day 2 — Boss Fight (Rotwood) | 🟡 Not Started | Tomorrow. |
| S2 Day 3 — Polish Layer 1 (HitStop, Shake, Damage Numbers, Flash) | 🟡 Not Started | After Day 2. |
| S3 / S4 | 🟡 Not Started | — |

---

## 🎯 WHEN YOU SIT DOWN AGAIN — FINISH SPRINT 2 DAY 1 IN THE EDITOR

Code is complete. Remaining work is **Unity Editor only**:

### 1. Re-run Sprint 2 setup tool to add KnockbackReceiver to Kai/Lyra

```
Tools → Mythfall → Sprint 2 — Build Placeholder Prefabs
```

**Verify after run:**
- `Assets/Mythfall/Prefabs/Players/Kai.prefab` Inspector → `KnockbackReceiver` component present
- `Assets/Mythfall/Prefabs/Players/Lyra.prefab` → same

### 2. Run Sprint 2 variety setup tool (new)

```
Tools → Mythfall → Sprint 2 — Build Enemy Variety Prefabs
```

**Verify after run:**
- `Resources/Prefabs/Enemies/Brute.prefab` (purple capsule, taller)
- `Resources/Prefabs/Enemies/Shooter.prefab` (yellow slimmer, Muzzle child)
- `Resources/Prefabs/Projectiles/EnemyProjectile.prefab` (orange sphere)
- `Resources/Enemies/Brute_Data.asset` + `Shooter_Data.asset`
- `Materials/Brute_Mat.mat` + `Shooter_Mat.mat` + `EnemyProjectile_Mat.mat`

### 3. Wire WaveSpawner in GameplayScene

[WaveSpawner.cs](Assets/Mythfall/Scripts/Gameplay/WaveSpawner.cs) was refactored from single-enemy to weighted mix. The GameObject needs:

- **Separate from `[GameplaySpawner]`** — create `[WaveSpawner]` empty GameObject with `WaveSpawner` component
- Add 4 child empty Transforms (SpawnPoint_0..3) around the map (e.g. ±8 on X and Z)
- Inspector:
  - `Spawn Points`: drag the 4 child Transforms
  - `Enemy Entries` size 3: `Enemy_Swarmer` w3, `Enemy_Brute` w1, `Enemy_Shooter` w1
  - `Enemies Per Wave` 5, `Wave Interval` 5, `Spawn On Start` ✓
  - `Boss Spawn Time` 60 (or shorter for testing), `Boss Pool Key` Enemy_Rotwood, `Stop Waves On Boss` ✓

### 4. Smoke test the variety

Press Play from MenuScene → Confirm Kai → expect:
- Wave 1 (immediate): 5 enemies mix of Swarmer/Brute/Shooter
- Swarmer chases continuously (re-chases if Kai walks away — Day 1 bug fix)
- Brute approaches slow, telegraphs 0.8s, slam knockbacks Kai (player gets shoved ~2-3m)
- Shooter kites 5-10m, fires orange projectile every 2s, projectile damages Kai
- @60s boss trigger log: `[WaveSpawner] Boss trigger fired — listener should spawn 'Enemy_Rotwood'.` (Day 2 wires actual spawn)

### 5. Decide gravity-juice (pending)

User found during Day 1 testing: setting Rigidbody non-kinematic + UseGravity gives projectiles emergent push-back + ragdoll-on-death feel for free. Two paths to decide tomorrow:

- **Path A** (recommend): update Sprint2Setup + Sprint2VarietySetup to default ALL enemy prefabs to non-kinematic + UseGravity + FreezeRotation X/Z + reasonable mass (Brute 4, Swarmer 2, Shooter 1.5) + drag 0.5
- **Path B**: keep kinematic default, user toggles per-prefab manually

---

## 🧠 KEY CHANGES THIS SESSION

### New code

```
Assets/Mythfall/Scripts/
  Enemy/
    EnemyBase.cs              ← REFACTORED — state machine (Idle/Chase/Attack/Stunned/Dying)
                                + TransitionTo + OnStateEnter/Exit + abstract TickState
                                + SetStatMultipliers (for EliteModifier)
    SwarmerEnemy.cs           ← MIGRATED to state machine + stuck-attack bug fixed
                                (TickAttack re-checks range after cooldown; OnAttackHit
                                always transitions back to Chase regardless of hit)
    BruteEnemy.cs             ← NEW — telegraph 0.8s + hit window 0.2s + recovery 0.5s
                                + AoE 2m slam + knockback player 10f
    ShooterEnemy.cs           ← NEW — kite 5-10m + projectile fire every 2s
    EliteModifier.cs          ← NEW — x3 HP / x2 dmg / 1.3x scale / red emission
    EnemyProjectile.cs        ← NEW — pooled ammo for Shooter
  Polish/
    KnockbackReceiver.cs      ← NEW — applies impulse via CharacterController.Move + decay
  Gameplay/
    WaveSpawner.cs            ← REFACTORED — WaveEntry[] weighted mix + boss timer fires
                                BossSpawnTriggeredEvent
    GameplaySpawner.cs        ← (unchanged from Sprint 1)
    CameraFollow.cs           ← Bill.IsReady gate fix in OnDisable
  Core/
    Events/GameEvents.cs      ← +BossSpawnTriggeredEvent +ScreenShakeEvent
    Bootstrap/GameBootstrap.cs ← shared TryRegister helper, 5 pools, Bill.IsReady gate
                                  fix in OnEnable
  UI/Panels/HudPanel.cs       ← Bill.IsReady gate fix in OnDestroy
  Editor/
    Sprint2Setup.cs           ← +KnockbackReceiver in BuildPlayerPrefab
    Sprint2VarietySetup.cs    ← NEW tool — scaffolds Brute + Shooter + EnemyProjectile
                                  prefabs + materials + EnemyDataSO assets
```

### New localization

- `enemy.brute.{name,desc}` — "Mộc Đoạt Tử" / "Wood-Render"
- `enemy.shooter.{name,desc}` — "Cung Ảo Quỷ" / "Spectral Archer"
- `enemy.rotwood.{name,desc}` — Day 2 boss

### Bug fixes during Day 1

1. **SwarmerEnemy stuck in Attack state when player dodges out of range** — TickAttack now re-checks distance after cooldown; OnAttackHit always TransitionsTo(Chase) regardless of whether damage applied.
2. **`Bill.Events?` null-conditional logs SERVICE NOT FOUND error** — the getter calls ServiceLocator.Get which logs error BEFORE returning null. Null check is too late. Fixed in GameBootstrap.OnEnable, CameraFollow.OnDisable, HudPanel.OnDestroy by gating on `Bill.IsReady` instead. **5-6 other entry points (Awake/Start subscribing to GameReady) still use the pattern but only log on cold-boot race — cosmetic, defer to Sprint 4.**

---

## 🚧 KNOWN CARRY-OVERS

| Issue | Plan |
|---|---|
| Stale `Bill.Events?.Subscribe` pattern in GameplaySpawner / SceneStateBinder / CombatDebugProbe / LocalizedText / RangedCombat (Pool variant) | Sprint 4 polish — cosmetic, only logs on cold-boot race timing |
| Elite spawn rolling not wired in WaveSpawner | Day 2 or Day 3 — needs design: pool-per-elite OR runtime AddComponent (and reset on pool return) |
| BossEnemy + VictoryState + BossSpawnTriggeredEvent listener | Day 2 |
| HitStop / ScreenShake / DamageNumber / MaterialFlash | Day 3 polish layer |
| HudPanel pause button no-op log | Sprint 4 |
| GameOverPanel stats fields empty (time/wave/kills) | Sprint 4 — needs RunStatsTracker |

---

## 📂 KEY PATHS CHEAT SHEET

```
Assets/Mythfall/Scripts/
  Enemy/{EnemyBase, SwarmerEnemy, BruteEnemy, ShooterEnemy, EnemyDataSO,
         EliteModifier, EnemyProjectile}.cs
  Polish/KnockbackReceiver.cs
  Gameplay/{Projectile, WaveSpawner, GameplaySpawner, CameraFollow}.cs
  Editor/{Sprint0Setup, Sprint1Setup, Sprint2Setup, Sprint2VarietySetup}.cs

Assets/Mythfall/Resources/
  Localization/lang_{vi,en}.json
  Characters/{Kai,Lyra}_Data.asset
  Enemies/{Swarmer,Brute,Shooter}_Data.asset
  Prefabs/Enemies/{Swarmer, Brute, Shooter}.prefab
  Prefabs/Projectiles/{Arrow, EnemyProjectile}.prefab

Assets/Mythfall/Prefabs/Players/{Kai, Lyra}.prefab    ← now include KnockbackReceiver
                                                        (regenerate via Sprint 2 setup)
Assets/Mythfall/Scenes/{Bootstrap, Menu, Gameplay}Scene.unity
```

---

## 🔁 QUICK RECIPES

### Re-run any Sprint setup tool (idempotent)
```
Tools → Mythfall → Sprint 0 — Run Setup
Tools → Mythfall → Sprint 0 — Verify Setup
Tools → Mythfall → Sprint 1 — Create Character Data
Tools → Mythfall → Sprint 2 — Build Placeholder Prefabs           ← regen Kai/Lyra
Tools → Mythfall → Sprint 2 — Setup GameplayScene for Test         ← (Day 2 test layout — skip if using WaveSpawner)
Tools → Mythfall → Sprint 2 — Build Enemy Variety Prefabs          ← Day 1 NEW
```

### Wipe save data (test first-launch flow)
PlayerPrefs.DeleteAll() from a debug button, or Edit → Clear All PlayerPrefs.

---

## 🛌 GOOD NIGHT

Day 1 code is fully signed off. The next session is Editor verification + decision on gravity-juice path. If smoke test passes, start Day 2 (BossEnemy + VictoryState + BossSpawnTriggeredEvent listener).

If something behaves weird → check [Docs/ARCHITECTURE_DECISIONS.md](Docs/ARCHITECTURE_DECISIONS.md) first, then this HANDOFF's "Known Carry-Overs" table.
