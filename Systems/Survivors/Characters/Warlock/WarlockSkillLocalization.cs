using System;
using System.Text.RegularExpressions;
using R2API;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Warlock: traducciones verificadas a partir de WarlockMod.dll (29/10/2026).
    /// Solo tokens KENKO_WARLOCK_*; no modifica SkillDef, ni prefabs, ni condiciones.
    /// Los textos dinámicos se traducen al resolverse, conservando el valor
    /// numérico que publica el mod en la versión/configuración actual.
    /// </summary>
    internal static class WarlockSkillLocalization
    {
        private static void Add(string token, string value)
        {
            LanguageAPI.Add(token, value, "es-419");
            LanguageAPI.Add(token, value, "es-ES");
        }

        internal static void Register()
        {
            // Variantes de la rueda de potenciación de Maná carmesí.
            Add("KENKO_WARLOCK_PRIMARY_EMPOWER1_NAME", "Potenciar Oleada sobrenatural");
            Add("KENKO_WARLOCK_PRIMARY_EMPOWER1_DESCRIPTION",
                "Gasta <color=#981e33>1 de Maná carmesí</color> para preparar " +
                "<style=cIsUtility>1 activación potenciada de Oleada sobrenatural</style>. " +
                "Todos sus disparos <style=cIsUtility>debilitan y repelen a los enemigos</style>.");

            Add("KENKO_WARLOCK_SECONDARY_EMPOWER_NAME", "Potenciar Maleficio");
            Add("KENKO_WARLOCK_SECONDARY_EMPOWER_DESCRIPTION",
                "Gasta <color=#981e33>1 de Maná carmesí</color> para " +
                "<style=cIsUtility>restaurar todas las cargas de Maleficio</style> y preparar " +
                "activaciones potenciadas equivalentes a la mitad de sus cargas máximas, " +
                "redondeada hacia abajo. El Maleficio potenciado convierte el daño adicional " +
                "en una <style=cIsDamage>explosión alrededor del enemigo maldito</style>.");

            Add("KENKO_WARLOCK_UTILITY_EMPOWER_NAME", "Potenciar Desplazamiento sangriento");
            // La descripción de esta variante incluye un porcentaje configurable.
            // Se traduce en TryLocalizeRuntime, no se fija a 200%.

            Add("KENKO_WARLOCK_SPECIAL_EMPOWER_NAME", "Obtener Metamagia");
            Add("KENKO_WARLOCK_SPECIAL_EMPOWER_DESCRIPTION",
                "Gasta <color=#981e33>1 de Maná carmesí</color> para obtener " +
                "<color=#b97f8a>1 acumulación de Metamagia</color>.");

            // Variante de Ritual otorgada por Ancient Scepter.
            Add("KENKO_WARLOCK_SPECIAL_SCEPTER_RITUAL_NAME", "Rito");
            Add("KENKO_WARLOCK_SPECIAL_SCEPTER_RITUAL_DESCRIPTION",
                "Abre un menú para recuperar las cargas de una habilidad elegida y " +
                "potenciarla. Vuelve a activar Ritual para obtener " +
                "<color=#b97f8a>1 acumulación de Metamagia</color> a cambio de " +
                "<color=#981e33>1 de Maná carmesí</color>.");

            // Nombres declarados por el propio mod, no por TheEnd.
            Add("KENKO_WARLOCK_MASTERY_SKIN_NAME", "Alternativa");
            Add("KENKO_WARLOCK_DEFAULT_SKIN", "Predeterminada");
        }

        /// <summary>
        /// Solo actúa sobre un token nativo específico o sobre el texto inglés
        /// literal de la palabra clave propia de Warlock. Evita fijar una
        /// estadística configurable e ignora el resto del catálogo.
        /// </summary>
        internal static bool TryLocalizeRuntime(string token, string original, out string localized)
        {
            localized = null;
            if (string.Equals(token, "KENKO_WARLOCK_UTILITY_EMPOWER_DESCRIPTION",
                    StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(original) ||
                    original.IndexOf("Crimson Mana", StringComparison.OrdinalIgnoreCase) < 0 ||
                    original.IndexOf("stunning damage", StringComparison.OrdinalIgnoreCase) < 0)
                    return false;
                Match match = Regex.Match(original,
                    @"(?<amount>\d+(?:[.,]\d+)?)\s*%", RegexOptions.CultureInvariant);
                if (!match.Success) return false;
                string amount = match.Groups["amount"].Value;
                localized = "Gasta <color=#981e33>1 de Maná carmesí</color> para " +
                    "<style=cIsUtility>restaurar todas las cargas de Desplazamiento sangriento</style> " +
                    "y preparar <style=cIsUtility>1 activación potenciada</style>. " +
                    "Esa activación inflige <style=cIsDamage>" + amount +
                    "% de daño con aturdimiento tanto al salir como al llegar</style>.";
                return true;
            }

            // El panel de palabra clave aparece como texto compuesto por Warlock;
            // puede no disponer de un token KENKO_* independiente.
            string source = string.IsNullOrEmpty(original) ? token : original;
            if (string.IsNullOrEmpty(source) ||
                source.IndexOf("Crimson Mana Empowerment", StringComparison.OrdinalIgnoreCase) < 0 ||
                source.IndexOf("Empowered casts stack until used", StringComparison.OrdinalIgnoreCase) < 0)
                return false;

            localized = "<style=cKeywordName>Potenciación de Maná carmesí</style>" +
                "<style=cSub>Las activaciones potenciadas se acumulan hasta usarse.\n" +
                "Principal: 1 activación.\n" +
                "Secundaria: la mitad de las cargas máximas.\n" +
                "Utilidad: 1 activación.</style>";
            return true;
        }
    }
}
