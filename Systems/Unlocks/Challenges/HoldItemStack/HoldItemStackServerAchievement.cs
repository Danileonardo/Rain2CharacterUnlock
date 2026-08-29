using System;
using Newtonsoft.Json.Linq;
using RoR2;
using RoR2.Achievements;

namespace UniversalSurvivorUnlocks
{
    public class HoldItemStackServerAchievement
        : BaseServerAchievement
    {
        private const int DefaultTargetAmount =
            15;

        private int targetAmount =
            DefaultTargetAmount;

        private bool completed;


        // =========================================================
        // INSTALAR
        // =========================================================

        public override void OnInstall()
        {
            base.OnInstall();


            completed =
                false;


            targetAmount =
                ResolveTargetAmount();


            HoldItemStackTracker.ItemCountChanged +=
                OnItemCountChanged;


            /*
             * Por seguridad:
             *
             * Si el achievement se instala cuando
             * algún jugador ya posee las 15 bebidas,
             * comprobamos inmediatamente el mayor
             * conteo INDIVIDUAL actual.
             */
            CheckProgress(
                HoldItemStackTracker
                    .GetHighestCurrentCount()
            );
        }


        // =========================================================
        // DESINSTALAR
        // =========================================================

        public override void OnUninstall()
        {
            HoldItemStackTracker.ItemCountChanged -=
                OnItemCountChanged;


            base.OnUninstall();
        }


        // =========================================================
        // CAMBIÓ EL INVENTARIO DE UN JUGADOR
        // =========================================================

        private void OnItemCountChanged(
            CharacterMaster master,
            int currentCount
        )
        {
            /*
             * currentCount pertenece únicamente
             * al CharacterMaster recibido.
             *
             * Nunca se suman jugadores.
             */

            CheckProgress(
                currentCount
            );
        }


        // =========================================================
        // COMPROBAR OBJETIVO
        // =========================================================

        private void CheckProgress(
            int currentCount
        )
        {
            if (completed)
            {
                return;
            }


            if (
                currentCount <
                targetAmount
            )
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

        private static int ResolveTargetAmount()
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
                        "HoldItemStack",
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


                // =================================================
                // ASEGURAR QUE ESTE PRESET SEA ENERGY DRINK
                // =================================================

                JToken itemToken =
                    parameters["item"];


                if (itemToken == null)
                {
                    continue;
                }


                string itemName =
                    itemToken.Value<string>();


                if (
                    !string.Equals(
                        itemName,
                        "SprintBonus",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    continue;
                }


                // =================================================
                // LEER AMOUNT
                // =================================================

                JToken amountToken =
                    parameters["amount"];


                if (amountToken == null)
                {
                    continue;
                }


                try
                {
                    int amount =
                        amountToken.Value<int>();


                    if (amount > 0)
                    {
                        return amount;
                    }
                }
                catch
                {
                    /*
                     * JSON inválido:
                     * usamos 15 por defecto.
                     */
                }
            }


            return DefaultTargetAmount;
        }
    }
}