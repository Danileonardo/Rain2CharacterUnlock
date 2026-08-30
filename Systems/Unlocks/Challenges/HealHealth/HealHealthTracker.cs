using System;
using BepInEx.Logging;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    public static class HealHealthTracker
    {
        private static ManualLogSource logger;
        private static bool initialized;

        public static float TotalHealing { get; private set; }

        public static event Action<float, float> HealingChanged;

        public static void Initialize(ManualLogSource pluginLogger)
        {
            if (initialized)
                return;

            initialized = true;
            logger = pluginLogger;

            On.RoR2.HealthComponent.Heal += HealthComponent_Heal;

            Run.onRunStartGlobal += OnRunStart;
            Run.onRunDestroyGlobal += OnRunEnd;

            logger?.LogInfo(
                "HealHealthTracker inicializado."
            );
        }

        private static void OnRunStart(Run run)
        {
            TotalHealing = 0f;

            logger?.LogInfo(
                "[HealHealth] Nueva run detectada | Total reiniciado a 0."
            );
        }

        private static void OnRunEnd(Run run)
        {
            logger?.LogInfo(
                $"[HealHealth] Run finalizada | Total observado: {TotalHealing:0.##}"
            );

            TotalHealing = 0f;
        }

        private static float HealthComponent_Heal(
            On.RoR2.HealthComponent.orig_Heal orig,
            HealthComponent self,
            float amount,
            ProcChainMask procChainMask,
            bool nonRegen
        )
        {
            if (self == null)
            {
                return orig(
                    self,
                    amount,
                    procChainMask,
                    nonRegen
                );
            }

            float healthBefore = self.health;

            float result = orig(
                self,
                amount,
                procChainMask,
                nonRegen
            );

            if (!NetworkServer.active)
                return result;

            if (Run.instance == null)
                return result;

            CharacterBody body = self.body;

            if (body == null)
                return result;

            if (body.teamComponent == null)
                return result;

            if (
                body.teamComponent.teamIndex
                != TeamIndex.Player
            )
            {
                return result;
            }


            // =========================================================
            // IGNORAR REGENERACIÓN PASIVA
            // =========================================================
            //
            // nonRegen == false
            // corresponde a curación proveniente de regeneración.
            //
            // Para Oración de Esperanza queremos contar
            // curaciones activas y no la regeneración natural.
            // =========================================================

            if (!nonRegen)
            {
                return result;
            }


            float healthAfter = self.health;

            float actualHealing =
                Mathf.Max(
                    0f,
                    healthAfter - healthBefore
                );

            if (actualHealing <= 0f)
                return result;

            TotalHealing += actualHealing;

            string bodyName =
                body.gameObject != null
                    ? body.gameObject.name
                    : body.name;

            logger?.LogInfo(
                $"[HealHealth] CURACIÓN | " +
                $"Objetivo: {bodyName} | " +
                $"Antes: {healthBefore:0.##} | " +
                $"Después: {healthAfter:0.##} | " +
                $"Solicitado: {amount:0.##} | " +
                $"Real: {actualHealing:0.##} | " +
                $"nonRegen: {nonRegen} | " +
                $"TOTAL RUN: {TotalHealing:0.##}"
            );

            HealingChanged?.Invoke(
                actualHealing,
                TotalHealing
            );

            return result;
        }
    }
}