using System;

using BepInEx.Logging;
using R2API;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Integración curada de Spy 1.3.3 / SpyMod.
    ///
    /// Localiza las habilidades publicadas por Spy y el lore propio de USU:
    /// identidad, Vista general, lore nativo, pasiva, habilidades,
    /// skin de Maestría, achievement y keywords del panel lateral.
    ///
    /// El survivor no expone un desbloqueo original propio en el audit;
    /// USU conserva el comportamiento original de la misión "Sin que me veas venir" sin
    /// modificarla desde esta Definition.
    ///
    /// Spy pasa Agile, Espionage, Opportunist y Backstab a
    /// SkillDef.keywordTokens como cadenas formateadas completas.
    /// También cambia LOADOUT_SKILL_MISC al token literal "Passive".
    /// Ambos casos se localizan aquí sin referencia directa a SpyMod.dll.
    /// </summary>
    public sealed class SpyDefinition : SurvivorDefinition
    {
        public override string SourceIdentifier
        {
            get { return "com.kenko.Spy"; }
        }

        public override string BodyName
        {
            get { return "SpyBody"; }
        }

        public override string DefinitionName
        {
            get { return "Spy"; }
        }

        private const string SpyAgileKeywordEnglish = SpyLocalization.SpyAgileKeywordEnglish;

        private const string SpyEspionageKeywordEnglish = SpyLocalization.SpyEspionageKeywordEnglish;

        // Valor por defecto publicado por Spy 1.3.3. Si el jugador modifica
        // esta opción, el texto de la habilidad principal sigue traducido;
        // esta cadena sólo corresponde al tooltip lateral generado por el mod.
        private const string SpyOpportunistKeywordEnglish = SpyLocalization.SpyOpportunistKeywordEnglish;

        private const string SpyBackstabKeywordEnglish = SpyLocalization.SpyBackstabKeywordEnglish;

        private const string SpyAgileKeywordSpanish = SpyLocalization.SpyAgileKeywordSpanish;

        private const string SpyEspionageKeywordSpanish = SpyLocalization.SpyEspionageKeywordSpanish;

        private const string SpyOpportunistKeywordSpanish = SpyLocalization.SpyOpportunistKeywordSpanish;

        private const string SpyBackstabKeywordSpanish = SpyLocalization.SpyBackstabKeywordSpanish;

        private static readonly string SpySpanishDescription = SpyLocalization.SpySpanishDescription;

        // Diario curado USU «The Professional's Fee» — declaración privada en primera persona, seis párrafos por idioma.
        // Audio original TF2: Gentlemen., Right behind you., Magnificent!, You got blood on my suit.
        // Frases originales en inglés en EN y ES-419/ES-ES; solo lore, no toca lógica de misiones.
        private static readonly string SpySpanishLore = "[USU] Diario I — consultar progreso del superviviente.";

        private static readonly string SpyEnglishLore = "[USU] Diary I — check survivor progress.";

        public override void RegisterInstalledLocalization(
            SurvivorInfo survivor,
            ManualLogSource logger
        )
        {
            if (survivor == null || !Matches(survivor))
            {
                return;
            }

            RegisterSpySpanishTokens();

            UsuLog.Verbose(logger, 
                "[LOCALIZATION] Definition activa | SpyBody | " +
                "es-419 / es-ES + en + vista general + lore USU bilingüe + pasiva + skills + keywords + skin + achievement"
            );

            UsuLog.Verbose(logger, 
                "[SPY LOCALIZATION] Tokens registrados | identidad + skills + " +
                "Agile/Espionage/Opportunist/Backstab literales + encabezado Passive. " +
                "Sin que me veas venir: texto narrativo revisado, mecánica intacta."
            );
        }

        public override string ResolveSubtitle(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            return IsSpanishLanguage()
                ? "Maestro del espionaje"
                : fallback ?? "";
        }

        public override string ResolveDescription(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            return IsSpanishLanguage()
                ? SpySpanishDescription
                : fallback ?? "";
        }

        public override string ResolveLore(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            if (IsSpanishLanguage()) return USUSpyBanditEncounterGate.ResolveSpyLore(true);
            if (IsEnglishLanguage()) return USUSpyBanditEncounterGate.ResolveSpyLore(false);
            return fallback ?? "";
        }

        private static void RegisterSpySpanishTokens()
        {
            // Identidad y Vista general.
            RegisterSpanishToken("KENKO_SPY_NAME", "Spy");
            RegisterSpanishToken("KENKO_SPY_SUBTITLE", "Maestro del espionaje");
            RegisterSpanishToken("KENKO_SPY_DESCRIPTION", SpySpanishDescription);

            // El audit expone LoreToken vacío, por lo que el lore se sirve
            // mediante ResolveLore. Registramos también el token de respaldo
            // en es-419, es-ES y en, sin modificar la DLL del creador.
            RegisterSpanishToken("KENKO_SPY_LORE", SpySpanishLore);
            LanguageAPI.Add("KENKO_SPY_LORE", SpyEnglishLore, "en");

            // Pasiva.
            RegisterSpanishToken("KENKO_SPY_PASSIVE_NAME", "Puñalada por la espalda");
            RegisterSpanishSkillDescriptionToken(
                "KENKO_SPY_PASSIVE_DESCRIPTION",
                "<color=#62746f>Spy</color> puede asestar <style=cIsHealth>puñaladas por la espalda</style> a los enemigos con su " +
                "<color=#62746f>Cuchillo</color>, provocando un <style=cIsDamage>golpe crítico</style> que " +
                "<style=cIsHealth>mata al instante</style> a los enemigos más débiles."
            );

            // Primarias.
            RegisterSpanishToken("KENKO_SPY_PRIMARY_REVOLVER_NAME", "Diamondback");
            RegisterSpanishSkillDescriptionToken(
                "KENKO_SPY_PRIMARY_REVOLVER_DESCRIPTION",
                "Dispara una bala que inflige <style=cIsDamage>320% de daño</style>. Las muertes con " +
                "<style=cIsHealth>Puñalada por la espalda</style> otorgan <color=#62746f>Espionaje</color>. " +
                "Máximo de 5 acumulaciones."
            );

            RegisterSpanishToken("KENKO_SPY_PRIMARY_REVOLVER2_NAME", "Embajador");
            RegisterSpanishSkillDescriptionToken(
                "KENKO_SPY_PRIMARY_REVOLVER2_DESCRIPTION",
                "Dispara una bala que inflige <style=cIsDamage>260% de daño</style>. Acertar en la cabeza " +
                "<style=cIsDamage>asesta un golpe crítico</style>."
            );

            // Secundarias.
            RegisterSpanishToken("KENKO_SPY_SECONDARY_KNIFE_NAME", "Puñalada");
            RegisterSpanishSkillDescriptionToken(
                "KENKO_SPY_SECONDARY_KNIFE_DESCRIPTION",
                "Prepara tu <color=#62746f>Cuchillo</color>. Suelta para atacar e infligir " +
                "<style=cIsDamage>500% de daño</style>."
            );

            RegisterSpanishToken("KENKO_SPY_SECONDARY_KNIFE2_NAME", "Gran ganador");
            RegisterSpanishSkillDescriptionToken(
                "KENKO_SPY_SECONDARY_KNIFE2_DESCRIPTION",
                "<color=#62746f>Oportunista</color>. Prepara tu <color=#62746f>Cuchillo</color>. Suelta para atacar e infligir " +
                "<style=cIsDamage>500% de daño</style>. Las muertes con <style=cIsHealth>Puñalada por la espalda</style> " +
                "otorgan <style=cIsUtility>velocidad de movimiento</style> y <style=cIsHealing>barrera</style>, además de " +
                "<style=cIsUtility>reiniciar el enfriamiento de esta habilidad</style> durante un breve periodo."
            );

            // Utilidad.
            RegisterSpanishToken("KENKO_SPY_UTILITY_FLIP_NAME", "Sabotaje");
            RegisterSpanishSkillDescriptionToken(
                "KENKO_SPY_UTILITY_FLIP_DESCRIPTION",
                "<style=cIsUtility>Desplázate</style> en una dirección o da una <style=cIsUtility>voltereta</style> en el aire. " +
                "Coloca un <color=#62746f>Zapador</color> en un enemigo cercano, " +
                "<style=cIsUtility>electrocutándolo</style> a él y a los enemigos cercanos durante " +
                "<style=cIsUtility>5 segundos</style>."
            );

            // Especiales.
            RegisterSpanishToken("KENKO_SPY_SPECIAL_WATCH_NAME", "Camuflaje");
            RegisterSpanishSkillDescriptionToken(
                "KENKO_SPY_SPECIAL_WATCH_DESCRIPTION",
                "Vuélvete <style=cIsUtility>invisible</style> durante un máximo de <style=cIsUtility>8 segundos</style>. " +
                "Mientras está <style=cIsUtility>invisible</style>, <color=#62746f>Spy</color> no puede disparar."
            );

            RegisterSpanishToken("KENKO_SPY_SPECIAL_WATCH2_NAME", "Reloj del muerto");
            RegisterSpanishSkillDescriptionToken(
                "KENKO_SPY_SPECIAL_WATCH2_DESCRIPTION",
                "Saca tu <color=#62746f>Reloj del muerto</color>. Recibir <style=cIsDamage>daño</style> concede " +
                "<style=cIsUtility>invisibilidad</style> durante <style=cIsUtility>8 segundos</style> a costa de hasta un " +
                "<style=cIsHealth>25% de PS</style>. Mientras tengas fuera el <color=#62746f>Reloj del muerto</color>, " +
                "<color=#62746f>Spy</color> no puede disparar."
            );

            // Skin única detectada aparte de DEFAULT_SKIN.
            RegisterSpanishToken("KENKO_SPY_MASTERY_SKIN_NAME", "Alternativo");

            // Achievement de la skin.
            RegisterSpanishToken(
                "ACHIEVEMENT_KENKO_SPY_MASTERYACHIEVEMENT_NAME",
                "Spy: Maestría"
            );
            RegisterSpanishToken(
                "ACHIEVEMENT_KENKO_SPY_MASTERYACHIEVEMENT_DESCRIPTION",
                "Como Spy, supera el juego o obliterarte en Monzón."
            );

            // Spy, igual que Scout, pasa estas cadenas formateadas completas
            // a SkillDef.keywordTokens en vez de tokens de idioma convencionales.
            RegisterSpanishToken(SpyAgileKeywordEnglish, SpyAgileKeywordSpanish);
            RegisterSpanishToken(SpyEspionageKeywordEnglish, SpyEspionageKeywordSpanish);
            RegisterSpanishToken(SpyOpportunistKeywordEnglish, SpyOpportunistKeywordSpanish);
            RegisterSpanishToken(SpyBackstabKeywordEnglish, SpyBackstabKeywordSpanish);

            // El hook del propio Spy sustituye LOADOUT_SKILL_MISC por este
            // token literal cuando reconstruye el loadout del personaje.
            RegisterSpanishToken("Passive", "Pasiva");
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
