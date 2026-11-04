using System;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Verifica la extracción real de la Luna, no una victoria genérica.
    /// Se reutiliza entre HUNK (ruta de escape) y la campaña de AH-64.
    /// </summary>
    internal static class MoonEscapeEndingMatcher
    {
        internal static bool IsMoonEscape(GameEndingDef ending)
        {
            if (ending == null || !ending.isWin) return false;

            // La evacuación de Mithrix utiliza MainEnding; otras victorias
            // (Voidling, renacimiento, finales de otros mods) no son suficientes.
            string endingName = ending.cachedName ?? "";
            if (!string.Equals(endingName, "MainEnding",
                StringComparison.OrdinalIgnoreCase)) return false;

            // Se exige además que el final haya sucedido en la Luna.
            // El rastreador toma el nombre de la escena activa, no del texto local.
            string scene = MissionStageRuntimeTracker.CurrentStageName ?? "";
            return string.Equals(scene, "moon2", StringComparison.OrdinalIgnoreCase);
        }
    }
}
