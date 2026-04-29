# 🎬 NEXT SESSION — Sprint 1 Day 2 Verification

> Mày làm 3 phase này theo thứ tự **A → B → C** rồi paste kết quả vào Claude.
> A = Setup checklist (manual trong Unity).
> B = Test checklist (smoke test combat).
> C = Session prompt cho Claude (paste sau khi A+B xong).

---

## 🅰️ PHASE A — SETUP CHECKLIST (Unity Editor)

### A.1 — Open Unity, verify clean compile
- [ ] Mở project `d:/Projects/MythfallsSurvivor` trong Unity 6000.3.8f1
- [ ] Đợi Unity compile + reimport assets (~30-60s)
- [ ] Mở Console (`Window → General → Console`)
- [ ] Click Clear → expect **0 errors (red), warnings OK**
- [ ] Nếu có error: screenshot Console + paste vào Phase C report

**Expected warnings (ignore):**
- `BillSetupWizard.cs` — 3 warnings về `PlayerSettings.GetScriptingDefineSymbolsForGroup` obsolete
- `Cannot add menu item 'Tools/BillInspector/Validation Window'`
- `No Theme Style Sheet set to PanelSettings` (×3 lúc Play, từ Bill UI Toolkit)

### A.2 — Run Sprint 2 placeholder-prefab generator
- [ ] Menu bar → `Tools → Mythfall → Sprint 2 — Build Placeholder Prefabs`
- [ ] Console expected output (đúng 1 dòng cuối):
  ```
  [Sprint2Setup] DONE. 4 prefabs + 2 AnimatorControllers + 4 materials + Swarmer_Data + Layer 'Enemy'/'Projectile' configured.
  ```
- [ ] Nếu có log `Layer slot 8 → 'Enemy'` thì OK (lần đầu chạy). Nếu báo `already named ... leaving alone` thì cũng OK.

### A.3 — Verify generated assets
Click qua Project window, check tồn tại:

- [ ] `Assets/Mythfall/Prefabs/Players/Kai.prefab`
- [ ] `Assets/Mythfall/Prefabs/Players/Lyra.prefab`
- [ ] `Assets/Mythfall/Resources/Prefabs/Enemies/Swarmer.prefab`
- [ ] `Assets/Mythfall/Resources/Prefabs/Projectiles/Arrow.prefab`
- [ ] `Assets/Mythfall/Animations/PlayerAnimator.controller`
- [ ] `Assets/Mythfall/Animations/EnemyAnimator.controller`
- [ ] `Assets/Mythfall/Materials/Kai_Mat.mat` (color đỏ)
- [ ] `Assets/Mythfall/Materials/Lyra_Mat.mat` (color teal)
- [ ] `Assets/Mythfall/Materials/Swarmer_Mat.mat` (color xám)
- [ ] `Assets/Mythfall/Materials/Arrow_Mat.mat` (color vàng nhạt)
- [ ] `Assets/Mythfall/Resources/Enemies/Swarmer_Data.asset`

### A.4 — Verify Layers + Tag
- [ ] `Edit → Project Settings → Tags and Layers`
- [ ] Layer 8 = `Enemy`
- [ ] Layer 9 = `Projectile`
- [ ] Tag `Player` tồn tại (built-in, không cần tạo)

### A.5 — Verify CharacterDataSO wiring
- [ ] Click `Resources/Characters/Kai_Data.asset`
- [ ] Inspector → field `Character Prefab` → có ref `Kai` prefab (không phải None)
- [ ] Click `Resources/Characters/Lyra_Data.asset`
- [ ] Inspector → field `Character Prefab` → có ref `Lyra` prefab

### A.6 — Verify Kai prefab structure
- [ ] Double-click `Assets/Mythfall/Prefabs/Players/Kai.prefab` (mở Prefab View)
- [ ] Hierarchy:
  ```
  Kai (Tag: Player, Layer: Default)
  ├── Visual (capsule, no collider)
  ├── MuzzlePoint (empty Transform, position 0,1.2,0.4)
  └── Hitbox (SphereCollider trigger DISABLED, HitboxRelay)
  ```
- [ ] Inspector của root `Kai`:
  - Components có đủ: CharacterController, CharacterLocomotion, PlayerHealth, TargetSelector, PlayerFacing, MeleeCombat, MeleePlayer, Animator, DynamicAnimationEventHub
  - `MeleeCombat → Hitbox` field = SphereCollider trên child Hitbox
  - `MeleePlayer → Melee Combat` field = MeleeCombat component
  - `PlayerBase → Muzzle Point` = MuzzlePoint child
  - `PlayerBase → Animator` = Animator component
  - `Animator → Controller` = PlayerAnimator
- [ ] Click `Hitbox` child → Inspector:
  - `SphereCollider`: Is Trigger = ✓, **enabled = ✗ (off)**, Radius ≈ 1.2
  - `HitboxRelay → Target` = MeleeCombat trên root Kai

### A.7 — Verify Swarmer prefab structure
- [ ] Double-click `Assets/Mythfall/Resources/Prefabs/Enemies/Swarmer.prefab`
- [ ] Root `Swarmer` (Layer: Enemy):
  - CapsuleCollider (Is Trigger = ✗, dùng để TargetSelector OverlapSphere detect)
  - SwarmerEnemy component, field `Data` = Swarmer_Data SO
  - Animator + DynamicAnimationEventHub
- [ ] Child `Visual`: capsule, không collider, scale 0.8/0.7/0.8

---

## 🅱️ PHASE B — TEST CHECKLIST (Smoke test combat)

### ⚠️ Cách Play hoạt động ở Day 2 (chưa có MenuScene UI)

**KHÔNG cần** "MenuScene → GameplayScene" transition vì Day 3 mới làm UI button.
Day 2 test bằng cách Play **trực tiếp từ GameplayScene** — Bill auto-bounce:

```
1. Mày press Play khi đang mở GameplayScene
2. Bill.Phase1 detect scene index != 0:
   - save EditorPrefs Bill_ReturnScene = "GameplayScene"
   - load BootstrapScene (single mode)
3. BootstrapScene Awake → GameBootstrap.Awake configure BillStartup
   → detect EditorPrefs flag → KHÔNG set nextScene (bỏ qua MenuScene transition)
4. Bill.Phase2 init services → fire GameReadyEvent
5. BillStartup runs steps (Localization + Pools + HealthCheck)
6. Bill.Phase2 editor-return: load GameplayScene back (single mode)
7. Mày thấy GameplayScene với Kai + Swarmer + plane mà mày setup
```

Nếu mày Play từ BootstrapScene trực tiếp → splash → MenuScene trống (không có gì để click). Đó là expected, Day 3 sẽ thêm UI.

### B.1 — Auto-build test scene
- [ ] Menu bar → `Tools → Mythfall → Sprint 2 — Setup GameplayScene for Test`
- [ ] Console expected:
  ```
  [Sprint2Setup] GameplayScene populated: plane + light + Main Camera + Kai + 4 Swarmers. Press Play (from this scene) to test combat loop.
  ```
- [ ] Hierarchy giờ có:
  - `[TestSetup]` (chứa Ground plane + Directional Light)
  - `Main Camera` (đã reposition top-down)
  - `Kai` (instance từ prefab, position 0,0,0)
  - `Swarmer_1` đến `Swarmer_4` (positions ~4m quanh Kai)
- [ ] Verify Kai → Inspector → `CharacterLocomotion → Ground Layer` = `Default` (script auto-set)

### B.2 — (skipped — ground layer set automatically by B.1)

### B.3 — Press Play (chạy từ GameplayScene)
> GameplayScene phải đang là active scene khi mày press Play.

- [ ] Confirm GameplayScene đang mở (Hierarchy hiển thị Kai + Swarmers + Plane)
- [ ] Press Play
- [ ] **First ~1-2s:** màn hình đen → splash logo trắng nhỏ scale lên (placeholder Unity sprite) — đây là BootstrapScene đang init
- [ ] Console expected (theo thứ tự):
  - `[Bill] + Infrastructure (...ms)`
  - `[Bill] + Core Services (...ms)`
  - `[Bill] + State Machine (...ms)`
  - `[Bill] + Network (...ms)`
  - `[Bill] + Dev Tools (...ms)`
  - `[Bill.State] None -> Boot`
  - `[Bill] Ready. 14 services in ~350ms.`
  - `[BillStartup] > Initialize Localization...`
  - `[Localization] Loaded language 'vi' — N keys`
  - `[Localization] Loaded fallback 'en' — N keys`
  - `[GameBootstrap] LocalizationService ready, language: vi`
  - `[BillStartup] Initialize Localization done.`
  - `[BillStartup] > Register Pools...`
  - `[GameBootstrap] 2/2 pools registered.`
  - `[BillStartup] Register Pools done.`
  - `[BillStartup] > Health Check...`
  - `[Bill] Health Check: ... All services healthy.` (15 services giờ có thêm LocalizationService)
  - `[BillStartup] Health Check done.`
- [ ] Sau ~3s: BillStartup fade out, Bill editor-return load GameplayScene → Kai + Swarmers visible

### B.4 — Observe combat
Sau khi splash logo fade, scene render Kai + Swarmers + plane.

**Expected in ~3-5 seconds:**
- [ ] Swarmers từ từ chase Kai (di chuyển về phía Kai)
- [ ] Kai stand still nhưng auto-rotate face nearest Swarmer (PlayerFacing working)
- [ ] Khi Swarmer vào range 1.8m của Kai → Kai trigger `Attack_1` (Animator log warning về missing clip OK)
- [ ] **Timer fallback** kicks in: hitbox enable lúc 0.15s, disable lúc 0.4s
- [ ] Nếu Swarmer trong arc 120° + range → Swarmer take damage
- [ ] Sau ~6-8 hits (Kai 15 ATK, Swarmer 30 HP) → Swarmer die
- [ ] Console log `EnemyKilledEvent` fired (nếu mày add subscriber để check, hoặc check Bill.Trace)

**Visual check:**
- [ ] Kai capsule màu đỏ, Swarmer màu xám
- [ ] Swarmer có thể overlap player (CharacterController không stop them perfectly — chấp nhận)
- [ ] Kai nhận damage từ Swarmer attack (Swarmer trigger Attack → fallback OnAttackHit 0.3s sau → PlayerHealth.TakeDamage)
- [ ] Sau khi Kai nhận đủ damage (120 HP / 5 dmg per hit / 1.2s cooldown ≈ 30s) → Kai die → console log `[Bill.State] ... -> Defeat`

### B.5 — Test ranged player (optional)
- [ ] Stop Play, replace Kai bằng Lyra prefab
- [ ] Press Play
- [ ] Lyra range 10m → Lyra attack từ xa
- [ ] **Timer fallback** spawn Arrow projectile từ MuzzlePoint sau 0.5s
- [ ] Arrow bay tới Swarmer + damage on hit + return pool
- [ ] Console log `Spawn 'Projectile_Arrow'` (Bill.Pool log)

---

## 🅲️ PHASE C — REPORT BACK TO CLAUDE

### Khi A + B pass:
Paste prompt sau vào Claude session mới:

```
Mày là Mythfall agent. Đọc 3 file sau theo thứ tự:
1. CLAUDE.md (root) — architecture rules + workflow
2. PROGRESS.md — current sprint status
3. HANDOFF.md — last session snapshot

Status: Sprint 1 Day 2 in-editor verification PASSED. Combat loop hoạt động:
- Sprint2Setup tạo 4 prefab + 2 AnimatorController + 4 material + Swarmer_Data OK
- Compile 0 error
- GameplayScene smoke test: Swarmer chase Kai, Kai auto-attack với timer fallback,
  Swarmer die sau N hits, projectile Lyra hoạt động (nếu test)

Bắt đầu Sprint 1 Day 3:
- 4 UGUI panel scripts (logic only): MainMenuPanel, CharacterSelectPanel, HudPanel, GameOverPanel
- SettingsOverlay với language dropdown VN/EN
- InventoryService + PlayerData (Bill.Save persistence)
- VirtualJoystick UGUI → MobileInputManager.MoveVector wiring
- GameBootstrap: AddStep RegisterStates + RegisterUI panels
- CharacterSelectedEvent flow

Plan tasks → execute step-by-step. Block thì hỏi tao.
```

### Khi A hoặc B fail:
Paste prompt sau (kèm log/screenshot):

```
Mày là Mythfall agent. Đọc CLAUDE.md + HANDOFF.md.

Status: Sprint 1 Day 2 verification FAILED ở Phase [A.X / B.X].

[Paste console log đỏ ở đây]

[Mô tả what mày thấy / không thấy trong scene]

Diagnose root cause + fix. Nếu cần tao screenshot / Inspector data thì hỏi cụ thể.
```

---

## 🆘 TROUBLESHOOTING — Common failure modes

| Symptom | Likely cause | Quick fix |
|---|---|---|
| `DestroyImmediate not in context` compile error | Bug từ session trước | Đã fix trong commit; nếu vẫn lỗi, paste log |
| Sprint2Setup báo `Layer slot already named X` | Có gì đó dùng layer 8 hoặc 9 trước rồi | Mày check Tags and Layers, dọn manually nếu trùng |
| Kai prefab không có `MeleeCombat` component | Setup script lỗi giữa chừng | Re-run Sprint2Setup, idempotent |
| Swarmer không chase Kai | Kai chưa có tag `Player` HOẶC plane Layer khác | Set Kai tag = Player, plane Layer = Default |
| Kai rơi xuyên plane | Ground Layer trên CharacterLocomotion chưa set | Set field `Ground Layer` = Default |
| Hitbox không damage Swarmer | Layer collision matrix Default vs Enemy off | `Edit → Project Settings → Physics → Layer Collision Matrix` → đảm bảo Default ↔ Enemy = ✓ |
| Animator log warning `Parameter Speed does not exist` | AnimatorController chưa generate | Re-run Sprint2Setup |
| Timer fallback không trigger | Bill.Timer chưa init khi MeleeCombat.Execute() chạy | Đảm bảo Play từ BootstrapScene HOẶC GameplayScene (Bill auto-bounce) |
| `[GameBootstrap] 0/2 pools registered` | Prefab path sai trong Resources | Verify `Resources/Prefabs/Enemies/Swarmer.prefab` + `Resources/Prefabs/Projectiles/Arrow.prefab` exist |

---

## ⏭️ AFTER PASSING — Sprint 1 Day 3 preview

Day 3 Claude sẽ làm:
1. **InventoryService** + `PlayerData` (saves selected char qua Bill.Save)
2. **MainMenuPanel** script — Play button → CharacterSelectState; locked Gacha/Inventory/Shop/BP buttons
3. **CharacterSelectPanel** script — 2 char cards với star ★★★★ + name + title (LocalizedText), Confirm button
4. **HudPanel** script — HP bar + Level + skill cooldown placeholders
5. **GameOverPanel** script — Defeat title + Retry/Hub buttons
6. **SettingsOverlay** script — language dropdown VN/EN
7. **VirtualJoystick** UGUI component — drives MobileInputManager
8. **GameBootstrap** — register states + UI panels
9. **CharacterSelectedEvent** dispatch + handler

Mày sẽ build hierarchy + drag-drop refs trong Unity (script chỉ logic, đã agree từ session trước).

---

*Khi nào A+B pass và mày paste Day 3 prompt — Claude tiếp ngay.*
