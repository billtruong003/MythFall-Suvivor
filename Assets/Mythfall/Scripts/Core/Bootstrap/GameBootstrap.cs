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

        void Awake()
        {
            if (startup == null) startup = GetComponent<BillStartup>();
            if (startup == null)
            {
                Debug.LogError("[GameBootstrap] BillStartup component missing. Re-run Tools → Mythfall → Sprint 0 — Run Setup.");
                return;
            }

            startup.transition = TransitionType.Fade;
            startup.transitionDuration = 0.5f;

            // In Editor, when user pressed Play from a non-bootstrap scene, Bill saves that
            // scene name in EditorPrefs ("Bill_ReturnScene") and Phase2 loads it back after init.
            // If we ALSO set startup.nextScene = "MenuScene", BillStartup overrides Bill's
            // editor-return at the end of its coroutine — user lands on Menu instead of their
            // chosen test scene. Detect this and skip the override during editor bounces.
#if UNITY_EDITOR
            bool isEditorBounce = UnityEditor.EditorPrefs.GetInt("Bill_ReturnScene", -1) > 0;
            startup.nextScene = isEditorBounce ? "" : FirstScene;
#else
            startup.nextScene = FirstScene;
#endif

            startup.AddStep("Initialize Localization", () =>
            {
                ServiceLocator.Register(new LocalizationService());
                var loc = ServiceLocator.Get<LocalizationService>();
                Debug.Log($"[GameBootstrap] LocalizationService ready, language: {loc.CurrentLanguage}");
                return loc != null;
            });

            startup.AddStep("Register Pools", RegisterPools);

            startup.AddStep("Health Check", () =>
            {
                Bill.Trace.HealthCheck();
                return true;
            });

            // Sprint 1 Day 3+: AddStep("Register States", ...), AddStep("Register UI", ...)
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
