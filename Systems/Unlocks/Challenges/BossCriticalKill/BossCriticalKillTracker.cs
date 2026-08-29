using System;
using BepInEx.Logging;
using RoR2;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    public static class BossCriticalKillTracker
    {
        private static ManualLogSource logger;
        private static bool initialized;


        // =========================================================
        // EVENTO
        // =========================================================
        //
        // Se dispara cuando UN jugador consigue:
        //
        // - matar a un jefe
        // - con un golpe crítico
        //
        // El daño mínimo se comprobará posteriormente
        // en BossCriticalKillServerAchievement.
        // =========================================================

        public static event Action<float>
            BossCriticalKillDetected;


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


            logger?.LogInfo(
                "BossCriticalKillTracker inicializado."
            );
        }


        // =========================================================
        // MUERTE DE PERSONAJE
        // =========================================================

        private static void OnCharacterDeathGlobal(
            DamageReport damageReport
        )
        {
            // =====================================================
            // SÓLO EL SERVIDOR / HOST
            // =====================================================

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


            // =====================================================
            // NECESITAMOS VÍCTIMA Y ATACANTE
            // =====================================================

            CharacterBody victimBody =
                damageReport.victimBody;


            CharacterBody attackerBody =
                damageReport.attackerBody;


            if (
                victimBody == null ||
                attackerBody == null
            )
            {
                return;
            }


            // =====================================================
            // EL ATACANTE DEBE PERTENECER
            // AL EQUIPO DE LOS JUGADORES
            // =====================================================

            if (
                damageReport.attackerTeamIndex !=
                TeamIndex.Player
            )
            {
                return;
            }


            // =====================================================
            // LA VÍCTIMA DEBE SER UN JEFE
            // =====================================================

            if (!damageReport.victimIsBoss)
            {
                return;
            }


            // =====================================================
            // EL GOLPE MORTAL DEBE SER CRÍTICO
            // =====================================================

            if (!damageReport.damageInfo.crit)
            {
                return;
            }


            // =====================================================
            // DAÑO DEL ÚNICO GOLPE MORTAL
            // =====================================================
            //
            // IMPORTANTE:
            //
            // NO acumulamos daño.
            // NO sumamos jugadores.
            // NO sumamos varios críticos.
            //
            // damageDealt pertenece al golpe que provocó
            // este DamageReport de muerte.
            // =====================================================

            float damage =
                damageReport.damageDealt;


            if (damage <= 0f)
            {
                return;
            }


            // =====================================================
            // LOG
            // =====================================================

            string attackerName =
                attackerBody.gameObject != null
                    ? attackerBody.gameObject.name
                    : attackerBody.name;


            string victimName =
                victimBody.gameObject != null
                    ? victimBody.gameObject.name
                    : victimBody.name;


            logger?.LogInfo(
                $"[BossCriticalKill] CRÍTICO MORTAL CONTRA JEFE | " +
                $"Atacante: {attackerName} | " +
                $"Jefe: {victimName} | " +
                $"Daño: {damage:0.##}"
            );


            // =====================================================
            // AVISAR A LOS ACHIEVEMENTS
            // =====================================================

            BossCriticalKillDetected?.Invoke(
                damage
            );
        }
    }
}