using System;
using BepInEx.Logging;
using RoR2;
using UnityEngine;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Traduce un resultado real de la misión de HUNK a una medalla narrativa.
    /// NO cambia el AchievementDef, el nombre de la misión ni las recompensas.
    /// </summary>
    internal static class HunkMissionMedalReporter
    {
        private const string HunkBody = "RobHunkBody";
        private const string EscapeRoute = "hunk_escape";
        private const string SacrificeRoute = "hunk_obliterate";
        private const string Escape = "escape";
        private const string Sacrifice = "sacrifice";

        internal static string ResolveOutcome(string bodyName, string routeId)
        {
            if (!string.Equals(bodyName, HunkBody, StringComparison.Ordinal))
                return string.Empty;
            if (string.Equals(routeId, EscapeRoute, StringComparison.Ordinal))
                return Escape;
            if (string.Equals(routeId, SacrificeRoute, StringComparison.Ordinal))
                return Sacrifice;
            return string.Empty;
        }

        internal static void ShowIfApplicable(
            string bodyName, string outcome, ManualLogSource logger)
        {
            if (!string.Equals(bodyName, HunkBody, StringComparison.Ordinal) ||
                (outcome != Escape && outcome != Sacrifice))
                return;

            try
            {
                bool spanish = (Language.currentLanguageName ?? string.Empty)
                    .StartsWith("es", StringComparison.OrdinalIgnoreCase);
                bool escaped = outcome == Escape;

                MissionMedalNotificationManager.Show(new MissionMedalNotification
                {
                    Category = escaped
                        ? (spanish ? "UMBRELLA · PERSONAL APROBADO" : "UMBRELLA · RECRUITMENT NOTICE")
                        : (spanish ? "UMBRELLA · DIRECTIVA DE CONTINGENCIA" : "UMBRELLA · CONTINGENCY DIRECTIVE"),
                    Title = escaped
                        ? (spanish ? "Reclutamiento autorizado" : "Recruitment Authorized")
                        : (spanish ? "Solo queda La Parca" : "Only the Grim Reaper Remains"),
                    Description = escaped
                        ? (spanish ? "Operativo aprobado. Preséntese ante HUNK." :
                            "Operative approved. Report to HUNK.")
                        : (spanish ? "Otro operativo perdido. Desplieguen a HUNK." :
                            "Another operative lost. Deploy HUNK."),
                    Accent = escaped
                        ? new Color(0.80f, 0.72f, 0.44f, 1f)
                        : new Color(0.74f, 0.35f, 0.36f, 1f),
                    Symbol = escaped ? "★" : "◆",
                    DurationSeconds = 5.5f
                });

                logger?.LogInfo("[USU MEDAL] HUNK | Desenlace: " + outcome +
                    " | Solo notificación; recompensa original intacta.");
            }
            catch (Exception ex)
            {
                // Un error exclusivamente visual NO debe afectar al unlock.
                logger?.LogWarning("[USU MEDAL] No pudo mostrarse la medalla de HUNK: " + ex.Message);
            }
        }
    }
}
