using System;

using BepInEx.Logging;
using R2API;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Integración curada de Auriel.
    ///
    /// Autoridad USU para:
    /// - localización visible es-419 / es-ES;
    /// - nombres y descripciones de skills con token nativo;
    /// - skins y achievements visibles;
    /// - misión de desbloqueo Original;
    /// - lore / Logbook cuando el mod creador no aporta uno.
    ///
    /// No referencia Auriel.dll. Todo se aplica mediante identificadores y
    /// tokens públicos una vez que el Registry confirma que el survivor está
    /// realmente instalado.
    /// </summary>
    public sealed class AurielDefinition : SurvivorDefinition
    {
        public override string SourceIdentifier
        {
            get
            {
                return "com.Dragonyck.Auriel";
            }
        }

        public override string BodyName
        {
            get
            {
                return "AurielBody";
            }
        }

        public override string DefinitionName
        {
            get
            {
                return "Auriel";
            }
        }


        private static object AurielEnglishDescriptionOverlay;


        private static readonly string AurielSpanishDescription =
            "Su luz eterna ilumina incluso a las almas más oscuras. En su constante búsqueda " +
            "de armonía, es mediadora, consejera y, cuando hace falta, una guerrera " +
            "temeraria.<color=#CCD3E0>\r\n\r\n" +
            "< ! > Vuelo angélico permite a Auriel volar a voluntad; aprovecha la altura para " +
            "reposicionarte y mantener mejores líneas de ataque.\r\n\r\n" +
            "< ! > Luz abrasadora genera Energía. Úsala para habilitar las habilidades de Energía " +
            "cuyo coste coincida con la Energía disponible.\r\n\r\n" +
            "< ! > Flagelo sagrado atrae y arrastra a los enemigos cercanos, facilitando agruparlos " +
            "antes de atacar.\r\n\r\n" +
            "< ! > Soplo de esperanza recupera un 5% de salud y otorga un 25% de velocidad de " +
            "ataque y movimiento durante 4 s.";


        private static readonly string AurielEnglishDescription =
            "Her eternal light illuminates even the darkest souls. Seeking harmony in all things, " +
            "she is a mediator, a counselor, and when the need arises, a fearless warrior." +
            "<color=#CCD3E0>\r\n\r\n" +
            "< ! > Angelic Flight allows Auriel to fly at will; use the height advantage to " +
            "reposition and maintain better lines of attack.\r\n\r\n" +
            "< ! > Searing Light generates Energy. Use it to enable the Energy Skills whose cost " +
            "matches the Energy currently available.\r\n\r\n" +
            "< ! > Sacred Sweep pulls and drags nearby enemies, making it easier to group them " +
            "before attacking.\r\n\r\n" +
            "< ! > Bestow Hope restores 5% health and grants 25% attack speed and movement speed " +
            "for 4 seconds.";


        private static readonly string AurielSpanishLore =
            "Entrada recuperada de un archivo celestial. Clasificación de origen: Cielos Superiores.\n\n" +
            "Entre las voces del Concilio Angiris, Auriel nunca fue la más estridente. No " +
            "necesitaba serlo. Allí donde otros hablaban de valor, destino o justicia, ella " +
            "sostenía una idea más frágil y, por ello, más difícil de destruir: la esperanza.\n\n" +
            "Los relatos del Conflicto Eterno la describen como mediadora antes que conquistadora, " +
            "aunque ningún demonio que confundiera compasión con debilidad sobrevivía mucho tiempo " +
            "a ese error. Al'maiesh, la Cinta de la Esperanza, podía unir voluntades con la misma " +
            "facilidad con la que castigaba a quienes sembraban desesperación.\n\n" +
            "Cuando los Cielos Superiores quedaron al borde del colapso, incluso la luz de Auriel pudo " +
            "ser encadenada. Sin embargo, mientras quedara alguien dispuesto a levantarse una vez " +
            "más, su virtud no podía extinguirse por completo.\n\n" +
            "El registro termina con una observación añadida por una mano desconocida:\n\n" +
            "«No importa cuán oscuro sea este mundo. Si ella está aquí, todavía existe una razón " +
            "para mirar hacia la luz.»";


        private static readonly string AurielEnglishLore =
            "Entry recovered from a celestial archive. Origin classification: High Heavens.\n\n" +
            "Among the voices of the Angiris Council, Auriel was never the loudest. She did not " +
            "need to be. Where others spoke of valor, fate, or justice, she upheld something more " +
            "fragile and therefore harder to destroy: hope.\n\n" +
            "Accounts of the Eternal Conflict describe her as a mediator before a conqueror, yet " +
            "no demon that mistook compassion for weakness survived that mistake for long. " +
            "Al'maiesh, the Cord of Hope, could bind wills together as readily as it punished those " +
            "who spread despair.\n\n" +
            "When the High Heavens stood on the edge of collapse, even Auriel's light could be " +
            "chained. Still, as long as someone remained willing to rise once more, the virtue she " +
            "embodied could never be extinguished completely.\n\n" +
            "The record ends with an observation added by an unknown hand:\n\n" +
            "\"No matter how dark this world becomes. If she is here, there is still a reason to " +
            "look toward the light.\"";


        private static readonly string AurielSpanishPassiveDescription =
            "Auriel puede <style=cIsUtility>volar</style> a voluntad. Luz abrasadora genera " +
            "<style=cIsDamage>Energía</style>, que puede usarse para lanzar " +
            "<style=cIsDamage>habilidades de Energía</style>. Pulsa Alt izquierdo o izquierda " +
            "en la cruceta para habilitar las <style=cIsDamage>habilidades de Energía</style> " +
            "cuyo coste coincida con la <style=cIsDamage>Energía</style> disponible.";


        public override void RegisterInstalledLocalization(
            SurvivorInfo survivor,
            ManualLogSource logger
        )
        {
            if (
                survivor == null ||
                !Matches(survivor)
            )
            {
                return;
            }

            // El lore inglés continúa siendo fallback porque el creador no
            // publica uno. La Vista general ampliada es una capa curada de USU
            // que conserva íntegro el primer párrafo original de Auriel.
            RegisterEnglishLoreFallback();
            RegisterEnglishDescriptionOverlay();

            RegisterAurielSpanishTokens();
            RegisterAurielPassiveSpanish(
                survivor,
                logger
            );

            logger?.LogInfo(
                "[LOCALIZATION] Definition activa | " +
                "AurielBody | es-419 / es-ES + pasiva + vista general USU + lore"
            );
        }


        public override string ResolveSubtitle(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            return IsSpanishLanguage()
                ? "Arcángel de la Esperanza"
                : fallback ?? "";
        }


        public override string ResolveDescription(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            if (IsSpanishLanguage())
            {
                return AurielSpanishDescription;
            }

            if (IsEnglishLanguage())
            {
                return AurielEnglishDescription;
            }

            return fallback ?? "";
        }


        public override string ResolveLore(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            if (IsSpanishLanguage())
            {
                return AurielSpanishLore;
            }

            if (
                IsEnglishLanguage() &&
                IsMissingLoreText(fallback)
            )
            {
                return AurielEnglishLore;
            }

            return fallback ?? "";
        }


        public override string ResolveOriginalMissionName(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            return IsSpanishLanguage()
                ? "¿Una bendición o sólo suerte?"
                : fallback ?? "";
        }


        public override string ResolveOriginalMissionDescription(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            return IsSpanishLanguage()
                ? "Comprende la dualidad de la intervención divina."
                : fallback ?? "";
        }


        private static void RegisterEnglishDescriptionOverlay()
        {
            if (AurielEnglishDescriptionOverlay != null)
            {
                return;
            }

            // Auriel 1.7.2 sí publica una descripción inglesa corta. En vez de
            // modificar Auriel.dll, USU superpone en runtime una Vista general
            // ampliada que conserva ese párrafo y añade consejos equivalentes
            // a los registrados en español.
            AurielEnglishDescriptionOverlay =
                LanguageAPI.AddOverlay(
                    "AURIEL_DESCRIPTION",
                    AurielEnglishDescription,
                    "en"
                );
        }


        private static void RegisterEnglishLoreFallback()
        {
            // Auriel 1.7.2 no aporta lore. Si una versión futura del creador
            // registra AURIEL_LORE antes de USU, LanguageAPI conserva esa
            // entrada y este texto queda únicamente como fallback.
            LanguageAPI.Add(
                "AURIEL_LORE",
                AurielEnglishLore,
                "en"
            );
        }


        private static void RegisterAurielSpanishTokens()
        {
            // Survivor / Logbook
            RegisterSpanishToken(
                "AURIEL_SUBTITLE",
                "Arcángel de la Esperanza"
            );

            RegisterSpanishToken(
                "AURIEL_DESCRIPTION",
                AurielSpanishDescription
            );

            RegisterSpanishToken(
                "AURIEL_LORE",
                AurielSpanishLore
            );

            // Pasiva. "Vuelo angélico" es el nombre oficial usado por
            // Blizzard en español latino. La descripción se registra más
            // abajo usando el token REAL de SkillLocator.passiveSkill.
            RegisterSpanishToken(
                "AURIEL_PASSIVE_NAME",
                "Vuelo angélico"
            );

            // Primaria
            RegisterSpanishToken(
                "AURIEL_SEARINGLIGHT",
                "Luz abrasadora"
            );

            RegisterSpanishToken(
                "AURIEL_SEARINGLIGHT_DESCRIPTION",
                "Auriel dispara una luz angelical abrasadora que <style=cIsDamage>atraviesa</style> " +
                "a todos los enemigos e inflige <style=cIsDamage>260%</style> de daño. Mientras " +
                "está bajo los efectos de Soplo de esperanza, Luz abrasadora viaja más lejos y " +
                "más rápido."
            );

            // Secundaria
            RegisterSpanishToken(
                "AURIEL_SACREDSWEEP",
                "Flagelo sagrado"
            );

            // El token del mod contiene el typo ACREDSWEEP; se conserva tal
            // cual porque es el identificador real observado en runtime.
            RegisterSpanishToken(
                "AURIEL_ACREDSWEEP_DESCRIPTION",
                "Auriel barre el área con poder sagrado, <style=cIsUtility>atrayendo</style> a " +
                "todos los enemigos cercanos hacia el centro del área y " +
                "<style=cIsUtility>arrastrándolos</style> a lo largo de su trayectoria. " +
                "[<style=cIsDamage>Haz divino</style>]"
            );

            // Utilidad
            RegisterSpanishToken(
                "AURIEL_BESTOWHOPE",
                "Soplo de esperanza"
            );

            RegisterSpanishToken(
                "AURIEL_BESTOWHOPE_DESCRIPTION",
                "Auriel <style=cIsHealing>recupera un 5%</style> de su <style=cIsHealth>salud</style> " +
                "y obtiene <style=cIsDamage>25% de velocidad de ataque</style> y " +
                "<style=cIsUtility>25% de velocidad de movimiento</style> durante " +
                "<style=cIsUtility>4 s</style>. [<style=cIsDamage>Rayo del cielo</style>]"
            );

            // Especial
            RegisterSpanishToken(
                "AURIEL_WRATHOFHEAVEN",
                "Ira del cielo"
            );

            RegisterSpanishToken(
                "AURIEL_WRATHOFHEAVEN_DESCRIPTION",
                "Auriel canaliza un rayo celestial que le otorga " +
                "<style=cIsDamage>protección divina</style> e inflige <style=cIsDamage>400%</style> " +
                "de daño por impacto a los enemigos cercanos. Al terminar la canalización, Auriel " +
                "libera a su alrededor una explosión de luz abrasadora que inflige " +
                "<style=cIsDamage>600%</style> de daño. [<style=cIsDamage>Resurrección</style>]"
            );

            // Skins
            RegisterSpanishToken(
                "AURIELBODY_DEFAULT_SKIN_NAME",
                "Predeterminado"
            );

            RegisterSpanishToken(
                "AURIELBODY_ARCHANGEL_SKIN_NAME",
                "Arcángel"
            );

            RegisterSpanishToken(
                "AURIELBODY_SAKURA_SKIN_NAME",
                "Sakura"
            );

            RegisterSpanishToken(
                "AURIELBODY_SPIRIT_SKIN_NAME",
                "Sanadora espiritual"
            );

            RegisterSpanishToken(
                "AURIELBODY_DEMONIC_SKIN_NAME",
                "Demoníaca"
            );

            // Unlock original del survivor
            RegisterSpanishToken(
                "AURIEL_UNLOCKABLE_ACHIEVEMENT_NAME",
                "¿Una bendición o sólo suerte?"
            );

            RegisterSpanishToken(
                "AURIEL_UNLOCKABLE_ACHIEVEMENT_DESC",
                "Comprende la dualidad de la intervención divina."
            );

            // Achievements de skins
            RegisterSpanishToken(
                "AURIEL_MASTERY_ACHIEVEMENT_NAME",
                "Auriel: Maestría"
            );

            RegisterSpanishToken(
                "AURIEL_MASTERY_ACHIEVEMENT_DESC",
                "Como Auriel, completa el juego o oblítérate en Monzón."
            );

            RegisterSpanishToken(
                "AURIEL_SAKURA_ACHIEVEMENT_NAME",
                "Paraíso de Mercurio"
            );

            RegisterSpanishToken(
                "AURIEL_SAKURA_ACHIEVEMENT_DESC",
                "Como Auriel, visita el jardín."
            );

            RegisterSpanishToken(
                "AURIEL_SPIRIT_ACHIEVEMENT_NAME",
                "Evaluación del alma"
            );

            RegisterSpanishToken(
                "AURIEL_SPIRIT_ACHIEVEMENT_DESC",
                "Como Auriel, vuelve a la vida gracias al poder de Dio."
            );

            RegisterSpanishToken(
                "AURIEL_DEMONIC_ACHIEVEMENT_NAME",
                "Perversa"
            );

            RegisterSpanishToken(
                "AURIEL_DEMONIC_ACHIEVEMENT_DESC",
                "Como Auriel, comete un acto de herejía."
            );
        }


        private static void RegisterAurielPassiveSpanish(
            SurvivorInfo survivor,
            ManualLogSource logger
        )
        {
            string nameToken;
            string descriptionToken;

            bool found =
                TryGetPassiveLocalizationTokens(
                    survivor,
                    out nameToken,
                    out descriptionToken
                );

            if (!found)
            {
                logger?.LogWarning(
                    "[LOCALIZATION] Pasiva no resuelta | " +
                    "AurielBody | SkillLocator.passiveSkill no disponible"
                );

                return;
            }

            if (!string.IsNullOrWhiteSpace(nameToken))
            {
                RegisterSpanishToken(
                    nameToken,
                    "Vuelo angélico"
                );
            }

            if (!string.IsNullOrWhiteSpace(descriptionToken))
            {
                RegisterSpanishToken(
                    descriptionToken,
                    AurielSpanishPassiveDescription
                );
            }

            logger?.LogInfo(
                "[LOCALIZATION] Pasiva localizada | " +
                "AurielBody | NameToken: " +
                (nameToken ?? "") +
                " | DescriptionToken: " +
                (descriptionToken ?? "")
            );
        }


        private static void RegisterSpanishToken(
            string token,
            string spanish
        )
        {
            if (
                string.IsNullOrWhiteSpace(token) ||
                string.IsNullOrWhiteSpace(spanish)
            )
            {
                return;
            }

            // Los mods creadores registran idiomas antes de onLoad. Si una
            // versión futura de Auriel aporta español propio, su entrada
            // permanece como autoridad y USU sólo actúa como fallback.
            LanguageAPI.Add(
                token,
                spanish,
                "es-419"
            );

            LanguageAPI.Add(
                token,
                spanish,
                "es-ES"
            );
        }


        private static bool IsSpanishLanguage()
        {
            string language =
                Language.currentLanguageName ?? "";

            return
                language.StartsWith(
                    "es",
                    StringComparison.OrdinalIgnoreCase
                );
        }


        private static bool IsEnglishLanguage()
        {
            string language =
                Language.currentLanguageName ?? "";

            return
                language.StartsWith(
                    "en",
                    StringComparison.OrdinalIgnoreCase
                );
        }


        private static bool IsMissingLoreText(
            string text
        )
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return true;
            }

            string trimmed =
                text.Trim();

            return
                trimmed.IndexOf(' ') < 0 &&
                trimmed.IndexOf('_') >= 0 &&
                string.Equals(
                    trimmed,
                    trimmed.ToUpperInvariant(),
                    StringComparison.Ordinal
                );
        }
    }
}
