using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Logging;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace UniversalSurvivorUnlocks
{
    // Localization is isolated to this survivor. No dependency on any other character.
    internal static class GwenSkillLocalization
    {
        private static readonly Dictionary<string, string> Names =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"A Thousand Cuts", "Mil cortes"},
                {"Thousand Cuts", "Mil cortes"},
                {"Hallowed Cuts", "Cortes sagrados"},
                {"Snip Snip!", "¡Corta, corta!"},
                {"Hallowed Mist", "Niebla Sagrada"},
                {"Skip 'n Slash", "Salto y corte"},
                {"Needlework", "Labores de aguja"}
            };

        // Textos completos curados a partir de los tokens originales publicados en
        // el log de Gwen 1.1.0. Las cifras se comprueban antes de sustituirlas.
        private static readonly Dictionary<string, string> CompleteDescriptions =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {
                    "GWEN__M1_DESCRIPTION",
                    "Corta hacia delante e inflige <style=cIsDamage>250% de daño</style>. " +
                    "Cada tercer golpe realiza una estocada que inflige <style=cIsDamage>350% de daño</style>. " +
                    "Genera una acumulación de <style=cIsDamage>Snippy</style> por golpe, " +
                    "hasta un máximo de <style=cIsDamage>4</style>."
                },
                {
                    "GWEN__M2_DESCRIPTION",
                    "Gwen realiza al menos dos cortes hacia delante con sus tijeras e inflige " +
                    "<style=cIsDamage>150% de daño</style> por corte. El último corte inflige " +
                    "<style=cIsDamage>400% de daño</style>. Ejecuta a enemigos con menos del " +
                    "<style=cIsHealing>5% de salud</style>."
                },
                {
                    "GWEN__UTIL_DESCRIPTION",
                    "Gwen se abalanza hacia delante y potencia sus Cortes sagrados durante " +
                    "<style=cIsUtility>5 s</style>: cada golpe inflige el daño del tercer impacto, " +
                    "más un <style=cIsDamage>50% adicional</style>, y obtiene " +
                    "<style=cIsDamage>50% de velocidad de ataque</style>. " +
                    "Cada impacto reduce <style=cIsUtility>4 s</style> su enfriamiento."
                },
                {
                    "GWEN__SPEC_DESCRIPTION",
                    "<style=cIsDamage>Ralentiza</style>. Gwen lanza una aguja en la dirección " +
                    "apuntada e inflige <style=cIsDamage>350% de daño</style>. Puede volver a usar " +
                    "Labores de aguja dos veces durante los siguientes 8 segundos; cada lanzamiento " +
                    "aumenta en <style=cIsDamage>2</style> el número de agujas disparadas."
                }
            };

        private static readonly Dictionary<string, string[]> ExpectedStats =
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                { "GWEN__M1_DESCRIPTION", new[] { "250%", "350%", "4" } },
                { "GWEN__M2_DESCRIPTION", new[] { "150%", "400%", "5%" } },
                { "GWEN__UTIL_DESCRIPTION", new[] { "5s", "50%", "4s" } },
                { "GWEN__SPEC_DESCRIPTION", new[] { "350%", "8 seconds", "2" } }
            };

        private const string SpanishPassive =
            "Los Cortes sagrados de Gwen, el centro de ¡Corta, corta! y Labores de aguja " +
            "infligen daño adicional según la salud máxima del objetivo.";

        private static bool StatsMatch(string source, string token)
        {
            if (!ExpectedStats.TryGetValue(token, out string[] required)) return false;
            foreach (string value in required)
                if (source.IndexOf(value, StringComparison.OrdinalIgnoreCase) < 0) return false;
            return true;
        }

        private static readonly List<object> Overlays = new List<object>();
        private static readonly HashSet<string> Applied = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        internal static void RegisterFor(ManualLogSource logger)
        {
            try
            {
                // RoR2 builds may expose the catalog as a FIELD or a PROPERTY.
                // The previous shared helper only probed two fields and failed at runtime.
                var catalogType = typeof(RoR2.Skills.SkillCatalog);
                var flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
                IEnumerable collection = null;
                foreach (string candidate in new[] { "allSkillDefs", "skillDefs" })
                {
                    var field = catalogType.GetField(candidate, flags);
                    if (field != null) collection = field.GetValue(null) as IEnumerable;
                    if (collection == null)
                    {
                        var property = catalogType.GetProperty(candidate, flags);
                        if (property != null && property.GetIndexParameters().Length == 0)
                            collection = property.GetValue(null, null) as IEnumerable;
                    }
                    if (collection != null) break;
                }
                if (collection == null)
                {
                    // Last resort: find an enumerable of SkillDef in catalog members.
                    foreach (var field in catalogType.GetFields(flags))
                    {
                        if (field.FieldType == typeof(string)) continue;
                        if (field.GetValue(null) is IEnumerable possible &&
                            ContainsSkillDef(possible)) { collection = possible; break; }
                    }
                }
                if (collection == null)
                {
                    logger?.LogWarning("[Gwen LOCALIZATION] SkillCatalog no ofrece una colección de skills; no se aplicaron traducciones.");
                    return;
                }

                int translatedNames = 0, translatedDescriptions = 0;
                foreach (var obj in collection)
                {
                    var skill = obj as RoR2.Skills.SkillDef;
                    if (skill == null) continue;
                    string token = skill.skillNameToken;
                    if (string.IsNullOrWhiteSpace(token) ||
                        token.IndexOf("Gwen", StringComparison.OrdinalIgnoreCase) < 0) continue;
                    string original = (Language.GetString(token) ?? "").Replace('’', '\u0027').Trim();
                    if (!Names.TryGetValue(original, out string spanish))
                    {
                        logger?.LogInfo("[GWEN AUDIT] Skill sin coincidencia | " + token + " | " + original);
                        continue;
                    }
                    AddSpanish(token, spanish);
                    translatedNames++;
                    if (!string.IsNullOrWhiteSpace(skill.skillDescriptionToken))
                    {
                        string englishDescription = Language.GetString(skill.skillDescriptionToken) ?? "";
                        if (CompleteDescriptions.TryGetValue(skill.skillDescriptionToken, out string complete))
                        {
                            if (StatsMatch(englishDescription, skill.skillDescriptionToken))
                            {
                                AddSpanish(skill.skillDescriptionToken, complete);
                                translatedDescriptions++;
                            }
                            else
                                logger?.LogWarning("[GWEN LOCALIZATION] Estadísticas cambiadas; " +
                                    "no se sobrescribe el texto original: " + skill.skillDescriptionToken);
                        }
                        else
                            logger?.LogInfo("[GWEN AUDIT] Descripción no curada | " +
                                skill.skillDescriptionToken + " | EN=" + englishDescription);
                    }
                }
                logger?.LogInfo("[Gwen LOCALIZATION] ES: " + translatedNames +
                    " nombres, " + translatedDescriptions + " descripciones completas verificadas contra estadísticas.");
            }
            catch (Exception ex)
            {
                logger?.LogWarning("[Gwen LOCALIZATION] " + ex.GetType().Name + ": " + ex.Message);
            }
        }

        internal static void RegisterPassive(string nameToken, string descriptionToken, ManualLogSource logger)
        {
            if (!string.IsNullOrWhiteSpace(nameToken))
                AddSpanish(nameToken, "Mil cortes");
            if (string.IsNullOrWhiteSpace(descriptionToken)) return;
            string original = Language.GetString(descriptionToken) ?? "";
            if (original.IndexOf("Hallowed Cuts", StringComparison.OrdinalIgnoreCase) >= 0 &&
                original.IndexOf("Needlework", StringComparison.OrdinalIgnoreCase) >= 0 &&
                original.IndexOf("maximum health", StringComparison.OrdinalIgnoreCase) >= 0)
                AddSpanish(descriptionToken, SpanishPassive);
            else
                logger?.LogWarning("[GWEN LOCALIZATION] Pasiva cambió; conservar original: " + original);
            logger?.LogInfo("[GWEN LOCALIZATION] Pasiva detectada | " + nameToken + " / " + descriptionToken);
        }

        private static void AddSpanish(string token, string translation)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(translation)) return;
            foreach (string locale in new[] { "es-419", "es-ES" })
            {
                if (!Applied.Add(locale + ":" + token)) continue;
                Overlays.Add(LanguageAPI.AddOverlay(token, translation, locale));
            }
        }

        internal static void RegisterSkins(SurvivorInfo survivor, ManualLogSource logger)
        {
            if (survivor?.SurvivorDef?.bodyPrefab == null) return;
            var skins = new HashSet<SkinDef>();
            var body = survivor.SurvivorDef.bodyPrefab.GetComponent<CharacterBody>();
            if (body != null)
            {
                try
                {
                    var method = typeof(SkinCatalog).GetMethod("GetBodySkins",
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
                        null, new[] { typeof(BodyIndex) }, null);
                    if (method != null && method.Invoke(null, new object[] { body.bodyIndex }) is IEnumerable known)
                        foreach (object obj in known) if (obj is SkinDef skin) skins.Add(skin);
                }
                catch (Exception ex) { logger?.LogWarning("[GWEN SKINS] Catálogo: " + ex.Message); }
            }
            var model = survivor.SurvivorDef.bodyPrefab.GetComponent<ModelLocator>();
            if (model?.modelTransform != null)
            {
                var controller = model.modelTransform.GetComponent<ModelSkinController>();
                if (controller != null)
                {
                    var member = controller.GetType().GetField("skins",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (member != null && member.GetValue(controller) is IEnumerable found)
                        foreach (object obj in found) if (obj is SkinDef skin) skins.Add(skin);
                }
            }
            int translated = 0;
            foreach (SkinDef skin in skins)
            {
                if (skin == null || string.IsNullOrWhiteSpace(skin.nameToken)) continue;
                string original = Language.GetString(skin.nameToken) ?? "";
                if (!TryTranslateSkin(original, out string localized))
                {
                    logger?.LogInfo("[GWEN AUDIT] Skin sin traducción | " + skin.nameToken + " | " + original);
                    continue;
                }
                // Evitamos tokens literales compartidos por otros survivors.
                if (skin.nameToken == "Default" || skin.nameToken == "DEFAULT_SKIN") continue;
                AddSpanish(skin.nameToken, localized);
                translated++;
            }
            logger?.LogInfo("[GWEN SKINS] Encontrados: " + skins.Count + "; traducidos: " + translated);
        }

        private static bool TryTranslateSkin(string name, out string spanish)
        {
            spanish = null;
            if (string.IsNullOrWhiteSpace(name)) return false;
            string key = name.Trim();
            return SkinNames.TryGetValue(key, out spanish);
        }

        private static readonly Dictionary<string, string> SkinNames =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"Rain Shepherd", "Pastora de la lluvia"},
                {"Space Groove", "Ritmo espacial"},
                {"Cafe Cuties", "Dulzura de cafetería"},
                {"Café Cuties", "Dulzura de cafetería"},
                {"Soul Fighter", "Luchadora de almas"},
                {"Battle Queen", "Reina de batalla"},
                {"Mythmaker", "Creadora de mitos"},
                {"Prestige", "Prestigio"},
                {"Prestige Gwen", "Gwen de prestigio"},
                {"Default Gwen", "Gwen predeterminada"},
            };

        private static bool ContainsSkillDef(IEnumerable sequence)
        {
            // Check array/list element type instead of enumerating a lazy collection.
            var type = sequence.GetType();
            if (type.IsArray) return typeof(RoR2.Skills.SkillDef).IsAssignableFrom(type.GetElementType());
            foreach (var interfaceType in type.GetInterfaces())
            {
                if (interfaceType.IsGenericType &&
                    interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>) &&
                    typeof(RoR2.Skills.SkillDef).IsAssignableFrom(interfaceType.GetGenericArguments()[0]))
                    return true;
            }
            return false;
        }
    }
}
