# 🔌 BILLGAMECORE API REFERENCE

> **Mục đích:** Quick lookup cho Bill.X services. Đọc khi cần API chi tiết.

---

## 📦 SERVICE OVERVIEW

| Service | Access | Purpose |
|---|---|---|
| `Bill.State` | GameStateMachine | State transitions |
| `Bill.Events` | IEventBus | Pub/sub events |
| `Bill.Pool` | IPoolService | Object pooling |
| `Bill.UI` | IUIService | Panel management |
| `Bill.Save` | ISaveService | Persistent storage |
| `Bill.Audio` | IAudioService | SFX + BGM |
| `Bill.Timer` | ITimerService | Delays + repeating |
| `Bill.Scene` | ISceneService | Scene transitions |
| `Bill.Config` | IConfigService | Remote config |
| `Bill.Tween` | ITweenService | Easing animations |
| `Bill.Net` | INetworkService | Networking |
| `Bill.Cheat` | CheatConsole | Dev console |
| `Bill.Debug` | DebugOverlay | FPS/stats overlay |
| `Bill.Trace` | (static class) | Diagnostics |

---

## ⚡ Bill.IsReady

Check framework đã initialize chưa:

```csharp
void Start() {
    if (Bill.IsReady) {
        Initialize();
    } else {
        Bill.Events.SubscribeOnce<GameReadyEvent>(_ => Initialize());
    }
}
```

**Always check** trước khi dùng services nếu code chạy trước Bootstrap done.

---

## 🎯 Bill.State (GameStateMachine)

### Define State

```csharp
public class InRunState : GameState {
    public override void Enter() {
        Debug.Log("Entering InRunState");
        Bill.UI.Open<HudPanel>();
    }

    public override void Tick(float dt) {
        // Optional update logic
    }

    public override void Exit() {
        Bill.UI.Close<HudPanel>();
    }
}
```

### Register & Transition

```csharp
// Register (in GameBootstrap)
Bill.State.AddState<MainMenuState>();
Bill.State.AddState<InRunState>();
Bill.State.AddState<DefeatState>();

// Transition
Bill.State.GoTo<InRunState>();

// Check current state
if (Bill.State.IsCurrentState<InRunState>()) { ... }

// Get state history (for debugging)
var history = Bill.State.GetHistoryLog();
```

---

## 📢 Bill.Events (IEventBus)

### Define Event Struct

```csharp
public struct EnemyHitEvent : IEvent {
    public PlayerBase attacker;
    public EnemyBase victim;
    public float damage;
    public bool isCrit;
    public Vector3 hitPoint;
}
```

**LƯU Ý:** Use `struct` (not class) cho events — value type, no GC alloc.

### Fire Event

```csharp
Bill.Events.Fire(new EnemyHitEvent {
    attacker = this,
    victim = enemy,
    damage = 50,
    isCrit = true,
    hitPoint = enemy.transform.position
});
```

### Subscribe / Unsubscribe

```csharp
void OnEnable() {
    Bill.Events.Subscribe<EnemyHitEvent>(OnEnemyHit);
}

void OnDisable() {
    Bill.Events.Unsubscribe<EnemyHitEvent>(OnEnemyHit);
}

void OnEnemyHit(EnemyHitEvent e) {
    Debug.Log($"Hit {e.victim.name} for {e.damage}");
}
```

**ALWAYS** unsubscribe! Memory leak nếu subscribe và GameObject destroyed without unsub.

### Subscribe Once

```csharp
// Auto-unsubscribe sau lần fire đầu tiên
Bill.Events.SubscribeOnce<GameReadyEvent>(_ => {
    Debug.Log("Game ready!");
});
```

Useful cho one-time bootstrap events.

---

## 🏊 Bill.Pool (IPoolService)

### Register Pool

```csharp
// In GameBootstrap.RegisterPools()
Bill.Pool.Register("Enemy_Swarmer",
    Resources.Load<GameObject>("Prefabs/Enemies/Swarmer"),
    warmCount: 30); // Pre-instantiate 30 instances
```

**warmCount** = số instances pre-create để tránh hitching khi spawn first time.

### Spawn

```csharp
// Basic spawn
GameObject enemy = Bill.Pool.Spawn("Enemy_Swarmer",
    position, Quaternion.identity);

// Generic spawn (returns component)
Projectile proj = Bill.Pool.Spawn<Projectile>("Projectile_Arrow",
    position, rotation);

// Spawn parent
GameObject pickup = Bill.Pool.Spawn("XP_Gem", pos, rot, parent);
```

### Return

```csharp
// Immediate return
Bill.Pool.Return(gameObject);

// Delayed return (for death animations, fade-outs)
Bill.Pool.Return(gameObject, delay: 1f);
```

### Stats (debugging)

```csharp
var stats = Bill.Pool.GetStats("Enemy_Swarmer");
Debug.Log($"Pool: active={stats.activeCount}, total={stats.totalCount}");
```

Use để detect leaks — activeCount tăng dần = leak.

---

## 🎨 Bill.UI (IUIService)

### Define Panel

```csharp
public class HudPanel : BasePanel {
    UIDocument document;

    public override void OnOpen() {
        // Setup UXML reference
        Bill.Events.Subscribe<PlayerDamagedEvent>(OnPlayerDamaged);
    }

    public override void OnClose() {
        Bill.Events.Unsubscribe<PlayerDamagedEvent>(OnPlayerDamaged);
    }

    void OnPlayerDamaged(PlayerDamagedEvent e) {
        // Update HP bar
    }
}
```

### Open / Close

```csharp
// Simple open
Bill.UI.Open<HudPanel>();

// Open with setup callback
Bill.UI.Open<UpgradePanel>(panel => {
    panel.SetCards(threeCards);
    panel.OnCardSelected += OnCardSelected;
});

// Close
Bill.UI.Close<HudPanel>();

// Toggle
Bill.UI.Toggle<PausePanel>();

// Check open
if (Bill.UI.IsOpen<PausePanel>()) { ... }
```

---

## 💾 Bill.Save (ISaveService)

### Save Data

```csharp
[System.Serializable]
public class PlayerData {
    public int gold;
    public List<string> ownedCharacters;
}

// Set
var data = new PlayerData { gold = 100 };
Bill.Save.Set("player", data);

// Force write to disk
Bill.Save.Flush();

// Auto-save typically every X seconds, but flush on critical changes
```

### Load Data

```csharp
// Get with type
var data = Bill.Save.Get<PlayerData>("player");
if (data == null) data = new PlayerData(); // first time

// Or with default
var data = Bill.Save.Get<PlayerData>("player", new PlayerData());
```

### Delete

```csharp
Bill.Save.Delete("player"); // remove key
Bill.Save.Clear();          // clear all
```

---

## 🔊 Bill.Audio (IAudioService)

### Play SFX

```csharp
// 2D SFX (no spatial)
Bill.Audio.Play("sfx_button_click");

// 3D SFX with position
Bill.Audio.Play("sfx_enemy_hit", enemy.transform.position);

// With volume override
Bill.Audio.Play("sfx_swing", volume: 0.7f);
```

### Play Music

```csharp
// Crossfade to new BGM
Bill.Audio.PlayMusic("bgm_combat", fadeDuration: 1.5f);

// Stop music
Bill.Audio.StopMusic(fadeDuration: 1f);
```

### Volume Control

```csharp
Bill.Audio.SetVolume(AudioChannel.Master, 1.0f);
Bill.Audio.SetVolume(AudioChannel.Music, 0.6f);
Bill.Audio.SetVolume(AudioChannel.SFX, 1.0f);

float vol = Bill.Audio.GetVolume(AudioChannel.Music);
```

### Audio Loading Convention

Place audio files trong:
```
Assets/Mythfall/Resources/Audio/SFX/sfx_button_click.ogg
Assets/Mythfall/Resources/Audio/Music/bgm_combat.ogg
```

Bill.Audio auto-load qua key matching filename.

---

## ⏰ Bill.Timer (ITimerService)

### Delay

```csharp
// Run after 2s
Bill.Timer.Delay(2f, () => {
    Debug.Log("2 seconds passed");
});

// Get cancellable handle
var handle = Bill.Timer.Delay(5f, OnTimeout);
handle.Cancel(); // Cancel before fire
```

### Repeating

```csharp
// Run every 1s
var handle = Bill.Timer.Repeat(1f, OnTick);

// Stop repeating
handle.Cancel();
```

### Conditional / Time-based

```csharp
// Wait until condition
Bill.Timer.WaitUntil(() => player.HP > 50, () => {
    Debug.Log("Player healed!");
});
```

**LƯU Ý:** Bill.Timer respects Time.timeScale. For unscaled time (e.g., during pause), use `Bill.Timer.UnscaledDelay()`.

---

## 🎬 Bill.Scene (ISceneService)

### Load Scene

```csharp
// Simple load
Bill.Scene.Load("GameplayScene");

// With transition
Bill.Scene.Load("GameplayScene", TransitionType.Fade, duration: 0.5f);

// Async with callback
Bill.Scene.LoadAsync("GameplayScene",
    progress => Debug.Log($"Loading {progress * 100}%"),
    () => Debug.Log("Loaded!"));
```

### Available Transitions

```csharp
public enum TransitionType {
    None,    // instant
    Fade,    // black fade
    Slide,   // sliding panel
    Wipe     // wipe effect
}
```

---

## ⚙️ Bill.Config (IConfigService)

### Read Config Values

```csharp
// With default
float swarmerHP = Bill.Config.GetFloat("enemy.swarmer.hp", 30f);
int waveCount = Bill.Config.GetInt("game.waves_per_run", 10);
string bossName = Bill.Config.GetString("boss.ch1.name", "Rotwood");
bool feature = Bill.Config.GetBool("feature.gacha_enabled", false);
```

Useful cho remote balance tuning (Firebase Remote Config).

---

## 🎢 Bill.Tween (ITweenService)

### Basic Tween

```csharp
// Move from A to B
Bill.Tween.Move(transform, targetPos, duration: 1f, Ease.OutQuad);

// Scale
Bill.Tween.Scale(transform, Vector3.one * 1.5f, 0.5f);

// Custom value
Bill.Tween.To(0f, 100f, 1f, value => {
    progressBar.fillAmount = value / 100f;
});
```

### Easing Types

```csharp
Ease.Linear
Ease.InQuad / OutQuad / InOutQuad
Ease.InCubic / OutCubic / InOutCubic
Ease.OutBounce / OutElastic
// ...
```

---

## 🌐 Bill.Net (INetworkService)

Sprint 4 không cần dùng. Reference cho Sprint sau (LiveOps).

```csharp
Bill.Net.Get<UserData>("https://api.com/user", OnReceived);
Bill.Net.Post("https://api.com/save", payload, OnSaved);
```

---

## 🎮 Bill.Cheat (Dev Only)

```csharp
#if DEVELOPMENT_BUILD || UNITY_EDITOR
Bill.Cheat.Register("godmode", () => {
    var player = FindObjectOfType<PlayerHealth>();
    player?.SetInvincible(true, 9999f);
}, "Player invincible");

Bill.Cheat.Register("addxp", (int amount) => {
    Bill.Events.Fire(new XPCollectedEvent { amount = amount });
}, "Add XP");

Bill.Cheat.Register("killall", () => {
    foreach (var e in FindObjectsOfType<EnemyBase>()) {
        e.TakeDamage(99999, null);
    }
}, "Kill all enemies");
#endif
```

Console mở qua keyboard shortcut (~ key default) trong dev build.

---

## 📊 Bill.Debug (Dev Only)

```csharp
Bill.Debug.SetEnabled(true);
// Shows FPS, draw calls, memory, pool stats overlay
```

---

## 🔍 Bill.Trace (Diagnostics)

```csharp
// Print full dependency graph
Bill.Trace.Print();

// Recent service access log
Bill.Trace.Log(40);

// Health check (any service failing?)
Bill.Trace.HealthCheck();

// Find dead/unused services
Bill.Trace.Unused();

// Toggle tracing
Bill.Trace.Enabled = true;
```

Useful cho debugging architecture issues.

---

## 🎓 BEST PRACTICES

### Do ✅

```csharp
// Subscribe in OnEnable, unsubscribe in OnDisable
void OnEnable() => Bill.Events.Subscribe<XEvent>(OnX);
void OnDisable() => Bill.Events.Unsubscribe<XEvent>(OnX);

// Cache pool spawns when frequent
[SerializeField] string projectilePool = "Projectile_Arrow";
void Fire() => Bill.Pool.Spawn(projectilePool, ...);

// Use struct events for performance
public struct MyEvent : IEvent { public int value; }

// Wait for ready before using services
if (!Bill.IsReady) {
    Bill.Events.SubscribeOnce<GameReadyEvent>(_ => Init());
    return;
}
```

### Don't ❌

```csharp
// Don't forget unsubscribe
Bill.Events.Subscribe<XEvent>(OnX);
// Never unsubscribed → memory leak

// Don't use class events (allocates)
public class MyEvent : IEvent { } // ❌ allocates

// Don't bypass Bill.Pool
Instantiate(prefab); // ❌

// Don't access services without IsReady check
void Awake() {
    Bill.UI.Open<X>(); // ❌ might fail before Bill.Init
}
```

---

## 🆘 TROUBLESHOOTING

### "Bill.X is null" or NullRefException

**Cause:** Accessing service before Bill ready, or service not registered.

**Fix:**
```csharp
if (!Bill.IsReady) {
    Bill.Events.SubscribeOnce<GameReadyEvent>(_ => DoStuff());
    return;
}
DoStuff();
```

### "Pool key not registered"

**Cause:** Forgot to register in `GameBootstrap.RegisterPools()`.

**Fix:**
```csharp
Bill.Pool.Register("MyPoolKey", prefab, warmCount: 10);
```

### Event not firing on subscriber

**Causes:**
1. Subscribed AFTER fire (timing issue)
2. Subscribed wrong event type
3. Used SubscribeOnce already fired
4. Listener GameObject destroyed (need re-subscribe in OnEnable)

### Memory leak (Pool stats grow)

**Cause:** Spawn nhưng không Return.

**Fix:** Audit code paths — every `Spawn` cần matching `Return`.

```csharp
// Common pattern
var vfx = Bill.Pool.Spawn("VFX", pos);
Bill.Timer.Delay(1f, () => Bill.Pool.Return(vfx));
```

---

*End of BillGameCore API Reference.*
