namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Consenso comunitario agregado para un survivor.
    /// </summary>
    public sealed class CommunityPowerAssessment
    {
        public string SurvivorId
        {
            get;
            set;
        } = "";

        public CommunityPowerTier Tier
        {
            get;
            set;
        } = CommunityPowerTier.Unknown;

        public int SourceCount
        {
            get;
            set;
        }

        public int IndependentSourceCount
        {
            get;
            set;
        }

        /// <summary>
        /// 0..1. Aumenta con múltiples fuentes útiles y acuerdo entre ellas.
        /// </summary>
        public float Confidence
        {
            get;
            set;
        }

        /// <summary>
        /// 0..1. 1 implica fuentes alineadas; valores bajos implican debate.
        /// </summary>
        public float Consensus
        {
            get;
            set;
        }

        public bool IsKnown
        {
            get;
            set;
        }
    }
}
