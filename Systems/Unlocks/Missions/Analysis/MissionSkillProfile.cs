using System.Collections.Generic;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Snapshot diagnóstico de una SkillDef instalada.
    ///
    /// Contiene únicamente datos derivados del runtime actual. No se serializa
    /// en Survivors.json y no reemplaza SurvivorContentSkillVariant.
    /// </summary>
    public sealed class MissionSkillProfile
    {
        public string StableId
        {
            get;
            set;
        } = "";

        public SurvivorContentSkillSlot Slot
        {
            get;
            set;
        } = SurvivorContentSkillSlot.Extra;

        public int VariantIndex
        {
            get;
            set;
        }

        public bool IsDefaultVariant
        {
            get;
            set;
        }

        public string InternalName
        {
            get;
            set;
        } = "";

        public string DisplayName
        {
            get;
            set;
        } = "";

        public string Description
        {
            get;
            set;
        } = "";

        public string NameToken
        {
            get;
            set;
        } = "";

        public string SkillDefType
        {
            get;
            set;
        } = "";

        public bool HasUnlockRequirement
        {
            get;
            set;
        }

        public bool IsUnlocked
        {
            get;
            set;
        }

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

        public int RequiredStock
        {
            get;
            set;
        }

        public int StockToConsume
        {
            get;
            set;
        }

        public bool BeginCooldownOnSkillEnd
        {
            get;
            set;
        }

        public bool CancelSprintingOnActivation
        {
            get;
            set;
        }

        public bool ForceSprintDuringState
        {
            get;
            set;
        }

        public string ActivationStateMachineName
        {
            get;
            set;
        } = "";

        public string ActivationStateType
        {
            get;
            set;
        } = "";

        public MissionSkillUsageKind UsageKind
        {
            get;
            set;
        } = MissionSkillUsageKind.Active;

        /// <summary>
        /// Indica si esta variante puede aportar señales al perfil agregado
        /// del survivor. Opciones alternativas de loadout que no están
        /// activas simultáneamente se conservan para diagnóstico, pero no
        /// inflan las capacidades agregadas que consumirá G4.
        /// </summary>
        public bool ContributesToAggregate
        {
            get;
            set;
        } = true;

        public List<string> KeywordTokens
        {
            get;
        } = new List<string>();

        /// <summary>
        /// Nombres/tokens de estados detectados en texto o metadata runtime.
        /// Ej.: Poison, Burn, Stun, Soaked, Tangled, ArmorBreak.
        /// </summary>
        public List<string> StatusEffects
        {
            get;
        } = new List<string>();

        /// <summary>
        /// Descripción por skill de qué hace, separada de las capacidades
        /// agregadas del survivor.
        /// </summary>
        public List<MissionSkillEffectProfile> Effects
        {
            get;
        } = new List<MissionSkillEffectProfile>();

        public List<MissionProfileCapability> Capabilities
        {
            get;
        } = new List<MissionProfileCapability>();

        public List<MissionCapabilityEvidence> Evidence
        {
            get;
        } = new List<MissionCapabilityEvidence>();
    }
}
