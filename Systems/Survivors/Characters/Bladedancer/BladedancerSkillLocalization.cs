using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using BepInEx.Logging;
using R2API;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Traducciones EXCLUSIVAS de Bladedancer (IreliaBody), obtenidas de
    /// IreliaMod.dll de Slendykray. No utiliza la lista de habilidades de LoL.
    /// Cubre las dos pasivas, las cuatro habilidades, el Overview y las skins
    /// de Maestria/Guardiana estelar. Preserva los valores dinamicos y el color
    /// del texto generado por el propio mod.
    /// </summary>
    internal static class BladedancerSkillLocalization
    {
        private const string Prefix = "SLENDYKRAY_IRELIA_";
        private const string BodyName = "IreliaBody";
        private static readonly string[] Locales = { "es-419", "es-ES" };
        private static readonly HashSet<string> Applied = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly List<object> ActiveOverlays = new List<object>();

        internal const string SpanishOverview =
            "Entrenada en las antiguas danzas de su provincia, Bladedancer adaptó su arte para el combate. " +
            "Con movimientos elegantes y cuidadosamente practicados, hace levitar numerosas hojas mortales.\n\n" +
            "< ! > Oleada de cuchillas puede darte el alcance adicional necesario contra enemigos voladores.\n\n" +
            "< ! > Filo de la vanguardia puede aturdir zonas enteras y ayudarte a salir de una multitud de enemigos.\n\n" +
            "< ! > Danza desafiante te vuelve invulnerable, pero también te deja inmóvil. Elige bien tu posición.\n\n" +
            "< ! > Nunca dejes de aprender. Siempre hay una forma que todavía no conoces.";

        private static readonly Dictionary<string, string> SpanishNames =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "PASSIVE_STRIKE_NAME", "Fervor jonio" },
                { "PASSIVE_SHURIKEN_NAME", "Shuriken jonio" },
                { "PRIMARY_SLASH_NAME", "Corte adaptado" },
                { "SECONDARY_DASH_NAME", "Oleada de cuchillas" },
                { "UTILITY_EDGE_NAME", "Filo de la vanguardia" },
                { "SPECIAL_SHIELD_NAME", "Danza desafiante" },
                { "MASTERY_SKIN_NAME", "Alternativa" },
                { "GUARDIAN_SKIN_NAME", "Guardiana estelar" },
                { "SUBTITLE", "La Danzarina de las Hojas" }
            };

        internal static void RegisterFor(SurvivorInfo survivor, ManualLogSource logger)
        {
            if (survivor == null || survivor.SurvivorDef == null ||
                !string.Equals(survivor.BodyName, BodyName, StringComparison.OrdinalIgnoreCase))
                return;

            // El Overview de la DLL se registra en este token. Cubrimos ademas
            // descriptionToken real si una version del mod utiliza otro nombre.
            string realOverviewToken = ReadString(survivor.SurvivorDef, "descriptionToken");
            int overviewCount = 0;
            if (OverlayOverview(Prefix + "DESCRIPTION")) overviewCount++;
            if (IsOwnToken(realOverviewToken) && OverlayOverview(realOverviewToken)) overviewCount++;

            int nameCount = 0, descriptionCount = 0;
            foreach (KeyValuePair<string, string> entry in SpanishNames)
                if (Overlay(Prefix + entry.Key, entry.Value)) nameCount++;

            // La DLL genera porcentajes desde su configuracion. Leemos las
            // DESCRIPCIONES YA FORMATEADAS y traducimos solo sus partes en ingles,
            // sin tocar números, <style>, colores ni el funcionamiento del mod.
            string[] descriptions =
            {
                "PASSIVE_STRIKE_DESCRIPTION", "PASSIVE_SHURIKEN_DESCRIPTION",
                "PRIMARY_SLASH_DESCRIPTION", "SECONDARY_DASH_DESCRIPTION",
                "UTILITY_EDGE_DESCRIPTION", "SPECIAL_SHIELD_DESCRIPTION"
            };
            foreach (string suffix in descriptions)
            {
                string token = Prefix + suffix;
                string original = ReadToken(token);
                string spanish = TranslateDescription(suffix, original);
                if (spanish != null)
                {
                    if (Overlay(token, spanish)) descriptionCount++;
                }
                else
                {
                    logger?.LogWarning("[BLADEDANCER I18N] Descripcion sin traduccion segura: " + token);
                }
            }

            UsuLog.Verbose(logger, "[BLADEDANCER I18N] DLL IreliaMod: overview=" + overviewCount +
                "; nombres y skins=" + nameCount + "; descripciones=" + descriptionCount +
                "/6. Sin modificar tokens globales ni la mision.");
        }

        private static bool OverlayOverview(string token)
        {
            if (!IsOwnToken(token)) return false;
            string original = ReadToken(token);
            // No reemplazar una descripcion desconocida de una version futura.
            if (original.IndexOf("Trained in the ancient dances", StringComparison.OrdinalIgnoreCase) < 0 ||
                original.IndexOf("Bladesurge may help", StringComparison.OrdinalIgnoreCase) < 0)
                return false;
            return Overlay(token, SpanishOverview);
        }

        private static string TranslateDescription(string suffix, string source)
        {
            if (string.IsNullOrWhiteSpace(source)) return null;
            string result = source;
            switch (suffix)
            {
                case "PASSIVE_STRIKE_DESCRIPTION":
                    if (!Has(source, "Every fourth attack becomes")) return null;
                    result = Replace(result, "Every fourth attack becomes a quick double attack",
                        "Cada cuarto ataque se convierte en un rápido ataque doble");
                    break;

                case "PASSIVE_SHURIKEN_DESCRIPTION":
                    if (!Has(source, "Every third attack")) return null;
                    result = Replace(result, "Every third attack fire shuriken for",
                        "Cada tercer ataque lanza shurikens que infligen");
                    result = Replace(result, "damage", "de daño");
                    break;

                case "PRIMARY_SLASH_DESCRIPTION":
                    if (!Has(source, "Attack with 6 blades")) return null;
                    result = Replace(result, "Agile", "Ágil");
                    result = Replace(result, "Attack with 6 blades, for", "Ataca con 6 hojas e inflige");
                    result = Replace(result, "Attacking repeatedly", "Atacar repetidamente");
                    result = Replace(result, "increases attack speed", "aumenta la velocidad de ataque");
                    result = Replace(result, "by", "en un");
                    result = Replace(result, "for", "durante");
                    result = Replace(result, "seconds", "segundos");
                    result = Replace(result, "damage", "de daño");
                    break;

                case "SECONDARY_DASH_DESCRIPTION":
                    if (!Has(source, "Dash forward")) return null;
                    result = Replace(result, "Agile", "Ágil");
                    result = Replace(result, "Dash forward, for", "Avanza con una embestida e inflige");
                    result = Replace(result, "Hitting enemy", "Golpear a un enemigo");
                    result = Replace(result, "lowers Special cooldown by 1s, up to 3s",
                        "reduce la recarga de la habilidad especial en 1 s, hasta 3 s");
                    result = Replace(result, "Eliminations", "Las eliminaciones");
                    result = Replace(result, "reset the dash", "restablecen la embestida");
                    result = Replace(result, "Enemies are", "Los enemigos quedan");
                    result = Replace(result, "marked", "marcados");
                    result = Replace(result, "when within execution threshold",
                        "cuando están dentro del umbral de ejecución");
                    result = Replace(result, "damage", "de daño");
                    break;

                case "UTILITY_EDGE_DESCRIPTION":
                    if (!Has(source, "Fly backwards")) return null;
                    result = Replace(result, "Stunning", "Aturdimiento");
                    result = Replace(result,
                        "Fly backwards, then release a diamond-shaped volley of blades for",
                        "Retrocede de un salto y lanza una ráfaga de hojas en forma de diamante que inflige");
                    result = Replace(result, "damage", "de daño");
                    break;

                case "SPECIAL_SHIELD_DESCRIPTION":
                    if (!Has(source, "Form a defensive barrier of blades")) return null;
                    result = Replace(result, "Stunning", "Aturdimiento");
                    result = Replace(result, "Form a defensive barrier of blades that makes you",
                        "Forma una barrera defensiva de hojas que te vuelve");
                    result = Replace(result, "After", "Después de");
                    result = Replace(result, "seconds", "segundos");
                    result = Replace(result, "the blades explode for up to",
                        "las hojas explotan e infligen hasta");
                    result = Replace(result, "Press again to explode early",
                        "Vuelve a pulsar para provocar la explosión antes de tiempo");
                    result = Replace(result, "damage", "de daño");
                    break;

                default:
                    return null;
            }

            // Si la DLL cambia la redaccion, no mostrar una traduccion a medias:
            // para esta version cada fragmento narrativo en ingles debe desaparecer.
            string plain = Regex.Replace(result, "<[^>]*>", " ");
            if (Regex.IsMatch(plain,
                @"\b(?:Every|Attack|Attacking|Agile|Stunning|Fly|After|Seconds|Dash|forward|damage|Press|Special|Eliminations|Enemies|Hitting|marked|for|by|cooldown|blades|shuriken|release|increases|when)\b",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
                return null;
            return string.Equals(result, source, StringComparison.Ordinal) ? null : result;
        }

        private static bool Has(string value, string fragment)
        {
            return value.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string Replace(string text, string oldText, string newText)
        {
            // Los limites de palabra evitan alterar atributos como cIsDamage.
            return Regex.Replace(text,
                @"(?<![A-Za-z])" + Regex.Escape(oldText) + @"(?![A-Za-z])",
                match => newText, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        private static string ReadToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return "";
            string result = Language.GetString(token) ?? "";
            return string.Equals(result, token, StringComparison.OrdinalIgnoreCase) ? "" : result;
        }

        private static bool IsOwnToken(string token)
        {
            return !string.IsNullOrWhiteSpace(token) &&
                token.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase);
        }

        private static string ReadString(object source, string name)
        {
            if (source == null) return "";
            try
            {
                const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                FieldInfo field = source.GetType().GetField(name, flags);
                if (field != null && field.FieldType == typeof(string))
                    return field.GetValue(source) as string ?? "";
                PropertyInfo property = source.GetType().GetProperty(name, flags);
                if (property != null && property.PropertyType == typeof(string) &&
                    property.GetIndexParameters().Length == 0)
                    return property.GetValue(source, null) as string ?? "";
            }
            catch { }
            return "";
        }

        private static bool Overlay(string token, string spanish)
        {
            if (!IsOwnToken(token) || string.IsNullOrWhiteSpace(spanish)) return false;
            bool changed = false;
            foreach (string locale in Locales)
            {
                if (!Applied.Add(locale + ":" + token)) continue;
                ActiveOverlays.Add(LanguageAPI.AddOverlay(token, spanish, locale));
                changed = true;
            }
            return changed;
        }
    }
}
