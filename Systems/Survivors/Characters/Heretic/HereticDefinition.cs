using System;

using BepInEx.Bootstrap;
using BepInEx.Logging;
using R2API;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Integración curada de Moffein-Heretic 1.2.10.
    ///
    /// Caso especial: este mod NO crea un SurvivorDef modded. Reutiliza el
    /// Heretic.asset de RoR2, lo vuelve seleccionable y le asigna su propio
    /// unlock. Por eso HereticBody continúa siendo clasificado por el detector
    /// como contenido oficial aunque com.Moffein.Heretic esté instalado.
    ///
    /// Esta Definition se activa únicamente cuando el plugin de Moffein está
    /// cargado y existe HereticBody. No cambia la clasificación del survivor,
    /// no lo agrega a Survivors.json y no sustituye su unlock original.
    ///
    /// La identidad, lore y nombres de las cuatro habilidades siguen usando la
    /// localización oficial de Risk of Rain 2. USU cubre los tokens nuevos o
    /// reemplazados por Moffein y aporta una Vista general ampliada, porque la
    /// descripción vanilla de Hereje es demasiado breve para la ficha de USU.
    /// </summary>
    public sealed class HereticDefinition : SurvivorDefinition
    {
        private static object HereticSpanish419DescriptionOverlay;
        private static object HereticSpanishEsDescriptionOverlay;
        private static object HereticEnglishDescriptionOverlay;

        private static readonly string HereticSpanishDescription =
            "La Hereje es una superviviente secreta y extremadamente poderosa, pero su regeneración negativa convierte cada segundo en una carrera contra el tiempo.<color=#CCD3E0>\n\n" +
            "< ! > Su pasiva aumenta su movilidad y le permite aletear dos veces en el aire; úsalo para reposicionarte y evitar daño.\n\n" +
            "< ! > Ganchos de Herejía mantiene a los enemigos dentro de una esfera de cuchillas y termina en una explosión que los enraíza.\n\n" +
            "< ! > Esencia de Herejía acumula Ruina al infligir daño. Actívala cuando hayas acumulado suficientes marcas para detonar todas a la vez.\n\n" +
            "< ! > Al reunir las cuatro piezas de Herejía, Marca de Herejía aumenta la salud un 400% y el daño un 50%, pero fija la regeneración base en -6 PS/s.\n\n" +
            "< ! > La Hereje depende de mantener el ritmo ofensivo: aprovecha su gran movilidad y daño para compensar el deterioro constante de su salud.";

        private static readonly string HereticEnglishDescription =
            "The Heretic is a secret and extremely powerful survivor, but her negative health regeneration turns every second into a race against time.<color=#CCD3E0>\n\n" +
            "< ! > Her passive increases her mobility and lets her flap twice in midair; use it to reposition and avoid damage.\n\n" +
            "< ! > Hooks of Heresy keeps enemies inside a sphere of blades and ends in an explosion that roots them.\n\n" +
            "< ! > Essence of Heresy builds Ruin when dealing damage. Activate it after building enough stacks to detonate them all at once.\n\n" +
            "< ! > After collecting all four Heresy pieces, Mark of Heresy increases health by 400% and damage by 50%, but sets base health regeneration to -6 HP/s.\n\n" +
            "< ! > The Heretic depends on maintaining offensive momentum: use her high mobility and damage to offset her constant health decay.";

        public override string SourceIdentifier
        {
            get { return "com.Moffein.Heretic"; }
        }

        public override string BodyName
        {
            get { return "HereticBody"; }
        }

        public override string DefinitionName
        {
            get { return "Hereje / Moffein-Heretic"; }
        }

        /// <summary>
        /// Moffein-Heretic modifica el SurvivorDef vanilla de Hereje, por lo
        /// que no podemos exigir survivor.IsModded ni ContentPackIdentifier.
        /// El GUID del plugin es la prueba de procedencia en este caso.
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

            return
                Chainloader.PluginInfos != null &&
                Chainloader.PluginInfos.ContainsKey(
                    SourceIdentifier
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

            RegisterOverviewOverlays();
            RegisterHereticSpanishTokens();

            logger?.LogInfo(
                "[LOCALIZATION] Definition activa | HereticBody | " +
                "adaptador Moffein-Heretic + vista general USU + es-419 / es-ES para pasiva + " +
                "Marca de Herejía + skills corregidas + unlock original"
            );

            logger?.LogInfo(
                "[HERETIC LOCALIZATION] HereticBody es SurvivorDef vanilla reutilizado por " +
                "com.Moffein.Heretic; identidad/lore/skills base permanecen oficiales. " +
                "Vista general ampliada por USU. Unlock original Survivors.MoffeinHeretic preservado."
            );
        }

        public override string ResolveDescription(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            if (IsSpanishLanguage())
            {
                return HereticSpanishDescription;
            }

            if (IsEnglishLanguage())
            {
                return HereticEnglishDescription;
            }

            return fallback ?? "";
        }

        public override string ResolveOriginalMissionName(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            return IsSpanishLanguage()
                ? "Resurrección poco ortodoxa"
                : fallback ?? "";
        }

        public override string ResolveOriginalMissionDescription(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            return IsSpanishLanguage()
                ? "Escapa de la Luna como la superviviente secreta."
                : fallback ?? "";
        }

        private static void RegisterOverviewOverlays()
        {
            // HERETIC_DESCRIPTION existe en el juego base, pero sólo contiene
            // una frase muy breve. Al superponer el token también corregimos
            // Character Select, mientras ResolveDescription cubre la ficha USU.
            if (HereticSpanish419DescriptionOverlay == null)
            {
                HereticSpanish419DescriptionOverlay =
                    LanguageAPI.AddOverlay(
                        "HERETIC_DESCRIPTION",
                        HereticSpanishDescription,
                        "es-419"
                    );
            }

            if (HereticSpanishEsDescriptionOverlay == null)
            {
                HereticSpanishEsDescriptionOverlay =
                    LanguageAPI.AddOverlay(
                        "HERETIC_DESCRIPTION",
                        HereticSpanishDescription,
                        "es-ES"
                    );
            }

            if (HereticEnglishDescriptionOverlay == null)
            {
                HereticEnglishDescriptionOverlay =
                    LanguageAPI.AddOverlay(
                        "HERETIC_DESCRIPTION",
                        HereticEnglishDescription,
                        "en"
                    );
            }
        }

        private static void RegisterHereticSpanishTokens()
        {
            // Pasiva añadida por Moffein al SkillLocator de HereticBody.
            RegisterSpanishToken(
                "MOFFEINHERETIC_PASSIVE_DESCRIPTION",
                "La Hereje <style=cIsUtility>es más rápida</style> y puede " +
                "<style=cIsUtility>aletear dos veces</style> para ganar altura adicional en el aire."
            );

            // Marca de Herejía, item propio del mod otorgado al reunir las
            // cuatro piezas de Herejía.
            RegisterSpanishToken(
                "MOFFEINHERETIC_STATBONUSITEM_NAME",
                "Marca de Herejía"
            );

            RegisterSpanishToken(
                "MOFFEINHERETIC_STATBONUSITEM_PICKUP",
                "Obtén una gran mejora de estadísticas... " +
                "<color=#FF7F7F>PERO tu salud decae con el tiempo.</color>"
            );

            RegisterSpanishToken(
                "MOFFEINHERETIC_STATBONUSITEM_DESC",
                "Aumenta la <style=cIsHealing>salud</style> un " +
                "<style=cIsHealing>400%</style> y el <style=cIsDamage>daño</style> un " +
                "<style=cIsDamage>50%</style>. Reduce la " +
                "<style=cIsHealing>regeneración de salud base</style> a " +
                "<style=cIsHealing>-6 PS/s</style>."
            );

            // Con Fix Skill Descriptions activado (valor por defecto), el mod
            // reemplaza únicamente los descriptionToken de Hooks/Essence por
            // estos dos tokens propios. Nombres y demás skills siguen siendo
            // los tokens vanilla de Risk of Rain 2.
            RegisterSpanishSkillDescriptionToken(
                "MOFFEINHERETIC_SKILL_LUNAR_SECONDARY_REPLACEMENT_DESCRIPTION",
                "Carga una esfera de cuchillas que inflige " +
                "<style=cIsDamage>175% de daño</style> repetidamente. Tras un tiempo, explota y " +
                "<style=cIsDamage>enraíza</style> a todos los enemigos, infligiendo " +
                "<style=cIsDamage>700% de daño</style>."
            );

            RegisterSpanishSkillDescriptionToken(
                "MOFFEINHERETIC_SKILL_LUNAR_SPECIAL_REPLACEMENT_DESCRIPTION",
                "Infligir daño añade una acumulación de <style=cIsDamage>Ruina</style>. " +
                "Activar la habilidad <style=cIsDamage>detona todas las acumulaciones de Ruina</style> " +
                "en un alcance ilimitado, infligiendo <style=cIsDamage>300% de daño</style> más " +
                "<style=cIsDamage>120% de daño</style> por cada acumulación de " +
                "<style=cIsDamage>Ruina</style>."
            );

            // Achievement real de Moffein-Heretic.
            RegisterSpanishToken(
                "ACHIEVEMENT_MOFFEINHERETICUNLOCK_NAME",
                "Resurrección poco ortodoxa"
            );

            RegisterSpanishToken(
                "ACHIEVEMENT_MOFFEINHERETICUNLOCK_DESCRIPTION",
                "Escapa de la Luna como la superviviente secreta."
            );

            // El UnlockableDef del mod usa explícitamente esta variante con
            // guion bajo como nameToken, distinta del token autogenerado del
            // AchievementDef. Ambas son superficies visibles reales.
            RegisterSpanishToken(
                "ACHIEVEMENT_MOFFEINHERETIC_UNLOCK_NAME",
                "Resurrección poco ortodoxa"
            );
        }

        private static void RegisterSpanishSkillDescriptionToken(
            string token,
            string spanish
        )
        {
            RegisterSpanishToken(
                token,
                FormatLongSkillText(spanish)
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

            LanguageAPI.Add(token, spanish, "es-419");
            LanguageAPI.Add(token, spanish, "es-ES");
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
