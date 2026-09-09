using BepInEx.Configuration;
using BepInEx.Logging;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Política central de logs de USU.
    ///
    /// Por defecto dejamos en consola solamente información útil para el
    /// jugador/desarrollador (resúmenes, cambios de asignación, warnings y
    /// errores). Los detalles de diagnóstico de arranque se pueden volver a
    /// habilitar desde el cfg de BepInEx con Logging.VerboseLogging=true.
    /// </summary>
    public static class UsuLog
    {
        private static ConfigEntry<bool> verboseLogging;

        public static bool VerboseEnabled =>
            verboseLogging != null &&
            verboseLogging.Value;

        public static void Initialize(
            ConfigFile config
        )
        {
            if (config == null)
            {
                return;
            }

            verboseLogging =
                config.Bind(
                    "Logging",
                    "VerboseLogging",
                    false,
                    "Muestra detalles de diagnóstico de Universal Survivor Unlocks. " +
                    "Déjalo en false para una consola limpia; actívalo sólo al depurar."
                );
        }

        public static void Verbose(
            ManualLogSource logger,
            string message
        )
        {
            if (!VerboseEnabled)
            {
                return;
            }

            logger?.LogInfo(message);
        }
    }
}
