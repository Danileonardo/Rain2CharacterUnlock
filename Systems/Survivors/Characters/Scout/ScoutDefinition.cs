using System;

using BepInEx.Logging;
using R2API;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Integración curada de Scout 0.9.9 / OfficialScoutMod.
    ///
    /// Localiza únicamente contenido visible publicado por Scout:
    /// identidad, Vista general, lore nativo, pasiva, habilidades y keywords.
    ///
    /// La versión auditada sólo publica DEFAULT_SKIN y no expone un unlock
    /// original funcional para el survivor. USU conserva su misión legacy
    /// "Sed Termonuclear" sin modificarla desde esta Definition.
    ///
    /// El creador utiliza dos peculiaridades que se respetan aquí:
    /// - Agile y Atomic Crits se pasan a SkillDef.keywordTokens como texto
    ///   formateado completo, no como nombres de token convencionales;
    /// - el panel de loadout sustituye LOADOUT_SKILL_MISC por los tokens
    ///   literales "Passive" y "Swap" para Scout.
    ///
    /// No referencia OfficialScoutMod.dll; trabaja sólo con Body y tokens.
    /// </summary>
    public sealed class ScoutDefinition : SurvivorDefinition
    {
        public override string SourceIdentifier
        {
            get { return "com.kenko.Scout"; }
        }

        public override string BodyName
        {
            get { return "ScoutBody"; }
        }

        public override string DefinitionName
        {
            get { return "Scout"; }
        }

        private const string ScoutAgileKeywordEnglish = ScoutLocalization.ScoutAgileKeywordEnglish;

        private const string ScoutAtomicCritsKeywordEnglish = ScoutLocalization.ScoutAtomicCritsKeywordEnglish;

        private const string ScoutAgileKeywordSpanish = ScoutLocalization.ScoutAgileKeywordSpanish;

        private const string ScoutAtomicCritsKeywordSpanish = ScoutLocalization.ScoutAtomicCritsKeywordSpanish;

        private static readonly string ScoutSpanishDescription = ScoutLocalization.ScoutSpanishDescription;

        // Diario narrativo USU: "The Last Train Out". Se conserva el origen
        // radiactivo del mod y la personalidad de Scout (TF2). Las frases
        // de audio originales de TF2 siguen EN INGLES tambien en es-419/es-ES.
        // No afecta misiones, habilidades ni la DLL de OfficialScoutMod.
        private static object ScoutEnglishLoreOverlay;



        public override void RegisterInstalledLocalization(
            SurvivorInfo survivor,
            ManualLogSource logger
        )
        {
            if (survivor == null || !Matches(survivor))
            {
                return;
            }

            RegisterScoutSpanishTokens();
            RegisterScoutEnglishLoreOverlay();

            UsuLog.Verbose(logger, 
                "[LOCALIZATION] Definition activa | ScoutBody | " +
                "es-419 / es-ES / en + vista general + diario USU + pasiva + skills + keywords"
            );

            UsuLog.Verbose(logger, 
                "[SCOUT LOCALIZATION] Tokens registrados | identidad + skills + " +
                "Agile/Atomic Crits literales + encabezados Passive/Swap. " +
                "Sed Termonuclear legacy sin cambios."
            );
        }

        public override string ResolveSubtitle(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            return IsSpanishLanguage()
                ? "Fuerza de la naturaleza"
                : fallback ?? "";
        }

        public override string ResolveDescription(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            return IsSpanishLanguage()
                ? ScoutSpanishDescription
                : fallback ?? "";
        }

        public override string ResolveLore(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            if (IsSpanishLanguage())
            {
                return USUScoutHuntressEncounterGate.ResolveScoutLore(true);
            }

            if (IsEnglishLanguage())
            {
                return USUScoutHuntressEncounterGate.ResolveScoutLore(false);
            }

            return fallback ?? "";
        }

        private static void RegisterScoutEnglishLoreOverlay()
        {
            if (ScoutEnglishLoreOverlay != null)
            {
                return;
            }

            // La DLL original publica un token ingles de lore. El overlay
            // permite que el Logbook nativo muestre el diario curado de USU.
            ScoutEnglishLoreOverlay = LanguageAPI.AddOverlay(
                "KENKO_SCOUT_LORE",
                USUScoutHuntressEncounterGate.ResolveScoutLore(false),
                "en"
            );
        }

        private static void RegisterScoutSpanishTokens()
        {
            // Identidad, Vista general y Notas/Logbook.
            RegisterSpanishToken("KENKO_SCOUT_NAME", "Scout");
            RegisterSpanishToken("KENKO_SCOUT_SUBTITLE", "Fuerza de la naturaleza");
            RegisterSpanishToken("KENKO_SCOUT_DESCRIPTION", ScoutSpanishDescription);
            RegisterSpanishToken("KENKO_SCOUT_LORE", USUScoutHuntressEncounterGate.ResolveScoutLore(true));

            // Textos de final publicados por el creador.
            RegisterSpanishToken(
                "KENKO_SCOUT_OUTRO_FLAVOR",
                "Grass grows, birds fly, sun shines, and brother, I hurt people."
            );
            RegisterSpanishToken(
                "KENKO_SCOUT_OUTRO_FAILURE",
                "...¿qué demonios fue esa basura?"
            );

            // Pasiva.
            RegisterSpanishToken("KENKO_SCOUT_PASSIVE_NAME", "Núcleo atómico");
            RegisterSpanishSkillDescriptionToken(
                "KENKO_SCOUT_PASSIVE_DESCRIPTION",
                "Scout puede <style=cIsUtility>saltar dos veces</style>. Inflige <style=cIsDamage>daño</style> para acumular " +
                "<style=cHumanObjective>Núcleo atómico</style>. Recibir daño reduce el <style=cHumanObjective>Núcleo atómico</style>. " +
                "El <style=cHumanObjective>Núcleo atómico</style> aumenta la <style=cIsUtility>velocidad de movimiento</style>."
            );

            // Primarias.
            RegisterSpanishToken("KENKO_SCOUT_PRIMARY_SPLATTERGUN_NAME", "Escopeta salpicadora");
            RegisterSpanishSkillDescriptionToken(
                "KENKO_SCOUT_PRIMARY_SPLATTERGUN_DESCRIPTION",
                "<style=cIsUtility>Ágil</style>. Dispara una ráfaga dispersa que inflige <style=cIsDamage>12x65% de daño</style>."
            );

            RegisterSpanishToken("KENKO_SCOUT_PRIMARY_RIFLE_NAME", "Enano ruin");
            RegisterSpanishSkillDescriptionToken(
                "KENKO_SCOUT_PRIMARY_RIFLE_DESCRIPTION",
                "Dispara un proyectil de alta velocidad que inflige <style=cIsDamage>240% de daño</style>. " +
                "<style=cIsDamage>Los disparos a la cabeza infligen 2x de daño</style>."
            );

            // Secundaria.
            RegisterSpanishToken("KENKO_SCOUT_SECONDARY_CLEAVER_NAME", "Cuchilla tóxica");
            RegisterSpanishSkillDescriptionToken(
                "KENKO_SCOUT_SECONDARY_CLEAVER_DESCRIPTION",
                "<style=cIsUtility>Ágil</style>. Lanza tu cuchilla, aplicando <style=cIsDamage>Añublo</style> e infligiendo " +
                "<style=cIsDamage>400% de daño</style>. Asesta <style=cIsDamage>golpes críticos</style> y " +
                "<style=cIsHealing>envenena</style> a los enemigos <style=cIsDamage>aturdidos</style>."
            );

            // Secundaria del modo Bate.
            RegisterSpanishToken("KENKO_SCOUT_SECONDARY_SPIKEDBALL_NAME", "Pelota con púas");
            RegisterSpanishSkillDescriptionToken(
                "KENKO_SCOUT_SECONDARY_SPIKEDBALL_DESCRIPTION",
                "<style=cIsUtility>Ágil</style>. Golpea tu pelota de béisbol, <style=cIsDamage>aturdiendo</style> e infligiendo " +
                "<style=cIsDamage>300% de daño</style>. La duración del <style=cIsDamage>aturdimiento</style> aumenta según la distancia recorrida."
            );

            // Utilidad.
            RegisterSpanishToken("KENKO_SCOUT_UTILITY_ATOMICBLAST_NAME", "Explosión atómica");
            RegisterSpanishSkillDescriptionToken(
                "KENKO_SCOUT_UTILITY_ATOMICBLAST_DESCRIPTION",
                "Consume tu <style=cHumanObjective>Núcleo atómico</style> para obtener <style=cIsDamage>Críticos atómicos</style>, " +
                "<style=cIsDamage>velocidad de ataque</style> y <style=cIsUtility>velocidad de movimiento</style>. " +
                "Inflige entre <style=cIsDamage>100% y 1000% de daño</style> a tu alrededor según la carga."
            );

            // Especial / cambio de arma.
            RegisterSpanishToken("KENKO_SCOUT_SPECIAL_SWAP_NAME", "Cambiar");
            RegisterSpanishSkillDescriptionToken(
                "KENKO_SCOUT_SPECIAL_SWAP_DESCRIPTION",
                "Cambia a tu bate."
            );

            // Variante de Ancient Scepter, si esa integración está presente.
            RegisterSpanishToken("KENKO_SCOUT_SPECIAL_SCEPTER_SWAP_NAME", "Cambiar");
            RegisterSpanishSkillDescriptionToken(
                "KENKO_SCOUT_SPECIAL_SCEPTER_SWAP_DESCRIPTION",
                "Cambia a tu bate.\n<color=#d299ff>CETRO: aún puedes obtener <style=cHumanObjective>Carga atómica</style> mientras " +
                "se consume el <style=cHumanObjective>Núcleo atómico</style>.</color>"
            );

            // Primaria del modo Bate.
            RegisterSpanishToken("KENKO_SCOUT_PRIMARY_BONK_NAME", "Pata de elefante");
            RegisterSpanishSkillDescriptionToken(
                "KENKO_SCOUT_PRIMARY_BONK_DESCRIPTION",
                "<style=cIsUtility>Ágil</style>. Golpea con tu bate e inflige <style=cIsDamage>300% de daño</style>."
            );

            // Scout no usa identificadores de token normales para estas keywords:
            // pasa estas cadenas completas directamente a SkillDef.keywordTokens.
            RegisterSpanishToken(ScoutAgileKeywordEnglish, ScoutAgileKeywordSpanish);
            RegisterSpanishToken(ScoutAtomicCritsKeywordEnglish, ScoutAtomicCritsKeywordSpanish);

            // El hook del propio Scout convierte dos LOADOUT_SKILL_MISC en estos
            // tokens literales. Son texto visible, por lo que también se localizan.
            RegisterSpanishToken("Passive", "Pasiva");
            RegisterSpanishToken("Swap", "Cambio");
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
