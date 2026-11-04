using System;
using System.IO;
using System.Text;
using BepInEx;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    // Additive integration: never overrides existing vanilla Logbook/Diaries.
    internal static class USUTinkatonEncounterGate
    {
        // Call ONLY after confirmed escape/obliteration as Tinkaton, for the local profile.
        internal static void RecordTinkatonEnding(string profileId)
        {
            if (string.IsNullOrWhiteSpace(profileId)) return;
            string path = VictoryPath(profileId);
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            if (!File.Exists(path)) File.WriteAllText(path, "tinkaton-run-completed-v1", Encoding.UTF8);
        }

        internal static bool HasTinkatonEnding()
        {
            LocalUser local = LocalUserManager.GetFirstLocalUser();
            string id = local != null && local.userProfile != null ? local.userProfile.name : null;
            return USUVanillaDiaryVictoryTracker.IsModDiaryIUnlockedForLocalProfile("TinkatonBody") ||
                   (!string.IsNullOrWhiteSpace(id) && File.Exists(VictoryPath(id)));
        }

        internal static bool HasCaptainBroadcast()
        {
            return VanillaBroadcastUSUDiary.IsUnlockedForLocalProfile();
        }


        // Mismo texto de la ficha y del token: ningún lore anterior se filtra al panel.
        internal static string ResolveTinkatonLore(bool spanish)
        {
            if (!HasCaptainBroadcast()) return CaptainCorruption(spanish);
            if (!HasTinkatonEnding()) return spanish
                ? "Diario I — Bloqueado.\nCompleta una partida con este superviviente (Luna u Obelisco) para recuperar el registro."
                : "Diary I — Locked.\nComplete a run with this survivor (Moon or Obelisk) to recover the entry.";
            return TinkatonEngineerEncounterUSU.GetBody(spanish);
        }

        // UI entry for Tinkaton's personal Diary I.
        internal static string TinkatonDiaryI(string previous, bool spanish)
        {
            if (!HasCaptainBroadcast()) return Append(previous, spanish ? "Diario I — Señal incompleta" : "Diary I — Incomplete signal", CaptainCorruption(spanish));
            if (!HasTinkatonEnding()) return Append(previous, spanish ? "Diario I — Bloqueado" : "Diary I — Locked", spanish ? "Completa una partida con este superviviente para leer su historia." : "Complete a run with this survivor to read its story.");
            return TinkatonEngineerEncounterUSU.Append(previous, spanish);
        }

        // UI entry for Engineer Diary IV; does not require an Engineer victory.
        internal static string EngineerDiaryIV(string previous, bool spanish)
        {
            if (!HasCaptainBroadcast()) return Append(previous, spanish ? "Diario IV — Señal incompleta" : "Diary IV — Incomplete signal", CaptainCorruption(spanish));
            if (!HasTinkatonEnding()) return Append(previous, spanish ? "Diario IV — Registro corrupto" : "Diary IV — Corrupted record", spanish ?
                "▓▒████▒░▓████▓▒███\n...un pequeño martillo...\n█▓██▒▓████░▒▓███" :
                "▓▒████▒░▓████▓▒███\n...a little hammer...\n█▓██▒▓████░▒▓███");
            return EngineerTinkatinkEncounterUSU.Append(previous, spanish);
        }

        private static string CaptainCorruption(bool es)
        {
            return es ? "▓▒████▒██▓░████▒▓██\n...posición establecida...\n██▓▒███▓████▒░██\n...un lugar al que regresar...\n▒▓███░████▒▓██" :
                        "▓▒████▒██▓░████▒▓██\n...position established...\n██▓▒███▓████▒░██\n...somewhere to return...\n▒▓███░████▒▓██";
        }

        private static string Append(string previous, string heading, string body)
        {
            return (previous ?? string.Empty) + "\n\n<size=18><b>" + heading + " (USU)</b></size>\n" + body;
        }

        private static string VictoryPath(string id)
        {
            string safe = Convert.ToBase64String(Encoding.UTF8.GetBytes(id)).Replace('/', '_').Replace('+', '-').TrimEnd('=');
            return System.IO.Path.Combine(Paths.ConfigPath, "UniversalSurvivorUnlocks", "ModDiaryI", safe, "Tinkaton.flag");
        }
    }
}
