using RoR2;

namespace UniversalSurvivorUnlocks
{
    public sealed class SessionUnlockGrantResult
    {
        public string BodyName
        {
            get;
        }


        public LocalUser LocalUser
        {
            get;
        }


        public bool AchievementBefore
        {
            get;
        }


        public bool UnlockableBefore
        {
            get;
        }


        public bool AchievementAfter
        {
            get;
        }


        public bool UnlockableAfter
        {
            get;
        }


        public bool AchievementWasNew =>
            !AchievementBefore &&
            AchievementAfter;


        public bool UnlockableWasNew =>
            !UnlockableBefore &&
            UnlockableAfter;


        // Puede estar desbloqueado por el proveedor Original y recibir
        // solamente la normalización silenciosa del Unlockable USU.
        public bool Success => UnlockableAfter;


        public SessionUnlockGrantResult(
            string bodyName,
            LocalUser localUser,
            bool achievementBefore,
            bool unlockableBefore,
            bool achievementAfter,
            bool unlockableAfter
        )
        {
            BodyName =
                bodyName;

            LocalUser =
                localUser;

            AchievementBefore =
                achievementBefore;

            UnlockableBefore =
                unlockableBefore;

            AchievementAfter =
                achievementAfter;

            UnlockableAfter =
                unlockableAfter;
        }
    }
}
