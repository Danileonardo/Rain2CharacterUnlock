using System;
using System.Collections.Generic;

using BepInEx.Logging;

using RoR2;

using UnityEngine.Networking;


namespace UniversalSurvivorUnlocks
{
    public static class KillEnemiesTracker
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
            new Dictionary<
                CharacterMaster,
                int
            >();


        // =========================================================
        // EVENTO
        // =========================================================
        //
        // CharacterMaster = jugador propietario de la baja.
        // int             = bajas actuales de ESE jugador.
        //
        // Las bajas de jugadores diferentes NO se suman.
        // =========================================================

        public static event Action<
            CharacterMaster,
            int
        > KillCountChanged;


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


            GlobalEventManager
                .onCharacterDeathGlobal +=
                OnCharacterDeathGlobal;


            Run.onRunStartGlobal +=
                OnRunStart;


            Run.onRunDestroyGlobal +=
                OnRunEnd;


            logger?.LogInfo(
                "KillEnemiesTracker inicializado."
            );
        }


        // =========================================================
        // RUN
        // =========================================================

        private static void OnRunStart(
            Run run
        )
        {
            PlayerKillCounts.Clear();


            logger?.LogInfo(
                "[KillEnemies] Nueva run | " +
                "Contadores reiniciados."
            );
        }


        private static void OnRunEnd(
            Run run
        )
        {
            PlayerKillCounts.Clear();
        }


        // =========================================================
        // MUERTE
        // =========================================================

        private static void OnCharacterDeathGlobal(
            DamageReport damageReport
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


            if (damageReport == null)
            {
                return;
            }


            CharacterBody victimBody =
                damageReport.victimBody;


            if (
                victimBody == null ||
                victimBody.teamComponent == null
            )
            {
                return;
            }


            // =====================================================
            // LA VÍCTIMA DEBE SER ENEMIGA DEL EQUIPO PLAYER
            // =====================================================

            TeamMask enemyTeams =
                TeamMask.GetEnemyTeams(
                    TeamIndex.Player
                );


            if (
                !enemyTeams.HasTeam(
                    victimBody
                        .teamComponent
                        .teamIndex
                )
            )
            {
                return;
            }


            // =====================================================
            // RESOLVER JUGADOR RESPONSABLE
            // =====================================================
            //
            // Esto permite:
            //
            // Jugador -> jugador
            // Drone   -> propietario
            // Minion  -> propietario
            //
            // =====================================================

            CharacterMaster attackerMaster =
                damageReport.attackerMaster;


            if (
                attackerMaster == null &&
                damageReport.attackerBody != null
            )
            {
                attackerMaster =
                    damageReport
                        .attackerBody
                        .master;
            }


            CharacterMaster playerMaster =
                PlayerOwnerResolver
                    .ResolveOwningPlayerMaster(
                        attackerMaster
                    );


            if (playerMaster == null)
            {
                return;
            }


            // =====================================================
            // SUMAR
            // =====================================================

            int currentCount =
                0;


            PlayerKillCounts.TryGetValue(
                playerMaster,
                out currentCount
            );


            currentCount++;


            PlayerKillCounts[
                playerMaster
            ] =
                currentCount;


            logger?.LogInfo(
                "[KillEnemies] BAJA | " +
                $"Jugador: {GetPlayerName(playerMaster)} | " +
                $"Víctima: {victimBody.name} | " +
                $"Contador: {currentCount}"
            );


            KillCountChanged?.Invoke(
                playerMaster,
                currentCount
            );
        }


        // =========================================================
        // OBTENER PROGRESO
        // =========================================================

        public static int GetCurrentCount(
            CharacterMaster playerMaster
        )
        {
            if (playerMaster == null)
            {
                return 0;
            }


            if (
                PlayerKillCounts.TryGetValue(
                    playerMaster,
                    out int count
                )
            )
            {
                return count;
            }


            return 0;
        }


        // =========================================================
        // NOMBRE
        // =========================================================

        private static string GetPlayerName(
            CharacterMaster playerMaster
        )
        {
            foreach (
                PlayerCharacterMasterController controller
                in PlayerCharacterMasterController.instances
            )
            {
                if (
                    controller == null ||
                    controller.master != playerMaster
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
                playerMaster?.name ??
                "<desconocido>";
        }
    }
}