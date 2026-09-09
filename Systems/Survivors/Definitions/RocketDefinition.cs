using System;

using BepInEx.Logging;
using R2API;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Integración curada de RocketSurvivor 1.1.4.
    ///
    /// Localiza únicamente contenido visible publicado por Rocket:
    /// identidad, Vista general, lore nativo, pasiva, habilidades,
    /// desbloqueos de habilidades, skin de Maestría y textos de final.
    ///
    /// Rocket no publica un desbloqueo original para el survivor en el
    /// contenido auditado. USU conserva su misión legacy "La gravedad es
    /// opcional" sin modificarla desde esta Definition.
    ///
    /// No referencia RocketSurvivor.dll; trabaja sólo con Body y tokens.
    /// </summary>
    public sealed class RocketDefinition : SurvivorDefinition
    {
        public override string SourceIdentifier
        {
            get { return "com.EnforcerGang.RocketSurvivor"; }
        }

        public override string BodyName
        {
            get { return "RocketSurvivorBody"; }
        }

        public override string DefinitionName
        {
            get { return "Rocket"; }
        }

        private static readonly string RocketSpanishDescription =
            "Rocket es un experto en explosivos extremadamente móvil que inflige un daño de área devastador." +
            "<style=cSub>\r\n\r\n" +
            "< ! > Usa saltos explosivos con frecuencia para recorrer el mapa rápidamente y escapar del peligro.\r\n\r\n" +
            "< ! > Tu Lanzacohetes HG4 tiene un gran retroceso contra los enemigos. Úsalo para arrojarlos por los bordes.\r\n\r\n" +
            "< ! > Detonador remoto es útil para golpear enemigos voladores y también puede usarse para moverte.\r\n\r\n" +
            "< ! > Carga de nitro es excelente tanto para daño de área como para movilidad.\r\n\r\n" +
            "< ! > Rearme rápido puede usarse para saltarte el largo tiempo de recarga de tu lanzacohetes.";

        // Traducción íntegra del lore nativo publicado por RocketSurvivor.
        private static readonly string RocketSpanishLore =
            "Registro - Día 432\n\n" +
            "¿Sabes qué es mejor que un lanzacohetes? NADA.\n\n" +
            "Llevo más de un año volando por los aires alienígenas, rocas y Dios sabe qué más por este " +
            "planeta olvidado, y déjame decirte algo: no hay problema que una explosión bien sincronizada " +
            "no pueda arreglar. ¿Se te viene encima un enjambre de bichos? BOOM. ¿Necesitas cruzar un hueco? " +
            "BAM. Diablos, hasta he hecho explotar la cena un par de veces. La carne carbonizada sabe mejor " +
            "cuando está crujiente, ¿vale?\n\n" +
            "Me mandaron aquí por mi \"comportamiento imprudente\". A ver, ¿de verdad es imprudencia si da " +
            "resultados? Sí, puede que haya arrasado un par de bases por accidente. ¿Quién no? Intenta mantener " +
            "tranquilo el dedo del gatillo cuando cargas con 60 libras de diversión explosiva.\n\n" +
            "Ahora estoy atrapado luchando contra cosas-lagarto y bichos espaciales. Pero ¿sabes qué? Me lo " +
            "estoy pasando bomba (literalmente).\n\n" +
            "Cada vez que oigo ese dulce fuuush de un cohete surcando el aire, sé que va a ser un buen día. " +
            "¿Y cuando impacta? Bueno, nunca he sido muy de fijarme en los detalles, pero las vistas siempre " +
            "son espectaculares.\n\n" +
            "Claro, puede que algún día me haga volar por los aires. Pero hasta entonces, voy a agarrar este " +
            "lanzador y cabalgar la explosión hasta casa.\n\n" +
            "Fin del registro. Y sí, también hice explotar la grabadora. Ups.";

        public override void RegisterInstalledLocalization(
            SurvivorInfo survivor,
            ManualLogSource logger
        )
        {
            if (survivor == null || !Matches(survivor))
            {
                return;
            }

            RegisterRocketSpanishTokens();

            logger?.LogInfo(
                "[LOCALIZATION] Definition activa | RocketSurvivorBody | " +
                "es-419 / es-ES + vista general + lore nativo + pasiva + skills + " +
                "desbloqueos de skills + skin de Maestría"
            );

            logger?.LogInfo(
                "[ROCKET LOCALIZATION] Tokens registrados | identidad + skills + " +
                "achievements + Bombardier + finales. Misión USU legacy sin cambios."
            );
        }

        public override string ResolveSubtitle(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            return IsSpanishLanguage()
                ? "Conmoción y pavor"
                : fallback ?? "";
        }

        public override string ResolveDescription(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            return IsSpanishLanguage()
                ? RocketSpanishDescription
                : fallback ?? "";
        }

        public override string ResolveLore(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            return IsSpanishLanguage()
                ? RocketSpanishLore
                : fallback ?? "";
        }

        private static void RegisterRocketSpanishTokens()
        {
            // Identidad, Vista general y Notas/Logbook.
            RegisterSpanishToken("MOFFEIN_ROCKET_BODY_NAME", "Rocket");
            RegisterSpanishToken("MOFFEIN_ROCKET_BODY_SUBTITLE", "Conmoción y pavor");
            RegisterSpanishToken("MOFFEIN_ROCKET_BODY_DESCRIPTION", RocketSpanishDescription);
            RegisterSpanishToken("MOFFEIN_ROCKET_BODY_LORE", RocketSpanishLore);

            // Textos de final del survivor.
            RegisterSpanishToken(
                "MOFFEIN_ROCKET_BODY_OUTRO_FLAVOR",
                "...y así se fue, despegando otra vez."
            );
            RegisterSpanishToken(
                "MOFFEIN_ROCKET_BODY_MAIN_ENDING_ESCAPE_FAILURE_FLAVOR",
                "...y así desapareció, para no volver a tocar el suelo."
            );

            // Pasiva.
            RegisterSpanishToken("MOFFEIN_ROCKET_BODY_PASSIVE_NAME", "Despegue");
            RegisterSpanishSkillDescriptionToken(
                "MOFFEIN_ROCKET_BODY_PASSIVE_DESCRIPTION",
                "Tus explosivos pueden usarse para realizar <style=cIsUtility>saltos explosivos</style>."
            );

            // Primaria.
            RegisterSpanishToken("MOFFEIN_ROCKET_BODY_PRIMARY_NAME", "Lanzacohetes HG4");
            RegisterSpanishSkillDescriptionToken(
                "MOFFEIN_ROCKET_BODY_PRIMARY_DESCRIPTION",
                "<style=cIsUtility>Ágil</style>. Dispara un cohete que inflige " +
                "<style=cIsDamage>600% de daño</style>. Puede almacenar hasta 4."
            );

            // Primaria alternativa.
            RegisterSpanishToken("MOFFEIN_ROCKET_BODY_PRIMARY_ALT_NAME", "Lanzador SAM HG4");
            RegisterSpanishSkillDescriptionToken(
                "MOFFEIN_ROCKET_BODY_PRIMARY_ALT_DESCRIPTION",
                "<style=cIsUtility>Ágil</style>. Dispara un misil <style=cIsUtility>buscador de calor</style> " +
                "con un <style=cIsHealth>radio de explosión menor</style> que inflige " +
                "<style=cIsDamage>600% de daño</style>. Puede almacenar hasta 4."
            );

            // Secundaria.
            RegisterSpanishToken("MOFFEIN_ROCKET_BODY_SECONDARY_NAME", "Detonador remoto");
            RegisterSpanishSkillDescriptionToken(
                "MOFFEIN_ROCKET_BODY_SECONDARY_DESCRIPTION",
                "Detona <style=cIsDamage>todos los explosivos</style>. Los <style=cIsDamage>cohetes</style> " +
                "detonados obtienen un <style=cIsDamage>50%</style> adicional de " +
                "<style=cIsDamage>daño</style> y <style=cIsDamage>radio de explosión</style>."
            );

            // Utilidad.
            RegisterSpanishToken("MOFFEIN_ROCKET_BODY_UTILITY_NAME", "Carga de nitro");
            RegisterSpanishSkillDescriptionToken(
                "MOFFEIN_ROCKET_BODY_UTILITY_DESCRIPTION",
                "<style=cIsDamage>Aturdidor</style>. Coloca una carga explosiva. Usa " +
                "<style=cIsDamage>Detonador remoto</style> para detonarla e infligir " +
                "<style=cIsDamage>1200% de daño</style>."
            );

            // Utilidad alternativa. El mismo SkillDef también puede aparecer en Especial.
            RegisterSpanishToken("MOFFEIN_ROCKET_BODY_UTILITY_ALT_NAME", "Bombardeo");
            RegisterSpanishSkillDescriptionToken(
                "MOFFEIN_ROCKET_BODY_UTILITY_ALT_DESCRIPTION",
                "<style=cIsUtility>Pesado</style>. <style=cIsDamage>Aturdidor</style>. Golpea a un enemigo " +
                "con tu pala explosiva e inflige <style=cIsDamage>1200% de daño</style>. Inflige " +
                "<style=cIsDamage>golpes críticos</style> mientras realizas un " +
                "<style=cIsUtility>salto explosivo</style>."
            );

            // Especial.
            RegisterSpanishToken("MOFFEIN_ROCKET_BODY_SPECIAL_NAME", "Rearme rápido");
            RegisterSpanishSkillDescriptionToken(
                "MOFFEIN_ROCKET_BODY_SPECIAL_DESCRIPTION",
                "<style=cIsDamage>Dispara rápidamente 4 cohetes</style> y luego " +
                "<style=cIsUtility>recarga tu arma</style>."
            );

            // Desbloqueo de Lanzador SAM HG4.
            RegisterSpanishToken(
                "ACHIEVEMENT_MOFFEINROCKETHOMINGUNLOCK_NAME",
                "Rocket: La velocidad es vida"
            );
            RegisterSpanishToken(
                "ACHIEVEMENT_MOFFEINROCKETHOMINGUNLOCK_DESCRIPTION",
                "Como Rocket, alcanza y atraviesa el Portal celestial en 25 minutos o menos."
            );

            // Desbloqueo de Bombardeo.
            RegisterSpanishToken(
                "ACHIEVEMENT_MOFFEINROCKETMARKETGARDENUNLOCK_NAME",
                "Rocket: Pogo"
            );
            RegisterSpanishToken(
                "ACHIEVEMENT_MOFFEINROCKETMARKETGARDENUNLOCK_DESCRIPTION",
                "Como Rocket, realiza 10 saltos explosivos seguidos sin tocar el suelo."
            );

            // Skin de Maestría y su achievement. DEFAULT_SKIN es global y no se toca.
            RegisterSpanishToken("MOFFEIN_ROCKET_BODY_MASTERY_SKIN_NAME", "Bombardero");
            RegisterSpanishToken(
                "ACHIEVEMENT_MOFFEINROCKETCLEARGAMEMONSOON_NAME",
                "Rocket: Maestría"
            );
            RegisterSpanishToken(
                "ACHIEVEMENT_MOFFEINROCKETCLEARGAMEMONSOON_DESCRIPTION",
                "Como Rocket, completa el juego u oblítérate en Monzón."
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
    }
}
