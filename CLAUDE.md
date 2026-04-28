# 🤖 CLAUDE.md — Mythfall Survivor Agent Guide

> **Bạn (Claude Code) là agent đang work trên project Mythfall: Survivor.**
> File này nằm ở **root project folder** và sẽ được đọc đầu mỗi conversation.
> Đây là source of truth cho mọi quyết định architecture và workflow.

---

## 🎯 PROJECT MISSION

Build **vertical slice playable** của Mythfall: Survivor trong **2 tuần**.

**Định nghĩa "vertical slice playable":**
- Game chạy được end-to-end: Menu → Character Select → Gameplay → Win/Lose → Menu
- 2 character với playstyle khác nhau rõ rệt (Melee + Ranged)
- 4+ skill với feel "juicy", satisfying
- 3+ enemy types với behavior khác nhau
- 1 boss fight có phase
- In-run progression (XP + level up + 8+ upgrade cards)
- Polish layer: hitstop, screen shake, VFX, audio feedback
- Build APK chạy được trên Android device, 30+ FPS

**KHÔNG có trong vertical slice:**
- Gacha system, monetization, IAP, ads
- Multi-chapter, equipment, set bonus
- Cloud save, backend, leaderboard
- Multi-language localization

---

## 🛠️ TECH STACK

- **Unity 6.3.10f1** với URP (Universal Render Pipeline)
- **C#**, .NET Standard 2.1
- Input System: New Input System (mobile virtual joystick)
- Target: Android 9.0+ (API 28+)
- Build: IL2CPP, ARM64

**Pre-built Frameworks (đã có trong project — KHÔNG modify):**

| Framework | Path | Purpose |
|---|---|---|
| BillGameCore | `Assets/BillGameCore/` | Service Locator, State Machine, Pool, Audio, UI, Save, Events, Timer, Scene |
| ModularTopDown.Locomotion | `Assets/ModularTopDown/` | CharacterController-based movement với `ExternalRotationControl` flag |
| VAT | `Assets/VAT/` | GPU instancing animation cho enemies |

**Game code:** tất cả nằm trong `Assets/Mythfall/`.

---

## 📋 WORKFLOW

### Đầu mỗi session, BẠN PHẢI:

1. **Đọc CLAUDE.md** (file này) để load context
2. **Đọc PROGRESS.md** để biết sprint hiện tại và status
3. **Đọc Sprint Doc tương ứng** trong folder `Sprints/`
4. **Confirm context** với user: "Đang ở Sprint X, task là Y, đúng không?"

### Trong khi làm việc:

5. **Plan trước khi code** — list files sẽ tạo/sửa, architecture decisions
6. **Implement step-by-step** — từng commit-able unit
7. **Tuân thủ Architecture Rules** ở section bên dưới
8. **Update PROGRESS.md** sau mỗi task hoàn thành

### Cuối sprint:

9. **Verify Definition of Done** trong sprint doc
10. **Run test checklist** trong sprint doc
11. **Update PROGRESS.md status** → 🟢 Done
12. **Báo cáo deliverables** cho user

---

## 🏗️ ARCHITECTURE RULES — KHÔNG VI PHẠM

### Rule 1: Data-Driven (CRITICAL)
**TẤT CẢ stats, balance values, tham số game phải nằm trong ScriptableObject.**

```csharp
// ❌ SAI
public class Kai : PlayerBase {
    void Awake() { maxHP = 120f; } // hardcoded
}

// ✅ ĐÚNG
public class PlayerBase : MonoBehaviour {
    [SerializeField] CharacterDataSO data;
    void Start() { stats = new RuntimeCharacterStats(data.baseStats); }
}
```

### Rule 2: Communication qua Bill.Events
Components giao tiếp qua event bus, không tight-coupling references.

```csharp
// ❌ SAI
hudPanel.UpdateHP(hp);
audioMgr.Play("hit");

// ✅ ĐÚNG
Bill.Events.Fire(new EnemyHitEvent { damage = dmg });
// HUD, Audio, VFX subscribe và tự handle
```

**Exception:** Components cùng GameObject (PlayerBase ↔ PlayerHealth) có thể reference trực tiếp.

### Rule 3: Object Pool — KHÔNG Instantiate/Destroy
```csharp
// ❌ SAI
Instantiate(enemyPrefab); Destroy(gameObject);

// ✅ ĐÚNG
Bill.Pool.Spawn("Enemy_Swarmer", pos, rot);
Bill.Pool.Return(gameObject, delay: 1f);
```

Đăng ký pool trong `GameBootstrap.RegisterPools()`.

### Rule 4: Locomotion + Facing Tách Biệt
**Quyết định architecture quan trọng nhất.** Top-down game cần Movement và Facing độc lập.

```csharp
// PHẢI có dòng này trong PlayerBase.Awake()
locomotion.ExternalRotationControl = true;
// → CharacterLocomotion KHÔNG tự rotate
// → PlayerFacing component handle rotation theo target
```

### Rule 5: Player vs Enemy Animation
- **Player** → Unity Animator (cần precise animation events cho hitbox timing)
- **Enemy thường** → `VAT_Animator_Instanced` (1 draw call cho hàng trăm enemy)
- **Boss** → `VAT_Animator` (crossfade giữa phase animations)

KHÔNG dùng Unity Animator cho enemy thường — kill performance.

### Rule 6: Skill = SO + Execution (Strategy Pattern)

```csharp
public abstract class SkillDataSO : ScriptableObject {
    public abstract ISkillExecution CreateExecution(SkillContext context);
}

public interface ISkillExecution {
    bool CanExecute();
    void Execute();
    void Tick(float dt);
    bool IsFinished { get; }
}
```

Mỗi skill mới = 2 file (SO + Execution). KHÔNG nhồi nhét logic skill vào PlayerCombat.

**Đọc `Docs/SKILL_DESIGN_GUIDE.md` cho design feel patterns.**

### Rule 7: Tuyệt Đối KHÔNG Modify Framework
Không sửa file trong `BillGameCore/`, `ModularTopDown/`, `VAT/`. Cần extend → tạo class mới wrap/inherit.

### Rule 8: Localization-First (CRITICAL)
**TUYỆT ĐỐI KHÔNG hardcode user-facing string trong code.** Mọi UI text phải qua `LocalizationService`.

```csharp
// ❌ SAI
button.text = "Play";
hpLabel.text = $"HP: {hp}/{maxHP}";

// ✅ ĐÚNG
button.text = LocalizationService.Get("ui.menu.play");
hpLabel.text = LocalizationService.GetFormatted("ui.hud.hp", hp, maxHP);

// ✅ TỐT NHẤT — dùng LocalizedText component, không cần code
// (component tự bind key → text + auto-update khi switch language)
```

**ScriptableObject (CharacterDataSO, SkillDataSO) chứa LOCALIZATION KEYS, không phải raw text.**

```csharp
// ❌ SAI
public string displayName = "Kai Stormbringer";

// ✅ ĐÚNG
public string nameKey = "character.kai.name";  // resolve at runtime
```

**Đọc `Docs/LOCALIZATION_GUIDE.md` cho implementation details.**

---

## 🚫 ANTI-PATTERNS

| ❌ KHÔNG làm | ✅ Thay bằng |
|---|---|
| `GameObject.Find()` runtime | DI hoặc `Bill.Events` |
| `static GameManager.Instance` | `ServiceLocator.Register<T>()` |
| `SendMessage("OnDamage")` | Interface hoặc `Bill.Events` |
| Update() poll mỗi frame | Subscribe events |
| Hardcoded scene names | Constants class + `Bill.Scene.Load()` |
| Instantiate/Destroy | `Bill.Pool.Spawn/Return` |

---

## 📁 FOLDER STRUCTURE (BẮT BUỘC)

```
Assets/Mythfall/
├── Scripts/
│   ├── Core/             (GameBootstrap, States, Events)
│   ├── Characters/       (CharacterDataSO, RuntimeStats, Registry)
│   ├── Player/           (PlayerBase + subclasses, Health, Facing, etc.)
│   ├── Skills/           (SkillDataSO + executions)
│   ├── Enemy/            (EnemyBase + subclasses)
│   ├── Inventory/        (PlayerData, InventoryService)
│   ├── Gameplay/         (Spawner, Projectile, XPGem, Wave)
│   ├── UI/               (Panels)
│   ├── Input/            (MobileInput, VirtualJoystick)
│   └── Polish/           (HitStop, ScreenShake, DamageNumber)
├── Prefabs/
│   ├── Players/, Enemies/, Projectiles/, VFX/, Items/
├── Resources/
│   ├── Characters/       (CharacterDataSO assets)
│   ├── Skills/           (SkillDataSO assets)
│   ├── Enemies/          (EnemyDataSO + VAT data assets)
│   ├── Cards/            (UpgradeCardSO assets)
│   └── UI/               (UXML files)
├── Scenes/               (Bootstrap, Menu, Gameplay)
├── Animations/           (Animator Controllers, .anim)
└── Materials/            (Toon shader instances)
```

**Naming conventions:**
- ScriptableObject class: `XxxSO`
- Asset file: `Xxx_Data.asset`, `Xxx_Skill.asset`
- Abstract class: `XxxBase`
- Service: `XxxService`
- Event struct: `XxxEvent`

---

## 🔌 BILLGAMECORE API CHEAT SHEET

```csharp
// State machine
Bill.State.GoTo<InRunState>();

// Event bus (define struct với IEvent interface)
Bill.Events.Fire(new EnemyHitEvent { damage = 50 });
Bill.Events.Subscribe<EnemyHitEvent>(OnHit);
Bill.Events.Unsubscribe<EnemyHitEvent>(OnHit);

// Pool
var enemy = Bill.Pool.Spawn("Enemy_Swarmer", pos, rot);
Bill.Pool.Return(go, delay: 1f);

// UI
Bill.UI.Open<HudPanel>();
Bill.UI.Close<HudPanel>();

// Save
var data = Bill.Save.Get<PlayerData>("player") ?? new PlayerData();
Bill.Save.Set("player", data);
Bill.Save.Flush();

// Audio
Bill.Audio.Play("sfx_hit");
Bill.Audio.PlayMusic("bgm_combat", fadeDuration: 1.5f);

// Timer
Bill.Timer.Delay(2f, () => SpawnBoss());
var handle = Bill.Timer.Repeat(1f, OnTick);
handle.Cancel();

// Scene
Bill.Scene.Load("GameplayScene", TransitionType.Fade, 0.5f);

// Diagnostics
Bill.Trace.HealthCheck(); // log service health
Bill.Trace.Print(); // dependency report

// Wait for ready
void Start() {
    if (Bill.IsReady) Init();
    else Bill.Events.SubscribeOnce<GameReadyEvent>(_ => Init());
}
```

**Đọc `Docs/BILLGAMECORE_API.md` cho chi tiết.**

---

## 🚶 LOCOMOTION API

```csharp
[RequireComponent(typeof(CharacterController))]
public class PlayerBase : MonoBehaviour {
    CharacterLocomotion locomotion;

    void Awake() {
        locomotion = GetComponent<CharacterLocomotion>();
        locomotion.ExternalRotationControl = true; // BẮT BUỘC
        locomotion.ConfigureJumps(allowDoubleJump: false);
    }

    void Update() {
        var input = MobileInput.MoveVector;
        if (locomotion.IsGrounded())
            locomotion.HandleGroundedMovement(input, isRunning: true);
        else
            locomotion.HandleAirborneMovement(input);
    }
}

// Available properties:
// locomotion.HorizontalVelocity (Vector3)
// locomotion.PlayerVelocity (Vector3)
// locomotion.RunSpeed (float)
```

---

## 🎬 VAT API (Enemy Animation)

```csharp
// Setup workflow:
// 1. Create character với Unity Animator + clips
// 2. Tools → BillTheDev → VAT → Baker Window
// 3. Bake → tạo VAT_AnimationData.asset + baked mesh + position texture
// 4. Create enemy prefab với MeshFilter + MeshRenderer + VAT_Animator_Instanced

// Runtime control:
[SerializeField] VAT_Animator_Instanced vatAnim;
vatAnim.Play("Idle");      // for normal enemies (instant switch)
vatAnim.Play("Attack");

// For boss với crossfade:
[SerializeField] VAT_Animator vatAnim;  // non-instanced version
vatAnim.CrossFadeToClip("Slam", 0.2f);  // smooth transition
```

---

## 🎮 SPRINT EXECUTION PROTOCOL

Khi user yêu cầu "làm Sprint X":

### Step 1: Load Context (luôn luôn)
```
1. Read PROGRESS.md → check current sprint status
2. Read Sprints/SPRINT_X_*.md → understand goals + tasks
3. Read Docs/ relevant guides nếu task chạm tới (skill = SKILL_DESIGN_GUIDE, polish = COMBAT_FEEL_GUIDE)
4. Confirm với user: "Sprint X về [topic], task hiện tại là [Y]. Bắt đầu?"
```

### Step 2: Plan
```
1. List files sẽ tạo (path + purpose)
2. List files sẽ sửa (path + change)
3. Identify dependencies (cần component nào, SO nào)
4. Identify risks ("VAT bake cần manual step")
```

### Step 3: Execute
```
1. Tạo/sửa files theo plan
2. Mỗi file phải đầy đủ — không stub
3. Comment chỗ nào cần manual step trong Unity Editor
4. Test mental simulation — code có chạy không?
```

### Step 4: Verify
```
1. Run mental compile check (no syntax error)
2. Architecture rule check (no hardcode, no Instantiate, etc.)
3. Update PROGRESS.md task status
4. Provide test instructions cho user
```

---

## 🎯 DECISION-MAKING AUTHORITY

### Bạn (Claude Code) ĐƯỢC quyền tự quyết:
- ✅ Skill values (damage multiplier, cooldown, range) — feel-driven
- ✅ Animation timing (frame events, transitions)
- ✅ VFX placeholder design (particle settings)
- ✅ Audio cue placement
- ✅ Code architecture details (miễn tuân thủ rules)
- ✅ Variable names, namespace organization

### Bạn PHẢI hỏi user khi:
- ❓ Modify framework code (BillGameCore/Locomotion/VAT)
- ❓ Add dependency mới (NuGet, third-party SDK)
- ❓ Change scope của sprint (add/remove features)
- ❓ Conflict giữa documents (CLAUDE.md vs Sprint doc)
- ❓ Performance trade-off lớn (giảm visual fidelity)

---

## 💡 SKILL DESIGN PHILOSOPHY (cho user requirements)

User explicitly delegated **skill feel + design** cho bạn. Đây là priorities:

1. **Each skill phải có "moment"** — một beat satisfying khi trigger
2. **Visible feedback ngay lập tức** — VFX + audio + screen effect
3. **Clear power expression** — player thấy mình mạnh hơn
4. **Synergy potential** — skill có thể combo với upgrade cards
5. **Distinct feel** giữa melee (impact, hitstop) vs ranged (charge, projectile)

**Mỗi khi tạo skill mới, đọc `Docs/SKILL_DESIGN_GUIDE.md` để follow patterns.**

---

## 📊 KEY METRICS (mỗi sprint phải đạt)

| Metric | Target |
|---|---|
| Compile errors | 0 |
| Warnings | < 5 (có comment lý do nếu có) |
| FPS trên Android mid-range | 30+ stable |
| Memory leak (Bill.Pool stable) | 0 sau 3 runs |
| Crash trong 5 phút playtest | 0 |
| Architecture violations | 0 |

---

## 📚 REFERENCE FILES

Khi nào đọc gì:

| File | Khi nào đọc |
|---|---|
| `CLAUDE.md` | Đầu mỗi session (file này) |
| `PROGRESS.md` | Đầu session để biết sprint nào |
| `Sprints/SPRINT_X_*.md` | Khi làm sprint X |
| `Docs/GAME_DESIGN.md` | **Khi cần lore/character/world context** ⭐ |
| `Docs/ARCHITECTURE.md` | Khi unsure về design pattern |
| `Docs/BILLGAMECORE_API.md` | Khi cần API reference chi tiết |
| `Docs/SKILL_DESIGN_GUIDE.md` | **Mỗi khi tạo skill mới** |
| `Docs/COMBAT_FEEL_GUIDE.md` | Khi polish combat (Sprint 2, 4) |
| `Docs/LOCALIZATION_GUIDE.md` | **Khi implement UI hoặc thêm string mới** |
| `Docs/UI_VISUAL_GUIDE.md` | Khi build UI panels (Sprint 1, 4) |
| `Templates/*.cs` | Khi cần code skeleton |

---

## ⚠️ FINAL REMINDERS

1. **Vertical slice trong 2 tuần là tight** — KHÔNG over-engineer
2. **Combat feel quan trọng hơn feature count** — 4 skill juicy > 10 skill flat
3. **User prepare animation + character visual** — bạn lo logic, skill, polish
4. **Mỗi sprint = 1 build playable** — không ship code half-done
5. **Hỏi khi unsure** — đừng đoán mò architecture decision
6. **Luôn reference GAME_DESIGN.md cho character lore/skill fantasy** — không tự bịa
7. **Localization từ ngày đầu** — KHÔNG hardcode string, dùng LocalizedText + keys
8. **Test EN+VN switch trên mỗi UI panel** trước khi commit

---

*Khi paste vào Claude Code lần đầu: "Đọc CLAUDE.md ở root folder và confirm bạn đã hiểu workflow + architecture rules. Sau đó đọc PROGRESS.md để biết sprint hiện tại."*

**Version:** 1.0 | **Last updated:** 2026-04-28
