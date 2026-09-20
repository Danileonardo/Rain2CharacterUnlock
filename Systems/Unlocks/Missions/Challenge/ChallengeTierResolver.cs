using System;
using System.Collections.Generic;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// G5 Phase 1.1 - Resolved Power Integration.
    ///
    /// G5 toma toda la evaluación G4, pero mantiene responsabilidades claras:
    /// - LocalPowerScore sigue siendo el ancla principal de potencia;
    /// - ResolvedPowerScore sólo introduce el ajuste secundario que G4 ya
    ///   limitó según evidencia comunitaria, confianza y consenso;
    /// - GameplayComplexity se calcula aparte y sólo matiza la dificultad;
    /// - CommunityPowerTier nunca se convierte directamente en ChallengeTier.
    ///
    /// Esto evita dos problemas:
    /// 1) que una tierlist/opinión domine el resultado local;
    /// 2) que un ajuste pequeño de comunidad cambie una banda completa sólo
    ///    por cruzar accidentalmente un umbral 85/95/105/115.
    ///
    /// IMPORTANTE:
    /// - sólo resuelve targets Survivor en Phase 1.1;
    /// - no decide si un skill/skin DEBE bloquearse;
    /// - no genera ni asigna misiones;
    /// - no modifica providers/unlockables;
    /// - no persiste resultados.
    /// </summary>
    public static class ChallengeTierResolver
    {
        public const int ResolverVersion = 2;

        private const float PowerPointsPerBand = 10f;
        private const float MaximumResolvedDelta = 10f;

        public static List<ChallengeTierAssessment> ResolveSurvivors(
            IReadOnlyList<LocalPowerAssessment> powerAssessments
        )
        {
            List<ChallengeTierAssessment> result =
                new List<ChallengeTierAssessment>();

            if (powerAssessments == null)
            {
                return result;
            }

            for (int i = 0; i < powerAssessments.Count; i++)
            {
                LocalPowerAssessment power = powerAssessments[i];

                if (power == null)
                {
                    continue;
                }

                float complexityIndex =
                    CalculateGameplayComplexityIndex(power);

                GameplayComplexityLevel complexityLevel =
                    ResolveComplexityLevel(complexityIndex);

                int localPowerBand =
                    ResolvePowerBand(power.LocalPowerScore);

                float resolvedPowerScore =
                    ResolvePowerScore(power);

                float resolvedDelta =
                    Clamp(
                        resolvedPowerScore - power.LocalPowerScore,
                        -MaximumResolvedDelta,
                        MaximumResolvedDelta
                    );

                /*
                 * La banda local permanece como ancla. El delta comunitario
                 * se expresa como fracción de banda (10 puntos = 1 banda).
                 * Así, por ejemplo, -1.7 puntos no convierte instantáneamente
                 * una banda 4 en banda 3 sólo por estar cerca del umbral 105.
                 */
                float effectivePowerIndex =
                    Clamp(
                        localPowerBand +
                        resolvedDelta / PowerPointsPerBand,
                        1f,
                        5f
                    );

                float challengeScore =
                    ResolveChallengeScore(
                        effectivePowerIndex,
                        complexityLevel,
                        power.Confidence,
                        power.SemanticCoverage
                    );

                ChallengeTier tier =
                    ResolveTier(
                        challengeScore,
                        localPowerBand
                    );

                bool hasCommunity =
                    power.CommunityPowerTier != CommunityPowerTier.Unknown &&
                    power.CommunityIndependentSourceCount >= 2;

                result.Add(
                    new ChallengeTierAssessment
                    {
                        ResolverVersion = ResolverVersion,
                        TargetKind = ChallengeTargetKind.Survivor,
                        TargetId = power.SurvivorId ?? "",
                        OwnerSurvivorId = power.SurvivorId ?? "",
                        BodyName = power.BodyName ?? "",
                        DisplayName = power.DisplayName ?? "",
                        IsModded = power.IsModded,
                        LocalPowerScore = power.LocalPowerScore,
                        ResolvedPowerScore = resolvedPowerScore,
                        PowerBand = localPowerBand,
                        EffectivePowerIndex = effectivePowerIndex,
                        CommunityPowerTier = power.CommunityPowerTier,
                        CommunitySourceCount = power.CommunitySourceCount,
                        CommunityIndependentSourceCount =
                            power.CommunityIndependentSourceCount,
                        CommunityConfidence = power.CommunityConfidence,
                        CommunityConsensus = power.CommunityConsensus,
                        CommunityAdjustment = resolvedDelta,
                        ComplexityIndex = complexityIndex,
                        GameplayComplexity = complexityLevel,
                        ChallengeScore = challengeScore,
                        ResolvedTier = tier,
                        Confidence = power.Confidence,
                        Coverage = power.SemanticCoverage,
                        ResolutionSource = hasCommunity
                            ? "G4LocalAnchor+CommunityDelta"
                            : "G4LocalAnchor"
                    }
                );
            }

            return result;
        }


        /// <summary>
        /// Índice 0..100 de complejidad estructural/mecánica.
        ///
        /// No intenta medir dificultad de ejecución perfecta ni conocimiento
        /// del jugador. Estima cuántas piezas distintas del kit deben
        /// entenderse: amplitud de skills, versatilidad, señales indirectas y
        /// cobertura real del análisis G4.
        /// </summary>
        private static float CalculateGameplayComplexityIndex(
            LocalPowerAssessment power
        )
        {
            if (power == null)
            {
                return 0f;
            }

            float skillBreadth =
                Clamp01(
                    power.AggregateSkillCount / 8f
                );

            float versatility =
                Clamp01(
                    power.VersatilityCount / 6f
                );

            float weightedIndirectSignals =
                power.HighConfidenceIndirectSignals * 1.25f +
                power.MediumConfidenceIndirectSignals * 1.00f +
                power.LowConfidenceIndirectSignals * 0.55f;

            float indirectDepth =
                Clamp01(
                    weightedIndirectSignals / 4f
                );

            float semanticCoverage =
                Clamp01(
                    power.SemanticCoverage / 100f
                );

            return Clamp(
                100f *
                (
                    0.32f * skillBreadth +
                    0.28f * versatility +
                    0.25f * indirectDepth +
                    0.15f * semanticCoverage
                ),
                0f,
                100f
            );
        }


        /// <summary>
        /// LocalPowerScore ya tiene 100 como centro vanilla. Estas bandas no
        /// son CommunityPowerTier: sólo conservan el ancla local discreta que
        /// G5 utiliza para evitar saltos bruscos por ajustes secundarios.
        /// </summary>
        private static int ResolvePowerBand(
            float localPowerScore
        )
        {
            if (localPowerScore < 85f)
            {
                return 1;
            }

            if (localPowerScore < 95f)
            {
                return 2;
            }

            if (localPowerScore < 105f)
            {
                return 3;
            }

            if (localPowerScore < 115f)
            {
                return 4;
            }

            return 5;
        }


        private static float ResolvePowerScore(
            LocalPowerAssessment power
        )
        {
            if (power == null)
            {
                return 100f;
            }

            if (power.ResolvedPowerScore > 0f &&
                !float.IsNaN(power.ResolvedPowerScore) &&
                !float.IsInfinity(power.ResolvedPowerScore))
            {
                return power.ResolvedPowerScore;
            }

            return power.LocalPowerScore;
        }


        private static GameplayComplexityLevel ResolveComplexityLevel(
            float complexityIndex
        )
        {
            if (complexityIndex < 45f)
            {
                return GameplayComplexityLevel.Level1;
            }

            if (complexityIndex < 60f)
            {
                return GameplayComplexityLevel.Level2;
            }

            if (complexityIndex < 75f)
            {
                return GameplayComplexityLevel.Level3;
            }

            if (complexityIndex < 90f)
            {
                return GameplayComplexityLevel.Level4;
            }

            return GameplayComplexityLevel.Level5;
        }


        private static float ResolveChallengeScore(
            float effectivePowerIndex,
            GameplayComplexityLevel complexityLevel,
            float confidence,
            float coverage
        )
        {
            /*
             * Potencia sigue mandando (80%). GameplayComplexity aporta 20%.
             * CommunityPowerTier no aparece aquí: su única contribución es el
             * pequeño delta que G4 ya incorporó a EffectivePowerIndex.
             */
            float blended =
                effectivePowerIndex * 0.80f +
                (int)complexityLevel * 0.20f;

            /*
             * Una lectura poco confiable se acerca suavemente al centro en
             * lugar de declarar extremos con falsa precisión. Incluso con
             * baja confianza se conserva al menos 90% de la desviación para
             * que Confidence no se convierta en un castigo de poder.
             */
            float reliability =
                Clamp01(
                    (Math.Min(confidence, coverage) - 55f) / 35f
                );

            float deviationWeight =
                0.90f + 0.10f * reliability;

            return Clamp(
                3f +
                (blended - 3f) * deviationWeight,
                1f,
                5f
            );
        }


        private static ChallengeTier ResolveTier(
            float challengeScore,
            int localPowerBand
        )
        {
            int tier =
                (int)Math.Floor(
                    challengeScore + 0.5f
                );

            /*
             * GameplayComplexity + comunidad pueden matizar el resultado, pero
             * G5 no permite que se aleje más de un nivel de la banda local.
             * LocalPower continúa siendo la referencia principal.
             */
            int minimum =
                Math.Max(1, localPowerBand - 1);

            int maximum =
                Math.Min(5, localPowerBand + 1);

            tier =
                Math.Max(
                    minimum,
                    Math.Min(maximum, tier)
                );

            tier =
                Math.Max(
                    1,
                    Math.Min(5, tier)
                );

            return (ChallengeTier)tier;
        }


        private static float Clamp01(
            float value
        )
        {
            return Clamp(value, 0f, 1f);
        }


        private static float Clamp(
            float value,
            float minimum,
            float maximum
        )
        {
            if (value < minimum)
            {
                return minimum;
            }

            if (value > maximum)
            {
                return maximum;
            }

            return value;
        }
    }
}
