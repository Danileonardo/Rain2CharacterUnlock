using System;
using RoR2;
namespace UniversalSurvivorUnlocks
{
    internal static class USUScoutHuntressEncounterGate
    {
        internal static bool HasScoutVictory() => USUVanillaDiaryVictoryTracker.IsModDiaryIUnlockedForLocalProfile("ScoutBody");
        internal static bool HasCaptainBroadcast() => VanillaBroadcastUSUDiary.IsUnlockedForLocalProfile();
        private static string CaptainHint(bool es) => es
            ? "▓▒████▒██▓░████▒▓██\n...posición establecida...\n██▓▒███▓████▒░██\n...un lugar al que regresar..."
            : "▓▒████▒██▓░████▒▓██\n...position established...\n██▓▒███▓████▒░██\n...somewhere to return...";
        internal static string ResolveScoutLore(bool es)
        {
            if (!HasCaptainBroadcast()) return CaptainHint(es);
            if (!HasScoutVictory()) return es
                ? "Diario I — Bloqueado.\nCompleta una partida con Scout escapando de la Luna u obliterándote para recuperar su historia."
                : "Diary I — Locked.\nComplete a run with Scout by escaping the Moon or obliterating to recover his story.";
            return ScoutHuntressEncounterUSU.GetScout(es);
        }
        internal static string AppendHuntressIV(string previous, bool es)
        {
            string text = previous ?? "";
            if (text.IndexOf("La presa que no sabía detenerse", StringComparison.OrdinalIgnoreCase) >= 0 ||
                text.IndexOf("The Prey That Wouldn't Stop", StringComparison.OrdinalIgnoreCase) >= 0 ||
                text.IndexOf("Diario IV —", StringComparison.OrdinalIgnoreCase) >= 0 ||
                text.IndexOf("Diary IV —", StringComparison.OrdinalIgnoreCase) >= 0) return text;
            string heading = es ? "Diario IV — La presa que no sabía detenerse (USU)"
                                : "Diary IV — The Prey That Wouldn't Stop (USU)";
            string body;
            if (!HasCaptainBroadcast()) body = CaptainHint(es);
            else if (!HasScoutVictory()) body = es
                ? "Registro bloqueado. No todas las presas huyen por miedo. Algunas corren tan deprisa que olvidan mirar hacia dónde van."
                : "Locked record. Not everything that runs is fleeing in fear. Some move so fast they forget where they are going.";
            else body = ScoutHuntressEncounterUSU.GetHuntress(es);
            return text.TrimEnd() + "\n\n<size=18><b>" + heading + "</b></size>\n" + body;
        }
    }
}
