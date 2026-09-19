using System;

using BepInEx.Logging;

using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Punto único de enlace entre Mission V2 y el ciclo de vida global de Run.
    ///
    /// Antes, cada componente Mission V2 se suscribía directamente a
    /// Run.onRunStartGlobal / Run.onRunDestroyGlobal. Eso hacía más difícil
    /// auditar el orden de ejecución y distinguir hooks de infraestructura de
    /// hooks de gameplay.
    ///
    /// Esta clase conserva el mismo modelo de eventos y orden de suscripción,
    /// pero Mission V2 mantiene sólo un par de enlaces directos con RoR2.
    /// SessionMissionRegistry permanece conectado directamente y se inicializa
    /// antes que este lifecycle para que sus snapshots estén preparados antes
    /// de que ActivityPlan/Catalog reconstruyan el runtime de la misión.
    /// </summary>
    public static class MissionRunLifecycle
    {
        private static ManualLogSource logger;
        private static bool initialized;

        public static event Action<Run> RunStarted;
        public static event Action<Run> RunDestroyed;

        public static void Initialize(
            ManualLogSource log
        )
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            logger = log;

            Run.onRunStartGlobal += OnRunStartGlobal;
            Run.onRunDestroyGlobal += OnRunDestroyGlobal;

            UsuLog.Verbose(
                logger,
                "[MISSION RUNTIME] RunLifecycle inicializado."
            );
        }

        private static void OnRunStartGlobal(
            Run run
        )
        {
            RunStarted?.Invoke(run);
        }

        private static void OnRunDestroyGlobal(
            Run run
        )
        {
            RunDestroyed?.Invoke(run);
        }
    }
}
