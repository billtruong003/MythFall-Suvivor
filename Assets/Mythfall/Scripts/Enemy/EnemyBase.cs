using UnityEngine;
using BillGameCore;
using Mythfall.Events;
using Mythfall.Player;

namespace Mythfall.Enemy
{
    /// <summary>
    /// Common enemy contract: HP, damage intake, death routing, pool-aware spawn/despawn.
    /// Subclasses implement movement + attack behavior.
    /// </summary>
    public abstract class EnemyBase : MonoBehaviour
    {
        [SerializeField] protected EnemyDataSO data;

        protected float currentHP;
        protected Transform playerTransform;

        public bool IsAlive { get; protected set; }
        public EnemyDataSO Data => data;
        public float CurrentHP => currentHP;

        const string PlayerTag = "Player";
        const float DespawnDelay = 1f;

        protected virtual void Start()
        {
            // Scene-placed instance (e.g. dropped into GameplayScene via editor) — pool spawner
            // would have called OnSpawn already, but for editor-only test layouts we need to
            // self-init so chase/attack logic activates.
            if (!IsAlive) OnSpawn();
        }

        public virtual void OnSpawn()
        {
            if (data == null)
            {
                Debug.LogError($"[{name}] EnemyDataSO not assigned.", this);
                return;
            }

            currentHP = data.maxHP;
            IsAlive = true;
            gameObject.SetActive(true);

            // Player may not be in scene yet — GameplaySpawner is async on Bill ready
            // and may run after WaveSpawner spawns this enemy. Subclasses lazy-fetch
            // via ResolvePlayerTransform() in their Update tick.
            playerTransform = null;
        }

        /// <summary>
        /// Lazy-fetch player transform when subclasses first need it. Cached on
        /// success — subsequent calls are O(1). Called from Update tick to handle
        /// the GameplaySpawner-vs-WaveSpawner ordering race.
        /// </summary>
        protected Transform ResolvePlayerTransform()
        {
            if (playerTransform != null) return playerTransform;
            var p = GameObject.FindGameObjectWithTag(PlayerTag);
            if (p != null) playerTransform = p.transform;
            return playerTransform;
        }

        public virtual void TakeDamage(float amount, PlayerBase attacker)
        {
            if (!IsAlive || amount <= 0f) return;

            currentHP -= amount;
            if (currentHP <= 0f) Die(attacker);
        }

        protected virtual void Die(PlayerBase killer)
        {
            IsAlive = false;
            OnDeath();

            Bill.Events.Fire(new EnemyKilledEvent
            {
                enemy = this,
                killer = killer,
                position = transform.position,
            });

            // Brief delay so death anim can play before pool returns the GO
            Bill.Timer.Delay(DespawnDelay, ReturnToPool);
        }

        protected virtual void OnDeath() { }

        void ReturnToPool()
        {
            if (Bill.Pool != null) Bill.Pool.Return(gameObject);
            else gameObject.SetActive(false);
        }
    }
}
