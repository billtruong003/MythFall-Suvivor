using UnityEngine;
using BillGameCore;
using Mythfall.Localization;

namespace Mythfall.Core
{
    /// <summary>
    /// Orchestrates Mythfall's initialization. Lives on [Bootstrap] GameObject
    /// alongside <see cref="BillStartup"/>. Awake() configures BillStartup's
    /// step queue + next-scene before BillStartup.Start() runs the splash sequence.
    ///
    /// Step lambdas execute inside BillStartup's coroutine, which only begins after
    /// <see cref="GameReadyEvent"/> fires — so Bill services are guaranteed available
    /// when steps run.
    /// </summary>
    [RequireComponent(typeof(BillStartup))]
    public class GameBootstrap : MonoBehaviour
    {
        const string FirstScene = "MenuScene";

        [SerializeField] BillStartup startup;

        bool _poolsRegistered;

        void Awake()
        {
            // === Register Mythfall services in Awake (not via BillStartup steps) ===
            // Why: BillStartup runs its steps inside a coroutine that includes ~1.5s of logo
            // animation. In editor-bounce mode (Play from non-bootstrap scene), Bill's Phase2
            // editor-return fires Bill.Scene.Load() immediately after GameReadyEvent — that
            // scene transition completes (~0.5s) before BillStartup's logo finishes, killing
            // the coroutine and skipping all steps. By registering here in Awake we guarantee
            // services are available regardless of scene-transition timing.
            if (!ServiceLocator.Has<LocalizationService>())
            {
                ServiceLocator.Register(new LocalizationService());
                var loc = ServiceLocator.Get<LocalizationService>();
                Debug.Log($"[GameBootstrap] LocalizationService registered, language: {loc.CurrentLanguage}");
            }

            if (startup == null) startup = GetComponent<BillStartup>();
            if (startup == null)
            {
                Debug.LogError("[GameBootstrap] BillStartup component missing. Re-run Tools → Mythfall → Sprint 0 — Run Setup.");
                return;
            }

            startup.transition = TransitionType.Fade;
            startup.transitionDuration = 0.5f;

            // Editor bounce detection — see ARCHITECTURE_DECISIONS.md
#if UNITY_EDITOR
            bool isEditorBounce = UnityEditor.EditorPrefs.GetInt("Bill_ReturnScene", -1) > 0;
            startup.nextScene = isEditorBounce ? "" : FirstScene;
#else
            startup.nextScene = FirstScene;
#endif

            // Cosmetic step — verifies all services healthy after pool registration.
            // Logo animation will eat ~1.5s of this in production; editor bounce kills it.
            startup.AddStep("Health Check", () =>
            {
                Bill.Trace.HealthCheck();
                return true;
            });
        }

        void Start()
        {
            // Pool registration needs Bill.Pool which is registered in Phase2 (after Awake,
            // before Start). Register here so pools are ready before scene transition fires.
            if (!_poolsRegistered && Bill.IsReady)
            {
                RegisterPools();
                _poolsRegistered = true;
            }
        }

        bool RegisterPools()
        {
            int registered = 0;

            var swarmer = Resources.Load<GameObject>("Prefabs/Enemies/Swarmer");
            if (swarmer != null)
            {
                Bill.Pool.Register("Enemy_Swarmer", swarmer, warmCount: 30);
                registered++;
            }
            else Debug.LogWarning("[GameBootstrap] Resources/Prefabs/Enemies/Swarmer.prefab missing — skipping pool.");

            var arrow = Resources.Load<GameObject>("Prefabs/Projectiles/Arrow");
            if (arrow != null)
            {
                Bill.Pool.Register("Projectile_Arrow", arrow, warmCount: 20);
                registered++;
            }
            else Debug.LogWarning("[GameBootstrap] Resources/Prefabs/Projectiles/Arrow.prefab missing — skipping pool.");

            Debug.Log($"[GameBootstrap] {registered}/2 pools registered.");
            return true;
        }
    }
}
