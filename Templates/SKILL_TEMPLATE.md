// ============================================================
// SKILL TEMPLATE — Mythfall: Survivor
// ============================================================
// Copy this template để tạo skill mới. Replace placeholders.
//
// Phase pattern reference (xem SKILL_DESIGN_GUIDE.md):
// - Pattern 1: Dash Strike (movement + damage)
// - Pattern 2: Channeled Charge (buildup + release)
// - Pattern 3: Burst AoE (instant field effect)
// - Pattern 4: Buff Activation (self-buff)
// - Pattern 5: Summon Pet (persistent ally)
// ============================================================

using UnityEngine;

namespace Mythfall.Skills
{
    /// <summary>
    /// SKILL DATA — ScriptableObject với tất cả tunable parameters.
    /// Designer tweak values qua inspector, không touch code.
    /// </summary>
    [CreateAssetMenu(
        fileName = "Skill_TEMPLATE",
        menuName = "Mythfall/Skills/TEMPLATE_SKILL_NAME"
    )]
    public class TemplateSkillSO : SkillDataSO
    {
        [Header("Skill Parameters")]

        // Damage scaling (multiplicative với owner ATK)
        [Tooltip("Damage multiplier. 1 = 100% ATK, 3 = 300% ATK")]
        public float damageMultiplier = 2f;

        // Range or area
        [Tooltip("Effect range or AoE radius in meters")]
        public float effectRange = 4f;

        // Special parameters (uncomment as needed)
        // public float chargeTime = 1f;          // for Channeled Charge
        // public float dashDistance = 8f;        // for Dash Strike
        // public float dashSpeed = 16f;          // for Dash Strike
        // public float buffDuration = 5f;        // for Buff Activation
        // public float summonDuration = 8f;      // for Summon Pet
        // public GameObject summonPrefab;        // for Summon Pet

        [Header("Visual + Audio")]
        public string vfxAnticipationPoolKey = "VFX_ChargeRed";
        public string vfxImpactPoolKey = "VFX_BigBurst";
        public string sfxAnticipationKey = "sfx_skill_charge";
        public string sfxImpactKey = "sfx_skill_impact";

        [Header("Camera Effect")]
        public float screenShakeIntensity = 0.5f;
        public float screenShakeDuration = 0.2f;
        public float hitstopDuration = 0.08f;

        // Đảm bảo set defaults trong OnEnable
        void OnEnable()
        {
            skillType = SkillType.Active;
            cooldown = 12f;
            duration = 0.7f;          // total animation duration
            animationTrigger = "Skill_Active_1";  // match Animator trigger
        }

        public override ISkillExecution CreateExecution(SkillContext context)
        {
            return new TemplateSkillExecution(this, context);
        }
    }


    /// <summary>
    /// SKILL EXECUTION — Runtime logic, follows phase pattern.
    /// Implements ISkillExecution để PlayerSkillManager call.
    /// </summary>
    public class TemplateSkillExecution : ISkillExecution
    {
        // Phase definitions — adjust durations theo skill design
        const float ANTICIPATION_END = 0.2f;  // 0.0-0.2s windup
        const float EXECUTION_END = 0.4f;     // 0.2-0.4s active hit
        const float RESOLUTION_END = 0.7f;    // 0.4-0.7s recovery

        readonly TemplateSkillSO data;
        readonly SkillContext ctx;

        float elapsed;
        bool finished;
        bool anticipationFired;
        bool executionFired;

        // Track hit enemies (avoid double-hit in single skill)
        readonly System.Collections.Generic.HashSet<EnemyBase> hitThisSkill = new();

        public bool IsFinished => finished;

        public TemplateSkillExecution(TemplateSkillSO data, SkillContext ctx)
        {
            this.data = data;
            this.ctx = ctx;
        }

        public bool CanExecute()
        {
            // Add custom prerequisites here, e.g.:
            // - Has target?
            //   if (ctx.TargetSelector.CurrentTarget == null) return false;
            // - Stat requirement?
            //   if (ctx.Stats.GetFinal(StatType.Mana) < 10) return false;
            return true;
        }

        public void Execute()
        {
            // Trigger animation
            ctx.Owner.GetComponent<Animator>()?.SetTrigger(data.animationTrigger);

            // Anticipation phase: start charge VFX + SFX
            Bill.Pool.Spawn(data.vfxAnticipationPoolKey,
                ctx.Owner.transform.position, Quaternion.identity);
            Bill.Audio.Play(data.sfxAnticipationKey);

            // Lock player rotation during cast
            ctx.Owner.GetComponent<PlayerFacing>()?.LockRotation(true);

            // Optional: lock movement
            // ctx.Owner.GetComponent<CharacterLocomotion>().enabled = false;

            elapsed = 0f;
            finished = false;
            hitThisSkill.Clear();
        }

        public void Tick(float dt)
        {
            elapsed += dt;

            if (elapsed < ANTICIPATION_END)
            {
                TickAnticipation(dt);
            }
            else if (elapsed < EXECUTION_END)
            {
                if (!executionFired)
                {
                    TriggerExecution();
                    executionFired = true;
                }
                TickExecution(dt);
            }
            else if (elapsed < RESOLUTION_END)
            {
                TickResolution(dt);
            }
            else
            {
                FinishSkill();
            }
        }

        // PHASE 1 — Anticipation (windup, charge buildup)
        void TickAnticipation(float dt)
        {
            // Optional per-frame logic during windup
            // E.g., expand charge VFX size:
            // chargeVFX.transform.localScale = Vector3.one * (elapsed / ANTICIPATION_END);
        }

        // PHASE 2 — Execution (skill lands)
        void TriggerExecution()
        {
            // Spawn impact VFX
            Bill.Pool.Spawn(data.vfxImpactPoolKey,
                ctx.Owner.transform.position + ctx.Owner.transform.forward * 2f,
                Quaternion.identity);

            // Audio peak
            Bill.Audio.Play(data.sfxImpactKey);

            // Camera reactions
            Bill.Events.Fire(new ScreenShakeEvent {
                intensity = data.screenShakeIntensity,
                duration = data.screenShakeDuration
            });

            // Hitstop trigger via Bill.Events nếu HitStopController subscribed
            // (Hoặc tự apply Time.timeScale here)

            // Apply skill effect
            ApplySkillEffect();
        }

        void TickExecution(float dt)
        {
            // Continuous damage during execution phase
            // E.g., for sweep attack continually checking hitbox
        }

        void ApplySkillEffect()
        {
            // CUSTOMIZE THIS based on skill type:

            // === Pattern: Burst AoE ===
            var hits = Physics.OverlapSphere(
                ctx.Owner.transform.position,
                data.effectRange,
                LayerMask.GetMask("Enemy"));

            foreach (var hit in hits)
            {
                var enemy = hit.GetComponent<EnemyBase>();
                if (enemy == null || hitThisSkill.Contains(enemy)) continue;
                hitThisSkill.Add(enemy);

                // Calculate damage
                float baseDmg = ctx.Stats.GetFinal(StatType.AttackPower);
                float critRate = ctx.Stats.GetFinal(StatType.CritRate);
                float critDmg = ctx.Stats.GetFinal(StatType.CritDamage);

                bool isCrit = Random.Range(0f, 100f) < critRate;
                float finalDmg = baseDmg * data.damageMultiplier;
                if (isCrit) finalDmg *= (critDmg / 100f);

                enemy.TakeDamage(finalDmg, ctx.Owner);

                // Fire hit event for VFX/audio/numbers
                Bill.Events.Fire(new EnemyHitEvent {
                    attacker = ctx.Owner,
                    victim = enemy,
                    damage = finalDmg,
                    isCrit = isCrit,
                    hitPoint = enemy.transform.position
                });
            }

            // === Pattern: Dash Strike (movement) ===
            // var charCtrl = ctx.Owner.GetComponent<CharacterController>();
            // var forward = ctx.Owner.transform.forward;
            // charCtrl.Move(forward * data.dashSpeed * dt);
            // // Damage enemies trên path

            // === Pattern: Buff Activation ===
            // ctx.Stats.AddModifier(StatType.AttackPower, 0.5f, ModifierType.Multiplicative);
            // Bill.Timer.Delay(data.buffDuration, () => {
            //     ctx.Stats.RemoveModifier(StatType.AttackPower, 0.5f, ModifierType.Multiplicative);
            // });

            // === Pattern: Summon Pet ===
            // var pet = Bill.Pool.Spawn(petPoolKey, ctx.Owner.transform.position, Quaternion.identity);
            // pet.GetComponent<PetController>().Initialize(ctx.Owner, data.summonDuration);
        }

        // PHASE 3 — Resolution (recovery)
        void TickResolution(float dt)
        {
            // Wind-down logic (e.g., fade out VFX)
        }

        void FinishSkill()
        {
            finished = true;

            // Restore player state
            ctx.Owner.GetComponent<PlayerFacing>()?.LockRotation(false);
            // ctx.Owner.GetComponent<CharacterLocomotion>().enabled = true;
        }
    }
}


// ============================================================
// USAGE INSTRUCTIONS
// ============================================================
//
// 1. Copy this file, rename:
//    - Class names: TemplateSkillSO → BerserkerRushSO (etc)
//    - Filename: TemplateSkill.cs → BerserkerRushSO.cs
//
// 2. Customize parameters in [Header] sections
//
// 3. Choose pattern in ApplySkillEffect():
//    - Burst AoE (default)
//    - Dash Strike
//    - Buff Activation
//    - Summon Pet
//    - Custom hybrid
//
// 4. Adjust phase timings:
//    - ANTICIPATION_END
//    - EXECUTION_END
//    - RESOLUTION_END
//
// 5. Assign VFX/SFX pool keys (must register pools first)
//
// 6. Create asset:
//    Right-click in Project → Create → Mythfall → Skills → Your Skill
//    Save trong Resources/Skills/
//
// 7. Assign to character:
//    Open CharacterDataSO asset
//    Drag skill into appropriate slot (autoAttack/active/passive)
//
// 8. Test:
//    Play scene, trigger skill, verify all phases visible
//
// 9. Polish (xem SKILL_DESIGN_GUIDE.md):
//    - All 7 layers active (anim, hitbox, number, flash, VFX, audio, camera)
//    - Tune timing for "feel"
//    - Iterate based on playtest
//
// ============================================================
