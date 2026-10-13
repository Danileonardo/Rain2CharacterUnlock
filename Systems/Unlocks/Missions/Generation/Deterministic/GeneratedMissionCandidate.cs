namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Candidato producido por G7.
    ///
    /// G8 Phase 1.5 lo materializa como Mission V2 y lo persiste con
    /// Source=Generated. Sigue siendo fallback: Original/Official/Custom
    /// siempre tienen prioridad.
    /// </summary>
    public sealed class GeneratedMissionCandidate
    {
        public int GeneratorVersion
        {
            get;
            set;
        } = DeterministicMissionGenerator.GeneratorVersion;

        public string CandidateId
        {
            get;
            set;
        } = "";

        public string TemplateId
        {
            get;
            set;
        } = "";

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

        public MissionTemplateFamily Family
        {
            get;
            set;
        } = MissionTemplateFamily.Unknown;

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

        public float CompatibilityScore
        {
            get;
            set;
        }

        public float DistinctivenessScore
        {
            get;
            set;
        }

        public float PerformerAvailabilityScore
        {
            get;
            set;
        }

        public float SelectionScore
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

        public string GenerationNonce
        {
            get;
            set;
        } = "";


        public string StableSeed
        {
            get;
            set;
        } = "";

        public uint SeedHash
        {
            get;
            set;
        }

        public string ProfileFingerprint
        {
            get;
            set;
        } = "";

        public GeneratedMissionParameterPlan Parameters
        {
            get;
            set;
        } = new GeneratedMissionParameterPlan();

        /// <summary>
        /// El blueprint es temporal; G8 persiste su MissionDefinition materializada.
        /// </summary>
        public bool IsTemporary
        {
            get;
            set;
        } = true;

        /// <summary>
        /// Un contenido autorado por USU (Official/Custom) siempre gana.
        /// </summary>
        public bool IsFallbackOnly
        {
            get;
            set;
        } = true;

        public bool AuthoredMissionOverridesGenerated
        {
            get;
            set;
        } = true;

        public bool RequiresTargetUsage
        {
            get;
            set;
        }
    }
}
