# 🎨 UI VISUAL GUIDE — Mythfall: Survivors

> **Mục đích:** Implementation guidelines cho UI panels. GAME_DESIGN.md đã có ASCII mockups — file này là how-to-implement chi tiết.
> **Đọc khi:** Build UI panels (Sprint 1, 4).

---

## 🎯 OVERVIEW

Mythfall UI dùng **Unity UI Toolkit (UXML/USS)** thay vì Canvas UGUI, vì:
- Lighter on mobile (less draw call)
- Easier responsive layout (flexbox-style)
- Better tooling for designers (UI Builder visual editor)
- Better separation HTML-like markup vs styling

**Fallback:** Nếu UI Toolkit gặp issue, dùng UGUI Canvas. Document patterns work cả 2.

---

## 📐 SCREEN DIMENSIONS & SAFE ZONES

**Reference resolution:** 1080×2340 (modern Android phone, 19.5:9 ratio)

**Safe zones:**
- Top: 80px (status bar buffer)
- Bottom: 120px (gesture nav buffer iPhone)
- Left/right: 24px (rounded corner buffer)

**Aspect ratio support:** 16:9 → 21:9 (wide handle với letterbox color)

---

## 🎨 DESIGN TOKENS

### Color Palette

```css
/* Primary brand colors */
--color-primary: #FFD700;          /* Gold - lightning, mandate, achievements */
--color-secondary: #5DCAA5;        /* Teal - mystic, mana, info */
--color-tertiary: #D85A30;         /* Coral - urgent, fire, danger */

/* Semantic colors */
--color-hp: #E24B4A;               /* Red HP */
--color-xp: #F4C0D1;               /* Pink XP */
--color-success: #97C459;          /* Green */
--color-warning: #EF9F27;          /* Amber */
--color-danger: #A32D2D;           /* Dark red */

/* Star tier colors */
--star-3-color: #B4B2A9;           /* Gray (3★) */
--star-4-color: #5DCAA5;           /* Teal (4★) */
--star-5-color: #BA7517;           /* Gold (5★) */
--star-6-red-color: #E24B4A;       /* Red (6★ Đỏ) */

/* Rarity colors (for cards) */
--rarity-common: #B4B2A9;          /* Gray */
--rarity-rare: #378ADD;            /* Blue */
--rarity-epic: #7F77DD;            /* Purple */
--rarity-game-changer: #FFD700;    /* Gold */

/* Background depths */
--bg-darkest: #0A0A14;             /* Modal overlay */
--bg-dark: #1A1A28;                /* Panel base */
--bg-mid: #2A2A3C;                 /* Card surface */
--bg-light: #3C3C50;               /* Hover state */

/* Text */
--text-primary: #F1EFE8;           /* Primary white */
--text-secondary: #B4B2A9;         /* Muted gray */
--text-disabled: #5F5E5A;          /* Disabled gray */
--text-info: #5DCAA5;              /* Info teal */
--text-danger: #E24B4A;            /* Error red */
```

### Typography

**Font stack:**
- VN/EN: NotoSans-Regular (TMP Dynamic SDF)
- ZH-CN: NotoSansSC-Regular (post-launch)
- JA: NotoSansJP-Regular (post-launch)

**Sizes (sp-equivalent for Unity TMP):**
```
--font-h1: 36px      /* Game title only */
--font-h2: 28px      /* Major panel title */
--font-h3: 22px      /* Section header */
--font-body-l: 18px  /* Primary content */
--font-body-m: 16px  /* Default body */
--font-body-s: 14px  /* Secondary info */
--font-caption: 12px /* Hint, label */

/* Weights — only 2 weights */
--weight-regular: 400
--weight-bold: 600   /* Used sparingly — buttons, emphasis */
```

**Avoid:** font-weight-700+ (looks heavy on mobile screens).

### Spacing Scale

```
--space-xs: 4px
--space-s: 8px
--space-m: 12px
--space-l: 16px
--space-xl: 24px
--space-2xl: 32px
--space-3xl: 48px
```

### Border Radius

```
--radius-s: 4px      /* Inputs, small chips */
--radius-m: 8px      /* Default buttons, cards */
--radius-l: 12px     /* Large cards, panels */
--radius-xl: 16px    /* Hero panels, modal */
--radius-pill: 999px /* Pills, badges */
```

---

## 🧩 COMPONENT LIBRARY

### 1. Button

**Variants:**
- **Primary CTA** (gold gradient, bold) — main action like "ENTER STAGE"
- **Secondary** (outlined, no fill) — back, settings
- **Tertiary** (ghost, no border) — text-only nav
- **Icon button** (square, just icon)

**State system:**
- Default: 100% opacity
- Hover/Press: 120% scale + slight brighten
- Active: 95% scale (button being pressed)
- Disabled: 50% opacity, no interaction

**UXML pattern:**
```xml
<Button class="btn btn-primary btn-lg">
    <LocalizedText key="ui.character_select.enter_stage" />
</Button>
```

**USS:**
```css
.btn {
    border-radius: var(--radius-m);
    padding: var(--space-m) var(--space-xl);
    font-size: var(--font-body-l);
    transition: scale 0.15s, opacity 0.15s;
}

.btn-primary {
    background-color: var(--color-primary);
    color: var(--bg-darkest);
    font-weight: var(--weight-bold);
}

.btn-primary:hover {
    scale: 1.02;
}

.btn-primary:active {
    scale: 0.98;
}
```

### 2. Card (Character/Item)

**Anatomy:**
```
┌─────────────────────┐
│  ★★★★              │  ← Star rating (top-left)
│                     │
│   [Portrait]        │  ← Character/item art (centered)
│                     │
│   Name              │  ← Localized name
│   Title             │  ← Localized title
│   ⚡ 1820           │  ← Combat power / level
└─────────────────────┘
```

**State:**
- Selected: `border: 2px solid var(--color-primary)`
- Locked: opacity 0.4, lock icon overlay
- New (recent unlock): pulse animation gold

### 3. HP/XP Bar

**HP Bar:**
```
┌─────────────────────────┐
│ ██████████████░░░░░░░░░ │
└─────────────────────────┘
   ↑ red gradient, easing fill animation
```

**Animation:** Khi damage taken, fill drops với 0.3s ease-out + slight color flash.

```css
.hp-bar-fill {
    background-color: var(--color-hp);
    transition: width 0.3s ease-out;
}

.hp-bar-fill.taking-damage {
    /* Brief flash white */
    background-color: var(--text-primary);
}
```

**XP Bar:**
- Khác HP: empty state là gray, fill là yellow
- Trên level up: flash gold + scale pulse

### 4. Skill Button (HUD)

**Anatomy:**
```
┌────────────┐
│   [Icon]   │  ← Skill icon
│            │
│   ┌───┐    │  ← Cooldown ring overlay
│   │ 8 │    │  ← Cooldown number (if on CD)
│   └───┘    │
└────────────┘
```

**States:**
- Ready: full color, glow outline
- Cooldown: grayscale + ring fills clockwise
- Disabled: faded (rare in slice — ammo system not in)

**Touch handling:** Large touch target 80×80px minimum. Visual size có thể nhỏ hơn (50×50) nhưng touch zone phải lớn.

### 5. Damage Number

**Floating text trên enemy khi bị hit:**

```
Normal hit:    "150"  (white, 5pt, regular)
Crit hit:      "150!" (yellow, 7pt, bold)
Big hit:       "350"  (orange, 8pt, bold)
Heal:          "+50"  (green, 5pt, regular)
```

**Animation:**
- Spawn at hit point + slight random offset (max 0.3m)
- Float up 1.5m over 0.8s
- Scale: 110% → 100% (pop in)
- Fade out from 70% lifetime
- Always face camera (billboard)

### 6. Upgrade Card (in UpgradePanel)

```
┌──────────────────┐
│   [RARITY BADGE] │ ← Border color: common/rare/epic/golden
│                  │
│      [Icon]      │
│                  │
│      Name        │
│      Description │
│                  │
└──────────────────┘
```

**Stagger animation:**
- 3 cards appear with 0.15s delay each
- Each card: scale 0 → 1 với ease-back

---

## 🖼️ SCREEN-BY-SCREEN IMPLEMENTATION

### Main Menu

**Hierarchy:**
```
MainMenuPanel (UIDocument)
├── Background (full screen, parallax)
├── TopBar
│   ├── SettingsButton
│   └── GameTitle
├── PlayerInfoStrip
│   ├── PlayerAvatar
│   ├── PlayerLevel
│   └── CombatPower
├── PlayButton (large CTA)
├── FeatureGrid (2x3 grid)
│   ├── CharactersBtn (always enabled)
│   ├── GachaBtn (LOCKED in slice)
│   ├── InventoryBtn (LOCKED in slice)
│   ├── ShopBtn (LOCKED in slice)
│   ├── BPBtn (LOCKED in slice)
│   └── MissionBtn (LOCKED in slice)
└── CurrencyBar (bottom)
    ├── CrystalAmount
    ├── GoldAmount
    └── TicketAmount
```

**Locked button visual:**
- Faded (opacity 0.5)
- Lock icon overlay
- Tooltip on tap: "Coming Soon" (localized: `ui.menu.coming_soon`)
- Don't trigger any action

**In Sprint 1:** Implement only PlayButton + CharactersBtn functional. Others = visual only.

### Character Select

**Hierarchy:**
```
CharacterSelectPanel
├── BackButton
├── Title ("Choose Hero")
├── CharacterGrid (2x2 scrollable)
│   ├── CharacterCard_Kai (selectable)
│   ├── CharacterCard_Lyra (selectable)
│   ├── LockedCard_1 (disabled)
│   └── LockedCard_2 (disabled)
├── PreviewPanel
│   ├── BigPortrait
│   ├── SkillList (3 skills shown)
│   └── StatsDisplay
└── ConfirmButton ("Enter Stage")
```

**Selection logic:**
- Tap character card → highlight border + show in preview
- Confirm → save selection + load gameplay

**In Sprint 1:** 2 cards (Kai, Lyra) functional. 2-3 locked cards visible cho future content tease.

### Gameplay HUD

**Hierarchy:**
```
HUDPanel (overlay, no background)
├── TopStrip
│   ├── HPBar (left)
│   ├── XPBar (mid)
│   ├── LevelText
│   └── TimerText (right)
├── PauseButton (top-right)
├── BottomLeft (joystick zone)
│   └── VirtualJoystick
└── BottomRight (action zone)
    └── SkillButton
```

**Render layer:** Always on top, ignore raycast except interactive elements.

**Touch zones:**
- Bottom-left half = movement (joystick anywhere on tap)
- Bottom-right corner = skill button
- Pause button + HP bar = top-right (avoid common touch area)

### Game Over Panel

**Hierarchy:**
```
GameOverPanel (modal overlay)
├── DimBackground (semi-transparent)
├── ResultTitle ("YOU FELL" or "VICTORY!")
├── Subtitle (flavor text)
├── StatsContainer
│   ├── TimeStat
│   ├── KillsStat
│   ├── WaveStat
│   └── LevelStat
├── ActionButtons
│   ├── RetryButton
│   └── HubButton
└── (optional) RewardsPreview
```

**Background dim:** `rgba(10, 10, 20, 0.85)`

**Animation sequence:**
1. Time slow to 0.3x for 0.5s
2. Background fade in
3. Title flies in từ top
4. Stats appear stagger (0.1s delay each)
5. Buttons fade in last

### Upgrade Panel

**Hierarchy:**
```
UpgradePanel (modal, pause-on-show)
├── DimBackground
├── Title ("LEVEL UP!")
├── Subtitle ("Choose an upgrade")
├── CardGrid (3 cards horizontal)
│   ├── UpgradeCard_1
│   ├── UpgradeCard_2
│   └── UpgradeCard_3
└── RerollButton ("🎲 Reroll (1 free)")
```

**Animation:**
- Pause time
- Cards spawn with stagger animation
- On selection: card pulse → all cards fade out → resume time

---

## 📱 RESPONSIVE LAYOUT

### Breakpoints

```css
/* Phone portrait — primary */
@media (max-aspect-ratio: 9/16) {
    .character-grid { grid-template-columns: 1fr 1fr; }
}

/* Tablet portrait */
@media (min-aspect-ratio: 9/12) {
    .character-grid { grid-template-columns: 1fr 1fr 1fr; }
}

/* Tablet landscape — bonus */
@media (min-aspect-ratio: 16/9) {
    .layout { flex-direction: row; }
}
```

UI Toolkit tự handle aspect ratio qua flexbox + percent units. Use `%` thay vì `px` cho widths.

### Notch / Cutout Handling

```csharp
// Get safe area at runtime
Rect safeArea = Screen.safeArea;
float topInset = Screen.height - safeArea.height - safeArea.y;
float bottomInset = safeArea.y;

// Apply to root canvas
rootElement.style.paddingTop = new Length(topInset, LengthUnit.Pixel);
rootElement.style.paddingBottom = new Length(bottomInset, LengthUnit.Pixel);
```

---

## 🎬 ANIMATION PATTERNS

### Panel Open

**Default open animation:**
- Scale 0.9 → 1.0
- Opacity 0 → 1
- Duration 0.25s
- Ease: ease-out-back

```csharp
// Tween in PanelBase
public override void OnOpen() {
    transform.localScale = Vector3.one * 0.9f;
    canvasGroup.alpha = 0;

    Bill.Tween.Scale(transform, Vector3.one, 0.25f, Ease.OutBack);
    Bill.Tween.To(0f, 1f, 0.25f, v => canvasGroup.alpha = v);
}
```

### Stagger Children

```csharp
// Stagger card appearance
for (int i = 0; i < cards.Length; i++) {
    int idx = i;
    Bill.Timer.Delay(idx * 0.15f, () => {
        cards[idx].SetActive(true);
        // Trigger card individual animation
    });
}
```

### Number Counter

**For currency, XP, etc:**
```csharp
public void AnimateNumber(int from, int to, float duration) {
    Bill.Tween.To(from, to, duration, value => {
        text.text = ((int)value).ToString("N0");
    }, Ease.OutQuad);
}
```

---

## 🧪 UI TESTING CHECKLIST

### Visual
- [ ] All text readable on 5" device
- [ ] All text readable on tablet
- [ ] No clipping in either VN or EN language
- [ ] Star ratings render correctly
- [ ] Locked elements clearly distinguishable
- [ ] Hover/press states visible

### Interaction
- [ ] All buttons have ≥ 80×80px touch target
- [ ] Joystick responsive without dead zone
- [ ] No accidental touches between regions
- [ ] Back button works on all screens
- [ ] Modal blocks underlying input

### Performance
- [ ] UI doesn't drop FPS below 30
- [ ] No GC.Alloc on UI updates (use cached strings)
- [ ] Atlas optimal — UI textures combined into 1-2 atlases

### Localization
- [ ] All text uses LocalizedText component
- [ ] Test EN ↔ VN switch — no overflow
- [ ] Vietnamese diacritics render correctly
- [ ] Format strings work `{0}` `{1}`

---

## 🎨 ASSET RECOMMENDATIONS

### Free Icon Packs (Placeholder OK for slice)

- **Game Icons** (game-icons.net) — CC BY 3.0
- **Tabler Icons** (tabler-icons.io) — MIT
- **Heroicons** (heroicons.com) — MIT

### Sprite Settings

- **UI Atlas:** 2048×2048, max 4 atlases per scene
- **Filter Mode:** Bilinear (avoid Point cho UI)
- **Compression:** ASTC 6×6 (mobile)
- **Border (9-slice):** Set khi cần stretch (buttons, panels)

---

## 🚀 IMPLEMENTATION PRIORITY (Sprint 1)

**Must have:**
1. MainMenuPanel — Play button + locked feature buttons (visible)
2. CharacterSelectPanel — 2 functional cards + 2 locked
3. HUDPanel — HP bar, XP bar, skill button, joystick
4. GameOverPanel — Stats + Retry/Hub buttons

**Nice to have (Sprint 4 polish):**
- Animations smooth on all panels
- Stagger card animation
- Number counter animation cho rewards
- Particle effects on level up

**Skip for slice:**
- Settings panel (just basic volume + language)
- Inventory, Gacha, Shop, BP panels (just locked buttons visible)
- Detailed character preview

---

*End of UI_VISUAL_GUIDE.md — Reference khi build UI panels.*
