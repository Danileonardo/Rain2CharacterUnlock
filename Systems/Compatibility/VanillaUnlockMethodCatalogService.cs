using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx.Logging;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// C2.3 - Read-only catalog of official survivor unlock methods.
    ///
    /// Purpose: treat vanilla/DLC achievements as a library of proven unlock
    /// patterns that USU can later generalize into Mission V2 objectives and
    /// conditions for modded survivors.
    ///
    /// This service never changes achievements, unlockables, providers,
    /// persistence, or Generated authority.
    /// </summary>
    public static class VanillaUnlockMethodCatalogService
    {
        public static void Audit(
            IReadOnlyList<SurvivorInfo> survivors,
            ManualLogSource logger
        )
        {
            if (survivors == null)
            {
                return;
            }

            int officialSurvivors = 0;
            int withOriginalUnlock = 0;
            int achievementResolved = 0;
            int serverTracked = 0;
            int clientOnly = 0;
            int missingAchievement = 0;

            for (int i = 0; i < survivors.Count; i++)
            {
                SurvivorInfo survivor = survivors[i];
                if (
                    survivor == null ||
                    survivor.SurvivorDef == null ||
                    survivor.IsModded
                )
                {
                    continue;
                }

                officialSurvivors++;

                UnlockableDef unlockable =
                    survivor.SurvivorDef.unlockableDef;

                if (unlockable == null)
                {
                    continue;
                }

                withOriginalUnlock++;

                AchievementDef achievement =
                    FindAchievementForUnlockable(unlockable);

                if (achievement == null)
                {
                    missingAchievement++;
                    UsuLog.Verbose(
                        logger,
                        "[VANILLA UNLOCK METHOD] " +
                        SafeBodyName(survivor) +
                        " | Unlockable: " +
                        (unlockable.cachedName ?? "") +
                        " | Achievement: NO RESUELTO"
                    );
                    continue;
                }

                achievementResolved++;

                string achievementType =
                    TypeName(achievement.type);

                string serverTrackerType =
                    TypeName(achievement.serverTrackerType);

                if (achievement.serverTrackerType != null)
                {
                    serverTracked++;
                }
                else
                {
                    clientOnly++;
                }

                UsuLog.Verbose(
                    logger,
                    "[VANILLA UNLOCK METHOD] " +
                    SafeBodyName(survivor) +
                    " | Achievement: " +
                    (achievement.identifier ?? "") +
                    " | Type: " + achievementType +
                    " | ServerTracker: " + serverTrackerType +
                    " | Prerequisite: " +
                    (achievement.prerequisiteAchievementIdentifier ?? "") +
                    " | Reward: " +
                    (achievement.unlockableRewardIdentifier ?? "")
                );
            }

            UsuLog.Diagnostic(
                logger,
                "[VANILLA UNLOCK METHOD] C2.3 catálogo construido | " +
                "Survivors oficiales: " + officialSurvivors +
                " | Con unlock Original: " + withOriginalUnlock +
                " | Achievement resuelto: " + achievementResolved +
                " | Server-tracked: " + serverTracked +
                " | Client-only: " + clientOnly +
                " | Sin AchievementDef: " + missingAchievement +
                " | Autoridad modificada: no"
            );
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
                // Read-only diagnostic. A custom achievement system may not
                // expose its definitions through AchievementManager.
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
