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

        private const string ScoutAgileKeywordEnglish =
            "<style=cKeywordName>Agile</style><style=cSub>The skill can be used while sprinting.</style>";

        private const string ScoutAtomicCritsKeywordEnglish =
            "<style=cKeywordName>Atomic Crits</style><style=cSub>Damage is increased by 125% and all attacks apply Weaken.</style>";

        private const string ScoutAgileKeywordSpanish =
            "<style=cKeywordName>Ágil</style><style=cSub>La habilidad puede usarse mientras corres.</style>";

        private const string ScoutAtomicCritsKeywordSpanish =
            "<style=cKeywordName>Críticos atómicos</style><style=cSub>El daño aumenta un 125% y todos los ataques aplican Debilitar.</style>";

        private static readonly string ScoutSpanishDescription =
            "Scout es un superviviente extremadamente móvil de daño explosivo que puede concentrarse en enemigos individuales con facilidad." +
            "<color=#CCD3E0>\r\n\r\n" +
            "< ! > Usa el retroceso de tu Escopeta salpicadora para alcanzar lugares altos o evitar ataques enemigos.\r\n\r\n" +
            "< ! > Combinar tu Pelota de béisbol y Cuchilla puede infligir un daño enorme desde lejos.\r\n\r\n" +
            "< ! > El Bate puede generar la mayor cantidad de carga del Núcleo atómico cuando hay grupos grandes de enemigos.\r\n\r\n" +
            "< ! > Las cargas adicionales de Explosión atómica aumentan su duración, mientras que la reducción de enfriamiento aumenta la velocidad de carga del Núcleo atómico.\r\n\r\n";

        // Traducción íntegra del lore nativo publicado por Scout.
        private static readonly string ScoutSpanishLore =
            "Una huida de la central eléctrica. Un accidente que se propagaría como la pólvora por las noticias, de causa desconocida. " +
            "Un trabajador solitario se aleja lentamente, tambaleándose, con su bate como único apoyo. Vagó sin rumbo, con el dolor y la conmoción recorriéndole el cuerpo. " +
            "Al acercarse a lo que parecía ser una vía ferroviaria abandonada, a la distancia apenas podía distinguir lo que, lógicamente, consideró otra persona. " +
            "Había algo extraño en su aura, así que mantuvo la guardia en alto e intentó identificar cualquier posible amenaza en la figura.\r\n\r\n" +
            "“Es una buena hora para dar un paseo; ¿te molesta si te acompaño junto a estas viejas vías?”\r\n\r\n" +
            "“¿Q-Quién eres…? ¿Q-qué quieres de mí?”\r\n\r\n" +
            "“Mi identidad no te incumbe.”\r\n" +
            "“... aunque puedo notar que has tenido una noche bastante dura. Cerca de las puertas de la Muerte, ¿eh?”\r\n" +
            "“Puede que tenga algo que pueda ayudarte a salir de esto, una… especie de oferta.”\r\n\r\n" +
            "“¿U-un… trato..?”\r\n\r\n" +
            "“Precisamente.”\r\n\r\n" +
            "Se le cerró la garganta, el corazón comenzó a acelerarse y su mente empezó a fingir que nada de esto estaba ocurriendo. " +
            "Vaciló y finalmente dejó escapar el aliento que había contenido durante lo que pareció una eternidad mientras la persona hablaba. " +
            "¿Era miedo lo que sentía? ¿Confusión? ¿O era la idea de poder tener el mundo al alcance de la mano lo que lo empujaba hacia una tentación casi eufórica? " +
            "Fuera lo que fuese, continuó escuchando atentamente, concentrándose y descifrando cada palabra. Tal vez esto pudiera sacarlo de aquel infierno.\r\n\r\n" +
            "“Puedo sentir la radiación que emanas… hm…”\r\n" +
            "“... tengo justo lo que necesitas. ¿Qué te parece si convierto eso en algo que te dé poder, eh? Hacerte mejor y más fuerte que antes a cambio de… veamos… algunos favores. " +
            "Suena bien, ¿no? A mí me lo parece.”\r\n" +
            "“También tengo unos viejos amigos que pueden ayudar con el procedimiento.”\r\n\r\n" +
            "“... P-pero ¿qué hay de mi trabajo?”\r\n" +
            "“¿Adónde iré? Preferiría no quedarme sin empleo…”\r\n\r\n" +
            "“Qué curioso que lo menciones. He oído de un trabajo que va hacia algún lugar en las profundidades del espacio. Imagino que allí podrían aprovechar tu nueva velocidad y agilidad cuando todo esté terminado.”\r\n" +
            "“... y un consejo: gana algo de confianza. Puede mejorar tus posibilidades.”\r\n\r\n" +
            "“Hm…”\r\n" +
            "“... Está bien, trato hecho… quienquiera que seas.”\r\n\r\n" +
            "“Maravilloso.”\r\n\r\n" +
            "A lo lejos se oye el sonido de un tren. Mientras reducía la velocidad y resonaba el silbato de su vieja locomotora, los dos desaparecieron.";

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

            logger?.LogInfo(
                "[LOCALIZATION] Definition activa | ScoutBody | " +
                "es-419 / es-ES + vista general + lore nativo + pasiva + skills + keywords"
            );

            logger?.LogInfo(
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
            return IsSpanishLanguage()
                ? ScoutSpanishLore
                : fallback ?? "";
        }

        private static void RegisterScoutSpanishTokens()
        {
            // Identidad, Vista general y Notas/Logbook.
            RegisterSpanishToken("KENKO_SCOUT_NAME", "Scout");
            RegisterSpanishToken("KENKO_SCOUT_SUBTITLE", "Fuerza de la naturaleza");
            RegisterSpanishToken("KENKO_SCOUT_DESCRIPTION", ScoutSpanishDescription);
            RegisterSpanishToken("KENKO_SCOUT_LORE", ScoutSpanishLore);

            // Textos de final publicados por el creador.
            RegisterSpanishToken(
                "KENKO_SCOUT_OUTRO_FLAVOR",
                "... la hierba crece, los pájaros vuelan, el sol brilla y, hermano, yo hago daño a la gente."
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
