using System;

namespace UniversalSurvivorUnlocks
{
    public static class ChallengeManager
    {
        // =========================================================
        // TIPO DE ACHIEVEMENT
        // =========================================================

        public static Type GetAchievementType(
            SurvivorChallengeJson challenge
        )
        {
            if (
                challenge == null ||
                !challenge.Enabled
            )
            {
                return typeof(
                    UniversalSurvivorAchievement
                );
            }


            string challengeType =
                challenge.Type?
                    .Trim()
                    .ToLowerInvariant()
                ?? "";


            switch (challengeType)
            {
                case "applystatuseffects":
                case "healhealth":
                case "bosscriticalkill":
                case "holditemstack":
                case "backstabbosskill":
                case "airborneexplosionkills":
                case "precisionexecutionstreak":
                    return typeof(
                        UniversalServerTrackedAchievement
                    );


                default:
                    return typeof(
                        UniversalSurvivorAchievement
                    );
            }
        }


        // =========================================================
        // TRACKER DEL SERVIDOR
        // =========================================================

        public static Type GetServerTrackerType(
            SurvivorChallengeJson challenge
        )
        {
            if (
                challenge == null ||
                !challenge.Enabled
            )
            {
                return null;
            }


            string challengeType =
                challenge.Type?
                    .Trim()
                    .ToLowerInvariant()
                ?? "";


            switch (challengeType)
            {
                case "applystatuseffects":
                    return typeof(
                        ApplyStatusEffectsServerAchievement
                    );


                case "healhealth":
                    return typeof(
                        HealHealthServerAchievement
                    );


                case "bosscriticalkill":
                    return typeof(
                        BossCriticalKillServerAchievement
                    );

                case "holditemstack":
                    return typeof(
                        HoldItemStackServerAchievement
                    );

                case "backstabbosskill":
                    return typeof(
                        BackstabBossKillServerAchievement
                    );

                case "airborneexplosionkills":
                    return typeof(
                        AirborneExplosionKillsServerAchievement
                    );

                case "precisionexecutionstreak":
                    return typeof(
                        PrecisionExecutionStreakServerAchievement
                    );

                default:
                    return null;
            }
        }
    }
}