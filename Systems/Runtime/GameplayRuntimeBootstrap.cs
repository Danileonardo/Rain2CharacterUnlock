using BepInEx.Logging;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Punto único de arranque para los sistemas gameplay/runtime de USU.
    ///
    /// Mantiene deliberadamente el mismo orden de inicialización histórico:
    /// trackers legacy -> sesión/recompensas -> Mission v2 -> router legacy ->
    /// refresh runtime. Centralizarlo evita que Plugin.cs tenga que conocer cada
    /// hook concreto y nos deja una única frontera para futuras optimizaciones.
    /// </summary>
    public static class GameplayRuntimeBootstrap
    {
        private static bool initialized;

        public static void Initialize(
            ManualLogSource logger
        )
        {
            if (initialized)
            {
                return;
            }

            initialized = true;

            // Contextos runtime compartidos por contenido/misiones.
            ContentRuntimeContextTracker.Initialize(
                logger
            );

            // Eventos globales compartidos por trackers Legacy y Mission V2.
            // Debe inicializarse antes de registrar sus consumidores.
            GameplayEventHub.Initialize(
                logger
            );

            // Los trackers Legacy y SessionUnlockManager comparten un único
            // enlace temprano al ciclo de Run. Debe instalarse antes que sus
            // consumidores y antes de SessionMissionRegistry para conservar
            // el orden histórico de preparación de estado/snapshot.
            LegacyRunLifecycle.Initialize(
                logger
            );

            // Schema v1 / recetas Legacy conservadas.
            StatusEffectTracker.Initialize(
                logger
            );

            HealHealthTracker.Initialize(
                logger
            );

            BossCriticalKillTracker.Initialize(
                logger
            );

            HoldItemStackTracker.Initialize(
                logger
            );

            BackstabBossKillTracker.Initialize(
                logger
            );

            ExplosionKillTracker.Initialize(
                logger
            );

            AirborneExplosionKillTracker.Initialize(
                logger
            );

            PrecisionExecutionStreakTracker.Initialize(
                logger
            );

            ScrapItemBossFinisherTracker.Initialize(
                logger
            );

            KillEnemiesTracker.Initialize(
                logger
            );

            // Recompensas y autoridad de sesión.
            SessionUnlockManager.Initialize(
                logger
            );

            SessionMissionRegistry.Initialize(
                logger
            );

            // Mission V2 comparte un único enlace con el ciclo global de Run.
            // SessionMissionRegistry queda antes a propósito: sus snapshots deben
            // existir antes de reconstruir ActivityPlan y RuntimeCatalog.
            MissionRunLifecycle.Initialize(
                logger
            );

            // Mission System v2.
            MissionRuntimeActivityPlan.Initialize(
                logger
            );

            MissionRuntimeCatalog.Initialize(
                logger
            );

            // Este resumen se mantiene visible incluso con VerboseLogging=false.
            MissionPresetLibraryService.LogSummary(
                logger
            );

            MissionRuntimeBudget.Initialize(
                logger
            );

            MissionLogLimiter.Initialize(
                logger
            );

            MissionProgressRegistry.Initialize(
                logger
            );

            MissionStageRuntimeTracker.Initialize(
                logger
            );

            GenericMissionDispatcher.Initialize(
                logger
            );

            SessionMissionLobbySyncManager.Initialize(
                logger
            );

            // Compatibilidad Schema v1 y hot refresh permanecen después de v2,
            // exactamente como en el orden previo de Plugin.OnGameLoaded().
            ChallengeCompletionRouter.Initialize(
                logger
            );

            MissionRuntimeRefreshService.Initialize(
                logger
            );

            UsuLog.Verbose(
                logger,
                "[RUNTIME] Bootstrap gameplay completado."
            );
        }
    }
}
