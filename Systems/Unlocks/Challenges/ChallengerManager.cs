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

                default:
                    return null;
            }
        }
    }
}