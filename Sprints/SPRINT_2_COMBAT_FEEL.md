# ⚔️ SPRINT 2 — Combat Variety + Boss + Polish Layer 1

> **Duration:** 3 ngày | **Output:** Combat feel "juicy", 3+ enemy types, boss fight có phase transition.

---

## 🎯 Sprint Goal

Combat trong Sprint 1 functional nhưng "flat". Sprint 2 thêm **feel** và **variety**:

End of sprint, gameplay có:
- 3 enemy types với behavior phân biệt rõ (Swarmer fast, Brute slow tank, Shooter ranged kite)
- Elite enemy variant (đỏ glow, HP/DMG x3)
- Boss fight (Rotwood) với 2 phase mechanically khác nhau
- **Hitstop** trên crit hits → cảm giác impact
- **Screen shake** trên big hits + boss attacks
- **Damage numbers** floating trên enemy
- **Material flash** đỏ khi enemy bị hit
- **Knockback** trên heavy attacks
- **Environment theme** match Sundered Grove (Thanh Lâm Vực corrupted)

## 📚 PHẢI ĐỌC TRƯỚC KHI BẮT ĐẦU

- ⭐ **`Docs/GAME_DESIGN.md`** section 8.6 (Environment Theme — Chapter 1) — visual direction cho map
- ⭐ **`Docs/COMBAT_FEEL_GUIDE.md`** — polish techniques
- **`Docs/LOCALIZATION_GUIDE.md`** — enemy names dùng keys

## ✅ Prerequisites
- [ ] Sprint 1 done — Player + Swarmer + Scene loop works
- [ ] User prepared: Brute prefab + Shooter prefab + Rotwood prefab (animator hoặc VAT)
- [ ] User prepared: Animation clips cho new enemies (Idle, Move, Attack, Death)
- [ ] User prepared: Environment art cho Sundered Grove (sương xanh, cây thối rữa, fog) — placeholder OK

---

## 📋 TASK BREAKDOWN

### Day 1 — Enemy Variety + AI

#### Task 2.1: Enemy AI State Machine
**File:** `Scripts/Enemy/EnemyAIState.cs`, refactor `EnemyBase.cs`

```csharp
public enum EnemyAIState { Idle, Chase, Attack, Stunned, Dying }

public abstract class EnemyBase : MonoBehaviour {
    protected EnemyAIState currentState = EnemyAIState.Idle;
    protected float stateTimer;

    public virtual void TransitionTo(EnemyAIState newState) {
        OnStateExit(currentState);
        currentState = newState;
        stateTimer = 0f;
        OnStateEnter(newState);
    }

    protected virtual void OnStateEnter(EnemyAIState state) { }
    protected virtual void OnStateExit(EnemyAIState state) { }
    protected abstract void TickState(float dt);
}
```

#### Task 2.2: BruteEnemy
**File:** `Scripts/Enemy/BruteEnemy.cs`

Behavior:
- Slow chase (speed 2)
- High HP (80)
- Big slam attack với telegraph 0.8s
- Knockback player on hit (knockback force 10)
- Plays roar sound on attack

```csharp
public class BruteEnemy : EnemyBase {
    [SerializeField] Animator anim;
    float attackTelegraph = 0.8f;
    float attackDamage; // = data.attackPower * 1.5f

    protected override void TickState(float dt) {
        switch (currentState) {
            case EnemyAIState.Chase: TickChase(dt); break;
            case EnemyAIState.Attack: TickAttack(dt); break;
        }
    }

    void TickChase(float dt) {
        var toPlayer = player.position - transform.position;
        toPlayer.y = 0;
        if (toPlayer.magnitude < 1.5f) {
            TransitionTo(EnemyAIState.Attack);
            return;
        }
        transform.position += toPlayer.normalized * data.moveSpeed * dt;
        transform.rotation = Quaternion.LookRotation(toPlayer.normalized);
    }

    void TickAttack(float dt) {
        stateTimer += dt;
        if (stateTimer < attackTelegraph) {
            // Telegraph — play windup animation
            // Visual: red ground indicator?
        } else if (stateTimer < attackTelegraph + 0.2f) {
            // Hit window
            DealAttackDamage();
        } else if (stateTimer > attackTelegraph + 1f) {
            // Recovery done
            TransitionTo(EnemyAIState.Chase);
        }
    }

    void DealAttackDamage() {
        var dist = Vector3.Distance(transform.position, player.position);
        if (dist < 2f) {
            var ph = player.GetComponent<PlayerHealth>();
            ph?.TakeDamage(attackDamage);
            // Knockback player
            var kb = player.GetComponent<KnockbackReceiver>();
            kb?.ApplyKnockback((player.position - transform.position).normalized * 10f);
        }
    }
}
```

#### Task 2.3: ShooterEnemy
**File:** `Scripts/Enemy/ShooterEnemy.cs`

Behavior:
- Maintain 6-10m từ player (kite)
- Fire projectile mỗi 2s khi trong range + line of sight
- Low HP (40)
- Move backward nếu player < 5m
- Aim ahead of player movement (lead shot)

```csharp
public class ShooterEnemy : EnemyBase {
    [SerializeField] string projectilePoolKey = "Enemy_Projectile";
    [SerializeField] Transform muzzle;
    float fireTimer;
    const float MIN_DIST = 5f;
    const float MAX_DIST = 10f;
    const float FIRE_INTERVAL = 2f;

    protected override void TickState(float dt) {
        if (player == null) return;
        var toPlayer = player.position - transform.position;
        toPlayer.y = 0;
        float dist = toPlayer.magnitude;

        // Movement: maintain 6-10m
        if (dist < MIN_DIST) {
            // Back away
            transform.position -= toPlayer.normalized * data.moveSpeed * dt;
        } else if (dist > MAX_DIST) {
            // Move closer
            transform.position += toPlayer.normalized * data.moveSpeed * dt;
        }

        transform.rotation = Quaternion.LookRotation(toPlayer.normalized);

        // Fire
        fireTimer -= dt;
        if (fireTimer <= 0f && dist <= MAX_DIST && dist >= MIN_DIST - 1f) {
            FireProjectile();
            fireTimer = FIRE_INTERVAL;
        }
    }

    void FireProjectile() {
        var dir = (player.position - muzzle.position).normalized;
        var proj = Bill.Pool.Spawn(projectilePoolKey, muzzle.position,
            Quaternion.LookRotation(dir));
        // Setup projectile damage
    }
}
```

#### Task 2.4: Elite Modifier Component
**File:** `Scripts/Enemy/EliteModifier.cs`

```csharp
[RequireComponent(typeof(EnemyBase))]
public class EliteModifier : MonoBehaviour {
    [SerializeField] float hpMultiplier = 3f;
    [SerializeField] float damageMultiplier = 2f;
    [SerializeField] float scaleMultiplier = 1.3f;
    [SerializeField] Color emissionColor = new Color(1f, 0.2f, 0.2f) * 2f;

    void Awake() {
        // Modify stats — cần expose mutator trong EnemyBase hoặc EnemyDataSO clone
        // Scale up
        transform.localScale *= scaleMultiplier;

        // Emission glow
        var renderer = GetComponentInChildren<Renderer>();
        if (renderer != null) {
            var mpb = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(mpb);
            mpb.SetColor("_EmissionColor", emissionColor);
            renderer.SetPropertyBlock(mpb);
        }

        // Enable emission keyword on material
    }
}
```

WaveSpawner update: 10% chance spawn elite từ wave 5+.

### Day 2 — Boss Fight

#### Task 2.5: BossEnemy với Phase System
**File:** `Scripts/Enemy/BossEnemy.cs`

```csharp
public class BossEnemy : EnemyBase {
    [SerializeField] VAT_Animator vatAnim; // Crossfade-capable for phases
    [SerializeField] Renderer renderer;

    public enum Phase { One, Two, Dead }
    Phase currentPhase = Phase.One;

    string[] phase1Patterns = { "Slam", "RootTrap" };
    string[] phase2Patterns = { "Slam", "Charge", "RootTrap" };
    int patternIndex;
    float attackTimer = 3f;

    public override void OnSpawn() {
        base.OnSpawn();
        currentPhase = Phase.One;
        Bill.Events.Fire(new BossSpawnedEvent { boss = this });
        vatAnim.PlayClip("Idle");
    }

    void Update() {
        if (currentPhase == Phase.Dead || player == null) return;

        // Phase transition
        if (currentPhase == Phase.One && currentHP < data.maxHP * 0.5f) {
            EnterPhase2();
        }

        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f) {
            ExecuteAttack();
            attackTimer = (currentPhase == Phase.Two) ? 1.8f : 3f;
        }

        // Slow chase
        var toPlayer = player.position - transform.position;
        toPlayer.y = 0;
        if (toPlayer.magnitude > 4f) {
            transform.position += toPlayer.normalized * data.moveSpeed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(toPlayer.normalized);
        }
    }

    void ExecuteAttack() {
        var patterns = (currentPhase == Phase.One) ? phase1Patterns : phase2Patterns;
        string pattern = patterns[patternIndex % patterns.Length];
        patternIndex++;

        switch (pattern) {
            case "Slam": PerformSlam(); break;
            case "Charge": PerformCharge(); break;
            case "RootTrap": PerformRootTrap(); break;
        }
    }

    void PerformSlam() {
        vatAnim.CrossFadeToClip("Slam", 0.2f);
        // Telegraph — show ground indicator
        Bill.Timer.Delay(0.6f, () => {
            // Damage AoE 3m
            var hits = Physics.OverlapSphere(transform.position, 3f,
                LayerMask.GetMask("Player"));
            foreach (var h in hits) {
                h.GetComponent<PlayerHealth>()?.TakeDamage(data.attackPower * 2f);
            }
            Bill.Events.Fire(new ScreenShakeEvent {
                intensity = 0.6f, duration = 0.3f
            });
        });
    }

    void PerformCharge() {
        // Phase 2 only — lao thẳng vào player
        vatAnim.CrossFadeToClip("Charge", 0.2f);
        // Implement charge logic over 1.5s
    }

    void PerformRootTrap() {
        // Spawn root trap at player position
        // Player bị slow 50% nếu đứng trong 3s
    }

    void EnterPhase2() {
        currentPhase = Phase.Two;
        Bill.Events.Fire(new BossPhaseChangedEvent { boss = this, newPhase = 2 });

        // Visual: red glow
        var mpb = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(mpb);
        mpb.SetColor("_EmissionColor", new Color(1f, 0.3f, 0.3f) * 2f);
        renderer.SetPropertyBlock(mpb);

        // Roar
        vatAnim.CrossFadeToClip("Roar", 0.3f);
        Bill.Events.Fire(new ScreenShakeEvent { intensity = 1f, duration = 0.5f });
    }

    protected override void Die(PlayerBase killer) {
        currentPhase = Phase.Dead;
        base.Die(killer);
        // Trigger victory after delay
        Bill.Timer.Delay(2f, () => Bill.State.GoTo<VictoryState>());
    }

    protected override void OnDeath() {
        vatAnim.CrossFadeToClip("Death", 0.3f);
    }
}
```

**Boss spawn trigger:** Trong WaveSpawner, sau 60s gameplay → stop spawning swarmers, spawn boss.

#### Task 2.6: VictoryState + VictoryPanel
Tương tự DefeatState nhưng "You Win!" + show stats (time survived, kill count).

### Day 3 — Polish Layer 1

#### Task 2.7: HitStop System
**File:** `Scripts/Polish/HitStopController.cs`

```csharp
public class HitStopController : MonoBehaviour {
    void Awake() {
        Bill.Events.Subscribe<EnemyHitEvent>(OnEnemyHit);
    }

    void OnEnemyHit(EnemyHitEvent e) {
        if (e.isCrit) {
            StartCoroutine(DoHitStop(0.05f)); // 50ms freeze
        }
    }

    IEnumerator DoHitStop(float duration) {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }

    void OnDestroy() => Bill.Events.Unsubscribe<EnemyHitEvent>(OnEnemyHit);
}
```

Place script ở GameplayScene root GameObject.

#### Task 2.8: Screen Shake System
**File:** `Scripts/Polish/CameraShake.cs`

Attach to MainCamera trong GameplayScene.

```csharp
public class CameraShake : MonoBehaviour {
    Vector3 originalPos;
    float currentIntensity;
    float currentDuration;
    float elapsed;

    void Start() {
        originalPos = transform.localPosition;
        Bill.Events.Subscribe<ScreenShakeEvent>(OnShake);
    }

    void OnShake(ScreenShakeEvent e) {
        currentIntensity = Mathf.Max(currentIntensity, e.intensity);
        currentDuration = Mathf.Max(currentDuration, e.duration);
        elapsed = 0f;
    }

    void Update() {
        if (currentDuration <= 0f) return;
        elapsed += Time.deltaTime;
        float remaining = 1f - (elapsed / currentDuration);

        if (remaining <= 0f) {
            transform.localPosition = originalPos;
            currentDuration = 0f;
            return;
        }

        var randomOffset = Random.insideUnitSphere * currentIntensity * remaining * 0.3f;
        transform.localPosition = originalPos + randomOffset;
    }
}
```

Crit hits → fire shake event với intensity 0.3f.
Boss attacks → intensity 0.6-1.0f.

#### Task 2.9: Damage Number Floating
**File:** `Scripts/Polish/DamageNumberSpawner.cs`, `DamageNumber.cs`

```csharp
// Damage number prefab: TextMeshPro 3D + Animator
public class DamageNumber : MonoBehaviour {
    [SerializeField] TextMeshPro tmp;
    Vector3 startPos;
    float lifetime = 0.8f;
    float elapsed;
    Color startColor;

    public void Setup(float damage, bool isCrit) {
        tmp.text = isCrit ? $"<b>{damage:0}!</b>" : damage.ToString("0");
        tmp.color = isCrit ? Color.yellow : Color.white;
        tmp.fontSize = isCrit ? 8f : 6f;
        startPos = transform.position;
        startColor = tmp.color;
        elapsed = 0f;
    }

    void Update() {
        elapsed += Time.deltaTime;
        float t = elapsed / lifetime;

        // Float up
        transform.position = startPos + Vector3.up * (t * 1.5f);
        // Face camera (billboard)
        transform.rotation = Camera.main.transform.rotation;

        // Fade out
        var c = startColor;
        c.a = 1f - t;
        tmp.color = c;

        if (t >= 1f) Bill.Pool.Return(gameObject);
    }
}

public class DamageNumberSpawner : MonoBehaviour {
    void Awake() => Bill.Events.Subscribe<EnemyHitEvent>(OnHit);
    void OnHit(EnemyHitEvent e) {
        var dn = Bill.Pool.Spawn("VFX_DamageNumber",
            e.hitPoint + Vector3.up * 1.5f, Quaternion.identity);
        dn.GetComponent<DamageNumber>().Setup(e.damage, e.isCrit);
    }
}
```

#### Task 2.10: Material Flash on Hit
**File:** `Scripts/Polish/HitFlashController.cs` (component on enemy)

```csharp
public class HitFlashController : MonoBehaviour {
    [SerializeField] Renderer targetRenderer;
    MaterialPropertyBlock mpb;
    Color originalColor;
    Coroutine activeFlash;

    void Awake() {
        mpb = new MaterialPropertyBlock();
        targetRenderer.GetPropertyBlock(mpb);
    }

    public void Flash() {
        if (activeFlash != null) StopCoroutine(activeFlash);
        activeFlash = StartCoroutine(FlashCoroutine());
    }

    IEnumerator FlashCoroutine() {
        targetRenderer.GetPropertyBlock(mpb);
        mpb.SetColor("_BaseColor", Color.red);
        targetRenderer.SetPropertyBlock(mpb);

        yield return new WaitForSeconds(0.1f);

        mpb.SetColor("_BaseColor", Color.white);
        targetRenderer.SetPropertyBlock(mpb);
    }
}

// Call from EnemyBase.TakeDamage():
GetComponent<HitFlashController>()?.Flash();
```

#### Task 2.11: Knockback System
**File:** `Scripts/Polish/KnockbackReceiver.cs`

```csharp
[RequireComponent(typeof(CharacterController))]
public class KnockbackReceiver : MonoBehaviour {
    CharacterController controller;
    Vector3 knockbackVelocity;
    float decayRate = 5f;

    void Awake() => controller = GetComponent<CharacterController>();

    public void ApplyKnockback(Vector3 force) {
        knockbackVelocity = force;
    }

    void Update() {
        if (knockbackVelocity.sqrMagnitude < 0.01f) return;
        controller.Move(knockbackVelocity * Time.deltaTime);
        knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero,
            decayRate * Time.deltaTime);
    }
}
```

Attach to Player + Enemy.

#### Task 2.12: Boss Spawn Trigger Update WaveSpawner

```csharp
public class WaveSpawner : MonoBehaviour {
    // Existing fields...
    [SerializeField] float bossSpawnTime = 60f;
    [SerializeField] string bossPoolKey = "Enemy_Rotwood";
    [SerializeField] Transform bossSpawnPoint;
    bool bossSpawned;

    void Start() {
        Bill.Timer.Repeat(waveInterval, SpawnWave);
        Bill.Timer.Delay(bossSpawnTime, SpawnBoss);
    }

    void SpawnBoss() {
        if (bossSpawned) return;
        bossSpawned = true;
        // Stop wave spawning... or keep going for chaos
        var boss = Bill.Pool.Spawn(bossPoolKey, bossSpawnPoint.position,
            Quaternion.identity);
        boss.GetComponent<BossEnemy>().OnSpawn();
    }
}
```

---

## ✅ DEFINITION OF DONE

### Functional
- [ ] 3 enemy types (Swarmer, Brute, Shooter) spawn trong wave rotation
- [ ] Elite enemies (10% chance) larger + red glow
- [ ] Boss spawn sau 60s
- [ ] Boss có 2 phase, transition visible (color change + roar)
- [ ] Boss death → VictoryState

### Polish
- [ ] Hitstop trên crit hits feel impactful
- [ ] Screen shake trên big hits + boss attacks
- [ ] Damage numbers float up + fade
- [ ] Crit numbers larger + yellow color
- [ ] Enemy material flash đỏ khi bị hit
- [ ] Knockback player khi bị heavy attack

### Technical
- [ ] FPS ≥ 30 với 50 enemies on screen
- [ ] No null ref khi enemy die mid-attack
- [ ] Pool stats stable

---

## 🧪 TEST CHECKLIST (Sprint 2 v0.3)

### Enemy Behavior
- [ ] Swarmer chase và attack giống Sprint 1
- [ ] Brute approach slowly, telegraph attack 0.8s, AoE damage on slam
- [ ] Shooter maintain 6-10m, kite when player approaches
- [ ] Shooter projectile flies straight to player position
- [ ] Elite enemy visually distinct (đỏ glow, lớn hơn)

### Boss Fight
- [ ] Boss spawns sau 60s, big size, intro animation
- [ ] Phase 1: Slam + RootTrap rotation
- [ ] Boss HP 50% → Phase 2 transition (visual + audio cue)
- [ ] Phase 2: Slam + Charge + RootTrap rotation
- [ ] Boss attacks have telegraph (player có thể dodge)
- [ ] Boss death → VictoryState → Victory panel

### Polish — Hitstop
- [ ] Crit hits có 50ms freeze visible
- [ ] Non-crit hits không hitstop
- [ ] Hitstop không break gameplay (UI vẫn responsive)

### Polish — Screen Shake
- [ ] Crit hits → light shake (intensity 0.3)
- [ ] Boss slam → medium shake (intensity 0.6)
- [ ] Boss phase transition → heavy shake (intensity 1.0)
- [ ] Shake không cause motion sickness (max 0.5s duration)

### Polish — Damage Numbers
- [ ] Numbers spawn at hit point
- [ ] Float up over 0.8s
- [ ] Crit numbers yellow + larger + bold
- [ ] Normal numbers white
- [ ] Numbers face camera (billboard)
- [ ] Pool stats stable (no leak)

### Polish — Material Flash
- [ ] Enemy hit → red flash 0.1s
- [ ] Multiple hits in quick succession không break flash
- [ ] Boss flash visible cũng

### Polish — Knockback
- [ ] Brute slam → player knocked back
- [ ] Brute slam → Brute itself doesn't move
- [ ] Knockback doesn't push through walls

### Performance
- [ ] FPS ≥ 30 với boss + 30 enemies
- [ ] APK size < 40 MB
- [ ] No crash trong 5 phút playtest with boss

---

## ⚠️ COMMON ISSUES

| Issue | Solution |
|---|---|
| Hitstop frozen permanently | Coroutine WaitForSecondsRealtime, check OnDestroy unsub |
| Damage numbers không hiện | TextMeshPro material + sorting layer correct? |
| Material flash strobe rapid | Stop previous coroutine before new flash |
| Boss phase không transition | Check `currentHP < data.maxHP * 0.5f` not `<=` |
| Knockback launch player vào trời | CharacterController.Move only, không thêm rigidbody |

---

## 🎬 NEXT — Sprint 3

Sau Sprint 2 done, tiếp Sprint 3: **Skills + In-Run Progression**.

Đọc `Sprints/SPRINT_3_SKILLS_PROGRESSION.md`.
