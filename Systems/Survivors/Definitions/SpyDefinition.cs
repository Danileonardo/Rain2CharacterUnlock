using System;

using BepInEx.Logging;
using R2API;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Integración curada de Spy 1.3.3 / SpyMod.
    ///
    /// Localiza únicamente contenido visible publicado por Spy:
    /// identidad, Vista general, lore nativo, pasiva, habilidades,
    /// skin de Maestría, achievement y keywords del panel lateral.
    ///
    /// El survivor no expone un desbloqueo original propio en el audit;
    /// USU conserva la misión legacy "Sin que me veas venir" sin
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

        private const string SpyAgileKeywordEnglish =
            "<style=cKeywordName>Agile</style><style=cSub>The skill can be used while sprinting.</style>";

        private const string SpyEspionageKeywordEnglish =
            "<style=cKeywordName>Espionage</style><style=cSub>Your next shot has <style=cIsDamage>100% Crit Chance</style>. " +
            "Rolling a <style=cIsDamage>Critical Strike</style> with <color=#62746f>Espionage</color> stacks increases " +
            "<style=cIsDamage>damage by 2x</style> instead. <style=cIsHealth>Backstabs</style> will always grant " +
            "<color=#62746f>Espionage</color> stacks on champion enemies.</style>";

        // Valor por defecto publicado por Spy 1.3.3. Si el jugador modifica
        // esta opción, el texto de la habilidad principal sigue traducido;
        // esta cadena sólo corresponde al tooltip lateral generado por el mod.
        private const string SpyOpportunistKeywordEnglish =
            "<style=cKeywordName>Opportunist</style><style=cSub>Your <style=cIsHealth>HP</style> and " +
            "<style=cIsHealing>health regneration</style> are <style=cDeath>permanently reduced by 25%</style>. " +
            "<style=cIsHealth>Backstabs</style> that don't kill champion enemies grant " +
            "<style=cIsUtility>movement speed</style> and <style=cIsHealing>barrier</style>. </style>";

        private const string SpyBackstabKeywordEnglish =
            "<style=cKeywordName>Backstab</style><style=cSub>Deals <style=cIsDamage>2x damage</style> against champion enemies " +
            "and <style=cIsHealth>30% HP</style> or more to elites.</style>";

        private const string SpyAgileKeywordSpanish =
            "<style=cKeywordName>Ágil</style><style=cSub>La habilidad puede usarse mientras corres.</style>";

        private const string SpyEspionageKeywordSpanish =
            "<style=cKeywordName>Espionaje</style><style=cSub>Tu siguiente disparo tiene un " +
            "<style=cIsDamage>100% de probabilidad de crítico</style>. Obtener un " +
            "<style=cIsDamage>golpe crítico</style> con acumulaciones de <color=#62746f>Espionaje</color> " +
            "aumenta el <style=cIsDamage>daño a 2x</style> en su lugar. Las " +
            "<style=cIsHealth>puñaladas por la espalda</style> siempre otorgan acumulaciones de " +
            "<color=#62746f>Espionaje</color> contra enemigos campeones.</style>";

        private const string SpyOpportunistKeywordSpanish =
            "<style=cKeywordName>Oportunista</style><style=cSub>Tu <style=cIsHealth>salud</style> y tu " +
            "<style=cIsHealing>regeneración de salud</style> se reducen " +
            "<style=cDeath>permanentemente un 25%</style>. Las <style=cIsHealth>puñaladas por la espalda</style> " +
            "que no maten a enemigos campeones otorgan <style=cIsUtility>velocidad de movimiento</style> y " +
            "<style=cIsHealing>barrera</style>.</style>";

        private const string SpyBackstabKeywordSpanish =
            "<style=cKeywordName>Puñalada por la espalda</style><style=cSub>Inflige " +
            "<style=cIsDamage>2x de daño</style> contra enemigos campeones y al menos un " +
            "<style=cIsHealth>30% de la salud</style> a los élites.</style>";

        private static readonly string SpySpanishDescription =
            "Spy es un asesino cuerpo a cuerpo frágil que destaca al eliminar con facilidad objetivos de alta prioridad." +
            "<color=#CCD3E0>\r\n\r\n" +
            "< ! > Acumula Espionaje con el cuchillo de Spy y desata golpes críticos con Diamondback.\r\n\r\n" +
            "< ! > Puñalada es una habilidad devastadora capaz de matar al instante a los enemigos más débiles.\r\n\r\n" +
            "< ! > Sabotaje puede ser una gran herramienta para colocarte a alcance de una Puñalada por la espalda mientras aturdes a los enemigos cercanos.\r\n\r\n" +
            "< ! > Camuflaje sirve tanto para iniciar un enfrentamiento como para escapar, pero exige administrar correctamente el recurso.\r\n\r\n";

        // Traducción íntegra del lore nativo auditado de Spy.
        private static readonly string SpySpanishLore =
            "A medida que el rápido desarrollo de los imperios espaciales se extendía por las estrellas, también lo hacían las guerras silenciosas que ardían entre ellos. " +
            "Las fachadas diplomáticas ocultaban la tensión, pero bajo la superficie se escondían feroces disputas políticas, alianzas que se fracturaban e ideologías que chocaban en el silencio del espacio.\r\n\r\n" +
            "En esta era de ambición interestelar, un conflicto abierto corría el riesgo de provocar la aniquilación total. En su lugar, los líderes recurrieron a las sombras para ejecutar su voluntad. " +
            "El espionaje se convirtió en el arma predilecta: una guerra silenciosa librada mediante flujos de datos, códigos susurrados y secretos robados. Civilizaciones enteras fueron inclinadas no por flotas ni por potencia de fuego, sino por un susurro bien colocado, un protocolo corrompido o un funcionario desaparecido.\r\n\r\n" +
            "Los espías se convirtieron en fantasmas dentro de la maquinaria de los imperios. Maestros del engaño, la infiltración y la subversión, cruzaban fronteras sin ser detectados y penetraban en los imperios con precisión quirúrgica. " +
            "Algunos servían a soberanos. Otros servían a ideales. Unos pocos sólo se servían a sí mismos.\r\n\r\n" +
            "Sus victorias nunca desfilan ante el público. Sus nombres nunca se pronuncian. Sus misiones son enterradas por diseño. Y, aun así, el ascenso y la caída de naciones estelares enteras han dependido de las acciones de unos pocos agentes invisibles.\r\n\r\n" +
            "A un buen espía nunca lo atrapan. La huella que deja en la historia no puede rastrearse, pero en el silencioso cambio del poder, en el instante en que un imperio surge o se derrumba, su mano siempre está allí, invisible... pero innegable.";

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

            logger?.LogInfo(
                "[LOCALIZATION] Definition activa | SpyBody | " +
                "es-419 / es-ES + vista general + lore nativo + pasiva + skills + keywords + skin + achievement"
            );

            logger?.LogInfo(
                "[SPY LOCALIZATION] Tokens registrados | identidad + skills + " +
                "Agile/Espionage/Opportunist/Backstab literales + encabezado Passive. " +
                "Sin que me veas venir legacy sin cambios."
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
            return IsSpanishLanguage()
                ? SpySpanishLore
                : fallback ?? "";
        }

        private static void RegisterSpySpanishTokens()
        {
            // Identidad y Vista general.
            RegisterSpanishToken("KENKO_SPY_NAME", "Spy");
            RegisterSpanishToken("KENKO_SPY_SUBTITLE", "Maestro del espionaje");
            RegisterSpanishToken("KENKO_SPY_DESCRIPTION", SpySpanishDescription);

            // El audit expone LoreToken vacío, por lo que el lore se sirve
            // mediante ResolveLore. Se registra también el token convencional
            // por compatibilidad si una versión futura del mod lo publica.
            RegisterSpanishToken("KENKO_SPY_LORE", SpySpanishLore);

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
