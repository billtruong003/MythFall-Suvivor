# 📦 Mythfall Workflow Package

> **Version:** 1.0 | **Target:** Vertical slice playable trong 2 tuần
> **Engine:** Unity 6.3.10f1 (URP)

---

## 🎯 PACKAGE PURPOSE

Đây là workflow package để build vertical slice của **Mythfall: Survivor** — mobile 3D top-down survivor roguelite game — bằng Claude Code.

Files được organize theo cách Claude Code đọc + execute mỗi sprint:

1. **CLAUDE.md** ở root → master agent prompt, đọc đầu mỗi session
2. **PROGRESS.md** → sprint tracker, status của project
3. **Sprints/** → 5 sprint detail docs với tasks + test checklists
4. **Docs/** → reference guides cho architecture, API, skill design
5. **Templates/** → code templates + setup instructions

---

## 🚀 HOW TO USE

### Setup (one-time)

1. Copy folder này vào root của Unity project, hoặc keep separate và reference path
2. Đảm bảo BillGameCore + ModularTopDown.Locomotion + VAT đã có trong Assets/
3. Đảm bảo Unity 6.3.10f1 đã cài

### Workflow mỗi sprint

**Đầu sprint (token-efficient):**
```
1. Mở Claude Code (session MỚI cho mỗi sprint)
2. Mở SESSION_PROMPTS.md → copy prompt cho sprint hiện tại
3. Paste vào Claude Code
4. Claude Code đọc docs + hỏi mày các câu cụ thể
5. Mày paste lại câu trả lời (lấy từ chat chính với "main agent")
6. Claude Code execute step-by-step
```

**Cuối sprint:**
- Run post-sprint checklist (trong SESSION_PROMPTS.md)
- Setup ScriptableObject assets manually trong Inspector
- Link references trong prefabs
- Test build trên device
- Update PROGRESS.md → 🟢 Done
- **Fix issues TRƯỚC khi sang sprint kế tiếp**

---

## 📁 FILE STRUCTURE

```
Mythfall_Workflow/
├── CLAUDE.md                          # 🤖 Master agent prompt (đọc đầu session)
├── PROGRESS.md                        # 📊 Sprint tracker + status
├── README.md                          # 📦 File này
├── SESSION_PROMPTS.md                 # 🎬 5 prompts (1 per sprint) + checklists ⭐
│
├── Sprints/                           # Sprint detail documents
│   ├── SPRINT_0_SETUP.md              # 🛠️  Project + framework + localization setup
│   ├── SPRINT_1_FOUNDATION.md         # 🎮 Player + enemy + scene loop + UI
│   ├── SPRINT_2_COMBAT_FEEL.md        # ⚔️  Variety + boss + polish layer 1
│   ├── SPRINT_3_SKILLS_PROGRESSION.md # ✨ Skills + level up + cards
│   └── SPRINT_4_POLISH_LOOP.md        # 🎨 VFX + audio + localization final + build
│
├── Docs/                              # Reference guides
│   ├── GAME_DESIGN.md                 # 🏛️  Full game design vision (lore + chars + mechanics) ⭐
│   ├── ARCHITECTURE.md                # 🏛️  Patterns + anti-patterns
│   ├── BILLGAMECORE_API.md            # 🔌 API reference cho Bill.X services
│   ├── SKILL_DESIGN_GUIDE.md          # 🎯 Skill feel + 5 pattern templates ⭐
│   ├── COMBAT_FEEL_GUIDE.md           # 💥 Combat polish techniques
│   ├── LOCALIZATION_GUIDE.md          # 🌐 Custom JSON localization system ⭐
│   └── UI_VISUAL_GUIDE.md             # 🎨 UI implementation + design tokens
│
└── Templates/                         # Reusable templates
    ├── SKILL_TEMPLATE.cs              # Code template skill mới
    ├── CHARACTER_DATA_TEMPLATE.md     # Step-by-step setup character
    └── SPRINT_TEST_CHECKLIST.md       # Test checklist template
```

---

## 🎯 SPRINT OVERVIEW

| Sprint | Focus | Days | Output |
|---|---|---|---|
| **0** | Setup | 0.5-1 | Project boots, framework ready |
| **1** | Foundation | 3 | 2 chars + Swarmer + full loop |
| **2** | Combat Variety + Boss | 3 | 3 enemies + boss + polish layer 1 |
| **3** | Skills + Progression | 3 | Skills + level up + 8 cards |
| **4** | Polish + Final Build | 2-3 | Vertical slice playable APK |

**Total:** ~12 ngày thực, ~14 ngày với buffer.

---

## ⚠️ IMPORTANT NOTES

### Realistic Timeline
**1-2 tuần ra vertical slice là tight but achievable** với Claude Code + AI assistance, GIVEN:
- User prepare assets (animations, character visuals) song song
- User available để answer questions và do manual Unity setup
- No major scope changes mid-sprint
- Bug fixes happen trong cùng sprint

### What Vertical Slice IS
- ✅ Playable end-to-end
- ✅ 2 character với playstyle khác biệt
- ✅ Combat loop satisfying
- ✅ Boss fight epic
- ✅ Polish ở mức "show được"

### What Vertical Slice IS NOT
- ❌ Full game ready for launch
- ❌ Gacha system, monetization
- ❌ Multi-chapter, equipment, set bonus
- ❌ Cloud save, backend, leaderboard
- ❌ Localization

Vertical slice là để **validate combat is fun** trước khi đầu tư build full game.

---

## 🤝 USER vs CLAUDE CODE RESPONSIBILITIES

### User (you) responsibilities
- ✋ Prepare character visuals (FBX models, textures)
- ✋ Prepare animation clips (Idle, Run, Attack, Skill, Death)
- ✋ Manual Unity Editor setup (tags, layers, scene config)
- ✋ Run Unity Editor to test
- ✋ Build APK and test on device
- ✋ Source audio files (or use placeholders)
- ✋ Source VFX prefabs (or use Unity ParticleSystem placeholders)

### Claude Code responsibilities
- 🤖 Implement all C# scripts (Player, Enemy, Skills, UI, etc.)
- 🤖 Design skill feel (timing, parameters, juice layers)
- 🤖 Architecture decisions trong rules
- 🤖 Code review for compile errors
- 🤖 Update PROGRESS.md
- 🤖 Provide test instructions
- 🤖 Suggest debugging steps when issues arise

---

## 🎬 GETTING STARTED

```bash
# Step 1: Read CLAUDE.md to understand the project
# Step 2: Read PROGRESS.md to see current sprint
# Step 3: Read Sprints/SPRINT_0_SETUP.md
# Step 4: Execute Sprint 0 tasks
# Step 5: Update PROGRESS.md
# Step 6: Move to Sprint 1
```

---

## 📞 TROUBLESHOOTING

### Claude Code không follow architecture
- Re-read CLAUDE.md ở đầu conversation
- Reference Docs/ARCHITECTURE.md cho specific patterns

### Skill feel không "juicy"
- Claude Code phải đọc Docs/SKILL_DESIGN_GUIDE.md
- Run through 7 layers checklist
- Iterate based on playtest

### Build fail
- Reference Sprints/SPRINT_0_SETUP.md "Common Issues" section
- Check Unity 6.3.10f1 compatibility với framework

### Performance không đạt 30 FPS
- Reference Docs/COMBAT_FEEL_GUIDE.md "Performance" section
- Profile trên device, not just editor

---

## 🎉 SUCCESS CRITERIA

End of Sprint 4, mày sẽ có:
- ✅ APK playable vertical slice
- ✅ Combat feel polished
- ✅ Demo show được cho playtester
- ✅ Foundation strong để build tiếp full game

**Goal:** Get to playtest stage trong 2 tuần. Iterate from there.

---

*Good luck với Mythfall: Survivor! 🏛️⚔️*

*Workflow package version 1.0 — Created 2026-04-28*
