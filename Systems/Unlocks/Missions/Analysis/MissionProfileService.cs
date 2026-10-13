using System;
using System.Collections.Generic;

using BepInEx.Logging;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Registro runtime de perfiles G3.
    ///
    /// Sólo conserva perfiles en memoria mientras el contenido existe en la
    /// sesión actual. No escribe Survivors.json ni dormantMissions.
    /// </summary>
    public static class MissionProfileService
    {
        private static readonly List<MissionProfile>
            Profiles =
                new List<MissionProfile>();

        private static readonly Dictionary<string, MissionProfile>
            ProfilesBySurvivorId =
                new Dictionary<string, MissionProfile>(
                    StringComparer.OrdinalIgnoreCase
                );

        private static readonly Dictionary<string, MissionProfile>
            ProfilesByBody =
                new Dictionary<string, MissionProfile>(
                    StringComparer.OrdinalIgnoreCase
                );

        private static ManualLogSource logger;

        public static int Count =>
            Profiles.Count;

        public static void Initialize(
            IReadOnlyList<SurvivorContentProfile> contentProfiles,
            ManualLogSource activeLogger
        )
        {
            logger =
                activeLogger;

            Rebuild(
                contentProfiles
            );
        }


        public static void Rebuild(
            IReadOnlyList<SurvivorContentProfile> contentProfiles
        )
        {
            Profiles.Clear();
            ProfilesBySurvivorId.Clear();
            ProfilesByBody.Clear();

            if (contentProfiles == null)
            {
                LogSummary();
                return;
            }

            for (
                int i = 0;
                i < contentProfiles.Count;
                i++
            )
            {
                SurvivorContentProfile contentProfile =
                    contentProfiles[i];

                /*
                 * Un ContentProfile sólo es analizable si representa contenido
                 * real de esta sesión. Nunca fabricamos un perfil desde
                 * dormantMissions o Definitions de mods ausentes.
                 */
                if (
                    contentProfile == null ||
                    contentProfile.BodyPrefab == null ||
                    contentProfile.SurvivorDef == null
                )
                {
                    continue;
                }

                MissionProfile missionProfile =
                    MissionProfileAnalyzer.Analyze(
                        contentProfile
                    );

                if (missionProfile == null)
                {
                    continue;
                }

                Profiles.Add(
                    missionProfile
                );

                if (
                    !string.IsNullOrWhiteSpace(
                        missionProfile.SurvivorId
                    )
                )
                {
                    ProfilesBySurvivorId[
                        missionProfile.SurvivorId
                    ] = missionProfile;
                }

                if (
                    !string.IsNullOrWhiteSpace(
                        missionProfile.BodyName
                    )
                )
                {
                    ProfilesByBody[
                        missionProfile.BodyName
                    ] = missionProfile;
                }
            }

            Profiles.Sort(
                delegate(
                    MissionProfile a,
                    MissionProfile b
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
            LogVerboseProfiles();
        }


        public static IReadOnlyList<MissionProfile> GetAll()
        {
            return Profiles;
        }


        public static bool TryGetBySurvivorId(
            string survivorId,
            out MissionProfile profile
        )
        {
            profile = null;

            if (string.IsNullOrWhiteSpace(survivorId))
            {
                return false;
            }

            return ProfilesBySurvivorId.TryGetValue(
                survivorId,
                out profile
            );
        }


        public static bool TryGetByBodyName(
            string bodyName,
            out MissionProfile profile
        )
        {
            profile = null;

            if (string.IsNullOrWhiteSpace(bodyName))
            {
                return false;
            }

            return ProfilesByBody.TryGetValue(
                bodyName,
                out profile
            );
        }


        private static void LogSummary()
        {
            int moddedProfiles = 0;
            int skillVariants = 0;
            int aggregateEvidenceCount = 0;
            int allSkillEvidenceCount = 0;
            int excludedSkillEvidenceCount = 0;
            int capabilityLinks = 0;
            int skillEffectCount = 0;
            int statusReferenceCount = 0;
            int highConfidenceEvidence = 0;
            int mediumConfidenceEvidence = 0;
            int lowConfidenceEvidence = 0;
            int aggregateSkillVariants = 0;
            int loadoutOptionVariants = 0;

            for (
                int i = 0;
                i < Profiles.Count;
                i++
            )
            {
                MissionProfile profile =
                    Profiles[i];

                if (profile == null)
                {
                    continue;
                }

                if (profile.IsModded)
                {
                    moddedProfiles++;
                }

                skillVariants +=
                    profile.SkillVariantCount;

                aggregateEvidenceCount +=
                    profile.Evidence.Count;

                for (
                    int evidenceIndex = 0;
                    evidenceIndex < profile.Evidence.Count;
                    evidenceIndex++
                )
                {
                    MissionCapabilityEvidence evidence =
                        profile.Evidence[evidenceIndex];

                    if (evidence == null)
                    {
                        continue;
                    }

                    switch (evidence.Confidence)
                    {
                        case MissionEvidenceConfidence.High:
                            highConfidenceEvidence++;
                            break;

                        case MissionEvidenceConfidence.Medium:
                            mediumConfidenceEvidence++;
                            break;

                        default:
                            lowConfidenceEvidence++;
                            break;
                    }
                }

                capabilityLinks +=
                    profile.Capabilities.Count;

                for (
                    int skillIndex = 0;
                    skillIndex < profile.Skills.Count;
                    skillIndex++
                )
                {
                    MissionSkillProfile skill =
                        profile.Skills[skillIndex];

                    if (skill == null)
                    {
                        continue;
                    }

                    if (skill.ContributesToAggregate)
                    {
                        aggregateSkillVariants++;
                    }

                    if (
                        skill.UsageKind ==
                        MissionSkillUsageKind.LoadoutOption
                    )
                    {
                        loadoutOptionVariants++;
                    }

                    skillEffectCount +=
                        skill.Effects.Count;

                    statusReferenceCount +=
                        skill.StatusEffects.Count;

                    allSkillEvidenceCount +=
                        skill.Evidence.Count;

                    if (!skill.ContributesToAggregate)
                    {
                        excludedSkillEvidenceCount +=
                            skill.Evidence.Count;
                    }
                }
            }

            UsuLog.Diagnostic(
                logger,
                "[MISSION PROFILE] G3 Phase 2.3 construido | " +
                $"Profiles runtime: {Profiles.Count} | " +
                $"Modded: {moddedProfiles} | " +
                $"Skill variants: {skillVariants} | " +
                $"Aggregate: {aggregateSkillVariants} | " +
                $"Loadout options: {loadoutOptionVariants} | " +
                $"Capabilities: {capabilityLinks} | " +
                $"Skill effects: {skillEffectCount} | " +
                $"Status refs: {statusReferenceCount} | " +
                $"Evidencias aggregate: {aggregateEvidenceCount} | " +
                $"Evidencias total: {allSkillEvidenceCount} | " +
                $"Excluded evidence: {excludedSkillEvidenceCount} | " +
                $"Confianza aggregate H/M/L: " +
                $"{highConfidenceEvidence}/" +
                $"{mediumConfidenceEvidence}/" +
                $"{lowConfidenceEvidence} | " +
                "Persistencia: no"
            );
        }


        private static void LogVerboseProfiles()
        {
            if (!UsuLog.ExtendedAuditEnabled)
            {
                return;
            }

            for (
                int i = 0;
                i < Profiles.Count;
                i++
            )
            {
                MissionProfile profile =
                    Profiles[i];

                if (profile == null)
                {
                    continue;
                }

                UsuLog.ExtendedAudit(
                    logger,
                    "[MISSION PROFILE] " +
                    profile.DisplayName +
                    " | Id: " +
                    profile.SurvivorId +
                    " | Skills: " +
                    profile.SkillVariantCount +
                    " | Aggregate: " +
                    CountAggregateSkills(profile) +
                    " | LoadoutOptions: " +
                    CountLoadoutOptions(profile) +
                    " | Alt: " +
                    profile.AlternateSkillVariantCount +
                    " | Locked: " +
                    profile.LockedSkillVariantCount +
                    " | ExtraSlots: " +
                    (profile.HasExtraSkillSlots ? "Sí" : "No") +
                    " | Signals: " +
                    JoinCapabilities(profile.Capabilities)
                );

                for (
                    int skillIndex = 0;
                    skillIndex < profile.Skills.Count;
                    skillIndex++
                )
                {
                    MissionSkillProfile skill =
                        profile.Skills[skillIndex];

                    if (skill == null)
                    {
                        continue;
                    }

                    UsuLog.ExtendedAudit(
                        logger,
                        "[MISSION SKILL] " +
                        profile.DisplayName +
                        " | " +
                        skill.Slot +
                        "[" +
                        skill.VariantIndex +
                        "] " +
                        (
                            string.IsNullOrWhiteSpace(skill.DisplayName)
                                ? skill.InternalName
                                : skill.DisplayName
                        ) +
                        " | Usage: " +
                        skill.UsageKind +
                        (skill.ContributesToAggregate ? "" : "/Excluded") +
                        " | SkillDef: " +
                        ShortTypeName(skill.SkillDefType) +
                        " | State: " +
                        ShortTypeName(skill.ActivationStateType) +
                        " | CD: " +
                        skill.RechargeInterval +
                        " | Stock: " +
                        skill.MaxStock +
                        " | Effects: " +
                        JoinEffects(skill.Effects) +
                        " | Status: " +
                        JoinStrings(skill.StatusEffects)
                    );
                }
            }
        }


        private static int CountAggregateSkills(
            MissionProfile profile
        )
        {
            if (profile == null)
            {
                return 0;
            }

            int count = 0;

            for (
                int i = 0;
                i < profile.Skills.Count;
                i++
            )
            {
                MissionSkillProfile skill =
                    profile.Skills[i];

                if (
                    skill != null &&
                    skill.ContributesToAggregate
                )
                {
                    count++;
                }
            }

            return count;
        }


        private static int CountLoadoutOptions(
            MissionProfile profile
        )
        {
            if (profile == null)
            {
                return 0;
            }

            int count = 0;

            for (
                int i = 0;
                i < profile.Skills.Count;
                i++
            )
            {
                MissionSkillProfile skill =
                    profile.Skills[i];

                if (
                    skill != null &&
                    skill.UsageKind ==
                    MissionSkillUsageKind.LoadoutOption
                )
                {
                    count++;
                }
            }

            return count;
        }


        private static string JoinEffects(
            IReadOnlyList<MissionSkillEffectProfile> effects
        )
        {
            if (
                effects == null ||
                effects.Count == 0
            )
            {
                return "None";
            }

            string result = "";

            for (
                int i = 0;
                i < effects.Count;
                i++
            )
            {
                MissionSkillEffectProfile effect =
                    effects[i];

                if (effect == null)
                {
                    continue;
                }

                bool alreadyWritten = false;

                for (
                    int previousIndex = 0;
                    previousIndex < i;
                    previousIndex++
                )
                {
                    MissionSkillEffectProfile previous =
                        effects[previousIndex];

                    if (
                        previous != null &&
                        previous.Kind == effect.Kind
                    )
                    {
                        alreadyWritten = true;
                        break;
                    }
                }

                if (alreadyWritten)
                {
                    continue;
                }

                MissionEvidenceConfidence strongest =
                    effect.Confidence;

                for (
                    int compareIndex = i + 1;
                    compareIndex < effects.Count;
                    compareIndex++
                )
                {
                    MissionSkillEffectProfile candidate =
                        effects[compareIndex];

                    if (
                        candidate != null &&
                        candidate.Kind == effect.Kind &&
                        (int)candidate.Confidence > (int)strongest
                    )
                    {
                        strongest = candidate.Confidence;
                    }
                }

                if (!string.IsNullOrWhiteSpace(result))
                {
                    result += ",";
                }

                result +=
                    effect.Kind.ToString();

                if (
                    strongest ==
                    MissionEvidenceConfidence.High
                )
                {
                    result += "(H)";
                }
                else if (
                    strongest ==
                    MissionEvidenceConfidence.Medium
                )
                {
                    result += "(M)";
                }
                else
                {
                    result += "(L)";
                }
            }

            return
                string.IsNullOrWhiteSpace(result)
                    ? "None"
                    : result;
        }


        private static string JoinStrings(
            IReadOnlyList<string> values
        )
        {
            if (
                values == null ||
                values.Count == 0
            )
            {
                return "None";
            }

            string result = "";

            for (
                int i = 0;
                i < values.Count;
                i++
            )
            {
                string value =
                    values[i];

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

            return
                string.IsNullOrWhiteSpace(result)
                    ? "None"
                    : result;
        }


        private static string ShortTypeName(
            string fullName
        )
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return "None";
            }

            int index =
                fullName.LastIndexOf('.');

            if (
                index < 0 ||
                index >= fullName.Length - 1
            )
            {
                return fullName;
            }

            return
                fullName.Substring(
                    index + 1
                );
        }


        private static string JoinCapabilities(
            IReadOnlyList<MissionProfileCapability> capabilities
        )
        {
            if (
                capabilities == null ||
                capabilities.Count == 0
            )
            {
                return "None";
            }

            string result = "";

            for (
                int i = 0;
                i < capabilities.Count;
                i++
            )
            {
                if (i > 0)
                {
                    result += ",";
                }

                result +=
                    capabilities[i].ToString();
            }

            return result;
        }
    }
}
