using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Logging;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// C2.4 - Read-only coverage audit between official survivor unlock
    /// implementations and Mission V2's reusable objective library.
    ///
    /// This intentionally reports conservative CANDIDATES. It does not infer
    /// exact gameplay rules from a class name, and it never changes unlock
    /// authority, missions, persistence, providers, or Generated output.
    /// </summary>
    public static class VanillaUnlockCoverageService
    {
        private sealed class CoverageResult
        {
            public string Status = "Unmapped";
            public string Candidate = "<none>";
            public string Evidence = "none";
        }

        public static void Audit(
            IReadOnlyList<SurvivorInfo> survivors,
            ManualLogSource logger
        )
        {
            if (survivors == null)
            {
                return;
            }

            int inspected = 0;
            int mapped = 0;
            int likely = 0;
            int unmapped = 0;

            for (int i = 0; i < survivors.Count; i++)
            {
                SurvivorInfo survivor = survivors[i];
                if (
                    survivor == null ||
                    survivor.SurvivorDef == null ||
                    survivor.IsModded ||
                    survivor.SurvivorDef.unlockableDef == null
                )
                {
                    continue;
                }

                AchievementDef achievement =
                    FindAchievementForUnlockable(
                        survivor.SurvivorDef.unlockableDef
                    );

                if (achievement == null)
                {
                    continue;
                }

                inspected++;

                string achievementType = TypeName(achievement.type);
                string trackerType = TypeName(achievement.serverTrackerType);

                CoverageResult coverage = Classify(
                    achievement.identifier,
                    achievementType,
                    trackerType
                );

                if (coverage.Status == "Covered")
                {
                    mapped++;
                }
                else if (coverage.Status == "LikelyCovered")
                {
                    likely++;
                }
                else
                {
                    unmapped++;
                }

                UsuLog.Verbose(
                    logger,
                    "[VANILLA UNLOCK COVERAGE] " +
                    SafeBodyName(survivor) +
                    " | Achievement: " +
                    (achievement.identifier ?? "") +
                    " | Coverage: " + coverage.Status +
                    " | Candidate: " + coverage.Candidate +
                    " | Evidence: " + coverage.Evidence +
                    " | Type: " + achievementType +
                    " | ServerTracker: " + trackerType
                );
            }

            UsuLog.Diagnostic(
                logger,
                "[VANILLA UNLOCK COVERAGE] C2.4 auditado | " +
                "Achievement methods: " + inspected +
                " | Covered: " + mapped +
                " | LikelyCovered: " + likely +
                " | Unmapped: " + unmapped +
                " | Nuevos objetivos creados: 0" +
                " | Autoridad modificada: no"
            );
        }

        private static CoverageResult Classify(
            string identifier,
            string achievementType,
            string trackerType
        )
        {
            string text = (
                (identifier ?? "") + " " +
                (achievementType ?? "") + " " +
                (trackerType ?? "")
            ).ToLowerInvariant();

            // Strong/simple structural matches. These are still candidates,
            // but the Mission V2 primitive already directly represents the
            // broad action named by the official achievement/tracker type.
            if (ContainsAny(text, "teleporter"))
                return Covered(MissionPresetIds.CompleteTeleporter, "teleporter");

            if (ContainsAny(text, "heal", "healing"))
                return Covered(MissionPresetIds.HealHealth, "heal/healing");

            if (ContainsAny(text, "level"))
                return Covered(MissionPresetIds.ReachLevel, "level");

            if (ContainsAny(text, "minion", "drone", "turret"))
                return Covered(MissionPresetIds.RecruitMinions, "minion/drone/turret");

            if (ContainsAny(text, "umbra", "vengeance"))
                return Covered(MissionPresetIds.DefeatUmbraWaves, "umbra/vengeance");

            if (ContainsAny(text, "obliterate", "ending"))
                return Covered(MissionPresetIds.CompleteEnding, "obliterate/ending");

            if (ContainsAny(text, "interact", "shrine", "altar"))
                return Likely(MissionPresetIds.InteractWithTarget, "interaction keyword");

            if (ContainsAny(text, "acolyte"))
                return Likely(
                    MissionPresetIds.ConsumeItemOnInteraction,
                    "Acolyte unlock family; exact item rule must be verified at runtime"
                );

            if (ContainsAny(text, "skill", "ability"))
                return Likely(MissionPresetIds.UseSkill, "skill/ability");

            if (ContainsAny(text, "equipment"))
                return Likely(MissionPresetIds.CarryEquipment, "equipment");

            if (ContainsAny(text, "item", "inventory"))
                return Likely(MissionPresetIds.HoldItemStack, "item/inventory");

            if (ContainsAny(text, "boss"))
                return Likely(MissionPresetIds.KillBoss, "boss");

            if (ContainsAny(text, "kill", "death", "defeat"))
                return Likely(MissionPresetIds.KillEnemies, "kill/death/defeat");

            if (ContainsAny(text, "stage", "scene"))
                return Likely(MissionPresetIds.CompleteStage, "stage/scene");

            if (ContainsAny(text, "run", "game"))
                return Likely(MissionPresetIds.CompleteRun, "run/game");

            return new CoverageResult
            {
                Status = "Unmapped",
                Candidate = "<none>",
                Evidence = "no safe keyword mapping"
            };
        }

        private static CoverageResult Covered(
            string candidate,
            string evidence
        )
        {
            return new CoverageResult
            {
                Status = "Covered",
                Candidate = candidate ?? "<none>",
                Evidence = evidence ?? ""
            };
        }

        private static CoverageResult Likely(
            string candidate,
            string evidence
        )
        {
            return new CoverageResult
            {
                Status = "LikelyCovered",
                Candidate = candidate ?? "<none>",
                Evidence = evidence ?? ""
            };
        }

        private static bool ContainsAny(string text, params string[] terms)
        {
            if (string.IsNullOrEmpty(text) || terms == null)
            {
                return false;
            }

            for (int i = 0; i < terms.Length; i++)
            {
                string term = terms[i];
                if (
                    !string.IsNullOrEmpty(term) &&
                    text.IndexOf(term, StringComparison.Ordinal) >= 0
                )
                {
                    return true;
                }
            }

            return false;
        }

        private static AchievementDef FindAchievementForUnlockable(
            UnlockableDef unlockable
        )
        {
            if (
                unlockable == null ||
                string.IsNullOrWhiteSpace(unlockable.cachedName)
            )
            {
                return null;
            }

            try
            {
                IEnumerable definitions =
                    AchievementManager.achievementDefs as IEnumerable;

                if (definitions == null)
                {
                    return null;
                }

                foreach (object raw in definitions)
                {
                    AchievementDef achievement = raw as AchievementDef;
                    if (achievement == null)
                    {
                        continue;
                    }

                    if (
                        string.Equals(
                            achievement.unlockableRewardIdentifier,
                            unlockable.cachedName,
                            StringComparison.Ordinal
                        )
                    )
                    {
                        return achievement;
                    }
                }
            }
            catch
            {
                // Diagnostic only.
            }

            return null;
        }

        private static string SafeBodyName(SurvivorInfo survivor)
        {
            if (
                survivor != null &&
                !string.IsNullOrWhiteSpace(survivor.BodyName)
            )
            {
                return survivor.BodyName;
            }

            return "<unknown>";
        }

        private static string TypeName(Type type)
        {
            return type != null
                ? (type.FullName ?? type.Name ?? "")
                : "<none>";
        }
    }
}
