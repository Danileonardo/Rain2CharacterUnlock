using System;
namespace UniversalSurvivorUnlocks {
 internal static class USUMercSoraEncounterGate {
  internal static bool CaptainReady() => VanillaBroadcastUSUDiary.IsUnlockedForLocalProfile();
  internal static bool SoraReady() => USUVanillaDiaryVictoryTracker.IsModDiaryIUnlockedForLocalProfile("SoraBody");
  private static string CaptainCorrupt(bool es) => es ? "[ARCHIVO CORRUPTO — TRANSMISIÓN INCOMPLETA]\n▓▓██▒░▓▓▓████▒░\n...a todas las unidades...\n...mantengan las comunicaciones...\n▓▒████░▓▓▓▓" : "[CORRUPTED FILE — INCOMPLETE TRANSMISSION]\n▓▓██▒░▓▓▓████▒░\n...to all units...\n...maintain communications...\n▓▒████░▓▓▓▓";
  private static string EncounterCorrupt(bool es) => es ? "[ARCHIVO CORRUPTO — REGISTROS SIN RECUPERAR]\n▓▓██▒░▓▓▓████▒░\n...una secuencia de símbolos...\n...la abertura...\n▓▒████░▓▓▓▓\n...dos siluetas..." : "[CORRUPTED FILE — RECORDS NOT RECOVERED]\n▓▓██▒░▓▓▓████▒░\n...a sequence of symbols...\n...the passage...\n▓▒████░▓▓▓▓\n...two silhouettes...";
  internal static string ResolveSora(bool es) { if (!CaptainReady()) return CaptainCorrupt(es); if (!SoraReady()) return EncounterCorrupt(es); return es ? USUMercSoraEncounterTexts.SoraEs : USUMercSoraEncounterTexts.SoraEn; }
  internal static string AppendMercIV(string original, bool es) {
   string value=(original ?? "").TrimEnd();
   if (value.IndexOf("Lo que permanece",StringComparison.OrdinalIgnoreCase)>=0 || value.IndexOf("What Remains",StringComparison.OrdinalIgnoreCase)>=0 || value.IndexOf("[MERC IV / USU]",StringComparison.OrdinalIgnoreCase)>=0) return value;
   string title= es ? "Diario IV — Lo que permanece (USU)" : "Log IV — What Remains (USU)";
   string content = !CaptainReady() ? CaptainCorrupt(es) : !SoraReady() ? EncounterCorrupt(es) : (es ? USUMercSoraEncounterTexts.MercEs : USUMercSoraEncounterTexts.MercEn);
   return value+"\n\n<size=18><b>"+title+"</b></size>\n"+content;
  }
 }
}
