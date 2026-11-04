using System;
using BepInEx.Logging;
using RoR2;
using R2API;
using System.Collections.Generic;
using System.Reflection;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// USU bilingual introduction and original Logbook fallback for Gwen.
    /// Does not reference the external mod's DLL or modify its Original provider.
    /// Skill localization is maintained independently for this survivor.
    /// </summary>
    public sealed class GwenDefinition : SurvivorDefinition
    {
        public override string SourceIdentifier { get { return "com.Dragonyck.Gwen"; } }
        public override string BodyName { get { return "GwenBody"; } }
        public override string DefinitionName { get { return "Gwen"; } }

        private static readonly string SpanishDescription = GwenLocalization.SpanishDescription;
        private static readonly string EnglishDescription = GwenLocalization.EnglishDescription;
        // Curated six-paragraph USU Logbook account: Gwen’s handwritten memory and sewing journal.
        private static readonly string SpanishLore = LoreDialogueLineFormatter.Format(GwenLore.SpanishLore);
        private static readonly string EnglishLore = LoreDialogueLineFormatter.Format(GwenLore.EnglishLore);

        public override void RegisterInstalledLocalization(SurvivorInfo survivor, ManualLogSource logger)
        {
            if (survivor == null || !Matches(survivor)) return;
            RegisterNativeTextTokens(survivor, logger);
            GwenSkillLocalization.RegisterFor(logger);
            string passiveName, passiveDescription;
            if (TryGetPassiveLocalizationTokens(survivor, out passiveName, out passiveDescription))
            {
                GwenSkillLocalization.RegisterPassive(passiveName, passiveDescription, logger);
            }
            GwenSkillLocalization.RegisterSkins(survivor, logger);
        }

        public override string ResolveSubtitle(SurvivorContentProfile profile, string fallback)
        {
            if (IsSpanish()) return "La Costurera Sagrada";
            return IsMissing(fallback) ? "The Hallowed Seamstress" : fallback ?? "";
        }

        public override string ResolveDescription(SurvivorContentProfile profile, string fallback)
        {
            // Preserve authored data. Only fill missing native content.
            if (IsSpanish()) return SpanishDescription;
            return IsMissing(fallback) || IsBrokenDescription(fallback) ? EnglishDescription : fallback;
        }

        public override string ResolveLore(SurvivorContentProfile profile, string fallback)
        {
            // Original creator lore always takes priority when valid.
            // Este mod deja en el Logbook la entrada genérica de Datascraper.
            // La entrada USU se registra también en el token nativo.
            if (IsSpanish()) return SpanishLore;
            return IsMissing(fallback) || IsDatascraper(fallback) ? EnglishLore : fallback;
        }

        private static readonly List<object> NativeOverlays = new List<object>();
        private static readonly HashSet<string> RegisteredOverlayKeys =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private static void RegisterNativeTextTokens(SurvivorInfo survivor, ManualLogSource logger)
        {
            if (survivor?.SurvivorDef == null) return;
            var def = survivor.SurvivorDef;
            string descriptionToken = ReadTextMember(def, "descriptionToken");
            string displayToken = ReadTextMember(def, "displayNameToken");
            var body = def.bodyPrefab != null ? def.bodyPrefab.GetComponent<CharacterBody>() : null;
            string subtitleToken = ReadTextMember(body, "subtitleNameToken", "subtitleToken");
            string bodyNameToken = ReadTextMember(body, "baseNameToken");

            // Las vistas generales nativas no pasan por ResolveDescription;
            // hay que localizar directamente SurvivorDef.descriptionToken.
            AddSpanish(descriptionToken, SpanishDescription);
            AddSpanish(subtitleToken, IsSpanishSubtitle);

            var loreTokens = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            AddLoreToken(loreTokens, displayToken);
            AddLoreToken(loreTokens, bodyNameToken);
            loreTokens.Add("GWENGWEN_LORE");
            foreach (string token in loreTokens)
            {
                AddNative(token, SpanishLore, "es-419");
                AddNative(token, SpanishLore, "es-ES");
                AddNative(token, EnglishLore, "en");
            }
            UsuLog.Verbose(logger, "[GWEN NATIVE] Tokens de ficha: " +
                descriptionToken + " | " + subtitleToken +
                " | lore: " + string.Join(",", loreTokens));
        }

        private const string IsSpanishSubtitle = "La Costurera Sagrada";

        private static void AddLoreToken(HashSet<string> lore, string nameToken)
        {
            if (string.IsNullOrWhiteSpace(nameToken)) return;
            if (nameToken.EndsWith("_NAME", StringComparison.OrdinalIgnoreCase))
                lore.Add(nameToken.Substring(0, nameToken.Length - 5) + "_LORE");
            if (nameToken.EndsWith("_DISPLAYNAME", StringComparison.OrdinalIgnoreCase))
                lore.Add(nameToken.Substring(0, nameToken.Length - 12) + "_LORE");
        }

        private static string ReadTextMember(object target, params string[] names)
        {
            if (target == null) return "";
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (string name in names)
            {
                try
                {
                    var field = target.GetType().GetField(name, flags);
                    if (field != null && field.GetValue(target) is string value) return value;
                    var prop = target.GetType().GetProperty(name, flags);
                    if (prop != null && prop.GetIndexParameters().Length == 0 &&
                        prop.GetValue(target, null) is string propertyValue) return propertyValue;
                }
                catch { }
            }
            return "";
        }

        private static void AddSpanish(string token, string text)
        {
            AddNative(token, text, "es-419");
            AddNative(token, text, "es-ES");
        }

        private static void AddNative(string token, string value, string language)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(value)) return;
            if (!RegisteredOverlayKeys.Add(language + ":" + token)) return;
            // AddOverlay es el mismo mecanismo validado en Auriel: permite
            // corregir textos originales malformados que LanguageAPI.Add no sustituye.
            NativeOverlays.Add(LanguageAPI.AddOverlay(token, value, language));
        }

        private static bool IsDatascraper(string text)
        {
            return !string.IsNullOrWhiteSpace(text) &&
                (text.IndexOf("Datascraper", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 text.IndexOf("No parece haber datos", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static bool IsBrokenDescription(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return true;
            string withoutMarkers = text.Replace("< ! >", "").Replace("<style=cSub>", "").Trim();
            return withoutMarkers.Length < 20;
        }

        private static bool IsSpanish()
        {
            return (Language.currentLanguageName ?? "").StartsWith("es", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsMissing(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return true;
            string text = value.Trim();
            // Missing localization tokens may display as ALL_CAPS_IDENTIFIER.
            return text.IndexOf(' ') < 0 && text.IndexOf('_') >= 0 &&
                   string.Equals(text, text.ToUpperInvariant(), StringComparison.Ordinal);
        }
    }
}
