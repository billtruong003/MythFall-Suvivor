# 💥 COMBAT FEEL GUIDE

> **Mục đích:** Polish techniques để combat "juicy" và satisfying.
> **Đọc khi:** Sprint 2 và 4 — combat polish phases.

---

## 🎯 THE 7 LAYERS OF COMBAT FEEL

```
1. Animation        — Visible motion of attacker
2. Hitbox           — Physical impact registration
3. Damage Number    — Numerical feedback
4. Material Flash   — Color reaction on victim
5. Particle VFX     — World response
6. Audio            — Sonic feedback
7. Camera           — Screen-level reaction
```

**Bad combat:** Layer 1+2 only (animation + damage tick)
**Good combat:** All 7 layers, properly orchestrated

---

## 1️⃣ ANIMATION

### Hitbox Timing trong Animation
Animation có 4 phases với timing rõ ràng:

```
Frame:    1-----6======10----14====18
Phase:    Wind  Active  Recov Combo End
Hitbox:   Off   ON      Off   Off   Off
Move:     Slow  Locked  Slow  Free  Free
```

**Critical:** Hitbox chỉ active trong 4 frames middle. Trước đó là windup, sau đó là recovery.

### Animation Events Setup

Trong Unity Editor:
1. Open animation clip (e.g., `Kai_Attack_1.anim`)
2. Click Animation tab
3. Add Animation Event tại frame:
   - Frame 1: `OnAttackStart()`
   - Frame 6: `OnHitboxEnable()`
   - Frame 10: `OnHitboxDisable()`
   - Frame 14: `OnComboWindowOpen()`
   - Frame 18: `OnAttackEnd()`

Function names PHẢI match exact với public methods trong PlayerBase/MeleeCombat.

### Animation Curves

**Recommended easing:**
- Windup: ease-out (slow start, fast end)
- Active hit: linear (consistent speed)
- Recovery: ease-in (slowing to neutral)

---

## 2️⃣ HITBOX

### Hitbox Shapes

| Type | Shape | Use Case |
|---|---|---|
| Sphere | Radius from char center | Wide melee swings |
| Box | Forward arc | Sword stab, thrust |
| Cone | Forward 60-120° | Standard melee |
| Capsule | Long thin | Spear, polearm |

### Hitbox Activation Pattern

```csharp
public class MeleeHitbox : MonoBehaviour {
    [SerializeField] SphereCollider hitbox;
    [SerializeField] float hitboxArcAngle = 120f;
    HashSet<EnemyBase> hitThisSwing = new();

    public void EnableHitbox() {
        hitbox.enabled = true;
        hitThisSwing.Clear(); // reset trên mỗi swing
    }

    public void DisableHitbox() {
        hitbox.enabled = false;
    }

    void OnTriggerEnter(Collider other) {
        if (!hitbox.enabled) return; // safety check
        var enemy = other.GetComponent<EnemyBase>();
        if (enemy == null || hitThisSwing.Contains(enemy)) return;

        // Arc check (front-only attacks)
        var toEnemy = (enemy.transform.position - owner.transform.position).normalized;
        var angle = Vector3.Angle(owner.transform.forward, toEnemy);
        if (angle > hitboxArcAngle / 2f) return;

        ApplyDamage(enemy);
        hitThisSwing.Add(enemy);
    }
}
```

**Common bugs:**
- Hit cùng enemy nhiều lần trong 1 swing → fix bằng `hitThisSwing` HashSet
- Hit enemy phía sau (hit through back) → fix arc check
- Hitbox active sau swing end → ensure DisableHitbox event fires

---

## 3️⃣ DAMAGE NUMBERS

### Style Guidelines

| State | Color | Size | Style |
|---|---|---|---|
| Normal hit | White | 5pt | Regular |
| Critical hit | Yellow | 7pt | Bold + "!" |
| Heal | Green | 5pt | "+" prefix |
| Damage taken | Red | 5pt | Negative for player |
| Big hit (skill) | Orange | 8pt | Bold |

### Animation
```csharp
public class DamageNumber : MonoBehaviour {
    void Update() {
        elapsed += Time.deltaTime;
        float t = elapsed / lifetime;

        // Float up + slight horizontal drift
        var offset = Vector3.up * (t * 1.5f) + Vector3.right * Mathf.Sin(t * 5f) * 0.2f;
        transform.position = startPos + offset;

        // Scale pop (110% → 100%)
        float scale = Mathf.Lerp(1.1f, 1f, t * 3f); // pop in fast 0.3s
        transform.localScale = Vector3.one * scale;

        // Fade out (last 30%)
        if (t > 0.7f) {
            var c = startColor;
            c.a = 1f - ((t - 0.7f) / 0.3f);
            tmp.color = c;
        }

        // Always face camera
        transform.rotation = Camera.main.transform.rotation;

        if (t >= 1f) Bill.Pool.Return(gameObject);
    }
}
```

### Stack Visual
Khi nhiều damage numbers spawn cùng lúc, scatter slightly để không overlap:

```csharp
var spawnPos = e.hitPoint + Vector3.up * 1.5f
    + new Vector3(Random.Range(-0.3f, 0.3f), 0, Random.Range(-0.3f, 0.3f));
```

---

## 4️⃣ MATERIAL FLASH

### Flash on Hit (Enemy)
```csharp
public void Flash() {
    if (activeFlash != null) StopCoroutine(activeFlash);
    activeFlash = StartCoroutine(FlashCoroutine());
}

IEnumerator FlashCoroutine() {
    // Set red
    mpb.SetColor("_BaseColor", Color.red);
    renderer.SetPropertyBlock(mpb);
    yield return new WaitForSeconds(0.1f);

    // Restore white
    mpb.SetColor("_BaseColor", Color.white);
    renderer.SetPropertyBlock(mpb);
}
```

**Pro tip:** Sử dụng `_EmissionColor` thay vì `_BaseColor` để effect more glowy:
```csharp
mpb.SetColor("_EmissionColor", new Color(1, 0, 0) * 2f);
```

### Flash on Crit (Stronger)
```csharp
// Crit flash: white + emission burst
mpb.SetColor("_BaseColor", Color.white);
mpb.SetColor("_EmissionColor", Color.white * 3f);
yield return new WaitForSeconds(0.15f);
// Return to normal
```

### Flash on Death
```csharp
// Death flash: long fade to transparent
for (float t = 0; t < 0.5f; t += Time.deltaTime) {
    mpb.SetFloat("_Alpha", 1f - (t / 0.5f));
    renderer.SetPropertyBlock(mpb);
    yield return null;
}
```

---

## 5️⃣ PARTICLE VFX

### VFX Categories

**Tier 1 — Auto-Attack:**
- Hit spark (8 particles, 0.1s, yellow)
- Splash burst on impact (subtle)

**Tier 2 — Skill:**
- Multi-layer burst (20-30 particles)
- Energy ring expanding
- Trail along motion path

**Tier 3 — Death:**
- Big burst (30-50 particles)
- Death flash
- Maybe ragdoll (later sprint)

**Tier 4 — Boss / Epic:**
- Multiple layers (40-100 particles)
- Slow-mo trigger
- Screen flash overlay
- Ground crack decals

### Particle Performance

**Mobile budget per frame:**
- < 50 active particles total → safe
- 50-150 → monitor FPS
- > 150 → risk frame drop

**Optimization:**
```
- Use GPU instancing (Renderer module)
- Disable Light module (mobile expensive)
- Use simple shaders (Standard Unlit)
- Cull beyond camera frustum
- Pool VFX prefabs aggressively
```

### VFX Spawn Pattern

```csharp
// Layered burst
void SpawnImpactVFX(Vector3 pos) {
    // Layer 1: Instant hit spark
    Bill.Pool.Spawn("VFX_HitSpark", pos);

    // Layer 2: Delayed shockwave (50ms)
    Bill.Timer.Delay(0.05f, () => {
        Bill.Pool.Spawn("VFX_Shockwave", pos);
    });

    // Layer 3: Lingering smoke (100ms)
    Bill.Timer.Delay(0.1f, () => {
        Bill.Pool.Spawn("VFX_Smoke", pos);
    });
}
```

Ba layers tạo cảm giác "impact deeper" thay vì 1 burst đơn lẻ.

---

## 6️⃣ AUDIO

### Audio Stacking

Combat audio có **3 layers:**

**Layer 1 — Action (verb):**
- Sword swing whoosh
- Bow draw + release
- Magic incantation

**Layer 2 — Impact (noun):**
- Flesh impact thud
- Metal clang
- Magic explosion

**Layer 3 — Reaction:**
- Enemy hurt grunt
- Bone crack (heavy hit)
- Fire crackle (DoT)

```csharp
void OnEnemyHit(EnemyHitEvent e) {
    // All 3 layers play near-simultaneously
    Bill.Audio.Play("sfx_kai_swing", e.hitPoint);    // verb (might already played from anim event)
    Bill.Audio.Play("sfx_flesh_impact", e.hitPoint); // noun
    Bill.Audio.Play("sfx_enemy_hurt", e.victim.transform.position); // reaction
}
```

### Pitch Variation

Avoid same SFX repeating identical → fatigue:

```csharp
public void PlayWithVariation(string key, Vector3 pos) {
    var source = Bill.Audio.Play(key, pos);
    source.pitch = Random.Range(0.95f, 1.05f);
}
```

### Audio Priority

Mobile có max 8-16 concurrent AudioSources. Khi exceed:
- Drop oldest non-essential
- Keep player-critical (own SFX)
- Spatial dropout cho audio xa player

```csharp
public class AudioPriority {
    public const int Critical = 100;  // player skill, death
    public const int High = 75;       // player auto-attack hit
    public const int Medium = 50;     // enemy actions near player
    public const int Low = 25;        // ambient enemy sounds
}
```

---

## 7️⃣ CAMERA

### Camera Shake

```csharp
public class CameraShake : MonoBehaviour {
    Vector3 originalPos;
    float intensity, duration, elapsed;

    void OnShake(ScreenShakeEvent e) {
        // Stack: take stronger
        if (e.intensity > intensity) {
            intensity = e.intensity;
            duration = e.duration;
            elapsed = 0;
        }
    }

    void Update() {
        if (duration <= 0) return;
        elapsed += Time.unscaledDeltaTime; // ignore hitstop
        float remaining = 1f - (elapsed / duration);

        if (remaining <= 0) {
            transform.localPosition = originalPos;
            duration = 0;
            return;
        }

        // Decay shake
        var offset = Random.insideUnitSphere * intensity * remaining * 0.3f;
        offset.z = 0; // 2D shake only
        transform.localPosition = originalPos + offset;
    }
}
```

### Shake Intensity Scale

| Event | Intensity | Duration |
|---|---|---|
| Auto-attack non-crit | 0 | - |
| Auto-attack crit | 0.1 | 0.05s |
| Active skill cast | 0.3 | 0.15s |
| Active skill impact | 0.5 | 0.2s |
| Boss attack | 0.6 | 0.3s |
| Boss phase change | 1.0 | 0.5s |
| Player death | 0.8 | 0.4s |

### Hitstop

```csharp
IEnumerator DoHitStop(float duration) {
    Time.timeScale = 0f;
    yield return new WaitForSecondsRealtime(duration);
    Time.timeScale = 1f;
}
```

**Hitstop intensity scale:**
- Auto-attack crit: 50ms
- Active skill hit: 80-120ms
- Boss attack hit: 100ms
- Big skill (ultimate-tier): 150ms

**WARNING:** Hitstop quá long → game feels broken. Max 200ms.

### Camera Punch (FOV)

```csharp
public void PunchFOV(float intensity = 5f, float duration = 0.2f) {
    var cam = Camera.main;
    float orig = cam.fieldOfView;
    cam.fieldOfView -= intensity;
    DOTween.To(() => cam.fieldOfView, x => cam.fieldOfView = x, orig, duration);
}
```

**Use cases:** Active skill cast, boss appearance, heavy crit hit.

---

## 🎬 ORCHESTRATING ALL LAYERS

### Example: Berserker Rush Land Hit

```
T+0ms:    Skill button pressed
T+10ms:   Layer 1 — Animation start (windup)
          Layer 6 — Audio: "WAAAAH" battle cry
          Layer 7 — Camera shake light (0.2 intensity)
T+100ms:  Layer 2 — Hitbox enable, sweep forward
T+150ms:  Layer 2 — Hit enemy
          Layer 3 — Damage number "1500!" yellow
          Layer 4 — Enemy material flash red
          Layer 5 — VFX hit spark + shockwave
          Layer 6 — Audio impact thud
          Layer 7 — Camera shake medium (0.5)
          Layer 7 — Hitstop 80ms
T+250ms:  Layer 5 — Trail VFX continues fading
          Layer 6 — Audio tail fade
T+500ms:  Animation recovery complete
          All layers settled
```

7 layers fired across 500ms — that's the recipe for "juicy".

---

## 🧪 TESTING COMBAT FEEL

### Subjective Test
Cho 5 người chơi 2 phút:

**Questions:**
1. Hits có feel "chắc tay" không? (target: 8+/10)
2. Crit có feel "epic" không? (target: 9+/10)
3. Active skill có feel "powerful" không? (target: 8+/10)
4. Boss attacks có feel "dangerous" không? (target: 8+/10)
5. Combat overall có vui không? (target: 8+/10)

### Objective Test

**FPS measurement during max combat:**
- Spawn 30 enemies + boss
- Use all skills simultaneously
- Measure FPS (should be 30+)

**Memory measurement:**
- Run 10 minutes continuous combat
- Check Bill.Pool stats stable
- Check no GC.Alloc spike per frame

---

## 🎯 PRIORITY MATRIX cho Sprint 4

Nếu thiếu thời gian, polish theo priority:

| Layer | Priority | Effort |
|---|---|---|
| Damage numbers | 🔴 Critical | Low |
| Material flash | 🔴 Critical | Low |
| Hit spark VFX | 🟠 High | Medium |
| Hit audio | 🟠 High | Medium |
| Hitstop on crit | 🟠 High | Low |
| Camera shake | 🟡 Medium | Low |
| Death burst VFX | 🟡 Medium | Medium |
| Death audio | 🟡 Medium | Low |
| Skill VFX | 🟡 Medium | High |
| Skill audio | 🟡 Medium | Medium |
| BGM transitions | 🟢 Low | Medium |
| Camera FOV punch | 🟢 Low | Low |

**Critical layers** — do FIRST, không skip.
**Low priority** — skip nếu cần ship vertical slice nhanh.

---

## ❌ COMMON MISTAKES

1. **Over-shake** — every hit shakes screen → motion sickness
   - Fix: Only crit + skill + boss, không auto-attack

2. **Hitstop everywhere** — every hit freezes → feels janky
   - Fix: Only crit + skill, max 100ms

3. **Audio fatigue** — same SFX without pitch variation
   - Fix: Random pitch ±5%, pool 3-5 variants

4. **VFX disappear instantly** — particles cut off mid-animation
   - Fix: ParticleSystem main → Stop Action: Disable, không Destroy

5. **No anticipation** — skills trigger instantly
   - Fix: Min 100ms windup with audio cue

6. **Damage numbers overlap** — 5 numbers spawn cùng vị trí
   - Fix: Random offset, smart stacking

7. **Crit indistinguishable from normal hit**
   - Fix: Yellow + bold + larger + extra VFX layer

---

## 🎉 FINAL TIPS

**Iterate, không perfect:**
- Implement rough trước
- Playtest 5 lần
- Tweak numbers
- Repeat

**Watch other people play:**
- Họ flinch ở đâu? Đó là feel hit
- Họ smile ở đâu? Đó là feel reward
- Họ frustrate ở đâu? Đó là feedback gap

**Less can be more:**
- 1 polished skill > 5 flat skills
- 1 great audio cue > 10 mediocre cues

---

*End of Combat Feel Guide. Reference khi polish combat.*
