using System;

using BepInEx.Logging;
using R2API;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Integración curada de MinerUnearthed 1.10.0.
    ///
    /// Miner publica un sistema original de desbloqueo real:
    /// MINER_UNLOCKABLE_ACHIEVEMENT_ID -> MINER_UNLOCKABLE_REWARD_ID.
    /// Su requisito es abrir un Cofre legendario. USU conserva por completo
    /// esa lógica original; esta Definition sólo localiza el texto visible.
    ///
    /// También aporta es-419 / es-ES para identidad, Vista general, pasiva,
    /// habilidades, skins, achievements originales y el lore nativo del creador.
    /// El inglés original nunca se reemplaza.
    ///
    /// No referencia MinerUnearthed.dll. Sólo usa Body y tokens públicos.
    /// </summary>
    public sealed class MinerDefinition : SurvivorDefinition
    {
        public override string SourceIdentifier
        {
            get { return "DiggerUnearthed.DiggerContent"; }
        }

        public override string BodyName
        {
            get { return "MinerBody"; }
        }

        public override string DefinitionName
        {
            get { return "Miner"; }
        }

        private static readonly string MinerSpanishDescription =
            "El Minero es un superviviente cuerpo a cuerpo rápido y sumamente móvil que prioriza encadenar " +
            "largas rachas de bajas para acumular cargas de su pasiva.<color=#CCD3E0>\r\n\r\n" +
            "< ! > Cuando tengas una buena cantidad de cargas de Adrenalina, Tajo y Aplastar serán tus mejores " +
            "fuentes de daño.\r\n\r\n" +
            "< ! > Cargar Carga perforadora sólo afecta al daño infligido. Apunta al suelo o directamente a los " +
            "enemigos para concentrar el daño.\r\n\r\n" +
            "< ! > Puedes pulsar Retroexplosión para recorrer una distancia corta. Mantenla pulsada para llegar " +
            "más lejos.\r\n\r\n" +
            "< ! > Hasta las estrellas puede infligir mucho daño a enemigos grandes.";

        // Traducción íntegra del lore nativo de MinerUnearthed.
        // Se conservan los cortes/corrupciones visuales que forman parte del texto del creador.
        private static readonly string MinerSpanishLore =
            "\nEntre el acero frío de las ruinas del carguero y el aire de la noche helada, la tenue luz azul de " +
            "un diario de audio llama tu atención. El diario yace completamente destrozado entre los restos de la " +
            "carga y, aun así, persiste y continúa funcionando. Los botones analógicos de la carcasa pulsan " +
            "lentamente, como si invitaran a cualquiera a escuchar sus grabaciones deformadas, cada vez menos " +
            "coherentes...\n\n" +
            "<color=#8990A7><CLIC></color>\n\n" +
            "<color=#8990A7>/ / - - R  E  G  I  S  T  R  O     1 - - / /</color>\n" +
            "''Primer registro. Fecha, ehhh... cero-cinco, cero-uno, veinte-cincuenta y cinco. Los mandos decidieron " +
            "sacarnos del trabajo de minería. Después de meses excavando, después de meses de sangre, su" +
            "<color=#8990A7>-///////-</color>y millones gastados en esta operación, ¿simplemente vamos a abandonarla? " +
            "Mier<color=#8990A7>-//-</color>.''\n\n" +
            "<color=#8990A7>/ / - - R  E  G  I  S  T  R  O     5 - - / /</color>\n" +
            "''<color=#8990A7>-////////-</color>ntro del asteroide. Nunca había visto nada parecido. Era como un " +
            "fr<color=#8990A7>-////////-</color>tal de ensueño... Por dentro es casi como un caleidoscopio. Tiene que " +
            "valer<color=#8990A7>-/////-</color>y los mandos quieren que nos larguemos, YA. Bueno, pueden besarme el " +
            "cu<color=#8990A7>-////-</color>. Y esta noche voy a salir a excavar el resto... Si consigo vender esto, " +
            "los próximos mil sueldos no le llegarían ni a los talones. Llevo esperando una oportunidad así toda " +
            "la vida.''\n\n" +
            "<color=#8990A7>/ / - - R  E  G  I  S  T  R  O     7 - - / /</color>\n" +
            "''Estoy bastante seguro de que ya lo saben todo. Ahora están interrogando a la gente. Escondí el " +
            "artefacto en un conducto de ventilación en<color=#8990A7>-//////-</color>, debería estar seguro. Un " +
            "carguero llamado UE<color=#8990A7>-////-</color> atracará en unos meses. Sólo tengo que aguantar y " +
            "colarme a bordo.''\n\n" +
            "<color=#8990A7>/ / - - R  E  G  I  S  T  R  O     15 - - / /</color>\n" +
            "''Me están buscando. No falta mucho para que me encuentren. Escondido en las paredes de la nave como " +
            "una pu<color=#8990A7>-//////-</color> rata. El carguero va con retraso. Lo di todo por esto. Lo di " +
            "<i>todo</i> por esto.''\n\n" +
            "<color=#8990A7>/ / - - R  E  G  I  S  T  R  O     16 - - / /</color>\n" +
            "''En algún lugar del casco. Nadie viene hasta aquí. Ni siquiera mantenimiento. Debería estar a salvo " +
            "hasta que llegue la nave.''\n\n" +
            "<color=#8990A7>/ / - - R  E  G  I  S  T  R  O     18 - - / /</color>\n" +
            "''La nave sigue retenida. No tengo forma de saber cuándo dejará de estarlo.'' " +
            "<color=#8990A7><Quejido>.</color> ''Esto es ago<color=#8990A7>-//-</color>izante.''\n\n " +
            "<color=#8990A7>/ / - - R  E  G  I  S  T  R  O     29 - - / /</color>\n" +
            "''<color=#8990A7>-//////////-</color> está aquí. Por fin. Por fin. Tengo que subir el artefacto a bordo. " +
            "Hay seguridad por todas partes. Mercenarios...'' " +
            "<color=#8990A7><Voces lejanas mantienen una conversación ininteligible.></color> " +
            "''Bien. Cállate. Cállate y vete. Ya voy. Voy. Esta noche.''\n\n " +
            "<color=#8990A7>/ / - - R  E  G  I  S  T  R  O     31 - - / /</color>\n" +
            "''<color=#8990A7>-//////-</color>algo le está pasando a la nave. La carga sale volando. Perdí el " +
            "artefacto. Lo perdí todo. . . . Lo perdí to<color=#8990A7>-//-</color>.''\n\n" +
            "La pantalla del diario de audio chisporrotea y estalla, dejándote en completa oscuridad, acompañada " +
            "por el silencio ensordecedor que traen consigo las ominosas últimas palabras del minero.";

        public override void RegisterInstalledLocalization(
            SurvivorInfo survivor,
            ManualLogSource logger
        )
        {
            if (survivor == null || !Matches(survivor))
            {
                return;
            }

            RegisterMinerSpanishTokens();

            logger?.LogInfo(
                "[LOCALIZATION] Definition activa | MinerBody | " +
                "es-419 / es-ES + vista general + pasiva + skills + skins + achievements originales + lore nativo"
            );

            logger?.LogInfo(
                "[MINER UNLOCK] Original conservado | MINER_UNLOCKABLE_ACHIEVEMENT_ID -> " +
                "MINER_UNLOCKABLE_REWARD_ID | Requisito: abrir un Cofre legendario."
            );
        }

        public override string ResolveSubtitle(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            return IsSpanishLanguage()
                ? "Drogadicto autodestructivo"
                : fallback ?? "";
        }

        public override string ResolveDescription(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            return IsSpanishLanguage()
                ? MinerSpanishDescription
                : fallback ?? "";
        }

        public override string ResolveLore(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            // El creador publica un lore nativo de 454 palabras. USU conserva
            // el original inglés y sólo proporciona su traducción al español.
            return IsSpanishLanguage()
                ? MinerSpanishLore
                : fallback ?? "";
        }

        private static void RegisterMinerSpanishTokens()
        {
            // Identidad y Vista general.
            RegisterSpanishToken("MINER_NAME", "Minero");
            RegisterSpanishToken("MINER_SUBTITLE", "Drogadicto autodestructivo");
            RegisterSpanishToken("MINER_DESCRIPTION", MinerSpanishDescription);
            RegisterSpanishToken("MINER_LORE", MinerSpanishLore);

            // Pasiva.
            RegisterSpanishToken("MINER_PASSIVE_NAME", "Fiebre del oro");
            RegisterSpanishSkillDescriptionToken(
                "MINER_PASSIVE_DESCRIPTION",
                "Obtén <style=cIsHealth>ADRENALINA</style> al recibir oro, aumentando la " +
                "<style=cIsDamage>velocidad de ataque</style>, la <style=cIsUtility>velocidad de movimiento</style> " +
                "y la <style=cIsHealing>regeneración de salud</style>."
            );

            // Keyword propio de Miner. El paquete original también publica KEYWORD_CLEAVING,
            // pero su es-ES usa "Descomposición" mientras las descripciones de USU ya emplean
            // "Hendidura". Se unifica aquí el término visible en todas las superficies.
            RegisterSpanishToken(
                "KEYWORD_CLEAVING",
                "<style=cKeywordName>Hendidura</style><style=cSub>Aplica una desventaja acumulativa que reduce la " +
                "<style=cIsDamage>armadura</style> en <style=cIsHealth>3 por acumulación</style>.</style>"
            );

            // Primarias.
            RegisterSpanishToken("MINER_PRIMARY_GOUGE_NAME", "Tajo");
            RegisterSpanishSkillDescriptionToken(
                "MINER_PRIMARY_GOUGE_DESCRIPTION",
                "<style=cIsUtility>Ágil.</style> Ataca salvajemente a los enemigos cercanos e inflige " +
                "<style=cIsDamage>270% de daño</style>, aplicando <style=cIsHealth>Hendidura</style> a su armadura."
            );

            RegisterSpanishToken("MINER_PRIMARY_CRUSH_NAME", "Aplastar");
            RegisterSpanishSkillDescriptionToken(
                "MINER_PRIMARY_CRUSH_DESCRIPTION",
                "<style=cIsUtility>Ágil.</style> Aplasta a los enemigos cercanos e inflige " +
                "<style=cIsDamage>360% de daño</style>. <style=cIsUtility>El alcance aumenta con la velocidad de ataque</style>."
            );

            // Secundarias.
            RegisterSpanishToken("MINER_SECONDARY_CHARGE_NAME", "Carga perforadora");
            RegisterSpanishSkillDescriptionToken(
                "MINER_SECONDARY_CHARGE_DESCRIPTION",
                "Carga y embiste a los enemigos para infligir hasta <style=cIsDamage>6x180% de daño</style>. " +
                "<style=cIsUtility>No puedes recibir impactos durante la habilidad.</style>"
            );

            RegisterSpanishToken("MINER_SECONDARY_BREAK_NAME", "Martillo de fractura");
            RegisterSpanishSkillDescriptionToken(
                "MINER_SECONDARY_BREAK_DESCRIPTION",
                "Avanza rápidamente y explota al entrar en contacto con un enemigo, infligiendo " +
                "<style=cIsDamage>2x240% de daño</style>. <style=cIsUtility>No puedes recibir impactos durante la habilidad.</style>"
            );

            // Utilidades.
            RegisterSpanishToken("MINER_UTILITY_BACKBLAST_NAME", "Retroexplosión");
            RegisterSpanishSkillDescriptionToken(
                "MINER_UTILITY_BACKBLAST_DESCRIPTION",
                "<style=cIsDamage>Aturdidor.</style> Impúlsate hacia atrás y golpea a los enemigos cercanos por " +
                "<style=cIsDamage>600% de daño</style>. Mantén pulsado para recorrer más distancia. " +
                "<style=cIsUtility>No puedes recibir impactos durante la habilidad.</style>"
            );

            RegisterSpanishToken("MINER_UTILITY_CAVEIN_NAME", "Derrumbe");
            RegisterSpanishSkillDescriptionToken(
                "MINER_UTILITY_CAVEIN_DESCRIPTION",
                "<style=cIsUtility>Aturdidor.</style> Impúlsate hacia atrás y <style=cIsUtility>atrae</style> a todos " +
                "los enemigos dentro de un gran radio. <style=cIsUtility>No puedes recibir impactos durante la habilidad.</style>"
            );

            // Especiales.
            RegisterSpanishToken("MINER_SPECIAL_TOTHESTARSCLASSIC_NAME", "Hasta las estrellas");
            RegisterSpanishSkillDescriptionToken(
                "MINER_SPECIAL_TOTHESTARSCLASSIC_DESCRIPTION",
                "Salta por los aires y golpea a los enemigos justo debajo de ti por " +
                "<style=cIsDamage>6x300% de daño</style>."
            );

            RegisterSpanishToken("MINER_SPECIAL_TOTHESTARS_NAME", "Lluvia de meteoros");
            RegisterSpanishSkillDescriptionToken(
                "MINER_SPECIAL_TOTHESTARS_DESCRIPTION",
                "Salta por los aires y dispara una lluvia de metralla hacia abajo que inflige " +
                "<style=cIsDamage>15x160% de daño</style>."
            );

            // Skins.
            RegisterSpanishToken("MINERBODY_DEFAULT_SKIN_NAME", "Predeterminado");
            RegisterSpanishToken("MINERBODY_MOLTEN_SKIN_NAME", "Fundido");
            RegisterSpanishToken("MINERBODY_BLACKSMITH_SKIN_NAME", "Herrero");
            RegisterSpanishToken("MINERBODY_PUPLE_SKIN_NAME", "Puple");
            RegisterSpanishToken("MINERBODY_TUNDRA_SKIN_NAME", "Tundra");
            RegisterSpanishToken("MINERBODY_TYPHOON_SKIN_NAME", "Conquistador");

            // Desbloqueo ORIGINAL DEL SURVIVOR.
            // Importante: sólo se localizan los tokens. No se cambia AchievementDef,
            // UnlockableDef, tracker ni condición alguna del creador.
            RegisterSpanishToken("MINER_UNLOCKABLE_ACHIEVEMENT_NAME", "Subidón de adrenalina");
            RegisterSpanishToken(
                "MINER_UNLOCKABLE_ACHIEVEMENT_DESC",
                "Abre un Cofre legendario."
            );

            // Desbloqueos originales de habilidades.
            RegisterSpanishToken("MINER_CRUSHUNLOCKABLE_ACHIEVEMENT_NAME", "Minero: Oro saqueado");
            RegisterSpanishToken(
                "MINER_CRUSHUNLOCKABLE_ACHIEVEMENT_DESC",
                "Como Minero, aplica 12 acumulaciones de Hendidura al guardián único de Costa Dorada."
            );

            RegisterSpanishToken("MINER_CRACKUNLOCKABLE_ACHIEVEMENT_NAME", "Minero: Adicto");
            RegisterSpanishToken(
                "MINER_CRACKUNLOCKABLE_ACHIEVEMENT_DESC",
                "Como Minero, consigue 50 acumulaciones de Adrenalina."
            );

            RegisterSpanishToken("MINER_CAVEINUNLOCKABLE_ACHIEVEMENT_NAME", "Minero: Compactado");
            RegisterSpanishToken(
                "MINER_CAVEINUNLOCKABLE_ACHIEVEMENT_DESC",
                "Como Minero, golpea a 20 enemigos con una sola Retroexplosión."
            );

            // Desbloqueos originales de skins.
            RegisterSpanishToken("MINER_MONSOONUNLOCKABLE_ACHIEVEMENT_NAME", "Minero: Maestría");
            RegisterSpanishToken(
                "MINER_MONSOONUNLOCKABLE_ACHIEVEMENT_DESC",
                "Como Minero, completa el juego u oblítérate en Monzón."
            );

            RegisterSpanishToken("MINER_BLACKSMITHUNLOCKABLE_ACHIEVEMENT_NAME", "Minero: Templado");
            RegisterSpanishToken(
                "MINER_BLACKSMITHUNLOCKABLE_ACHIEVEMENT_DESC",
                "Como Minero, encuentra una herramienta de herrería en el planeta."
            );

            RegisterSpanishToken("MINER_PUPLEUNLOCKABLE_ACHIEVEMENT_NAME", "Minero: Las cinco llaves");
            RegisterSpanishToken(
                "MINER_PUPLEUNLOCKABLE_ACHIEVEMENT_DESC",
                "Como Minero, descubre el secreto de depuración de G-N-O-M-E."
            );

            RegisterSpanishToken("MINER_TUNDRAUNLOCKABLE_ACHIEVEMENT_NAME", "Minero: Congelado");
            RegisterSpanishToken(
                "MINER_TUNDRAUNLOCKABLE_ACHIEVEMENT_DESC",
                "Como Minero, llega a Punto de Reunión Delta en menos de 8 minutos."
            );

            RegisterSpanishToken("MINER_TYPHOONUNLOCKABLE_ACHIEVEMENT_NAME", "Minero: Gran maestría");
            RegisterSpanishToken(
                "MINER_TYPHOONUNLOCKABLE_ACHIEVEMENT_DESC",
                "Como Minero, completa el juego u oblítérate en Tifón o Eclipse.\n" +
                "<color=#8888>(La dificultad Tifón requiere Starstorm 2)</color>"
            );
        }

        private static void RegisterSpanishSkillDescriptionToken(
            string token,
            string spanish
        )
        {
            RegisterSpanishToken(token, FormatLongSkillText(spanish));
        }

        private static void RegisterSpanishToken(
            string token,
            string spanish
        )
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(spanish))
            {
                return;
            }

            LanguageAPI.Add(token, spanish, "es-419");
            LanguageAPI.Add(token, spanish, "es-ES");
        }

        private static bool IsSpanishLanguage()
        {
            string language = Language.currentLanguageName ?? "";
            return language.StartsWith("es", StringComparison.OrdinalIgnoreCase);
        }
    }
}
