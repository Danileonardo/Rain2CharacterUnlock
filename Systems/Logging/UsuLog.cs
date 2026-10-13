using BepInEx.Configuration;
using BepInEx.Logging;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Política central de logs de USU.
    ///
    /// Normal (default): arranque/cambios importantes + warnings/errores.
    /// Diagnostic: resúmenes técnicos G3-G8/C2/C4/C5.
    /// Verbose: detalle por survivor/asset/provider.
    /// </summary>
    public static class UsuLog
    {
        private static ConfigFile boundConfig;
        private static ConfigEntry<bool> diagnosticLogging;
        private static ConfigEntry<bool> verboseLogging;
        private static ConfigEntry<bool> extendedAuditLogging;

        public static bool DiagnosticEnabled =>
            (diagnosticLogging != null && diagnosticLogging.Value) ||
            VerboseEnabled;

        public static bool VerboseEnabled =>
            verboseLogging != null &&
            verboseLogging.Value;

        // Auditorías masivas (skills, plantillas, tokens, ContentPacks).
        // Es independiente de Verbose: facilita depurar sin saturar cada inicio.
        public static bool ExtendedAuditEnabled =>
            extendedAuditLogging != null && extendedAuditLogging.Value;

        public static ConfigEntry<bool> ExtendedAuditLoggingEntry =>
            extendedAuditLogging;

        public static ConfigEntry<bool> DiagnosticLoggingEntry =>
            diagnosticLogging;

        public static ConfigEntry<bool> VerboseLoggingEntry =>
            verboseLogging;

        public static void Initialize(ConfigFile config)
        {
            if (config == null)
            {
                return;
            }

            boundConfig = config;

            diagnosticLogging =
                config.Bind(
                    "Logging",
                    "DiagnosticLogging",
                    false,
                    "Muestra los resúmenes técnicos de los sistemas internos de USU. Déjalo desactivado para un arranque limpio."
                );

            verboseLogging =
                config.Bind(
                    "Logging",
                    "VerboseLogging",
                    false,
                    "Muestra el detalle técnico completo de USU. Implica también el nivel Diagnóstico. Úsalo sólo al depurar."
                );

            extendedAuditLogging =
                config.Bind(
                    "Logging",
                    "ExtendedAuditLogging",
                    false,
                    "Imprime auditorías extensas: cada habilidad, plantilla, puntuación, ContentPack y textos completos de localización. Independiente de Logs detallados; requiere reiniciar para auditar el arranque."
                );
        }

        public static void ToggleDiagnostic()
        {
            if (diagnosticLogging != null)
            {
                diagnosticLogging.Value = !diagnosticLogging.Value;
                boundConfig?.Save();
            }
        }

        public static void ToggleVerbose()
        {
            if (verboseLogging != null)
            {
                verboseLogging.Value = !verboseLogging.Value;
                boundConfig?.Save();
            }
        }

        public static void ToggleExtendedAudit()
        {
            if (extendedAuditLogging != null)
            {
                extendedAuditLogging.Value = !extendedAuditLogging.Value;
                boundConfig?.Save();
            }
        }


        public static void Diagnostic(
            ManualLogSource logger,
            string message
        )
        {
            if (!DiagnosticEnabled)
            {
                return;
            }

            logger?.LogInfo(message);
        }

        // Sólo se usa para volcados masivos; los resúmenes permanecen
        // disponibles con DiagnosticLogging/VerboseLogging.
        public static void ExtendedAudit(
            ManualLogSource logger,
            string message
        )
        {
            if (!ExtendedAuditEnabled)
            {
                return;
            }

            logger?.LogInfo(message);
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
