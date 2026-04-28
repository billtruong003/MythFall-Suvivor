# 🧪 SPRINT TEST CHECKLIST TEMPLATE

> **Mục đích:** Standardized test checklist mỗi sprint. Copy + customize cho từng sprint version.
> **Dùng khi:** Cuối mỗi sprint, verify Definition of Done.

---

## 📋 INSTRUCTIONS

1. Copy file này thành `SPRINT_X_TEST_v0.Y.md`
2. Update version + date + sprint name
3. Run từng test, check ✅ pass, ❌ fail
4. Note issues trong "Issues Found"
5. Sign-off khi all critical tests pass

---

## 🏷️ VERSION INFO

```
Sprint: [X]
Version: v0.[Y]
Date: [YYYY-MM-DD]
Tester: [Name]
Build: [APK filename or commit hash]
Test device: [device name + Android version]
```

---

## 🎯 CRITICAL TESTS (Must Pass)

### Build & Boot
- [ ] APK build thành công, no errors
- [ ] APK install lên device thành công
- [ ] App launch không crash
- [ ] Boot time < 5 giây
- [ ] Console không có ERROR red

### Core Loop
- [ ] Main menu hiển thị
- [ ] Click Play → next screen works
- [ ] Game enters playable state
- [ ] Player character spawns
- [ ] Player input responsive (joystick, buttons)
- [ ] Game ends correctly (win/lose)
- [ ] Returns to menu

### Save/Load
- [ ] Player choices persisted
- [ ] Quit + reopen → state restored

---

## 🎮 GAMEPLAY TESTS

### Movement
- [ ] Joystick deadzone reasonable (no drift)
- [ ] Player moves smoothly trong all directions
- [ ] Character rotates đúng (face target hoặc move direction)
- [ ] Camera follows player smoothly
- [ ] Player không bị stuck trong geometry

### Combat — Melee
- [ ] Auto-attack triggers when enemy in range
- [ ] Animation plays correctly
- [ ] Hitbox active đúng frames
- [ ] Damage dealt to enemy
- [ ] Multiple enemies in arc bị hit
- [ ] Crit happens (~CritRate %)
- [ ] Crit visually distinct (yellow numbers, etc.)

### Combat — Ranged
- [ ] Projectile spawns from muzzle
- [ ] Projectile travels straight to target
- [ ] Hits enemy → damage + return to pool
- [ ] Multiple projectiles không leak

### Enemies
- [ ] Spawn correctly (right position, right time)
- [ ] AI behavior matches design
- [ ] Take damage, HP decrease
- [ ] Death animation plays
- [ ] Body returns to pool

### Skills (if applicable)
- [ ] Active skill button responsive
- [ ] Cooldown indicator updates
- [ ] Skill effect visible (VFX, audio, screen shake)
- [ ] Damage applied correctly
- [ ] Cannot spam skill (CD enforced)
- [ ] Passive skill triggers at threshold

---

## 🎨 POLISH TESTS (if applicable)

### Visual Feedback
- [ ] Hit spark on every hit
- [ ] Material flash đỏ on enemy hit
- [ ] Damage numbers float and fade
- [ ] Crit numbers larger/yellow
- [ ] Death burst on enemy death
- [ ] Level up ring + chime

### Audio Feedback
- [ ] BGM playing correctly per state
- [ ] Combat SFX trigger on actions
- [ ] No audio overlap clipping
- [ ] BGM crossfade smooth on transition
- [ ] Volume balanced

### Camera
- [ ] Smooth follow (no jitter)
- [ ] Shake on big events (proportional intensity)
- [ ] Hitstop on crits (50-100ms)
- [ ] Hitstop on skills (100-150ms)
- [ ] No motion sickness from shake

### UI Polish
- [ ] HUD readable on small screen
- [ ] Buttons responsive (visual feedback on press)
- [ ] Transitions smooth (fade, slide)
- [ ] No UI glitches when rotate device

---

## 🚀 PERFORMANCE TESTS

### Frame Rate
- [ ] Menu: 60+ FPS stable
- [ ] Gameplay (low load): 60 FPS stable
- [ ] Gameplay (mid load — 20 enemies): 30+ FPS stable
- [ ] Gameplay (high load — 50+ enemies + boss): 30+ FPS stable
- [ ] Skill cast moment: no FPS drop > 10

### Memory
- [ ] Initial memory < 200 MB
- [ ] After 5 min play < 250 MB
- [ ] After 10 min play < 300 MB
- [ ] No continuous growth (leak)

### Pool Stats
```
Bill.Pool.GetStats() at:
  - Run start: [active count]
  - Wave 5: [active count]
  - Wave 10: [active count]
  - Run end: [active count]
```

- [ ] Active count stable (returns to baseline after run)
- [ ] No "stuck" pooled objects

### Battery
- [ ] 15 min play → battery drop < 10%
- [ ] No thermal throttling warning

### Network (if applicable)
- [ ] Offline mode works
- [ ] Reconnect handle gracefully

---

## 📱 DEVICE COMPATIBILITY

Test trên minimum 2 devices nếu có thể:

**Device 1: [Name]**
- OS: Android [version]
- RAM: [amount]
- Test result: ✅ / ❌
- Notes: [issues]

**Device 2: [Name]**
- OS: Android [version]
- RAM: [amount]
- Test result: ✅ / ❌
- Notes: [issues]

---

## 🐛 EDGE CASES

- [ ] Spam click skill button — no double-cast
- [ ] Pause game → resume → state correct
- [ ] Player chết during skill animation → cleanup OK
- [ ] Multiple level-ups in quick succession → handle correctly
- [ ] Quit app mid-run → reopen → start menu OK (no broken state)
- [ ] Lock screen for 2 min → unlock → game continues OK
- [ ] Phone call during play → app pauses correctly → resume OK
- [ ] Notifications during play → no crash

---

## 📝 ISSUES FOUND

Format:
```
[Severity] [Component] Description
- Severity: P0 (blocking), P1 (high), P2 (medium), P3 (low)
- Component: Combat / UI / Audio / Performance / etc.
- Description: clear repro steps
```

### Critical (P0)
- [ ] None / [list]

### High (P1)
- [ ] None / [list]

### Medium (P2)
- [ ] None / [list]

### Low (P3)
- [ ] None / [list]

---

## ✅ SIGN-OFF

- [ ] All P0 issues resolved
- [ ] All P1 issues resolved or scheduled
- [ ] Definition of Done items checked
- [ ] PROGRESS.md updated với sprint status

**Sprint [X] Status:** 🟢 Done / 🔵 In Progress / 🔴 Blocked

**Notes:**
[Any final notes]

**Tester signature:** [Name + date]

---

## 🎬 NEXT STEPS

After sprint sign-off:
- [ ] Update PROGRESS.md status
- [ ] Move to next sprint
- [ ] Address any deferred issues in backlog
- [ ] Send build link to playtesters (if applicable)

---

*Use this template cho mỗi sprint version test. Maintain history of test files trong Sprints/Tests/ folder.*
