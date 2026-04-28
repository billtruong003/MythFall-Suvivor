#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using ModularTopDown.Locomotion;
using Mythfall.Characters;
using Mythfall.Enemy;
using Mythfall.Gameplay;
using Mythfall.Player;

namespace Mythfall.EditorTools
{
    /// <summary>
    /// One-shot Sprint 2 setup. Creates layers (Enemy, Projectile), AnimatorControllers
    /// with the required parameters, URP materials, and four placeholder prefabs:
    ///   - Kai (capsule, red, melee — root + visual + muzzle + hitbox child)
    ///   - Lyra (capsule, teal, ranged — root + visual + muzzle)
    ///   - Swarmer (capsule, gray — root + visual + collider for player hit detection)
    ///   - Arrow (small projectile — kinematic Rigidbody + trigger sphere)
    /// Also creates Swarmer_Data.asset (EnemyDataSO) and wires CharacterDataSO prefab refs.
    /// Idempotent — re-run replaces existing assets.
    ///
    /// Run via: Tools → Mythfall → Sprint 2 — Build Placeholder Prefabs
    /// </summary>
    public static class Sprint2Setup
    {
        const string PrefabsRoot = "Assets/Mythfall/Prefabs";
        const string ResourcesRoot = "Assets/Mythfall/Resources";
        const string AnimationsFolder = "Assets/Mythfall/Animations";
        const string MaterialsFolder = "Assets/Mythfall/Materials";

        const string PlayerPrefabsFolder = PrefabsRoot + "/Players";
        const string EnemyPrefabsFolder = ResourcesRoot + "/Prefabs/Enemies";
        const string ProjectilePrefabsFolder = ResourcesRoot + "/Prefabs/Projectiles";
        const string CharactersDataFolder = ResourcesRoot + "/Characters";
        const string EnemiesDataFolder = ResourcesRoot + "/Enemies";

        const string PlayerAnimCtrlPath = AnimationsFolder + "/PlayerAnimator.controller";
        const string EnemyAnimCtrlPath = AnimationsFolder + "/EnemyAnimator.controller";

        const string KaiPrefabPath = PlayerPrefabsFolder + "/Kai.prefab";
        const string LyraPrefabPath = PlayerPrefabsFolder + "/Lyra.prefab";
        const string SwarmerPrefabPath = EnemyPrefabsFolder + "/Swarmer.prefab";
        const string ArrowPrefabPath = ProjectilePrefabsFolder + "/Arrow.prefab";

        const string KaiDataPath = CharactersDataFolder + "/Kai_Data.asset";
        const string LyraDataPath = CharactersDataFolder + "/Lyra_Data.asset";
        const string SwarmerDataPath = EnemiesDataFolder + "/Swarmer_Data.asset";

        const string KaiMatPath = MaterialsFolder + "/Kai_Mat.mat";
        const string LyraMatPath = MaterialsFolder + "/Lyra_Mat.mat";
        const string SwarmerMatPath = MaterialsFolder + "/Swarmer_Mat.mat";
        const string ArrowMatPath = MaterialsFolder + "/Arrow_Mat.mat";

        const int EnemyLayer = 8;
        const int ProjectileLayer = 9;

        [MenuItem("Tools/Mythfall/Sprint 2 — Build Placeholder Prefabs")]
        public static void Build()
        {
            EnsureLayer(EnemyLayer, "Enemy");
            EnsureLayer(ProjectileLayer, "Projectile");

            EnsureFolders();

            var playerCtrl = CreatePlayerAnimatorController();
            var enemyCtrl = CreateEnemyAnimatorController();

            var kaiMat = CreateUrpLitMaterial(KaiMatPath, new Color(0.847f, 0.353f, 0.188f)); // #D85A30
            var lyraMat = CreateUrpLitMaterial(LyraMatPath, new Color(0.365f, 0.792f, 0.647f)); // #5DCAA5
            var swarmerMat = CreateUrpLitMaterial(SwarmerMatPath, new Color(0.373f, 0.369f, 0.353f)); // #5F5E5A
            var arrowMat = CreateUrpLitMaterial(ArrowMatPath, new Color(0.95f, 0.85f, 0.55f));

            var kaiPrefab = BuildPlayerPrefab(KaiPrefabPath, "Kai", isMelee: true, mat: kaiMat, ctrl: playerCtrl);
            var lyraPrefab = BuildPlayerPrefab(LyraPrefabPath, "Lyra", isMelee: false, mat: lyraMat, ctrl: playerCtrl);
            var swarmerPrefab = BuildSwarmerPrefab(SwarmerPrefabPath, mat: swarmerMat, ctrl: enemyCtrl);
            var arrowPrefab = BuildArrowPrefab(ArrowPrefabPath, mat: arrowMat);

            BuildSwarmerData(SwarmerDataPath, swarmerPrefab);
            WirePlayerDataPrefab(KaiDataPath, kaiPrefab);
            WirePlayerDataPrefab(LyraDataPath, lyraPrefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Sprint2Setup] DONE. 4 prefabs + 2 AnimatorControllers + 4 materials + Swarmer_Data + Layer 'Enemy'/'Projectile' configured.");
        }

        // -------------------------------------------------------------------
        // Layers
        // -------------------------------------------------------------------

        static void EnsureLayer(int slot, string name)
        {
            const string path = "ProjectSettings/TagManager.asset";
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            if (assets == null || assets.Length == 0)
            {
                Debug.LogWarning($"[Sprint2Setup] TagManager.asset not loadable; set layer '{name}' manually at slot {slot}.");
                return;
            }

            var so = new SerializedObject(assets[0]);
            var layers = so.FindProperty("layers");
            if (layers == null || slot >= layers.arraySize) return;

            var layerProp = layers.GetArrayElementAtIndex(slot);
            if (string.IsNullOrEmpty(layerProp.stringValue))
            {
                layerProp.stringValue = name;
                so.ApplyModifiedPropertiesWithoutUndo();
                Debug.Log($"[Sprint2Setup] Layer slot {slot} → '{name}'");
            }
            else if (layerProp.stringValue != name)
            {
                Debug.LogWarning($"[Sprint2Setup] Layer slot {slot} already named '{layerProp.stringValue}', expected '{name}' — leaving alone.");
            }
        }

        // -------------------------------------------------------------------
        // Folders
        // -------------------------------------------------------------------

        static void EnsureFolders()
        {
            EnsureFolder(PlayerPrefabsFolder);
            EnsureFolder(EnemyPrefabsFolder);
            EnsureFolder(ProjectilePrefabsFolder);
            EnsureFolder(EnemiesDataFolder);
            EnsureFolder(AnimationsFolder);
            EnsureFolder(MaterialsFolder);
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string leaf = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        // -------------------------------------------------------------------
        // Animator Controllers
        // -------------------------------------------------------------------

        static AnimatorController CreatePlayerAnimatorController()
        {
            var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(PlayerAnimCtrlPath);
            if (ctrl == null) ctrl = AnimatorController.CreateAnimatorControllerAtPath(PlayerAnimCtrlPath);

            // Reset parameters to canonical set
            for (int i = ctrl.parameters.Length - 1; i >= 0; i--) ctrl.RemoveParameter(i);
            ctrl.AddParameter("Speed", AnimatorControllerParameterType.Float);
            ctrl.AddParameter("Attack_1", AnimatorControllerParameterType.Trigger);
            ctrl.AddParameter("Skill_Active_1", AnimatorControllerParameterType.Trigger);
            ctrl.AddParameter("Skill_Cast_Long", AnimatorControllerParameterType.Trigger);
            ctrl.AddParameter("Death", AnimatorControllerParameterType.Trigger);
            return ctrl;
        }

        static AnimatorController CreateEnemyAnimatorController()
        {
            var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(EnemyAnimCtrlPath);
            if (ctrl == null) ctrl = AnimatorController.CreateAnimatorControllerAtPath(EnemyAnimCtrlPath);

            for (int i = ctrl.parameters.Length - 1; i >= 0; i--) ctrl.RemoveParameter(i);
            ctrl.AddParameter("Speed", AnimatorControllerParameterType.Float);
            ctrl.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
            ctrl.AddParameter("Death", AnimatorControllerParameterType.Trigger);
            return ctrl;
        }

        // -------------------------------------------------------------------
        // Materials (URP Lit)
        // -------------------------------------------------------------------

        static Material CreateUrpLitMaterial(string path, Color color)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                if (existing.HasProperty("_BaseColor")) existing.SetColor("_BaseColor", color);
                EditorUtility.SetDirty(existing);
                return existing;
            }

            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            var mat = new Material(shader) { name = Path.GetFileNameWithoutExtension(path) };
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            else mat.color = color;
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        // -------------------------------------------------------------------
        // Player prefab (Kai melee, Lyra ranged)
        // -------------------------------------------------------------------

        static GameObject BuildPlayerPrefab(string path, string charName, bool isMelee, Material mat, AnimatorController ctrl)
        {
            // Build in-memory hierarchy then save as prefab
            var root = new GameObject(charName);
            root.tag = "Player";

            var cc = root.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.4f;
            cc.center = new Vector3(0f, 0.9f, 0f);

            var loco = root.AddComponent<CharacterLocomotion>();

            root.AddComponent<PlayerHealth>();
            root.AddComponent<TargetSelector>();
            root.AddComponent<PlayerFacing>();

            // Visual mesh child (capsule)
            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Visual";
            Object.DestroyImmediate(visual.GetComponent<CapsuleCollider>()); // visual only — no collider
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            visual.GetComponent<MeshRenderer>().sharedMaterial = mat;

            // Muzzle point (used by both melee and ranged — chest height, slightly forward)
            var muzzle = new GameObject("MuzzlePoint");
            muzzle.transform.SetParent(root.transform, false);
            muzzle.transform.localPosition = new Vector3(0f, 1.2f, 0.4f);

            // Animator (on root so DynamicAnimationEventHub on root receives events)
            var animator = root.AddComponent<Animator>();
            animator.runtimeAnimatorController = ctrl;
            animator.applyRootMotion = false;

            // DynamicAnimationEventHub on root — Animation Events from clips call hub.Trigger("...")
            root.AddComponent<DynamicAnimationEventHub>();

            // Combat component + concrete player class
            if (isMelee)
            {
                var combat = root.AddComponent<MeleeCombat>();
                var player = root.AddComponent<MeleePlayer>();

                // Hitbox child with SphereCollider trigger + HitboxRelay
                var hitboxGo = new GameObject("Hitbox");
                hitboxGo.transform.SetParent(root.transform, false);
                hitboxGo.transform.localPosition = new Vector3(0f, 1.0f, 0.9f);
                var sphere = hitboxGo.AddComponent<SphereCollider>();
                sphere.isTrigger = true;
                sphere.radius = 1.2f;
                sphere.enabled = false; // OnHitboxEnable toggles it on during attack window

                var relay = hitboxGo.AddComponent<HitboxRelay>();
                relay.Bind(combat);

                // Wire MeleeCombat.hitbox + MeleePlayer.meleeCombat via SerializedObject
                SetObjectRef(combat, "hitbox", sphere);
                SetObjectRef(player, "meleeCombat", combat);
            }
            else
            {
                var combat = root.AddComponent<RangedCombat>();
                var player = root.AddComponent<RangedPlayer>();
                SetObjectRef(player, "rangedCombat", combat);
            }

            // Wire PlayerBase common refs
            var basePlayer = root.GetComponent<PlayerBase>();
            SetObjectRef(basePlayer, "muzzlePoint", muzzle.transform);
            SetObjectRef(basePlayer, "animator", animator);

            // Save as prefab
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        // -------------------------------------------------------------------
        // Swarmer prefab
        // -------------------------------------------------------------------

        static GameObject BuildSwarmerPrefab(string path, Material mat, AnimatorController ctrl)
        {
            var root = new GameObject("Swarmer");
            root.layer = EnemyLayer;

            // Body collider — used by player attacks (TargetSelector OverlapSphere + MeleeCombat hitbox)
            // NOT a trigger so OverlapSphere with QueryTriggerInteraction.Ignore still finds it.
            var body = root.AddComponent<CapsuleCollider>();
            body.height = 1.4f;
            body.radius = 0.4f;
            body.center = new Vector3(0f, 0.7f, 0f);
            body.isTrigger = false;

            root.AddComponent<SwarmerEnemy>();
            root.AddComponent<DynamicAnimationEventHub>();

            // Visual child (capsule, no collider)
            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Visual";
            Object.DestroyImmediate(visual.GetComponent<CapsuleCollider>());
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            visual.transform.localScale = new Vector3(0.8f, 0.7f, 0.8f); // slightly smaller than player
            visual.GetComponent<MeshRenderer>().sharedMaterial = mat;

            // Animator on root
            var animator = root.AddComponent<Animator>();
            animator.runtimeAnimatorController = ctrl;
            animator.applyRootMotion = false;

            var swarmer = root.GetComponent<SwarmerEnemy>();
            SetObjectRef(swarmer, "animator", animator);

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        // -------------------------------------------------------------------
        // Arrow prefab
        // -------------------------------------------------------------------

        static GameObject BuildArrowPrefab(string path, Material mat)
        {
            var root = new GameObject("Arrow");
            root.layer = ProjectileLayer;

            var rb = root.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            var col = root.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 0.15f;

            root.AddComponent<Projectile>();

            // Visual child (small stretched cube as arrow shaft)
            var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Visual";
            Object.DestroyImmediate(visual.GetComponent<BoxCollider>());
            visual.transform.SetParent(root.transform, false);
            visual.transform.localScale = new Vector3(0.08f, 0.08f, 0.5f);
            visual.GetComponent<MeshRenderer>().sharedMaterial = mat;

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        // -------------------------------------------------------------------
        // EnemyDataSO
        // -------------------------------------------------------------------

        static void BuildSwarmerData(string path, GameObject prefab)
        {
            var so = AssetDatabase.LoadAssetAtPath<EnemyDataSO>(path);
            if (so == null)
            {
                so = ScriptableObject.CreateInstance<EnemyDataSO>();
                AssetDatabase.CreateAsset(so, path);
            }

            so.enemyId = "swarmer";
            so.nameKey = "enemy.swarmer.name";
            so.descKey = "enemy.swarmer.desc";
            so.maxHP = 30f;
            so.attackPower = 5f;
            so.moveSpeed = 4f;
            so.attackRange = 0.8f;
            so.attackCooldown = 1.2f;
            so.xpReward = 1f;
            so.prefab = prefab;
            so.poolKey = "Enemy_Swarmer";
            EditorUtility.SetDirty(so);

            // Wire Swarmer prefab's data field
            if (prefab != null)
            {
                var swarmer = prefab.GetComponent<SwarmerEnemy>();
                if (swarmer != null)
                {
                    SetObjectRef(swarmer, "data", so);
                    EditorUtility.SetDirty(prefab);
                }
            }
        }

        // -------------------------------------------------------------------
        // CharacterDataSO ↔ player prefab wiring
        // -------------------------------------------------------------------

        static void WirePlayerDataPrefab(string dataPath, GameObject prefab)
        {
            var so = AssetDatabase.LoadAssetAtPath<CharacterDataSO>(dataPath);
            if (so == null)
            {
                Debug.LogWarning($"[Sprint2Setup] {dataPath} missing — run Sprint 1 setup first.");
                return;
            }
            so.characterPrefab = prefab;
            EditorUtility.SetDirty(so);
        }

        // -------------------------------------------------------------------
        // SerializedObject field setter (works for any [SerializeField] / public field)
        // -------------------------------------------------------------------

        static void SetObjectRef(Object target, string fieldName, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(fieldName);
            if (prop != null)
            {
                prop.objectReferenceValue = value;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            else
            {
                Debug.LogWarning($"[Sprint2Setup] '{fieldName}' not found on {target.GetType().Name}");
            }
        }
    }
}
#endif
