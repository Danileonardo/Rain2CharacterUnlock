using System;
using System.Collections.Generic;

using BepInEx.Logging;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Registro runtime G4 Phase 1.4.
    ///
    /// Mantiene evaluaciones locales + comunidad curada de la sesión actual.
    /// No persiste LocalPower/ResolvedPower y todavía no resuelve PowerTier.
    /// </summary>
    public static class LocalPowerAssessmentService
    {
        private static readonly List<LocalPowerAssessment>
            Assessments =
                new List<LocalPowerAssessment>();

        private static readonly Dictionary<string, LocalPowerAssessment>
            AssessmentsBySurvivorId =
                new Dictionary<string, LocalPowerAssessment>(
                    StringComparer.OrdinalIgnoreCase
                );

        private static readonly Dictionary<string, LocalPowerAssessment>
            AssessmentsByBody =
                new Dictionary<string, LocalPowerAssessment>(
                    StringComparer.OrdinalIgnoreCase
                );

        private static ManualLogSource logger;

        public static int Count => Assessments.Count;


        public static void Initialize(
            IReadOnlyList<MissionProfile> missionProfiles,
            ManualLogSource activeLogger
        )
        {
            logger = activeLogger;
            Rebuild(missionProfiles);
        }


        public static void Rebuild(
            IReadOnlyList<MissionProfile> missionProfiles
        )
        {
            Assessments.Clear();
            AssessmentsBySurvivorId.Clear();
            AssessmentsByBody.Clear();

            List<LocalPowerAssessment> rebuilt =
                LocalPowerAssessmentAnalyzer.AnalyzeAll(
                    missionProfiles
                );

            for (int i = 0; i < rebuilt.Count; i++)
            {
                LocalPowerAssessment assessment = rebuilt[i];

                if (assessment == null)
                {
                    continue;
                }

                Assessments.Add(assessment);

                if (!string.IsNullOrWhiteSpace(assessment.SurvivorId))
                {
                    AssessmentsBySurvivorId[
                        assessment.SurvivorId
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
                    LocalPowerAssessment a,
                    LocalPowerAssessment b
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


        public static IReadOnlyList<LocalPowerAssessment> GetAll()
        {
            return Assessments;
        }


        public static bool TryGetBySurvivorId(
            string survivorId,
            out LocalPowerAssessment assessment
        )
        {
            assessment = null;

            if (string.IsNullOrWhiteSpace(survivorId))
            {
                return false;
            }

            return AssessmentsBySurvivorId.TryGetValue(
                survivorId,
                out assessment
            );
        }


        public static bool TryGetByBodyName(
            string bodyName,
            out LocalPowerAssessment assessment
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
            int vanillaOffenseRefs = 0;
            int vanillaSustainRefs = 0;
            int vanillaDefenseRefs = 0;
            int vanillaMobilityRefs = 0;
            int vanillaControlRefs = 0;
            int vanillaUtilityRefs = 0;
            int vanillaIndirectRefs = 0;
            int vanillaQuantitativeRefs = 0;
            int moddedIndirectSignals = 0;
            int moddedQuantitativeSkills = 0;
            int moddedQuantitativeScoredSkills = 0;
            int moddedDefaultLoadoutOptions = 0;
            int communityKnown = 0;
            float moddedScoreTotal = 0f;
            float moddedResolvedScoreTotal = 0f;
            float moddedConfidenceTotal = 0f;

            for (int i = 0; i < Assessments.Count; i++)
            {
                LocalPowerAssessment assessment = Assessments[i];

                if (assessment == null)
                {
                    continue;
                }

                if (assessment.IsModded)
                {
                    modded++;
                    moddedScoreTotal += assessment.LocalPowerScore;
                    moddedResolvedScoreTotal += assessment.ResolvedPowerScore;
                    moddedConfidenceTotal += assessment.Confidence;
                    moddedIndirectSignals += assessment.IndirectSignalCount;
                    moddedQuantitativeSkills += assessment.QuantitativeSkillCount;
                    moddedQuantitativeScoredSkills +=
                        assessment.QuantitativeScoredSkillCount;
                    moddedDefaultLoadoutOptions +=
                        assessment.DefaultLoadoutOptionCount;

                    if (assessment.CommunityPowerTier != CommunityPowerTier.Unknown)
                    {
                        communityKnown++;
                    }
                }
                else
                {
                    vanilla++;

                    if (assessment.OffenseIndex > 0f) vanillaOffenseRefs++;
                    if (assessment.SustainIndex > 0f) vanillaSustainRefs++;
                    if (assessment.DefenseIndex > 0f) vanillaDefenseRefs++;
                    if (assessment.MobilityIndex > 0f) vanillaMobilityRefs++;
                    if (assessment.ControlIndex > 0f) vanillaControlRefs++;
                    if (assessment.UtilityIndex > 0f) vanillaUtilityRefs++;
                    if (assessment.IndirectPowerIndex > 0f) vanillaIndirectRefs++;
                    if (assessment.QuantitativePowerIndex > 0f) vanillaQuantitativeRefs++;
                }
            }

            float averageModdedScore =
                modded > 0
                    ? moddedScoreTotal / modded
                    : 0f;

            float averageModdedResolvedScore =
                modded > 0
                    ? moddedResolvedScoreTotal / modded
                    : 0f;

            float averageModdedConfidence =
                modded > 0
                    ? moddedConfidenceTotal / modded
                    : 0f;

            logger?.LogInfo(
                "[POWER ASSESSMENT] G4 Phase 1.4 construido | " +
                $"Profiles: {Assessments.Count} | " +
                $"Modded: {modded} | " +
                $"Vanilla reference: {vanilla} | " +
                $"Refs positivas O/S/D/M/C/U: " +
                $"{vanillaOffenseRefs}/{vanillaSustainRefs}/" +
                $"{vanillaDefenseRefs}/{vanillaMobilityRefs}/" +
                $"{vanillaControlRefs}/{vanillaUtilityRefs} | " +
                $"Refs indirectas vanilla: {vanillaIndirectRefs} | " +
                $"Refs cuantitativas vanilla: {vanillaQuantitativeRefs} | " +
                $"Señales indirectas modded: {moddedIndirectSignals} | " +
                $"Skills cuantitativas modded: " +
                $"{moddedQuantitativeScoredSkills}/{moddedQuantitativeSkills} | " +
                $"Default loadout options modded: {moddedDefaultLoadoutOptions} | " +
                $"Community evidence: " +
                $"{CommunityPowerCatalog.Instance.RegisteredSourceCount} sources/" +
                $"{CommunityPowerCatalog.Instance.RegisteredSurvivorCount} survivors | " +
                $"Community known: {communityKnown}/{modded} | " +
                $"Min independent: {CommunityPowerCatalog.Instance.MinimumIndependentSources} | " +
                $"LocalPower medio modded: {Format(averageModdedScore)} | " +
                $"ResolvedPower medio modded: {Format(averageModdedResolvedScore)} | " +
                $"Confianza media modded: {Format(averageModdedConfidence)} | " +
                "Community runtime scraping: no | PowerTier: no | " +
                "ChallengeTier: no | Persistencia: no"
            );
        }


        private static void LogVerboseAssessments()
        {
            if (!UsuLog.VerboseEnabled)
            {
                return;
            }

            for (int i = 0; i < Assessments.Count; i++)
            {
                LocalPowerAssessment assessment = Assessments[i];

                if (assessment == null)
                {
                    continue;
                }

                UsuLog.Verbose(
                    logger,
                    "[POWER PROFILE] " +
                    assessment.DisplayName +
                    " | Id: " +
                    assessment.SurvivorId +
                    " | LocalPower: " +
                    Format(assessment.LocalPowerScore) +
                    " | ResolvedPower: " +
                    Format(assessment.ResolvedPowerScore) +
                    " | Quantitative: " +
                    Format(assessment.QuantitativePowerIndex) +
                    " | QuantRaw: " +
                    Format(assessment.QuantitativeRawIndex) +
                    " | QuantReliability: " +
                    Format(assessment.QuantitativeReliability) +
                    " | QuantCoverage: " +
                    Format(assessment.QuantitativeCoverage) +
                    " | Community: " +
                    assessment.CommunityPowerTier +
                    "(" +
                    assessment.CommunitySourceCount +
                    ") | Confidence: " +
                    Format(assessment.Confidence) +
                    " | Coverage: " +
                    Format(assessment.SemanticCoverage) +
                    " | Body: " +
                    Format(assessment.BodyIndex) +
                    " | Offense: " +
                    Format(assessment.OffenseIndex) +
                    " | Sustain: " +
                    Format(assessment.SustainIndex) +
                    " | Defense: " +
                    Format(assessment.DefenseIndex) +
                    " | Mobility: " +
                    Format(assessment.MobilityIndex) +
                    " | Control: " +
                    Format(assessment.ControlIndex) +
                    " | Utility: " +
                    Format(assessment.UtilityIndex) +
                    " | Indirect: " +
                    Format(assessment.IndirectPowerIndex) +
                    " | IndirectSignals H/M/L: " +
                    assessment.HighConfidenceIndirectSignals +
                    "/" +
                    assessment.MediumConfidenceIndirectSignals +
                    "/" +
                    assessment.LowConfidenceIndirectSignals +
                    " | Versatility: " +
                    assessment.VersatilityCount +
                    "/6 | Skills: " +
                    assessment.ScoredSkillCount +
                    "/" +
                    assessment.AggregateSkillCount +
                    " | DefaultLoadout: " +
                    assessment.DefaultLoadoutOptionCount +
                    " | Evidence H/M/L: " +
                    assessment.HighConfidenceEvidence +
                    "/" +
                    assessment.MediumConfidenceEvidence +
                    "/" +
                    assessment.LowConfidenceEvidence
                );

                if (assessment.IndirectSignals.Count > 0)
                {
                    string signalSummary = "";

                    for (int signalIndex = 0;
                         signalIndex < assessment.IndirectSignals.Count;
                         signalIndex++)
                    {
                        IndirectPowerSignal signal =
                            assessment.IndirectSignals[signalIndex];

                        if (signal == null)
                        {
                            continue;
                        }

                        if (signalSummary.Length > 0)
                        {
                            signalSummary += ",";
                        }

                        signalSummary +=
                            signal.Kind +
                            "(" +
                            ConfidenceLetter(signal.Confidence) +
                            ")";
                    }

                    UsuLog.Verbose(
                        logger,
                        "[POWER INDIRECT] " +
                        assessment.DisplayName +
                        " | Signals: " +
                        signalSummary
                    );
                }

                LogQuantitativeProfile(assessment);
                LogCommunityProfile(assessment);
                LogCommunitySources(assessment);
                LogPowerReview(assessment);
            }
        }


        private static void LogQuantitativeProfile(
            LocalPowerAssessment assessment
        )
        {
            if (assessment == null)
            {
                return;
            }

            List<SkillMagnitudeProfile> top =
                new List<SkillMagnitudeProfile>();

            for (int i = 0; i < assessment.SkillMagnitudes.Count; i++)
            {
                SkillMagnitudeProfile candidate =
                    assessment.SkillMagnitudes[i];

                if (
                    candidate == null ||
                    !candidate.ContributesToAggregate ||
                    candidate.UsageKind == MissionSkillUsageKind.LoadoutOption ||
                    !candidate.HasQuantitativeSignal ||
                    candidate.RawPowerContribution <= 0f
                )
                {
                    continue;
                }

                int insertAt = top.Count;

                for (int topIndex = 0; topIndex < top.Count; topIndex++)
                {
                    if (
                        candidate.RawPowerContribution >
                        top[topIndex].RawPowerContribution
                    )
                    {
                        insertAt = topIndex;
                        break;
                    }
                }

                top.Insert(insertAt, candidate);

                if (top.Count > 4)
                {
                    top.RemoveAt(top.Count - 1);
                }
            }

            string summary = "";

            for (int i = 0; i < top.Count; i++)
            {
                SkillMagnitudeProfile magnitude = top[i];

                if (summary.Length > 0)
                {
                    summary += "; ";
                }

                summary +=
                    magnitude.DisplayName +
                    "{Raw=" +
                    Format(magnitude.RawPowerContribution) +
                    ",Dmg=" +
                    Format(magnitude.PeakDamagePercent) +
                    "%,CD=" +
                    Format(magnitude.RechargeInterval) +
                    ",Exec=" +
                    Format(magnitude.ExecutionLeverage) +
                    ",Crit=" +
                    Format(magnitude.CriticalLeverage) +
                    ",Syn=" +
                    Format(magnitude.SynergyLeverage) +
                    ",Hits=" +
                    magnitude.HitCount +
                    ",AS=" +
                    Format(magnitude.AttackSpeedBonusPercent) +
                    "%,MS=" +
                    Format(magnitude.MoveSpeedBonusPercent) +
                    "%,Armor+=" +
                    Format(magnitude.ArmorBonusFlat) +
                    ",Armor-=" +
                    Format(magnitude.ArmorReductionFlat) +
                    ",DR=" +
                    Format(magnitude.DamageReductionPercent) +
                    "%}";
            }

            UsuLog.Verbose(
                logger,
                "[POWER MAGNITUDE] " +
                assessment.DisplayName +
                " | Quantitative: " +
                Format(assessment.QuantitativePowerIndex) +
                " | Raw: " +
                Format(assessment.QuantitativeRawIndex) +
                " | Reliability: " +
                Format(assessment.QuantitativeReliability) +
                " | Coverage: " +
                assessment.QuantitativeScoredSkillCount +
                "/" +
                assessment.QuantitativeSkillCount +
                " | Top: " +
                (summary.Length > 0 ? summary : "None")
            );
        }


        private static void LogCommunityProfile(
            LocalPowerAssessment assessment
        )
        {
            if (assessment == null || !assessment.IsModded)
            {
                return;
            }

            UsuLog.Verbose(
                logger,
                "[COMMUNITY POWER] " +
                assessment.DisplayName +
                " | Tier: " +
                assessment.CommunityPowerTier +
                " | Sources: " +
                assessment.CommunitySourceCount +
                " | Independent: " +
                assessment.CommunityIndependentSourceCount +
                " | Confidence: " +
                Format(assessment.CommunityConfidence) +
                " | Consensus: " +
                Format(assessment.CommunityConsensus) +
                " | Adjustment: " +
                Format(assessment.CommunityAdjustment) +
                " | ResolvedPower: " +
                Format(assessment.ResolvedPowerScore)
            );
        }


        private static void LogCommunitySources(
            LocalPowerAssessment assessment
        )
        {
            if (assessment == null || !assessment.IsModded)
            {
                return;
            }

            IReadOnlyList<CommunityPowerSource> sources =
                CommunityPowerCatalog.Instance.GetSources(
                    assessment.SurvivorId
                );

            if (sources == null || sources.Count == 0)
            {
                return;
            }

            for (int i = 0; i < sources.Count; i++)
            {
                CommunityPowerSource source = sources[i];

                if (source == null)
                {
                    continue;
                }

                UsuLog.Verbose(
                    logger,
                    "[COMMUNITY SOURCE] " +
                    assessment.DisplayName +
                    " | Tier: " +
                    source.Tier +
                    " | Kind: " +
                    source.Kind +
                    " | Weight: " +
                    Format(source.Weight * 100f) +
                    " | Group: " +
                    source.IndependentGroupId +
                    " | Date: " +
                    source.PublishedDate +
                    " | Context: " +
                    source.Context +
                    " | Evidence: " +
                    source.EvidenceLabel +
                    " | Basis: " +
                    source.NormalizationBasis
                );
            }
        }


        private static void LogPowerReview(
            LocalPowerAssessment assessment
        )
        {
            if (assessment == null)
            {
                return;
            }

            string dataStatus;

            if (
                assessment.SemanticCoverage >= 85f &&
                assessment.QuantitativeCoverage >= 75f
            )
            {
                dataStatus = "StrongData";
            }
            else if (
                assessment.SemanticCoverage >= 70f ||
                assessment.QuantitativeCoverage >= 50f
            )
            {
                dataStatus = "PartialData";
            }
            else
            {
                dataStatus = "LowData";
            }

            string localBand;

            if (assessment.ResolvedPowerScore >= 120f)
            {
                localBand = "VeryStrong";
            }
            else if (assessment.ResolvedPowerScore >= 110f)
            {
                localBand = "Strong";
            }
            else if (assessment.ResolvedPowerScore >= 90f)
            {
                localBand = "ReferenceRange";
            }
            else if (assessment.ResolvedPowerScore >= 80f)
            {
                localBand = "BelowReference";
            }
            else
            {
                localBand = "LowOrUnresolved";
            }

            UsuLog.Verbose(
                logger,
                "[POWER REVIEW] " +
                assessment.DisplayName +
                " | Local: " +
                Format(assessment.LocalPowerScore) +
                " | Resolved: " +
                Format(assessment.ResolvedPowerScore) +
                " | QuantRaw/Adj: " +
                Format(assessment.QuantitativeRawIndex) +
                "/" +
                Format(assessment.QuantitativePowerIndex) +
                " | QuantReliability: " +
                Format(assessment.QuantitativeReliability) +
                " | Semantic/QuantCoverage: " +
                Format(assessment.SemanticCoverage) +
                "/" +
                Format(assessment.QuantitativeCoverage) +
                " | DefaultLoadout: " +
                assessment.DefaultLoadoutOptionCount +
                " | Indirect: " +
                Format(assessment.IndirectPowerIndex) +
                " | Community: " +
                assessment.CommunityPowerTier +
                "(" +
                assessment.CommunityIndependentSourceCount +
                " independent) | Data: " +
                dataStatus +
                " | LocalBandHint: " +
                localBand
            );
        }


        private static string ConfidenceLetter(
            MissionEvidenceConfidence confidence
        )
        {
            switch (confidence)
            {
                case MissionEvidenceConfidence.High:
                    return "H";

                case MissionEvidenceConfidence.Medium:
                    return "M";

                default:
                    return "L";
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
