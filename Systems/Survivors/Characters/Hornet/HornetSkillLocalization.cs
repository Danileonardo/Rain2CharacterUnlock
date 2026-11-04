using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using BepInEx.Logging;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Localización ES de Hornet (fanto, HornetSilksong).
    /// Solo traduce los SkillDef y SkinDef encontrados en HornetBody.
    /// Las siete crestas son variantes de habilidad, no skins.
    /// Los textos desconocidos o con estadísticas no verificadas se conservan.
    /// </summary>
    internal static class HornetSkillLocalization
    {
        private sealed class Entry
        {
            internal readonly string Name;
            internal readonly Dictionary<string, string> Descriptions =
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            internal Entry(string name, params string[] pairs)
            {
                Name = name;
                for (int i = 0; i + 1 < pairs.Length; i += 2)
                    Descriptions[NormalizeDescription(pairs[i])] = pairs[i + 1];
            }
        }

        // Nombres y mecánicas publicados por el autor para HornetSilksong 1.0.0.
        // Los textos descriptivos se aplican SOLO cuando el texto completo coincide.
        private static readonly Dictionary<string, Entry> Skills =
            new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase)
            {
                {"silk spool", new Entry("Carrete de seda",
                    "Needle hits fill a silk spool (max 9). When health drops low, Hornet binds herself and heals.",
                    "Los golpes con la aguja llenan un carrete de seda (máximo 9). Cuando su salud baja demasiado, Hornet utiliza seda para vendarse y curarse.")},
                {"needle", new Entry("Aguja",
                    "Three-hit combo. Each crest is a variant: Hunter, Reaper, Wanderer, Beast, Witch, Architect, Shaman.",
                    "Combo de tres golpes. Cada cresta ofrece una variante: Cazador, Segador, Errante, Bestia, Bruja, Arquitecto y Chamán.")},
                {"pogo", new Entry("Golpe rebotador",
                    "Attack while airborne and looking down: dive, strike and bounce off the enemy, refreshing your jumps.",
                    "Ataca en el aire mientras miras hacia abajo: desciende, golpea y rebota sobre el enemigo para recuperar los saltos.")},
                {"silk spear", new Entry("Lanza de seda",
                    "Costs silk. Piercing spear throw.",
                    "Consume seda. Lanza una aguja que atraviesa a los enemigos.")},
                {"silkspear", new Entry("Lanza de seda")},
                {"thread storm", new Entry("Tormenta de hilos",
                    "Costs silk. Spinning storm of threads (hold to extend).",
                    "Consume seda. Gira en medio de una tormenta de hilos; mantén pulsado para prolongarla.")},
                {"silk dash", new Entry("Impulso de seda",
                    "Dash in any direction with no cooldown.",
                    "Desplázate rápidamente en cualquier dirección, sin tiempo de reutilización.")},
                {"clawline", new Entry("Arpón de seda",
                    "Harpoon toward a target.",
                    "Lanza la aguja a modo de arpón hacia un objetivo para impulsarte hasta él.")},
                {"cross stitch", new Entry("Punto de cruz",
                    "Parry counter.", "Para un ataque y contraataca de inmediato.")},
                {"rune rage", new Entry("Furia rúnica",
                    "Runic blasts around you.", "Desata explosiones rúnicas a tu alrededor.")},
                {"pale nails", new Entry("Aguijones pálidos",
                    "Homing nails.", "Lanza aguijones que persiguen a los enemigos.")},
                {"hunter crest", new Entry("Cresta del cazador")},
                {"reaper crest", new Entry("Cresta del segador")},
                {"wanderer crest", new Entry("Cresta del errante")},
                {"beast crest", new Entry("Cresta de la bestia")},
                {"witch crest", new Entry("Cresta de la bruja")},
                {"architect crest", new Entry("Cresta del arquitecto")},
                {"shaman crest", new Entry("Cresta del chamán")},
                {"hunter", new Entry("Cazador")},
                {"reaper", new Entry("Segador")},
                {"wanderer", new Entry("Errante")},
                {"beast", new Entry("Bestia")},
                {"witch", new Entry("Bruja")},
                {"architect", new Entry("Arquitecto")},
                {"shaman", new Entry("Chamán")},
                {"needle (hunter crest)", new Entry("Aguja (cresta del cazador)")},
                {"needle (reaper crest)", new Entry("Aguja (cresta del segador)")},
                {"needle (wanderer crest)", new Entry("Aguja (cresta del errante)")},
                {"needle (beast crest)", new Entry("Aguja (cresta de la bestia)")},
                {"needle (witch crest)", new Entry("Aguja (cresta de la bruja)")},
                {"needle (architect crest)", new Entry("Aguja (cresta del arquitecto)")},
                {"needle (shaman crest)", new Entry("Aguja (cresta del chamán)")},
                {"needle - hunter", new Entry("Aguja del cazador")},
                {"needle - reaper", new Entry("Aguja del segador")},
                {"needle - wanderer", new Entry("Aguja del errante")},
                {"needle - beast", new Entry("Aguja de la bestia")},
                {"needle - witch", new Entry("Aguja de la bruja")},
                {"needle - architect", new Entry("Aguja del arquitecto")},
                {"needle - shaman", new Entry("Aguja del chamán")}
            };

        // Mantener vivos los LanguageOverlay garantiza su persistencia.
        private static readonly List<object> Overlays = new List<object>();
        private static readonly HashSet<string> Applied =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        internal static void RegisterFor(SurvivorInfo survivor, ManualLogSource logger)
        {
            if (survivor == null || survivor.SurvivorDef == null ||
                survivor.SurvivorDef.bodyPrefab == null ||
                !string.Equals(survivor.BodyName, "HornetBody", StringComparison.OrdinalIgnoreCase))
                return;

            try
            {
                GameObject prefab = survivor.SurvivorDef.bodyPrefab;
                var found = new HashSet<SkillDef>();
                foreach (GenericSkill slot in prefab.GetComponentsInChildren<GenericSkill>(true))
                {
                    if (slot == null) continue;
                    object family = ReadMember(slot, "skillFamily");
                    if (!(ReadMember(family, "variants") is IEnumerable variants)) continue;
                    foreach (object variant in variants)
                    {
                        SkillDef skill = ReadMember(variant, "skillDef") as SkillDef;
                        if (skill != null) found.Add(skill);
                    }
                }

                int names = 0;
                int descriptions = 0;
                foreach (SkillDef skill in found)
                    TranslateSkill(skill, logger, ref names, ref descriptions);

                // Algunas pasivas son entradas de SkillLocator fuera de SkillFamily.
                SkillLocator locator = prefab.GetComponent<SkillLocator>();
                object passive = ReadMember(locator, "passiveSkill");
                if (passive != null)
                {
                    string token = ReadString(passive, "skillNameToken", "nameToken");
                    Entry entry;
                    if (FindEntry(VisibleText(token), out entry) ||
                        FindEntry(ReadString(passive, "skillName", "skillNameString"), out entry))
                    {
                        if (AddSpanish(token, entry.Name)) names++;
                        if (TranslateDescription(ReadString(passive, "skillDescriptionToken", "descriptionToken"), entry))
                            descriptions++;
                    }
                }

                RegisterNativeTokens(logger);
                int skins = TranslateSkins(prefab, logger);
                UsuLog.Verbose(logger, "[HORNET LOCALIZATION] Skills=" + found.Count +
                    " | nombres=" + names + " | descripciones verificadas=" + descriptions +
                    " | skins=" + skins + " | ES-419/ES-ES");
            }
            catch (Exception ex)
            {
                logger?.LogWarning("[HORNET LOCALIZATION] " + ex.GetType().Name + ": " + ex.Message);
            }
        }

        private static void TranslateSkill(SkillDef skill, ManualLogSource logger,
            ref int names, ref int descriptions)
        {
            if (skill == null) return;
            string nameToken = skill.skillNameToken;
            Entry entry;
            if (!FindEntry(VisibleText(nameToken), out entry) &&
                !FindEntry(ReadString(skill, "skillName"), out entry))
            {
                // Hay mods que cambian el texto visible, pero conservan el token.
                string nativeName;
                if (!NativeNames.TryGetValue(nameToken ?? "", out nativeName))
                {
                    UsuLog.Verbose(logger, "[HORNET AUDIT] Nombre no identificado: " +
                        nameToken + " | " + VisibleText(nameToken));
                    return;
                }
                entry = new Entry(nativeName);
            }

            if (AddSpanish(nameToken, entry.Name)) names++;
            if (TranslateDescription(skill.skillDescriptionToken, entry))
                descriptions++;
            else
                UsuLog.Verbose(logger, "[HORNET AUDIT] Descripcion sin equivalencia exacta: " +
                    skill.skillDescriptionToken + " | " + VisibleText(skill.skillDescriptionToken));
        }

        private static bool TranslateDescription(string token, Entry entry)
        {
            if (string.IsNullOrWhiteSpace(token) || entry == null) return false;
            string original = VisibleText(token);
            if (string.IsNullOrWhiteSpace(original)) return false;

            // Los textos reales VIBON_HORNET_* contienen números calculados y
            // etiquetas <style>. Una comparación exacta con frases publicadas
            // no puede funcionar en esas condiciones. Conservar los tags y las
            // cifras actuales, sustituyendo solo fragmentos ingleses conocidos.
            string translation = TranslateNativeDescription(token, original);
            if (string.IsNullOrWhiteSpace(translation))
                entry.Descriptions.TryGetValue(NormalizeDescription(original), out translation);
            return !string.IsNullOrWhiteSpace(translation) && AddSpanish(token, translation);
        }

        // Entradas verificadas en HornetMod.dll (com.vibon.HornetSilksong 1.0.0).
        // La DLL construye las descripciones uniendo piezas con porcentajes de configuracion.
        // Traducimos las piezas SIN reconstruir el valor ni retirar el marcado <style>.
        private static string TranslateNativeDescription(string token, string english)
        {
            if (string.IsNullOrWhiteSpace(english) ||
                !token.StartsWith("VIBON_HORNET_", StringComparison.OrdinalIgnoreCase))
                return null;
            string id = token.ToUpperInvariant();
            string result = english;

            // HornetMod concatena esta frase de pogo DESPUES de construir
            // cada descripcion de crest (no pertenece solo a Hunter).
            // Traducirla para las SIETE variantes preserva el marcado de color,
            // los numeros calculados y el resto de la descripcion nativa.
            if (id.StartsWith("VIBON_HORNET_PRIMARY_", StringComparison.Ordinal) &&
                id.EndsWith("_DESCRIPTION", StringComparison.Ordinal))
            {
                result = ReplacePhrases(result,
                    "Look down in the air to", "Mira hacia abajo mientras estás en el aire para",
                    "pogo", "rebotar",
                    "off enemies", "sobre los enemigos");
            }

            if (id == "VIBON_HORNET_PASSIVE_DESCRIPTION")
                result = ReplacePhrases(result,
                    "Needle hits weave", "Los golpes de aguja generan",
                    "Silk Skills spend it.", "Las habilidades de seda la consumen.",
                    "With a full spool, dropping below", "Con el carrete lleno, bajar del",
                    "Binds", "activa el vendaje",
                    ", consuming all silk to heal", ", consumiendo toda la seda para recuperar",
                    "max health", "de la salud máxima",
                    "health", "de salud", "(max", "(máximo",
                    "silk", "seda");
            else if (id == "VIBON_HORNET_PRIMARY_HUNTER_DESCRIPTION")
                result = ReplacePhrases(result,
                    "Slash with the needle for", "Ataca con la aguja e inflige",
                    "Look down in the air to", "Mira hacia abajo en el aire para",
                    "off enemies", "sobre los enemigos", "pogo", "rebotar",
                    "damage", "de daño");
            else if (id == "VIBON_HORNET_PRIMARY_REAPER_DESCRIPTION")
                result = ReplacePhrases(result,
                    "Swing a wide, slow scythe arc for", "Realiza un amplio barrido lento de guadaña e inflige",
                    "Hits weave", "Los impactos generan",
                    "double silk", "el doble de seda", "damage", "de daño");
            else if (id == "VIBON_HORNET_PRIMARY_WANDERER_DESCRIPTION")
                result = ReplacePhrases(result,
                    "Rapid short stabs for", "Realiza estocadas cortas y rápidas que infligen",
                    "critical chance", "de probabilidad de golpe crítico",
                    "with", "con", "damage", "de daño");
            else if (id == "VIBON_HORNET_PRIMARY_BEAST_DESCRIPTION")
                result = ReplacePhrases(result,
                    "Savage claws for", "Ataca con garras feroces e inflige",
                    "healing", "recuperando", "of damage dealt", "del daño infligido",
                    "damage", "de daño");
            else if (id == "VIBON_HORNET_PRIMARY_WITCH_DESCRIPTION")
                result = ReplacePhrases(result,
                    "Lash a long silk whip for", "Azota con un largo látigo de seda e inflige",
                    "damage", "de daño");
            else if (id == "VIBON_HORNET_PRIMARY_ARCHITECT_DESCRIPTION")
                result = ReplacePhrases(result,
                    "Drill forward, hitting", "Perfora hacia delante y golpea",
                    "times", "veces", "for", "e inflige", "each", "por impacto",
                    "damage", "de daño");
            else if (id == "VIBON_HORNET_PRIMARY_SHAMAN_DESCRIPTION")
                result = ReplacePhrases(result,
                    "While equipped, Silk Skills deal", "Mientras llevas esta cresta, las habilidades de seda infligen",
                    "Slash for", "Ataca e inflige", "damage", "de daño");
            else if (id == "VIBON_HORNET_SECONDARY_SPEAR_DESCRIPTION")
                result = ReplacePhrases(result,
                    "Hurl the needle in a", "Lanza la aguja en una trayectoria",
                    "piercing", "perforante", "line for", "que inflige",
                    "Costs", "Consume", "silk", "unidades de seda", "damage", "de daño");
            else if (id == "VIBON_HORNET_SECONDARY_STORM_DESCRIPTION")
                result = ReplacePhrases(result,
                    "Spin a storm of silk around you, hitting repeatedly for", "Gira en una tormenta de seda que golpea repetidamente e inflige",
                    "Hold to extend it.", "Mantén pulsado para prolongarla.",
                    "Costs", "Consume", "silk", "unidades de seda", "damage", "de daño");
            else if (id == "VIBON_HORNET_UTILITY_DASH_DESCRIPTION")
                result = ReplacePhrases(result,
                    "No cooldown.", "Sin tiempo de reutilización.",
                    "Dash in any direction.", "Impúlsate en cualquier dirección.",
                    "In the air, dash where you aim.", "En el aire, impúlsate hacia donde apuntas.");
            else if (id == "VIBON_HORNET_UTILITY_HARPOON_DESCRIPTION")
                result = ReplacePhrases(result,
                    "Fire a harpoon and reel yourself to it, striking nearby enemies for", "Lanza un arpón y atráete hacia él, golpeando a los enemigos cercanos e infligiendo",
                    "on arrival.", "al llegar.", "damage", "de daño");
            else if (id == "VIBON_HORNET_SPECIAL_CROSS_DESCRIPTION")
                result = ReplacePhrases(result,
                    "Take a parry stance.", "Adopta una postura de parada.",
                    "a hit unleashes an invulnerable stitching dash that", "un golpe desencadena una embestida invulnerable que",
                    "Parrying", "Parar", "stuns", "aturde", "for", "e inflige",
                    "Costs", "Consume", "silk", "unidades de seda", "damage", "de daño");
            else if (id == "VIBON_HORNET_SPECIAL_RUNE_DESCRIPTION")
                result = ReplacePhrases(result,
                    "waves of silk runes around you, each blast dealing", "oleadas de runas de seda a tu alrededor; cada explosión inflige",
                    "Erupt", "Desata", "damage", "de daño",
                    "Costs", "Consume", "silk", "unidades de seda");
            else if (id == "VIBON_HORNET_SPECIAL_NAILS_DESCRIPTION")
            {
                result = ReplacePhrases(result,
                    "Summon", "Invoca", "homing", "teledirigidos",
                    "nails for", "aguijones que infligen", "each", "cada uno",
                    "damage", "de daño", "Costs", "Consume", "silk", "unidades de seda");
                // La etiqueta de color permanece unida al adjetivo sin invertir su estilo.
                result = Regex.Replace(result,
                    @"(<style[^>]*>teledirigidos</style>)\s+aguijones que infligen",
                    "aguijones $1 que infligen", RegexOptions.IgnoreCase);
            }
            return result == english ? null : result;
        }

        private static string ReplacePhrases(string value, params string[] pairs)
        {
            string output = value;
            for (int i = 0; i + 1 < pairs.Length; i += 2)
            {
                string original = pairs[i];
                // Para palabras aisladas impedir sustituciones dentro de otro termino.
                string pattern = Regex.Escape(original);
                if (Regex.IsMatch(original, @"^[A-Za-z]+$"))
                    pattern = @"\b" + pattern + @"\b";
                output = Regex.Replace(output, pattern, m => pairs[i + 1],
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            }
            return output;
        }

        // Ademas de SkillFamily, registrar tokens nativos por identificador: algunas
        // pasivas/variantes no estan en un slot y requieren overlay independiente.
        private static readonly Dictionary<string, string> NativeNames =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"VIBON_HORNET_SUBTITLE", "Princesa protectora de Hallownest"},
                {"VIBON_HORNET_PASSIVE_NAME", "Carrete de seda"},
                {"VIBON_HORNET_PRIMARY_HUNTER_NAME", "Cresta del cazador"},
                {"VIBON_HORNET_PRIMARY_REAPER_NAME", "Cresta del segador"},
                {"VIBON_HORNET_PRIMARY_WANDERER_NAME", "Cresta del errante"},
                {"VIBON_HORNET_PRIMARY_BEAST_NAME", "Cresta de la bestia"},
                {"VIBON_HORNET_PRIMARY_WITCH_NAME", "Cresta de la bruja"},
                {"VIBON_HORNET_PRIMARY_ARCHITECT_NAME", "Cresta del arquitecto"},
                {"VIBON_HORNET_PRIMARY_SHAMAN_NAME", "Cresta del chamán"},
                {"VIBON_HORNET_SECONDARY_SPEAR_NAME", "Lanza de seda"},
                {"VIBON_HORNET_SECONDARY_STORM_NAME", "Tormenta de hilos"},
                {"VIBON_HORNET_UTILITY_DASH_NAME", "Impulso de seda"},
                {"VIBON_HORNET_UTILITY_HARPOON_NAME", "Arpón de seda"},
                {"VIBON_HORNET_SPECIAL_CROSS_NAME", "Punto de cruz"},
                {"VIBON_HORNET_SPECIAL_RUNE_NAME", "Furia rúnica"},
                {"VIBON_HORNET_SPECIAL_NAILS_NAME", "Aguijones pálidos"},
                {"VIBON_HORNET_MASTERY_SKIN_NAME", "Alternativa"},
                {"VIBON_HORNET_masteryAchievement_NAME", "Hornet: Maestría"},
                {"VIBON_HORNET_masteryAchievement_DESCRIPTION", "Con Hornet, gana una partida o oblitérate en dificultad Monzón."}
            };

        private static readonly string[] NativeDescriptionTokens =
        {
            "VIBON_HORNET_PASSIVE_DESCRIPTION",
            "VIBON_HORNET_PRIMARY_HUNTER_DESCRIPTION",
            "VIBON_HORNET_PRIMARY_REAPER_DESCRIPTION",
            "VIBON_HORNET_PRIMARY_WANDERER_DESCRIPTION",
            "VIBON_HORNET_PRIMARY_BEAST_DESCRIPTION",
            "VIBON_HORNET_PRIMARY_WITCH_DESCRIPTION",
            "VIBON_HORNET_PRIMARY_ARCHITECT_DESCRIPTION",
            "VIBON_HORNET_PRIMARY_SHAMAN_DESCRIPTION",
            "VIBON_HORNET_SECONDARY_SPEAR_DESCRIPTION",
            "VIBON_HORNET_SECONDARY_STORM_DESCRIPTION",
            "VIBON_HORNET_UTILITY_DASH_DESCRIPTION",
            "VIBON_HORNET_UTILITY_HARPOON_DESCRIPTION",
            "VIBON_HORNET_SPECIAL_CROSS_DESCRIPTION",
            "VIBON_HORNET_SPECIAL_RUNE_DESCRIPTION",
            "VIBON_HORNET_SPECIAL_NAILS_DESCRIPTION"
        };

        internal static readonly string SpanishOverview =
            "Hornet es una cazadora veloz que convierte la seda en artes letales.<color=#CCD3E0>\n\n" +
            "< ! > Los golpes de aguja llenan tu carrete de seda; las habilidades de seda la consumen.\n\n" +
            "< ! > Mira hacia abajo mientras estás en el aire para rebotar sobre enemigos: cada rebote restaura tus saltos.\n\n" +
            "< ! > Impulso de seda no tiene tiempo de reutilización. Encadena impulsos en cualquier dirección.\n\n" +
            "< ! > Las crestas cambian la forma de atacar con la aguja: elige la que mejor se adapte a la partida.</color>";

        private static void RegisterNativeTokens(ManualLogSource logger)
        {
            foreach (KeyValuePair<string, string> item in NativeNames)
            {
                string original = VisibleText(item.Key);
                if (string.IsNullOrWhiteSpace(original)) continue;
                AddSpanish(item.Key, item.Value);
            }
            // Las descripciones del mod se resuelven ya con sus valores dinamicos
            // en Language.GetString: mantener exactamente esos numeros.
            foreach (string token in NativeDescriptionTokens)
            {
                // Puede haber sido registrado desde SkillFamily más arriba.
                if (Applied.Contains("es-ES:" + token) || Applied.Contains("es-419:" + token))
                    continue;
                string original = VisibleText(token);
                string translation = TranslateNativeDescription(token, original);
                if (string.IsNullOrWhiteSpace(translation))
                    UsuLog.Verbose(logger, "[HORNET L10N] Sin traduccion nativa: " + token);
                else AddSpanish(token, translation);
            }
            // Overview del selector nativo; el panel USU tiene ResolveDescription.
            if (!string.IsNullOrWhiteSpace(VisibleText("VIBON_HORNET_DESCRIPTION")))
                AddSpanish("VIBON_HORNET_DESCRIPTION", SpanishOverview);
        }

        private static int TranslateSkins(GameObject prefab, ManualLogSource logger)
        {
            var skins = new HashSet<SkinDef>();
            ModelLocator locator = prefab.GetComponent<ModelLocator>();
            if (locator != null && locator.modelTransform != null)
            {
                var controller = locator.modelTransform.GetComponent<ModelSkinController>();
                if (controller != null && ReadMember(controller, "skins") is IEnumerable items)
                    foreach (object item in items)
                        if (item is SkinDef skin) skins.Add(skin);
            }

            CharacterBody body = prefab.GetComponent<CharacterBody>();
            if (body != null)
            {
                try
                {
                    MethodInfo method = typeof(SkinCatalog).GetMethod("GetBodySkins",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                        null, new[] { typeof(BodyIndex) }, null);
                    if (method != null &&
                        method.Invoke(null, new object[] { body.bodyIndex }) is IEnumerable found)
                        foreach (object item in found)
                            if (item is SkinDef skin) skins.Add(skin);
                }
                catch (Exception ex)
                {
                    UsuLog.Verbose(logger, "[HORNET SKINS] Catalogo: " + ex.Message);
                }
            }

            int names = 0;
            foreach (SkinDef skin in skins)
            {
                if (skin == null || string.IsNullOrWhiteSpace(skin.nameToken)) continue;
                string token = skin.nameToken;
                string original = VisibleText(token);
                // Solo skins vinculadas a Hornet; nunca tokens compartidos como 'Default'.
                bool owned = token.IndexOf("HORNET", StringComparison.OrdinalIgnoreCase) >= 0;
                if (owned && (EqualsText(original, "Default") ||
                              EqualsText(original, "Default Hornet") ||
                              EqualsText(original, "Hornet Default") ||
                              EqualsText(original, "Default Skin") ||
                              EqualsText(original, "Alternate")))
                {
                    if (AddSpanish(token, EqualsText(original, "Alternate") ? "Alternativa" : "Predeterminada")) names++;
                }
                else
                    UsuLog.Verbose(logger, "[HORNET SKINS] Sin skin verificada: " +
                        token + " | " + original);
            }
            return names;
        }

        private static bool FindEntry(string raw, out Entry entry)
        {
            return Skills.TryGetValue(NormalizeName(raw), out entry);
        }

        private static bool EqualsText(string a, string b)
        {
            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "";
            return Regex.Replace(name.Trim().Replace('’', '\''), @"\s+", " ")
                .ToLowerInvariant();
        }

        private static string NormalizeDescription(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";
            string stripped = Regex.Replace(text, "<[^>]+>", " ");
            return Regex.Replace(stripped.Trim().Replace('’', '\''), @"\s+", " ")
                .ToLowerInvariant();
        }

        private static string VisibleText(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return "";
            string text = Language.GetString(token) ?? "";
            return EqualsText(text, token) ? "" : text;
        }

        private static string ReadString(object source, params string[] fields)
        {
            foreach (string field in fields)
                if (ReadMember(source, field) is string s && !string.IsNullOrWhiteSpace(s))
                    return s;
            return "";
        }

        private static object ReadMember(object source, string key)
        {
            if (source == null) return null;
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            try
            {
                FieldInfo field = source.GetType().GetField(key, flags);
                if (field != null) return field.GetValue(source);
                PropertyInfo property = source.GetType().GetProperty(key, flags);
                if (property != null && property.GetIndexParameters().Length == 0)
                    return property.GetValue(source, null);
            }
            catch { }
            return null;
        }

        private static bool AddSpanish(string token, string translation)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(translation))
                return false;
            bool added = false;
            foreach (string locale in new[] { "es-419", "es-ES" })
            {
                string id = locale + ":" + token;
                if (!Applied.Add(id)) continue;
                Overlays.Add(LanguageAPI.AddOverlay(token, translation, locale));
                added = true;
            }
            return added;
        }
    }
}
