using System;
using BepInEx.Logging;
using RoR2;
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

            GameplayEventHub.HealingProcessed += OnHealingProcessed;

            LegacyRunLifecycle.RunStarted += OnRunStart;
            LegacyRunLifecycle.RunDestroyed += OnRunEnd;

            UsuLog.Verbose(
                logger,
                "HealHealthTracker inicializado."
            );
        }

        private static void OnRunStart(Run run)
        {
            TotalHealing = 0f;

            if (MissionRuntimeActivityPlan.IsTypeActive("HealHealth"))
            {
                logger?.LogInfo("[HealHealth] Nueva run | Total reiniciado.");
            }
        }

        private static void OnRunEnd(Run run)
        {
            if (MissionRuntimeActivityPlan.IsTypeActive("HealHealth"))
            {
                logger?.LogInfo(
                    $"[HealHealth] Run finalizada | Total: {TotalHealing:0.##}"
                );
            }

            TotalHealing = 0f;
        }

        private static void OnHealingProcessed(
            GameplayHealingEvent healingEvent
        )
        {
            if (
                !NetworkServer.active ||
                Run.instance == null ||
                healingEvent == null ||
                !MissionRuntimeActivityPlan.IsTypeActive("HealHealth")
            )
            {
                return;
            }

            CharacterBody body = healingEvent.Body;

            if (
                body == null ||
                body.teamComponent == null ||
                body.teamComponent.teamIndex != TeamIndex.Player ||
                !healingEvent.NonRegen ||
                healingEvent.ActualHealing <= 0f
            )
            {
                return;
            }

            TotalHealing += healingEvent.ActualHealing;

            if (
                MissionLogLimiter.ShouldLogMilestone(
                    "legacy-healing",
                    TotalHealing,
                    1000d
                )
            )
            {
                logger?.LogInfo(
                    $"[HealHealth] PROGRESO | Total: {TotalHealing:0.##}"
                );
            }

            HealingChanged?.Invoke(
                healingEvent.ActualHealing,
                TotalHealing
            );
        }
    }
}
