# 🎯 SKILL DESIGN GUIDE

> **Mục đích:** Hướng dẫn Claude Code cách design skill có "feel" — vì user delegate quyền design feel cho Claude.
> **Đọc khi:** Mỗi lần tạo skill mới hoặc tune skill existing.

---

## 🧠 CORE PHILOSOPHY

Skill không chỉ là damage và cooldown. Skill phải có **moment** — một beat satisfying khi trigger.

### 4 Layers của một Good Skill

```
1. ANTICIPATION  → Player sees skill incoming (animation windup, charge VFX)
2. EXECUTION    → Skill lands (damage, knockback, big VFX)
3. IMPACT       → World reacts (screen shake, hitstop, audio peak)
4. RESOLUTION   → Wind down (recovery anim, fade VFX, return to neutral)
```

**Bad skill (flat):** Click → instant damage → done.
**Good skill:** Click → windup glow + audio cue → POW + shake + freeze → particles fade + woosh → ready next.

---

## 🎮 SKILL TYPES

### Type 1: Auto-Attack
**Goal:** Reliable, satisfying repeated action — KHÔNG cần epic mỗi lần.

| Aspect | Recommendation |
|---|---|
| Anim duration | 0.4-0.7s (interruptible) |
| VFX | Subtle hit spark per hit |
| Audio | Quick swing + impact |
| Hitstop | Only on crits (50ms) |
| Screen shake | Tiny (0.1 intensity) on crits |

**Avoid:** Big screen shake / hitstop on auto-attacks → feels exhausting.

### Type 2: Active Skill (Cooldown-based)
**Goal:** Power moment, player feels strong.

| Aspect | Recommendation |
|---|---|
| Anim duration | 0.5-1.5s |
| Anticipation | Charge sound + VFX 0.2-0.5s |
| VFX peak | Big particle burst, beam, shockwave |
| Audio | Distinct cue + impact thud |
| Hitstop | Yes (80-120ms) on hit |
| Screen shake | Medium (0.4-0.6) |
| Cooldown range | 8-25s (rewarding to wait) |

**Pro tip:** Add "slow motion" or "zoom in" 100ms khi skill activates → makes it feel cinematic.

### Type 3: Passive Skill
**Goal:** Reward thoughtful play, không spam visual.

| Aspect | Recommendation |
|---|---|
| Trigger | Threshold-based (HP%, kill count, stack) |
| Visual | Subtle aura tint on character |
| Audio | One-time chime when activate |
| Persistent | Yes — buff stays till condition unmet |

**Avoid:** Loud audio every tick — sẽ annoying. Activate sound 1 lần là đủ.

---

## 🎨 SKILL FEEL CHECKLIST

Khi implement skill, verify từng item:

### Anticipation (Trước khi land)
- [ ] Animation windup visible (player thấy something is coming)
- [ ] Audio cue distinct (player hear skill starting)
- [ ] VFX preview (charge effect, glow, particle gather)
- [ ] Camera nudge (slight zoom in for big skills)

### Execution (Lúc skill land)
- [ ] Big visual moment — particles, beam, shockwave
- [ ] Damage feedback — number popup, enemy material flash
- [ ] Audio peak — thud, impact, shatter
- [ ] Physics reaction — knockback, ragdoll, fly off

### Impact (Reaction từ world)
- [ ] Screen shake (size proportional to skill)
- [ ] Hitstop (brief freeze on hit) — 50-150ms
- [ ] Camera lerp (slight zoom punch)
- [ ] Time slow nếu epic (200ms post-impact)

### Resolution (Sau skill)
- [ ] Wind-down animation (recovery)
- [ ] VFX fade out (không cut abrupt)
- [ ] Audio tail (echo, fade)
- [ ] Cooldown UI updates immediately

---

## 📐 SKILL TUNING NUMBERS

### Damage Scaling

```
Auto-attack:        100% ATK
Light active:       150-200% ATK (CD 5-8s)
Medium active:      250-400% ATK (CD 10-15s)
Heavy active:       500-800% ATK (CD 18-25s)
Ultimate:           1000%+ ATK (CD 30-60s, signature moment)
```

### Cooldown Sweet Spots

```
Always-available:   0s (auto-attack)
Frequent dash:      4-6s (movement skill)
Rotation skill:     8-12s (use every fight)
Big cooldown:       15-25s (key engagement skill)
Ultimate:           30-60s (build-up, payoff)
```

### Animation Timing

```
Auto-attack:        18 frames (~0.5s) — 6-10 hitbox window
Quick active:       18-24 frames (0.5-0.7s)
Medium active:      24-36 frames (0.7-1.0s)
Heavy windup:       36-60 frames (1.0-1.7s) + 8 frames release
```

---

## 🎯 PATTERN LIBRARY

### Pattern 1: "Dash Strike" (Movement + Damage)

```
Phase 1 (0.0-0.1s): Crouch windup, audio "swoosh prep"
Phase 2 (0.1-0.4s): Lunge forward 8m, trail VFX, deal damage on path
Phase 3 (0.4-0.6s): Skid stop, slash animation, big damage
Phase 4 (0.6-0.8s): Recovery
```

**Used for:** Berserker Rush, Shadow Step, Lunge Strike

**Implementation:**
```csharp
public void Tick(float dt) {
    elapsed += dt;
    if (elapsed < 0.1f) {
        // Anticipation — locked in place
    } else if (elapsed < 0.4f) {
        // Movement phase
        controller.Move(transform.forward * dashSpeed * dt);
        DamageEnemiesInPath(0.5f); // small radius
    } else if (elapsed < 0.6f) {
        // Final strike
        if (!finalStruck) {
            DamageEnemiesInRadius(2f, 5f); // big radius, bonus damage
            Bill.Events.Fire(new ScreenShakeEvent { intensity = 0.5f, duration = 0.2f });
            finalStruck = true;
        }
    } else if (elapsed < 0.8f) {
        // Recovery
    } else {
        finished = true;
    }
}
```

### Pattern 2: "Channeled Charge" (Buildup + Release)

```
Phase 1 (0.0-1.0s): Charge — player slow, VFX growing, audio whir
Phase 2 (1.0-1.1s): Release — beam/projectile fires
Phase 3 (1.1-1.3s): Recovery — bow lowered
```

**Used for:** Overcharge Shot, Mana Bomb, Plasma Cannon

**Implementation:**
```csharp
public void Tick(float dt) {
    chargeElapsed += dt;
    if (chargeElapsed < 1.0f) {
        // Visual buildup
        UpdateChargeVFX(chargeElapsed / 1.0f); // 0-1 fill
        // Slow player movement
    } else if (!fired) {
        FireProjectile();
        fired = true;
        Bill.Events.Fire(new ScreenShakeEvent { intensity = 0.4f, duration = 0.15f });
    } else if (chargeElapsed > 1.3f) {
        finished = true;
    }
}
```

### Pattern 3: "Burst AoE" (Instant Field Effect)

```
Phase 1 (0.0-0.2s): Telegraph — ground indicator, audio gather
Phase 2 (0.2-0.3s): Explosion — big VFX burst, damage AoE
Phase 3 (0.3-0.5s): Lingering field (optional debuff)
```

**Used for:** Earthquake Slam, Explosion, Frost Nova

### Pattern 4: "Buff Activation" (Self-Buff)

```
Phase 1 (0.0-0.3s): Pose animation, glow VFX surround player
Phase 2 (0.3-buff_duration): Buff active, persistent aura
Phase 3 (end): Fade VFX, audio "buff end" cue
```

**Used for:** Berserker Mode, Ice Shield, Mana Surge

### Pattern 5: "Summon Pet" (Persistent)

```
Phase 1 (0.0-0.5s): Summoning circle VFX, audio incantation
Phase 2 (0.5s): Pet spawn with materialize VFX
Phase 3 (duration): Pet exists, attacks autonomously
Phase 4 (end): Pet de-spawn with fade
```

**Used for:** Phantom Clone, Spirit Wraith, Battle Drone

---

## 💡 MAKING SKILL "JUICY"

### Universal Juice Techniques

**1. Hitstop Stack:**
```csharp
// On crit
Time.timeScale = 0f;
yield return new WaitForSecondsRealtime(0.08f);
Time.timeScale = 1f;
```

**2. Camera Punch:**
```csharp
// Slight zoom in 100ms then back
camera.fieldOfView -= 5f;
DOTween.To(() => camera.fieldOfView, x => camera.fieldOfView = x,
    originalFOV, 0.2f);
```

**3. Squash & Stretch (procedural anim):**
```csharp
// On impact
target.transform.localScale = new Vector3(1.2f, 0.8f, 1.2f); // squash
DOTween.To(() => target.transform.localScale,
    x => target.transform.localScale = x,
    Vector3.one, 0.15f).SetEase(Ease.OutBounce);
```

**4. VFX Layering:**
```csharp
// Combine 3 effects for big skill
SpawnVFX("BigBurst", pos);          // base explosion
SpawnVFX("DustRing", pos, 0.1f);    // ground dust delayed
SpawnVFX("EnergyShockwave", pos, 0.05f); // ring expanding
```

**5. Audio Stack:**
```csharp
// Multiple SFX layers
PlaySound("hit_thud");          // physical impact
PlaySound("magic_explosion");   // magical layer
PlaySound("low_rumble", delay: 0.05f); // bass tail
```

### Skill-Specific Juice

**Melee skills:** Emphasize PHYSICAL impact
- Heavy thud audio
- Big screen shake
- Hitstop strong (100-150ms)
- Knockback enemies hard
- Material flash white instead of red for power moments

**Ranged skills:** Emphasize PROJECTILE feel
- Trail VFX along projectile path
- Whoosh audio extending
- Small camera nudge on fire
- Enemy pierce satisfaction (multiple damage numbers chain)

**AoE skills:** Emphasize SCALE
- Ground crack decal placeholder
- Multiple particle bursts radiating
- Sound layered (close + distant rumble)
- Camera shake longer (0.4-0.6s)

**Buff skills:** Emphasize TRANSFORMATION
- Color tint on character (red rage, blue ice, gold blessing)
- Aura particles persistent
- Subtle audio loop while active
- UI border glow on character portrait

---

## 🎚️ BALANCE FRAMEWORK

### Damage vs Cooldown Curve

```
DPS contribution = (Damage × Hits) / Cooldown

Auto-attack (1 hit/0.6s, 100% ATK):  166% DPS
Berserker Rush (5 hits×300% ATK / 12s):  125% DPS
Overcharge Shot (1 hit×500% ATK / 15s): 33% DPS  ← need higher

Active skill DPS should be 30-60% of auto-attack DPS
But with utility (knockback, AoE, range) to justify use
```

### Cost-Benefit Matrix

```
                 LOW POWER        HIGH POWER
SHORT CD     |   Often used   |  OP, ban     |
LONG CD      |   Useless      |  Centerpiece |
```

Aim for "Often used" (low CD, decent power) hoặc "Centerpiece" (long CD, big payoff). Avoid "Useless" và "OP".

### Synergy with Upgrade Cards

Mỗi skill nên có ít nhất 2-3 upgrade cards có thể buff nó:

**Berserker Rush synergies:**
- "Bloodlust" (more ATK at low HP) → Rush damage scales
- "Vampiric Touch" (lifesteal) → Rush kills heal more
- "Reaper's Contract" (+100% ATK) → Rush damage doubled

**Overcharge Shot synergies:**
- "Critical Mastery" (+CRIT) → Shot has high crit chance
- "Piercing Edge" (+pierce) → Shot pierce more
- "Chain Lightning" → bounces from beam

---

## ❌ ANTI-PATTERNS

### Don't:

1. **One-button autopilot** — Skill always optimal usage = boring
   - Fix: Add positioning requirement, target type variation

2. **Visual spam** — Too many VFX on auto-attacks
   - Fix: Subtle for repeats, big for special moments

3. **Audio fatigue** — Loud hit sound mỗi attack
   - Fix: Vary pitch, use lighter SFX for auto, big for crits

4. **Cooldown punishment** — Player spam click → annoying error sound
   - Fix: Soft visual feedback (button greyed), no audio

5. **No anticipation** — Skill triggers instantly, feels weightless
   - Fix: Add 0.1-0.3s windup minimum

6. **No reaction** — Damage tick, no feedback
   - Fix: Numbers, flash, particles, sound — at minimum 2 of these

---

## 🎬 SKILL DESIGN WORKFLOW

Khi tạo skill mới, follow checklist:

### Step 1: Define the Fantasy
- "Player feels like ___" (e.g., berserker frenzy, sniper precision, mage power)
- "When skill triggers, player should ___" (be amazed, feel dominant, feel cool)

### Step 2: Choose Pattern
- Dash Strike, Channeled Charge, Burst AoE, Buff Activation, Summon Pet?
- Mix patterns nếu cần (e.g., Channeled + Burst AoE)

### Step 3: Define Phases
- Anticipation: ___ms, animation, audio, VFX
- Execution: ___ms, damage formula, hit pattern
- Impact: screen shake, hitstop, camera punch
- Resolution: ___ms recovery

### Step 4: Implement
- SkillSO with parameters
- SkillExecution with phase logic
- Reference Pattern code template
- Test in isolation first

### Step 5: Polish Pass
- Add VFX placeholders → iterate
- Add audio cues → tune timing
- Add screen shake/hitstop → tune intensity
- Playtest 5+ times for feel

### Step 6: Balance Pass
- Check DPS contribution vs auto-attack
- Check synergy với existing cards
- Check counter-play (can enemies dodge?)
- Adjust numbers based on feel + math

---

## 📋 SKILL DESIGN TEMPLATE

Khi user yêu cầu thêm skill, fill template này trước khi code:

```markdown
## Skill: [Name]

### Fantasy
- Player feels: [emotion]
- Visual: [appearance]
- Sound: [feel]

### Pattern
- Type: [Dash Strike / Channel / Burst AoE / Buff / Summon]
- Total duration: [seconds]

### Phases
| Phase | Time | Action | VFX | Audio |
|---|---|---|---|---|
| Anticipation | 0.0-0.2s | Windup pose | Charge gather | Whir up |
| Execution | 0.2-0.4s | Damage AoE 4m | Big burst | Thunderclap |
| Impact | 0.4s | Hitstop 100ms | Shockwave | Bass impact |
| Resolution | 0.4-0.7s | Recovery | Fade particles | Tail echo |

### Numbers
- Damage: 350% ATK
- Cooldown: 12s
- Range: 4m AoE
- Special: stun 1s

### Synergies
- "Bloodthirst" — Damage scales with ATK upgrades
- "Critical Mastery" — Crit can roll on AoE
- "Chain Lightning" — Bounces from initial hit

### Counter-play
- Telegraph 0.2s → enemies có thể dodge
- Limited range → enemies có thể kite
```

---

## 🎯 FINAL NOTES

**Remember:**
- Players remember **feel** more than numbers
- Better to have 4 amazing skills than 10 flat ones
- Iteration > perfection — implement rough → polish → polish more
- Watch other people play — your fingers learn before your eyes

**For Mythfall vertical slice:**
- 2 active skills (Kai's Rush, Lyra's Overcharge) phải feel amazing
- 2 passives (Bloodlust, MarkedTarget) phải có visible feedback
- 1-2 phút playtest sẽ tell you if combat is fun

---

*End of Skill Design Guide. Reference khi tạo skill mới.*
