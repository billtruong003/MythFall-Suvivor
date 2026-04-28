# 🛠️ SPRINT 0 — Project Setup

> **Duration:** 0.5-1 ngày | **Output:** Empty project boots, framework ready, scene flow stub

---

## 🎯 Sprint Goal

Tạo Unity project, integrate framework, configure build pipeline. End of sprint: APK build chạy trên Android device, console hiển thị "Bill ready", auto-transition Bootstrap → Menu.

## ✅ Prerequisites
- [ ] Unity 6.3.10f1 đã cài
- [ ] Android Build Support module đã cài
- [ ] Android device hoặc emulator để test
- [ ] BillGameCore + ModularTopDown + VAT packages có sẵn

---

## 📋 TASKS

### Task 0.1: Create Unity Project (15 min) — USER MANUAL

**User làm trong Unity Hub:**
1. New Project → 3D (URP) Core template
2. Unity 6.3.10f1
3. Project name: `Mythfall`
4. Save location: chọn thư mục

### Task 0.2: Import Frameworks (15 min) — USER MANUAL

**User import 3 packages:**
1. Drop `BillGameCore` folder vào `Assets/`
2. Drop `ModularTopDown` folder vào `Assets/`
3. Drop `VAT` folder vào `Assets/`
4. Wait for Unity compile, check Console — phải 0 error

### Task 0.3: Folder Structure (Claude Code) — 10 min

Tạo folder structure trong `Assets/Mythfall/`:

```
Assets/Mythfall/
├── Scripts/
│   ├── Core/
│   ├── Characters/
│   ├── Player/
│   ├── Skills/
│   ├── Enemy/
│   ├── Inventory/
│   ├── Gameplay/
│   ├── UI/
│   ├── Input/
│   └── Polish/
├── Prefabs/
│   ├── Players/
│   ├── Enemies/
│   ├── Projectiles/
│   ├── VFX/
│   └── Items/
├── Resources/
│   ├── Characters/
│   ├── Skills/
│   ├── Enemies/
│   ├── Cards/
│   └── UI/
├── Scenes/
├── Animations/
└── Materials/
```

Có thể tạo bằng AssetDatabase API hoặc user tạo manual qua Project window.

### Task 0.4: Create Scenes (Claude Code + USER) — 15 min

3 scenes empty trong `Assets/Mythfall/Scenes/`:

1. **BootstrapScene.unity** — chỉ 1 GameObject `[Bootstrap]` empty
2. **MenuScene.unity** — chỉ 1 GameObject `[MenuRoot]` với MainCamera
3. **GameplayScene.unity** — empty với MainCamera

**Build Settings (User manual qua File → Build Settings):**
- Index 0: BootstrapScene
- Index 1: MenuScene
- Index 2: GameplayScene
- Platform: Android
- Switch Platform → Android

### Task 0.5: Configure BillBootstrapConfig (Claude Code)

Create asset `Assets/Mythfall/Resources/BillBootstrapConfig.asset`:

```
- enforceBootstrapScene: true
- targetFrameRate: 60
- vSyncCount: 0
- enableTracing: true
- includeDebugOverlay: true
- includeCheatConsole: true
- defaultGameScene: "MenuScene"
- defaultPools: [] (empty for now)
```

**LƯU Ý:** Asset path phải là `Resources/BillBootstrapConfig` để Bill auto-load.

### Task 0.6: GameBootstrap Component (Claude Code)

File: `Assets/Mythfall/Scripts/Core/GameBootstrap.cs`

```csharp
using UnityEngine;
using BillTheDev.GameCore;
using BillTheDev.GameCore.Bootstrap;
using Mythfall.Localization;

namespace Mythfall.Core
{
    /// <summary>
    /// Custom bootstrap that runs after BillGameCore is ready.
    /// Register custom states, pools, UI panels, services here.
    /// Place this on BootstrapScene's [Bootstrap] GameObject.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        void Start()
        {
            if (Bill.IsReady)
            {
                Initialize();
            }
            else
            {
                // Wait for Bill to finish initializing
                Bill.Events.SubscribeOnce<GameReadyEvent>(_ => Initialize());
            }
        }

        void Initialize()
        {
            Debug.Log("[GameBootstrap] Bill is ready. Starting game initialization...");

            RegisterServices();
            RegisterStates();
            RegisterPools();
            RegisterUIPanels();

            // Health check
            Bill.Trace.HealthCheck();

            // Transition to menu
            Bill.Scene.Load("MenuScene", TransitionType.Fade, 0.5f);
        }

        void RegisterServices()
        {
            // Localization service — MUST be registered first vì UI panels depend on it
            var localization = new LocalizationService();
            ServiceLocator.Register<LocalizationService>(localization);
            localization.Initialize();
            Debug.Log($"[GameBootstrap] Localization service registered, language: {localization.CurrentLanguage}");

            // Inventory service
            // var inventory = new InventoryService();
            // ServiceLocator.Register<InventoryService>(inventory);
            // inventory.Initialize();
        }

        void RegisterStates()
        {
            // Will be filled in Sprint 1
            Debug.Log("[GameBootstrap] States registered: 0 (placeholder)");
        }

        void RegisterPools()
        {
            // Will be filled in Sprint 1+
            Debug.Log("[GameBootstrap] Pools registered: 0 (placeholder)");
        }

        void RegisterUIPanels()
        {
            // Will be filled in Sprint 1
            Debug.Log("[GameBootstrap] UI panels registered: 0 (placeholder)");
        }
    }
}
```

**User manual:** Add `GameBootstrap` component vào `[Bootstrap]` GameObject trong BootstrapScene.

### Task 0.6.5: Localization Setup (CRITICAL — DO NOT SKIP)

**Đọc `Docs/LOCALIZATION_GUIDE.md` trước khi làm.**

#### Step 1: Create folder structure

```
Assets/Mythfall/Resources/Localization/
├── lang_vi.json    (Vietnamese - primary)
└── lang_en.json    (English)
```

#### Step 2: Create initial JSON templates

**`lang_vi.json`** (Vietnamese — default language):

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
      "coming_soon": "Sắp ra mắt"
    }
  }
}
```

**`lang_en.json`** (English):

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
      "coming_soon": "Coming Soon"
    }
  }
}
```

(Sẽ expand thêm keys trong các sprint sau khi build UI.)

#### Step 3: Create LocalizationService

Tạo `Assets/Mythfall/Scripts/Core/Localization/LocalizationService.cs` theo template trong `Docs/LOCALIZATION_GUIDE.md`.

**Cần Newtonsoft.Json package:**
- Window → Package Manager
- Add package by name: `com.unity.nuget.newtonsoft-json`
- Wait for import

#### Step 4: Create LocalizedText component

Tạo `Assets/Mythfall/Scripts/UI/LocalizedText.cs` theo template trong `Docs/LOCALIZATION_GUIDE.md`.

#### Step 5: Create event struct

`Assets/Mythfall/Scripts/Core/Events/LanguageChangedEvent.cs`:

```csharp
using BillTheDev.GameCore;

namespace Mythfall.Localization
{
    public struct LanguageChangedEvent : IEvent
    {
        public string newLanguage;
    }
}
```

#### Step 6: Verify

Test in editor:
- Console log "[Localization] Loaded language: vi"
- Call `ServiceLocator.Get<LocalizationService>().Get("ui.menu.play")` returns "Chơi"
- Call `SetLanguage("en")` → log changes, `Get("ui.menu.play")` returns "Play"

#### Step 7: Add language preference saved

PlayerPrefs auto-handles trong LocalizationService. Verify by:
- Switch to EN
- Restart app
- Verify EN persists

### Task 0.7: Android Build Settings (USER + Claude Code guidance)

**User manual qua Project Settings → Player → Android:**

- Company Name: <user choice>
- Product Name: Mythfall Survivor
- Package Name: com.<yourname>.mythfall
- Minimum API Level: Android 9.0 (API 28)
- Target API Level: latest
- Scripting Backend: IL2CPP
- Target Architectures: ARM64 ✓ (uncheck ARMv7 nếu không cần)
- Configuration: Release
- Compression Method: LZ4HC

**Graphics:**
- Color Space: Linear
- Auto Graphics API: uncheck
- Graphics APIs: chỉ Vulkan + OpenGLES3

### Task 0.8: First Build (USER MANUAL)

1. File → Build Settings → Build And Run
2. Choose output folder
3. Wait for build (5-10 min lần đầu)
4. APK auto-install lên device
5. Open app trên device
6. **Expected:** App boot → Bill init logs → MenuScene load → empty scene

### Task 0.9: Smoke Test

User check:
- [ ] App khởi động không crash
- [ ] Console log hiện "Bill is ready"
- [ ] Console log hiện "Bill.Trace.HealthCheck" thành công
- [ ] App transition tới MenuScene
- [ ] Không có error đỏ trong logcat

---

## ✅ DEFINITION OF DONE

- [ ] Unity project mở được, 0 compile error
- [ ] Folder structure đúng theo CLAUDE.md
- [ ] 3 scenes tạo + add vào Build Settings
- [ ] BillBootstrapConfig asset trong Resources/
- [ ] GameBootstrap component attach vào BootstrapScene
- [ ] **Localization folder + lang_vi.json + lang_en.json** ✨
- [ ] **LocalizationService registered** + verified Get("ui.menu.play") works ✨
- [ ] **LocalizedText component created** + tested in editor ✨
- [ ] APK build thành công
- [ ] APK install + boot trên Android device
- [ ] Console không có warning/error đáng kể
- [ ] PROGRESS.md updated: Sprint 0 → 🟢 Done

---

## 🧪 TEST CHECKLIST (Sprint 0 v0.1)

### Build Test
- [ ] Build APK thành công, file size < 30 MB
- [ ] Install lên Android device qua USB
- [ ] App icon hiển thị trong launcher

### Boot Test
- [ ] App khởi động trong < 5s
- [ ] Splash screen Unity hiện rồi tắt
- [ ] Không có dialog crash hiện ra

### Logcat Test (qua `adb logcat`)
- [ ] Không có "FATAL" hoặc "ERROR" liên quan đến Bill
- [ ] Có log "[GameBootstrap] Bill is ready"
- [ ] Có log "[GameBootstrap] Pools registered: 0 (placeholder)"

### Memory Test
- [ ] Memory < 100 MB sau 30s idle
- [ ] FPS stable 60 (target frame rate)

---

## ⚠️ COMMON ISSUES

| Issue | Solution |
|---|---|
| `BillBootstrapConfig` not found | Asset path phải đúng `Resources/BillBootstrapConfig.asset` (không có folder con) |
| Compile error sau import framework | Check Unity version 6.3.10f1, một số API có thể khác |
| Build fail "JDK not found" | Project Settings → External Tools → install Android JDK |
| APK install fail "INSTALL_FAILED" | Uninstall version cũ trên device, hoặc check package name không trùng |
| Bill not ready forever | Check BootstrapScene là build index 0 |

---

## 🚀 NEXT STEP

Sau khi Sprint 0 done:
- Update `PROGRESS.md`: Sprint 0 → 🟢 Done
- Đọc `Sprints/SPRINT_1_FOUNDATION.md`
- Bắt đầu Sprint 1

---

*Sprint 0 — Estimated 6h. Most of work là setup + manual config.*
