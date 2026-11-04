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


        private static readonly string AurielSpanishDescription = AurielLocalization.AurielSpanishDescription;


        private static readonly string AurielEnglishDescription = AurielLocalization.AurielEnglishDescription;


        private static readonly string AurielSpanishLore = LoreDialogueLineFormatter.Format(AurielLore.AurielSpanishLore);


        private static readonly string AurielEnglishLore = LoreDialogueLineFormatter.Format(AurielLore.AurielEnglishLore);


        private static readonly string AurielSpanishPassiveDescription = AurielLocalization.AurielSpanishPassiveDescription;


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

            UsuLog.Verbose(logger, 
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
                "[<style=cIsDamage>Rayo divino</style>]"
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

            // Habilidades potenciadas nativas: tokens observados en Auriel.dll.
            // Se conservan los costes, estilos y magnitudes originales.
            // Secundaria potenciada (25 de energía)
            RegisterSpanishToken(
                "AURIEL_SECONDARYEMP",
                "<style=cIsDamage>Rayo divino</style> [<style=cIsDamage>25</style>]"
            );

            RegisterSpanishToken(
                "AURIEL_SECONDARYEMP_DESCRIPTION",
                "Auriel concentra poder divino y <style=cIsDamage>atraviesa</style> a sus " +
                "enemigos con un potente rayo de luz que inflige " +
                "<style=cIsDamage>100%</style> <style=cIsDamage>+Energía</style> " +
                "de daño por impacto y <style=cIsDamage>quema</style> su carne."
            );

            // Utilidad potenciada (50 de energía)
            RegisterSpanishToken(
                "AURIEL_UTILITYEMP",
                "<style=cIsDamage>Rayo del cielo</style> [<style=cIsDamage>50</style>]"
            );

            RegisterSpanishToken(
                "AURIEL_UTILITYEMP_DESCRIPTION",
                "Auriel bendice el suelo con poder divino y " +
                "<style=cIsHealing>cura</style> a sus aliados y a sí misma un " +
                "<style=cIsHealing>5%</style> de su salud máxima " +
                "<style=cIsDamage>+Energía</style> por segundo."
            );

            // Especial potenciada (100 de energía)
            RegisterSpanishToken(
                "AURIEL_SPECIALEMP",
                "<style=cIsDamage>Resurrección</style> [<style=cIsDamage>100</style>]"
            );

            RegisterSpanishToken(
                "AURIEL_SPECIALEMP_DESCRIPTION",
                "Auriel canaliza el espíritu de un aliado caído. Tras " +
                "<style=cIsUtility>2 s</style> de canalización, lo devuelve a la " +
                "<style=cIsHealing>vida</style>. Si no hay ningún aliado que " +
                "resucitar, Auriel <style=cIsHealing>recupera</style> toda su " +
                "<style=cIsHealth>salud</style>."
            );

            // Token nativo de la pasiva: fallback aunque la búsqueda dinámica
            // de SkillLocator.passiveSkill no esté disponible durante el registro.
            RegisterSpanishToken(
                "AURIEL_PASSIVE_DESCRIPTION",
                AurielSpanishPassiveDescription
            );

            // Frases de cierre propias de Auriel.dll; no alteran su misión.
            RegisterSpanishToken(
                "AURIEL_OUTRO",
                "...y así se marchó, mientras el poder de la esperanza fluye " +
                "por el tejido de la creación."
            );

            RegisterSpanishToken(
                "AURIEL_FAIL",
                "...y así desapareció; ante la adversidad, la luz de la " +
                "esperanza brilla con más fuerza."
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

            UsuLog.Verbose(logger, 
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
