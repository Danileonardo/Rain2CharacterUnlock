using System;
using BepInEx.Logging;
using RoR2;
using R2API;
using System.Collections.Generic;
using System.Reflection;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// USU bilingual introduction and original Logbook fallback for Kindred.
    /// Does not reference the external mod's DLL or modify its Original provider.
    /// Skill localization is maintained independently for this survivor.
    /// </summary>
    public sealed class KindredDefinition : SurvivorDefinition
    {
        public override string SourceIdentifier { get { return "com.Dragonyck.Kindred"; } }
        public override string BodyName { get { return "KindredBody"; } }
        public override string DefinitionName { get { return "Kindred"; } }

        private static readonly string SpanishDescription =
            "Kindred combina la precisión de Cordero con la persecución incansable de Lobo. Marca presas y aprovecha sus habilidades para sobrevivir a la cacería.<style=cSub>\n\n" +
            "< ! > La Marca de Kindred recompensa la eliminación de presas designadas.\n\n" +
            "< ! > Alterna Danza de flechas con la presión de Lobo sobre enemigos cercanos.\n\n" +
            "< ! > Tregua de Cordero permite retrasar la muerte en un área durante un breve período.";
        private static readonly string EnglishDescription =
            "Kindred embodies two inseparable aspects of death. Lamb offers a swift end to those who accept their fate, while Wolf hunts those who try to escape.";
        private static readonly string SpanishLore =
            "Transcripción parcial recuperada de una baliza de emergencia. Se desconoce quién realizó la grabación.\n" +
            "\n" +
            "—Corren muy deprisa —dijo una voz suave.\n" +
            "\n" +
            "—No lo bastante —contestó otra, grave y hambrienta.\n" +
            "\n" +
            "La señal apareció justo después de que un grupo de exploradores abandonara un campamento. No había rastros de pasos, aunque los sensores registraron dos siluetas superpuestas entre los árboles: una figura blanca que aguardaba inmóvil y otra oscura que no dejaba de moverse.\n" +
            "\n" +
            "—Aquí vuelven una y otra vez —dijo la primera voz—. Abren caminos, regresan de heridas imposibles y creen que cada victoria les concede un día más.\n" +
            "\n" +
            "—Entonces tendré mucho que perseguir.\n" +
            "\n" +
            "—A veces se detienen por los demás.\n" +
            "\n" +
            "—A veces los demás los dejan atrás.\n" +
            "\n" +
            "Se oyó tensarse una cuerda.\n" +
            "\n" +
            "La segunda voz soltó una carcajada, tan cerca del micrófono que hizo vibrar la grabación.\n" +
            "\n" +
            "—¿Cuál de ellos es nuestro?\n" +
            "\n" +
            "—Todos, algún día.\n" +
            "\n" +
            "La baliza dejó de transmitir antes de que comenzara el ataque. Cuando el equipo de rescate llegó, no encontró cuerpos ni señales de lucha: solamente una flecha clavada en la tierra y marcas de garras que desaparecían al borde del bosque.\n" +
            "\n" +
            "La grabación termina con un susurro imposible de atribuir a cualquiera de las dos voces: «Elige».";
        private static readonly string EnglishLore =
            "Partial transcript recovered from an emergency beacon. The recorder is unknown.\n" +
            "\n" +
            "“They run so quickly,” said a gentle voice.\n" +
            "\n" +
            "“Not quickly enough,” answered another, deep and hungry.\n" +
            "\n" +
            "The signal appeared shortly after a group of explorers abandoned a camp. There were no footprints, though sensors registered two overlapping shapes among the trees: a white figure standing still and a dark one that would not stop moving.\n" +
            "\n" +
            "“They return here again and again,” said the first voice. “They open paths, recover from impossible wounds, and believe every victory grants them one more day.”\n" +
            "\n" +
            "“Then I shall have plenty to chase.”\n" +
            "\n" +
            "“Sometimes they stop for one another.”\n" +
            "\n" +
            "“Sometimes they leave one another behind.”\n" +
            "\n" +
            "A bowstring tightened.\n" +
            "\n" +
            "The second voice laughed, so close to the microphone that the recording crackled.\n" +
            "\n" +
            "“Which one belongs to us?”\n" +
            "\n" +
            "“All of them, someday.”\n" +
            "\n" +
            "The beacon stopped transmitting before any attack began. When the rescue team arrived, they found no bodies and no signs of struggle: only an arrow buried in the earth and claw marks fading at the forest's edge.\n" +
            "\n" +
            "The recording ends with a whisper that cannot be attributed to either voice: “Choose.”";

        public override void RegisterInstalledLocalization(SurvivorInfo survivor, ManualLogSource logger)
        {
            if (survivor == null || !Matches(survivor)) return;
            RegisterNativeTextTokens(survivor, logger);
            KindredSkillLocalization.RegisterFor(logger);
            string passiveName, passiveDescription;
            if (TryGetPassiveLocalizationTokens(survivor, out passiveName, out passiveDescription))
            {
                KindredSkillLocalization.RegisterPassive(passiveName, passiveDescription, logger);
            }
            KindredSkillLocalization.RegisterPassiveKeywords(survivor, logger);
            KindredSkillLocalization.RegisterSkins(survivor, logger);
        }

        public override string ResolveSubtitle(SurvivorContentProfile profile, string fallback)
        {
            if (IsSpanish()) return "Los Cazadores Eternos";
            return IsMissing(fallback) ? "The Eternal Hunters" : fallback ?? "";
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
            loreTokens.Add("KINDRED_LORE");
            foreach (string token in loreTokens)
            {
                AddNative(token, SpanishLore, "es-419");
                AddNative(token, SpanishLore, "es-ES");
                AddNative(token, EnglishLore, "en");
            }
            logger?.LogInfo("[KINDRED NATIVE] Tokens de ficha: " +
                descriptionToken + " | " + subtitleToken +
                " | lore: " + string.Join(",", loreTokens));
        }

        private const string IsSpanishSubtitle = "Los Cazadores Eternos";

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
