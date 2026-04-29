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
            Debug.Log($"[GameBootstrap] Awake fired @ frame={Time.frameCount} BillReady={Bill.IsReady} sceneCount={UnityEngine.SceneManagement.SceneManager.sceneCount} activeScene={UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");

            // Register Localization (no Bill deps — just JSON + PlayerPrefs)
            if (!ServiceLocator.Has<LocalizationService>())
            {
                ServiceLocator.Register(new LocalizationService());
                var loc = ServiceLocator.Get<LocalizationService>();
                Debug.Log($"[GameBootstrap] LocalizationService registered, language: {loc.CurrentLanguage}");
            }

            // Pool registration: try now if Bill ready (editor bounce flow), else defer
            TryRegisterPools("Awake");

            // Configure BillStartup splash
            if (startup == null) startup = GetComponent<BillStartup>();
            if (startup == null)
            {
                Debug.LogError("[GameBootstrap] BillStartup component missing.");
                return;
            }

            startup.transition = TransitionType.Fade;
            startup.transitionDuration = 0.5f;

#if UNITY_EDITOR
            bool isEditorBounce = UnityEditor.EditorPrefs.GetInt("Bill_ReturnScene", -1) > 0;
            startup.nextScene = isEditorBounce ? "" : FirstScene;
            Debug.Log($"[GameBootstrap] editorBounce={isEditorBounce} → BillStartup.nextScene='{startup.nextScene}'");
#else
            startup.nextScene = FirstScene;
#endif

            startup.AddStep("Health Check", () =>
            {
                Bill.Trace.HealthCheck();
                return true;
            });
        }

        void OnEnable()
        {
            Debug.Log($"[GameBootstrap] OnEnable @ frame={Time.frameCount}");
            // Subscribe to GameReadyEvent in case Bill wasn't ready in Awake (production flow).
            if (Bill.Events != null)
                Bill.Events.SubscribeOnce<GameReadyEvent>(OnGameReady);
        }

        void Start()
        {
            Debug.Log($"[GameBootstrap] Start @ frame={Time.frameCount} BillReady={Bill.IsReady} poolsRegistered={_poolsRegistered}");
            TryRegisterPools("Start");
        }

        void OnDestroy()
        {
            Debug.Log($"[GameBootstrap] OnDestroy @ frame={Time.frameCount}");
        }

        void OnGameReady(GameReadyEvent _)
        {
            Debug.Log($"[GameBootstrap] OnGameReady @ frame={Time.frameCount}");
            TryRegisterPools("GameReadyEvent");
        }

        void TryRegisterPools(string source)
        {
            if (_poolsRegistered)
            {
                Debug.Log($"[GameBootstrap] TryRegisterPools({source}) skipped — already registered");
                return;
            }
            if (!Bill.IsReady)
            {
                Debug.Log($"[GameBootstrap] TryRegisterPools({source}) deferred — Bill not ready yet");
                return;
            }
            RegisterPools();
            _poolsRegistered = true;
            Debug.Log($"[GameBootstrap] TryRegisterPools({source}) DONE");
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
