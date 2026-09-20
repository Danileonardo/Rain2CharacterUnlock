using System.Collections.Generic;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Perfil cuantitativo de una habilidad instalado en runtime.
    ///
    /// Todos los valores son diagnósticos. No crean requisitos de desbloqueo,
    /// no cambian la SkillDef y no se persisten.
    /// </summary>
    public sealed class SkillMagnitudeProfile
    {
        public string SkillStableId
        {
            get;
            set;
        } = "";

        public SurvivorContentSkillSlot Slot
        {
            get;
            set;
        } = SurvivorContentSkillSlot.Extra;

        public string DisplayName
        {
            get;
            set;
        } = "";


        public MissionSkillUsageKind UsageKind
        {
            get;
            set;
        } = MissionSkillUsageKind.Active;

        public bool ContributesToAggregate
        {
            get;
            set;
        } = true;

        public float RechargeInterval
        {
            get;
            set;
        }

        public int MaxStock
        {
            get;
            set;
        }

        public float BaseDamagePercent
        {
            get;
            set;
        }

        public float PeakDamagePercent
        {
            get;
            set;
        }

        public float DamageMultiplier
        {
            get;
            set;
        } = 1f;

        public int HitCount
        {
            get;
            set;
        } = 1;

        public float HealingPercent
        {
            get;
            set;
        }

        public float BarrierPercent
        {
            get;
            set;
        }

        public float MaxHealthDamagePercent
        {
            get;
            set;
        }

        public float InvulnerabilitySeconds
        {
            get;
            set;
        }

        public float InvisibilitySeconds
        {
            get;
            set;
        }

        public float CrowdControlSeconds
        {
            get;
            set;
        }

        public float ResourceGain
        {
            get;
            set;
        }

        public float ResourceCost
        {
            get;
            set;
        }

        public float ScalingPerTargetPercent
        {
            get;
            set;
        }

        public float ScalingPerStackPercent
        {
            get;
            set;
        }

        public float AttackSpeedBonusPercent
        {
            get;
            set;
        }

        public float MoveSpeedBonusPercent
        {
            get;
            set;
        }

        public float ArmorBonusFlat
        {
            get;
            set;
        }

        public float ArmorReductionFlat
        {
            get;
            set;
        }

        public float DamageReductionPercent
        {
            get;
            set;
        }

        public float CriticalLeverage
        {
            get;
            set;
        }

        public float ExecutionLeverage
        {
            get;
            set;
        }

        public float CooldownLeverage
        {
            get;
            set;
        }

        public float AreaLeverage
        {
            get;
            set;
        }

        /// <summary>
        /// Bonificación genérica por interacción entre señales de la misma
        /// habilidad (por ejemplo daño + crítico + execute, o daño + reset).
        /// No identifica survivors concretos.
        /// </summary>
        public float SynergyLeverage
        {
            get;
            set;
        }

        /// <summary>
        /// Contribución escalar cruda. Sólo se compara contra la mediana
        /// vanilla runtime; no es un tier ni un porcentaje de poder.
        /// </summary>
        public float RawPowerContribution
        {
            get;
            set;
        }

        public float Confidence
        {
            get;
            set;
        }

        public bool HasQuantitativeSignal
        {
            get;
            set;
        }

        public List<SkillMagnitudeSignal> Signals
        {
            get;
        } = new List<SkillMagnitudeSignal>();
    }
}
