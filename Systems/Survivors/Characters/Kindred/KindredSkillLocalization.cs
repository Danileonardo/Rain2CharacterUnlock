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
    internal static class KindredSkillLocalization
    {
        private static readonly Dictionary<string, string> Names =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"Mark of the Kindred", "Marca de Kindred"},
                {"Dance of Arrows", "Danza de flechas"},
                {"Wolf's Frenzy", "Frenesí de Lobo"},
                {"Mounting Dread", "Temor creciente"},
                {"Lamb's Respite", "Tregua de Cordero"}
            };

        // Traducciones completas basadas en los textos nativos observados en el log
        // de DragonycksModdingComms Kindred 1.1.9. No mezclar sustituciones parciales.
        private static readonly Dictionary<string, string> CompleteDescriptions =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {
                    "KINDRED_M1_DESCRIPTION",
                    "<style=cIsUtility>Ágil</style>. Cordero dispara una flecha a su objetivo actual " +
                    "e inflige <style=cIsDamage>185% de daño</style>."
                },
                {
                    "KINDRED_M2_DESCRIPTION",
                    "Cordero se desplaza hacia delante y obtiene <style=cIsDamage>30% de velocidad de ataque</style> " +
                    "durante <style=cIsUtility>4 s</style>. Dispara una flecha a un máximo de 3 enemigos cercanos, " +
                    "priorizando los que estén en la dirección de apuntado, hasta el 150% de su alcance."
                },
                {
                    "KINDRED_UTIL_DESCRIPTION",
                    "Lobo se abalanza sobre el objetivo y se separa de Cordero durante " +
                    "<style=cIsUtility>8,5 s</style>. Ataca automáticamente al enemigo más cercano a su alcance. " +
                    "Sus ataques infligen <style=cIsDamage>180% de daño</style>. Tras eliminar a una presa, " +
                    "busca a la siguiente; si no quedan enemigos, regresa junto a Cordero."
                },
                {
                    "KINDRED_SPEC_DESCRIPTION",
                    "Cordero bendice el suelo bajo sus pies durante <style=cIsUtility>4 s</style>. " +
                    "Los aliados dentro de la zona no pueden bajar del <style=cIsHealing>10% de su salud máxima</style> " +
                    "y se vuelven <style=cIsDamage>inmunes</style> durante el tiempo restante. " +
                    "Cuando termina la bendición, todas las unidades seleccionables dentro del área " +
                    "se <style=cIsHealing>curan</style> por un valor equivalente al " +
                    "<style=cIsHealing>50%</style> de la <style=cIsHealth>salud máxima</style> de Cordero."
                }
            };

        // Hitos que protegen de aplicar números obsoletos ante un cambio del mod externo.
        private static readonly Dictionary<string, string[]> ExpectedStats =
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                { "KINDRED_M1_DESCRIPTION", new[] { "185%" } },
                { "KINDRED_M2_DESCRIPTION", new[] { "30%", "4s", "150%" } },
                { "KINDRED_UTIL_DESCRIPTION", new[] { "8.5s", "180%" } },
                { "KINDRED_SPEC_DESCRIPTION", new[] { "4s", "10%", "50%" } }
            };

        private const string SpanishPassive =
            "Cada 60 s, un enemigo aleatorio queda marcado durante 45 s. " +
            "Eliminarlo concede una acumulación de <style=cIsDamage>Marca de Kindred</style>.\n" +
            "Al moverse y atacar, Kindred acumula hasta 100 cargas de " +
            "<style=cIsDamage>Vigor del cazador</style>. Con 100 cargas, el siguiente golpe de " +
            "Cordero la cura entre un <style=cIsHealing>10% y un 25%</style> de su salud máxima, " +
            "según la salud que le falte.";

        private const string SpanishMarkKeyword =
            "<style=cKeywordName>Marca de Kindred</style>\n" +
            "<style=cSub>Aumenta el alcance de Kindred en 1, y el daño y " +
            "la velocidad de movimiento en un 5%.</style>";

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
                    logger?.LogWarning("[Kindred LOCALIZATION] SkillCatalog no ofrece una colección de skills; no se aplicaron traducciones.");
                    return;
                }

                int translatedNames = 0, translatedDescriptions = 0;
                foreach (var obj in collection)
                {
                    var skill = obj as RoR2.Skills.SkillDef;
                    if (skill == null) continue;
                    string token = skill.skillNameToken;
                    if (string.IsNullOrWhiteSpace(token) ||
                        token.IndexOf("Kindred", StringComparison.OrdinalIgnoreCase) < 0) continue;
                    string original = (Language.GetString(token) ?? "").Replace('’', '\u0027').Trim();
                    if (!Names.TryGetValue(original, out string spanish))
                    {
                        logger?.LogInfo("[KINDRED AUDIT] Skill sin coincidencia | " + token + " | " + original);
                        continue;
                    }
                    AddSpanish(token, spanish);
                    translatedNames++;
                    if (!string.IsNullOrWhiteSpace(skill.skillDescriptionToken))
                    {
                        string englishDescription = Language.GetString(skill.skillDescriptionToken) ?? "";
                        string translated = null;
                        if (CompleteDescriptions.TryGetValue(skill.skillDescriptionToken, out string complete))
                        {
                            if (StatsMatch(englishDescription, skill.skillDescriptionToken))
                                translated = complete;
                            else
                                logger?.LogWarning("[KINDRED LOCALIZATION] Estadísticas cambiadas; no se " +
                                    "sobrescribe el texto original: " + skill.skillDescriptionToken);
                        }
                        else
                            logger?.LogInfo("[KINDRED AUDIT] Descripción no curada | " +
                                skill.skillDescriptionToken + " | EN=" + englishDescription);
                        if (!string.IsNullOrWhiteSpace(translated))
                        {
                            AddSpanish(skill.skillDescriptionToken, translated);
                            translatedDescriptions++;
                        }
                        RegisterSkillKeywords(skill, logger);
                    }
                }
                logger?.LogInfo("[Kindred LOCALIZATION] ES: " + translatedNames +
                    " nombres, " + translatedDescriptions + " descripciones completas verificadas contra estadísticas.");
            }
            catch (Exception ex)
            {
                logger?.LogWarning("[Kindred LOCALIZATION] " + ex.GetType().Name + ": " + ex.Message);
            }
        }

        internal static void RegisterPassive(string nameToken, string descriptionToken, ManualLogSource logger)
        {
            if (!string.IsNullOrWhiteSpace(nameToken))
                AddSpanish(nameToken, "Marca de Kindred");
            if (string.IsNullOrWhiteSpace(descriptionToken)) return;
            string original = Language.GetString(descriptionToken) ?? "";
            // Los cuatro valores están presentes en la pasiva vista en juego.
            // No inventamos valores ni afectamos versiones futuras con otras cifras.
            if (original.Contains("60s") && original.Contains("45s") &&
                original.Contains("100") && original.Contains("10%") && original.Contains("25%"))
                AddSpanish(descriptionToken, SpanishPassive);
            else
                logger?.LogWarning("[KINDRED LOCALIZATION] Pasiva cambió; revisar texto nativo antes de traducir: " + original);
            logger?.LogInfo("[KINDRED LOCALIZATION] Pasiva detectada | " + nameToken + " / " + descriptionToken);
        }

        // La explicación que aparece a la derecha se expone mediante keywordTokens
        // (o PassiveSkill.keywordToken), no mediante skillDescriptionToken.
        internal static void RegisterPassiveKeywords(SurvivorInfo survivor, ManualLogSource logger)
        {
            var prefab = survivor?.SurvivorDef?.bodyPrefab;
            if (prefab == null) return;
            SkillLocator locator = prefab.GetComponent<SkillLocator>();
            if (locator == null) return;
            object passive = ReadMember(locator, "passiveSkill");
            if (passive == null) return;
            RegisterKeywordMembers(passive, logger);
        }

        private static void RegisterSkillKeywords(SkillDef skill, ManualLogSource logger)
        {
            if (skill == null) return;
            RegisterKeywordMembers(skill, logger);
        }

        private static void RegisterKeywordMembers(object owner, ManualLogSource logger)
        {
            foreach (string memberName in new[] { "keywordToken", "keywordTokens" })
            {
                object raw = ReadMember(owner, memberName);
                if (raw is string single)
                    RegisterKeyword(single, logger);
                else if (raw is IEnumerable multiple)
                    foreach (object entry in multiple)
                        if (entry is string token) RegisterKeyword(token, logger);
            }
        }

        private static object ReadMember(object owner, string name)
        {
            if (owner == null) return null;
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            try
            {
                var field = owner.GetType().GetField(name, flags);
                if (field != null) return field.GetValue(owner);
                var property = owner.GetType().GetProperty(name, flags);
                return property != null && property.GetIndexParameters().Length == 0
                    ? property.GetValue(owner, null) : null;
            }
            catch { return null; }
        }

        private static void RegisterKeyword(string token, ManualLogSource logger)
        {
            if (string.IsNullOrWhiteSpace(token)) return;
            string english = Language.GetString(token) ?? "";
            // Nunca modificar keywords globales que también usan otros survivors.
            if (english.IndexOf("Mark of the Kindred", StringComparison.OrdinalIgnoreCase) >= 0 &&
                english.IndexOf("range", StringComparison.OrdinalIgnoreCase) >= 0 &&
                english.Contains("5%"))
            {
                AddSpanish(token, SpanishMarkKeyword);
                logger?.LogInfo("[KINDRED KEYWORD] Traducción completa | " + token);
            }
            else
                logger?.LogInfo("[KINDRED AUDIT] Keyword pendiente | " + token + " | " + english);
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
                catch (Exception ex) { logger?.LogWarning("[KINDRED SKINS] Catálogo: " + ex.Message); }
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
                    logger?.LogInfo("[KINDRED AUDIT] Skin sin traducción | " + skin.nameToken + " | " + original);
                    continue;
                }
                // Evitamos tokens literales compartidos por otros survivors.
                if (skin.nameToken == "Default" || skin.nameToken == "DEFAULT_SKIN") continue;
                AddSpanish(skin.nameToken, localized);
                translated++;
            }
            logger?.LogInfo("[KINDRED SKINS] Encontrados: " + skins.Count + "; traducidos: " + translated);
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
                {"Prestige Porcelain", "Porcelana de prestigio"},
                {"Porcelain", "Porcelana"},
                {"Spirit Blossom", "Flor espiritual"},
                {"Woof and Lamb", "Lobo y Cordero"},
                {"DRX", "DRX"},
                {"Shadowfire", "Fuego sombrío"},
                {"Super Galaxy", "Supergalácticos"},
                {"Star Guardian", "Guardianes estelares"},
                {"Default Kindred", "Kindred predeterminado"},
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
