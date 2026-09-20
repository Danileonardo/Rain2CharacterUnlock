namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Resultado diagnóstico G5 Phase 1.1.
    ///
    /// Es runtime-only. No modifica provider, unlockables, presets ni
    /// persistencia. TargetKind existe desde ahora para que la futura rama de
    /// skills/skins no tenga que reutilizar a la fuerza las reglas de
    /// survivors.
    /// </summary>
    public sealed class ChallengeTierAssessment
    {
        public int ResolverVersion
        {
            get;
            set;
        } = ChallengeTierResolver.ResolverVersion;

        public ChallengeTargetKind TargetKind
        {
            get;
            set;
        } = ChallengeTargetKind.Survivor;

        public string TargetId
        {
            get;
            set;
        } = "";

        public string OwnerSurvivorId
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

        /// <summary>
        /// Resultado puramente local de G4. Sigue siendo el ancla principal
        /// para G5 y conserva 100 como referencia vanilla runtime.
        /// </summary>
        public float LocalPowerScore
        {
            get;
            set;
        }

        /// <summary>
        /// Resultado final preparado por G4 después de aplicar, cuando existe,
        /// un ajuste comunitario limitado y ponderado por evidencia.
        /// </summary>
        public float ResolvedPowerScore
        {
            get;
            set;
        }

        /// <summary>
        /// Banda discreta 1..5 derivada sólo de LocalPowerScore. Se conserva
        /// como ancla para que un cambio pequeño de comunidad no provoque un
        /// salto brusco al cruzar un umbral.
        /// </summary>
        public int PowerBand
        {
            get;
            set;
        }

        /// <summary>
        /// Índice continuo 1..5 que parte de PowerBand y recibe únicamente el
        /// delta LocalPower -> ResolvedPower expresado en fracciones de banda.
        /// La comunidad puede matizar, nunca sustituir el análisis local.
        /// </summary>
        public float EffectivePowerIndex
        {
            get;
            set;
        }

        public CommunityPowerTier CommunityPowerTier
        {
            get;
            set;
        } = CommunityPowerTier.Unknown;

        public int CommunitySourceCount
        {
            get;
            set;
        }

        public int CommunityIndependentSourceCount
        {
            get;
            set;
        }

        public float CommunityConfidence
        {
            get;
            set;
        }

        public float CommunityConsensus
        {
            get;
            set;
        }

        public float CommunityAdjustment
        {
            get;
            set;
        }

        public float ComplexityIndex
        {
            get;
            set;
        }

        public GameplayComplexityLevel GameplayComplexity
        {
            get;
            set;
        } = GameplayComplexityLevel.Level3;

        /// <summary>
        /// Valor continuo previo al redondeo, conservado para diagnóstico.
        /// </summary>
        public float ChallengeScore
        {
            get;
            set;
        }

        public ChallengeTier ResolvedTier
        {
            get;
            set;
        } = ChallengeTier.Tier3;

        public float Confidence
        {
            get;
            set;
        }

        public float Coverage
        {
            get;
            set;
        }

        public string ResolutionSource
        {
            get;
            set;
        } = "G4LocalAnchor+ResolvedDelta";
    }
}
