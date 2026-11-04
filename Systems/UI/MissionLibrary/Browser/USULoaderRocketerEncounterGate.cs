using System;
namespace UniversalSurvivorUnlocks
{
    // Narrative dependency: Captain broadcast -> Rocketer encounter -> Loader III.
    // No extra mission, and no duplicate progress file: Captain's existing profile flag
    // is the single source of truth, so preexisting Captain completions are respected.
    internal static class USULoaderRocketerEncounterGate
    {
        private static bool HasCaptainBroadcast() => VanillaBroadcastUSUDiary.IsUnlockedForLocalProfile();
        private static bool HasRocketerVictory() => USUVanillaDiaryVictoryTracker.IsModDiaryIUnlockedForLocalProfile("RocketSurvivorBody");
        private static bool IsRocketerEncounterUnlocked() => HasCaptainBroadcast() && HasRocketerVictory();
        private static bool IsLoaderIIIUnlocked() => IsRocketerEncounterUnlocked();

        private static string Corrupt(bool es) => es
            ? "<color=#888888>REGISTRO · SEÑAL INTERFERIDA</color>\n<color=#666666>▓▒████░▓██▒████▓▒░███▓▒░</color>\n<color=#BBBBBB>██▓▒ ...a todas las unidades... ▓██</color>\n<color=#666666>▒████▓░▒███▓████▒░▓████</color>\n<color=#BBBBBB>▓▒██ ...mantengan... ███▒▓</color>\n<color=#666666>██▓▒░████▒▓███░▒████▓▒</color>\n<color=#FFFFFF>▒██ ...un lugar al que regresar... ██▓</color>\n<color=#666666>▓▒██████░▓██▒████░▓▒███</color>\n<color=#BBBBBB>███ ...comunicaciones... ░▓██</color>\n<color=#666666>▒▓███░████▓▒██░▓████▒░</color>"
            : "<color=#888888>LOG · SIGNAL INTERFERENCE</color>\n<color=#666666>▓▒████░▓██▒████▓▒░███▓▒░</color>\n<color=#BBBBBB>██▓▒ ...to all units... ▓██</color>\n<color=#666666>▒████▓░▒███▓████▒░▓████</color>\n<color=#BBBBBB>▓▒██ ...maintain... ███▒▓</color>\n<color=#666666>██▓▒░████▒▓███░▒████▓▒</color>\n<color=#FFFFFF>▒██ ...a place to return to... ██▓</color>\n<color=#666666>▓▒██████░▓██▒████░▓▒███</color>\n<color=#BBBBBB>███ ...communications... ░▓██</color>\n<color=#666666>▒▓███░████▓▒██░▓████▒░</color>";

        // Mismo patrón que Spy y Scout: sustituir el lore visible por el
        // encuentro de USU, sin concatenar la historia anterior del personaje.
        // La misión de desbloqueo jugable de Rocketer no se altera.
        internal static string ResolveRocketerLore(bool es)
        {
            if (!IsRocketerEncounterUnlocked())
                return Corrupt(es);
            return LoaderRocketerEncounterUSU.Rocketer(es);
        }

        internal static string AppendLoaderIII(string original, bool es)
        {
            string source = original ?? "";
            if (source.IndexOf("Una cuestión de altura", StringComparison.OrdinalIgnoreCase) >= 0 ||
                source.IndexOf("A Matter of Height", StringComparison.OrdinalIgnoreCase) >= 0) return source;
            string heading = !IsLoaderIIIUnlocked() ? (es ? "Diario III — [DATOS DAÑADOS]" : "Log III — [CORRUPTED DATA]") : (es ? "Diario III — Una cuestión de altura (USU)" : "Log III — A Matter of Height (USU)");
            string body = IsLoaderIIIUnlocked() ? LoaderRocketerEncounterUSU.Loader(es) : Corrupt(es);
            return source.TrimEnd() + "\n\n<size=18><b>" + heading + "</b></size>\n" + body;
        }
    }
}
