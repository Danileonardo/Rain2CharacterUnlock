using System;
using System.Collections.Generic;

using BepInEx.Logging;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Registro runtime de G6 Phase 1.
    ///
    /// Mantiene el ranking de familias compatible con cada survivor, pero no
    /// crea MissionDefinition, no asigna provider y no persiste resultados.
    /// </summary>
    public static class MissionTemplateCompatibilityService
    {
        private static readonly List<MissionTemplateCompatibilityAssessment>
            Assessments =
                new List<MissionTemplateCompatibilityAssessment>();

        private static readonly Dictionary<string, List<MissionTemplateCompatibilityAssessment>>
            AssessmentsBySurvivorId =
                new Dictionary<string, List<MissionTemplateCompatibilityAssessment>>(
                    StringComparer.OrdinalIgnoreCase
                );

        private static ManualLogSource logger;

        public static int Count => Assessments.Count;


        public static void Initialize(
            IReadOnlyList<MissionProfile> profiles,
            IReadOnlyList<ChallengeTierAssessment> challengeAssessments,
            ManualLogSource activeLogger
        )
        {
            logger = activeLogger;
            Rebuild(profiles, challengeAssessments);
        }


        public static void Rebuild(
            IReadOnlyList<MissionProfile> profiles,
            IReadOnlyList<ChallengeTierAssessment> challengeAssessments
        )
        {
            Assessments.Clear();
            AssessmentsBySurvivorId.Clear();

            List<MissionTemplateCompatibilityAssessment> rebuilt =
                MissionTemplateCompatibilityResolver.Resolve(
                    profiles,
                    challengeAssessments
                );

            for (int i = 0; i < rebuilt.Count; i++)
            {
                MissionTemplateCompatibilityAssessment assessment = rebuilt[i];

                if (assessment == null)
                {
                    continue;
                }

                Assessments.Add(assessment);

                if (string.IsNullOrWhiteSpace(assessment.SurvivorId))
                {
                    continue;
                }

                List<MissionTemplateCompatibilityAssessment> survivorAssessments;

                if (!AssessmentsBySurvivorId.TryGetValue(
                    assessment.SurvivorId,
                    out survivorAssessments
                ))
                {
                    survivorAssessments =
                        new List<MissionTemplateCompatibilityAssessment>();

                    AssessmentsBySurvivorId[
                        assessment.SurvivorId
                    ] = survivorAssessments;
                }

                survivorAssessments.Add(assessment);
            }

            foreach (
                KeyValuePair<string, List<MissionTemplateCompatibilityAssessment>> pair
                in AssessmentsBySurvivorId
            )
            {
                pair.Value.Sort(CompareAssessmentDescending);
            }

            LogSummary(profiles);
            LogVerboseProfiles();
        }


        public static IReadOnlyList<MissionTemplateCompatibilityAssessment> GetAll()
        {
            return Assessments;
        }


        public static IReadOnlyList<MissionTemplateCompatibilityAssessment> GetForSurvivor(
            string survivorId
        )
        {
            if (string.IsNullOrWhiteSpace(survivorId))
            {
                return new MissionTemplateCompatibilityAssessment[0];
            }

            List<MissionTemplateCompatibilityAssessment> result;

            if (AssessmentsBySurvivorId.TryGetValue(survivorId, out result))
            {
                return result;
            }

            return new MissionTemplateCompatibilityAssessment[0];
        }


        private static void LogSummary(
            IReadOnlyList<MissionProfile> profiles
        )
        {
            int profileCount = profiles != null ? profiles.Count : 0;
            int modded = 0;
            int withoutOriginalUnlock = 0;
            int compatible = 0;
            int strong = 0;
            int preferred = 0;

            if (profiles != null)
            {
                for (int i = 0; i < profiles.Count; i++)
                {
                    MissionProfile profile = profiles[i];

                    if (profile == null || !profile.IsModded)
                    {
                        continue;
                    }

                    modded++;

                    if (!profile.HasOriginalUnlock)
                    {
                        withoutOriginalUnlock++;
                    }
                }
            }

            for (int i = 0; i < Assessments.Count; i++)
            {
                MissionTemplateCompatibilityAssessment assessment = Assessments[i];

                if (assessment == null)
                {
                    continue;
                }

                if ((int)assessment.CompatibilityLevel >=
                    (int)MissionTemplateCompatibilityLevel.Compatible)
                {
                    compatible++;
                }

                if (assessment.CompatibilityLevel ==
                    MissionTemplateCompatibilityLevel.Strong)
                {
                    strong++;
                }

                if (assessment.CompatibilityLevel ==
                    MissionTemplateCompatibilityLevel.Preferred)
                {
                    preferred++;
                }
            }

            logger?.LogInfo(
                "[MISSION COMPATIBILITY] G6 Phase 1 construido | " +
                $"Profiles: {profileCount} | " +
                $"Modded: {modded} | " +
                "Families: 16 | " +
                $"Assessments: {Assessments.Count} | " +
                $"Compatible+: {compatible} | " +
                $"Strong: {strong} | " +
                $"Preferred: {preferred} | " +
                $"Modded without original unlock: {withoutOriginalUnlock} | " +
                "Target usage: no | " +
                "Kit weight: 70% | Performer feasibility: 20% | " +
                "ChallengeTier fit: 10% | " +
                "Generation: no | Assignment: no | Persistencia: no"
            );
        }


        private static void LogVerboseProfiles()
        {
            if (!UsuLog.VerboseEnabled)
            {
                return;
            }

            foreach (
                KeyValuePair<string, List<MissionTemplateCompatibilityAssessment>> pair
                in AssessmentsBySurvivorId
            )
            {
                List<MissionTemplateCompatibilityAssessment> ranked = pair.Value;

                if (ranked == null || ranked.Count == 0)
                {
                    continue;
                }

                MissionTemplateCompatibilityAssessment first = ranked[0];

                UsuLog.Verbose(
                    logger,
                    "[MISSION COMPATIBILITY PROFILE] " +
                    first.DisplayName +
                    " | Id: " +
                    first.SurvivorId +
                    " | Tier: " +
                    (int)first.ChallengeTier +
                    " | Complexity: " +
                    (int)first.GameplayComplexity +
                    " | OriginalUnlock: " +
                    (first.HasOriginalUnlock ? "Sí" : "No") +
                    " | NoOriginalUnlock: " +
                    (first.LacksOriginalUnlock ? "Sí" : "No") +
                    " | Top: " +
                    JoinTop(ranked, 5)
                );

                int detailCount = Math.Min(5, ranked.Count);

                for (int i = 0; i < detailCount; i++)
                {
                    MissionTemplateCompatibilityAssessment assessment = ranked[i];

                    UsuLog.Verbose(
                        logger,
                        "[MISSION TEMPLATE] " +
                        assessment.DisplayName +
                        " | Family: " +
                        assessment.Family +
                        " | Score: " +
                        Format(assessment.CompatibilityScore) +
                        " | Level: " +
                        assessment.CompatibilityLevel +
                        " | Theme: " +
                        Format(assessment.ThemeScore) +
                        " | Performer: " +
                        Format(assessment.PerformerFeasibilityScore) +
                        " | TierFit: " +
                        Format(assessment.TierFitScore) +
                        " | AltPerformers: " +
                        assessment.AlternativePerformerCount +
                        " | UnlockedAlt: " +
                        assessment.UnlockedAlternativePerformerCount +
                        " | TargetUsage: no" +
                        " | Signals: " +
                        JoinStrings(assessment.SupportingSignals) +
                        " | Skills: " +
                        JoinStrings(assessment.SupportingSkillIds)
                    );
                }
            }
        }


        private static int CompareAssessmentDescending(
            MissionTemplateCompatibilityAssessment a,
            MissionTemplateCompatibilityAssessment b
        )
        {
            float aScore = a != null ? a.CompatibilityScore : 0f;
            float bScore = b != null ? b.CompatibilityScore : 0f;

            int scoreCompare = bScore.CompareTo(aScore);

            if (scoreCompare != 0)
            {
                return scoreCompare;
            }

            string aFamily = a != null ? a.Family.ToString() : "";
            string bFamily = b != null ? b.Family.ToString() : "";

            return string.Compare(
                aFamily,
                bFamily,
                StringComparison.OrdinalIgnoreCase
            );
        }


        private static string JoinTop(
            IReadOnlyList<MissionTemplateCompatibilityAssessment> ranked,
            int maximum
        )
        {
            if (ranked == null || ranked.Count == 0)
            {
                return "None";
            }

            string result = "";
            int count = Math.Min(maximum, ranked.Count);

            for (int i = 0; i < count; i++)
            {
                MissionTemplateCompatibilityAssessment assessment = ranked[i];

                if (assessment == null)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(result))
                {
                    result += ",";
                }

                result +=
                    assessment.Family +
                    "(" +
                    Format(assessment.CompatibilityScore) +
                    "/" +
                    assessment.CompatibilityLevel +
                    ")";
            }

            return string.IsNullOrWhiteSpace(result) ? "None" : result;
        }


        private static string JoinStrings(
            IReadOnlyList<string> values
        )
        {
            if (values == null || values.Count == 0)
            {
                return "None";
            }

            string result = "";

            for (int i = 0; i < values.Count; i++)
            {
                string value = values[i];

                if (string.IsNullOrWhiteSpace(value))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(result))
                {
                    result += ",";
                }

                result += value;
            }

            return string.IsNullOrWhiteSpace(result) ? "None" : result;
        }


        private static string Format(
            float value
        )
        {
            return value.ToString("0.0");
        }
    }
}
