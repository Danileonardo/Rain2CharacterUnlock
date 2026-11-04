using System;
using System.Reflection;
using RoR2;
using UnityEngine;

namespace UniversalSurvivorUnlocks
{
    internal static class USUVanillaNativeLogbookBridge
    {
        private static bool registered;
        internal static void Initialize()
        {
            if (registered) return;
            registered = true;
            On.RoR2.Language.GetLocalizedStringByToken += OnLookup;
        }

        private static string OnLookup(On.RoR2.Language.orig_GetLocalizedStringByToken orig,
            Language language, string token)
        {
            string original = orig(language, token);
            if (string.IsNullOrEmpty(token)) return original;
            // Tinkaton's definition still registers a placeholder for the native
            // Logbook token. Supply the same gated diary used by the USU browser.
            if (string.Equals(token, "TINKATON_LORE", StringComparison.OrdinalIgnoreCase))
            {
                bool spanish = (Language.currentLanguageName ?? "").StartsWith("es", StringComparison.OrdinalIgnoreCase);
                return USUTinkatonEncounterGate.ResolveTinkatonLore(spanish);
            }
            if (string.Equals(token, "KENKO_SPY_LORE", StringComparison.OrdinalIgnoreCase))
            {
                bool spanish = (Language.currentLanguageName ?? "").StartsWith("es", StringComparison.OrdinalIgnoreCase);
                return USUSpyBanditEncounterGate.ResolveSpyLore(spanish);
            }
            if (string.Equals(token, "KENKO_SCOUT_LORE", StringComparison.OrdinalIgnoreCase))
            {
                bool spanish = (Language.currentLanguageName ?? "").StartsWith("es", StringComparison.OrdinalIgnoreCase);
                return USUScoutHuntressEncounterGate.ResolveScoutLore(spanish);
            }
            // Resolve Rocketer's encounter live. Static localization overlays are not
            // refreshed after a Captain victory, so do not cache the gated text.
            if (string.Equals(token, "MOFFEIN_ROCKET_BODY_LORE", StringComparison.OrdinalIgnoreCase))
            {
                bool spanish = (Language.currentLanguageName ?? "").StartsWith("es", StringComparison.OrdinalIgnoreCase);
                return USULoaderRocketerEncounterGate.ResolveRocketerLore(spanish);
            }
            // Bayonetta's active lore is resolved live, not cached in a static overlay.
            if (string.Equals(token, "PSYCHE_BAYO_LORE", StringComparison.OrdinalIgnoreCase))
            {
                bool spanish = (Language.currentLanguageName ?? "").StartsWith("es", StringComparison.OrdinalIgnoreCase);
                return USUCommandoBayonettaEncounterGate.ResolveBayonetta(spanish);
            }
            string bodyName = FindVanillaBody(token);
            return bodyName == null ? original : USUAdditionalDiaries.AppendToNativeLore(bodyName, original);
        }

        private static string FindVanillaBody(string token)
        {
            var defs = SurvivorCatalog.survivorDefs;
            if (defs == null) return null;
            foreach (var def in defs)
            {
                if (def == null || def.bodyPrefab == null) continue;
                string bodyName = def.bodyPrefab.name;
                switch (bodyName)
                {
                    case "CommandoBody": case "HuntressBody": case "Bandit2Body":
                    case "ToolbotBody": case "EngiBody": case "MageBody":
                    case "MercBody": case "TreebotBody": case "LoaderBody":
                    case "CrocoBody": case "CaptainBody": break;
                    default: continue;
                }
                // SurvivorDef does NOT expose loreToken in some RoR2 builds.
                // Read potential members via reflection rather than direct C# access.
                string lore = GetStringMember(def, "loreToken");
                if (Matches(token, lore)) return bodyName;
                var body = def.bodyPrefab.GetComponent<CharacterBody>();
                lore = GetStringMember(body, "loreToken");
                if (Matches(token, lore)) return bodyName;
                string nameToken = GetStringMember(def, "displayNameToken");
                if (string.IsNullOrEmpty(nameToken)) nameToken = GetStringMember(body, "baseNameToken");
                if (MatchesDerivedLore(token, nameToken)) return bodyName;
            }
            return null;
        }

        private static bool Matches(string a, string b)
        {
            return !string.IsNullOrEmpty(b) && string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }

        private static bool MatchesDerivedLore(string requested, string nameToken)
        {
            if (string.IsNullOrEmpty(nameToken)) return false;
            if (nameToken.EndsWith("_NAME", StringComparison.OrdinalIgnoreCase))
                return Matches(requested, nameToken.Substring(0, nameToken.Length - 5) + "_LORE");
            if (nameToken.EndsWith("_DISPLAYNAME", StringComparison.OrdinalIgnoreCase))
                return Matches(requested, nameToken.Substring(0, nameToken.Length - 12) + "_LORE");
            return false;
        }

        private static string GetStringMember(object target, string member)
        {
            if (target == null) return null;
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            try
            {
                var field = target.GetType().GetField(member, flags);
                if (field != null) return field.GetValue(target) as string;
                var prop = target.GetType().GetProperty(member, flags);
                if (prop != null && prop.GetIndexParameters().Length == 0)
                    return prop.GetValue(target, null) as string;
            }
            catch { }
            return null;
        }
    }
}
