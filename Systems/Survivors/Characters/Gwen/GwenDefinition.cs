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

        private static readonly string SpanishDescription =
            "Gwen combate con grandes tijeras y agujas encantadas. Alterna cortes rápidos con movimientos evasivos para enfrentar a enemigos poderosos.<style=cSub>\n\n" +
            "< ! > Cortes sagrados remata cada tercer golpe con una estocada y acumula Snippy.\n\n" +
            "< ! > Aprovecha ¡Corta, corta! para golpear al objetivo con el centro de las tijeras.\n\n" +
            "< ! > Usa Salto y corte para reposicionarte y Labores de aguja para atacar a distancia.";
        private static readonly string EnglishDescription =
            "Gwen is a doll brought to life by the Hallowed Mist. Armed with oversized scissors, needles, and enchanted thread, she combines agile attacks with protective magic.";
        private static readonly string SpanishLore =
            "Registro recuperado. Identificación del emisor: desconocida.\n" +
            "\n" +
            "Al principio creyeron que se trataba de una herramienta. Dos hojas desmesuradas, afiladas y brillantes, abandonadas en un claro donde los árboles habían sido cortados de manera imposible.\n" +
            "\n" +
            "Después encontraron el hilo.\n" +
            "\n" +
            "Recorría las ramas, enredaba las piedras y se perdía entre restos de criaturas que nadie había logrado identificar. A pesar de todo, no había sangre en la tela. Solo pequeñas puntadas, cuidadosas, como si alguien hubiese intentado reparar algo que el planeta ya había dado por perdido.\n" +
            "\n" +
            "Una exploradora habló de una muchacha que avanzaba entre los monstruos sin dejar de mirar el paisaje. Portaba unas tijeras gigantescas. Se detuvo para levantar del suelo un trozo de tela desgarrada y preguntó si alguien lo echaría de menos.\n" +
            "\n" +
            "Nadie supo qué responder.\n" +
            "\n" +
            "La muchacha sonrió. Dijo que había sido hecha con cariño y que, desde que podía caminar por sí misma, había aprendido una cosa: todo lo que vive merece al menos una oportunidad.\n" +
            "\n" +
            "Cuando la exploradora volvió a mirar, los monstruos y la muchacha habían desaparecido. Sobre una roca quedaba la tela, ahora remendada.\n" +
            "\n" +
            "Nota añadida al registro: nadie ha podido explicar quién enseñó a coser a aquella muñeca. Ni por qué unas tijeras destinadas a cortar podían dejar algo entero.";
        private static readonly string EnglishLore =
            "Recovered record. Sender identification: unknown.\n" +
            "\n" +
            "At first, they believed it was a tool. Two immense, shining blades, sharp beyond reason, abandoned in a clearing where the trees had been cut in impossible ways.\n" +
            "\n" +
            "Then they found the thread.\n" +
            "\n" +
            "It ran across branches, tangled around stones, and vanished among the remains of creatures no one had identified. Yet there was no blood on the cloth. Only tiny, careful stitches, as though someone had tried to repair something the planet had already given up on.\n" +
            "\n" +
            "One explorer spoke of a young woman walking among monsters without losing her curiosity about the scenery. She carried enormous scissors. She stopped to pick up a torn scrap of fabric and asked whether anyone would miss it.\n" +
            "\n" +
            "No one knew what to say.\n" +
            "\n" +
            "The young woman smiled. She said she had been made with love, and that since learning to walk on her own, she had discovered one thing: everything alive deserves at least a chance.\n" +
            "\n" +
            "When the explorer looked again, the monsters and the young woman were gone. On a rock lay the fabric, now neatly mended.\n" +
            "\n" +
            "Note appended to the record: no one has explained who taught that doll to sew, or why scissors made for cutting could leave something whole.";

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
            logger?.LogInfo("[GWEN NATIVE] Tokens de ficha: " +
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
