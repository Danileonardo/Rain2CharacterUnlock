using System;
using System.Collections.Generic;
using BepInEx.Logging;
using R2API;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Traducción nativa de KnightMod.dll (com.vibon.TheKnight).
    /// Los sufijos VIBON_KNIGHT_* fueron verificados en la DLL facilitada.
    /// No interviene en el desbloqueo ni modifica objetos originales del mod.
    /// </summary>
    internal static class KnightSkillLocalization
    {
        private const string Prefix = "VIBON_KNIGHT_";
        private static readonly List<object> Overlays = new List<object>();
        private static readonly HashSet<string> Applied =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        internal const string SpanishOverview =
            "El Caballerito es un receptáculo silencioso que combate con su Aguijón y convierte ALMA en hechizos.\n\n" +
            "< ! > Los golpes del Aguijón reúnen ALMA; los hechizos la consumen.\n\n" +
            "< ! > Apunta hacia abajo mientras estás en el aire para rebotar sobre enemigos. Cada rebote recupera tus saltos.\n\n" +
            "< ! > La Capa de ala de polilla no tiene tiempo de reutilización. Desplázate en cualquier dirección.\n\n" +
            "< ! > Concentración convierte ALMA en salud. Busca un momento seguro.";

        internal static bool IsSpanish()
        {
            return (Language.currentLanguageName ?? "").StartsWith("es", StringComparison.OrdinalIgnoreCase);
        }

        internal static void RegisterFor(SurvivorInfo survivor, ManualLogSource logger)
        {
            if (survivor == null || survivor.SurvivorDef == null ||
                survivor.SurvivorDef.bodyPrefab == null ||
                !string.Equals(survivor.BodyName, "KnightBody", StringComparison.OrdinalIgnoreCase))
                return;

            // Estos tokens se crean en KnightMod.dll. No se derivan del nombre
            // visible (que puede estar localizado a otros idiomas).
            Add(Prefix + "SUBTITLE", "Receptáculo del Abismo");
            Add(Prefix + "DESCRIPTION", SpanishOverview);
            Add(Prefix + "PASSIVE_NAME", "Recipiente de ALMA");
            Add(Prefix + "PRIMARY_NAIL_NAME", "Aguijón");
            Add(Prefix + "SECONDARY_SPIRIT_NAME", "Espíritu vengativo");
            Add(Prefix + "SECONDARY_WRAITHS_NAME", "Espectros aulladores");
            Add(Prefix + "SECONDARY_FOCUS_NAME", "Concentración");
            Add(Prefix + "UTILITY_DASH_NAME", "Capa de ala de polilla");
            Add(Prefix + "UTILITY_SHADE_NAME", "Capa sombría");
            Add(Prefix + "SPECIAL_DIVE_NAME", "Salto desolador");
            Add(Prefix + "MASTERY_SKIN_NAME", "Alternativa");

            // No traducimos DEFAULT_SKIN global para no modificar otros mods.
            // Se traduce únicamente si TheKnight crea un token propio.
            AddIfExists(Prefix + "DEFAULT_SKIN", "Predeterminado");

            // Estas son las ocho descripciones de habilidad/pasiva reales.
            // El código conserva los valores interpolados por el creador.
            string[] descriptions =
            {
                "PASSIVE_DESCRIPTION", "PRIMARY_NAIL_DESCRIPTION",
                "SECONDARY_SPIRIT_DESCRIPTION", "SECONDARY_WRAITHS_DESCRIPTION",
                "SECONDARY_FOCUS_DESCRIPTION", "UTILITY_DASH_DESCRIPTION",
                "UTILITY_SHADE_DESCRIPTION", "SPECIAL_DIVE_DESCRIPTION"
            };
            int translated = 0;
            for (int i = 0; i < descriptions.Length; ++i)
            {
                string suffix = descriptions[i];
                string token = Prefix + suffix;
                string original = Language.GetString(token) ?? "";
                string spanish = TranslateVerifiedDescription(suffix, original);
                if (!string.IsNullOrEmpty(spanish))
                {
                    Add(token, spanish);
                    translated++;
                }
                else
                {
                    UsuLog.Verbose(logger, "[KNIGHT I18N] Texto dinámico no disponible; se conserva el original: " + token);
                }
            }

            UsuLog.Verbose(logger,
                "[KNIGHT I18N] tokens nativos verificados; descripciones=" + translated +
                "/" + descriptions.Length + " | skins: alternativa + predeterminada (si es exclusiva)");
        }

        private static string TranslateVerifiedDescription(string suffix, string source)
        {
            if (string.IsNullOrWhiteSpace(source) || source == Prefix + suffix)
                return null;

            // Solo se transforma la plantilla inglesa conocida de la DLL.
            // Sustituimos texto, nunca números, tags o porcentajes interpolados.
            switch (suffix)
            {
                case "PASSIVE_DESCRIPTION":
                    if (!Contains(source, "Nail hits gather") || !Contains(source, "Monarch Wings")) return null;
                    return source
                        .Replace("Nail hits gather ", "Los golpes del Aguijón reúnen ")
                        .Replace(" (max ", " (máximo ")
                        .Replace("). Spells spend it.", "). Los hechizos la consumen.")
                        .Replace("Monarch Wings", "Alas de monarca")
                        .Replace(": you can jump twice.", ": puedes saltar dos veces.");

                case "PRIMARY_NAIL_DESCRIPTION":
                    if (!Contains(source, "Slash for ") || !Contains(source, "Aim up to slash upward")) return null;
                    return source
                        .Replace("Slash for ", "Ataca e inflige ")
                        .Replace(" damage</style>.", " de daño</style>.")
                        .Replace("Aim up to slash upward; aim down in the air to ",
                            "Apunta hacia arriba para atacar hacia arriba; apunta hacia abajo en el aire para ")
                        .Replace("pogo", "rebotar")
                        .Replace(" off enemies.", " sobre los enemigos.");

                case "SECONDARY_SPIRIT_DESCRIPTION":
                    if (!Contains(source, "Conjure a spirit") || !Contains(source, "piercing")) return null;
                    return TranslateCost(source)
                        .Replace("Conjure a spirit that flies forward, ", "Invoca un espíritu que avanza y ")
                        .Replace("piercing", "atraviesa")
                        .Replace(" enemies for ", " a los enemigos e inflige ")
                        .Replace(" damage</style>.", " de daño</style>.");

                case "SECONDARY_WRAITHS_DESCRIPTION":
                    if (!Contains(source, "Scream, unleashing wraiths") || !Contains(source, " times</style>")) return null;
                    return TranslateCost(source)
                        .Replace("Scream, unleashing wraiths above you that strike ",
                            "Grita y libera espectros sobre ti que golpean ")
                        .Replace(" times</style> for ", " veces</style> e infligen ")
                        .Replace(" damage</style>.", " de daño</style>.");

                case "SECONDARY_FOCUS_DESCRIPTION":
                    if (!Contains(source, "Hold to focus SOUL") || !Contains(source, "You cannot move while focusing")) return null;
                    return source
                        .Replace("Hold to focus SOUL: every ", "Mantén pulsado para concentrar ALMA: cada ")
                        .Replace("s spends ", " s consume ")
                        .Replace(" SOUL</style> to ", " de ALMA</style> para ")
                        .Replace("heal ", "curar ")
                        .Replace(" max health", " de salud máxima")
                        .Replace("You cannot move while focusing.", "No puedes moverte mientras te concentras.");

                case "UTILITY_DASH_DESCRIPTION":
                    if (!Contains(source, "No cooldown.") || !Contains(source, "In the air, dash")) return null;
                    return source
                        .Replace("No cooldown.", "Sin tiempo de reutilización.")
                        .Replace("Dash in any direction.", "Desplázate en cualquier dirección.")
                        .Replace("In the air, dash where you aim.", "En el aire, desplázate hacia donde apuntes.");

                case "UTILITY_SHADE_DESCRIPTION":
                    if (!Contains(source, "as a shadow") || !Contains(source, "passing through enemies")) return null;
                    return source
                        .Replace("Dash in any direction as a shadow:", "Desplázate en cualquier dirección como una sombra:")
                        .Replace("invulnerable", "invulnerable")
                        .Replace("passing through enemies", "atravesando enemigos")
                        .Replace(" and ", " y ");

                case "SPECIAL_DIVE_DESCRIPTION":
                    if (!Contains(source, "Plunge straight down") || !Contains(source, "The impact deals")) return null;
                    return TranslateCost(source)
                        .Replace("Plunge straight down, ", "Desciende en picado, ")
                        .Replace("The impact deals ", "El impacto inflige ")
                        .Replace(" damage</style> around you.", " de daño</style> a tu alrededor.");
            }
            return null;
        }

        private static string TranslateCost(string value)
        {
            return value
                .Replace("Costs ", "Cuesta ")
                .Replace(" SOUL</style>.", " de ALMA</style>.");
        }

        private static bool Contains(string source, string part)
        {
            return source.IndexOf(part, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void AddIfExists(string token, string value)
        {
            if (!string.Equals(Language.GetString(token), token, StringComparison.OrdinalIgnoreCase))
                Add(token, value);
        }

        private static void Add(string token, string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            foreach (string locale in new[] { "es-419", "es-ES" })
            {
                if (!Applied.Add(locale + ":" + token)) continue;
                Overlays.Add(LanguageAPI.AddOverlay(token, value, locale));
            }
        }
    }
}
