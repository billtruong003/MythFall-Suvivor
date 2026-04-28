# 🏛️ ARCHITECTURE QUICK REFERENCE

> **Mục đích:** Quick reference cho architecture decisions. Đọc khi unsure về design pattern.

---

## 🎯 CORE PATTERNS

### 1. Service Locator (BillGameCore native)

Đăng ký services 1 lần, access ở mọi nơi qua `Bill.X`.

```csharp
// Register (in GameBootstrap)
ServiceLocator.Register<InventoryService>(new InventoryService());

// Access anywhere
var inv = ServiceLocator.Get<InventoryService>();
// Or via Bill.X for built-in services
Bill.Pool.Spawn(...)
```

### 2. State Machine (BillGameCore native)

Game flow controlled qua states:

```csharp
// Define
public class InRunState : GameState {
    public override void Enter() { /* setup */ }
    public override void Tick(float dt) { /* update */ }
    public override void Exit() { /* cleanup */ }
}

// Register + transition
Bill.State.AddState<InRunState>();
Bill.State.GoTo<InRunState>();
```

### 3. Event Bus (BillGameCore native)

Decoupled communication:

```csharp
// Define event
public struct EnemyHitEvent : IEvent {
    public PlayerBase attacker;
    public float damage;
}

// Fire
Bill.Events.Fire(new EnemyHitEvent { damage = 50 });

// Subscribe
Bill.Events.Subscribe<EnemyHitEvent>(OnHit);
// Always unsubscribe!
Bill.Events.Unsubscribe<EnemyHitEvent>(OnHit);
```

### 4. Object Pool (BillGameCore native)

```csharp
// Register (Bootstrap)
Bill.Pool.Register("Enemy_Swarmer", prefab, warmCount: 30);

// Spawn
var enemy = Bill.Pool.Spawn("Enemy_Swarmer", pos, rot);

// Return
Bill.Pool.Return(gameObject, delay: 1f);
```

### 5. ScriptableObject Data (Mythfall pattern)

Data-driven design — tất cả balance values trong SO:

```csharp
[CreateAssetMenu(menuName = "Mythfall/Character Data")]
public class CharacterDataSO : ScriptableObject {
    public CharacterBaseStats baseStats;
    public SkillDataSO autoAttackSkill;
    // ...
}
```

### 6. Strategy Pattern (Skills)

```csharp
public abstract class SkillDataSO : ScriptableObject {
    public abstract ISkillExecution CreateExecution(SkillContext ctx);
}

public interface ISkillExecution {
    bool CanExecute();
    void Execute();
    void Tick(float dt);
    bool IsFinished { get; }
}
```

Mỗi skill = SO + Execution class. Logic strategy-based.

---

## 🎮 PLAYER ARCHITECTURE

### Component Composition

```
Player GameObject (e.g. Kai prefab)
├── CharacterController (Unity)
├── CharacterLocomotion (Modular framework)
├── Animator (Unity)
├── PlayerBase (abstract → MeleePlayer / RangedPlayer)
├── PlayerHealth
├── PlayerFacing
├── TargetSelector
├── PlayerSkillManager
├── PlayerCombatBase (abstract → MeleeCombat / RangedCombat)
├── KnockbackReceiver
└── HitFlashController
```

### Component Responsibilities

| Component | Read | Write | Notes |
|---|---|---|---|
| `CharacterLocomotion` | Input | transform.position | Movement only |
| `PlayerFacing` | TargetSelector.target | transform.rotation | Rotation only |
| `TargetSelector` | Physics layer | CurrentTarget property | Update 0.2s |
| `PlayerCombatBase` | TargetSelector, Stats | Animation triggers | Decide attack |
| `PlayerSkillManager` | CharacterData | Skill executions | Run skills |
| `PlayerHealth` | (TakeDamage calls) | HP, events | HP + iFrame |
| `PlayerBase` | Coordinates all above | - | Orchestrator |

### Critical Setup

```csharp
// In PlayerBase.Awake() — MUST DO
locomotion.ExternalRotationControl = true;
```

Without this → CharacterLocomotion tự rotate, conflict với PlayerFacing → bug rotation.

---

## 👹 ENEMY ARCHITECTURE

### Hierarchy

```
EnemyBase (abstract)
├── SwarmerEnemy (fast melee, simple AI)
├── BruteEnemy (slow tank, telegraph attack)
├── ShooterEnemy (kite, ranged)
└── BossEnemy (multi-phase)

EliteModifier (component) — apply to any EnemyBase
```

### Component Composition

```
Enemy GameObject (e.g. Swarmer prefab)
├── EnemyBase subclass
├── VAT_Animator_Instanced (or Animator for boss)
├── MeshFilter + MeshRenderer
├── SphereCollider (trigger for hit detection)
├── HitFlashController
└── KnockbackReceiver (optional, for big enemies)
```

### Enemy AI Pattern

```csharp
public enum EnemyAIState { Idle, Chase, Attack, Stunned, Dying }

public abstract class EnemyBase : MonoBehaviour {
    protected EnemyAIState currentState;

    public void TransitionTo(EnemyAIState newState) {
        OnStateExit(currentState);
        currentState = newState;
        OnStateEnter(newState);
    }

    void Update() {
        if (currentState == EnemyAIState.Dying) return;
        TickState(Time.deltaTime);
    }

    protected abstract void TickState(float dt);
}
```

---

## 🎨 UI ARCHITECTURE

### Panel Pattern (BillGameCore)

```csharp
public class HudPanel : BasePanel {
    public override void OnOpen() {
        // Subscribe events
        Bill.Events.Subscribe<PlayerDamagedEvent>(UpdateHP);
    }

    public override void OnClose() {
        // ALWAYS unsubscribe
        Bill.Events.Unsubscribe<PlayerDamagedEvent>(UpdateHP);
    }

    void UpdateHP(PlayerDamagedEvent e) {
        // Update UI element
    }
}

// Usage
Bill.UI.Open<HudPanel>();
Bill.UI.Close<HudPanel>();
```

### UI Communication

UI **NEVER** call game logic directly. Chỉ call:
- Service methods (e.g., `inventory.SetCurrentCharacter()`)
- State transitions (`Bill.State.GoTo<X>()`)
- Event firing (`Bill.Events.Fire(...)`)

UI **subscribe** events để update:
- `PlayerDamagedEvent` → update HP bar
- `XPChangedEvent` → update XP bar
- `PlayerLeveledUpEvent` → show level number

---

## 💾 DATA FLOW

### Save/Load

```
[App start]
    ↓
InventoryService.Initialize()
    ↓
Bill.Save.Get<PlayerData>("player")
    ↓ (if null) → new PlayerData() with defaults
[Game running]
    ↓
PlayerData modified (e.g., character selected)
    ↓
Bill.Save.Set("player", data)
    ↓
Bill.Save.Flush() (force write to disk)
[App close → reopen]
    ↓
Same flow, data persisted
```

### Combat Damage Flow

```
[MeleeCombat OnTriggerEnter]
    ↓ Calculate damage from Stats
    ↓ enemy.TakeDamage(damage, owner)
[EnemyBase.TakeDamage]
    ↓ currentHP -= amount
    ↓ HitFlash component
    ↓ Bill.Events.Fire(EnemyHitEvent)
[Multiple subscribers handle event]
    ├── DamageNumberSpawner → spawn number
    ├── AudioListener → play hit SFX
    ├── CameraShake → small shake nếu crit
    └── HitstopController → freeze 50ms nếu crit
[If HP <= 0]
    ↓ Die() → spawn XP gem
    ↓ Bill.Events.Fire(EnemyKilledEvent)
    ↓ Pool.Return after delay
```

---

## 🚨 ARCHITECTURE VIOLATIONS — DETECT & FIX

### Violation 1: Hardcoded Stats

**Symptom:**
```csharp
public class SwarmerEnemy {
    void Start() { maxHP = 30f; } // ❌
}
```

**Fix:** Move to EnemyDataSO:
```csharp
public class SwarmerEnemy : EnemyBase {
    void Start() {
        maxHP = data.maxHP; // ✅
    }
}
```

### Violation 2: Direct Reference Cross-Component

**Symptom:**
```csharp
public class MeleeCombat {
    [SerializeField] HudPanel hudPanel; // ❌ tight coupling
    void OnHit() { hudPanel.UpdateDamage(50); }
}
```

**Fix:** Use events:
```csharp
public class MeleeCombat {
    void OnHit() { Bill.Events.Fire(new EnemyHitEvent { damage = 50 }); }
}
public class HudPanel {
    void OnEnable() { Bill.Events.Subscribe<EnemyHitEvent>(UpdateDamage); }
}
```

### Violation 3: Instantiate/Destroy Direct

**Symptom:**
```csharp
Instantiate(arrowPrefab, pos, rot); // ❌
Destroy(gameObject, 1f); // ❌
```

**Fix:** Use Pool:
```csharp
Bill.Pool.Spawn("Projectile_Arrow", pos, rot); // ✅
Bill.Pool.Return(gameObject, delay: 1f); // ✅
```

### Violation 4: GameObject.Find In Update

**Symptom:**
```csharp
void Update() {
    var player = GameObject.Find("Player"); // ❌ expensive every frame
}
```

**Fix:** Cache reference or use events:
```csharp
Transform player;
void Start() {
    var p = GameObject.FindGameObjectWithTag("Player");
    player = p?.transform;
}
// Or subscribe to PlayerSpawnedEvent
```

### Violation 5: Static Singleton (Non-Bill)

**Symptom:**
```csharp
public class GameManager {
    public static GameManager Instance; // ❌ custom singleton
}
```

**Fix:** Register as service:
```csharp
public class GameManager : IService {
    public void Initialize() { /* setup */ }
}
// In bootstrap:
ServiceLocator.Register<GameManager>(new GameManager());
// Access:
ServiceLocator.Get<GameManager>().DoSomething();
```

### Violation 6: Polling Instead of Events

**Symptom:**
```csharp
void Update() {
    if (player.HP < 50) ApplyBuff(); // ❌ check every frame
}
```

**Fix:** Subscribe damaged event:
```csharp
void Awake() {
    Bill.Events.Subscribe<PlayerDamagedEvent>(CheckThreshold);
}

void CheckThreshold(PlayerDamagedEvent e) {
    if (player.HP < 50 && !buffApplied) ApplyBuff();
}
```

### Violation 7: Modify Framework

**Symptom:** Edit file in `Assets/BillGameCore/`, `Assets/ModularTopDown/`, or `Assets/VAT/`.

**Fix:** Tạo extension class:
```csharp
// Don't modify CharacterLocomotion.cs
// Instead, create extension method or wrapper
public static class CharacterLocomotionExtensions {
    public static void SetSpeedMultiplier(this CharacterLocomotion loc, float mult) {
        // Custom logic
    }
}
```

---

## 📐 NAMESPACE CONVENTIONS

```
Mythfall.Core           — Bootstrap, States, Events
Mythfall.Characters     — Character data, runtime stats
Mythfall.Player         — Player components
Mythfall.Skills         — Skill SOs and executions
Mythfall.Enemy          — Enemy classes
Mythfall.Inventory      — Player data, services
Mythfall.Gameplay       — Game systems (spawner, projectile, etc.)
Mythfall.UI             — UI panels
Mythfall.Input          — Mobile input
Mythfall.Polish         — VFX, audio, camera
Mythfall.Events         — Event structs
```

---

## 🎯 DECISION TREE: New Feature

```
User wants: "Add new feature X"
    ↓
Q1: Is it a stat/balance value?
    YES → ScriptableObject, no code change
    NO ↓
Q2: Is it new behavior on existing object?
    YES → Add component, subscribe events
    NO ↓
Q3: Is it new game system?
    YES → Create service, register in Bootstrap
    NO ↓
Q4: Is it just UI?
    YES → Create new BasePanel, subscribe events
    NO ↓
Q5: Is it framework extension?
    YES → ASK user before modifying framework
```

---

## 🧪 ARCHITECTURE QUALITY CHECKS

Before completing sprint, run mental check:

- [ ] All stats trong ScriptableObject (no hardcode)
- [ ] All cross-component communication qua events
- [ ] All runtime spawned objects qua Bill.Pool
- [ ] All subscriptions có matching unsubscriptions
- [ ] No `GameObject.Find` trong Update loops
- [ ] No `Instantiate`/`Destroy` direct calls (use pool)
- [ ] No modifications to framework folders
- [ ] PlayerFacing handles rotation, not Locomotion
- [ ] Player uses Animator, enemies use VAT
- [ ] Skills implement ISkillExecution pattern

---

*End of Architecture Reference. Use as quick lookup.*
