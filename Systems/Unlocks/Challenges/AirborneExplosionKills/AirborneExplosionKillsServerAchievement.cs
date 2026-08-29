using System;
using Newtonsoft.Json.Linq;
using RoR2;
using RoR2.Achievements;

namespace UniversalSurvivorUnlocks
{
    public class AirborneExplosionKillsServerAchievement
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


            AirborneExplosionKillTracker
                .ExplosionKillCountChanged +=
                OnExplosionKillCountChanged;


            /*
             * Por seguridad:
             *
             * si el achievement se instala cuando
             * algún jugador ya lleva progreso,
             * revisamos la mayor racha INDIVIDUAL.
             */
            CheckProgress(
                AirborneExplosionKillTracker
                    .GetHighestCurrentCount()
            );
        }


        // =========================================================
        // DESINSTALAR
        // =========================================================

        public override void OnUninstall()
        {
            AirborneExplosionKillTracker
                .ExplosionKillCountChanged -=
                OnExplosionKillCountChanged;


            base.OnUninstall();
        }


        // =========================================================
        // CAMBIÓ EL CONTADOR DE UN JUGADOR
        // =========================================================

        private void OnExplosionKillCountChanged(
            CharacterMaster playerMaster,
            int currentCount
        )
        {
            /*
             * currentCount pertenece solamente
             * al jugador recibido.
             *
             * Nunca se suman los contadores
             * de distintos jugadores.
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
        // LEER AMOUNT DESDE SURVIVORS.JSON
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
                        "AirborneExplosionKills",
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
                     * Si el JSON contiene un valor
                     * inválido, usamos 15.
                     */
                }
            }


            return DefaultTargetAmount;
        }
    }
}