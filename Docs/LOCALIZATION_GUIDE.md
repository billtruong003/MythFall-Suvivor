# 🌐 LOCALIZATION GUIDE — Mythfall: Survivors

> **Mục đích:** Architecture cho localization custom JSON-based, chuẩn bị từ Sprint 0 để tránh rewrite massive sau này.
> **Languages:** VN + EN cho vertical slice. Architecture sẵn sàng cho zh-CN, ja, ko, es post-launch.

---

## 🎯 CRITICAL RULES (READ FIRST)

1. **TUYỆT ĐỐI KHÔNG hardcode user-facing strings trong code**
2. **Mọi UI text phải qua `LocalizationService.Get(key)`**
3. **ScriptableObject (CharacterDataSO, SkillDataSO) chứa LOCALIZATION KEYS, KHÔNG phải raw text**
4. **Font support CJK + Vietnamese diacritics từ ngày 1**
5. **UI layout flexible** — không fix-width text containers
6. **Test cả 2 ngôn ngữ trước khi commit** — VN có dấu thường dài hơn EN, layout phải work cả hai

---

## 🏗️ ARCHITECTURE OVERVIEW

```
┌──────────────────────────────────────────────────────┐
│  Resources/Localization/                             │
│  ├── lang_vi.json    (Vietnamese - primary)          │
│  ├── lang_en.json    (English)                       │
│  ├── lang_zh.json    (future)                        │
│  └── lang_ja.json    (future)                        │
└──────────────────────────────────────────────────────┘
                       ↓
              ┌─────────────────────┐
              │ LocalizationService │  (registered in GameBootstrap)
              │  - Get(key)         │
              │  - SetLanguage(...) │
              └─────────────────────┘
                       ↓
        ┌──────────────┴───────────────┐
        ↓                              ↓
  [LocalizedText]              [CharacterDataSO]
  Component on UI              Stores keys not text
  Auto-binds + updates         Resolved at runtime
        ↓                              ↓
  TMP Text element              Display text
```

**Flow:**
1. Bootstrap loads JSON cho default language → cache vào memory
2. UI components dùng `LocalizedText` tự bind key → text
3. Khi language change → broadcast event → all `LocalizedText` re-fetch
4. ScriptableObjects chứa key → resolve khi display, không bake text

---

## 📄 JSON FILE FORMAT

### Structure

```json
{
  "_meta": {
    "language": "vi",
    "version": "1.0",
    "displayName": "Tiếng Việt",
    "fallback": "en"
  },
  "ui": {
    "menu": {
      "play": "Chơi",
      "settings": "Cài đặt",
      "characters": "Anh hùng",
      "gacha": "Triệu hồi",
      "inventory": "Túi đồ",
      "shop": "Cửa hàng",
      "battle_pass": "Thẻ chiến",
      "coming_soon": "Sắp ra mắt"
    },
    "character_select": {
      "title": "Chọn Anh Hùng",
      "enter_stage": "Vào trận",
      "back": "Quay lại",
      "locked": "Đã khóa"
    },
    "hud": {
      "level": "Cấp {0}",
      "xp_progress": "{0}/{1}",
      "skill_cooldown": "{0}s",
      "wave": "Đợt {0}"
    },
    "game_over": {
      "title_defeat": "Bạn đã ngã xuống",
      "title_victory": "Chiến thắng!",
      "subtitle_defeat": "Hư Vô đã thắng...",
      "stats_time": "Thời gian sống: {0}",
      "stats_kills": "Số tiêu diệt: {0}",
      "stats_wave": "Đợt cao nhất: {0}",
      "stats_level": "Cấp đạt: {0}",
      "btn_retry": "Thử lại",
      "btn_hub": "Về Đại sảnh"
    },
    "upgrade": {
      "title": "LÊN CẤP! Chọn nâng cấp",
      "rarity_common": "Phổ thông",
      "rarity_rare": "Hiếm",
      "rarity_epic": "Sử thi",
      "btn_reroll": "Lắc lại"
    }
  },
  "character": {
    "kai": {
      "name": "Kai Lôi Phong",
      "title": "Cô Lang",
      "lore": "Hậu duệ trực hệ Lôi Hoàng. Khi Hư Vô đến, cả tộc bị tiêu diệt 3 ngày. Kai sống sót vì lúc đó đi săn xa.",
      "voice_attack": "Chết đi!",
      "voice_skill": "Lôi Đình!",
      "voice_low_hp": "Vẫn... chưa xong..."
    },
    "lyra": {
      "name": "Lyra Vọng Nguyệt",
      "title": "Kẻ Phản Đồ",
      "lore": "Phát hiện trưởng lão tộc thông đồng với Hư Vô. Tố cáo, bị xử tử hình, trốn thoát. Mang gánh nặng vì phát hiện quá muộn.",
      "voice_attack": "Mục tiêu khóa.",
      "voice_skill": "Nguyệt quang...",
      "voice_low_hp": "Vẫn còn một mũi tên..."
    }
  },
  "skill": {
    "kai_auto": {
      "name": "Song Kiếm",
      "desc": "Chém liên tục với 2 thanh kiếm vào kẻ thù gần nhất."
    },
    "kai_berserker_rush": {
      "name": "Lôi Bộc",
      "desc": "Lao về phía trước 8m, gây {0}% sát thương trên đường đi. Bất tử trong khi lao."
    },
    "kai_bloodlust": {
      "name": "Khát Máu",
      "desc": "Khi HP dưới 50%, ATK +30%. Kill địch hồi 1% HP tối đa."
    },
    "lyra_auto": {
      "name": "Cung Nguyệt",
      "desc": "Bắn mũi tên xuyên thấu vào kẻ thù gần nhất."
    },
    "lyra_overcharge": {
      "name": "Nguyệt Quang Tiễn",
      "desc": "Tích lũy {0}s, bắn ra một tia sáng xuyên thấu mọi kẻ địch."
    },
    "lyra_marked": {
      "name": "Nguyệt Ấn",
      "desc": "Mỗi đòn đánh trúng cùng kẻ địch tăng sát thương lên +20%, tối đa 4 lần."
    }
  },
  "enemy": {
    "swarmer": {
      "name": "Hư Linh Quỷ",
      "desc": "Quái nhỏ tấn công thành đàn"
    },
    "brute": {
      "name": "Hư Linh Cự",
      "desc": "Quái lớn, chậm, đòn nặng"
    },
    "shooter": {
      "name": "Hư Linh Xạ",
      "desc": "Quái bắn từ xa, kite player"
    },
    "rotwood": {
      "name": "Mộc Hủ Quân Vương",
      "desc": "Linh hồn rừng cổ bị Hư Vô nuốt chửng"
    }
  },
  "card": {
    "bloodthirst": {
      "name": "Khát Khao",
      "desc": "+12% ATK"
    },
    "iron_skin": {
      "name": "Thiết Cốt",
      "desc": "+15% Máu tối đa"
    },
    "critical_mastery": {
      "name": "Tinh Thông Chí Mạng",
      "desc": "+8% Tỉ lệ chí mạng, +20% Sát thương chí mạng"
    },
    "swift_steps": {
      "name": "Bước Chân Nhẹ",
      "desc": "+10% Tốc độ di chuyển"
    },
    "vampiric_touch": {
      "name": "Hấp Huyết",
      "desc": "+3% Hồi máu mỗi đòn đánh"
    },
    "piercing_edge": {
      "name": "Lưỡi Xuyên Thấu",
      "desc": "(Cung thủ) Mũi tên xuyên thêm 1 mục tiêu"
    },
    "shockwave": {
      "name": "Sóng Chấn",
      "desc": "(Cận chiến) Mỗi đòn đánh tỏa sát thương AoE 1.5m"
    },
    "reapers_contract": {
      "name": "Hợp Đồng Tử Thần",
      "desc": "+100% ATK, -40% Máu tối đa"
    }
  }
}
```

### English equivalent

```json
{
  "_meta": {
    "language": "en",
    "version": "1.0",
    "displayName": "English",
    "fallback": null
  },
  "ui": {
    "menu": {
      "play": "Play",
      "settings": "Settings",
      "characters": "Heroes",
      "gacha": "Summon",
      "inventory": "Inventory",
      "shop": "Shop",
      "battle_pass": "Battle Pass",
      "coming_soon": "Coming Soon"
    },
    "character_select": {
      "title": "Choose Hero",
      "enter_stage": "Enter Stage",
      "back": "Back",
      "locked": "Locked"
    },
    "hud": {
      "level": "Lv {0}",
      "xp_progress": "{0}/{1}",
      "skill_cooldown": "{0}s",
      "wave": "Wave {0}"
    }
  },
  "character": {
    "kai": {
      "name": "Kai Stormbringer",
      "title": "The Lone Wolf",
      "lore": "Direct descendant of the Thunder Sovereign. When the Hollow came, his entire tribe was wiped out in 3 days. Kai survived only because he was hunting far away.",
      "voice_attack": "Die!",
      "voice_skill": "Lightning!",
      "voice_low_hp": "Not... done yet..."
    },
    "lyra": {
      "name": "Lyra Moonweaver",
      "title": "The Renegade",
      "lore": "Discovered her tribe's elders conspiring with the Hollow. Reported them, was sentenced to death, escaped. Carries the weight of discovering it too late.",
      "voice_attack": "Target locked.",
      "voice_skill": "Moonlight...",
      "voice_low_hp": "One arrow remains..."
    }
  }
}
```

### Key Naming Convention

```
ui.{screen}.{element}              — UI strings
character.{id}.{field}             — Character data
skill.{id}.{field}                 — Skill data
enemy.{id}.{field}                 — Enemy data
card.{id}.{field}                  — Upgrade card data
item.{id}.{field}                  — Item data (post-slice)
region.{id}.{field}                — Region/chapter data (post-slice)
notification.{type}                — Push notifications
error.{code}                       — Error messages
```

**Use case-insensitive snake_case** cho mọi key.

### Format Strings

Use `{0}`, `{1}` placeholders cho dynamic values:

```json
"hud.level": "Lv {0}"
"hud.xp_progress": "{0}/{1}"
"skill.kai_berserker_rush.desc": "Lao về phía trước 8m, gây {0}% sát thương..."
```

In code:
```csharp
string text = LocalizationService.GetFormatted("hud.level", playerLevel);
// "Lv 5"
```

---

## 💻 IMPLEMENTATION

### LocalizationService.cs

`Assets/Mythfall/Scripts/Core/Localization/LocalizationService.cs`

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using BillTheDev.GameCore;

namespace Mythfall.Localization
{
    public class LocalizationService : IService, IInitializable
    {
        const string DEFAULT_LANG = "vi";
        const string FALLBACK_LANG = "en";
        const string RESOURCE_PATH = "Localization/";

        Dictionary<string, string> currentStrings;
        Dictionary<string, string> fallbackStrings;
        string currentLanguage;

        public string CurrentLanguage => currentLanguage;
        public event Action<string> OnLanguageChanged;

        public void Initialize()
        {
            // Load saved preference or default
            string savedLang = PlayerPrefs.GetString("language", DEFAULT_LANG);
            LoadLanguage(savedLang);

            // Always load English as fallback
            if (savedLang != FALLBACK_LANG)
                LoadFallback(FALLBACK_LANG);
        }

        public void SetLanguage(string langCode)
        {
            if (langCode == currentLanguage) return;

            LoadLanguage(langCode);
            PlayerPrefs.SetString("language", langCode);
            PlayerPrefs.Save();

            OnLanguageChanged?.Invoke(langCode);
            Bill.Events.Fire(new LanguageChangedEvent { newLanguage = langCode });
        }

        void LoadLanguage(string langCode)
        {
            try
            {
                var textAsset = Resources.Load<TextAsset>($"{RESOURCE_PATH}lang_{langCode}");
                if (textAsset == null)
                {
                    Debug.LogWarning($"[Localization] Language file not found: {langCode}, using fallback");
                    if (langCode != FALLBACK_LANG)
                        LoadLanguage(FALLBACK_LANG);
                    return;
                }

                currentStrings = ParseJson(textAsset.text);
                currentLanguage = langCode;
                Debug.Log($"[Localization] Loaded language: {langCode}, {currentStrings.Count} keys");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Localization] Failed to load {langCode}: {e.Message}");
            }
        }

        void LoadFallback(string langCode)
        {
            try
            {
                var textAsset = Resources.Load<TextAsset>($"{RESOURCE_PATH}lang_{langCode}");
                if (textAsset != null)
                    fallbackStrings = ParseJson(textAsset.text);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Localization] Failed to load fallback {langCode}: {e.Message}");
            }
        }

        Dictionary<string, string> ParseJson(string jsonText)
        {
            // Use Unity's JsonUtility hoặc Newtonsoft.Json để parse
            // Flatten nested JSON to flat dot-notation keys
            var result = new Dictionary<string, string>();
            var rawDict = MiniJsonParser.Parse(jsonText);
            FlattenDict(rawDict, "", result);
            return result;
        }

        void FlattenDict(Dictionary<string, object> source, string prefix, Dictionary<string, string> result)
        {
            foreach (var kvp in source)
            {
                if (kvp.Key.StartsWith("_")) continue; // skip metadata

                string key = string.IsNullOrEmpty(prefix) ? kvp.Key : $"{prefix}.{kvp.Key}";

                if (kvp.Value is Dictionary<string, object> nested)
                    FlattenDict(nested, key, result);
                else if (kvp.Value is string str)
                    result[key] = str;
            }
        }

        /// <summary>
        /// Get localized string by key. Returns key itself if not found (for debugging).
        /// </summary>
        public string Get(string key)
        {
            if (currentStrings != null && currentStrings.TryGetValue(key, out var value))
                return value;

            if (fallbackStrings != null && fallbackStrings.TryGetValue(key, out var fallback))
                return fallback;

            Debug.LogWarning($"[Localization] Missing key: {key}");
            return $"[{key}]"; // Visible in build cho missing keys
        }

        /// <summary>
        /// Get localized string with format placeholders.
        /// </summary>
        public string GetFormatted(string key, params object[] args)
        {
            string template = Get(key);
            try
            {
                return string.Format(template, args);
            }
            catch
            {
                Debug.LogWarning($"[Localization] Format error for key: {key}");
                return template;
            }
        }

        /// <summary>
        /// Check if key exists in current language.
        /// </summary>
        public bool HasKey(string key)
        {
            return currentStrings != null && currentStrings.ContainsKey(key);
        }
    }

    public struct LanguageChangedEvent : IEvent
    {
        public string newLanguage;
    }
}
```

### MiniJsonParser

Lightweight JSON parser nếu không muốn import Newtonsoft. Place in same namespace.

```csharp
// Simple recursive descent parser cho game JSON
// Handles: object, array, string, number, bool, null
// Hoặc dùng Newtonsoft.Json package nếu prefer

public static class MiniJsonParser {
    public static Dictionary<string, object> Parse(string json) {
        // Implementation: recursive descent
        // Skip cho brevity — recommend dùng Newtonsoft.Json
    }
}
```

**Recommendation:** Cài `com.unity.nuget.newtonsoft-json` (Unity Package Manager) cho robust parsing.

### LocalizedText Component

`Assets/Mythfall/Scripts/UI/LocalizedText.cs`

```csharp
using UnityEngine;
using TMPro;
using BillTheDev.GameCore;

namespace Mythfall.UI
{
    /// <summary>
    /// Auto-binds TextMeshPro text to localization key.
    /// Updates automatically when language changes.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField] string localizationKey;
        [Tooltip("If true, format with these args at runtime")]
        [SerializeField] string[] formatArgs;

        TMP_Text text;
        LocalizationService localization;

        void Awake()
        {
            text = GetComponent<TMP_Text>();
        }

        void OnEnable()
        {
            localization = ServiceLocator.Get<LocalizationService>();
            if (localization != null)
            {
                localization.OnLanguageChanged += OnLanguageChanged;
                Refresh();
            }
        }

        void OnDisable()
        {
            if (localization != null)
                localization.OnLanguageChanged -= OnLanguageChanged;
        }

        void OnLanguageChanged(string newLang) => Refresh();

        public void SetKey(string key)
        {
            localizationKey = key;
            Refresh();
        }

        public void SetFormatArgs(params string[] args)
        {
            formatArgs = args;
            Refresh();
        }

        void Refresh()
        {
            if (string.IsNullOrEmpty(localizationKey)) return;
            if (localization == null) return;

            if (formatArgs != null && formatArgs.Length > 0)
                text.text = localization.GetFormatted(localizationKey, formatArgs);
            else
                text.text = localization.Get(localizationKey);
        }

        void OnValidate()
        {
            // Editor preview
            if (Application.isPlaying) return;
            if (text == null) text = GetComponent<TMP_Text>();
            if (text != null && !string.IsNullOrEmpty(localizationKey))
                text.text = $"[{localizationKey}]";
        }
    }
}
```

**Usage trong Unity Editor:**
1. Add `LocalizedText` component vào GameObject có TMP_Text
2. Set field `Localization Key` = `ui.menu.play` (hoặc key tương ứng)
3. Khi runtime, text auto bind + update khi language change

---

## 🎨 SCRIPTABLE OBJECT INTEGRATION

### CharacterDataSO Updates

```csharp
[CreateAssetMenu(menuName = "Mythfall/Character Data")]
public class CharacterDataSO : ScriptableObject {
    [Header("Identity (use localization keys, NOT raw text)")]
    public string characterId;             // "kai"
    public string nameKey;                 // "character.kai.name"
    public string titleKey;                // "character.kai.title"
    public string loreKey;                 // "character.kai.lore"

    [Header("Visual")]
    public Sprite portrait;
    public Sprite icon;

    [Header("Type")]
    public CombatRole role;

    [Header("Stats")]
    public CharacterBaseStats baseStats;

    [Header("Skills")]
    public SkillDataSO autoAttackSkill;
    public SkillDataSO activeSkill;
    public SkillDataSO passiveSkill;

    [Header("Prefab")]
    public GameObject characterPrefab;

    // Helper methods để resolve keys at runtime
    public string GetDisplayName() {
        var loc = ServiceLocator.Get<LocalizationService>();
        return loc?.Get(nameKey) ?? characterId;
    }

    public string GetTitle() {
        var loc = ServiceLocator.Get<LocalizationService>();
        return loc?.Get(titleKey) ?? "";
    }

    public string GetLore() {
        var loc = ServiceLocator.Get<LocalizationService>();
        return loc?.Get(loreKey) ?? "";
    }
}
```

### SkillDataSO Updates

```csharp
public abstract class SkillDataSO : ScriptableObject {
    [Header("Identity (use localization keys)")]
    public string skillId;
    public string nameKey;        // "skill.kai_berserker_rush.name"
    public string descKey;        // "skill.kai_berserker_rush.desc"
    public Sprite icon;

    [Header("Type")]
    public SkillType skillType;
    public float cooldown = 0f;

    public string GetDisplayName() {
        var loc = ServiceLocator.Get<LocalizationService>();
        return loc?.Get(nameKey) ?? skillId;
    }

    public string GetDescription(params object[] args) {
        var loc = ServiceLocator.Get<LocalizationService>();
        if (args != null && args.Length > 0)
            return loc?.GetFormatted(descKey, args) ?? "";
        return loc?.Get(descKey) ?? "";
    }

    public abstract ISkillExecution CreateExecution(SkillContext ctx);
}
```

---

## 🔤 FONT HANDLING

### TMP Font Asset Setup

**Vertical slice support cần:**
- **Vietnamese** với toàn bộ diacritics (ã, ạ, ằ, ắ, ẳ, ẵ, ặ, etc.)
- **English** ASCII
- **Future:** CJK characters

**Option 1: Use Google Noto fonts**

Free, supports all scripts. Download:
- `NotoSans-Regular.ttf` (Vietnamese + English ASCII)
- `NotoSansSC-Regular.ttf` (Chinese Simplified)
- `NotoSansJP-Regular.ttf` (Japanese)
- `NotoSansKR-Regular.ttf` (Korean)

**Option 2: Per-language TMP Font Asset**

Create separate TMP font asset per language:
```
Resources/Fonts/Font_VI.asset    (Latin + Vietnamese diacritics)
Resources/Fonts/Font_EN.asset    (Latin only — smaller, faster)
Resources/Fonts/Font_ZH.asset    (CJK — larger, ~30MB)
```

### Auto Font Switch

```csharp
public class LocalizedFontController : MonoBehaviour {
    [SerializeField] TMP_Text text;
    [SerializeField] TMP_FontAsset fontVI;
    [SerializeField] TMP_FontAsset fontEN;
    [SerializeField] TMP_FontAsset fontZH;

    void Start() {
        Bill.Events.Subscribe<LanguageChangedEvent>(OnLanguageChanged);
        UpdateFont(ServiceLocator.Get<LocalizationService>().CurrentLanguage);
    }

    void OnLanguageChanged(LanguageChangedEvent e) => UpdateFont(e.newLanguage);

    void UpdateFont(string lang) {
        text.font = lang switch {
            "vi" => fontVI,
            "en" => fontEN,
            "zh-CN" => fontZH,
            _ => fontEN
        };
    }

    void OnDestroy() => Bill.Events.Unsubscribe<LanguageChangedEvent>(OnLanguageChanged);
}
```

### Dynamic SDF Atlas (Recommended)

For runtime adding characters (Vietnamese diacritics that may not be pre-baked):

```
TMP Font Asset → Atlas Population Mode: Dynamic
Font Atlas Size: 1024x1024
```

This generates glyphs on-the-fly. Slight performance cost first time character displayed, but flexible.

---

## 📐 LAYOUT FLEXIBILITY

### Anti-Pattern (DON'T DO)

```csharp
// ❌ Fixed width text container
RectTransform.sizeDelta = new Vector2(120f, 30f);
text.fontSize = 14;
text.text = LocalizationService.Get("ui.menu.play");
// "Chơi" fits, but "Triệu Hồi" overflows!
```

### Correct Pattern

```csharp
// ✅ Flexible width with min/max
text.enableAutoSizing = true;
text.fontSizeMin = 10;
text.fontSizeMax = 18;

// Or use ContentSizeFitter:
ContentSizeFitter fitter = button.GetComponent<ContentSizeFitter>();
fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
```

### Layout Group Patterns

**Use HorizontalLayoutGroup for variable-width items:**

```
ButtonContainer (HorizontalLayoutGroup, ChildForceExpand: false, ChildControlSize: true)
├── Button1 (LocalizedText: "ui.menu.play")
├── Button2 (LocalizedText: "ui.menu.gacha")
└── Button3 (LocalizedText: "ui.menu.shop")
```

Items auto-resize based on text length.

### Length Estimates (Plan Buffer)

| Text type | EN avg | VI avg | ZH avg | JA avg |
|---|---|---|---|---|
| Button label | 8 chars | 10 chars | 4 chars | 6 chars |
| Skill name | 15 chars | 20 chars | 8 chars | 12 chars |
| Skill description | 80 chars | 100 chars | 40 chars | 70 chars |
| Lore (paragraph) | 200 chars | 250 chars | 100 chars | 180 chars |

**Plan UI buffer:** 25-30% extra width vs longest English string.

### CRITICAL: Test với cả VN và EN

Khi build UI:
1. Set initial language = VN, check layout
2. Switch to EN, check no overflow/clipping
3. Switch back, verify

Repeat for major panels: MainMenu, CharacterSelect, HUD, GameOver, Upgrade.

---

## 🧪 TESTING CHECKLIST

### Pre-Build Test
- [ ] All UI strings có LocalizedText component (no hardcoded text)
- [ ] All ScriptableObjects use keys (search code: no `name = "..."` raw)
- [ ] Both lang_vi.json + lang_en.json have all keys (compare key sets)
- [ ] No `[missing.key]` visible in build (test by switching languages)
- [ ] Format strings have correct placeholders
- [ ] Vietnamese diacritics render correctly (specifically: ã, ạ, ằ, ặ, ậ)
- [ ] No layout overflow in either language
- [ ] Font asset includes all needed glyphs

### Runtime Test
- [ ] Default language loads correctly (vi)
- [ ] Settings → switch to EN → all text updates
- [ ] Switch back to VN → all text updates correctly
- [ ] No null ref when language switches mid-scene
- [ ] Save preference persists after app restart
- [ ] Missing keys logged but don't crash

### Coverage Test (manually verify each screen)
- [ ] MainMenu — all buttons + title
- [ ] CharacterSelect — character names, titles, button labels
- [ ] HUD — level, XP, skill cooldown labels
- [ ] UpgradePanel — card names, descriptions, rarity labels
- [ ] GameOver/Victory — title, stats labels, button labels
- [ ] Settings — all options
- [ ] Pause panel
- [ ] Notifications

---

## 🚀 ADDING NEW LANGUAGE (Post-Slice)

### Steps to add Chinese (zh-CN)

1. **Translate JSON:**
```bash
cp Resources/Localization/lang_en.json Resources/Localization/lang_zh.json
```
Update `_meta.language: "zh-CN"`, `_meta.displayName: "简体中文"`. Translate all values.

2. **Add CJK font:**
- Download `NotoSansSC-Regular.ttf`
- Create TMP Font Asset → Dynamic atlas, 1024x1024
- Place in `Resources/Fonts/Font_ZH.asset`

3. **Update LocalizedFontController:**
```csharp
[SerializeField] TMP_FontAsset fontZH;

text.font = lang switch {
    // ...existing...
    "zh-CN" => fontZH,
    _ => fontEN
};
```

4. **Add language selector option:**
```csharp
// In Settings UI
new LanguageOption { code = "zh-CN", display = "简体中文" }
```

5. **Test:**
- Switch to ZH-CN, verify layout
- Test long Chinese characters trong UI panels
- Verify font renders correctly

**Total time:** ~1 ngày (excluding translation time)

---

## ⚠️ COMMON ISSUES

| Issue | Solution |
|---|---|
| Text shows `[ui.menu.play]` | Key not in JSON or LocalizationService not initialized. Check `Bill.Trace.HealthCheck()`. |
| Vietnamese diacritics show as `?` | Font không support, switch to NotoSans hoặc enable Dynamic Atlas |
| Text overflow button | Use ContentSizeFitter + auto-sizing |
| Language doesn't persist | Check `PlayerPrefs.Save()` called after `SetLanguage` |
| Some text English even khi set VN | Hardcoded string somewhere, search `text.text =` for raw assignments |
| Format `{0}` shows literal | Args không pass đúng, check `GetFormatted` calls |
| Performance lag khi switch language | UI có quá nhiều LocalizedText, tối ưu bằng cache key→text trong Refresh() |

---

## 📊 INTEGRATION VỚI BILL.EVENTS

Localization broadcasts events khi switch:

```csharp
// Subscribe to know when to refresh custom UI
public class CustomUIPanel : BasePanel {
    public override void OnOpen() {
        Bill.Events.Subscribe<LanguageChangedEvent>(OnLangChanged);
    }

    public override void OnClose() {
        Bill.Events.Unsubscribe<LanguageChangedEvent>(OnLangChanged);
    }

    void OnLangChanged(LanguageChangedEvent e) {
        // Re-render any non-LocalizedText UI elements
        UpdateSkillIcons(); // for example
    }
}
```

---

## 🎯 SPRINT INTEGRATION

| Sprint | Localization tasks |
|---|---|
| **Sprint 0** | Setup `Resources/Localization/` folder, create lang_vi.json + lang_en.json templates with placeholder keys, register `LocalizationService` in Bootstrap |
| **Sprint 1** | All UI panels use `LocalizedText`. CharacterDataSO uses keys. Test EN+VN switch in MainMenu + CharacterSelect |
| **Sprint 2** | Enemy data uses keys. Boss intro/death lines localized. |
| **Sprint 3** | All upgrade card names + descriptions localized. Skill names + descriptions in SOs. |
| **Sprint 4** | Final pass — verify no hardcoded strings, layout test EN+VN, font rendering check |

---

*End of LOCALIZATION_GUIDE.md — Reference khi cần localization implementation.*
