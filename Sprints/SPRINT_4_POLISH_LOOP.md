# 🎨 SPRINT 4 — Polish + Audio/VFX + Final Playable Build

> **Duration:** 2-3 ngày | **Output:** Vertical slice playable build, ready for playtest, polished feel.

---

## 🎯 Sprint Goal

Sprint 1-3 đã có foundation, combat, skills. Sprint 4 polish thành **playable demo show được**:

End of sprint:
- VFX integration (hit spark, death burst, level up ring, skill VFX)
- Audio integration (BGM + 15+ SFX)
- Camera smooth follow + impact zoom
- Game over screen polished với stats
- Victory screen polished với rewards
- Performance optimization pass
- Bug fixing pass
- **Localization final pass — VN+EN coverage 100%**
- Final APK build

## 📚 PHẢI ĐỌC TRƯỚC KHI BẮT ĐẦU

- **`Docs/COMBAT_FEEL_GUIDE.md`** — final polish techniques
- **`Docs/UI_VISUAL_GUIDE.md`** — UI testing checklist
- **`Docs/LOCALIZATION_GUIDE.md`** — testing checklist section

## ✅ Prerequisites
- [ ] Sprint 3 done — skills + level up working
- [ ] User prepared: VFX prefabs (placeholder OK) hoặc Claude Code dùng Unity ParticleSystem code-driven
- [ ] User prepared: Audio files (placeholder OK, dùng free SFX từ kenney.nl, freesound.org)

---

## 📋 TASK BREAKDOWN

### Day 1 — VFX Integration

#### Task 4.1: VFX Prefab Templates
Tạo 6 ParticleSystem prefab trong `Prefabs/VFX/`:

**HitSpark.prefab:**
- Burst 8 particles, 0.1s lifetime
- Color: yellow/white
- Size 0.1-0.3m
- Velocity outward random
- Auto-return to pool sau 0.2s

**DeathBurst.prefab:**
- Burst 20 particles, 0.4s lifetime
- Color: purple/blue (or theme-based)
- Size 0.2-0.5m
- Outward + slight upward
- Auto-return sau 0.6s

**LevelUpRing.prefab:**
- Ring shockwave (cylinder mesh scale up + fade)
- Color: gold/yellow
- 1s duration
- Particles upward column (50 particles, slow rise)

**KaiRushTrail.prefab:**
- Trail Renderer attached to Kai during rush
- Red/orange gradient
- 0.5m width, fade over 0.3s

**LyraChargeAura.prefab:**
- Sustain effect at muzzle while charging
- Blue electric crackle particles
- Looping

**OverchargeBeam.prefab:**
- LineRenderer hoặc beam mesh
- Bright cyan color
- 0.3s lifetime
- Trail effect along beam

#### Task 4.2: VFX Manager + Event Subscription
**File:** `Scripts/Polish/VFXManager.cs`

```csharp
public class VFXManager : MonoBehaviour {
    void Awake() {
        Bill.Events.Subscribe<EnemyHitEvent>(OnEnemyHit);
        Bill.Events.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
        Bill.Events.Subscribe<PlayerLeveledUpEvent>(OnLevelUp);
        Bill.Events.Subscribe<SkillCastEvent>(OnSkillCast);
        Bill.Events.Subscribe<BossPhaseChangedEvent>(OnBossPhase);
    }

    void OnEnemyHit(EnemyHitEvent e) {
        Bill.Pool.Spawn("VFX_HitSpark", e.hitPoint, Quaternion.identity);
    }

    void OnEnemyKilled(EnemyKilledEvent e) {
        Bill.Pool.Spawn("VFX_DeathBurst", e.position, Quaternion.identity);
    }

    void OnLevelUp(PlayerLeveledUpEvent e) {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            Bill.Pool.Spawn("VFX_LevelUpRing", p.transform.position, Quaternion.identity);
    }

    void OnSkillCast(SkillCastEvent e) {
        if (e.skillId == "kai_berserker_rush") {
            // Trail handled in skill execution itself
        }
    }

    void OnBossPhase(BossPhaseChangedEvent e) {
        var pos = e.boss.transform.position;
        Bill.Pool.Spawn("VFX_BossPhaseTransition", pos, Quaternion.identity);
    }

    void OnDestroy() {
        Bill.Events.Unsubscribe<EnemyHitEvent>(OnEnemyHit);
        Bill.Events.Unsubscribe<EnemyKilledEvent>(OnEnemyKilled);
        Bill.Events.Unsubscribe<PlayerLeveledUpEvent>(OnLevelUp);
        Bill.Events.Unsubscribe<SkillCastEvent>(OnSkillCast);
        Bill.Events.Unsubscribe<BossPhaseChangedEvent>(OnBossPhase);
    }
}
```

Place VFXManager trong GameplayScene root.

#### Task 4.3: Register VFX Pools
Update `GameBootstrap.RegisterPools()`:

```csharp
Bill.Pool.Register("VFX_HitSpark",
    Resources.Load<GameObject>("Prefabs/VFX/HitSpark"), warmCount: 30);
Bill.Pool.Register("VFX_DeathBurst",
    Resources.Load<GameObject>("Prefabs/VFX/DeathBurst"), warmCount: 15);
Bill.Pool.Register("VFX_LevelUpRing",
    Resources.Load<GameObject>("Prefabs/VFX/LevelUpRing"), warmCount: 3);
Bill.Pool.Register("VFX_BossPhaseTransition",
    Resources.Load<GameObject>("Prefabs/VFX/BossPhaseTransition"), warmCount: 2);
Bill.Pool.Register("VFX_DamageNumber",
    Resources.Load<GameObject>("Prefabs/VFX/DamageNumber"), warmCount: 30);
Bill.Pool.Register("XP_Gem",
    Resources.Load<GameObject>("Prefabs/Items/XPGem"), warmCount: 100);
```

### Day 2 — Audio Integration

#### Task 4.4: Audio Asset List
User download/create + place trong `Assets/Mythfall/Resources/Audio/`:

**BGM (3 tracks):**
- `bgm_menu` — calm orchestral loop
- `bgm_combat_ch1` — energetic combat loop
- `bgm_boss` — epic boss music loop

**SFX (~17 sounds):**

| Key | Description |
|---|---|
| sfx_ui_button_click | Soft UI click |
| sfx_ui_panel_open | Whoosh |
| sfx_kai_attack_swing | Sword swing |
| sfx_kai_attack_hit | Flesh impact |
| sfx_kai_rush_battle_cry | "WAAAAH!" |
| sfx_lyra_arrow_release | Bow twang |
| sfx_lyra_arrow_impact | Arrow thud |
| sfx_lyra_charge_whir | Electric buildup |
| sfx_lyra_bow_release_thwack | Powerful release |
| sfx_enemy_hit | Generic hit |
| sfx_enemy_death | Death gurgle |
| sfx_boss_roar | Big boss roar |
| sfx_xp_pickup | Crystal chime |
| sfx_level_up_chime | Ascending chime |
| sfx_player_hurt | Pained grunt |
| sfx_player_death | Dramatic |
| sfx_skill_ready | Notification |

Sources: kenney.nl, freesound.org, opengameart.org.

#### Task 4.5: Audio Listener Component
**File:** `Scripts/Polish/AudioListenerComponent.cs`

```csharp
public class AudioListenerComponent : MonoBehaviour {
    void Awake() {
        Bill.Events.Subscribe<EnemyHitEvent>(OnEnemyHit);
        Bill.Events.Subscribe<EnemyKilledEvent>(OnEnemyKilled);
        Bill.Events.Subscribe<PlayerDamagedEvent>(OnPlayerDamaged);
        Bill.Events.Subscribe<PlayerDiedEvent>(OnPlayerDied);
        Bill.Events.Subscribe<XPCollectedEvent>(OnXPCollected);
        Bill.Events.Subscribe<PlayerLeveledUpEvent>(OnLevelUp);
    }

    void OnEnemyHit(EnemyHitEvent e) =>
        Bill.Audio.Play("sfx_enemy_hit", e.hitPoint);
    void OnEnemyKilled(EnemyKilledEvent e) =>
        Bill.Audio.Play("sfx_enemy_death", e.position);
    void OnPlayerDamaged(PlayerDamagedEvent e) =>
        Bill.Audio.Play("sfx_player_hurt");
    void OnPlayerDied(PlayerDiedEvent e) =>
        Bill.Audio.Play("sfx_player_death");
    void OnXPCollected(XPCollectedEvent e) =>
        Bill.Audio.Play("sfx_xp_pickup");
    void OnLevelUp(PlayerLeveledUpEvent e) =>
        Bill.Audio.Play("sfx_level_up_chime");

    void OnDestroy() {
        // Unsubscribe all events
    }
}
```

#### Task 4.6: BGM Per State
Update game states:

```csharp
public class MainMenuState : GameState {
    public override void Enter() {
        Bill.UI.Open<MainMenuPanel>();
        Bill.Audio.PlayMusic("bgm_menu", fadeDuration: 1.5f);
    }
}

public class InRunState : GameState {
    public override void Enter() {
        Bill.UI.Open<HudPanel>();
        Bill.Audio.PlayMusic("bgm_combat_ch1", fadeDuration: 1.5f);
    }
}

// Subscribe BossSpawnedEvent in a manager:
Bill.Events.Subscribe<BossSpawnedEvent>(_ => {
    Bill.Audio.PlayMusic("bgm_boss", fadeDuration: 2f);
});
```

### Day 2-3 — Camera, UI Polish, Bug Fix

#### Task 4.7: Smooth Camera Follow
**File:** `Scripts/Polish/CameraFollow.cs`

```csharp
public class CameraFollow : MonoBehaviour {
    [SerializeField] Transform target;
    [SerializeField] Vector3 offset = new Vector3(0, 12, -8);
    [SerializeField] float smoothTime = 0.15f;
    Vector3 velocity;

    void Start() {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) target = p.transform;
    }

    void LateUpdate() {
        if (target == null) return;
        Vector3 desired = target.position + offset;
        transform.position = Vector3.SmoothDamp(
            transform.position, desired, ref velocity, smoothTime);
    }
}
```

Combine with CameraShake từ Sprint 2 — dùng child GameObject cho shake offset, parent là CameraFollow.

#### Task 4.8: RunStatsTracker
**File:** `Scripts/Gameplay/RunStatsTracker.cs`

```csharp
public class RunStatsTracker : IService, IInitializable {
    public float SurvivedTime { get; private set; }
    public int TotalKills { get; private set; }
    public int HighestWave { get; private set; }
    public int Level { get; private set; }

    float runStartTime;
    bool tracking;

    public void Initialize() {
        Bill.Events.Subscribe<RunStartedEvent>(OnRunStart);
        Bill.Events.Subscribe<RunEndedEvent>(OnRunEnd);
        Bill.Events.Subscribe<EnemyKilledEvent>(_ => TotalKills++);
        Bill.Events.Subscribe<PlayerLeveledUpEvent>(e => Level = e.newLevel);
    }

    void OnRunStart(RunStartedEvent e) {
        runStartTime = Time.time;
        tracking = true;
        TotalKills = 0;
        HighestWave = 0;
        Level = 1;
    }

    void OnRunEnd(RunEndedEvent e) {
        SurvivedTime = Time.time - runStartTime;
        tracking = false;
    }

    public void UpdateWave(int wave) {
        if (wave > HighestWave) HighestWave = wave;
    }
}
```

Register trong GameBootstrap.

#### Task 4.9: Game Over Panel Polish
**File:** `Scripts/UI/GameOverPanel.cs`

```csharp
public class GameOverPanel : BasePanel {
    UIDocument doc;
    Label timeLabel, waveLabel, killLabel;
    Button retryBtn, menuBtn;

    public override void OnOpen() {
        // Load UXML
        var stats = ServiceLocator.Get<RunStatsTracker>();

        timeLabel.text = $"Time: {FormatTime(stats.SurvivedTime)}";
        waveLabel.text = $"Wave: {stats.HighestWave}";
        killLabel.text = $"Kills: {stats.TotalKills}";

        retryBtn.clicked += OnRetry;
        menuBtn.clicked += OnMenu;

        // Fade in animation
        // BGM fade out (handled by state)
    }

    string FormatTime(float seconds) {
        int min = (int)(seconds / 60);
        int sec = (int)(seconds % 60);
        return $"{min:00}:{sec:00}";
    }

    void OnRetry() {
        Bill.Scene.Load("GameplayScene", TransitionType.Fade, 0.5f);
        Bill.Timer.Delay(0.6f, () => Bill.State.GoTo<InRunState>());
    }

    void OnMenu() {
        Bill.Scene.Load("MenuScene", TransitionType.Fade, 0.5f);
        Bill.Timer.Delay(0.6f, () => Bill.State.GoTo<MainMenuState>());
    }
}
```

#### Task 4.10: Victory Panel Polish
Tương tự GameOverPanel nhưng:
- "Victory!" text với gold particles spawn behind
- Stats display
- "Continue" và "Back to Menu" buttons
- Triumphant audio sting

#### Task 4.11: HUD Skill Cooldown Visual

```csharp
public class HudPanel : BasePanel {
    VisualElement hpBarFill, xpBarFill;
    VisualElement skillButton, skillCooldownOverlay;
    Label levelLabel;
    PlayerSkillManager skillManager;

    public override void OnOpen() {
        Bill.Events.Subscribe<PlayerDamagedEvent>(UpdateHP);
        Bill.Events.Subscribe<XPChangedEvent>(UpdateXP);
        Bill.Events.Subscribe<PlayerLeveledUpEvent>(UpdateLevel);

        // Find skill manager
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p) skillManager = p.GetComponent<PlayerSkillManager>();

        skillButton.RegisterCallback<ClickEvent>(_ => skillManager?.TryCastActive());
    }

    void Update() {
        if (skillManager == null) return;
        // Update cooldown ring
        float cdRemain = skillManager.ActiveCooldownRemaining;
        float cdTotal = skillManager.ActiveCooldownTotal;
        float fillRatio = cdTotal > 0 ? (cdRemain / cdTotal) : 0;
        skillCooldownOverlay.style.height = Length.Percent(fillRatio * 100);
    }

    void UpdateHP(PlayerDamagedEvent e) { /* update bar */ }
    void UpdateXP(XPChangedEvent e) { /* update bar */ }
    void UpdateLevel(PlayerLeveledUpEvent e) {
        levelLabel.text = $"Lv {e.newLevel}";
    }

    public override void OnClose() {
        // Unsubscribe
    }
}
```

### Day 3 — Performance + Bug Fix Pass

#### Task 4.12: Performance Profiling
Run Unity Profiler trên Android device:
- [ ] Check FPS với 50 enemies + boss
- [ ] Identify draw call hotspots
- [ ] Identify memory allocation hotspots
- [ ] Verify GC.Collect không spike

**Common optimizations:**
- Static batching cho terrain props
- GPU instancing cho VFX particles
- Reduce particle count nếu cần
- Disable shadows hoàn toàn (top-down không cần)
- Texture compression: ASTC 6x6

#### Task 4.13: Memory Leak Check
```csharp
// In dev console
Bill.Trace.HealthCheck();
Bill.Pool.GetStats(); // Check leak
```

Run game 10 phút, check stats stable.

**Common leaks:**
- Event subscription không unsubscribe (OnDestroy)
- Coroutines không dừng khi GameObject pooled
- VFX không return to pool
- Animator state với loop animation không exit

#### Task 4.14: Final Bug Fix Pass

**Test scenarios cần check:**

- [ ] Player spam click skill button → không double-cast
- [ ] Player chết trong khi skill active → state cleanup đúng
- [ ] Boss chết trong phase transition → không stuck
- [ ] Pause game (level up) trong khi enemy mid-attack → resume đúng
- [ ] Multiple enemies hit cùng lúc → pool stats không leak
- [ ] Quay menu → start lại → no residual state
- [ ] Quit app mid-run → reopen → start at menu (no broken state)

#### Task 4.15: Final Build + Smoke Test

1. Build APK release config
2. Install on test device
3. Play 10 phút full loop
4. Test critical scenarios trong Task 4.14
5. Verify FPS counter shows 30+ stable
6. Verify no audio glitches
7. Take screenshots cho documentation

#### Task 4.16: Localization Final Pass (CRITICAL)

**Đọc `Docs/LOCALIZATION_GUIDE.md` section "Testing Checklist".**

**Step 1: Coverage audit**
- [ ] Search code: `text.text = "` — phải KHÔNG còn match (nghĩa là không còn hardcode)
- [ ] Search SO: tất cả CharacterDataSO/SkillDataSO dùng keys (verify trong inspector)
- [ ] Compare lang_vi.json vs lang_en.json — tất cả keys phải match (no missing in either)
- [ ] Search tất cả prefab UI — TMP_Text components phải có LocalizedText component

**Step 2: Visual testing — switch language, kiểm tra từng panel:**

| Panel | VN test | EN test |
|---|---|---|
| MainMenu | ✓ no overflow | ✓ no overflow |
| CharacterSelect | ✓ name, title, lore visible | ✓ name, title, lore visible |
| HUD | ✓ level, wave, timer | ✓ level, wave, timer |
| UpgradePanel | ✓ card names, descriptions | ✓ card names, descriptions |
| GameOver | ✓ stats labels, buttons | ✓ stats labels, buttons |
| Settings | ✓ language switcher works | ✓ language switcher works |

**Step 3: Edge cases**
- [ ] Vietnamese diacritics render correctly: "Lôi Phong", "Vọng Nguyệt", "Cô Lang", "Phản Đồ"
- [ ] Long English description fit trong card without clipping
- [ ] Switch language → restart app → preference persisted
- [ ] Format strings work: "Lv 5" / "Wave 8" / "120/250 XP"

**Step 4: Add missing translations**
Nếu phát hiện missing keys trong mid-playtest, add vào cả 2 files với:
- VN: localized properly với diacritics đúng
- EN: localized với tone phù hợp character (Kai gruff, Lyra precise, etc.)

**Step 5: Console check**
- [ ] Trong toàn bộ playthrough, console không có log "[Localization] Missing key: ..."
- [ ] Không có `[some.key]` visible text trong UI

---

## ✅ DEFINITION OF DONE

### Visual Polish
- [ ] Hit spark visible on every hit
- [ ] Death burst on every enemy death
- [ ] Level up ring + chime audio
- [ ] Skill VFX rendered cho cả 2 active skills
- [ ] Boss phase transition VFX + roar audio
- [ ] Damage numbers từ Sprint 2 still working

### Audio
- [ ] BGM plays in Menu (calm), Combat (energetic), Boss (epic)
- [ ] BGM transitions có crossfade smooth
- [ ] All combat actions có SFX feedback
- [ ] No audio overlap clipping
- [ ] Audio volume balanced (BGM not louder than SFX)

### UI/UX Polish
- [ ] HUD readable on small mobile screen
- [ ] Skill button có cooldown visual feedback
- [ ] Game Over screen show stats
- [ ] Victory screen show stats + reward placeholder
- [ ] Retry button reload gameplay smooth
- [ ] Back to Menu button works

### Camera
- [ ] Smooth follow player, no stutter
- [ ] Shake không cause motion sickness
- [ ] Player luôn visible (không bị clip vào geometry)

### Performance
- [ ] FPS ≥ 30 trên reference Android device
- [ ] No memory leak qua 10 phút continuous play
- [ ] APK size < 50 MB
- [ ] Cold start < 5s

### Build
- [ ] APK build release config success
- [ ] Install + run trên Android device
- [ ] No crash trong 10 phút playtest
- [ ] Build version tagged: `v0.5-vertical-slice`

### Localization (CRITICAL)
- [ ] No hardcoded UI strings — all qua LocalizedText
- [ ] All ScriptableObjects (Character, Skill, Card, Enemy) dùng keys
- [ ] lang_vi.json và lang_en.json keys match completely
- [ ] Test EN+VN switch trên all panels — no overflow, no clipping
- [ ] Vietnamese diacritics render correctly trong all text
- [ ] No `[missing.key]` visible anywhere
- [ ] Console không có "[Localization] Missing key:" warnings
- [ ] Language preference persists qua app restart

---

## 🧪 TEST CHECKLIST (Sprint 4 v0.5 — FINAL)

### Full Playthrough Test
- [ ] App launch → Main menu shows với BGM
- [ ] Click Play → Character Select
- [ ] Pick Kai → Gameplay loads với combat BGM
- [ ] Movement smooth, joystick responsive
- [ ] Auto-attack hits enemy → hit spark + audio
- [ ] Enemy death → death burst + audio + XP gem
- [ ] Pickup XP → magnet + chime
- [ ] Level up → pause + 3 cards + ring VFX
- [ ] Pick card → effect apply, resume
- [ ] Cast Berserker Rush → trail + battle cry + screen shake
- [ ] Bloodlust trigger khi HP < 50% → buff effect
- [ ] Boss spawn after 60s → boss BGM + roar
- [ ] Boss phase 2 transition → red glow + audio
- [ ] Boss death → Victory panel với stats
- [ ] Click Back to Menu → return to menu
- [ ] Test với Lyra: Overcharge Shot feel weighty

### Audio Coverage
- [ ] Menu BGM playing
- [ ] Combat BGM playing during gameplay
- [ ] Boss BGM transitions when boss spawns
- [ ] All SFX trigger correctly
- [ ] Audio không overlap clipping

### Visual Polish
- [ ] VFX render correctly trên all events
- [ ] Material flash đỏ on enemy hit
- [ ] Damage numbers float correctly
- [ ] Hitstop trên crit hits
- [ ] Screen shake on big events

### Performance
- [ ] FPS counter visible trong dev build
- [ ] FPS ≥ 30 với 30+ enemies
- [ ] FPS ≥ 30 với boss + waves
- [ ] FPS ≥ 30 với all VFX active
- [ ] Memory stable (< 200 MB)

### Edge Cases
- [ ] Spam skill button → 1 cast per CD
- [ ] Pause mid-skill → resume properly
- [ ] Player chết during skill → cleanup OK
- [ ] Multiple level ups in quick succession → handle correctly
- [ ] Quit app mid-run → reopen safely

---

## 🚀 BUILD INSTRUCTIONS

### Build Configuration
```
Player Settings:
- Scripting Backend: IL2CPP
- Target Architectures: ARM64
- Target API: Latest
- Minimum API: 28 (Android 9.0)
- Compression: LZ4HC
- Strip Engine Code: Enabled
- Managed Stripping Level: Medium

Quality Settings (Mobile):
- Disable shadows
- Texture Quality: Half
- Anti Aliasing: Disabled
- VSync: Don't Sync

Build:
File → Build Settings → Build And Run
Output: Mythfall_v0.5_vertical_slice.apk
```

### Post-Build Verification
```bash
# On Android device with USB debugging
adb logcat -s Unity ActivityManager AndroidRuntime

# Look for:
# - "[GameBootstrap] Bill is ready" — framework boot OK
# - No "FATAL EXCEPTION" or "ANR"
# - No memory warnings
```

---

## ⚠️ COMMON ISSUES

| Issue | Solution |
|---|---|
| BGM stops on scene change | `Bill.Audio.PlayMusic` should persist across scenes; check AudioSource on DontDestroyOnLoad |
| VFX không return to pool | Particle System main module → Stop Action: Disable, plus ParticleAutoReturn script |
| Camera jitter on follow | LateUpdate (not Update); check rigidbody interpolation |
| Hit spark wrong position | hitPoint event field set correctly trong combat code |
| Skill cooldown UI not updating | Update() poll every frame, not just on event |

---

## 🎉 DELIVERABLES

End of Sprint 4, mày sẽ có:

- [ ] **Vertical slice APK** chạy được, ~50 MB
- [ ] **5+ phút gameplay** end-to-end với combat feel polished
- [ ] **2 character playable** với playstyle khác biệt
- [ ] **3 enemy types + 1 boss** với behavior rõ ràng
- [ ] **Active + Passive skills** với feel "juicy"
- [ ] **In-run progression** với 8+ upgrade cards
- [ ] **Audio + VFX** integrated cho combat feedback
- [ ] **Build playable** demo show được cho investor/playtester

**Update PROGRESS.md:** Sprint 4 → 🟢 Done. Vertical slice complete.

---

## 🎬 NEXT STEPS

Sau vertical slice, options:

1. **Playtest với 5-10 người** → collect feedback
2. **Iterate combat feel** based on feedback
3. **Decide:** continue full game development hoặc pivot
4. **If continue:** đọc full PROGRESS.md cho production roadmap (gacha, equipment, monetization, multi-chapter, etc.)

---

*End of Sprint 4. Vertical slice DONE.*
