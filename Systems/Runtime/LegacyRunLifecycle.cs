using System;

using BepInEx.Logging;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Hub del ciclo de vida de Run para los trackers Legacy y recompensas
    /// que históricamente se inicializan antes de SessionMissionRegistry.
    ///
    /// Se instala antes de esos consumidores y, por tanto, conserva su posición
    /// relativa frente al snapshot de sesión: primero se reinicia/prepara el
    /// estado Legacy, después SessionMissionRegistry congela la configuración
    /// efectiva y finalmente MissionRunLifecycle reconstruye Mission V2.
    /// </summary>
    public static class LegacyRunLifecycle
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
                "[RUNTIME] LegacyRunLifecycle inicializado."
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
