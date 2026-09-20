using System;
using System.Collections.Generic;
using System.Globalization;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// G7 Phase 1 - Deterministic Mission Generator.
    ///
    /// Produce un único candidato temporal por target sin Original y sin una
    /// misión autorada de USU. La selección usa el ranking G6, una señal de
    /// distintividad y una seed reproducible. No crea MissionDefinition.
    /// </summary>
    public static class DeterministicMissionGenerator
    {
        public const int GeneratorVersion = 1;

        private const float TopBandWidth = 8f;

        private sealed class WeightedOption
        {
            public MissionTemplateCompatibilityAssessment Assessment;
            public float Distinctiveness;
            public float Availability;
            public float SelectionScore;
            public double Weight;
        }


        public static GeneratedMissionCandidate GenerateForSurvivor(
            IReadOnlyList<MissionTemplateCompatibilityAssessment> assessments
        )
        {
            if (assessments == null || assessments.Count == 0)
            {
                return null;
            }

            List<WeightedOption> options =
                BuildEligibleOptions(assessments);

            if (options.Count == 0)
            {
                return null;
            }

            options.Sort(CompareOptionByFamily);

            float bestScore = 0f;

            for (int i = 0; i < options.Count; i++)
            {
                if (options[i].SelectionScore > bestScore)
                {
                    bestScore = options[i].SelectionScore;
                }
            }

            List<WeightedOption> shortlist =
                new List<WeightedOption>();

            for (int i = 0; i < options.Count; i++)
            {
                WeightedOption option = options[i];

                if (option.SelectionScore + TopBandWidth < bestScore)
                {
                    continue;
                }

                float shifted = Math.Max(
                    5f,
                    option.SelectionScore - 45f
                );

                option.Weight = shifted * shifted;
                shortlist.Add(option);
            }

            if (shortlist.Count == 0)
            {
                return null;
            }

            MissionTemplateCompatibilityAssessment first =
                shortlist[0].Assessment;

            string stableSeed =
                Plugin.PluginGuid +
                "|" +
                first.SurvivorId +
                "|g7-v" +
                GeneratorVersion;

            uint seedHash = StableHash(stableSeed);

            WeightedOption selected =
                SelectWeighted(shortlist, seedHash);

            if (selected == null || selected.Assessment == null)
            {
                return null;
            }

            MissionTemplateCompatibilityAssessment assessment =
                selected.Assessment;

            GeneratedMissionCandidate candidate =
                new GeneratedMissionCandidate
                {
                    GeneratorVersion = GeneratorVersion,
                    CandidateId =
                        "generated.v" +
                        GeneratorVersion +
                        "." +
                        StableHash(
                            stableSeed +
                            "|" +
                            assessment.Family
                        ).ToString("x8"),
                    TemplateId =
                        "g7." +
                        assessment.Family.ToString().ToLowerInvariant() +
                        ".v1",
                    SurvivorId = assessment.SurvivorId,
                    BodyName = assessment.BodyName,
                    DisplayName = assessment.DisplayName,
                    Family = assessment.Family,
                    ChallengeTier = assessment.ChallengeTier,
                    GameplayComplexity = assessment.GameplayComplexity,
                    CompatibilityScore = assessment.CompatibilityScore,
                    DistinctivenessScore = selected.Distinctiveness,
                    PerformerAvailabilityScore = selected.Availability,
                    SelectionScore = selected.SelectionScore,
                    AlternativePerformerCount =
                        assessment.AlternativePerformerCount,
                    UnlockedAlternativePerformerCount =
                        assessment.UnlockedAlternativePerformerCount,
                    StableSeed = stableSeed,
                    SeedHash = seedHash,
                    ProfileFingerprint = BuildProfileFingerprint(assessments),
                    RequiresTargetUsage = false,
                    IsTemporary = true,
                    IsFallbackOnly = true,
                    AuthoredMissionOverridesGenerated = true
                };

            candidate.Parameters =
                BuildParameterPlan(
                    candidate,
                    StableUnit(
                        StableHash(stableSeed + "|parameters")
                    )
                );

            return candidate;
        }


        private static List<WeightedOption> BuildEligibleOptions(
            IReadOnlyList<MissionTemplateCompatibilityAssessment> assessments
        )
        {
            List<WeightedOption> strong =
                new List<WeightedOption>();

            List<WeightedOption> compatible =
                new List<WeightedOption>();

            for (int i = 0; i < assessments.Count; i++)
            {
                MissionTemplateCompatibilityAssessment assessment =
                    assessments[i];

                if (
                    assessment == null ||
                    assessment.Family == MissionTemplateFamily.Unknown ||
                    assessment.RequiresTargetUsage ||
                    assessment.UnlockedAlternativePerformerCount <= 0
                )
                {
                    continue;
                }

                if ((int)assessment.CompatibilityLevel <
                    (int)MissionTemplateCompatibilityLevel.Compatible)
                {
                    continue;
                }

                float distinctiveness =
                    CalculateDistinctiveness(
                        assessment.AlternativePerformerCount
                    );

                float availability =
                    CalculateAvailability(
                        assessment.UnlockedAlternativePerformerCount
                    );

                float selectionScore =
                    Clamp(
                        0.62f * assessment.CompatibilityScore +
                        0.23f * distinctiveness +
                        0.10f * availability +
                        0.05f * assessment.TierFitScore,
                        0f,
                        100f
                    );

                WeightedOption option =
                    new WeightedOption
                    {
                        Assessment = assessment,
                        Distinctiveness = distinctiveness,
                        Availability = availability,
                        SelectionScore = selectionScore
                    };

                compatible.Add(option);

                if ((int)assessment.CompatibilityLevel >=
                    (int)MissionTemplateCompatibilityLevel.Strong)
                {
                    strong.Add(option);
                }
            }

            /*
             * Si hay una familia Strong/Preferred no dejamos que un fallback
             * genérico Compatible la desplace sólo por el random.
             */
            return strong.Count > 0 ? strong : compatible;
        }


        private static float CalculateDistinctiveness(
            int alternativePerformerCount
        )
        {
            if (alternativePerformerCount <= 0)
            {
                return 100f;
            }

            return Clamp(
                100f *
                (1f - Math.Min(alternativePerformerCount, 30) / 30f),
                0f,
                100f
            );
        }


        private static float CalculateAvailability(
            int unlockedAlternativePerformerCount
        )
        {
            if (unlockedAlternativePerformerCount <= 0)
            {
                return 0f;
            }

            return Clamp(
                30f + 14f * unlockedAlternativePerformerCount,
                0f,
                100f
            );
        }


        private static WeightedOption SelectWeighted(
            IReadOnlyList<WeightedOption> options,
            uint seedHash
        )
        {
            if (options == null || options.Count == 0)
            {
                return null;
            }

            double totalWeight = 0d;

            for (int i = 0; i < options.Count; i++)
            {
                totalWeight += Math.Max(0.0001d, options[i].Weight);
            }

            if (totalWeight <= 0d)
            {
                return options[0];
            }

            double roll =
                StableUnit(seedHash) * totalWeight;

            double cursor = 0d;

            for (int i = 0; i < options.Count; i++)
            {
                cursor += Math.Max(0.0001d, options[i].Weight);

                if (roll < cursor)
                {
                    return options[i];
                }
            }

            return options[options.Count - 1];
        }


        private static GeneratedMissionParameterPlan BuildParameterPlan(
            GeneratedMissionCandidate candidate,
            double variationUnit
        )
        {
            int tier = ClampTier((int)candidate.ChallengeTier);
            float variation =
                0.94f + 0.12f * (float)variationUnit;

            GeneratedMissionParameterPlan plan =
                new GeneratedMissionParameterPlan
                {
                    PerformerPolicy =
                        "AnyCompatibleUnlockedAlternative",
                    ResetScope = "Run",
                    RepeatCount = 1,
                    Amount = 1,
                    TimeLimitSeconds = 0f,
                    Notes =
                        "Temporary G7 blueprint; authored USU mission overrides."
                };

            switch (candidate.Family)
            {
                case MissionTemplateFamily.CombatKills:
                    plan.ObjectiveKind = "CombatKills";
                    plan.Amount = Vary(Scale(tier, 40, 60, 80, 100, 125), variation);
                    break;

                case MissionTemplateFamily.BossFinisher:
                    plan.ObjectiveKind = "BossFinalHits";
                    plan.Amount = Scale(tier, 1, 1, 1, 2, 2);
                    break;

                case MissionTemplateFamily.Execution:
                    plan.ObjectiveKind = "Executions";
                    plan.Amount = Vary(Scale(tier, 3, 5, 8, 12, 16), variation);
                    plan.ResetScope = tier >= 4 ? "Stage" : "Run";
                    break;

                case MissionTemplateFamily.CriticalHit:
                    plan.ObjectiveKind = "CriticalHits";
                    plan.Amount = Vary(Scale(tier, 10, 20, 35, 50, 75), variation);
                    plan.ResetScope = "Stage";
                    break;

                case MissionTemplateFamily.Backstab:
                    plan.ObjectiveKind = "Backstabs";
                    plan.Amount = Vary(Scale(tier, 3, 5, 8, 12, 16), variation);
                    plan.ResetScope = "Stage";
                    break;

                case MissionTemplateFamily.StatusEffect:
                    plan.ObjectiveKind = "StatusApplications";
                    plan.Amount = Vary(Scale(tier, 20, 35, 50, 75, 100), variation);
                    break;

                case MissionTemplateFamily.HealingSupport:
                    plan.ObjectiveKind = "HealingTotal";
                    plan.Amount = Vary(Scale(tier, 2500, 5000, 10000, 18000, 30000), variation);
                    break;

                case MissionTemplateFamily.DefenseSurvival:
                    plan.ObjectiveKind = "SurvivalWindow";
                    plan.TimeLimitSeconds = Scale(tier, 30, 45, 60, 90, 120);
                    break;

                case MissionTemplateFamily.MobilityTraversal:
                    plan.ObjectiveKind = "FastStageCompletion";
                    plan.TimeLimitSeconds = Scale(tier, 480, 420, 360, 300, 270);
                    plan.ResetScope = "Stage";
                    break;

                case MissionTemplateFamily.AirborneCombat:
                    plan.ObjectiveKind = "AirborneKills";
                    plan.Amount = Vary(Scale(tier, 3, 4, 5, 6, 8), variation);
                    plan.RepeatCount = Scale(tier, 1, 2, 2, 3, 3);
                    plan.ResetScope = "Stage";
                    break;

                case MissionTemplateFamily.SummonMinion:
                    plan.ObjectiveKind = "ActiveMinions";
                    plan.Amount = Scale(tier, 1, 2, 3, 3, 4);
                    break;

                case MissionTemplateFamily.ResourceMastery:
                    plan.ObjectiveKind = "ResourceActions";
                    plan.Amount = Vary(Scale(tier, 5, 8, 12, 16, 20), variation);
                    break;

                case MissionTemplateFamily.MeleeCombat:
                    plan.ObjectiveKind = "MeleeKills";
                    plan.Amount = Vary(Scale(tier, 15, 25, 40, 60, 80), variation);
                    plan.ResetScope = tier >= 4 ? "Stage" : "Run";
                    break;

                case MissionTemplateFamily.ProjectileCombat:
                    plan.ObjectiveKind = "ProjectileKills";
                    plan.Amount = Vary(Scale(tier, 20, 30, 45, 65, 90), variation);
                    break;

                case MissionTemplateFamily.AreaControl:
                    plan.ObjectiveKind = "AreaControlEvents";
                    plan.Amount = Vary(Scale(tier, 8, 12, 20, 30, 45), variation);
                    break;

                case MissionTemplateFamily.SelfBuffMastery:
                    plan.ObjectiveKind = "BuffedActions";
                    plan.Amount = Vary(Scale(tier, 5, 8, 12, 16, 20), variation);
                    break;

                default:
                    plan.ObjectiveKind = "Unknown";
                    break;
            }

            return plan;
        }


        private static string BuildProfileFingerprint(
            IReadOnlyList<MissionTemplateCompatibilityAssessment> assessments
        )
        {
            List<MissionTemplateCompatibilityAssessment> ordered =
                new List<MissionTemplateCompatibilityAssessment>();

            for (int i = 0; i < assessments.Count; i++)
            {
                if (assessments[i] != null)
                {
                    ordered.Add(assessments[i]);
                }
            }

            ordered.Sort(
                delegate(
                    MissionTemplateCompatibilityAssessment a,
                    MissionTemplateCompatibilityAssessment b
                )
                {
                    return ((int)a.Family).CompareTo((int)b.Family);
                }
            );

            string material = "";

            for (int i = 0; i < ordered.Count; i++)
            {
                MissionTemplateCompatibilityAssessment assessment =
                    ordered[i];

                material +=
                    ((int)assessment.Family).ToString(
                        CultureInfo.InvariantCulture
                    ) +
                    ":" +
                    assessment.CompatibilityScore.ToString(
                        "0.0",
                        CultureInfo.InvariantCulture
                    ) +
                    ":" +
                    ((int)assessment.ChallengeTier).ToString(
                        CultureInfo.InvariantCulture
                    ) +
                    ";";
            }

            return StableHash(material).ToString("x8");
        }


        public static uint StableHash(
            string value
        )
        {
            unchecked
            {
                uint hash = 2166136261u;
                string safe = value ?? "";

                for (int i = 0; i < safe.Length; i++)
                {
                    char character = safe[i];

                    hash ^= (byte)(character & 0xff);
                    hash *= 16777619u;

                    if (character > 255)
                    {
                        hash ^= (byte)((character >> 8) & 0xff);
                        hash *= 16777619u;
                    }
                }

                return hash;
            }
        }


        private static double StableUnit(
            uint hash
        )
        {
            return hash / 4294967296d;
        }


        private static int CompareOptionByFamily(
            WeightedOption a,
            WeightedOption b
        )
        {
            int aFamily =
                a != null && a.Assessment != null
                    ? (int)a.Assessment.Family
                    : int.MaxValue;

            int bFamily =
                b != null && b.Assessment != null
                    ? (int)b.Assessment.Family
                    : int.MaxValue;

            return aFamily.CompareTo(bFamily);
        }


        private static int Scale(
            int tier,
            int tier1,
            int tier2,
            int tier3,
            int tier4,
            int tier5
        )
        {
            switch (ClampTier(tier))
            {
                case 1:
                    return tier1;
                case 2:
                    return tier2;
                case 3:
                    return tier3;
                case 4:
                    return tier4;
                default:
                    return tier5;
            }
        }


        private static int Vary(
            int value,
            float factor
        )
        {
            return Math.Max(
                1,
                (int)Math.Round(value * factor)
            );
        }


        private static int ClampTier(
            int tier
        )
        {
            if (tier < 1)
            {
                return 1;
            }

            if (tier > 5)
            {
                return 5;
            }

            return tier;
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
