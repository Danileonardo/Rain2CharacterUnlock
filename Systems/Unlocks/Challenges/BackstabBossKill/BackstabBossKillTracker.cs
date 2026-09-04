using System;
using BepInEx.Logging;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    public static class BackstabBossKillTracker
    {
        private static ManualLogSource logger;
        private static bool initialized;


        // =========================================================
        // EVENTO
        // =========================================================

        public static event Action
            BackstabBossKillDetected;


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
                "BackstabBossKillTracker inicializado."
            );
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


            if (!MissionRuntimeActivityPlan.IsTypeActive("BackstabBossKill"))
            {
                return;
            }


            if (damageReport == null)
            {
                return;
            }


            CharacterBody attackerBody =
                damageReport.attackerBody;


            CharacterBody victimBody =
                damageReport.victimBody;


            if (
                attackerBody == null ||
                victimBody == null
            )
            {
                return;
            }


            // =====================================================
            // ATACANTE = JUGADOR
            // =====================================================

            if (
                damageReport.attackerTeamIndex !=
                TeamIndex.Player
            )
            {
                return;
            }


            // =====================================================
            // VÍCTIMA = BOSS
            // =====================================================

            if (!damageReport.victimIsBoss)
            {
                return;
            }


            // =====================================================
            // DEBE SER BANDIT
            // =====================================================

            string attackerBodyName =
                attackerBody.name;


            if (
                attackerBodyName.EndsWith(
                    "(Clone)",
                    StringComparison.Ordinal
                )
            )
            {
                attackerBodyName =
                    attackerBodyName.Substring(
                        0,
                        attackerBodyName.Length -
                        "(Clone)".Length
                    );
            }


            if (
                !string.Equals(
                    attackerBodyName,
                    "Bandit2Body",
                    StringComparison.Ordinal
                )
            )
            {
                return;
            }


            // =====================================================
            // DEBE SER DAÑO DE LA SECUNDARIA
            // =====================================================
            //
            // DamageSource utiliza flags:
            //
            // Primary   = 1
            // Secondary = 2
            // Utility   = 4
            // Special   = 8
            //
            // Comprobamos que el golpe mortal provenga
            // realmente de una habilidad Secondary.
            // =====================================================

            int damageSource =
                (int)damageReport
                    .damageInfo
                    .damageType
                    .damageSource;


            if (
                (damageSource & 2) == 0
            )
            {
                return;
            }


            // =====================================================
            // DEBE TENER SERRATED DAGGER EQUIPADA
            // =====================================================

            SkillLocator skillLocator =
                attackerBody.skillLocator;


            if (
                skillLocator == null ||
                skillLocator.secondary == null ||
                skillLocator.secondary.skillDef == null
            )
            {
                return;
            }


            string secondaryToken =
                skillLocator
                    .secondary
                    .skillDef
                    .skillNameToken;


            /*
             * BANDIT2_SECONDARY_NAME
             * = Serrated Dagger
             *
             * BANDIT2_SECONDARY_ALT_NAME
             * = Serrated Shiv
             */
            if (
                !string.Equals(
                    secondaryToken,
                    "BANDIT2_SECONDARY_NAME",
                    StringComparison.Ordinal
                )
            )
            {
                return;
            }


            // =====================================================
            // DEBE SER UN BACKSTAB REAL
            // =====================================================
            //
            // Utilizamos el mismo BackstabManager de Risk of Rain 2
            // que utiliza el propio mod de Spy.
            //
            // Spy calcula la dirección desde el atacante hacia
            // el punto de impacto y deja que BackstabManager
            // determine si realmente es una posición válida
            // para una puñalada por la espalda.
            //
            // Esto evita inventar nuestro propio ángulo y hace
            // que laterales o ataques frontales sean rechazados
            // según las reglas reales de Backstab.
            // =====================================================

            Vector3 attackerToHit =
                damageReport.damageInfo.position -
                attackerBody.corePosition;


            if (
                attackerToHit.sqrMagnitude <=
                0.0001f
            )
            {
                return;
            }


            if (
                !BackstabManager.IsBackstab(
                    attackerToHit,
                    victimBody
                )
            )
            {
                return;
            }


            // =====================================================
            // VALIDACIÓN EXTRA: BACKSTAB DE BANDIT
            // =====================================================
            //
            // Bandit's Backstab convierte ataques desde
            // detrás en críticos.
            //
            // Esto ayuda a verificar que el golpe letal
            // realmente se registró bajo esa condición.
            // =====================================================

            if (!damageReport.damageInfo.crit)
            {
                return;
            }


            string victimName =
                victimBody.gameObject != null
                    ? victimBody.gameObject.name
                    : victimBody.name;


            logger?.LogInfo(
                $"[BackstabBossKill] APUÑALADA MORTAL | " +
                $"Atacante: {attackerBodyName} | " +
                $"Jefe: {victimName} | " +
                $"Skill: Serrated Dagger | " +
                $"Backstab: válido"
            );


            BackstabBossKillDetected?.Invoke();
        }
    }
}