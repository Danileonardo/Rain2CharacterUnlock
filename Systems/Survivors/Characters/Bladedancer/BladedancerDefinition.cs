using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Logging;
using R2API;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Mantiene el cuerpo interno IreliaBody de BladedancerMod.
    /// Localiza el registro de plnk en espanol mediante overlay de idioma y
    /// anade la continuacion curada USU sin modificar la DLL original.
    /// </summary>
    public sealed class BladedancerDefinition : SurvivorDefinition
    {
        public override string SourceIdentifier => "com.slendykray.BladedancerMod";
        public override string BodyName => "IreliaBody"; // Identificador interno del creador: NO renombrar.
        public override string DefinitionName => "Bladedancer";

        private static SurvivorInfo installed;
        private static bool subscribed;
        private static readonly List<string> NativeLoreTokens = new List<string>();
        private static readonly Dictionary<string, string> OriginalLoreByLanguageAndToken =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, object> NativeOverlays =
            new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public override void RegisterInstalledLocalization(SurvivorInfo survivor, ManualLogSource logger)
        {
            if (survivor == null || !Matches(survivor)) return;
            installed = survivor;
            // Habilidades, pasivas y skins: solo sobre IreliaBody.
            BladedancerSkillLocalization.RegisterFor(survivor, logger);
            FindNativeLoreTokens(survivor);
            if (!subscribed)
            {
                subscribed = true;
                Language.onCurrentLanguageChanged += OnLanguageChanged;
            }
            AddCurrentLanguageLoreOverlay();
            UsuLog.Verbose(logger,
                "[BLADEDANCER LORE] Traduccion ES del original + continuacion USU EN/ES; " +
                "tokens nativos detectados: " + NativeLoreTokens.Count);
        }

        public override string ResolveDescription(SurvivorContentProfile profile, string fallback)
        {
            // La ficha de USU tiene su propio resolvedor: no depende del token
            // del Overview que utiliza el selector nativo de RoR2.
            return IsSpanish() ? BladedancerSkillLocalization.SpanishOverview : fallback ?? "";
        }

        public override string ResolveSubtitle(SurvivorContentProfile profile, string fallback)
        {
            return IsSpanish() ? "La Danzarina de las Hojas" : fallback ?? "";
        }

        public override string ResolveLore(SurvivorContentProfile profile, string fallback)
        {
            // Este metodo alimenta Notas en USU. El Logbook nativo usa los tokens
            // cuyo overlay se registra en RegisterInstalledLocalization().
            return JoinOriginalWithAddition(fallback ?? "", IsSpanish());
        }

        private static bool IsSpanish()
        {
            return (Language.currentLanguageName ?? "").StartsWith("es", StringComparison.OrdinalIgnoreCase);
        }

        private static void OnLanguageChanged()
        {
            if (installed != null) AddCurrentLanguageLoreOverlay();
        }

        private static string JoinOriginalWithAddition(string original, bool spanish)
        {
            string suffix = spanish ? BladedancerLore.SpanishAddition : BladedancerLore.EnglishAddition;
            string baseLore = original ?? "";

            // El texto nativo se traduce solo si coincide con la version
            // inspeccionada de IreliaMod.dll. Si cambia, se conserva intacto.
            // Esto se aplica tanto a Notas USU como al token del Logbook nativo.
            if (spanish && BladedancerNativeLoreEs.MatchesKnownOriginal(baseLore))
                baseLore = BladedancerNativeLoreEs.Text;

            if (baseLore.IndexOf(BladedancerLore.Marker, StringComparison.Ordinal) >= 0)
                return baseLore;
            if (string.IsNullOrWhiteSpace(baseLore) || LooksLikeMissing(baseLore, ""))
                return suffix.TrimStart();
            return baseLore.TrimEnd() + suffix;
        }

        private static bool LooksLikeMissing(string value, string token)
        {
            if (string.IsNullOrWhiteSpace(value)) return true;
            string s = value.Trim();
            if (s == "???" || s == "??") return true;
            if (!string.IsNullOrEmpty(token) && string.Equals(s, token, StringComparison.OrdinalIgnoreCase))
                return true;
            // Tokens inexistentes suelen devolverse literalmente como identificador.
            return s.Length < 160 && s.IndexOf(' ') < 0 && s.IndexOf('_') >= 0 &&
                   string.Equals(s, s.ToUpperInvariant(), StringComparison.Ordinal);
        }

        private static void AddCurrentLanguageLoreOverlay()
        {
            string language = Language.currentLanguageName ?? "";
            if (string.IsNullOrWhiteSpace(language)) return;
            bool spanish = language.StartsWith("es", StringComparison.OrdinalIgnoreCase);
            bool english = language.StartsWith("en", StringComparison.OrdinalIgnoreCase);
            if (!spanish && !english) return;

            foreach (string token in NativeLoreTokens)
            {
                string key = language + ":" + token;
                if (NativeOverlays.ContainsKey(key)) continue;
                string original;
                if (!OriginalLoreByLanguageAndToken.TryGetValue(key, out original))
                {
                    original = Language.GetString(token) ?? "";
                    if (LooksLikeMissing(original, token)) continue; // Nunca inventar el texto del creador.
                    OriginalLoreByLanguageAndToken[key] = original;
                }
                string combined = JoinOriginalWithAddition(original, spanish);
                NativeOverlays.Add(key, LanguageAPI.AddOverlay(token, combined, language));
            }
        }

        private static void FindNativeLoreTokens(SurvivorInfo survivor)
        {
            if (NativeLoreTokens.Count > 0) return;
            object survivorDef = survivor?.SurvivorDef;
            if (survivorDef == null) return;
            AddTokenCandidate(ReadString(survivorDef, "loreToken"));
            AddDerivedTokenCandidate(ReadString(survivorDef, "displayNameToken"));
            if (survivor.SurvivorDef.bodyPrefab != null)
            {
                CharacterBody body = survivor.SurvivorDef.bodyPrefab.GetComponent<CharacterBody>();
                AddDerivedTokenCandidate(ReadString(body, "baseNameToken"));
            }
            // Respeto a claves conocidas por convencion: solo se agregan si existen.
            // Token nativo confirmado mediante el prefijo de localizacion de IreliaMod.dll.
            AddTokenCandidate("SLENDYKRAY_IRELIA_LORE");
            AddTokenCandidate("IRELIA_LORE");
            AddTokenCandidate("IRELIA_BODY_LORE");
        }

        private static void AddDerivedTokenCandidate(string nameToken)
        {
            if (string.IsNullOrWhiteSpace(nameToken)) return;
            const string nameSuffix = "_NAME";
            const string displaySuffix = "_DISPLAYNAME";
            if (nameToken.EndsWith(nameSuffix, StringComparison.OrdinalIgnoreCase))
                AddTokenCandidate(nameToken.Substring(0, nameToken.Length - nameSuffix.Length) + "_LORE");
            if (nameToken.EndsWith(displaySuffix, StringComparison.OrdinalIgnoreCase))
                AddTokenCandidate(nameToken.Substring(0, nameToken.Length - displaySuffix.Length) + "_LORE");
        }

        private static void AddTokenCandidate(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return;
            foreach (string existing in NativeLoreTokens)
                if (string.Equals(existing, token, StringComparison.OrdinalIgnoreCase)) return;
            NativeLoreTokens.Add(token);
        }

        private static string ReadString(object target, string member)
        {
            if (target == null) return "";
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            try
            {
                FieldInfo field = target.GetType().GetField(member, flags);
                if (field != null && field.FieldType == typeof(string))
                    return field.GetValue(target) as string ?? "";
                PropertyInfo prop = target.GetType().GetProperty(member, flags);
                if (prop != null && prop.PropertyType == typeof(string) && prop.GetIndexParameters().Length == 0)
                    return prop.GetValue(target, null) as string ?? "";
            }
            catch { }
            return "";
        }
    }
}
