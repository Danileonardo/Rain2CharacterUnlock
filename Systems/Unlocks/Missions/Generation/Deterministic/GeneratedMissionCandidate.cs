namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Candidato runtime-only producido por G7 Phase 1.
    ///
    /// Es deliberadamente temporal: no cambia provider, no reemplaza una
    /// misión autorada, no escribe Survivors.json y no desbloquea nada.
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
        /// G7 Phase 1 nunca persiste este resultado.
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
