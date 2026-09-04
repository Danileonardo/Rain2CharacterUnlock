using System;
using System.Collections.Generic;
using BepInEx.Logging;
using RoR2;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    public static class AirborneExplosionKillTracker
    {
        private static ManualLogSource logger;
        private static bool initialized;


        // =========================================================
        // CONTADOR INDIVIDUAL POR JUGADOR
        // =========================================================

        private static readonly Dictionary<
            CharacterMaster,
            int
        > PlayerKillCounts =
            new Dictionary<CharacterMaster, int>();


        // =========================================================
        // EVENTO
        // =========================================================
        //
        // CharacterMaster = jugador dueño
        // int             = cantidad actual
        //
        // Nunca representa el total del equipo.
        // =========================================================

        public static event Action<
            CharacterMaster,
            int
        > ExplosionKillCountChanged;


        // =========================================================
        // INICIALIZAR
        // =========================================================

        public static void Initialize(
            ManualLogSource pluginLogger
        )
        {
            if (initialized)
            {
                return;
            }


            initialized =
                true;


            logger =
                pluginLogger;


            ExplosionKillTracker.LethalBlastDetected +=
                OnLethalBlastDetected;


            RoR2Application.onFixedUpdate +=
                FixedUpdate;


            Run.onRunStartGlobal +=
                OnRunStart;


            Run.onRunDestroyGlobal +=
                OnRunEnd;


            logger?.LogInfo(
                "AirborneExplosionKillTracker inicializado."
            );
        }


        // =========================================================
        // INICIO DE RUN
        // =========================================================

        private static void OnRunStart(
            Run run
        )
        {
            PlayerKillCounts.Clear();


            logger?.LogInfo(
                "[AirborneExplosionKills] Nueva run | " +
                "Contadores reiniciados."
            );
        }


        // =========================================================
        // FIN DE RUN
        // =========================================================

        private static void OnRunEnd(
            Run run
        )
        {
            PlayerKillCounts.Clear();
        }


        // =========================================================
        // BLAST MORTAL
        // =========================================================

        private static void OnLethalBlastDetected(
            CharacterMaster playerOwner,
            DamageReport damageReport,
            BlastAttack blastAttack
        )
        {
            if (!NetworkServer.active)
            {
                return;
            }


            if (Run.instance == null)
            {
                return;
            }


            if (!MissionRuntimeActivityPlan.IsTypeActive("AirborneExplosionKills"))
            {
                return;
            }


            if (playerOwner == null)
            {
                return;
            }


            CharacterBody playerBody =
                playerOwner.GetBody();


            if (playerBody == null)
            {
                return;
            }


            // =====================================================
            // IMPORTANTE:
            //
            // Comprobamos al JUGADOR.
            //
            // Nunca comprobamos si el dron,
            // misil o minion está flotando.
            // =====================================================

            if (
                playerBody.characterMotor == null
            )
            {
                return;
            }


            // =====================================================
            // SI ESTÁ EN EL SUELO, NO CUENTA
            // =====================================================

            if (
                playerBody
                    .characterMotor
                    .isGrounded
            )
            {
                return;
            }


            // =====================================================
            // SUMAR UNA MUERTE
            // =====================================================

            int currentCount =
                0;


            PlayerKillCounts.TryGetValue(
                playerOwner,
                out currentCount
            );


            currentCount++;


            PlayerKillCounts[
                playerOwner
            ] =
                currentCount;


            string playerName =
                GetPlayerName(
                    playerOwner
                );


            string victimName =
                damageReport?.victimBody != null
                    ? damageReport.victimBody.name
                    : "<desconocido>";


            if (
                MissionLogLimiter.ShouldLogMilestone(
                    "legacy-airborne:" + playerOwner.GetInstanceID(),
                    currentCount,
                    5d
                )
            )
            {
                logger?.LogInfo(
                    "[AirborneExplosionKills] PROGRESO | " +
                    $"Jugador: {playerName} | " +
                    $"Contador: {currentCount}"
                );
            }


            ExplosionKillCountChanged?.Invoke(
                playerOwner,
                currentCount
            );
        }


        // =========================================================
        // DETECTAR CUANDO UN JUGADOR TOCA EL SUELO
        // =========================================================

        private static void FixedUpdate()
        {
            if (!NetworkServer.active)
            {
                return;
            }


            if (Run.instance == null)
            {
                return;
            }


            if (!MissionRuntimeActivityPlan.IsTypeActive("AirborneExplosionKills"))
            {
                return;
            }


            foreach (
                PlayerCharacterMasterController controller
                in PlayerCharacterMasterController.instances
            )
            {
                if (
                    controller == null ||
                    controller.master == null
                )
                {
                    continue;
                }


                CharacterMaster master =
                    controller.master;


                CharacterBody body =
                    master.GetBody();


                if (
                    body == null ||
                    body.characterMotor == null
                )
                {
                    continue;
                }


                /*
                 * Si sigue en el aire,
                 * no hacemos nada.
                 */
                if (
                    !body.characterMotor.isGrounded
                )
                {
                    continue;
                }


                int currentCount =
                    0;


                if (
                    !PlayerKillCounts.TryGetValue(
                        master,
                        out currentCount
                    )
                )
                {
                    continue;
                }


                if (currentCount <= 0)
                {
                    continue;
                }


                // =================================================
                // TOCÓ SUELO
                // =================================================

                PlayerKillCounts[
                    master
                ] =
                    0;


                string playerName =
                    GetPlayerName(
                        master
                    );


                logger?.LogInfo(
                    "[AirborneExplosionKills] " +
                    "JUGADOR TOCÓ EL SUELO | " +
                    $"Jugador: {playerName} | " +
                    $"Racha anterior: {currentCount} | " +
                    "Contador reiniciado."
                );


                ExplosionKillCountChanged?.Invoke(
                    master,
                    0
                );
            }
        }


        // =========================================================
        // OBTENER CONTADOR DE UN JUGADOR
        // =========================================================

        public static int GetCurrentCount(
            CharacterMaster playerMaster
        )
        {
            if (playerMaster == null)
            {
                return 0;
            }


            int count =
                0;


            PlayerKillCounts.TryGetValue(
                playerMaster,
                out count
            );


            return count;
        }


        // =========================================================
        // MAYOR RACHA ACTUAL DE UN SOLO JUGADOR
        // =========================================================

        public static int GetHighestCurrentCount()
        {
            int highest =
                0;


            foreach (
                KeyValuePair<
                    CharacterMaster,
                    int
                > pair
                in PlayerKillCounts
            )
            {
                if (
                    pair.Value >
                    highest
                )
                {
                    highest =
                        pair.Value;
                }
            }


            return highest;
        }


        // =========================================================
        // NOMBRE DEL JUGADOR
        // =========================================================

        private static string GetPlayerName(
            CharacterMaster master
        )
        {
            foreach (
                PlayerCharacterMasterController controller
                in PlayerCharacterMasterController.instances
            )
            {
                if (
                    controller == null ||
                    controller.master != master
                )
                {
                    continue;
                }


                string displayName =
                    controller.GetDisplayName();


                if (
                    !string.IsNullOrWhiteSpace(
                        displayName
                    )
                )
                {
                    return displayName;
                }
            }


            return
                master.name;
        }
    }
}