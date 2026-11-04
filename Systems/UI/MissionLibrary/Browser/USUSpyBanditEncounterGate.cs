using System;
using RoR2;
namespace UniversalSurvivorUnlocks
{
    internal static class USUSpyBanditEncounterGate
    {
        internal static bool HasSpyVictory() => USUVanillaDiaryVictoryTracker.IsModDiaryIUnlockedForLocalProfile("SpyBody");
        internal static bool HasCaptainBroadcast() => VanillaBroadcastUSUDiary.IsUnlockedForLocalProfile();
        private static bool IsSpanish() => (Language.currentLanguageName ?? "").StartsWith("es", StringComparison.OrdinalIgnoreCase);
        private static string CaptainHint(bool es) => es
            ? "▓▒████▒██▓░████▒▓██\n...posición establecida...\n██▓▒███▓████▒░██\n...un lugar al que regresar..."
            : "▓▒████▒██▓░████▒▓██\n...position established...\n██▓▒███▓████▒░██\n...somewhere to return...";
        internal static string ResolveSpyLore(bool es)
        {
            if (!HasCaptainBroadcast()) return CaptainHint(es);
            if (!HasSpyVictory()) return es
                ? "Diario I — Bloqueado.\nCompleta una partida con Spy escapando de la Luna u obliterándote para recuperar su historia."
                : "Diary I — Locked.\nComplete a run with Spy by escaping the Moon or obliterating to recover his story.";
            return SpyBanditEncounterUSU.GetSpy(es);
        }
        internal static string AppendBanditIV(string previous, bool es)
        {
            string text = previous ?? "";
            if (text.IndexOf("Un caballero de dudosa confianza", StringComparison.OrdinalIgnoreCase) >= 0 ||
                text.IndexOf("A Gentleman of Questionable Trust", StringComparison.OrdinalIgnoreCase) >= 0 ||
                text.IndexOf("Diario IV —", StringComparison.OrdinalIgnoreCase) >= 0 ||
                text.IndexOf("Diary IV —", StringComparison.OrdinalIgnoreCase) >= 0) return text;
            string heading = es ? "Diario IV — Un caballero de dudosa confianza (USU)" : "Diary IV — A Gentleman of Questionable Trust (USU)";
            string body;
            if (!HasCaptainBroadcast()) body = CaptainHint(es);
            else if (!HasSpyVictory()) body = es
                ? "Registro bloqueado. Dos sombras conocen el valor de una espalda descubierta. Una daga puede convertir a un desconocido en un aliado inesperado."
                : "Locked record. Two shadows know the value of an exposed back. A dagger may turn a stranger into an unexpected ally.";
            else body = SpyBanditEncounterUSU.GetBandit(es);
            return text.TrimEnd() + "\n\n<size=18><b>" + heading + "</b></size>\n" + body;
        }
    }
}
