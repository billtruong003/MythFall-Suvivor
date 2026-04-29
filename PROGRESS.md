# 📊 PROGRESS — Mythfall: Survivor Vertical Slice

> **Goal:** Vertical slice playable trong 2 tuần (10-12 ngày)
> **Total Sprints:** 5 | **Status:** 🟡 Pre-development
> **Started:** [Update khi bắt đầu]
> **Target Completion:** [Started + 14 days]

---

## 🎯 SPRINT OVERVIEW

| Sprint | Focus | Days | Status | Build Output |
|---|---|---|---|---|
| **S0** | Project Setup + Framework Boot | 0.5-1 | 🟢 Done | Empty scenes load, Bill ready |
| **S1** | Player + Single Enemy + Scene Flow | 3 | 🟢 Done (code-side) | Walk, attack, kill enemy, full loop |
| **S2** | Combat Variety + Boss + Polish Layer 1 | 3 | 🟡 Not Started | 3+ enemies, boss fight, hitstop/shake |
| **S3** | Skills + In-Run Progression | 3 | 🟡 Not Started | Skills + level up + 8 upgrade cards |
| **S4** | Polish + Audio/VFX + Final Loop | 2-3 | 🟡 Not Started | Vertical slice playable build |

**Total:** 11.5-13 ngày = ~2 tuần với buffer.

**Status Legend:**
- 🟢 Done
- 🟡 Not Started
- 🔵 In Progress
- 🔴 Blocked
- ⚪ Skipped

---

# SPRINT 0 — Project Setup
**Days:** 0.5-1 | **Status:** 🟢 Done (APK build deferred per user)

**Goal:** Unity project chạy được, framework integrated, **localization service ready**, scene flow stub.

**Environment notes (2026-04-28):**
- Unity actual: **6000.3.8f1** (Unity 6.3 LTS) — sprint doc says 6.3.10f1 but this is the LTS version user is on. No API breakage observed in BillGameCore for Sprint 0 scope.
- Newtonsoft.Json added to `Packages/manifest.json` v3.2.1.
- **API doc fix:** Sprint 0 doc + `Docs/LOCALIZATION_GUIDE.md` reference namespace `BillTheDev.GameCore` and `BillTheDev.GameCore.Bootstrap`, but actual code uses `BillGameCore` (single namespace, no sub-namespace for Bootstrap). All Mythfall code uses the actual namespace. Docs should be updated separately.
- **Bootstrap flow deviation:** Sprint doc Task 0.5 sets `defaultGameScene: "MenuScene"` AND Task 0.6 has `GameBootstrap.Initialize()` also calling `Bill.Scene.Load("MenuScene")` — that would double-load. Actual config sets `defaultGameScene = ""` and lets `GameBootstrap` own the first transition (so it happens AFTER our services register). Confirmed correct via Bill.cs Phase2 source.
- **ModularTopDown framework:** missing from `Assets/`. Sprint 0 does not need it (Locomotion only used in Sprint 1+). User should drop it in before Sprint 1 starts.

## High-Level Tasks
- [x] ~~Create Unity 6.3.10f1 project với URP template~~ — Project exists, Unity 6000.3.8f1 (LTS)
- [x] Import BillGameCore + ~~ModularTopDown~~ + VAT — BillGameCore + VAT present, ModularTopDown deferred to Sprint 1
- [x] Setup Mythfall folder structure under `Assets/Mythfall/`
- [x] Code: GameBootstrap, LocalizationService, LocalizedText, LanguageChangedEvent
- [x] Localization JSON: lang_vi.json + lang_en.json with `ui.menu.*` + `ui.common.*` stub keys
- [x] Editor script: `Tools → Mythfall → Sprint 0 — Run Setup` creates 3 scenes, BillBootstrapConfig, Build Settings, Android Player Settings (IL2CPP, ARM64, min API 28)
- [ ] **USER:** Open Unity, wait for Newtonsoft.Json to install, run `Tools → Mythfall → Sprint 0 — Run Setup`
- [ ] **USER:** Run `Tools → Mythfall → Sprint 0 — Verify Setup` — expect 6/6 OK
- [ ] **USER:** Press Play in BootstrapScene — verify console logs `[Bill] Ready`, `[GameBootstrap] LocalizationService registered, language: vi`, then auto-transitions to MenuScene
- [ ] **USER:** First APK build, install on device, verify boot

## Definition of Done
- [ ] APK build + install thành công
- [ ] Game boot không error/crash
- [ ] Bill.IsReady = true
- [ ] **LocalizationService loaded (vi default), Get("ui.menu.play") returns "Chơi"** ✨
- [ ] Scene flow: Bootstrap → Menu (auto-transition)

📄 **Detail:** `Sprints/SPRINT_0_SETUP.md`

---

# SPRINT 1 — Foundation: Player + Enemy + Loop
**Days:** 3 | **Status:** 🟢 Done (code-side, awaiting user Editor build + smoke test)

**Goal:** Có thể chơi end-to-end: Menu → chọn 1 character → vào gameplay → đánh Swarmer → chết hoặc thắng → quay menu. **Toàn bộ UI localized VN+EN với star rating display.**

## High-Level Tasks

### Day 1 — Data + Player Components ✅
- [x] Modify CharacterLocomotion (5 additive lines) — see ARCHITECTURE_DECISIONS.md
- [x] Skills/ scaffolding (SkillDataSO + ISkillExecution + SkillContext stubs)
- [x] Characters: CharacterStats (StatType + CombatRole + CharacterBaseStats), RuntimeCharacterStats, CharacterDataSO
- [x] Player components: PlayerHealth, TargetSelector, PlayerFacing, PlayerCombatBase (stub), PlayerBase
- [x] Input: MobileInputManager
- [x] Events: PlayerDamagedEvent + PlayerDiedEvent
- [x] States: MainMenuState/CharacterSelectState/InRunState/DefeatState (stubs, Day 3 fills logic)
- [x] Localization: character.kai/lyra.* + ui.character_select.* + ui.hud.* + ui.game_over.* + ui.settings.*
- [x] Sprint1Setup editor menu — auto-generate Kai_Data + Lyra_Data SO assets
- [ ] **USER:** open Unity → wait compile clean → `Tools → Mythfall → Sprint 1 — Create Character Data`

### Day 2 — Combat + Enemy + Spawner ✅ (code-side)
- [x] PlayerBase getters (Animator, TargetSelector, Facing) — used by combat
- [x] HitboxRelay + MeleeCombat (hitbox + 120° arc + animation events OnHitboxEnable/Disable + timer fallback)
- [x] RangedCombat (projectile via Bill.Pool, animation event OnArrowRelease + timer fallback)
- [x] MeleePlayer + RangedPlayer concrete classes
- [x] Gameplay/Projectile (pool, pierce, lifetime, OnTriggerEnter)
- [x] Enemy/{EnemyDataSO, EnemyBase, SwarmerEnemy}
- [x] Gameplay/WaveSpawner (5 swarmers / 5s, configurable)
- [x] GameEvents extended: EnemyHitEvent + EnemyKilledEvent + CharacterSelectedEvent
- [x] GameBootstrap: AddStep "Register Pools" with Resources.Load fallback warnings
- [x] Sprint2Setup editor menu — Layers (Enemy=8, Projectile=9), AnimatorControllers (Player + Enemy with full param set), 4 URP Lit materials, 4 placeholder prefabs (Kai red capsule + hitbox child, Lyra teal capsule + muzzle, Swarmer gray capsule, Arrow), Swarmer_Data SO, wire CharacterDataSO.characterPrefab refs
- [x] enemy.swarmer.* localization keys (vi + en)
- [x] **USER:** open Unity → wait compile → run `Tools → Mythfall → Sprint 2 — Build Placeholder Prefabs`
- [x] **USER:** verify in Project: 4 prefabs + 2 controllers + 4 materials + Swarmer_Data exist; Kai_Data/Lyra_Data have characterPrefab assigned
- [x] **USER:** test in GameplayScene: drop Kai prefab + Swarmer prefab(s) — Swarmer chases, Kai auto-attacks; HP bars (placeholder log) decrement
  - **Fix applied:** added Kinematic Rigidbody (UseGravity off) on Swarmer prefab → fixes 2 issues at once: (a) Swarmer's solid CapsuleCollider no longer pushes Kai's CharacterController off the map via physics resolution; (b) trigger events on Kai's hitbox SphereCollider now fire (Unity needs ≥1 Rigidbody participant for trigger detection). Future enemies (Brute, Shooter, Boss) must follow same pattern.

### Day 3 — Scene Flow + UI + Integration ✅ (code-side)
- [x] InventoryService + PlayerData (Bill.Save round-trip, JsonUtility, schema versioned)
- [x] 4 UGUI panel scripts (logic only — header comment in each spells out hierarchy + serialized refs):
  - MainMenuPanel (Play → CharacterSelectState; Settings → toggle SettingsOverlay)
  - CharacterSelectPanel (2 Kai/Lyra cards + Confirm → InventoryService.SetCurrent + fire CharacterSelectedEvent + GoTo InRun + load GameplayScene)
  - HudPanel (HP fill via PlayerDamagedEvent, character name via LocalizedText.SetKey, 4★ display, Pause stub)
  - GameOverPanel (Defeat title/subtitle + Retry + Hub)
- [x] SettingsOverlay (VN/EN buttons via InventoryService.SetPreferredLanguage; Music/SFX sliders → Bill.Audio.SetVolume best-effort with try/catch + PlayerPrefs persist)
- [x] VirtualJoystick UGUI → writes MobileInputManager.MoveVector
- [x] MythfallPanelRegistry (static, CanvasGroup-based, desiredVisible survives scene unload) + MythfallPanelBase
- [x] MythfallStates filled in (Enter/Exit panel show/hide, DefeatState timeScale 0.3x, InRunState resets MobileInputManager)
- [x] SceneStateBinder (drop on per-scene GameObject; enum picks initial state)
- [x] GameBootstrap.RegisterGameLayer (mirrors RegisterPools defer-until-Bill-ready pattern; registers InventoryService + 4 Mythfall states)
- [x] CharacterSelectedEvent — already declared Day 2; CharacterSelectPanel fires it on confirm
- [x] CharacterSpawnedEvent + GameplaySpawner (Instantiate-based, fires event on player spawn — see ARCHITECTURE_DECISIONS.md 2026-04-29)
- [x] EnemyBase lazy-fetch player ref via ResolvePlayerTransform (handles GameplaySpawner-vs-WaveSpawner ordering race)
- [x] HudPanel subscribes CharacterSpawnedEvent for fresh PlayerHealth bind on retry flow
- [x] Sprint2Setup root-cause fix (groundLayer baked into Kai+Lyra prefab build, Swarmer kinematic Rigidbody added — see ARCHITECTURE_DECISIONS.md 2026-04-29)
- [x] Kai.prefab + Lyra.prefab YAML patch (groundLayer m_Bits 0→1)
- [ ] **USER:** open Unity → wait compile → re-run `Tools → Mythfall → Sprint 2 — Build Placeholder Prefabs` → verify Kai/Lyra Ground Layer = Default + Swarmer Rigidbody exists
- [ ] **USER:** build GameObject hierarchy in MenuScene + GameplayScene per each panel script's header comment, assign SerializeField refs
- [ ] **USER:** drop SceneStateBinder on MenuScene GO (Initial = MainMenu) + GameplayScene GO (Initial = InRun)
- [ ] **USER:** drop GameplaySpawner on GameplayScene "[GameplaySpawner]" GO with optional spawnPoint child Transform (Vector3.zero fallback if unset)
- [ ] **USER:** smoke test full loop: boot → Play → Kai → Gameplay (auto-spawn) → die → Retry / Hub. Switch VN/EN in Settings, verify all panels update.

## Definition of Done
- [ ] User flow: Menu → CharacterSelect → Gameplay → Death → Menu (loop)
- [ ] 2 character playable (Kai melee, Lyra ranged) — display 4★
- [ ] Auto-attack hits enemies → enemies die
- [ ] HP bar updates khi bị damage
- [ ] Save/load chosen character
- [ ] **Switch VN ↔ EN works on all panels** ✨
- [ ] Vietnamese diacritics render (Lôi Phong, Vọng Nguyệt visible) ✨
- [ ] 30+ FPS với 20 enemies on screen

📄 **Detail:** `Sprints/SPRINT_1_FOUNDATION.md`

---

# SPRINT 2 — Combat Variety + Boss + Polish Layer 1
**Days:** 3 | **Status:** 🟡 Not Started

**Goal:** Combat feel "juicy", có variety enemy, có boss fight epic.

## High-Level Tasks
- [ ] 2 enemy types thêm: BruteEnemy, ShooterEnemy
- [ ] Enemy AI state machine (Idle, Chase, Attack)
- [ ] Boss: BossEnemy với 2 phase (Rotwood)
- [ ] Boss fight trigger sau 60s wave
- [ ] Polish layer 1: Hitstop (50ms on crit), Screen shake, Damage numbers
- [ ] Material flash on hit (red 0.15s)
- [ ] Knockback system

## Definition of Done
- [ ] 3 enemy types với behavior phân biệt rõ
- [ ] Boss fight có phase transition visible
- [ ] Combat "feel" tốt: hit có impact, crit có moment
- [ ] Damage numbers floating từ enemy bị hit
- [ ] FPS stable với 50+ enemies + boss

📄 **Detail:** `Sprints/SPRINT_2_COMBAT_FEEL.md`

---

# SPRINT 3 — Skills + In-Run Progression
**Days:** 3 | **Status:** 🟡 Not Started

**Goal:** Active skills + passive skills + level up system + 8+ upgrade cards.

## High-Level Tasks
- [ ] Skill system: SkillDataSO + ISkillExecution
- [ ] 2 active skill: BerserkerRush (Kai), OverchargeShot (Lyra)
- [ ] 2 passive skill: Bloodlust (Kai), MarkedTarget (Lyra)
- [ ] PlayerSkillManager runtime
- [ ] Skill button UI + cooldown indicator
- [ ] XPGem + magnet pickup
- [ ] LevelSystem (XP threshold formula)
- [ ] UpgradeCardSO + 8 starter cards
- [ ] UpgradeSystem draw 3 + apply
- [ ] CardSelectionPanel UI

## Definition of Done
- [ ] Player level up trong run → 3 cards xuất hiện → pick → effect apply
- [ ] Active skill button hoạt động + có cooldown
- [ ] Passive skill trigger đúng (Bloodlust khi HP < 50%)
- [ ] 8 upgrade card distinct effect
- [ ] Run feel "build-ready" — mỗi run khác nhau

📄 **Detail:** `Sprints/SPRINT_3_SKILLS_PROGRESSION.md`

---

# SPRINT 4 — Polish + Audio/VFX + Final Loop
**Days:** 2-3 | **Status:** 🟡 Not Started

**Goal:** Vertical slice ready for playtest — combat feel polished, audio/VFX integrated, **localization VN+EN complete**, no critical bugs.

## High-Level Tasks
- [ ] VFX integration: hit spark, death burst, level up ring, skill VFX
- [ ] Audio integration: BGM + 15 SFX
- [ ] Camera effects: smooth follow, shake on big hits
- [ ] Game over polish: defeat screen, retry button
- [ ] Victory polish: rewards display, back to menu
- [ ] Performance optimization pass
- [ ] Bug fixing pass
- [ ] **Localization final pass — VN+EN coverage 100%** ✨
- [ ] Final APK build

## Definition of Done
- [ ] Build APK chạy 10+ phút không crash
- [ ] Combat feel: 8/10 satisfaction trong playtest
- [ ] Audio không silent moment, BGM loop seamless
- [ ] VFX không tank FPS
- [ ] **No hardcoded strings, no [missing.key] visible, EN+VN switch works flawlessly** ✨
- [ ] Vertical slice playable end-to-end, ready để show

📄 **Detail:** `Sprints/SPRINT_4_POLISH_LOOP.md`

---

# 📈 OVERALL ESTIMATES

| Sprint | Estimated Hours | Days @ 8h/day |
|---|---|---|
| S0 | 6h | 0.75 |
| S1 | 24h | 3 |
| S2 | 24h | 3 |
| S3 | 24h | 3 |
| S4 | 18h | 2.25 |
| **Total** | **96h** | **~12 ngày** |

**Buffer (1.2x for unforeseen):** ~115h ≈ **14-15 ngày = 2 tuần**

---

# 🎯 SUCCESS CRITERIA — Vertical Slice

End of Sprint 4, user phải:
- [ ] Build APK không crash
- [ ] Cài lên Android device
- [ ] Chơi 5-10 phút không issue
- [ ] Combat feel "fun" — muốn chơi lại
- [ ] Show được cho 10 người playtest, get feedback
- [ ] Decide: pivot, polish, hoặc continue full game development

---

# 📝 STATUS LOG

> Update format: `[YYYY-MM-DD] Sprint X status: <notes>`

- 2026-04-28 — Workflow package created, ready to start Sprint 0
- 2026-04-28 — Sprint 0 code-side complete. Created Mythfall folder structure, GameBootstrap, LocalizationService (Newtonsoft.Json), LocalizedText, LanguageChangedEvent, lang_vi.json + lang_en.json, Sprint0Setup editor tool. Logged 2 doc-vs-actual mismatches (namespace `BillGameCore` not `BillTheDev.GameCore`; defaultGameScene must be empty so GameBootstrap owns first transition). Awaiting user to run editor setup + verify boot + APK build.
- 2026-04-28 — Sprint 0 → 🟢 Done. Verified runtime: `[Bill] Ready. 14 services in 349ms`, LocalizationService loaded vi (8 keys) + en fallback (8 keys), HealthCheck all OK, BillStartup ran 2 steps and transitioned Bootstrap → MenuScene. Refactored GameBootstrap to use BillStartup AddStep pattern; added splash Logo + CanvasGroup auto-build in Sprint0Setup; added LocalizationTester temp script. **Deferred:** APK build (per user); 3 `No Theme Style Sheet set to PanelSettings` warnings (defer Sprint 1 UI work); BillInspector duplicate menu item (internal, ignored); 2 AudioListener warning in Bootstrap from orphan Unity-created Camera GO (dedupe logic added but user opted not to re-run setup). Sprint 1 → 🔵 In Progress.
- 2026-04-28 — Sprint 1 Day 1 code-side complete. Modified CharacterLocomotion (5 additive changes — see Docs/ARCHITECTURE_DECISIONS.md). Created Skills/SkillCore.cs (SkillDataSO + ISkillExecution + SkillContext stubs), Characters/{CharacterStats, RuntimeCharacterStats, CharacterDataSO}.cs, Player/{PlayerHealth, TargetSelector, PlayerFacing, PlayerCombatBase, PlayerBase}.cs, Input/MobileInputManager.cs, Core/Events/GameEvents.cs (PlayerDamagedEvent + PlayerDiedEvent), Core/States/MythfallStates.cs (4 state stubs). Expanded lang_vi.json + lang_en.json with character.kai/lyra.* + ui.character_select.* + ui.hud.* + ui.game_over.* + ui.settings.* keys. Sprint1Setup editor menu auto-generates Kai_Data + Lyra_Data ScriptableObjects with canonical stats (Kai: HP120/ATK15/Range1.8/Crit15%; Lyra: HP80/ATK20/Range10/Crit20%). **Awaiting user:** run `Tools → Mythfall → Sprint 1 — Create Character Data` to generate the 2 SO assets. Day 2 (combat + enemy + spawner) blocked on placeholder prefabs (will tackle next).
- 2026-04-28 — Sprint 1 Day 2 code-side complete. Combat/enemy stack: HitboxRelay, MeleeCombat (120° arc + animation event hooks OnHitboxEnable/Disable + timer fallback), RangedCombat (projectile spawn via OnArrowRelease + timer fallback), MeleePlayer + RangedPlayer concrete, Gameplay/Projectile (pool + pierce), Enemy/{EnemyDataSO, EnemyBase, SwarmerEnemy with chase+attack+OnAttackHit event}, Gameplay/WaveSpawner (5/5s). Extended GameEvents with EnemyHitEvent + EnemyKilledEvent + CharacterSelectedEvent. GameBootstrap registers `Enemy_Swarmer` + `Projectile_Arrow` pools via Resources.Load fallback. Sprint2Setup editor menu auto-generates: Layer 8=Enemy + Layer 9=Projectile, PlayerAnimator + EnemyAnimator AnimatorControllers (full param set), 4 URP Lit materials, 4 placeholder prefabs (Kai melee with hitbox child + Lyra ranged with muzzle + Swarmer + Arrow), Swarmer_Data SO, wires CharacterDataSO.characterPrefab refs. enemy.swarmer.* keys added to JSONs. **Animation event design:** all combat timing animation-event driven via DynamicAnimationEventHub (user wires UnityEvent in Inspector); each combat component has `useTimerFallback` flag (default true) to keep loop testable on placeholder Animator without clip events — flip false on real prefab once anim events wired. **Awaiting user:** run `Tools → Mythfall → Sprint 2 — Build Placeholder Prefabs` then test combat in GameplayScene.
- 2026-04-29 — Sprint 1 Day 3 spawner gap closed. GameplaySpawner created (Instantiate of CharacterDataSO.characterPrefab at scene-resident spawnPoint, fires CharacterSpawnedEvent). Pool route considered and rejected — pooling the player would require a `PlayerBase.OnSpawn()` re-init path (HP/stats/iFrame/animator/locomotion reset) that scene reload already gives us for free; documented in ARCHITECTURE_DECISIONS.md as a Rule-3 exception for singleton scene-bound entities. Race condition between GameplaySpawner (async on Bill ready) and WaveSpawner (also async) addressed via lazy-fetch in `EnemyBase.ResolvePlayerTransform()` called from SwarmerEnemy.Update — handles either spawn ordering, caches on first hit, no per-enemy event subscription overhead. HudPanel additionally subscribes CharacterSpawnedEvent on every show so retry flow rebinds HP fill to fresh PlayerHealth without relying on `SyncFromActivePlayer`'s FindGameObjectWithTag race. Sprint 1 → 🟢 Done code-side. Remaining work is all Editor: rebuild prefabs via Sprint 2 setup, build panel hierarchies per header specs, drop SceneStateBinder + GameplaySpawner in scenes, smoke test full loop with EN/VN switching.
- 2026-04-29 — Sprint 1 Day 3 code-side complete. Inventory layer (PlayerData POCO + InventoryService IInitializable, Bill.Save round-trip via JsonUtility, schema-versioned with stub MigrateData, language reconciliation makes PlayerData canonical over LocalizationService PlayerPref, defensive guards for empty owned list / orphan currentCharacterId / missing CharacterDataSO Resources). UGUI panel framework: MythfallPanelRegistry (static, CanvasGroup-based hide so panel stays registered while invisible, desiredVisible dict survives scene unload, Editor InitializeOnEnterPlayMode reset) + MythfallPanelBase ([RequireComponent CanvasGroup], OnPanelShown/Hidden hooks). 4 panels (MainMenu, CharacterSelect with [Serializable] CharacterCardWiring nested, Hud subscribing PlayerDamagedEvent + LocalizedText.SetKey for dynamic name, GameOver) + SettingsOverlay (VN/EN via InventoryService.SetPreferredLanguage, Music/SFX sliders best-effort Bill.Audio.SetVolume with try/catch + PlayerPrefs persist). VirtualJoystick UGUI (IPointerDown/Drag/Up → MobileInputManager.MoveVector, OnDisable resets so stale input doesn't bleed across scenes). MythfallStates filled in (DefeatState Time.timeScale 0.3x, InRunState resets MobileInputManager). SceneStateBinder (enum-based, defers to Bill ready) lets each scene declare its initial state without hardcoded GameBootstrap-vs-MenuScene coupling. GameBootstrap.RegisterGameLayer mirrors RegisterPools defer-until-Bill-ready pattern (registers InventoryService + 4 Mythfall states; called from Awake/Start/OnGameReady triggers). All UI text routes through LocalizedText component (CLAUDE.md Rule 8) — every required key already exists in lang_{vi,en}.json from Day 1. **Bonus tool fix:** Sprint2Setup.cs had two bugs sharing the "config-on-scene-instance-not-prefab-asset" anti-pattern — (1) Kai's groundLayer was set on the scene instance only, never on the asset, and Lyra never got it at all; (2) Swarmer's Day 2 kinematic Rigidbody fix only existed on the verified scene prefab, not in the regeneration tool. Both fixed by moving the configuration into BuildPlayerPrefab/BuildSwarmerPrefab BEFORE SaveAsPrefabAsset. Patched Kai.prefab + Lyra.prefab YAML (m_Bits 0 → 1) to match what Sprint2Setup would now generate. Documented in Docs/ARCHITECTURE_DECISIONS.md (2026-04-29 entry above the Day 2 RB entry). **Awaiting user:** re-run Sprint 2 setup to verify regenerated prefabs match the patched YAMLs, build GameObject hierarchies in MenuScene + GameplayScene per each panel script's header-comment spec, drop SceneStateBinder on per-scene Bootstrap GO, then smoke test full loop end-to-end with EN ↔ VN language switch.
- 2026-04-29 — Sprint 1 Day 2 verified end-to-end via diagnostic logging round. Initial smoke test surfaced two coupled bugs: (a) Kai pushed off the ground plane by Swarmers; (b) damage never landing despite Execute firing. Root cause for both: Swarmer prefab had a solid CapsuleCollider with no Rigidbody → physics engine treated it as a moving static collider that shoves Kai's CharacterController, AND Unity trigger events require ≥1 Rigidbody participant which neither the Hitbox child SphereCollider nor the Swarmer had. **User fix:** added a Kinematic Rigidbody (UseGravity off) on the Swarmer prefab. Re-test confirmed: Swarmers chase Kai, attack lands (Kai 115/120 after 4-Swarmer wave thanks to PlayerHealth's 0.3s iFrame collapsing simultaneous hits), Kai's swings land, Swarmers die and return to pool after the 1s death-anim delay. Animation invisible by design — capsule placeholders, no clips wired (Sprint 2 polish). 4 temp `[DIAG]` log sites added then removed cleanly. **Sprint 1 Day 2 → 🟢 Done. Sprint 1 Day 3 unblocked.**

---

*Update PROGRESS.md sau mỗi sprint completion. Đây là source of truth cho tiến độ project.*
