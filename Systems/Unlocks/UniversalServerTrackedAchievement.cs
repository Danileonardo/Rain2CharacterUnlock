using RoR2.Achievements;

namespace UniversalSurvivorUnlocks
{
    public class UniversalServerTrackedAchievement
        : BaseAchievement
    {
        public override void OnInstall()
        {
            base.OnInstall();

            SetServerTracked(
                true
            );
        }


        public override void OnUninstall()
        {
            SetServerTracked(
                false
            );

            base.OnUninstall();
        }
    }
}