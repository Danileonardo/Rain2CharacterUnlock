using System;
using Newtonsoft.Json.Linq;
using RoR2.Achievements;

namespace UniversalSurvivorUnlocks
{
    public class HealHealthServerAchievement
        : BaseServerAchievement
    {
        private const float DefaultTargetAmount =
            5000f;

        private float targetAmount =
            DefaultTargetAmount;

        private bool completed;


        // =========================================================
        // INSTALAR TRACKER DEL ACHIEVEMENT
        // =========================================================

        public override void OnInstall()
        {
            base.OnInstall();

            completed =
                false;

            targetAmount =
                ResolveTargetAmount();

            HealHealthTracker.HealingChanged +=
                OnHealingChanged;


            /*
             * Por seguridad:
             * si el tracker del achievement se instaló
             * después de que ya existiera progreso,
             * comprobamos el total actual.
             */
            CheckProgress(
                HealHealthTracker.TotalHealing
            );
        }


        // =========================================================
        // DESINSTALAR
        // =========================================================

        public override void OnUninstall()
        {
            HealHealthTracker.HealingChanged -=
                OnHealingChanged;

            base.OnUninstall();
        }


        // =========================================================
        // CAMBIÓ LA CURACIÓN GLOBAL DE LA RUN
        // =========================================================

        private void OnHealingChanged(
            float addedHealing,
            float totalHealing
        )
        {
            CheckProgress(
                totalHealing
            );
        }


        // =========================================================
        // COMPROBAR OBJETIVO
        // =========================================================

        private void CheckProgress(
            float totalHealing
        )
        {
            if (completed)
                return;

            if (totalHealing < targetAmount)
                return;


            completed =
                true;


            /*
             * Este BaseServerAchievement pertenece
             * a un jugador concreto.
             *
             * Todos escuchan el mismo contador global,
             * pero cada uno recibe su propio Grant().
             */
            Grant();

            ServerTryToCompleteActivity();
        }


        // =========================================================
        // LEER CANTIDAD DESDE SURVIVORS.JSON
        // =========================================================

        private static float ResolveTargetAmount()
        {
            SurvivorJsonFile config =
                SurvivorJsonManager.CurrentConfig;

            if (
                config == null ||
                config.AvailableSurvivors == null
            )
            {
                return DefaultTargetAmount;
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
                        "HealHealth",
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


                JToken amountToken =
                    parameters["amount"];

                if (amountToken == null)
                {
                    continue;
                }


                try
                {
                    float amount =
                        amountToken.Value<float>();

                    if (amount > 0f)
                    {
                        return amount;
                    }
                }
                catch
                {
                    // Si el JSON tiene un valor inválido,
                    // usamos el objetivo por defecto.
                }
            }


            return DefaultTargetAmount;
        }
    }
}