using System;
using System.Collections.Generic;

using BepInEx.Logging;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Registro runtime G5 Phase 1.1.
    ///
    /// Actualmente contiene sólo targets Survivor. LocalPower es el ancla;
    /// ResolvedPower aporta únicamente el delta secundario ya calibrado por G4.
    /// La estructura queda preparada para que skills/skins tengan resolvers
    /// propios en una fase futura.
    /// </summary>
    public static class ChallengeTierService
    {
        private static readonly List<ChallengeTierAssessment>
            Assessments =
                new List<ChallengeTierAssessment>();

        private static readonly Dictionary<string, ChallengeTierAssessment>
            AssessmentsByTargetId =
                new Dictionary<string, ChallengeTierAssessment>(
                    StringComparer.OrdinalIgnoreCase
                );

        private static readonly Dictionary<string, ChallengeTierAssessment>
            AssessmentsByBody =
                new Dictionary<string, ChallengeTierAssessment>(
                    StringComparer.OrdinalIgnoreCase
                );

        private static ManualLogSource logger;

        public static int Count => Assessments.Count;


        public static void Initialize(
            IReadOnlyList<LocalPowerAssessment> powerAssessments,
            ManualLogSource activeLogger
        )
        {
            logger = activeLogger;
            Rebuild(powerAssessments);
        }


        public static void Rebuild(
            IReadOnlyList<LocalPowerAssessment> powerAssessments
        )
        {
            Assessments.Clear();
            AssessmentsByTargetId.Clear();
            AssessmentsByBody.Clear();

            List<ChallengeTierAssessment> rebuilt =
                ChallengeTierResolver.ResolveSurvivors(
                    powerAssessments
                );

            for (int i = 0; i < rebuilt.Count; i++)
            {
                ChallengeTierAssessment assessment = rebuilt[i];

                if (assessment == null)
                {
                    continue;
                }

                Assessments.Add(assessment);

                if (!string.IsNullOrWhiteSpace(assessment.TargetId))
                {
                    AssessmentsByTargetId[
                        assessment.TargetId
                    ] = assessment;
                }

                if (!string.IsNullOrWhiteSpace(assessment.BodyName))
                {
                    AssessmentsByBody[
                        assessment.BodyName
                    ] = assessment;
                }
            }

            Assessments.Sort(
                delegate(
                    ChallengeTierAssessment a,
                    ChallengeTierAssessment b
                )
                {
                    return string.Compare(
                        a?.DisplayName,
                        b?.DisplayName,
                        StringComparison.CurrentCultureIgnoreCase
                    );
                }
            );

            LogSummary();
            LogVerboseAssessments();
        }


        public static IReadOnlyList<ChallengeTierAssessment> GetAll()
        {
            return Assessments;
        }


        public static bool TryGetByTargetId(
            string targetId,
            out ChallengeTierAssessment assessment
        )
        {
            assessment = null;

            if (string.IsNullOrWhiteSpace(targetId))
            {
                return false;
            }

            return AssessmentsByTargetId.TryGetValue(
                targetId,
                out assessment
            );
        }


        public static bool TryGetByBodyName(
            string bodyName,
            out ChallengeTierAssessment assessment
        )
        {
            assessment = null;

            if (string.IsNullOrWhiteSpace(bodyName))
            {
                return false;
            }

            return AssessmentsByBody.TryGetValue(
                bodyName,
                out assessment
            );
        }


        private static void LogSummary()
        {
            int modded = 0;
            int vanilla = 0;
            int communityKnown = 0;
            int[] tierCounts = new int[6];
            int[] complexityCounts = new int[6];
            float totalAbsoluteCommunityDelta = 0f;

            for (int i = 0; i < Assessments.Count; i++)
            {
                ChallengeTierAssessment assessment = Assessments[i];

                if (assessment == null)
                {
                    continue;
                }

                if (assessment.IsModded)
                {
                    modded++;
                }
                else
                {
                    vanilla++;
                }

                if (assessment.CommunityPowerTier != CommunityPowerTier.Unknown &&
                    assessment.CommunityIndependentSourceCount >= 2)
                {
                    communityKnown++;
                }

                totalAbsoluteCommunityDelta +=
                    Math.Abs(assessment.CommunityAdjustment);

                int tier = (int)assessment.ResolvedTier;
                int complexity = (int)assessment.GameplayComplexity;

                if (tier >= 1 && tier <= 5)
                {
                    tierCounts[tier]++;
                }

                if (complexity >= 1 && complexity <= 5)
                {
                    complexityCounts[complexity]++;
                }
            }

            float averageAbsoluteCommunityDelta =
                Assessments.Count > 0
                    ? totalAbsoluteCommunityDelta / Assessments.Count
                    : 0f;

            UsuLog.Diagnostic(
                logger,
                "[CHALLENGE TIER] G5 Phase 1.1 construido | " +
                $"Targets: {Assessments.Count} | " +
                $"Survivor: {Assessments.Count} | " +
                $"Modded: {modded} | " +
                $"Vanilla reference: {vanilla} | " +
                "Tier 1/2/3/4/5: " +
                $"{tierCounts[1]}/{tierCounts[2]}/{tierCounts[3]}/" +
                $"{tierCounts[4]}/{tierCounts[5]} | " +
                "Complexity 1/2/3/4/5: " +
                $"{complexityCounts[1]}/{complexityCounts[2]}/" +
                $"{complexityCounts[3]}/{complexityCounts[4]}/" +
                $"{complexityCounts[5]} | " +
                $"Community integrated: {communityKnown}/{modded} | " +
                "Community primary: no | " +
                "LocalPower anchor: yes | " +
                "Avg |community delta|: " +
                Format(averageAbsoluteCommunityDelta) +
                " | Skill/Skin resolver: no | " +
                "Generation: no | Assignment: no | Persistencia: no"
            );
        }


        private static void LogVerboseAssessments()
        {
            if (!UsuLog.ExtendedAuditEnabled)
            {
                return;
            }

            for (int i = 0; i < Assessments.Count; i++)
            {
                ChallengeTierAssessment assessment = Assessments[i];

                if (assessment == null)
                {
                    continue;
                }

                UsuLog.ExtendedAudit(
                    logger,
                    "[CHALLENGE PROFILE] " +
                    assessment.DisplayName +
                    " | Target: " +
                    assessment.TargetKind +
                    " | Id: " +
                    assessment.TargetId +
                    " | LocalPower: " +
                    Format(assessment.LocalPowerScore) +
                    " | ResolvedPower: " +
                    Format(assessment.ResolvedPowerScore) +
                    " | CommunityDelta: " +
                    Format(assessment.CommunityAdjustment) +
                    " | PowerBand(local): " +
                    assessment.PowerBand +
                    " | EffectivePower: " +
                    Format(assessment.EffectivePowerIndex) +
                    " | Complexity: " +
                    (int)assessment.GameplayComplexity +
                    " (" +
                    Format(assessment.ComplexityIndex) +
                    ") | ChallengeScore: " +
                    Format(assessment.ChallengeScore) +
                    " | ChallengeTier: " +
                    (int)assessment.ResolvedTier +
                    " | Community: " +
                    assessment.CommunityPowerTier +
                    "(" +
                    assessment.CommunityIndependentSourceCount +
                    " independent, conf " +
                    Format(assessment.CommunityConfidence) +
                    ", consensus " +
                    Format(assessment.CommunityConsensus) +
                    ") | Confidence: " +
                    Format(assessment.Confidence) +
                    " | Coverage: " +
                    Format(assessment.Coverage) +
                    " | Source: " +
                    assessment.ResolutionSource
                );
            }
        }


        private static string Format(
            float value
        )
        {
            return value.ToString("0.0");
        }
    }
}
