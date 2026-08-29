using System;
using Newtonsoft.Json.Linq;
using RoR2.Achievements;

namespace UniversalSurvivorUnlocks
{
    public class BossCriticalKillServerAchievement
        : BaseServerAchievement
    {
        private const float DefaultMinimumDamage =
            4444f;


        private float minimumDamage =
            DefaultMinimumDamage;


        private bool completed;


        // =========================================================
        // INSTALAR
        // =========================================================

        public override void OnInstall()
        {
            base.OnInstall();


            completed =
                false;


            minimumDamage =
                ResolveMinimumDamage();


            BossCriticalKillTracker
                .BossCriticalKillDetected +=
                OnBossCriticalKillDetected;
        }


        // =========================================================
        // DESINSTALAR
        // =========================================================

        public override void OnUninstall()
        {
            BossCriticalKillTracker
                .BossCriticalKillDetected -=
                OnBossCriticalKillDetected;


            base.OnUninstall();
        }


        // =========================================================
        // CRÍTICO MORTAL CONTRA JEFE DETECTADO
        // =========================================================

        private void OnBossCriticalKillDetected(
            float damage
        )
        {
            if (completed)
            {
                return;
            }


            /*
             * IMPORTANTE:
             *
             * Este valor corresponde a UN único
             * golpe crítico mortal.
             *
             * No existe acumulación.
             * No se suman golpes.
             * No se suman jugadores.
             */
            if (damage < minimumDamage)
            {
                return;
            }


            completed =
                true;


            Grant();


            ServerTryToCompleteActivity();
        }


        // =========================================================
        // LEER OBJETIVO DESDE SURVIVORS.JSON
        // =========================================================

        private static float ResolveMinimumDamage()
        {
            SurvivorJsonFile config =
                SurvivorJsonManager.CurrentConfig;


            if (
                config == null ||
                config.AvailableSurvivors == null
            )
            {
                return DefaultMinimumDamage;
            }


            foreach (
                SurvivorJsonEntry entry
                in config.AvailableSurvivors.Values
            )
            {
                SurvivorChallengeJson challenge =
                    entry?.Challenge;


                if (
                    challenge == null ||
                    !challenge.Enabled
                )
                {
                    continue;
                }


                if (
                    !string.Equals(
                        challenge.Type,
                        "BossCriticalKill",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    continue;
                }


                JObject parameters =
                    challenge.Parameters;


                if (parameters == null)
                {
                    continue;
                }


                JToken damageToken =
                    parameters["minimumDamage"];


                if (damageToken == null)
                {
                    continue;
                }


                try
                {
                    float configuredDamage =
                        damageToken.Value<float>();


                    if (configuredDamage > 0f)
                    {
                        return configuredDamage;
                    }
                }
                catch
                {
                    /*
                     * Si el JSON contiene un valor
                     * inválido utilizamos 4444.
                     */
                }
            }


            return DefaultMinimumDamage;
        }
    }
}