using System;
using System.Collections.Generic;

using BepInEx.Logging;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Registro runtime-only de G7 Phase 1.
    ///
    /// Guarda candidatos temporales y supresiones por contenido autorado.
    /// No persiste, no asigna provider y no materializa MissionDefinition.
    /// </summary>
    public static class DeterministicMissionGeneratorService
    {
        private static readonly List<GeneratedMissionCandidate>
            Candidates =
                new List<GeneratedMissionCandidate>();

        private static readonly List<GeneratedMissionSuppression>
            Suppressions =
                new List<GeneratedMissionSuppression>();

        private static readonly Dictionary<string, GeneratedMissionCandidate>
            CandidatesBySurvivorId =
                new Dictionary<string, GeneratedMissionCandidate>(
                    StringComparer.OrdinalIgnoreCase
                );

        private static ManualLogSource logger;

        public static int Count => Candidates.Count;


        public static void Initialize(
            IReadOnlyList<SurvivorInfo> survivors,
            IReadOnlyList<MissionTemplateCompatibilityAssessment> assessments,
            ManualLogSource activeLogger
        )
        {
            logger = activeLogger;
            Rebuild(survivors, assessments);
        }


        public static void Rebuild(
            IReadOnlyList<SurvivorInfo> survivors,
            IReadOnlyList<MissionTemplateCompatibilityAssessment> assessments
        )
        {
            Candidates.Clear();
            Suppressions.Clear();
            CandidatesBySurvivorId.Clear();

            Dictionary<string, List<MissionTemplateCompatibilityAssessment>> grouped =
                GroupEligibleTargets(assessments);

            foreach (
                KeyValuePair<string, List<MissionTemplateCompatibilityAssessment>> pair
                in grouped
            )
            {
                List<MissionTemplateCompatibilityAssessment> targetAssessments =
                    pair.Value;

                if (targetAssessments == null || targetAssessments.Count == 0)
                {
                    continue;
                }

                MissionTemplateCompatibilityAssessment first =
                    targetAssessments[0];

                string authorityDetail;

                GeneratedMissionAuthorityKind authorityKind =
                    GeneratedMissionAuthorityProbe.Detect(
                        survivors,
                        first.SurvivorId,
                        first.BodyName,
                        out authorityDetail
                    );

                if (authorityKind != GeneratedMissionAuthorityKind.None)
                {
                    Suppressions.Add(
                        new GeneratedMissionSuppression
                        {
                            SurvivorId = first.SurvivorId,
                            BodyName = first.BodyName,
                            DisplayName = first.DisplayName,
                            AuthorityKind = authorityKind,
                            Detail = authorityDetail
                        }
                    );

                    continue;
                }

                GeneratedMissionCandidate candidate =
                    DeterministicMissionGenerator.GenerateForSurvivor(
                        targetAssessments
                    );

                if (candidate == null)
                {
                    continue;
                }

                Candidates.Add(candidate);
                CandidatesBySurvivorId[candidate.SurvivorId] = candidate;
            }

            Candidates.Sort(
                delegate(
                    GeneratedMissionCandidate a,
                    GeneratedMissionCandidate b
                )
                {
                    return string.Compare(
                        a?.DisplayName,
                        b?.DisplayName,
                        StringComparison.CurrentCultureIgnoreCase
                    );
                }
            );

            Suppressions.Sort(
                delegate(
                    GeneratedMissionSuppression a,
                    GeneratedMissionSuppression b
                )
                {
                    return string.Compare(
                        a?.DisplayName,
                        b?.DisplayName,
                        StringComparison.CurrentCultureIgnoreCase
                    );
                }
            );

            LogSummary(grouped.Count);
            LogVerbose();
        }


        public static IReadOnlyList<GeneratedMissionCandidate> GetAll()
        {
            return Candidates;
        }


        public static IReadOnlyList<GeneratedMissionSuppression> GetSuppressions()
        {
            return Suppressions;
        }


        public static bool TryGetBySurvivorId(
            string survivorId,
            out GeneratedMissionCandidate candidate
        )
        {
            candidate = null;

            if (string.IsNullOrWhiteSpace(survivorId))
            {
                return false;
            }

            return CandidatesBySurvivorId.TryGetValue(
                survivorId,
                out candidate
            );
        }


        private static Dictionary<string, List<MissionTemplateCompatibilityAssessment>>
            GroupEligibleTargets(
                IReadOnlyList<MissionTemplateCompatibilityAssessment> assessments
            )
        {
            Dictionary<string, List<MissionTemplateCompatibilityAssessment>> grouped =
                new Dictionary<string, List<MissionTemplateCompatibilityAssessment>>(
                    StringComparer.OrdinalIgnoreCase
                );

            if (assessments == null)
            {
                return grouped;
            }

            for (int i = 0; i < assessments.Count; i++)
            {
                MissionTemplateCompatibilityAssessment assessment =
                    assessments[i];

                if (
                    assessment == null ||
                    !assessment.IsModded ||
                    !assessment.LacksOriginalUnlock ||
                    string.IsNullOrWhiteSpace(assessment.SurvivorId)
                )
                {
                    continue;
                }

                List<MissionTemplateCompatibilityAssessment> list;

                if (!grouped.TryGetValue(assessment.SurvivorId, out list))
                {
                    list =
                        new List<MissionTemplateCompatibilityAssessment>();

                    grouped[assessment.SurvivorId] = list;
                }

                list.Add(assessment);
            }

            return grouped;
        }


        private static void LogSummary(
            int consideredTargets
        )
        {
            int customSuppressions = 0;
            int officialSuppressions = 0;
            int existingSuppressions = 0;

            for (int i = 0; i < Suppressions.Count; i++)
            {
                GeneratedMissionSuppression suppression = Suppressions[i];

                if (suppression == null)
                {
                    continue;
                }

                switch (suppression.AuthorityKind)
                {
                    case GeneratedMissionAuthorityKind.UsuCustom:
                        customSuppressions++;
                        break;

                    case GeneratedMissionAuthorityKind.UsuOfficialPreset:
                        officialSuppressions++;
                        break;

                    default:
                        existingSuppressions++;
                        break;
                }
            }

            logger?.LogInfo(
                "[MISSION GENERATOR] G7 Phase 1.1 construido | " +
                $"Targets considered: {consideredTargets} | " +
                $"Temporary generated: {Candidates.Count} | " +
                $"Authored suppressed: {Suppressions.Count} | " +
                $"Official: {officialSuppressions} | " +
                $"Custom: {customSuppressions} | " +
                $"Existing authored: {existingSuppressions} | " +
                "Deterministic: yes | Weighted: yes | " +
                "Signature-aware: yes | Target usage: no | " +
                "Authored mission priority: yes | " +
                "Generated fallback only: yes | " +
                "Authority source: persisted+official-registry | " +
                "Assignment: no | Persistencia: no"
            );
        }


        private static void LogVerbose()
        {
            if (!UsuLog.VerboseEnabled)
            {
                return;
            }

            for (int i = 0; i < Candidates.Count; i++)
            {
                GeneratedMissionCandidate candidate = Candidates[i];

                if (candidate == null)
                {
                    continue;
                }

                GeneratedMissionParameterPlan plan =
                    candidate.Parameters ?? new GeneratedMissionParameterPlan();

                UsuLog.Verbose(
                    logger,
                    "[GENERATED MISSION] " +
                    candidate.DisplayName +
                    " | Id: " +
                    candidate.CandidateId +
                    " | Template: " +
                    candidate.TemplateId +
                    " | Family: " +
                    candidate.Family +
                    " | Tier: " +
                    (int)candidate.ChallengeTier +
                    " | Complexity: " +
                    (int)candidate.GameplayComplexity +
                    " | Compatibility: " +
                    Format(candidate.CompatibilityScore) +
                    " | Signature: " +
                    Format(candidate.DistinctivenessScore) +
                    " | Availability: " +
                    Format(candidate.PerformerAvailabilityScore) +
                    " | Selection: " +
                    Format(candidate.SelectionScore) +
                    " | AltPerformers: " +
                    candidate.AlternativePerformerCount +
                    " | UnlockedAlt: " +
                    candidate.UnlockedAlternativePerformerCount +
                    " | Objective: " +
                    plan.ObjectiveKind +
                    " | Amount: " +
                    plan.Amount +
                    " | Repeat: " +
                    plan.RepeatCount +
                    " | Time: " +
                    Format(plan.TimeLimitSeconds) +
                    "s | Reset: " +
                    plan.ResetScope +
                    " | PerformerPolicy: " +
                    plan.PerformerPolicy +
                    " | Seed: " +
                    candidate.SeedHash.ToString("x8") +
                    " | Fingerprint: " +
                    candidate.ProfileFingerprint +
                    " | Temporary: yes | FallbackOnly: yes | " +
                    "AuthoredOverride: yes | TargetUsage: no"
                );
            }

            for (int i = 0; i < Suppressions.Count; i++)
            {
                GeneratedMissionSuppression suppression = Suppressions[i];

                if (suppression == null)
                {
                    continue;
                }

                UsuLog.Verbose(
                    logger,
                    "[GENERATED SUPPRESSED] " +
                    suppression.DisplayName +
                    " | Id: " +
                    suppression.SurvivorId +
                    " | Authority: " +
                    suppression.AuthorityKind +
                    " | Detail: " +
                    suppression.Detail +
                    " | Reason: authored USU mission overrides temporary Generated"
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
