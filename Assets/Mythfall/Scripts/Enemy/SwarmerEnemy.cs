using UnityEngine;
using BillGameCore;
using Mythfall.Player;

namespace Mythfall.Enemy
{
    /// <summary>
    /// Basic chase-and-melee enemy. Walks toward player; once inside attackRange,
    /// triggers Attack animation. Damage application is animation-event driven via
    /// DynamicAnimationEventHub (event "OnAttackHit" → SwarmerEnemy.OnAttackHit).
    ///
    /// Timer fallback covers placeholder Animator without animation events so combat
    /// loop is testable on capsule prefab before real anim is wired.
    /// </summary>
    public class SwarmerEnemy : EnemyBase
    {
        [Header("Refs")]
        [SerializeField] Animator animator;

        [Header("Animation event fallback")]
        [SerializeField] bool useTimerFallback = true;
        [SerializeField] float fallbackHitDelay = 0.3f;

        float attackTimer;
        bool attackPending;

        static readonly int SpeedHash = Animator.StringToHash("Speed");
        static readonly int AttackHash = Animator.StringToHash("Attack");
        static readonly int DeathHash = Animator.StringToHash("Death");

        void Awake()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
            attackTimer = 0f;
            attackPending = false;
        }

        void Update()
        {
            if (!IsAlive) return;
            if (ResolvePlayerTransform() == null) return; // wait for GameplaySpawner to spawn the player

            if (attackTimer > 0f) attackTimer -= Time.deltaTime;

            Vector3 toPlayer = playerTransform.position - transform.position;
            toPlayer.y = 0f;
            float dist = toPlayer.magnitude;
            float range = data != null ? data.attackRange : 0.8f;

            if (dist > range)
            {
                Chase(toPlayer.normalized);
            }
            else
            {
                StopMoving();
                if (attackTimer <= 0f && !attackPending) AttackPlayer();
            }
        }

        void Chase(Vector3 direction)
        {
            float speed = data != null ? data.moveSpeed : 4f;
            transform.position += direction * speed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(direction);
            if (animator != null) animator.SetFloat(SpeedHash, 1f, 0.1f, Time.deltaTime);
        }

        void StopMoving()
        {
            if (animator != null) animator.SetFloat(SpeedHash, 0f, 0.1f, Time.deltaTime);
        }

        void AttackPlayer()
        {
            attackPending = true;
            attackTimer = data != null ? data.attackCooldown : 1.2f;
            if (animator != null) animator.SetTrigger(AttackHash);

            if (useTimerFallback)
                Bill.Timer.Delay(fallbackHitDelay, OnAttackHit);
        }

        // ----- Animation event handler (UnityEvent compatible) -----

        public void OnAttackHit()
        {
            if (!attackPending) return;
            attackPending = false;

            if (!IsAlive || playerTransform == null) return;

            float range = data != null ? data.attackRange : 0.8f;
            float dist = Vector3.Distance(transform.position, playerTransform.position);
            if (dist > range + 0.5f) return; // player walked out of melee range

            var hp = playerTransform.GetComponent<PlayerHealth>();
            float dmg = data != null ? data.attackPower : 5f;
            hp?.TakeDamage(dmg);
        }

        protected override void OnDeath()
        {
            if (animator != null) animator.SetTrigger(DeathHash);
            attackPending = false;
        }
    }
}
