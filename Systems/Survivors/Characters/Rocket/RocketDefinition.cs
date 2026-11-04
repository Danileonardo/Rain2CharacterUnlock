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

        private static object RocketEnglishLoreOverlay;

        private static readonly string RocketSpanishDescription = RocketLocalization.RocketSpanishDescription;
        // El diario activo de Rocketer se resuelve en USULoaderRocketerEncounterGate.

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
            RegisterRocketEnglishLore();

            UsuLog.Verbose(logger, 
                "[LOCALIZATION] Definition activa | RocketSurvivorBody | " +
                "es-419 / es-ES / en + vista general + lore USU + pasiva + skills + " +
                "desbloqueos de skills + skin de Maestría"
            );

            UsuLog.Verbose(logger, 
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
            if (IsSpanishLanguage()) return USULoaderRocketerEncounterGate.ResolveRocketerLore(true);
            if (IsEnglishLanguage()) return USULoaderRocketerEncounterGate.ResolveRocketerLore(false);
            return fallback ?? "";
        }

        private static void RegisterRocketEnglishLore()
        {
            // El Logbook nativo usa el puente de consulta dinámica para actualizar
            // el bloqueo; este overlay sólo proporciona un valor inicial.
            if (RocketEnglishLoreOverlay != null) return;
            RocketEnglishLoreOverlay = LanguageAPI.AddOverlay(
                "MOFFEIN_ROCKET_BODY_LORE", USULoaderRocketerEncounterGate.ResolveRocketerLore(false), "en"
            );
        }

        private static void RegisterRocketSpanishTokens()
        {
            // Identidad, Vista general y Notas/Logbook.
            RegisterSpanishToken("MOFFEIN_ROCKET_BODY_NAME", "Rocket");
            RegisterSpanishToken("MOFFEIN_ROCKET_BODY_SUBTITLE", "Conmoción y pavor");
            RegisterSpanishToken("MOFFEIN_ROCKET_BODY_DESCRIPTION", RocketSpanishDescription);
            RegisterSpanishToken("MOFFEIN_ROCKET_BODY_LORE", USULoaderRocketerEncounterGate.ResolveRocketerLore(true));

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

        private static bool IsEnglishLanguage()
        {
            string language = Language.currentLanguageName ?? "";
            return language.StartsWith("en", StringComparison.OrdinalIgnoreCase);
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
