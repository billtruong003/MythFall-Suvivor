# ✨ SPRINT 3 — Skills + In-Run Progression

> **Duration:** 3 ngày | **Output:** Active + Passive skills, level up system, 8 upgrade cards. Mỗi run feel khác nhau.

---

## 🎯 Sprint Goal

Sprint 1+2 có combat. Sprint 3 thêm **build depth** và **progression rewarding loop**:

End of sprint:
- 2 active skill (Berserker Rush cho Kai, Overcharge Shot cho Lyra) với feel epic
- 2 passive skill (Bloodlust cho Kai, Marked Target cho Lyra)
- XP gem drop từ enemy kill, magnet pickup
- Level up trigger pause + 3 cards
- 8 upgrade cards với distinct effects
- UpgradeCardSO architecture sẵn sàng cho thêm card sau

**Quan trọng:** Skill design feel là trách nhiệm chính của Claude Code.

## 📚 PHẢI ĐỌC TRƯỚC KHI BẮT ĐẦU

- ⭐ **`Docs/GAME_DESIGN.md`** sections 3.2.1 (Kai), 3.2.2 (Lyra) — bám đúng combat fantasy:
  - Kai: *"Càng đau càng khoẻ, cuối cùng hóa cơn bão sống"* — Lôi Bộc + Bloodlust
  - Lyra: *"Mỗi mục tiêu là một câu trả lời cho tội lỗi của tộc tôi"* — Nguyệt Quang Tiễn + Nguyệt Ấn
- ⭐ **`Docs/SKILL_DESIGN_GUIDE.md`** — 7 layers feel + 5 pattern templates
- **`Docs/LOCALIZATION_GUIDE.md`** — skill names + descriptions dùng keys
- **`Templates/SKILL_TEMPLATE.cs`** — code skeleton cho skill mới

## ✅ Prerequisites
- [ ] Sprint 2 done — combat polish + boss
- [ ] User prepared: Animation clips cho skills (Skill_Active_1 cho Kai rush, Skill_Cast cho Lyra charge)

---

## 📋 TASK BREAKDOWN

### Day 1 — Skill System Architecture

#### Task 3.1: SkillDataSO + ISkillExecution
**Files:** `Scripts/Skills/SkillDataSO.cs`, `ISkillExecution.cs`, `SkillContext.cs`

```csharp
namespace Mythfall.Skills
{
    public enum SkillType { AutoAttack, Active, Passive }
    public enum SkillTargetType { Self, NearestEnemy, AllEnemiesInRange, Direction }

    public abstract class SkillDataSO : ScriptableObject {
        [Header("Identity")]
        public string skillId;
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;

        [Header("Type")]
        public SkillType skillType;
        public SkillTargetType targetType;

        [Header("Cost & Cooldown")]
        public float cooldown = 0f;
        public float duration = 0f;

        [Header("Animation")]
        public string animationTrigger;
        public float animationDuration = 0.5f;

        public abstract ISkillExecution CreateExecution(SkillContext context);
    }

    public interface ISkillExecution {
        bool CanExecute();
        void Execute();
        void Tick(float dt);
        bool IsFinished { get; }
    }

    public class SkillContext {
        public PlayerBase Owner;
        public RuntimeCharacterStats Stats;
        public TargetSelector TargetSelector;
        public SkillDataSO Data;
    }
}
```

#### Task 3.2: PlayerSkillManager
**File:** `Scripts/Player/PlayerSkillManager.cs`

```csharp
public class PlayerSkillManager : MonoBehaviour {
    PlayerBase owner;
    List<ISkillExecution> passives = new();
    ISkillExecution autoAttack;
    ISkillExecution activeSkill;
    float activeSkillCooldown;
    SkillDataSO activeSkillData; // for cooldown tracking

    public float ActiveCooldownRemaining => Mathf.Max(0, activeSkillCooldown);
    public float ActiveCooldownTotal => activeSkillData?.cooldown ?? 0;

    public void Initialize(PlayerBase o, CharacterDataSO data) {
        owner = o;
        var ctx = new SkillContext {
            Owner = o, Stats = o.Stats,
            TargetSelector = o.GetComponent<TargetSelector>()
        };

        if (data.autoAttackSkill != null) {
            ctx.Data = data.autoAttackSkill;
            autoAttack = data.autoAttackSkill.CreateExecution(ctx);
        }
        if (data.passiveSkill != null) {
            ctx.Data = data.passiveSkill;
            passives.Add(data.passiveSkill.CreateExecution(ctx));
        }
        activeSkillData = data.activeSkill;
    }

    void Update() {
        float dt = Time.deltaTime;

        // Tick passives (always running)
        foreach (var p in passives) p.Tick(dt);

        // Auto-attack
        if (autoAttack != null) {
            autoAttack.Tick(dt);
            if (autoAttack.CanExecute()) autoAttack.Execute();
        }

        // Active skill
        if (activeSkillCooldown > 0f) activeSkillCooldown -= dt;
        if (activeSkill != null) {
            activeSkill.Tick(dt);
            if (activeSkill.IsFinished) activeSkill = null;
        }
    }

    public void TryCastActive() {
        if (activeSkill != null) return; // already casting
        if (activeSkillCooldown > 0f) return; // on CD
        if (activeSkillData == null) return;

        var ctx = new SkillContext {
            Owner = owner, Stats = owner.Stats,
            TargetSelector = owner.GetComponent<TargetSelector>(),
            Data = activeSkillData
        };
        activeSkill = activeSkillData.CreateExecution(ctx);

        if (activeSkill.CanExecute()) {
            activeSkill.Execute();
            activeSkillCooldown = activeSkillData.cooldown;
            Bill.Events.Fire(new SkillCastEvent {
                caster = owner, skillId = activeSkillData.skillId
            });
        } else {
            activeSkill = null;
        }
    }
}
```

#### Task 3.3: Refactor MeleeAutoAttack + RangedAutoAttack thành Skills
Move logic from `MeleeCombat.cs` / `RangedCombat.cs` vào dạng `ISkillExecution`. Combat scripts giờ chỉ là wrapper expose hitbox events.

**File:** `Scripts/Skills/MeleeAutoAttackSO.cs`

```csharp
[CreateAssetMenu(menuName = "Mythfall/Skills/Melee Auto-Attack")]
public class MeleeAutoAttackSO : SkillDataSO {
    public float damageMultiplier = 1f;
    public override ISkillExecution CreateExecution(SkillContext ctx) =>
        new MeleeAutoAttackExecution(this, ctx);
}

public class MeleeAutoAttackExecution : ISkillExecution {
    MeleeAutoAttackSO data;
    SkillContext ctx;
    float attackTimer;
    public bool IsFinished => false;

    public MeleeAutoAttackExecution(MeleeAutoAttackSO d, SkillContext c) {
        data = d; ctx = c;
    }

    public bool CanExecute() {
        if (attackTimer > 0) return false;
        var t = ctx.TargetSelector.CurrentTarget;
        if (t == null) return false;
        var dist = Vector3.Distance(ctx.Owner.transform.position, t.position);
        return dist <= ctx.Stats.GetFinal(StatType.AttackRange);
    }

    public void Execute() {
        // Trigger animator
        ctx.Owner.GetComponent<Animator>().SetTrigger(data.animationTrigger);
        attackTimer = ctx.Stats.GetFinal(StatType.AttackInterval);
        // Hitbox enabled via animation event → MeleeCombat handles damage
    }

    public void Tick(float dt) {
        if (attackTimer > 0) attackTimer -= dt;
    }
}
```

Similar cho RangedAutoAttackSO.

### Day 2 — Active + Passive Skills

#### Task 3.4: BerserkerRush (Kai's Active)
**File:** `Scripts/Skills/BerserkerRushSO.cs`

Đọc `Docs/SKILL_DESIGN_GUIDE.md` Section "Active Skill Patterns".

**Design feel:**
- Kai lao về phía trước rất nhanh (8m trong 0.5s)
- Trail VFX màu đỏ behind
- Invincible 2s
- Damage 300% ATK trên path
- Kill enemy trong rush → reset 50% CD
- Audio: "WAAAAH!" battle cry + woosh
- Screen shake mạnh khi rush start

```csharp
[CreateAssetMenu(menuName = "Mythfall/Skills/Berserker Rush")]
public class BerserkerRushSO : SkillDataSO {
    public float rushDistance = 8f;
    public float rushSpeed = 16f;
    public float damageMultiplier = 3f;
    public float invincibilityDuration = 2f;
    public float resetCDOnKillPercent = 0.5f;

    void OnEnable() {
        skillType = SkillType.Active;
        cooldown = 12f;
        animationTrigger = "Skill_Active_1";
    }

    public override ISkillExecution CreateExecution(SkillContext ctx) =>
        new BerserkerRushExecution(this, ctx);
}

public class BerserkerRushExecution : ISkillExecution {
    BerserkerRushSO data;
    SkillContext ctx;
    float elapsed;
    bool finished;
    HashSet<EnemyBase> hit = new();

    public bool IsFinished => finished;

    public BerserkerRushExecution(BerserkerRushSO d, SkillContext c) {
        data = d; ctx = c;
    }

    public bool CanExecute() => true;

    public void Execute() {
        // Trigger anim
        ctx.Owner.GetComponent<Animator>().SetTrigger(data.animationTrigger);

        // Invincibility
        ctx.Owner.Health.SetInvincible(true, data.invincibilityDuration);

        // Lock locomotion (don't auto-move via input)
        ctx.Owner.GetComponent<CharacterLocomotion>().enabled = false;

        // Lock rotation
        ctx.Owner.GetComponent<PlayerFacing>().LockRotation(true);

        // Audio + shake + VFX
        Bill.Audio.Play("sfx_kai_rush_battle_cry");
        Bill.Events.Fire(new ScreenShakeEvent { intensity = 0.5f, duration = 0.2f });
        // Spawn trail VFX (placeholder)
    }

    public void Tick(float dt) {
        elapsed += dt;
        float maxTime = data.rushDistance / data.rushSpeed;

        // Manual movement
        var charCtrl = ctx.Owner.GetComponent<CharacterController>();
        var forward = ctx.Owner.transform.forward;
        charCtrl.Move(forward * data.rushSpeed * dt);

        // Damage enemies trên path
        var hits = Physics.OverlapSphere(ctx.Owner.transform.position, 1.2f,
            LayerMask.GetMask("Enemy"));
        foreach (var h in hits) {
            var enemy = h.GetComponent<EnemyBase>();
            if (enemy == null || hit.Contains(enemy)) continue;
            hit.Add(enemy);

            float dmg = ctx.Stats.GetFinal(StatType.AttackPower) * data.damageMultiplier;
            enemy.TakeDamage(dmg, ctx.Owner);

            // Check kill → reset CD partial
            if (!enemy.IsAlive) {
                var sm = ctx.Owner.GetComponent<PlayerSkillManager>();
                // Need internal API to reduce cooldown
                sm.ReduceCooldown(data.cooldown * data.resetCDOnKillPercent);
            }
        }

        if (elapsed >= maxTime) Finish();
    }

    void Finish() {
        finished = true;
        ctx.Owner.GetComponent<CharacterLocomotion>().enabled = true;
        ctx.Owner.GetComponent<PlayerFacing>().LockRotation(false);
    }
}
```

**Note:** `PlayerSkillManager.ReduceCooldown(float)` needs to be added.

#### Task 3.5: OverchargeShot (Lyra's Active)
**File:** `Scripts/Skills/OverchargeShotSO.cs`

**Design feel:**
- Lyra slow xuống 0% movement, charge 1s
- Bow draws back animation, electric crackle audio + VFX
- Sau 1s, bắn beam xuyên tất cả enemies trên đường thẳng
- Damage 500% ATK
- Knockback enemies trên path
- Audio: charge whir + bow release THWACK

```csharp
[CreateAssetMenu(menuName = "Mythfall/Skills/Overcharge Shot")]
public class OverchargeShotSO : SkillDataSO {
    public float chargeTime = 1f;
    public float damageMultiplier = 5f;
    public string projectilePoolKey = "Projectile_OverchargedArrow";
    public float projectileSpeed = 30f;
    public float knockbackForce = 8f;

    void OnEnable() {
        skillType = SkillType.Active;
        cooldown = 15f;
        animationTrigger = "Skill_Cast_Long";
    }

    public override ISkillExecution CreateExecution(SkillContext ctx) =>
        new OverchargeShotExecution(this, ctx);
}

public class OverchargeShotExecution : ISkillExecution {
    OverchargeShotSO data;
    SkillContext ctx;
    float chargeElapsed;
    bool fired;
    public bool IsFinished => fired;

    public OverchargeShotExecution(OverchargeShotSO d, SkillContext c) {
        data = d; ctx = c;
    }

    public bool CanExecute() => ctx.TargetSelector.CurrentTarget != null;

    public void Execute() {
        ctx.Owner.GetComponent<Animator>().SetTrigger(data.animationTrigger);

        // Slow movement to 0% during charge
        // (set CharacterLocomotion speed multiplier nếu có, hoặc disable component)

        // Charging VFX + audio
        Bill.Audio.Play("sfx_lyra_charge_whir");
        // Spawn charge VFX at muzzle (sustain effect)
    }

    public void Tick(float dt) {
        chargeElapsed += dt;
        if (chargeElapsed < data.chargeTime) return;

        // Fire!
        FireBeam();
        fired = true;
    }

    void FireBeam() {
        var target = ctx.TargetSelector.CurrentTarget;
        if (target == null) return;

        var muzzle = ctx.Owner.MuzzlePoint;
        var dir = (target.position - muzzle.position).normalized;
        var proj = Bill.Pool.Spawn(data.projectilePoolKey, muzzle.position,
            Quaternion.LookRotation(dir));

        var pComp = proj.GetComponent<Projectile>();
        float dmg = ctx.Stats.GetFinal(StatType.AttackPower) * data.damageMultiplier;
        pComp.Setup(dir, data.projectileSpeed, dmg, isCrit: true,
            ctx.Owner, pierceCount: 999);
        pComp.KnockbackForce = data.knockbackForce; // add this property

        Bill.Audio.Play("sfx_lyra_bow_release_thwack");
        Bill.Events.Fire(new ScreenShakeEvent {
            intensity = 0.4f, duration = 0.15f
        });
    }
}
```

#### Task 3.6: Bloodlust Passive (Kai's)
**File:** `Scripts/Skills/BloodlustSO.cs`

**Design:** HP < 50% → ATK +40%, HP < 25% → ATK +40% more. Kill enemy → heal 1% max HP.

```csharp
[CreateAssetMenu(menuName = "Mythfall/Skills/Bloodlust")]
public class BloodlustSO : SkillDataSO {
    public float threshold1 = 0.5f;
    public float atkBonus1 = 0.4f;
    public float threshold2 = 0.25f;
    public float atkBonus2 = 0.4f;
    public float healPercentPerKill = 0.01f;

    public override ISkillExecution CreateExecution(SkillContext ctx) =>
        new BloodlustExecution(this, ctx);
}

public class BloodlustExecution : ISkillExecution {
    BloodlustSO data;
    SkillContext ctx;
    bool tier1, tier2;
    public bool IsFinished => false;

    public BloodlustExecution(BloodlustSO d, SkillContext c) {
        data = d; ctx = c;
        Bill.Events.Subscribe<EnemyKilledEvent>(OnKill);
        // Hook on tear down... but this is passive, runs throughout the run
    }

    public bool CanExecute() => true;
    public void Execute() { } // no manual trigger

    public void Tick(float dt) {
        float ratio = ctx.Owner.Health.CurrentHP / ctx.Stats.GetFinal(StatType.MaxHP);

        // Tier 1
        if (!tier1 && ratio < data.threshold1) {
            ctx.Stats.AddModifier(StatType.AttackPower, data.atkBonus1, ModifierType.Multiplicative);
            tier1 = true;
            ShowBuffEffect("Bloodlust I");
        } else if (tier1 && ratio >= data.threshold1) {
            ctx.Stats.RemoveModifier(StatType.AttackPower, data.atkBonus1, ModifierType.Multiplicative);
            tier1 = false;
        }

        // Tier 2
        if (!tier2 && ratio < data.threshold2) {
            ctx.Stats.AddModifier(StatType.AttackPower, data.atkBonus2, ModifierType.Multiplicative);
            tier2 = true;
            ShowBuffEffect("Bloodlust II");
        } else if (tier2 && ratio >= data.threshold2) {
            ctx.Stats.RemoveModifier(StatType.AttackPower, data.atkBonus2, ModifierType.Multiplicative);
            tier2 = false;
        }
    }

    void OnKill(EnemyKilledEvent e) {
        if (e.killer != ctx.Owner) return;
        float heal = ctx.Stats.GetFinal(StatType.MaxHP) * data.healPercentPerKill;
        ctx.Owner.Health.Heal(heal);
    }

    void ShowBuffEffect(string name) {
        // Visual: red rim light tint on player
        // Notification: "Bloodlust I activated!"
    }
}
```

#### Task 3.7: MarkedTarget Passive (Lyra's)
**File:** `Scripts/Skills/MarkedTargetSO.cs`

Mỗi hit cùng enemy stack damage +20%, max 4 stack. Enemy chết → transfer stack to next target.

### Day 3 — XP, Level Up, Upgrade Cards

#### Task 3.8: XPGem
**File:** `Scripts/Gameplay/XPGem.cs`

```csharp
public class XPGem : MonoBehaviour {
    [SerializeField] float xpValue = 1f;
    [SerializeField] float magnetRange = 3f;
    [SerializeField] float magnetSpeed = 12f;
    [SerializeField] float pickupDist = 0.5f;
    Transform player;

    public void Setup(float xp) {
        xpValue = xp;
        var p = GameObject.FindGameObjectWithTag("Player");
        player = p?.transform;
    }

    void Update() {
        if (player == null) return;
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist < pickupDist) {
            Bill.Events.Fire(new XPCollectedEvent { amount = xpValue });
            Bill.Pool.Return(gameObject);
            return;
        }

        if (dist < magnetRange) {
            transform.position = Vector3.MoveTowards(
                transform.position, player.position, magnetSpeed * Time.deltaTime);
        }
    }
}
```

Spawn từ EnemyBase.Die():
```csharp
protected virtual void Die(PlayerBase killer) {
    // ... existing code ...
    var gem = Bill.Pool.Spawn("XP_Gem", transform.position, Quaternion.identity);
    gem.GetComponent<XPGem>().Setup(data.xpReward);
}
```

#### Task 3.9: LevelSystem
**File:** `Scripts/Gameplay/LevelSystem.cs`

```csharp
public class LevelSystem : MonoBehaviour {
    public int CurrentLevel { get; private set; } = 1;
    public float CurrentXP { get; private set; }
    public float XPForNextLevel => Mathf.Pow(CurrentLevel, 1.5f) * 10f;

    void Awake() {
        Bill.Events.Subscribe<XPCollectedEvent>(OnXPCollected);
    }

    void OnXPCollected(XPCollectedEvent e) {
        CurrentXP += e.amount;
        Bill.Events.Fire(new XPChangedEvent {
            current = CurrentXP, target = XPForNextLevel
        });

        while (CurrentXP >= XPForNextLevel) {
            CurrentXP -= XPForNextLevel;
            CurrentLevel++;
            Bill.Events.Fire(new PlayerLeveledUpEvent { newLevel = CurrentLevel });
        }
    }

    void OnDestroy() => Bill.Events.Unsubscribe<XPCollectedEvent>(OnXPCollected);
}
```

#### Task 3.10: UpgradeCardSO + 8 Cards
**File:** `Scripts/Skills/UpgradeCardSO.cs`

```csharp
public enum CardRarity { Common, Rare, Epic }

public abstract class UpgradeCardSO : ScriptableObject {
    public string cardId;
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;
    public CardRarity rarity;
    public CombatRole? restrictRole; // null = applies all

    public abstract void Apply(PlayerBase player);
}

// Concrete cards
[CreateAssetMenu(menuName = "Mythfall/Cards/Stat Bonus")]
public class StatBonusCardSO : UpgradeCardSO {
    public StatType statType;
    public float bonusValue;
    public ModifierType modifierType;

    public override void Apply(PlayerBase player) {
        player.Stats.AddModifier(statType, bonusValue, modifierType);
    }
}
```

**8 Cards (mix rarity):**

| Card | Rarity | Effect |
|---|---|---|
| Bloodthirst | Common | +12% ATK (multiplicative) |
| Iron Skin | Common | +15% Max HP |
| Critical Mastery | Common | +8% CRIT Rate, +20% CRIT DMG |
| Swift Steps | Common | +10% Move Speed |
| Vampiric Touch | Rare | +3% Lifesteal |
| Piercing Edge | Rare | (Ranged only) Projectile pierce +1 |
| Shockwave | Rare | (Melee only) Each hit emits 1.5m AoE damage |
| Reaper's Contract | Epic | +100% ATK, -40% Max HP |

Tạo 8 SO assets trong `Resources/Cards/`.

#### Task 3.11: UpgradeSystem + Card Selection
**File:** `Scripts/Gameplay/UpgradeSystem.cs`

```csharp
public class UpgradeSystem : MonoBehaviour {
    [SerializeField] UpgradeCardSO[] allCards; // populate via inspector hoặc Resources.LoadAll
    PlayerBase player;

    void Awake() {
        Bill.Events.Subscribe<PlayerLeveledUpEvent>(OnLevelUp);
    }

    public void Initialize(PlayerBase p) {
        player = p;
        if (allCards == null || allCards.Length == 0) {
            allCards = Resources.LoadAll<UpgradeCardSO>("Cards");
        }
    }

    void OnLevelUp(PlayerLeveledUpEvent e) {
        var picks = DrawRandomCards(3);
        Time.timeScale = 0f;
        Bill.UI.Open<UpgradePanel>(p => p.Setup(picks, OnCardSelected));
    }

    UpgradeCardSO[] DrawRandomCards(int count) {
        // Filter by player role
        var role = player.Data.role;
        var filtered = allCards.Where(c => !c.restrictRole.HasValue
            || c.restrictRole.Value == role).ToList();
        // Shuffle, take count
        var picks = new List<UpgradeCardSO>();
        for (int i = 0; i < count && filtered.Count > 0; i++) {
            int idx = Random.Range(0, filtered.Count);
            picks.Add(filtered[idx]);
            filtered.RemoveAt(idx);
        }
        return picks.ToArray();
    }

    void OnCardSelected(UpgradeCardSO card) {
        card.Apply(player);
        Bill.UI.Close<UpgradePanel>();
        Time.timeScale = 1f;
        Bill.Events.Fire(new CardPickedEvent { cardId = card.cardId });
    }

    void OnDestroy() => Bill.Events.Unsubscribe<PlayerLeveledUpEvent>(OnLevelUp);
}
```

#### Task 3.12: UpgradePanel UI
**File:** `Scripts/UI/UpgradePanel.cs`

Hiện 3 cards:
- Card icon + name + description
- Rarity color border (gray/blue/purple)
- Stagger appear animation (delay 0.1s mỗi card)
- Click card → callback selected

#### Task 3.13: HUD Update — XP Bar + Skill Cooldown
Update `HudPanel`:
- XP bar bottom of screen, fill từ event `XPChangedEvent`
- Active skill button bottom-right có cooldown ring overlay
- Subscribe `Player.SkillManager.ActiveCooldownRemaining` để update fill

#### Task 3.14: Wire Skills to Character Data Assets

User manual trong Unity Editor:
- Open `Kai_Data.asset`
- Drag `Kai_AutoAttack.asset` vào `autoAttackSkill`
- Drag `Kai_BerserkerRush.asset` vào `activeSkill`
- Drag `Kai_Bloodlust.asset` vào `passiveSkill`

Similar cho Lyra.

---

## ✅ DEFINITION OF DONE

### Functional
- [ ] Player level up khi đủ XP
- [ ] Level up pause game + show 3 cards
- [ ] Pick card → effect apply ngay (test bằng stat check)
- [ ] Active skill button bottom-right hoạt động
- [ ] Active skill có cooldown indicator
- [ ] Bloodlust trigger khi HP < 50% (visible buff icon hoặc effect)
- [ ] 8 cards distinct effects, có thể reproduce trong run

### Skill Feel
- [ ] BerserkerRush feel "explosive" — fast, impactful, có audio cue
- [ ] OverchargeShot feel "weighty" — charge buildup tension, big release
- [ ] Bloodlust visible khi active (vfx hoặc tint)

### Technical
- [ ] PlayerSkillManager.ReduceCooldown method works
- [ ] Card pool draw không trùng trong cùng draw
- [ ] Time.timeScale restore = 1 sau pick card

---

## 🧪 TEST CHECKLIST (Sprint 3 v0.4)

### XP & Level Up
- [ ] Kill swarmer → XP gem drop
- [ ] Walk near gem → magnet pull
- [ ] Touch gem → XP added, gem disappear
- [ ] XP bar fill tăng
- [ ] Đủ XP → level up event
- [ ] Pause time, 3 cards xuất hiện
- [ ] Pick card → resume time, effect applied

### Active Skills — Berserker Rush (Kai)
- [ ] Skill button visible HUD
- [ ] Click button → Kai rushes forward
- [ ] Rush distance ~8m
- [ ] Damage enemies trên path (visible damage number)
- [ ] Invincible during rush (test bằng stand on enemy)
- [ ] Kill enemy in rush → CD reset 50%
- [ ] CD tracks correctly

### Active Skills — Overcharge Shot (Lyra)
- [ ] Click button → Lyra start charging
- [ ] Movement slow xuống
- [ ] 1s sau, beam release
- [ ] Beam xuyên qua nhiều enemies
- [ ] High damage (5x ATK visible in numbers)
- [ ] Knockback enemies trên path

### Passive Skills
- [ ] Bloodlust: damage Kai xuống < 50% HP → ATK boost noticeable
- [ ] Bloodlust: HP < 25% → bigger boost
- [ ] Bloodlust: kill enemy → heal 1% HP
- [ ] MarkedTarget: hit cùng enemy → damage tăng dần (numbers go up)

### Upgrade Cards
- [ ] Common card frequent
- [ ] Rare card sometimes
- [ ] Epic card rare
- [ ] Card tooltip readable
- [ ] Restrict role works (Piercing Edge không hiện cho Kai)
- [ ] No duplicate cards trong cùng draw

### Stack Effects
- [ ] Multiple Bloodthirst pick → ATK stack
- [ ] Multiple Iron Skin → HP stack
- [ ] Visual feedback khi stat change

### Performance
- [ ] FPS stable với 5+ XP gems on screen
- [ ] No memory leak qua 5 level ups

---

## ⚠️ COMMON ISSUES

| Issue | Solution |
|---|---|
| Time.timeScale = 0 stuck after pick | Ensure UpgradePanel.OnCardSelected sets Time.timeScale = 1 |
| Bloodlust trigger nhiều lần | Use bool flag tier1/tier2, check trước khi add modifier |
| Card pool empty | Resources.LoadAll path đúng "Cards" (without Resources/) |
| Active skill spam | Check activeSkillCooldown > 0 |
| XP gem stuck on geometry | Magnet ignore obstacles, just lerp position |

---

## 🎬 NEXT — Sprint 4

Sau Sprint 3 done, tiếp Sprint 4: **Polish + Audio/VFX + Final Loop**.

Đọc `Sprints/SPRINT_4_POLISH_LOOP.md`.
