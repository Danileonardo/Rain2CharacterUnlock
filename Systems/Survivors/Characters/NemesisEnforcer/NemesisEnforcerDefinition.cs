using System;

using BepInEx.Bootstrap;
using BepInEx.Logging;
using R2API;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Integración curada de Némesis Defensor / Nemesis Enforcer.
    ///
    /// Enforcer 3.11.9 ya publica localización propia completa para es-419 y
    /// es-ES: identidad, Vista general, pasiva, habilidades, keywords, skins y
    /// achievements. USU conserva esas traducciones del creador y sólo repara
    /// el lore, cuyo contenido oficial del mod es literalmente "heavy tf2" en
    /// inglés y español.
    ///
    /// El desbloqueo original ENFORCER_NEMESIS2UNLOCKABLE_REWARD_ID se conserva
    /// intacto. Su achievement mantiene deliberadamente el nombre "???" y la
    /// condición original: como Defensor, en Monzón o superior, estabilizar los
    /// Campos del Vacío y derrotar al Vestigio de Defensor.
    ///
    /// No referencia Enforcer.dll ni tipos externos del mod.
    /// </summary>
    public sealed class NemesisEnforcerDefinition : SurvivorDefinition
    {
        private const string PluginGuid =
            "com.EnforcerGang.Enforcer";

        private static object NemesisLoreSpanish419Overlay;
        private static object NemesisLoreSpanishEsOverlay;
        private static object NemesisLoreEnglishOverlay;

        private static readonly string NemesisSpanishLore =
            "Registro recuperado de los Campos del Vacío. Integridad del archivo: inestable.\n\n" +
            "La figura tenía la silueta de un Defensor, pero no había escudo. Sólo un martillo dorado, una masa imposible y el ruido de una minigun preparándose al otro lado de la niebla.\n\n" +
            "Las primeras grabaciones muestran al Vestigio atravesando las celdas estabilizadas sin buscar cobertura. Cada impacto abría espacio a su alrededor. Cuando un objetivo se alejaba, el martillo dejaba paso al fuego sostenido. No parecía imitar al Defensor: parecía llevar su obstinación hasta una conclusión monstruosa.\n\n" +
            "Quienes sobrevivieron describieron la misma impresión: enfrentarse a algo familiar y, al mismo tiempo, completamente equivocado. Una sombra nacida de la misma valentía, despojada de toda prudencia.\n\n" +
            "Cuando la última celda quedó estable, la presencia se volvió hacia su original.\n\n" +
            "No hubo transmisión. No hubo advertencia.\n\n" +
            "Sólo quedó una línea en el registro:\n\n" +
            "«El Vestigio ha encontrado a su original.»";

        private static readonly string NemesisEnglishLore =
            "Record recovered from the Void Fields. File integrity: unstable.\n\n" +
            "The figure had the silhouette of an Enforcer, but there was no shield. Only a golden hammer, an impossible mass, and the sound of a minigun spinning up beyond the fog.\n\n" +
            "The earliest recordings show the Vestige moving through the stabilized cells without seeking cover. Every impact opened space around it. When a target moved away, the hammer gave way to sustained fire. It did not seem to imitate the Enforcer; it seemed to carry his stubborn resolve to a monstrous conclusion.\n\n" +
            "Those who survived described the same impression: fighting something familiar and, at the same time, completely wrong. A shadow born from the same courage, stripped of all restraint.\n\n" +
            "When the final cell stabilized, the presence turned toward its original.\n\n" +
            "There was no transmission. No warning.\n\n" +
            "Only one line remained in the record:\n\n" +
            "‘The Vestige has found its original.’";

        public override string SourceIdentifier
        {
            get { return "Enforcer.EnforcerContent"; }
        }

        public override string BodyName
        {
            get { return "NemesisEnforcerBody"; }
        }

        public override string DefinitionName
        {
            get { return "Némesis Defensor"; }
        }

        /// <summary>
        /// El detector actual identifica este survivor con el content pack /
        /// assembly Enforcer.EnforcerContent, mientras BepInEx registra el mod
        /// bajo com.EnforcerGang.Enforcer. Aceptamos ambas procedencias para no
        /// depender de cómo R2API exponga el proveedor en una versión futura.
        /// </summary>
        public override bool Matches(
            SurvivorInfo survivor
        )
        {
            if (
                survivor == null ||
                string.IsNullOrWhiteSpace(survivor.BodyName) ||
                !string.Equals(
                    survivor.BodyName,
                    BodyName,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return false;
            }

            if (
                string.Equals(
                    survivor.ContentPackIdentifier,
                    SourceIdentifier,
                    StringComparison.OrdinalIgnoreCase
                ) ||
                string.Equals(
                    survivor.SourceAssembly,
                    SourceIdentifier,
                    StringComparison.OrdinalIgnoreCase
                ) ||
                string.Equals(
                    survivor.ContentPackIdentifier,
                    PluginGuid,
                    StringComparison.OrdinalIgnoreCase
                ) ||
                string.Equals(
                    survivor.SourceAssembly,
                    PluginGuid,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return true;
            }

            return
                Chainloader.PluginInfos != null &&
                Chainloader.PluginInfos.ContainsKey(
                    PluginGuid
                );
        }

        public override void RegisterInstalledLocalization(
            SurvivorInfo survivor,
            ManualLogSource logger
        )
        {
            if (survivor == null || !Matches(survivor))
            {
                return;
            }

            RegisterLoreOverlays();

            // No se tocan NEMFORCER_NAME, NEMFORCER_DESCRIPTION, skills,
            // keywords, skins ni achievements: el creador ya los publica
            // correctamente en es-419 / es-ES.
            logger?.LogInfo(
                "[LOCALIZATION] Definition activa | NemesisEnforcerBody | " +
                "localización nativa del creador + lore curado USU"
            );

            logger?.LogInfo(
                "[NEMESIS ENFORCER] Unlock original preservado | " +
                "ENFORCER_NEMESIS2UNLOCKABLE_REWARD_ID | Achievement: ??? | " +
                "Campos del Vacío + Vestigio de Defensor"
            );
        }

        public override string ResolveSubtitle(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            return fallback ?? "";
        }

        public override string ResolveDescription(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            // La Vista general del creador ya es completa y está localizada.
            return fallback ?? "";
        }

        public override string ResolveLore(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            if (IsSpanishLanguage())
            {
                return NemesisSpanishLore;
            }

            if (IsEnglishLanguage())
            {
                return NemesisEnglishLore;
            }

            return fallback ?? "";
        }

        public override string ResolveOriginalMissionName(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            // "???" es intencional en el archivo de idioma del creador.
            return fallback ?? "";
        }

        public override string ResolveOriginalMissionDescription(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            // Se conserva la descripción original del creador.
            return fallback ?? "";
        }

        private static void RegisterLoreOverlays()
        {
            // El creador registra NEMFORCER_LORE como "heavy tf2" incluso en
            // sus archivos EN/ES. En este caso aplicamos la excepción de lore
            // curado acordada para contenido inexistente o de placeholder.
            if (NemesisLoreSpanish419Overlay == null)
            {
                NemesisLoreSpanish419Overlay =
                    LanguageAPI.AddOverlay(
                        "NEMFORCER_LORE",
                        NemesisSpanishLore,
                        "es-419"
                    );
            }

            if (NemesisLoreSpanishEsOverlay == null)
            {
                NemesisLoreSpanishEsOverlay =
                    LanguageAPI.AddOverlay(
                        "NEMFORCER_LORE",
                        NemesisSpanishLore,
                        "es-ES"
                    );
            }

            if (NemesisLoreEnglishOverlay == null)
            {
                NemesisLoreEnglishOverlay =
                    LanguageAPI.AddOverlay(
                        "NEMFORCER_LORE",
                        NemesisEnglishLore,
                        "en"
                    );
            }
        }

        private static bool IsSpanishLanguage()
        {
            string language = Language.currentLanguageName ?? "";

            return language.StartsWith(
                "es",
                StringComparison.OrdinalIgnoreCase
            );
        }

        private static bool IsEnglishLanguage()
        {
            string language = Language.currentLanguageName ?? "";

            return
                string.Equals(
                    language,
                    "en",
                    StringComparison.OrdinalIgnoreCase
                ) ||
                language.StartsWith(
                    "en-",
                    StringComparison.OrdinalIgnoreCase
                );
        }
    }
}
