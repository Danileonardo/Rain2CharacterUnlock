using System;
using System.Reflection;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace UniversalSurvivorUnlocks
{
    internal static class USUCommandoBayonettaEncounterGate
    {
        internal static bool CaptainReady() => VanillaBroadcastUSUDiary.IsUnlockedForLocalProfile();
        internal static bool BayonettaReady() => USUVanillaDiaryVictoryTracker.IsModDiaryIUnlockedForLocalProfile("BayoBody");

        // Check the actual skill unlock from the current local profile; no extra USU flag.
        // This is deliberately evaluated every time, including unlocks earned before installation.
        internal static bool HasTacticalSlide()
        {
            try
            {
                var local = LocalUserManager.GetFirstLocalUser();
                if (local == null || local.userProfile == null) return false;
                GameObject body = BodyCatalog.FindBodyPrefab("CommandoBody");
                if (body == null) return false;
                SkillLocator locator = body.GetComponent<SkillLocator>();
                if (locator == null || locator.utility == null || locator.utility.skillFamily == null) return false;
                foreach (var variant in locator.utility.skillFamily.variants)
                {
                    if (variant.skillDef == null) continue;
                    string skillName = variant.skillDef.skillName ?? "";
                    string skillToken = variant.skillDef.skillNameToken ?? "";
                    if (skillName.IndexOf("slide", StringComparison.OrdinalIgnoreCase) < 0 &&
                        skillToken.IndexOf("slide", StringComparison.OrdinalIgnoreCase) < 0) continue;
                    if (variant.unlockableDef == null) return false;
                    return local.userProfile.HasUnlockable(variant.unlockableDef);
                }
            }
            catch (Exception) { /* Fail closed, never expose a locked passage. */ }
            return false;
        }

        private static string MissingCaptain(bool es) => es
            ? "<color=#888888>[ARCHIVO CORRUPTO — SEÑAL INTERFERIDA]</color>\n<color=#666666>▓▒████░▓██▒████▓▒░███▓▒░</color>\n...a todas las unidades...\n▓▓▓▓▓▓▓▓▓▓▓▓▓▓\n...mantengan...\n▓██▒████▓▒██\n...un lugar al que regresar...\n▓▓▓▓▓▓▓▓▓▓"
            : "<color=#888888>[CORRUPTED FILE — SIGNAL INTERFERENCE]</color>\n<color=#666666>▓▒████░▓██▒████▓▒░███▓▒░</color>\n...to all units...\n▓▓▓▓▓▓▓▓▓▓▓▓▓▓\n...maintain...\n▓██▒████▓▒██\n...a place to return to...\n▓▓▓▓▓▓▓▓▓▓";

        private static string MissingBayonetta(bool es) => es
            ? "<color=#888888>[ARCHIVO CORRUPTO — REGISTRO NO RECUPERADO]</color>\n▓▒███▓████▒░▓██\n...dos pistolas...\n████▒▓███░▒████\n...una desconocida...\n▓▓▓▓▓▓▓▓▓▓▓▓▓▓"
            : "<color=#888888>[CORRUPTED FILE — RECORD NOT RECOVERED]</color>\n▓▒███▓████▒░▓██\n...two pistols...\n████▒▓███░▒████\n...a stranger...\n▓▓▓▓▓▓▓▓▓▓▓▓▓▓";

        internal static string ResolveBayonetta(bool es)
        {
            if (!CaptainReady()) return MissingCaptain(es);
            if (!BayonettaReady()) return MissingBayonetta(es);
            if (es) return HasTacticalSlide() ? USUCommandoBayonettaTexts.BayonettaCompleteEs : USUCommandoBayonettaTexts.BayonettaCorruptEs;
            return HasTacticalSlide() ? USUCommandoBayonettaTexts.BayonettaCompleteEn : USUCommandoBayonettaTexts.BayonettaCorruptEn;
        }

        internal static string AppendCommandoIV(string original, bool es)
        {
            string text = (original ?? "").TrimEnd();
            if (text.IndexOf("Signals Among the Ruins", StringComparison.OrdinalIgnoreCase) >= 0 ||
                text.IndexOf("Señales entre las ruinas", StringComparison.OrdinalIgnoreCase) >= 0) return text;
            string title = es ? "Diario IV — Señales entre las ruinas (USU)" : "Log IV — Signals Among the Ruins (USU)";
            string body = !CaptainReady() ? MissingCaptain(es) : !BayonettaReady() ? MissingBayonetta(es) :
                (es ? (HasTacticalSlide() ? USUCommandoBayonettaTexts.CommandoCompleteEs : USUCommandoBayonettaTexts.CommandoCorruptEs)
                    : (HasTacticalSlide() ? USUCommandoBayonettaTexts.CommandoCompleteEn : USUCommandoBayonettaTexts.CommandoCorruptEn));
            return text + "\n\n<size=18><b>" + title + "</b></size>\n" + body;
        }
    }
}
