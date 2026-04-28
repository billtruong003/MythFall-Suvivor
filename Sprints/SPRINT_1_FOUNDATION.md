# 🎮 SPRINT 1 — Foundation: Player + Enemy + Scene Loop

> **Duration:** 3 ngày | **Output:** Full game loop playable. User chọn character → đánh wave Swarmer → chết hoặc thắng → quay menu.

---

## 🎯 Sprint Goal

End of sprint, user có thể:
1. Boot app
2. Click Play trên main menu
3. Chọn 1 trong 2 character (Kai melee hoặc Lyra ranged) — **với star rating 4★ hiển thị trên card**
4. Vào gameplay scene, di chuyển bằng joystick
5. Tự động attack enemy gần nhất
6. Kill được Swarmer (basic enemy)
7. Bị Swarmer kill → game over screen
8. Click "Back to Menu" → quay về main menu
9. Choice character được lưu (mở lại app, character vẫn là Kai/Lyra)
10. **Test switch language EN ↔ VN trong Settings → mọi UI update đúng**

## 📚 PHẢI ĐỌC TRƯỚC KHI BẮT ĐẦU

- ⭐ **`Docs/GAME_DESIGN.md`** sections 3.2 (Kai + Lyra full lore + skills) — Claude Code phải bám lore này khi tạo CharacterDataSO
- ⭐ **`Docs/LOCALIZATION_GUIDE.md`** — TẤT CẢ UI text phải qua keys, KHÔNG hardcode
- **`Docs/UI_VISUAL_GUIDE.md`** sections 6 (Screen-by-screen) — implementation patterns

## ✅ Prerequisites
- [ ] Sprint 0 done — project boots, scene flow stub works, **LocalizationService registered**
- [ ] User prepared: Kai prefab + Lyra prefab (humanoid mesh + Animator Controller với states Idle, Run, Attack_1, Death)
- [ ] User prepared: Swarmer prefab với placeholder mesh (capsule OK) + Animator hoặc VAT

---

## 📋 TASK BREAKDOWN

### Day 1 — Data Layer + Player Components

#### Task 1.1: CharacterDataSO + Stats System
**File:** `Scripts/Characters/CharacterDataSO.cs`, `RuntimeCharacterStats.cs`

```csharp
namespace Mythfall.Characters
{
    public enum StatType {
        MaxHP, AttackPower, Defense, MoveSpeed, AttackRange,
        AttackInterval, CritRate, CritDamage, CooldownReduction,
        Lifesteal, AoeRadius
    }

    public enum CombatRole { Melee, Ranged }

    [System.Serializable]
    public class CharacterBaseStats {
        public float maxHP = 100f;
        public float attackPower = 10f;
        public float defense = 5f;
        public float moveSpeed = 5f;
        public float attackRange = 1.8f;
        public float attackInterval = 0.8f;
        public float critRate = 5f;
        public float critDamage = 150f;
        public float cooldownReduction = 0f;
        public float lifesteal = 0f;
        public float aoeRadius = 0f;
    }

    [CreateAssetMenu(fileName = "CharacterData", menuName = "Mythfall/Character Data")]
    public class CharacterDataSO : ScriptableObject {
        [Header("Identity")]
        public string characterId;             // "kai", "lyra"

        [Header("Localization Keys (NOT raw text!)")]
        public string nameKey;                 // "character.kai.name"
        public string titleKey;                // "character.kai.title"
        public string loreKey;                 // "character.kai.lore"

        [Header("Visual")]
        public Sprite portrait;
        public Sprite icon;

        [Header("Star Tier (display only in slice)")]
        public int starTier = 4;               // 3, 4, 5, 6 — display ★★★★

        [Header("Type")]
        public CombatRole role;

        [Header("Stats")]
        public CharacterBaseStats baseStats;

        [Header("Skills (assigned in Sprint 3)")]
        public SkillDataSO autoAttackSkill;
        public SkillDataSO activeSkill;
        public SkillDataSO passiveSkill;

        [Header("Prefab")]
        public GameObject characterPrefab;

        // Helper methods to resolve keys at runtime
        public string GetDisplayName() {
            var loc = ServiceLocator.Get<LocalizationService>();
            return loc?.Get(nameKey) ?? characterId;
        }

        public string GetTitle() {
            var loc = ServiceLocator.Get<LocalizationService>();
            return loc?.Get(titleKey) ?? "";
        }

        public string GetLore() {
            var loc = ServiceLocator.Get<LocalizationService>();
            return loc?.Get(loreKey) ?? "";
        }
    }
```

**Tạo 2 asset (theo lore từ GAME_DESIGN.md section 3.2):**

**`Resources/Characters/Kai_Data.asset`:**
- characterId: `"kai"`
- nameKey: `"character.kai.name"` → "Kai Lôi Phong" / "Kai Stormbringer"
- titleKey: `"character.kai.title"` → "Cô Lang" / "The Lone Wolf"
- loreKey: `"character.kai.lore"`
- starTier: 4
- role: Melee
- Stats: HP 120, ATK 15, Range 1.8m, Interval 0.6s, Crit 15%, CritDmg 180%

**`Resources/Characters/Lyra_Data.asset`:**
- characterId: `"lyra"`
- nameKey: `"character.lyra.name"` → "Lyra Vọng Nguyệt" / "Lyra Moonweaver"
- titleKey: `"character.lyra.title"` → "Kẻ Phản Đồ" / "The Renegade"
- loreKey: `"character.lyra.lore"`
- starTier: 4
- role: Ranged
- Stats: HP 80, ATK 20, Range 10m, Interval 1.0s, Crit 20%, CritDmg 200%

**Cần add những keys này vào `lang_vi.json` + `lang_en.json` — see `Docs/LOCALIZATION_GUIDE.md` section 10.2 cho mẫu.**

#### Task 1.2: PlayerHealth + TargetSelector + PlayerFacing
**Files:** `Scripts/Player/PlayerHealth.cs`, `TargetSelector.cs`, `PlayerFacing.cs`

```csharp
public class PlayerHealth : MonoBehaviour {
    public float CurrentHP { get; private set; }
    public event Action<float> OnDamaged;
    public event Action OnDeath;
    bool isInvincible;

    public void Initialize(float maxHP) { ... }
    public void TakeDamage(float amount) {
        if (isInvincible) return;
        CurrentHP -= amount;
        OnDamaged?.Invoke(amount);
        Bill.Events.Fire(new PlayerDamagedEvent { damage = amount });
        SetInvincible(true, 0.3f); // brief iFrame
        if (CurrentHP <= 0) OnDeath?.Invoke();
    }
    public void Heal(float amount) { ... }
    public void SetInvincible(bool inv, float duration) { ... }
}

public class TargetSelector : MonoBehaviour {
    public Transform CurrentTarget { get; private set; }
    public event Action<Transform> OnTargetChanged;
    float searchRadius = 12f;

    void Start() {
        Bill.Timer.Repeat(0.2f, UpdateTarget);
    }

    void UpdateTarget() {
        var hits = Physics.OverlapSphere(transform.position, searchRadius,
            LayerMask.GetMask("Enemy"));
        // Find nearest alive
        // Fire event if changed
    }
}

public class PlayerFacing : MonoBehaviour {
    [SerializeField] float rotationSpeed = 15f;
    TargetSelector selector;
    CharacterLocomotion locomotion;
    bool rotationLocked;

    void Update() {
        if (rotationLocked) return;
        Vector3 lookDir;
        if (selector.CurrentTarget != null) {
            // Face target
            lookDir = (selector.CurrentTarget.position - transform.position).normalized;
            lookDir.y = 0;
        } else {
            // Face movement direction
            var vel = locomotion.HorizontalVelocity;
            if (vel.sqrMagnitude < 0.01f) return;
            lookDir = vel.normalized;
        }
        var targetRot = Quaternion.LookRotation(lookDir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot,
            Time.deltaTime * rotationSpeed);
    }

    public void LockRotation(bool locked) => rotationLocked = locked;
}
```

#### Task 1.3: PlayerBase Abstract Class
**File:** `Scripts/Player/PlayerBase.cs`

```csharp
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(CharacterLocomotion))]
public abstract class PlayerBase : MonoBehaviour {
    [SerializeField] protected CharacterDataSO characterData;
    [SerializeField] protected Transform muzzlePoint;

    protected CharacterLocomotion locomotion;
    protected PlayerFacing facing;
    protected TargetSelector targetSelector;
    protected PlayerHealth health;
    protected RuntimeCharacterStats stats;

    public CharacterDataSO Data => characterData;
    public RuntimeCharacterStats Stats => stats;
    public PlayerHealth Health => health;
    public Transform MuzzlePoint => muzzlePoint;
    public abstract PlayerCombatBase Combat { get; }

    protected virtual void Awake() {
        locomotion = GetComponent<CharacterLocomotion>();
        facing = GetComponent<PlayerFacing>();
        targetSelector = GetComponent<TargetSelector>();
        health = GetComponent<PlayerHealth>();

        // CRITICAL — transfer rotation control
        locomotion.ExternalRotationControl = true;
        locomotion.ConfigureJumps(allowDoubleJump: false);
    }

    protected virtual void Start() {
        stats = new RuntimeCharacterStats(characterData.baseStats);
        health.Initialize(stats.GetFinal(StatType.MaxHP));
        health.OnDeath += OnDeath;
    }

    protected virtual void Update() {
        var input = MobileInputManager.MoveVector;
        if (locomotion.IsGrounded())
            locomotion.HandleGroundedMovement(input, isRunning: true);
        else
            locomotion.HandleAirborneMovement(input);
    }

    protected virtual void OnDeath() {
        Bill.Events.Fire(new PlayerDiedEvent { player = this });
        Bill.State.GoTo<DefeatState>();
    }
}
```

### Day 2 — Combat + Enemy + Scene Flow

#### Task 1.4: PlayerCombatBase + Subclasses
**Files:** `Scripts/Player/PlayerCombatBase.cs`, `MeleeCombat.cs`, `RangedCombat.cs`

Combat base implements simple auto-attack logic — full skill system trong Sprint 3.

```csharp
public abstract class PlayerCombatBase : MonoBehaviour {
    protected PlayerBase owner;
    protected float attackTimer;

    public virtual void Initialize(PlayerBase o) { owner = o; }

    void Update() {
        if (attackTimer > 0) attackTimer -= Time.deltaTime;
        if (CanAttack()) Execute();
    }

    protected abstract bool CanAttack();
    protected abstract void Execute();

    protected float CalculateDamage(out bool isCrit) {
        float atk = owner.Stats.GetFinal(StatType.AttackPower);
        float critRate = owner.Stats.GetFinal(StatType.CritRate);
        float critDmg = owner.Stats.GetFinal(StatType.CritDamage);
        isCrit = UnityEngine.Random.Range(0f, 100f) < critRate;
        return isCrit ? atk * (critDmg / 100f) : atk;
    }
}

public class MeleeCombat : PlayerCombatBase {
    [SerializeField] SphereCollider hitbox;
    [SerializeField] float hitboxArcAngle = 120f;
    HashSet<EnemyBase> hitThisSwing = new();

    protected override bool CanAttack() {
        if (attackTimer > 0) return false;
        var target = owner.GetComponent<TargetSelector>().CurrentTarget;
        if (target == null) return false;
        var dist = Vector3.Distance(transform.position, target.position);
        return dist <= owner.Stats.GetFinal(StatType.AttackRange);
    }

    protected override void Execute() {
        // Trigger animation
        owner.GetComponent<Animator>().SetTrigger("Attack_1");
        attackTimer = owner.Stats.GetFinal(StatType.AttackInterval);
        // Hitbox enabled via animation event
    }

    // Animation event handlers (call from animation events)
    public void OnHitboxEnable() {
        hitbox.enabled = true;
        hitThisSwing.Clear();
    }
    public void OnHitboxDisable() => hitbox.enabled = false;

    void OnTriggerEnter(Collider other) {
        if (!hitbox.enabled) return;
        var enemy = other.GetComponent<EnemyBase>();
        if (enemy == null || hitThisSwing.Contains(enemy)) return;
        var toEnemy = (enemy.transform.position - owner.transform.position).normalized;
        var angle = Vector3.Angle(owner.transform.forward, toEnemy);
        if (angle > hitboxArcAngle / 2f) return;

        float damage = CalculateDamage(out bool isCrit);
        enemy.TakeDamage(damage, owner);
        hitThisSwing.Add(enemy);

        Bill.Events.Fire(new EnemyHitEvent {
            attacker = owner, victim = enemy, damage = damage, isCrit = isCrit,
            hitPoint = enemy.transform.position
        });
    }
}

public class RangedCombat : PlayerCombatBase {
    [SerializeField] GameObject projectilePrefab; // assigned in inspector
    public string projectilePoolKey = "Projectile_Arrow";
    public float projectileSpeed = 15f;

    protected override bool CanAttack() {
        if (attackTimer > 0) return false;
        var target = owner.GetComponent<TargetSelector>().CurrentTarget;
        if (target == null) return false;
        return Vector3.Distance(transform.position, target.position)
            <= owner.Stats.GetFinal(StatType.AttackRange);
    }

    protected override void Execute() {
        owner.GetComponent<Animator>().SetTrigger("Attack_1");
        attackTimer = owner.Stats.GetFinal(StatType.AttackInterval);

        // Spawn projectile
        var target = owner.GetComponent<TargetSelector>().CurrentTarget;
        var muzzle = owner.MuzzlePoint;
        var dir = (target.position - muzzle.position).normalized;
        var proj = Bill.Pool.Spawn(projectilePoolKey, muzzle.position,
            Quaternion.LookRotation(dir));
        var projComp = proj.GetComponent<Projectile>();
        float dmg = CalculateDamage(out bool isCrit);
        projComp.Setup(dir, projectileSpeed, dmg, isCrit, owner, pierceCount: 0);
    }
}
```

#### Task 1.5: MeleePlayer + RangedPlayer Concrete Classes
**Files:** `Scripts/Player/MeleePlayer.cs`, `RangedPlayer.cs`

```csharp
public class MeleePlayer : PlayerBase {
    [SerializeField] MeleeCombat meleeCombat;
    public override PlayerCombatBase Combat => meleeCombat;

    protected override void Start() {
        base.Start();
        meleeCombat.Initialize(this);
    }

    // Animation event proxies (Unity Animator gọi qua SendMessage tới root)
    public void OnHitboxEnable() => meleeCombat.OnHitboxEnable();
    public void OnHitboxDisable() => meleeCombat.OnHitboxDisable();
}

public class RangedPlayer : PlayerBase {
    [SerializeField] RangedCombat rangedCombat;
    public override PlayerCombatBase Combat => rangedCombat;

    protected override void Start() {
        base.Start();
        rangedCombat.Initialize(this);
    }
}
```

#### Task 1.6: Projectile + EnemyBase + SwarmerEnemy
**Files:** `Scripts/Gameplay/Projectile.cs`, `Scripts/Enemy/EnemyBase.cs`, `EnemyDataSO.cs`, `SwarmerEnemy.cs`

```csharp
public class Projectile : MonoBehaviour {
    Vector3 velocity;
    float damage;
    bool isCrit;
    int pierceLeft;
    PlayerBase owner;
    float lifetime = 5f;
    float aliveTimer;

    public void Setup(Vector3 dir, float speed, float dmg, bool crit,
                      PlayerBase owner, int pierceCount) {
        velocity = dir * speed;
        damage = dmg;
        isCrit = crit;
        pierceLeft = pierceCount;
        this.owner = owner;
        aliveTimer = 0f;
    }

    void Update() {
        transform.position += velocity * Time.deltaTime;
        aliveTimer += Time.deltaTime;
        if (aliveTimer > lifetime) Bill.Pool.Return(gameObject);
    }

    void OnTriggerEnter(Collider other) {
        var enemy = other.GetComponent<EnemyBase>();
        if (enemy == null) return;
        enemy.TakeDamage(damage, owner);

        Bill.Events.Fire(new EnemyHitEvent {
            attacker = owner, victim = enemy, damage = damage,
            isCrit = isCrit, hitPoint = transform.position
        });

        if (pierceLeft <= 0) Bill.Pool.Return(gameObject);
        else pierceLeft--;
    }
}
```

```csharp
[CreateAssetMenu(menuName = "Mythfall/Enemy Data")]
public class EnemyDataSO : ScriptableObject {
    public string enemyId;
    public float maxHP = 30f;
    public float attackPower = 5f;
    public float moveSpeed = 4f;
    public float attackRange = 0.8f;
    public float xpReward = 1f;
    public GameObject prefab;
    // VAT data optional in Sprint 1, can use Animator if simpler
}

public abstract class EnemyBase : MonoBehaviour {
    [SerializeField] protected EnemyDataSO data;
    protected float currentHP;
    protected Transform player;
    public bool IsAlive { get; private set; }

    public virtual void OnSpawn() {
        currentHP = data.maxHP;
        IsAlive = true;
        gameObject.SetActive(true);
        var p = GameObject.FindGameObjectWithTag("Player");
        player = p ? p.transform : null;
    }

    public virtual void TakeDamage(float amount, PlayerBase attacker) {
        if (!IsAlive) return;
        currentHP -= amount;
        if (currentHP <= 0) Die(attacker);
    }

    protected virtual void Die(PlayerBase killer) {
        IsAlive = false;
        Bill.Events.Fire(new EnemyKilledEvent {
            enemy = this, killer = killer, position = transform.position
        });
        OnDeath();
        Bill.Timer.Delay(1f, () => Bill.Pool.Return(gameObject));
    }

    protected abstract void OnDeath();
}

public class SwarmerEnemy : EnemyBase {
    [SerializeField] Animator anim; // Or VAT_Animator_Instanced
    float attackCooldown;

    void Update() {
        if (!IsAlive || player == null) return;

        var toPlayer = player.position - transform.position;
        toPlayer.y = 0;
        float dist = toPlayer.magnitude;

        if (dist > data.attackRange) {
            transform.position += toPlayer.normalized * data.moveSpeed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(toPlayer.normalized);
            // anim.Play("Move") — only if changed state, avoid stuttering
        } else {
            attackCooldown -= Time.deltaTime;
            if (attackCooldown <= 0f) {
                AttackPlayer();
                attackCooldown = 1.2f;
            }
        }
    }

    void AttackPlayer() {
        anim?.SetTrigger("Attack");
        Bill.Timer.Delay(0.3f, () => {
            if (!IsAlive || player == null) return;
            var dist = Vector3.Distance(transform.position, player.position);
            if (dist > data.attackRange + 0.5f) return;
            player.GetComponent<PlayerHealth>()?.TakeDamage(data.attackPower);
        });
    }

    protected override void OnDeath() {
        anim?.SetTrigger("Death");
    }
}
```

#### Task 1.7: WaveSpawner (Simple)
**File:** `Scripts/Gameplay/WaveSpawner.cs`

```csharp
public class WaveSpawner : MonoBehaviour {
    [SerializeField] Transform[] spawnPoints;
    [SerializeField] string enemyPoolKey = "Enemy_Swarmer";
    [SerializeField] int enemyPerWave = 5;
    [SerializeField] float waveInterval = 5f;

    void Start() {
        Bill.Timer.Repeat(waveInterval, SpawnWave);
    }

    void SpawnWave() {
        for (int i = 0; i < enemyPerWave; i++) {
            var point = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
            var enemy = Bill.Pool.Spawn(enemyPoolKey, point.position, Quaternion.identity);
            enemy.GetComponent<EnemyBase>().OnSpawn();
        }
    }
}
```

### Day 3 — Scene Flow + UI + Integration

#### Task 1.8: Game Events + States
**File:** `Scripts/Core/GameEvents.cs`, `Scripts/Core/States.cs`

```csharp
namespace Mythfall.Events {
    public struct EnemyHitEvent : IEvent {
        public PlayerBase attacker;
        public EnemyBase victim;
        public float damage;
        public bool isCrit;
        public Vector3 hitPoint;
    }
    public struct EnemyKilledEvent : IEvent {
        public EnemyBase enemy;
        public PlayerBase killer;
        public Vector3 position;
    }
    public struct PlayerDamagedEvent : IEvent { public float damage; }
    public struct PlayerDiedEvent : IEvent { public PlayerBase player; }
    public struct CharacterSelectedEvent : IEvent { public string characterId; }
}

namespace Mythfall.States {
    public class MainMenuState : GameState {
        public override void Enter() => Bill.UI.Open<MainMenuPanel>();
        public override void Exit() => Bill.UI.Close<MainMenuPanel>();
    }

    public class CharacterSelectState : GameState {
        public override void Enter() => Bill.UI.Open<CharacterSelectPanel>();
        public override void Exit() => Bill.UI.Close<CharacterSelectPanel>();
    }

    public class InRunState : GameState {
        public override void Enter() => Bill.UI.Open<HudPanel>();
        public override void Exit() => Bill.UI.Close<HudPanel>();
    }

    public class DefeatState : GameState {
        public override void Enter() {
            Time.timeScale = 0.3f;
            Bill.UI.Open<GameOverPanel>();
        }
        public override void Exit() {
            Time.timeScale = 1f;
            Bill.UI.Close<GameOverPanel>();
        }
    }
}
```

#### Task 1.9: InventoryService + PlayerData
**File:** `Scripts/Inventory/PlayerData.cs`, `InventoryService.cs`

```csharp
[System.Serializable]
public class PlayerData {
    public List<string> ownedCharacterIds = new() { "kai", "lyra" }; // both free in slice
    public string currentCharacterId = "kai";
    public int gold = 0;
    public int crystal = 500;
}

public class InventoryService : IService {
    PlayerData data;
    public PlayerData Data => data;

    public void Initialize() {
        data = Bill.Save.Get<PlayerData>("player") ?? new PlayerData();
    }

    public void SetCurrentCharacter(string id) {
        data.currentCharacterId = id;
        Bill.Save.Set("player", data);
        Bill.Save.Flush();
    }
}
```

#### Task 1.10: UI Panels (4 panels)
**Files:** `Scripts/UI/MainMenuPanel.cs`, `CharacterSelectPanel.cs`, `HudPanel.cs`, `GameOverPanel.cs`

Bạn chọn UI Toolkit (UXML) hoặc Canvas UGUI tùy familiarity. Recommend **UI Toolkit** cho mobile vì lighter.

**Đọc `Docs/UI_VISUAL_GUIDE.md` cho mockups + design tokens + color palette.**

Each panel extend BasePanel từ BillGameCore. **TẤT CẢ text dùng LocalizedText component** — không hardcode.

**MainMenuPanel:**
- Display "MYTHFALL SURVIVORS" title (text logo có thể là sprite, key: `ui.menu.title`)
- Play button (key `ui.menu.play`) → `Bill.State.GoTo<CharacterSelectState>()`
- **Locked feature buttons (visible nhưng disable):**
  - Gacha button — key `ui.menu.gacha`, tooltip key `ui.menu.coming_soon`
  - Inventory button — key `ui.menu.inventory`
  - Shop button — key `ui.menu.shop`
  - BP button — key `ui.menu.battle_pass`
- Settings button → opens settings overlay với language switcher (VN/EN)

**CharacterSelectPanel:**
- Title text key `ui.character_select.title`
- 2 character cards (Kai, Lyra) — show:
  - Portrait
  - Star rating ★★★★ (4★)
  - Name (resolved từ characterData.GetDisplayName())
  - Title (resolved từ characterData.GetTitle())
- 2-3 locked character cards với "???" + 🔒 icon
- Confirm button (key `ui.character_select.enter_stage`) → save selection → load Gameplay scene → `Bill.State.GoTo<InRunState>()`

**HudPanel:**
- HP bar (subscribe `PlayerDamagedEvent`) — color from UI guide
- XP bar (subscribe `XPChangedEvent` — implemented Sprint 3)
- Level text (key `ui.hud.level` formatted với current level)
- Active skill button (placeholder, wire trong Sprint 3)
- Pause button

**GameOverPanel:**
- Title key `ui.game_over.title_defeat` ("Bạn đã ngã xuống" / "You Fell")
- Subtitle key `ui.game_over.subtitle_defeat`
- Stats (time/wave/kills/level — implemented Sprint 4 với RunStatsTracker)
- Retry button (key `ui.game_over.btn_retry`)
- Hub button (key `ui.game_over.btn_hub`) → `Bill.State.GoTo<MainMenuState>()`

**Settings Overlay (NEW for slice):**
Simple overlay panel:
- Music volume slider
- SFX volume slider
- **Language dropdown:**
  - "Tiếng Việt" → calls `LocalizationService.SetLanguage("vi")`
  - "English" → calls `LocalizationService.SetLanguage("en")`
- Close button

**CRITICAL TEST:** Mỗi panel xong, switch language EN ↔ VN, verify:
- Tất cả text update đúng
- Không có overflow/clipping
- Không còn `[missing.key]` text visible

#### Task 1.11: Mobile Virtual Joystick
**File:** `Scripts/Input/MobileInputManager.cs`, `VirtualJoystick.cs`

```csharp
public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler {
    [SerializeField] RectTransform background;
    [SerializeField] RectTransform handle;
    Vector2 input;
    public Vector2 Input => input;

    public void OnPointerDown(PointerEventData e) { /* ... */ }
    public void OnDrag(PointerEventData e) {
        // Calc offset, clamp magnitude, set handle position
        // input = clampedOffset / radius
    }
    public void OnPointerUp(PointerEventData e) {
        input = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
    }
}

public static class MobileInputManager {
    public static Vector2 MoveVector => VirtualJoystick.Instance?.Input ?? Vector2.zero;
}
```

#### Task 1.12: GameplaySpawner (Update GameBootstrap)

```csharp
// Trong GameBootstrap.cs
void RegisterStates() {
    Bill.State.AddState<MainMenuState>();
    Bill.State.AddState<CharacterSelectState>();
    Bill.State.AddState<InRunState>();
    Bill.State.AddState<DefeatState>();
}

void RegisterPools() {
    Bill.Pool.Register("Enemy_Swarmer",
        Resources.Load<GameObject>("Prefabs/Enemies/Swarmer"), warmCount: 30);
    Bill.Pool.Register("Projectile_Arrow",
        Resources.Load<GameObject>("Prefabs/Projectiles/Arrow"), warmCount: 20);
}

void RegisterUIPanels() {
    // Bill.UI auto-discovers BasePanel subclasses via reflection
    // Hoặc manual register nếu framework yêu cầu
}
```

GameplayScene cần:
- WaveSpawner GameObject
- 4 spawn point GameObjects xung quanh map
- GameplaySpawner script gọi `InventoryService.Data.currentCharacterId` để spawn đúng character prefab

---

## ✅ DEFINITION OF DONE

### Functional
- [ ] Boot app → MainMenu hiển thị
- [ ] Click Play → CharacterSelect hiển thị
- [ ] Chọn Kai → load GameplayScene → Kai spawned
- [ ] Joystick di chuyển Kai
- [ ] Kai auto-attack Swarmer trong range
- [ ] Swarmer chase + attack player
- [ ] Player die → GameOver panel
- [ ] Back to menu button works
- [ ] Character choice saved (verify by quit + reopen)

### Localization (CRITICAL)
- [ ] Mọi UI text qua LocalizedText component (không có hardcoded text)
- [ ] CharacterDataSO dùng nameKey/titleKey/loreKey (không phải raw text)
- [ ] Settings overlay có language switcher VN/EN
- [ ] Switch VN→EN: tất cả UI update đúng, không clip, không [missing.key]
- [ ] Switch EN→VN: tương tự
- [ ] Language preference saved (verify by quit + reopen)
- [ ] Vietnamese diacritics render correctly (test với "Lôi Phong", "Vọng Nguyệt")

### Visual / Lore Compliance
- [ ] Character cards show 4★ rating
- [ ] 2-3 locked character cards visible (cho tease future)
- [ ] Locked feature buttons (Gacha/Inventory/Shop/BP) visible nhưng disable, có "Coming Soon" tooltip
- [ ] Character name + title match GAME_DESIGN.md (Kai = "Cô Lang", Lyra = "Kẻ Phản Đồ")

### Technical
- [ ] 0 compile error
- [ ] `locomotion.ExternalRotationControl = true` confirmed
- [ ] Pool.Spawn used cho enemies + projectiles
- [ ] `Bill.Events` dispatched cho hit/kill/damage
- [ ] FPS ≥ 30 với 20 swarmers on screen
- [ ] PROGRESS.md updated

---

## 🧪 TEST CHECKLIST (Sprint 1 v0.2)

### Build & Boot
- [ ] APK build < 35 MB
- [ ] Boot < 5s
- [ ] Main menu visible

### Menu Flow
- [ ] Play button → CharacterSelect
- [ ] Both characters visible với portrait
- [ ] Confirm button transitions to gameplay

### Gameplay — Movement
- [ ] Joystick responsive (deadzone không quá lớn)
- [ ] Player moves smoothly
- [ ] Camera follows player
- [ ] No stutter khi turn nhanh

### Gameplay — Combat (Melee Kai)
- [ ] Player auto-attack khi enemy < 1.8m
- [ ] Animation Attack_1 plays
- [ ] Hitbox damages enemy
- [ ] Crit hits xảy ra (~15% rate cho Kai)
- [ ] Enemy HP visually decrease (placeholder OK)
- [ ] Enemy dies sau N hits

### Gameplay — Combat (Ranged Lyra)
- [ ] Player attacks khi enemy < 10m
- [ ] Projectile spawns từ muzzle point
- [ ] Projectile flies straight to enemy
- [ ] Projectile hits → enemy damaged
- [ ] Projectile returns to pool sau 5s

### Gameplay — Enemy
- [ ] Wave 5 swarmers spawn mỗi 5s
- [ ] Swarmers chase player
- [ ] Swarmers attack player khi gần
- [ ] Player HP decrease khi bị hit
- [ ] Player có iFrame brief sau bị hit

### Game Over
- [ ] Player HP = 0 → GameOver panel
- [ ] Time scale slow đến 0.3x
- [ ] Back to Menu button works
- [ ] Quay về MainMenu, có thể play lại

### Save/Load
- [ ] Quit app trên gameplay
- [ ] Reopen app
- [ ] Last selected character được pre-select trong CharacterSelect

### Performance
- [ ] FPS ≥ 30 với 20 swarmers
- [ ] No memory leak: Bill.Pool stats stable sau 3 runs
- [ ] No crash trong 5 phút continuous play

---

## ⚠️ COMMON ISSUES

| Issue | Solution |
|---|---|
| Player rotates erratically | Confirm `ExternalRotationControl = true`, PlayerFacing has rotationLocked = false |
| Enemies không attack player | Check `Player` tag set on player GameObject |
| Hitbox not hitting | Check Layer Collision Matrix: PlayerHitbox vs Enemy |
| Projectile xuyên qua enemy | Check Projectile có Rigidbody (kinematic) + Collider (trigger) |
| Animator events không fire | Animation event function name match exact (case sensitive) |

---

## 🎬 NEXT — Sprint 2

Sau Sprint 1 done, tiếp Sprint 2: **Combat Variety + Boss + Polish Layer 1**.

Đọc `Sprints/SPRINT_2_COMBAT_FEEL.md`.
