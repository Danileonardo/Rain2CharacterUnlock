using System;
using System.Collections.Generic;
using BepInEx.Logging;
using R2API;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Traducción vinculada EXCLUSIVAMENTE a los tokens nativos del
    /// DarthVaderMod.dll entregado por el usuario (POPCORN_DARTHVADER_BODY_*).
    /// Los valores configurables de daño/energía/duración se conservan
    /// transformando el texto ya formateado por el mod, sin cifras fijas.
    /// </summary>
    internal static class DarthVaderSkillLocalization
    {
        private const string Prefix = "POPCORN_DARTHVADER_BODY_";
        private static readonly string[] Locales = { "es-419", "es-ES" };
        private static readonly List<object> Overlays = new List<object>();
        private static readonly HashSet<string> Registered =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Identificadores sacados de DarthVaderMod.dll (29-10-2026).
        // No se utiliza el texto visible «Default» como token global.
        private static readonly Dictionary<string, string> Names =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "SUBTITLE", "El Elegido" },
                { "PASSIVE_NAME", "El Elegido" },
                { "PASSIVE_COOLDOWN_NAME", "Sistema de recargas" },
                { "PASSIVE_ENERGY_NAME", "Sistema de energía" },
                { "PRIMARY_SLASH_NAME", "Sable de luz" },
                { "SECONDARY_FORCE_NAME", "Empuje/Atracción de la Fuerza" },
                { "UTILITY_DEFLECT_NAME", "Desviar" },
                { "SPECIAL_RAGE_NAME", "Ira desatada" },
                { "DEFAULT_SKIN_NAME", "Sable de luz rojo" },
                { "BLUESABER_SKIN_NAME", "Sable de luz azul" },
                { "YELLOWSABER_SKIN_NAME", "Sable de luz amarillo" },
                { "DARKSABER_SKIN_NAME", "Sable oscuro" }
            };

        private static readonly string[] Descriptions =
        {
            "PASSIVE_DESCRIPTION", "PASSIVE_COOLDOWN_DESCRIPTION",
            "PASSIVE_ENERGY_DESCRIPTION", "PRIMARY_SLASH_DESCRIPTION",
            "SECONDARY_FORCE_DESCRIPTION", "UTILITY_DEFLECT_DESCRIPTION",
            "SPECIAL_RAGE_DESCRIPTION",
            "KEYWORD_COOLDOWN_SLASH", "KEYWORD_ENERGY_SLASH",
            "KEYWORD_COOLDOWN_FORCE", "KEYWORD_ENERGY_FORCE",
            "KEYWORD_COOLDOWN_DEFLECT", "KEYWORD_ENERGY_DEFLECT",
            "KEYWORD_COOLDOWN_RAGE", "KEYWORD_ENERGY_RAGE"
        };

        internal static void RegisterFor(SurvivorInfo survivor, ManualLogSource logger)
        {
            if (survivor == null || !string.Equals(survivor.BodyName,
                    "DarthVaderBody", StringComparison.OrdinalIgnoreCase) ||
                survivor.SurvivorDef == null || survivor.SurvivorDef.bodyPrefab == null)
                return;

            int names = 0, descriptions = 0, unresolved = 0;
            foreach (KeyValuePair<string, string> pair in Names)
                if (Register(Prefix + pair.Key, pair.Value)) names++;

            foreach (string suffix in Descriptions)
            {
                string token = Prefix + suffix;
                string source = Language.GetString(token) ?? "";
                if (string.IsNullOrWhiteSpace(source) || source == "???" ||
                    string.Equals(source, token, StringComparison.OrdinalIgnoreCase))
                {
                    unresolved++;
                    UsuLog.Verbose(logger, "[VADER ES] Token pendiente al registrar: " + token);
                    continue;
                }

                string translated = TranslateDescription(suffix, source);
                if (translated != null && Register(token, translated)) descriptions++;
                else if (translated == null)
                {
                    unresolved++;
                    UsuLog.Verbose(logger, "[VADER ES] Texto no reconocido (sin reemplazo): " + token);
                }
            }
            UsuLog.Verbose(logger, "[VADER ES] Nombres/skins=" + names +
                " | descripciones/paneles=" + descriptions +
                " | pendientes=" + unresolved + " | idiomas=es-419,es-ES");
        }

        private static bool Register(string token, string value)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(value)) return false;
            bool added = false;
            foreach (string language in Locales)
            {
                if (!Registered.Add(language + ":" + token)) continue;
                Overlays.Add(LanguageAPI.AddOverlay(token, value, language));
                added = true;
            }
            return added;
        }

        // Sustituciones deliberadamente literales: preservan las etiquetas
        // <style=...>, los saltos de línea y TODOS los números de la DLL.
        // No se publica una traducción si la versión del mod cambia de sentido.
        private static string TranslateDescription(string suffix, string original)
        {
            string text = original;
            switch (suffix)
            {
                case "PASSIVE_DESCRIPTION":
                    if (!Has(text, "Attackspeed bonuses are converted to Damage")) return null;
                    return text.Replace("Attackspeed bonuses are converted to Damage.",
                          "Las bonificaciones de velocidad de ataque se convierten en daño.")
                        .Replace("Movespeed bonuses are converted to Armor.",
                          "Las bonificaciones de velocidad de movimiento se convierten en armadura.");

                case "PASSIVE_COOLDOWN_DESCRIPTION":
                    if (!Has(text, "Skills have cooldowns") || !Has(text, "holding jump fastfalls")) return null;
                    return text.Replace("Skills have cooldowns, melee attacks reduce all cooldowns by 1 second.",
                          "Las habilidades tienen tiempos de reutilización; los ataques cuerpo a cuerpo los reducen 1 segundo.")
                        .Replace("Reduced fallspeed, holding jump fastfalls.",
                          "Caes más lentamente; mantén pulsado el salto para descender rápidamente.");

                case "PASSIVE_ENERGY_DESCRIPTION":
                    if (!Has(text, "Skills have energy costs") || !Has(text, "Stock-based items")) return null;
                    return text.Replace("Skills have energy costs, regenerate ",
                          "Las habilidades consumen energía. Regeneras ")
                        .Replace("% energy per second, melee attacks regen ",
                          "% de energía por segundo; los ataques cuerpo a cuerpo recuperan ")
                        .Replace(" energy, scaling with attackspeed.",
                          " de energía, proporcionalmente a la velocidad de ataque.")
                        .Replace("Stock-based items increases total energy.",
                          "Los objetos que añaden cargas aumentan la energía máxima.")
                        .Replace("Cooldown-based items decreases energy costs.",
                          "Los objetos que reducen las recargas disminuyen los costes de energía.")
                        .Replace("Holding jump allows Darth Vader to super jump at a cost, holding it also slows down his fall at a cost.",
                          "Mantén pulsado el salto para realizar un supersalto o ralentizar la caída, consumiendo energía.");

                case "PRIMARY_SLASH_DESCRIPTION":
                    if (!Has(text, "Swing forward for") || !Has(text, "% damage")) return null;
                    return text.Replace("<style=cIsUtility>Agile.</style>",
                          "<style=cIsUtility>Ágil.</style>")
                        .Replace("Swing forward for", "Da un tajo frontal que inflige")
                        .Replace("% damage", "% de daño");

                case "SECONDARY_FORCE_DESCRIPTION":
                    if (!Has(text, "Tap to push enemies for") || !Has(text, "Hold to pull enemies for") ||
                        !Has(text, "Strength depends on the distance")) return null;
                    return text.Replace("Tap to push enemies for", "Pulsa para empujar a los enemigos e infligir")
                        .Replace("Hold to pull enemies for", "Mantén pulsado para atraerlos e infligir")
                        .Replace("% damage", "% de daño")
                        .Replace("Strength depends on the distance between the enemy and yourself.",
                          "La fuerza depende de la distancia entre Vader y el enemigo.")
                        .Replace("Weaker when in Rage Unleashed.",
                          "Es más débil durante Ira desatada.");

                case "UTILITY_DEFLECT_DESCRIPTION":
                    if (!Has(text, "Deflect all attacks for") || !Has(text, "2x the damage")) return null;
                    return text.Replace("Deflect all attacks for", "Desvía todos los ataques durante")
                        .Replace(" seconds", " segundos")
                        .Replace("for <style=cIsDamage>2x the damage, multiplied by luck</style>",
                          "y devuelve <style=cIsDamage>el doble del daño, modificado por la suerte</style>")
                        .Replace("Movespeed is halved and you're unable to attack with your lightsaber while deflecting.",
                          "Tu velocidad de movimiento se reduce a la mitad y no puedes atacar con el sable mientras desvías.");

                case "SPECIAL_RAGE_DESCRIPTION":
                    if (!Has(text, "Unleashes your rage") || !Has(text, "Double your attackspeed")) return null;
                    return text.Replace("Unleashes your rage,", "Desata tu ira,")
                        .Replace("fully healing yourself", "recuperando toda tu salud")
                        .Replace("removing your attackspeed and movespeed limiters",
                          "eliminando los límites de velocidad de ataque y movimiento")
                        .Replace("Double your attackspeed, movespeed and armor as well as remove all cooldowns for",
                          "Duplica la velocidad de ataque, la velocidad de movimiento y la armadura; además, elimina las recargas durante")
                        .Replace(" seconds", " segundos");

                case "KEYWORD_COOLDOWN_SLASH":
                    if (!Has(text, "reduces all cooldowns")) return null;
                    return CooldownHeading(text)
                        .Replace("Hitting enemies", "Golpear enemigos")
                        .Replace("reduces all cooldowns", "reduce todas las recargas")
                        .Replace("by 1 second", "en 1 segundo");

                case "KEYWORD_ENERGY_SLASH":
                    if (!Has(text, "Hitting enemies") || !Has(text, "regens ")) return null;
                    return EnergyHeading(text)
                        .Replace("Hitting enemies", "Golpear enemigos")
                        .Replace("regens ", "regenera ")
                        .Replace(" energy", " de energía")
                        .Replace("scaling with attackspeed", "en función de la velocidad de ataque");

                case "KEYWORD_COOLDOWN_FORCE":
                case "KEYWORD_COOLDOWN_DEFLECT":
                case "KEYWORD_COOLDOWN_RAGE":
                    if (!Has(text, "Has a ") || !Has(text, " second cooldown")) return null;
                    return CooldownHeading(text)
                        .Replace("Has a ", "Tiene una recarga de ")
                        .Replace(" second cooldown", " segundos de duración")
                        .Replace(" segundos de duración.", " segundos.");

                case "KEYWORD_ENERGY_FORCE":
                    if (!Has(text, "Costs ") || !Has(text, " Energy")) return null;
                    return EnergyHeading(text).Replace("Costs ", "Consume ")
                        .Replace(" Energy", " de energía");

                case "KEYWORD_ENERGY_DEFLECT":
                    if (!Has(text, "Holding the button toggles the skill")) return null;
                    return EnergyHeading(text)
                        .Replace("Holding the button toggles the skill on rather than a set duration.",
                          "Mantener pulsado el botón activa o desactiva la habilidad, en lugar de usar una duración fija.")
                        .Replace("Costs 20 Energy per hit blocked.",
                          "Consume 20 de energía por cada golpe bloqueado.");

                case "KEYWORD_ENERGY_RAGE":
                    if (!Has(text, "Requires a full energy bar")) return null;
                    return EnergyHeading(text)
                        .Replace("Requires a full energy bar.", "Requiere la barra de energía completa.")
                        .Replace("When activated, lasts until you run out of energy.",
                          "Al activarse, permanece hasta que se agota la energía.")
                        .Replace("Removes all energy costs.",
                          "Elimina todos los costes de energía.");
            }
            return null;
        }

        private static bool Has(string source, string value)
        {
            return source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string CooldownHeading(string text)
        {
            return text.Replace("[ Cooldown Passive ]", "[ Pasiva de recargas ]");
        }

        private static string EnergyHeading(string text)
        {
            return text.Replace("[ Energy Passive ]", "[ Pasiva de energía ]");
        }
    }
}
