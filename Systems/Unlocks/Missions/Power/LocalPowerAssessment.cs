using System.Collections.Generic;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Resultado diagnóstico G4 Phase 1.4 para un survivor instalado.
    ///
    /// LocalPowerScore usa 100 como referencia de la mediana vanilla de la
    /// sesión. No es un tier final y no genera/modifica ninguna misión.
    /// </summary>
    public sealed class LocalPowerAssessment
    {
        public int AssessmentVersion
        {
            get;
            set;
        } = LocalPowerAssessmentAnalyzer.AssessmentVersion;

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

        /// <summary>
        /// Índice relativo final. 100 = mediana vanilla runtime.
        /// </summary>
        public float LocalPowerScore
        {
            get;
            set;
        }

        /// <summary>
        /// Compuesto previo a la renormalización final contra vanilla.
        /// Se conserva para diagnóstico/calibración.
        /// </summary>
        public float CompositeIndex
        {
            get;
            set;
        }

        public float BodyIndex
        {
            get;
            set;
        }

        public float OffenseIndex
        {
            get;
            set;
        }

        public float SustainIndex
        {
            get;
            set;
        }

        public float DefenseIndex
        {
            get;
            set;
        }

        public float MobilityIndex
        {
            get;
            set;
        }

        public float ControlIndex
        {
            get;
            set;
        }

        public float UtilityIndex
        {
            get;
            set;
        }

        /// <summary>
        /// Índice relativo de mecánicas indirectas observadas en el kit.
        /// Ej.: daño por vida máxima, refunds de cooldown, buffs de velocidad
        /// de ataque, invulnerabilidad, acceso a arsenal o recasts.
        /// </summary>
        public float IndirectPowerIndex
        {
            get;
            set;
        }

        /// <summary>
        /// Índice relativo de magnitud cuantitativa del kit. Considera
        /// porcentajes de daño/curación, cooldown/stock, críticos, executes,
        /// ventanas defensivas, escalados y otros números observables.
        /// </summary>
        public float QuantitativePowerIndex
        {
            get;
            set;
        }

        /// <summary>
        /// Índice cuantitativo antes de aplicar neutralidad por cobertura.
        /// Se conserva para auditoría; un valor bajo con poca cobertura no se
        /// interpreta como debilidad real.
        /// </summary>
        public float QuantitativeRawIndex
        {
            get;
            set;
        }

        /// <summary>
        /// 0..100. Cuánto puede influir QuantitativeRawIndex en el compuesto.
        /// Por debajo de 35% de cobertura la magnitud desconocida es neutral.
        /// </summary>
        public float QuantitativeReliability
        {
            get;
            set;
        }

        public float QuantitativeCoverage
        {
            get;
            set;
        }

        public int QuantitativeSkillCount
        {
            get;
            set;
        }

        public int QuantitativeScoredSkillCount
        {
            get;
            set;
        }

        public List<SkillMagnitudeProfile> SkillMagnitudes
        {
            get;
        } = new List<SkillMagnitudeProfile>();

        public int IndirectSignalCount
        {
            get;
            set;
        }

        public int HighConfidenceIndirectSignals
        {
            get;
            set;
        }

        public int MediumConfidenceIndirectSignals
        {
            get;
            set;
        }

        public int LowConfidenceIndirectSignals
        {
            get;
            set;
        }

        public List<IndirectPowerSignal> IndirectSignals
        {
            get;
        } = new List<IndirectPowerSignal>();


        /// <summary>
        /// Potencia final preparada para consumidores posteriores. En 1.3.2 es
        /// LocalPowerScore más un ajuste comunitario muy limitado. G5 Phase 1
        /// todavía usa LocalPowerScore; una recalibración posterior decidirá
        /// cuándo adoptar ResolvedPowerScore.
        /// </summary>
        public float ResolvedPowerScore
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

        /// <summary>
        /// Confianza del diagnóstico local, 0..100. Combina calidad de
        /// evidencias G3 y cobertura semántica de las skills agregables.
        /// </summary>
        public float Confidence
        {
            get;
            set;
        }

        public float SemanticCoverage
        {
            get;
            set;
        }

        public int AggregateSkillCount
        {
            get;
            set;
        }

        /// <summary>
        /// Opciones Extra/LoadoutOption incluidas porque eran la variante
        /// predeterminada de su grupo. Esto conserva un loadout coherente sin
        /// sumar simultáneamente todas las alternativas desbloqueables.
        /// </summary>
        public int DefaultLoadoutOptionCount
        {
            get;
            set;
        }

        public int ScoredSkillCount
        {
            get;
            set;
        }

        public int VersatilityCount
        {
            get;
            set;
        }

        public int HighConfidenceEvidence
        {
            get;
            set;
        }

        public int MediumConfidenceEvidence
        {
            get;
            set;
        }

        public int LowConfidenceEvidence
        {
            get;
            set;
        }
    }
}
