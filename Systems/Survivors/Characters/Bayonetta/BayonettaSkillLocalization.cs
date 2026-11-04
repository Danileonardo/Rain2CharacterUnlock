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
    /// Localización aislada del personaje Bayonetta, de Psyche (BayoBody).
    /// Solo utiliza SkillDef y SkinDef vinculados al prefab de Bayonetta.
    /// No depende de la DLL de BayoMod ni reemplaza textos de otros survivors.
    /// Las descripciones se traducen exclusivamente cuando se reconocen las
    /// mecánicas completas; las cifras se extraen del texto del mod instalado.
    /// </summary>
    internal static class BayonettaSkillLocalization
    {
        private static readonly Dictionary<string, string> Names =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "bullet arts", "Artes balísticas" },
                { "umbran techniques", "Técnicas de Umbra" },
                { "dodge", "Esquiva" },
                { "witch time", "Tiempo Brujo" },
                { "tetsuzanko/heel stomp", "Tetsuzanko / Pisotón de tacón" },
                { "tetsuzanko", "Tetsuzanko" },
                { "heel stomp", "Pisotón de tacón" },
                { "after burner kick", "Patada propulsora" },
                { "heel tornado", "Tornado de tacones" },
                { "death drop", "Caída mortal" },
                { "heel slide", "Deslizamiento de tacón" },
                { "breakdance", "Baile acrobático" },
                { "full moon shoot", "Disparo de luna llena" },
                { "wicked weave", "Tejido infernal" },
                { "punish attack", "Ataque de castigo" },
                { "punish", "Castigo" }
            };

        private static readonly HashSet<string> Applied =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        // Los LanguageOverlay deben mantenerse vivos durante toda la sesión.
        private static readonly List<object> Overlays = new List<object>();

        internal static void RegisterFor(SurvivorInfo survivor, ManualLogSource logger)
        {
            if (survivor == null || survivor.SurvivorDef == null ||
                survivor.SurvivorDef.bodyPrefab == null ||
                !string.Equals(survivor.BodyName, "BayoBody", StringComparison.OrdinalIgnoreCase))
                return;

            try
            {
                GameObject prefab = survivor.SurvivorDef.bodyPrefab;
                HashSet<SkillDef> skills = new HashSet<SkillDef>();
                foreach (GenericSkill slot in prefab.GetComponentsInChildren<GenericSkill>(true))
                {
                    if (slot == null) continue;
                    object family = ReadMember(slot, "skillFamily");
                    IEnumerable variants = ReadMember(family, "variants") as IEnumerable;
                    if (variants == null) continue;
                    foreach (object variant in variants)
                    {
                        SkillDef skill = ReadMember(variant, "skillDef") as SkillDef;
                        if (skill != null) skills.Add(skill);
                    }
                }

                int names = 0;
                int descriptions = 0;
                foreach (SkillDef skill in skills)
                {
                    string visible = VisibleText(skill.skillNameToken);
                    if (string.IsNullOrWhiteSpace(visible))
                        visible = ReadString(skill, "skillName");
                    TranslateSkill(visible, skill.skillNameToken, skill.skillDescriptionToken,
                        logger, ref names, ref descriptions);
                }

                // Las pasivas pueden estar fuera de las variantes de SkillFamily.
                SkillLocator locator = prefab.GetComponent<SkillLocator>();
                object passive = ReadMember(locator, "passiveSkill");
                if (passive != null)
                {
                    string token = ReadString(passive, "skillNameToken", "nameToken");
                    string name = VisibleText(token);
                    if (string.IsNullOrWhiteSpace(name))
                        name = ReadString(passive, "skillName", "skillNameString");
                    TranslateSkill(name, token,
                        ReadString(passive, "skillDescriptionToken", "descriptionToken"),
                        logger, ref names, ref descriptions);
                }

                int skins = TranslateSkins(prefab, logger);
                UsuLog.Verbose(logger, "[BAYONETTA LOCALIZATION] SkillDefs=" + skills.Count +
                    "; nombres=" + names + "; descripciones=" + descriptions +
                    "; skins=" + skins + "; ES-419/ES-ES.");
            }
            catch (Exception ex)
            {
                logger?.LogWarning("[BAYONETTA LOCALIZATION] " +
                    ex.GetType().Name + ": " + ex.Message);
            }
        }

        private static void TranslateSkill(string name, string nameToken, string descriptionToken,
            ManualLogSource logger, ref int names, ref int descriptions)
        {
            string normalized = NormalizeName(name);
            if (!Names.TryGetValue(normalized, out string spanishName))
            {
                UsuLog.Verbose(logger, "[BAYONETTA AUDIT] Skill no reconocida: " +
                    nameToken + " | " + name);
                return;
            }

            // No modificar tokens globales compartidos por varios personajes.
            if (IsCharacterToken(nameToken) && AddSpanish(nameToken, spanishName)) names++;
            else if (!IsCharacterToken(nameToken))
                UsuLog.Verbose(logger, "[BAYONETTA AUDIT] Token de nombre no exclusivo: " + nameToken);

            if (string.IsNullOrWhiteSpace(descriptionToken) || !IsCharacterToken(descriptionToken))
                return;
            string english = VisibleText(descriptionToken);
            string translated = TranslateDescription(normalized, english);
            if (!string.IsNullOrWhiteSpace(translated) &&
                TryRestoreStyledNumbers(english, ref translated) &&
                AddSpanish(descriptionToken, translated))
                descriptions++;
            else
                UsuLog.Verbose(logger, "[BAYONETTA AUDIT] Descripción conservada (sin equivalencia segura): " +
                    descriptionToken + " | " + english);
        }

        private static string TranslateDescription(string name, string source)
        {
            if (string.IsNullOrWhiteSpace(source)) return null;
            string plain = NormalizeDescription(source);
            if (plain.Length < 10) return null;
            MatchCollection all = Regex.Matches(plain, @"\d+(?:[.,]\d+)?%");
            string Percent(int index) => index < all.Count ? all[index].Value : null;

            if (name == "umbran techniques" &&
                plain.StartsWith("perform a variety of different moves depending on movement inputs") &&
                plain.EndsWith("whether or not bayonetta is grounded."))
                return "Realiza diferentes técnicas según la dirección de movimiento y si Bayonetta está en el suelo o en el aire.";

            if (name == "bullet arts" && all.Count >= 4 &&
                plain.Contains("5 hit punch sequence") && plain.Contains("wicked weave when released") &&
                plain.Contains("fire her guns"))
            {
                string result = "Ejecuta una secuencia de cinco puñetazos: los primeros cuatro infligen " +
                    Percent(0) + " de daño cada uno. El remate es una ráfaga que inflige " +
                    Percent(1) + " por golpe y, al soltar el botón, invoca un Tejido infernal de " +
                    Percent(2) + " de daño. Durante la combinación también dispara sus armas: " +
                    Percent(3) + " de daño por disparo.";
                if (plain.Contains("if dodge is used during this sequence"))
                    result += " Si esquivas y mantienes pulsado el ataque principal, reanudas la combinación donde la interrumpiste.";
                if (plain.Contains("airborne enemies") && plain.Contains("juggle"))
                    result += " Contra enemigos en el aire, los mantiene suspendidos, los aturde y los impulsa hacia arriba.";
                return result;
            }

            if (name == "after burner kick" && all.Count == 1 &&
                plain.StartsWith("launch into the air with a blazing kick") &&
                plain.Contains("lighter enemies"))
                return "Lánzate por el aire con una patada ardiente que empuja a los enemigos ligeros e inflige " +
                    Percent(0) + " de daño. Si apuntas hacia abajo, no los elevarás y conservarás tu impulso.";

            if (name == "heel tornado" && all.Count == 1 &&
                plain.StartsWith("perform a spinning kick") && plain.Contains("jump cancellable"))
                return "Ejecuta una patada giratoria que inflige continuamente " + Percent(0) +
                    " de daño, aparta a los enemigos ligeros y puede cancelarse con un salto.";

            if (name == "death drop" && all.Count == 1 &&
                plain.StartsWith("quickly descend with a downwards kick") && plain.Contains("spiking enemies"))
                return "Desciende rápidamente con una patada hacia abajo que inflige continuamente " +
                    Percent(0) + " de daño y lanza a los enemigos contra el suelo.";

            if (name == "heel slide" && all.Count == 2 &&
                plain.StartsWith("slide forwards at high speed") && plain.Contains("rising kick"))
                return "Deslízate hacia delante a gran velocidad e inflige " + Percent(0) +
                    " de daño. Mantén pulsada la habilidad al detenerte para ejecutar una patada ascendente " +
                    "de " + Percent(1) + " de daño que eleva a los enemigos ligeros.";

            if (name == "breakdance" && all.Count == 1 &&
                plain.StartsWith("perform a breakdance") && plain.Contains("jump cancellable"))
                return "Realiza una danza acrobática que inflige continuamente " + Percent(0) +
                    " de daño por impacto a los enemigos cercanos. Mantiene suspendidos a los enemigos " +
                    "aéreos y puede cancelarse con un salto.";

            if (name == "full moon shoot" && all.Count == 2 &&
                plain.StartsWith("perform a backflip") && plain.Contains("keep spinning"))
                return "Ejecuta una voltereta hacia atrás que eleva a los enemigos ligeros e inflige " +
                    Percent(0) + " de daño. Mantén pulsado el botón para seguir girando e infligir " +
                    Percent(1) + " de daño de forma continua.";

            if ((name == "tetsuzanko/heel stomp" || name == "tetsuzanko" || name == "heel stomp") &&
                all.Count == 1 && plain.StartsWith("lock onto enemies") &&
                plain.Contains("wicked weave"))
                return "Fija a un enemigo y pulsa el ataque principal o secundario para invocar " +
                    "un Tejido infernal sobre su posición e infligir " + Percent(0) + " de daño.";

            if (name == "dodge" && plain.StartsWith("dodge a short distance") &&
                plain.Contains("witch time") && all.Count == 0 &&
                plain.Contains("100 armor") && plain.Contains("350 armor"))
            {
                string result = "Esquiva una corta distancia y obtén invulnerabilidad momentánea y 100 de armadura. " +
                    "Si recibes un ataque durante la invulnerabilidad, activas Tiempo Brujo. Durante Tiempo Brujo " +
                    "obtienes 350 de armadura: los enemigos y proyectiles cercanos se ralentizan drásticamente.";
                if (plain.Contains("gravity altered"))
                    result += " Los ataques que elevan enemigos ligeros afectan ahora a todos los enemigos que no son jefes, " +
                        "y la gravedad de los enemigos cambia para permitir nuevas combinaciones.";
                return result;
            }
            return null;
        }

        private static int TranslateSkins(GameObject prefab, ManualLogSource logger)
        {
            HashSet<SkinDef> skins = new HashSet<SkinDef>();
            ModelLocator model = prefab.GetComponent<ModelLocator>();
            if (model != null && model.modelTransform != null)
            {
                ModelSkinController controller = model.modelTransform.GetComponent<ModelSkinController>();
                IEnumerable list = ReadMember(controller, "skins") as IEnumerable;
                if (list != null)
                    foreach (object item in list)
                        if (item is SkinDef skin) skins.Add(skin);
            }
            CharacterBody body = prefab.GetComponent<CharacterBody>();
            if (body != null)
            {
                try
                {
                    MethodInfo method = typeof(SkinCatalog).GetMethod("GetBodySkins",
                        BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                        null, new[] { typeof(BodyIndex) }, null);
                    if (method != null && method.Invoke(null, new object[] { body.bodyIndex }) is IEnumerable known)
                        foreach (object item in known)
                            if (item is SkinDef skin) skins.Add(skin);
                }
                catch (Exception ex)
                {
                    UsuLog.Verbose(logger, "[BAYONETTA SKINS] Catálogo no disponible: " + ex.Message);
                }
            }

            int total = 0;
            foreach (SkinDef skin in skins)
            {
                if (skin == null || string.IsNullOrWhiteSpace(skin.nameToken)) continue;
                string token = skin.nameToken;
                string original = NormalizeName(VisibleText(token));
                // Los nombres compartidos (p.ej. 'Default') solo se traducen si
                // el token es exclusivo de Bayonetta. Nunca alterar otros survivors.
                if (!IsCharacterToken(token))
                {
                    UsuLog.Verbose(logger, "[BAYONETTA SKINS] Token compartido, se conserva: " + token);
                    continue;
                }
                string spanish = null;
                if (original == "famed" || original == "famed witch") spanish = "Bruja célebre";
                else if (original == "default" || original == "default skin" ||
                         original == "bayonetta" || original == "classic") spanish = "Clásica";
                if (spanish != null && AddSpanish(token, spanish)) total++;
                else if (spanish == null)
                    UsuLog.Verbose(logger, "[BAYONETTA SKINS] Apariencia no reconocida: " + token +
                        " | " + original);
            }
            return total;
        }

        private static bool IsCharacterToken(string token)
        {
            return !string.IsNullOrWhiteSpace(token) &&
                (token.IndexOf("BAYO", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 token.IndexOf("PSYCHE", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static string NormalizeName(string source)
        {
            if (string.IsNullOrWhiteSpace(source)) return "";
            string s = Regex.Replace(source.Trim().Replace('’', '\''), @"\s+", " ").ToLowerInvariant();
            return Regex.Replace(s, @"\s*/\s*", "/");
        }

        private static string NormalizeDescription(string source)
        {
            if (string.IsNullOrWhiteSpace(source)) return "";
            string withoutTags = Regex.Replace(source, @"<[^>]+>", " ");
            return Regex.Replace(withoutTags.Trim().Replace('’', '\''), @"\s+", " ").ToLowerInvariant();
        }

        // Se conserva la etiqueta del daño porcentual cuando es posible
        // asociarla sin ambigüedad con el valor original. Los marcados de
        // frases completas u otros componentes especiales no se alteran.
        private static bool TryRestoreStyledNumbers(string original, ref string translated)
        {
            if (string.IsNullOrWhiteSpace(original)) return false;
            const string styleOpen = @"<style(?:\s+[^>]*|=[^>]*)>";
            string sourceWithoutStyle = Regex.Replace(original,
                @"</?style(?:\s+[^>]*|=[^>]*)?>", "", RegexOptions.IgnoreCase);
            if (Regex.IsMatch(sourceWithoutStyle, @"<[^>]+>")) return false;

            MatchCollection styled = Regex.Matches(original,
                @"<style(?:\s+[^>]*|=[^>]*)>\s*(\d+(?:[.,]\d+)?%)\s*</style>",
                RegexOptions.IgnoreCase);
            if (Regex.Matches(original, styleOpen, RegexOptions.IgnoreCase).Count != styled.Count)
                return false;

            int offset = 0;
            foreach (Match part in styled)
            {
                string value = part.Groups[1].Value;
                int at = translated.IndexOf(value, offset, StringComparison.Ordinal);
                if (at < 0) return false;
                translated = translated.Substring(0, at) + part.Value +
                    translated.Substring(at + value.Length);
                offset = at + part.Value.Length;
            }
            return true;
        }

        private static string VisibleText(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return "";
            string text = Language.GetString(token) ?? "";
            return string.Equals(text, token, StringComparison.OrdinalIgnoreCase) ? "" : text;
        }

        private static string ReadString(object source, params string[] fields)
        {
            foreach (string key in fields)
                if (ReadMember(source, key) is string value && !string.IsNullOrWhiteSpace(value))
                    return value;
            return "";
        }

        private static object ReadMember(object source, string name)
        {
            if (source == null) return null;
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            try
            {
                FieldInfo field = source.GetType().GetField(name, flags);
                if (field != null) return field.GetValue(source);
                PropertyInfo property = source.GetType().GetProperty(name, flags);
                if (property != null && property.GetIndexParameters().Length == 0)
                    return property.GetValue(source, null);
            }
            catch { }
            return null;
        }

        private static bool AddSpanish(string token, string value)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(value)) return false;
            bool added = false;
            foreach (string locale in new[] { "es-419", "es-ES" })
            {
                string id = locale + ":" + token;
                if (!Applied.Add(id)) continue;
                Overlays.Add(LanguageAPI.AddOverlay(token, value, locale));
                added = true;
            }
            return added;
        }
    }
}
