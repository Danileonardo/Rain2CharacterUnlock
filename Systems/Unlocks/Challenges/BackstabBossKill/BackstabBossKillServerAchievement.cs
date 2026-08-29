using RoR2.Achievements;

namespace UniversalSurvivorUnlocks
{
    public class BackstabBossKillServerAchievement
        : BaseServerAchievement
    {
        private bool completed;


        // =========================================================
        // INSTALAR
        // =========================================================

        public override void OnInstall()
        {
            base.OnInstall();


            completed =
                false;


            BackstabBossKillTracker
                .BackstabBossKillDetected +=
                OnBackstabBossKillDetected;
        }


        // =========================================================
        // DESINSTALAR
        // =========================================================

        public override void OnUninstall()
        {
            BackstabBossKillTracker
                .BackstabBossKillDetected -=
                OnBackstabBossKillDetected;


            base.OnUninstall();
        }


        // =========================================================
        // BACKSTAB MORTAL CONTRA BOSS
        // =========================================================

        private void OnBackstabBossKillDetected()
        {
            if (completed)
            {
                return;
            }


            completed =
                true;


            Grant();


            ServerTryToCompleteActivity();
        }
    }
}