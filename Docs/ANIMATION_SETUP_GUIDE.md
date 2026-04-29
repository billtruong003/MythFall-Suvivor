# 🎬 ANIMATION SETUP GUIDE — Mythfall: Survivors

> **Mục đích:** Step-by-step setup AnimatorController + clips + animation events cho Kai (Melee) + Lyra (Ranged) + Swarmer (Enemy).
> Sau khi xong guide này → flip `useTimerFallback = false` → combat chạy 100% animation-event-driven.

---

## 📐 BIG PICTURE — Animation event flow

```
┌─────────────────────────────────────────────────────────────────┐
│                    Frame N (Unity Animator)                      │
│                                                                  │
│  Animator.SetTrigger("Attack_1")                                 │
│       ↓                                                          │
│  State machine: Idle → Attack_1 (transition)                    │
│       ↓                                                          │
│  Animation clip "Kai_Attack_1.anim" plays                        │
│       ↓ (at 30% timeline)                                        │
│  Animation Event fires: function="Trigger", string="OnHitboxEnable" │
│       ↓                                                          │
│  Unity calls DynamicAnimationEventHub.Trigger("OnHitboxEnable")  │
│       ↓                                                          │
│  Hub looks up EventMapping["OnHitboxEnable"] → UnityEvent        │
│       ↓                                                          │
│  UnityEvent invokes wired callback: MeleeCombat.OnHitboxEnable() │
│       ↓                                                          │
│  hitbox.enabled = true; hitThisSwing.Clear();                    │
└─────────────────────────────────────────────────────────────────┘
```

**Why animation-event-driven?**
- **Tight visual sync** — hitbox window matches sword swing animation pixel-for-pixel
- **Designer-friendly** — change hitbox timing = drag event marker in Animation window, no code recompile
- **Works with motion-captured / acquired animations** — events live on the clip, not the controller

---

## 🧩 CONCEPTS — Animator parameters

| Type | Use case | Example |
|---|---|---|
| **Float** | Continuous values, blend trees | `Speed` (0=idle, 1=run) |
| **Bool** | Persistent state | `IsGrounded` (true/false until changed) |
| **Trigger** | One-shot event, auto-resets after consumed | `Attack_1`, `Death` |
| **Int** | Discrete state index | `WeaponType` (0=sword, 1=bow) — not used Sprint 1 |

**Trigger vs Bool difference:**
- Trigger: `SetTrigger("Attack_1")` → consumes on first transition match → resets to false
- Bool: `SetBool("IsGrounded", true)` → stays true until you set false

**Mythfall conventions:**
- Movement Animator state changes via **Float Speed** (Idle↔Run blend)
- Combat actions via **Trigger** (one-shot Attack_1, Skill_*)
- Death via **Trigger** (one-way, never returns)

---

## 📋 PARAMETERS REQUIRED

### PlayerAnimator (Kai + Lyra share controller)

| Name | Type | Purpose |
|---|---|---|
| `Speed` | Float | Drives Idle ↔ Run blend (PlayerBase.Update writes 0-1 normalized) |
| `Attack_1` | Trigger | Auto-attack via PlayerCombatBase.Execute() |
| `Skill_Active_1` | Trigger | Sprint 3 — Kai's BerserkerRush, Lyra unused |
| `Skill_Cast_Long` | Trigger | Sprint 3 — Lyra's OvercharShot, Kai unused |
| `Death` | Trigger | PlayerBase.HandleDeath() |

→ Sprint2Setup đã tạo controller với đủ 5 params. Mày verify trong AnimatorController inspector → Parameters tab.

### EnemyAnimator (Swarmer)

| Name | Type | Purpose |
|---|---|---|
| `Speed` | Float | Idle ↔ Move blend (SwarmerEnemy chase logic writes 0/1) |
| `Attack` | Trigger | SwarmerEnemy.AttackPlayer() |
| `Death` | Trigger | SwarmerEnemy.OnDeath() |

→ Sprint2Setup đã tạo. Verify Parameters tab.

---

## 🎞️ ANIMATION CLIPS REQUIRED

### Kai (Melee — Cô Lang, dual sword)

| Clip name | Duration | Loop? | Description |
|---|---|---|---|
| `Kai_Idle` | 1-2s | ✓ Yes | Standing idle, weapon ready, slight breathing |
| `Kai_Run` | ~0.6s | ✓ Yes | Running animation (full cycle) |
| `Kai_Attack_1` | ~0.6s | ✗ No | Single sword slash, ends pointing forward |
| `Kai_Skill_Active_1` | ~0.7s | ✗ No | Sprint 3 — BerserkerRush dash forward |
| `Kai_Death` | ~1.5s | ✗ No | Fall to ground, freezes at end |

### Lyra (Ranged — Kẻ Phản Đồ, bow)

| Clip name | Duration | Loop? | Description |
|---|---|---|---|
| `Lyra_Idle` | 1-2s | ✓ Yes | Standing, bow lowered |
| `Lyra_Run` | ~0.6s | ✓ Yes | Running |
| `Lyra_Attack_1` | ~1.0s | ✗ No | Draw bow → release arrow |
| `Lyra_Skill_Cast_Long` | ~1.3s | ✗ No | Sprint 3 — Overcharge charge + release |
| `Lyra_Death` | ~1.5s | ✗ No | Fall, bow drops |

### Swarmer (Enemy — Hư Linh Quỷ)

| Clip name | Duration | Loop? | Description |
|---|---|---|---|
| `Swarmer_Idle` | 1-2s | ✓ Yes | Hover/sway idle |
| `Swarmer_Move` | ~0.5s | ✓ Yes | Shamble forward |
| `Swarmer_Attack` | ~0.6s | ✗ No | Lunge bite/claw at player |
| `Swarmer_Death` | ~1s | ✗ No | Dissolve/collapse |

**Đâu lấy clips?**
- **Mixamo (free)** — humanoid characters: chọn rig → drag fbx vào project → Unity tạo .anim, chỉ cần rename
- **Asset Store** — search "RPG Animation Pack" hoặc "Sword Combat Animations"
- **Custom** — Maya/Blender → export FBX với animation
- **Placeholder** — copy free Unity Standard Assets (Ethan model has Idle/Run/Attack)

> Tao recommend Mixamo cho Sprint 1 vì miễn phí + auto rigging + huge library. Lyra cần custom bow anim (Mixamo có "Standing Draw Arrow" + "Standing Aim Recoil" combine được).

---

## 🎯 ANIMATION EVENTS — Timing spec

> Tất cả events call `Trigger(string)` trên `DynamicAnimationEventHub` (root GameObject).
> Trong Animation window, click vào timeline tại frame % chỉ định, click Add Event icon, set Function = `Trigger`, String = event ID dưới đây.

### Kai_Attack_1.anim (~0.6s, 30 frames @ 50fps)

| % | Frame | Event ID (string param) | Wire to (UnityEvent) | Why |
|---|---|---|---|---|
| 5% | 1.5 | `OnAttackStart` | (optional) | Stamina cost, audio start cue |
| **30%** | **9** | **`OnHitboxEnable`** | **`MeleeCombat.OnHitboxEnable`** | **Sword reaches forward arc** |
| **55%** | **16.5** | **`OnHitboxDisable`** | **`MeleeCombat.OnHitboxDisable`** | **Sword returns** |
| 75% | 22.5 | `OnComboWindowOpen` | (optional) | Sprint 3 — combo system |
| 100% | 30 | `OnAttackEnd` | (optional) | Cleanup / reset cooldown |

**Bold = mandatory cho combat loop.** Skip optional ones nếu không cần.

### Kai_Skill_Active_1.anim (Sprint 3 — placeholder events giờ)

| % | Event ID | Wire to | Why |
|---|---|---|---|
| 20% | `OnSkillStart` | `BerserkerRushExecution.OnSkillStart` | Lock locomotion + invincible on |
| 50% | `OnSkillImpact` | `BerserkerRushExecution.OnSkillImpact` | Damage tick along path |
| 100% | `OnSkillEnd` | `BerserkerRushExecution.OnSkillEnd` | Cleanup, unlock locomotion |

### Lyra_Attack_1.anim (~1.0s, 50 frames @ 50fps)

| % | Frame | Event ID | Wire to | Why |
|---|---|---|---|---|
| **50%** | **25** | **`OnArrowRelease`** | **`RangedCombat.OnArrowRelease`** | **Bow string releases — projectile spawn** |

Just one mandatory event. Lyra nhanh.

### Lyra_Skill_Cast_Long.anim (Sprint 3 — placeholder)

| % | Event ID | Wire to | Why |
|---|---|---|---|
| 0% | `OnChargeStart` | `OverchargeExecution.OnChargeStart` | Start charge VFX |
| 80% | `OnBeamRelease` | `OverchargeExecution.OnBeamRelease` | Spawn beam projectile |
| 100% | `OnSkillEnd` | `OverchargeExecution.OnSkillEnd` | Cleanup |

### Swarmer_Attack.anim (~0.6s)

| % | Frame | Event ID | Wire to | Why |
|---|---|---|---|---|
| **50%** | **15** | **`OnAttackHit`** | **`SwarmerEnemy.OnAttackHit`** | **Lunge connects — damage player** |

Just one event. Swarmer simple.

---

## 🛠️ STEP-BY-STEP — Setup Kai AnimatorController

### Step 1 — Import / prepare clips

1. Drag 5 .fbx hoặc .anim files vào `Assets/Mythfall/Animations/Player/Kai/`
   - `Kai_Idle.anim`
   - `Kai_Run.anim`
   - `Kai_Attack_1.anim`
   - `Kai_Skill_Active_1.anim` (Sprint 3 — empty placeholder OK now)
   - `Kai_Death.anim`
2. Click mỗi .anim → Inspector → check `Loop Time`:
   - Idle, Run: **Loop Time = ✓**
   - Attack_1, Skill_Active_1, Death: **Loop Time = ✗**

### Step 2 — Mở AnimatorController

1. Double-click `Assets/Mythfall/Animations/PlayerAnimator.controller`
2. Window mở Animator graph view (nếu chưa có thì Window → Animation → Animator)
3. Verify Parameters tab (góc trái dưới) đã có:
   - Speed (Float)
   - Attack_1, Skill_Active_1, Skill_Cast_Long, Death (Trigger)

### Step 3 — Tạo Blend Tree cho Idle ↔ Run

1. Right-click trong graph → `Create State → From New Blend Tree`
2. Đặt tên: `Locomotion`
3. Right-click `Locomotion` → `Set as Layer Default State` (orange arrow chỉ vào)
4. Double-click `Locomotion` để vào Blend Tree
5. Inspector → Parameter: `Speed`
6. Click `+` → `Add Motion Field` (×2):
   - Field 1: Threshold = `0`, Motion = `Kai_Idle`
   - Field 2: Threshold = `1`, Motion = `Kai_Run`
7. Back arrow để return graph chính

> **Tại sao Blend Tree?** Mượt hơn 2 state Idle/Run với transition. Speed=0.5 thì blend 50/50 — khi đi chậm vẫn natural.

### Step 4 — Tạo state Attack_1

1. Right-click graph → `Create State → Empty`, name = `Attack_1`
2. Inspector → Motion = `Kai_Attack_1`
3. Right-click `Any State` → `Make Transition` → click `Attack_1`
   - Inspector transition:
     - Has Exit Time = ✗ (instant — attack must respond immediately)
     - Settings → Transition Duration = 0.05
     - Conditions: `Attack_1` (Trigger)
4. Right-click `Attack_1` → `Make Transition` → click `Locomotion`
   - Has Exit Time = ✓
   - Exit Time = 0.85 (transition out 85% through clip — feels snappier than 100%)
   - Transition Duration = 0.1
   - No conditions (auto exit)

### Step 5 — Tạo state Skill_Active_1 (Sprint 3, placeholder giờ)

Same pattern as Attack_1, replace name + condition trigger + motion clip.

### Step 6 — Tạo state Death

1. Create state `Death`, motion = `Kai_Death`
2. Any State → Death:
   - Has Exit Time = ✗
   - Transition Duration = 0
   - Conditions: `Death` (Trigger)
3. **No transition out** — Death is terminal. Player respawn = re-instantiate, not unstick from death state.

### Step 7 — Verify final layout

```
[Locomotion (Blend Tree)] ← default
       ↑
[Any State] ─trigger Attack_1→ [Attack_1] ─exit 85%→ [Locomotion]
       │                                                   ↑
       ├─trigger Skill_Active_1→ [Skill_Active_1] ─exit 90%┘
       │
       └─trigger Death→ [Death] (terminal)
```

---

## 🎯 STEP-BY-STEP — Add Animation Events to Kai_Attack_1

1. Click `Kai_Attack_1.anim` trong Project window
2. Window → Animation (nếu chưa mở)
3. Animation window hiển thị timeline của clip
4. Tại timeline phía trên có Add Event icon (bookmark with `+` symbol). Click để add event tại frame hiện tại.
5. Hoặc right-click trên timeline → `Add Animation Event`

**Add 2 mandatory events cho Attack_1:**

**Event 1 — OnHitboxEnable (frame 9 / 30%):**
1. Drag timeline scrubber tới frame 9
2. Add Event
3. Inspector của event:
   - Function: `Trigger`  ← MUST be exactly this (DynamicAnimationEventHub method name)
   - String: `OnHitboxEnable`  ← event ID matches Hub's EventMapping
4. Other fields (Float, Int, Object): leave default

**Event 2 — OnHitboxDisable (frame 16.5 / 55%):**
- Same pattern, String = `OnHitboxDisable`

**Verify:** Event markers (small bookmark icons) hiện trên timeline tại đúng frames.

---

## 🔌 STEP-BY-STEP — Wire DynamicAnimationEventHub

> Đây là bước **kết nối animation event → C# method**. Phải làm cho từng prefab có Hub.

### Kai prefab

1. Project window → Double-click `Kai.prefab` (mở Prefab View)
2. Click root `Kai` → Inspector → tìm `DynamicAnimationEventHub` component
3. Field `Event Mappings` → click `+` 2 lần (add 2 entries)
4. **Entry 1:**
   - Event ID: `OnHitboxEnable`
   - Actions To Trigger: click `+` trong UnityEvent panel
     - Drag root `Kai` GameObject vào field "None (Object)"
     - Function dropdown: `MeleeCombat → OnHitboxEnable()`
5. **Entry 2:**
   - Event ID: `OnHitboxDisable`
   - Actions To Trigger:
     - Drag root `Kai` vào
     - Function: `MeleeCombat → OnHitboxDisable()`
6. Save prefab (Ctrl+S in prefab view, or click ⬅ to exit and apply)

### Lyra prefab

Same flow, 1 entry:
- Event ID: `OnArrowRelease`
- Function: `RangedCombat → OnArrowRelease()`

### Swarmer prefab

Same flow, 1 entry:
- Event ID: `OnAttackHit`
- Function: `SwarmerEnemy → OnAttackHit()`

---

## 🚦 STEP-BY-STEP — Setup Swarmer AnimatorController

Same pattern as Kai but simpler. Mở `Assets/Mythfall/Animations/EnemyAnimator.controller`:

1. **Locomotion Blend Tree:** Speed → Idle (0) + Move (1)
2. **Attack state:** Any State → Attack on Trigger=Attack, exit 85% → Locomotion
3. **Death state:** Any State → Death on Trigger=Death, terminal

Add Animation Event on `Swarmer_Attack.anim`:
- Frame 50% → Trigger("OnAttackHit")

---

## 🧪 TESTING — Verify each piece works

### Test 1 — Locomotion blend
1. Press Play (GameplayScene auto-bounce)
2. In Hierarchy → click Kai → Inspector → Animator
3. Click "Open Animator window" → window highlights current state in graph
4. Move Kai (no joystick yet — in editor, hack: set MobileInputManager.MoveVector via test script OR drop a temporary `[SerializeField] Vector2 testInput;` field on a debug component)
5. Verify Speed parameter in Animator window goes from 0 → 1, blend tree blends Idle → Run smoothly

### Test 2 — Attack trigger
1. Place 1 Swarmer < 1.8m from Kai (e.g. (1, 0, 0))
2. Press Play
3. Console expected: `[BillStartup] ...` then in Animator window watch state machine → see `Attack_1` flash on every attack interval (0.6s for Kai)
4. Animation Event window (open via right-click clip → Show in Inspector) → console log if you put Debug.Log in OnHitboxEnable

### Test 3 — Disable timer fallback
Once events fire reliably:
1. Click Kai prefab → Inspector → MeleeCombat
2. Set `Use Timer Fallback` = ✗ (uncheck)
3. Save prefab
4. Press Play → combat should still work, driven 100% by animation events

If combat breaks when fallback disabled → animation events not firing. Re-check:
- Animation clip has events at correct frames
- Function = `Trigger` (case sensitive)
- String = exact event ID
- Hub on root has matching EventMapping
- UnityEvent target component method is public, void, no params

---

## ⚠️ COMMON ISSUES

| Symptom | Cause | Fix |
|---|---|---|
| Console: `Animation Event has no receiver` | Function name mismatch | Function MUST be `Trigger` (DynamicAnimationEventHub method), not `OnHitboxEnable` directly |
| Hitbox never enables | Event fires but UnityEvent not wired | Inspector → Hub → EventMappings → drag root GO + select MeleeCombat.OnHitboxEnable |
| Hitbox enables twice (re-hits enemies) | Both timer fallback AND anim event fire | Disable `Use Timer Fallback` once anim event verified working |
| Player slides during attack (no anim) | Attack_1 motion empty | Drag `Kai_Attack_1.anim` into state's Motion field |
| Attack stuck repeating | Has Exit Time = ✗ on Attack→Locomotion | Set Has Exit Time = ✓, Exit Time = 0.85 |
| Speed param doesn't blend | Blend Tree thresholds wrong | Field 1: threshold 0; Field 2: threshold 1 (not 0.5/1.5) |
| Death loops | Death state has transition out | Remove all outgoing transitions from Death |
| Animation events fire on prefab in scene but not pool-spawned | Pool doesn't reset Animator state | Add `animator.Rebind()` in EnemyBase.OnSpawn |

---

## 📊 PRIORITY ORDER (Day 2 minimum viable)

Nếu mày time-constrained, làm tối thiểu để combat loop chạy:

1. ✅ Kai_Attack_1 + 2 events (OnHitboxEnable @ 30%, OnHitboxDisable @ 55%)
2. ✅ Lyra_Attack_1 + 1 event (OnArrowRelease @ 50%)
3. ✅ Swarmer_Attack + 1 event (OnAttackHit @ 50%)
4. ✅ Idle + Run blend tree (cả Player + Enemy controller)
5. ✅ DynamicAnimationEventHub mappings trên 3 prefab
6. 🔵 Death animations (visual only, gameplay-wise tự fade qua despawn)
7. 🟡 Sprint 3 events (OnSkillStart/End/Impact) — defer

Sau khi 1-5 xong → flip `useTimerFallback = false` trên 3 component → combat 100% animation-driven, feels tight.

---

## 🚀 AFTER ANIMATION SETUP

1. Test combat ở GameplayScene (Phase B của NEXT_SESSION.md) — giờ có animation visual, không chỉ capsule slide
2. Polish timing nếu cần (drag event marker trên timeline, không cần recompile)
3. Commit `useTimerFallback = false` change → push
4. Day 3 unlock: UI + state flow

---

*Reference khi cần: tao có thể detail thêm bất kỳ step nào (Blend Tree settings, transition curves, Mixamo workflow, etc.). Hỏi specific.*
