# 🛏️ HANDOFF — 2026-04-29 (end of Sprint 1 Day 3 code session)

> Snapshot at end of session so future-you (or the next agent) can resume cold.
> Focus is "what to do next time you sit down", not full history.

---

## ✅ WHERE WE ARE

| Sprint | Status | Notes |
|---|---|---|
| **S0 — Setup + Bootstrap + Localization** | 🟢 Done | Bill ready, VN/EN localization, BillStartup splash → MenuScene transition. APK build deferred. |
| **S1 Day 1 — Data + Player Components** | 🟢 Done | Stats, RuntimeStats, CharacterDataSO, PlayerHealth, TargetSelector, PlayerFacing, PlayerBase, MobileInputManager, events, state stubs. |
| **S1 Day 2 — Combat + Enemy + Spawner** | 🟢 Done | Combat loop verified end-to-end. Swarmer kinematic Rigidbody fix landed. |
| **S1 Day 3 — UI Panels + State Flow + Inventory + Spawner** | 🟢 Done (code-side) | All scripts written. Awaiting Editor build + smoke test. **DO THIS NEXT.** |
| S2 / S3 / S4 | 🟡 Not Started | — |

---

## 🎯 WHEN YOU SIT DOWN AGAIN — FINISH SPRINT 1 DAY 3 IN THE EDITOR

Code-side is complete. Remaining work is **Unity Editor only**:

### 1. Re-run Sprint 2 setup tool to regenerate prefabs

```
Tools → Mythfall → Sprint 2 — Build Placeholder Prefabs
```

**Verify after run:**
- `Assets/Mythfall/Prefabs/Players/Kai.prefab` → CharacterLocomotion → Ground Layer = "Default" (not "Nothing")
- `Assets/Mythfall/Prefabs/Players/Lyra.prefab` → same
- `Assets/Mythfall/Resources/Prefabs/Enemies/Swarmer.prefab` → Rigidbody component present, Is Kinematic = ✓, Use Gravity = ☐, Constraints all unchecked

If `git diff` on the regenerated prefabs shows anything beyond expected fields, paste the diff and adjust `Sprint2Setup.cs`.

### 2. Build UGUI hierarchies in MenuScene + GameplayScene

Each panel script has a `==================== UNITY HIERARCHY SETUP ====================` block at the top spelling out:
- Required GameObject tree (parent paths, anchor positions, sizes)
- Components to add per node
- Which `LocalizedText` keys go on which `TMP_Text` children
- Which Inspector serialized fields to assign to which GameObjects

**Panels to build in MenuScene:**
- `MainMenuPanel` ([MainMenuPanel.cs](Assets/Mythfall/Scripts/UI/Panels/MainMenuPanel.cs))
- `CharacterSelectPanel` ([CharacterSelectPanel.cs](Assets/Mythfall/Scripts/UI/Panels/CharacterSelectPanel.cs))
- `SettingsOverlay` ([SettingsOverlay.cs](Assets/Mythfall/Scripts/UI/Panels/SettingsOverlay.cs)) — higher canvas sort order so it sits on top of MainMenu

**Panels to build in GameplayScene:**
- `HudPanel` ([HudPanel.cs](Assets/Mythfall/Scripts/UI/Panels/HudPanel.cs))
- `GameOverPanel` ([GameOverPanel.cs](Assets/Mythfall/Scripts/UI/Panels/GameOverPanel.cs))
- `VirtualJoystick` ([VirtualJoystick.cs](Assets/Mythfall/Scripts/Input/VirtualJoystick.cs))

**Both scenes need:**
- Canvas (Screen Space - Overlay, Scale With Screen Size, Reference Resolution 1920x1080)
- GraphicRaycaster (auto-added with Canvas)
- EventSystem GameObject (auto-added with first Canvas)

### 3. Drop scene bootstrap GameObjects

| Scene | GameObject | Component(s) | Inspector |
|---|---|---|---|
| MenuScene | `[SceneBootstrap]` | `SceneStateBinder` | InitialState = `MainMenu` |
| GameplayScene | `[SceneBootstrap]` | `SceneStateBinder` | InitialState = `InRun` |
| GameplayScene | `[GameplaySpawner]` | `GameplaySpawner` | spawnPoint = optional child Transform (origin if unset) |

GameplayScene also needs `WaveSpawner` (already exists from Day 2) with spawn points around the map.

### 4. Smoke test the full loop

End-to-end DoD verification:

1. Boot from BootstrapScene → splash → MenuScene → MainMenu visible
2. Click **Play** → CharacterSelectPanel opens
3. Click **Kai** card → highlight visible → click **Confirm** → fade → GameplayScene loads → Kai spawns at origin → joystick moves Kai → auto-attacks Swarmers → HP bar decreases on hit
4. Die → DefeatState slow-mo → GameOverPanel
5. Click **Retry** → GameplayScene reloads → Kai re-spawns → continue
6. Die → Click **Hub** → MenuScene → MainMenu
7. Click **Settings** → SettingsOverlay → switch **English** → all panel text reflows VN→EN with no `[missing.key]` and no clipping → switch back to **Tiếng Việt** → diacritics render correctly (Lôi Phong, Vọng Nguyệt)
8. Quit Unity Play mode, re-enter → CharacterSelectPanel pre-selects last-saved character + language sticks

If anything misbehaves, the first place to look is Console — every panel + state + spawner logs its lifecycle on entry.

---

## 🧠 KEY ARCHITECTURE DECISIONS THIS SESSION

All entries appended to [Docs/ARCHITECTURE_DECISIONS.md](Docs/ARCHITECTURE_DECISIONS.md) (most recent at top):

1. **Enemy player-reference resolution: lazy-fetch over event subscribe** — `EnemyBase.ResolvePlayerTransform()` handles either spawn order without sticky-event mechanics.
2. **GameplaySpawner uses Instantiate, not Bill.Pool** — Rule 3 exception documented for singleton scene-bound entities.
3. **Sprint2Setup prefab generation tool fixes** — config baked into prefab asset at build time, not on scene instances after instantiation.

UGUI route (custom `MythfallPanelRegistry`, NOT `Bill.UI`/UI Toolkit) was selected at the top of the session per user direction. Each panel inherits `MythfallPanelBase` which uses `CanvasGroup` (not `SetActive`) for hide so panels stay registered while invisible.

---

## 🚧 KNOWN CARRY-OVERS

| Issue | Plan |
|---|---|
| HudPanel pause button is a no-op log | Sprint 4 polish — implement pause overlay (Time.timeScale=0, settings reachable, resume) |
| GameOverPanel stats display empty (time/wave/kills/level) | Sprint 4 polish — needs `RunStatsTracker` to populate on death |
| 0.3s iFrame absorbs simultaneous Swarmer hits | By design Day 2; revisit Sprint 2 if combat feels too forgiving |
| Animation invisible | Placeholder Animator clips empty. Real anim arrives Sprint 2. |
| 3 `No Theme Style Sheet set to PanelSettings` warnings on Bill UI Toolkit | We don't use Bill.UI; warnings are harmless. Defer Sprint 4. |
| BillInspector duplicate menu item warning | Internal, ignore. |
| `ModularTopDown.PlayerInputHandler` legacy `Input.GetAxisRaw` | We use `MobileInputManager` + `VirtualJoystick`; framework's handler is unused. |
| APK build / Android device test | Deferred per user request. |

---

## 📂 KEY PATHS CHEAT SHEET

```
Assets/Mythfall/Scripts/
  Core/Bootstrap/GameBootstrap.cs            # RegisterPools + RegisterGameLayer (Inventory + 4 states)
  Core/Events/GameEvents.cs                  # +CharacterSpawnedEvent
  Core/States/MythfallStates.cs              # 4 states fully wired
  Core/States/SceneStateBinder.cs            # per-scene initial state binder (enum)
  Core/Localization/LocalizationService.cs   # unchanged
  Characters/{CharacterStats, RuntimeStats, CharacterDataSO}.cs
  Enemy/{EnemyDataSO, EnemyBase, SwarmerEnemy}.cs       # EnemyBase.ResolvePlayerTransform added
  Gameplay/{Projectile, WaveSpawner, GameplaySpawner}.cs   # GameplaySpawner new
  Input/{MobileInputManager, VirtualJoystick}.cs        # VirtualJoystick new
  Inventory/{PlayerData, InventoryService}.cs           # both new
  Player/{PlayerBase, PlayerHealth, PlayerFacing, TargetSelector,
          PlayerCombatBase, MeleeCombat, RangedCombat,
          MeleePlayer, RangedPlayer, HitboxRelay}.cs
  Skills/SkillCore.cs                        # stub — Sprint 3
  UI/{LocalizedText, LocalizationTester, MythfallPanelRegistry, MythfallPanelBase}.cs
  UI/Panels/{MainMenu, CharacterSelect, Hud, GameOver, SettingsOverlay}Panel.cs
  Editor/{Sprint0Setup, Sprint1Setup, Sprint2Setup}.cs   # Sprint2Setup root-cause fixed

Assets/Mythfall/Resources/
  Localization/lang_{vi,en}.json              # all Day 3 keys already present
  Characters/{Kai,Lyra}_Data.asset
  Enemies/Swarmer_Data.asset
  Prefabs/Enemies/Swarmer.prefab              # has Kinematic Rigidbody
  Prefabs/Projectiles/Arrow.prefab

Assets/Mythfall/Prefabs/Players/{Kai,Lyra}.prefab   # groundLayer.m_Bits = 1 (Default)
Assets/Mythfall/Scenes/{Bootstrap,Menu,Gameplay}Scene.unity
```

---

## 🔁 QUICK RECIPES

### Re-run any Sprint setup tool (idempotent)
```
Tools → Mythfall → Sprint 0 — Run Setup
Tools → Mythfall → Sprint 0 — Verify Setup
Tools → Mythfall → Sprint 1 — Create Character Data
Tools → Mythfall → Sprint 2 — Build Placeholder Prefabs
```

### Find architectural breadcrumbs
```
grep -rn "MYTHFALL ADDITIVE" Assets/Mythfall/Scripts/LocomotionModular/
```

### Wipe save data (test first-launch flow)
Edit → Clear All PlayerPrefs (Unity menu), or `PlayerPrefs.DeleteAll()` from a debug button.

---

## 🛌 GOOD NIGHT

Day 3 code is fully signed off. The next session is Editor-only — build hierarchies per header specs, drop scene bootstraps, smoke test the loop. If it all works, Sprint 1 closes and you're starting Sprint 2 (combat variety + boss + polish layer 1) per [Sprints/SPRINT_2_COMBAT_FEEL.md](Sprints/SPRINT_2_COMBAT_FEEL.md).

If something compiles but behaves weird → check [Docs/ARCHITECTURE_DECISIONS.md](Docs/ARCHITECTURE_DECISIONS.md) first, then this HANDOFF's "Known Carry-Overs" table.
