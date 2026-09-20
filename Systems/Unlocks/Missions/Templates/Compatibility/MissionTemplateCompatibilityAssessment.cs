using System.Collections.Generic;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Resultado runtime-only de G6 Phase 1 para una familia de misión.
    ///
    /// La puntuación se basa principalmente en el kit real del target. El
    /// ChallengeTier sólo matiza el encaje y nunca elige la familia por sí
    /// solo. Para unlocks de survivor, el target queda explícitamente fuera
    /// de la lista de performers para evitar misiones imposibles que exijan
    /// usar al personaje todavía bloqueado.
    /// </summary>
    public sealed class MissionTemplateCompatibilityAssessment
    {
        public int ResolverVersion
        {
            get;
            set;
        } = MissionTemplateCompatibilityResolver.ResolverVersion;

        public string SurvivorId
        {
            get;
            set;
        } = "";

        public string BodyName
        {
            get;
            set;
        } = "";

        public string DisplayName
        {
            get;
            set;
        } = "";

        public bool IsModded
        {
            get;
            set;
        }

        public bool HasOriginalUnlock
        {
            get;
            set;
        }

        public bool LacksOriginalUnlock
        {
            get;
            set;
        }

        public MissionTemplateFamily Family
        {
            get;
            set;
        } = MissionTemplateFamily.Unknown;

        public float ThemeScore
        {
            get;
            set;
        }

        public float PerformerFeasibilityScore
        {
            get;
            set;
        }

        public float TierFitScore
        {
            get;
            set;
        }

        public float CompatibilityScore
        {
            get;
            set;
        }

        public MissionTemplateCompatibilityLevel CompatibilityLevel
        {
            get;
            set;
        } = MissionTemplateCompatibilityLevel.Incompatible;

        public ChallengeTier ChallengeTier
        {
            get;
            set;
        } = ChallengeTier.Tier3;

        public GameplayComplexityLevel GameplayComplexity
        {
            get;
            set;
        } = GameplayComplexityLevel.Level3;

        /// <summary>
        /// Siempre false para templates de unlock de survivor. La misión
        /// futura debe poder completarse antes de poseer al target.
        /// </summary>
        public bool RequiresTargetUsage
        {
            get;
            set;
        }

        public int AlternativePerformerCount
        {
            get;
            set;
        }

        public int UnlockedAlternativePerformerCount
        {
            get;
            set;
        }

        public List<string> SupportingSignals
        {
            get;
        } = new List<string>();

        public List<string> SupportingSkillIds
        {
            get;
        } = new List<string>();
    }
}
