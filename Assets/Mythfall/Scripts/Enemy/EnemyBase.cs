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

            var p = GameObject.FindGameObjectWithTag(PlayerTag);
            playerTransform = p != null ? p.transform : null;
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
