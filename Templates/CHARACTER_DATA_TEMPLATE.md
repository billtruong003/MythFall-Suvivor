# 👤 CHARACTER DATA TEMPLATE

> **Mục đích:** Step-by-step để setup character mới trong Unity Editor.
> **Use khi:** Add character mới (ngoài Kai/Lyra default).

---

## 📋 PRE-REQUISITES

User phải prepare trước:
- [ ] Character humanoid 3D model (FBX)
- [ ] Animation clips: Idle, Run, Attack_1, Skill_Active_1, Death (minimum)
- [ ] Optional: Skill_Cast_Long, Hit_React

---

## 🎬 STEP-BY-STEP SETUP

### Step 1: Import Character Model
1. Drop FBX vào `Assets/Mythfall/Models/Characters/[CharacterName]/`
2. In FBX import settings:
   - Rig → Animation Type: **Humanoid**
   - Avatar Definition: Create From This Model
3. Apply

### Step 2: Setup Animator Controller
1. Right-click Animator folder → Create → Animator Controller
2. Save as `[CharacterName]_AnimatorController.controller` trong `Assets/Mythfall/Animations/[CharacterName]/`
3. Open Animator window

**Base Layer states:**
- `Idle` (default state, drag Idle clip)
- `Run` (drag Run clip)
- `Attack_1` (drag Attack_1 clip)
- `Skill_Active_1` (drag skill animation)
- `Death` (drag Death clip)

**Parameters:**
- `Speed` (Float) — drives Idle/Run blend
- `Attack_1` (Trigger) — auto-attack
- `Skill_Active_1` (Trigger) — active skill
- `Death` (Trigger) — death anim

**Transitions:**

| From | To | Condition | Settings |
|---|---|---|---|
| Idle | Run | `Speed > 0.1` | Has Exit Time: false, Duration: 0.1s |
| Run | Idle | `Speed < 0.1` | Has Exit Time: false, Duration: 0.1s |
| Any State | Attack_1 | Trigger Attack_1 | Has Exit Time: false, Duration: 0.05s |
| Attack_1 | Idle | After clip | Has Exit Time: true (0.85), Duration: 0.1s |
| Any State | Skill_Active_1 | Trigger Skill_Active_1 | Has Exit Time: false, Duration: 0.1s |
| Skill_Active_1 | Idle | After clip | Has Exit Time: true (0.85), Duration: 0.15s |
| Any State | Death | Trigger Death | Has Exit Time: false |
| Death | (none) | - | Stays in death state |

### Step 3: Add Animation Events
**For Attack_1 clip:**
1. Select clip trong Project
2. Open Animation tab
3. Add events at frames:

| Frame % | Function name |
|---|---|
| 5% | `OnAttackStart` |
| 30% | `OnHitboxEnable` |
| 55% | `OnHitboxDisable` |
| 75% | `OnComboWindowOpen` |
| 100% | `OnAttackEnd` |

**For Skill_Active_1 clip:**
| Frame % | Function name |
|---|---|
| 50% | `OnSkillImpact` |

### Step 4: Create Player Prefab

1. Drag character model vào scene
2. Add components:
   - **CharacterController** (radius 0.4, height 1.8)
   - **CharacterLocomotion** (set walkSpeed=3, runSpeed=6)
   - **PlayerFacing**
   - **TargetSelector**
   - **PlayerHealth**
   - **PlayerSkillManager**
   - **MeleeCombat** OR **RangedCombat** (theo role)
   - **MeleePlayer** OR **RangedPlayer**
   - **HitFlashController**
   - **KnockbackReceiver**

3. Set Player tag và Layer:
   - Tag: `Player`
   - Layer: `Player`

4. Add child GameObject `[MuzzlePoint]` ở vị trí weapon tip
   - Assign vào `PlayerBase.muzzlePoint` field

5. Add `[Hitbox]` child GameObject (chỉ cho melee):
   - SphereCollider (radius 1.5, IsTrigger = true)
   - Layer: `PlayerHitbox`
   - Position: vector forward 1m
   - Assign vào `MeleeCombat.hitbox`

6. Save as prefab: `Assets/Mythfall/Prefabs/Players/[CharacterName].prefab`

### Step 5: Create CharacterDataSO Asset

1. Right-click in `Resources/Characters/` folder
2. Create → Mythfall → Character Data
3. Rename: `[CharacterName]_Data.asset`
4. Fill fields:

```
Identity:
  characterId: "kai" (lowercase, no spaces)
  displayName: "Kai the Berserker"
  description: "A relentless warrior driven by fury..."
  portrait: <Sprite>
  icon: <Sprite>

Type:
  role: Melee (or Ranged)

Stats: (use vertical slice defaults — adjust later)
  Melee defaults:
    maxHP: 120
    attackPower: 15
    defense: 8
    moveSpeed: 6
    attackRange: 1.8
    attackInterval: 0.6
    critRate: 15
    critDamage: 180
  Ranged defaults:
    maxHP: 80
    attackPower: 20
    defense: 3
    moveSpeed: 5
    attackRange: 10
    attackInterval: 1.0
    critRate: 20
    critDamage: 200

Skills: (assign after creating skill SOs)
  autoAttackSkill: <SkillSO>
  activeSkill: <SkillSO>
  passiveSkill: <SkillSO>

Prefab:
  characterPrefab: <Prefab from Step 4>
```

### Step 6: Wire Animation Events to Scripts

In MeleePlayer script, expose handlers:

```csharp
public class MeleePlayer : PlayerBase {
    [SerializeField] MeleeCombat meleeCombat;

    // Animation events — Unity Animator gọi qua SendMessage tới root GameObject
    public void OnAttackStart() => meleeCombat.OnAttackStart();
    public void OnHitboxEnable() => meleeCombat.OnHitboxEnable();
    public void OnHitboxDisable() => meleeCombat.OnHitboxDisable();
    public void OnComboWindowOpen() => meleeCombat.OnComboWindowOpen();
    public void OnAttackEnd() => meleeCombat.OnAttackEnd();
    public void OnSkillImpact() {
        // Trigger skill impact effect
    }
}
```

### Step 7: Test in Editor

1. Drag character prefab vào GameplayScene
2. Set Tag = Player
3. Add CameraFollow component to Main Camera, drag character vào target
4. Play scene
5. Verify:
   - Joystick di chuyển character
   - Animation Idle → Run khi move
   - Auto-attack triggers gần enemy
   - Hitbox damage enemy đúng frame
   - Skill button → skill animation play

---

## ⚠️ COMMON ISSUES

| Issue | Fix |
|---|---|
| Animation events không fire | Function name match exact (case-sensitive). PlayerBase phải có method public. |
| Character không di chuyển | CharacterController component? CharacterLocomotion enabled? |
| Character rotates erratically | `ExternalRotationControl = true` trong PlayerBase.Awake()? |
| Hitbox không damage | Hitbox layer set "PlayerHitbox"? Layer Collision Matrix has PlayerHitbox vs Enemy? |
| Skill animation không play | Trigger name match Animator parameter? Transition condition correct? |
| Auto-attack spam (no cooldown) | `attackInterval` trong CharacterDataSO > 0? |

---

## 🎨 PROPORTION GUIDELINES

For top-down camera (45-50° angle), character size matters:
- **Height:** 1.7-1.9m (humanoid scale)
- **Radius (CharacterController):** 0.35-0.45m
- **Camera offset:** (0, 12, -8) for ~45° view

If character looks too small/large in screen:
- Adjust camera offset (zoom in/out)
- Or scale character up to 1.2x (max — beyond this, looks weird)

---

## 📐 STATS BALANCING GUIDE

**Vertical slice goal:** 2 chars feel distinct, both viable for fighting wave + boss.

### Melee Profile
- High HP (front-line tank)
- Medium ATK (hits frequent)
- Low Range (must close gap)
- Fast Attack Interval
- Med-High Crit Rate

### Ranged Profile
- Low HP (squishy)
- High ATK (less frequent hits)
- High Range (kite)
- Slow Attack Interval
- High Crit Rate + Crit Damage

### Glass Cannon Profile
- Very Low HP
- Very High ATK
- Med Range
- Med Interval
- Highest Crit

### Tank Profile
- Very High HP
- Low ATK
- Low Range
- Slow Interval
- Low Crit

---

## ✅ CHECKLIST: Character Setup Complete

- [ ] FBX imported with Humanoid rig
- [ ] Animator Controller with all states + transitions
- [ ] Animation events on Attack clips
- [ ] Player prefab with all components
- [ ] Tag = Player, Layer = Player
- [ ] Hitbox child object (melee only)
- [ ] MuzzlePoint child object
- [ ] CharacterDataSO asset filled
- [ ] Skills wired in CharacterDataSO
- [ ] Tested in editor — moves, attacks, takes damage

---

*Use this template để add character mới sau Sprint 1.*
