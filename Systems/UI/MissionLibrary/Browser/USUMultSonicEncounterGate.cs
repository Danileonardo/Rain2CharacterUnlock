using System;
namespace UniversalSurvivorUnlocks
{
    internal static class USUMultSonicEncounterGate
    {
        internal static bool CaptainReady() => VanillaBroadcastUSUDiary.IsUnlockedForLocalProfile();
        internal static bool SonicReady() => USUVanillaDiaryVictoryTracker.IsModDiaryIUnlockedForLocalProfile("SonicBody");
        private static string CaptainCorrupt(bool es) => es ?
            "[ARCHIVO CORRUPTO — INTERFERENCIA DE TRANSMISIÓN]\n▓▓██▒░▓▓▓████▒░\n...a todas las unidades...\n...mantengan las comunicaciones...\n▓▒████░▓▓▓▓" :
            "[CORRUPTED FILE — TRANSMISSION INTERFERENCE]\n▓▓██▒░▓▓▓████▒░\n...to all units...\n...maintain communications...\n▓▒████░▓▓▓▓";
        private static string EncounterCorrupt(bool es) => es ?
            "[ARCHIVO CORRUPTO — SECUENCIA NO RECUPERADA]\n▓▒███▒▓████░▓▓\n...un desplazamiento veloz...\n...un mecanismo rodante...\n▓▓▓███▒░██▓▓\n...una señal... dos caminos...\n▓████▒▓▓▒░" :
            "[CORRUPTED FILE — SEQUENCE UNRECOVERED]\n▓▒███▒▓████░▓▓\n...moving fast...\n...wheels in motion...\n▓▓▓███▒░██▓▓\n...one signal... two paths...\n▓████▒▓▓▒░";
        internal static string ResolveSonic(bool es)
        {
            if (!CaptainReady()) return CaptainCorrupt(es);
            if (!SonicReady()) return EncounterCorrupt(es);
            return es ? USUMultSonicEncounterTexts.SonicEs : USUMultSonicEncounterTexts.SonicEn;
        }
        internal static string AppendMultIV(string original, bool es)
        {
            string value = (original ?? "").TrimEnd();
            if (value.IndexOf("Un gesto aprendido", StringComparison.OrdinalIgnoreCase) >= 0 ||
                value.IndexOf("A Learned Gesture", StringComparison.OrdinalIgnoreCase) >= 0 ||
                value.IndexOf("[MUL-T IV / USU]", StringComparison.OrdinalIgnoreCase) >= 0) return value;
            string title = es ? "Diario IV — Un gesto aprendido (USU)" : "Log IV — A Learned Gesture (USU)";
            string body = !CaptainReady() ? CaptainCorrupt(es) : !SonicReady() ? EncounterCorrupt(es) :
                (es ? USUMultSonicEncounterTexts.MultEs : USUMultSonicEncounterTexts.MultEn);
            return value + "\n\n<size=18><b>" + title + "</b></size>\n" + body;
        }
    }
}
