using System;
using System.Collections;
using System.Reflection;

using BepInEx.Logging;
using R2API;
using RoR2;
using RoR2.Skills;
using RoR2.UI;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Integración curada de HUNK 1.5.1.
    ///
    /// El mod no resuelve localización española en el perfil es-419 auditado,
    /// por lo que USU aporta ES-419 / es-ES para la superficie visible que el
    /// juego expone a través de SurvivorDef, SkillDef, skins y achievements.
    ///
    /// El lore es contenido nativo del creador: USU conserva el inglés original
    /// y registra únicamente su traducción fiel al español. Las Notas continúan
    /// dependiendo del estado real del Logbook/Diario.
    ///
    /// La misión oficial USU "La Parca No Falla" permanece en el backend legacy.
    /// Esta Definition no modifica providers, trackers, multiplayer ni HunkMod.dll.
    /// </summary>
    public sealed class HunkDefinition : SurvivorDefinition
    {
        private static bool hunkLoadoutPanelHookInstalled;
        private static bool hunkVirusPanelFixLogged;
        private static ManualLogSource hunkRuntimeLogger;
        private static bool hunkLiveVirusUiFallbackInstalled;
        private static bool hunkLiveVirusUiFixLogged;
        private static int hunkLiveVirusUiFrameCounter;

        public override string SourceIdentifier
        {
            get { return "com.rob.Hunk"; }
        }

        public override string BodyName
        {
            get { return "RobHunkBody"; }
        }

        public override string DefinitionName
        {
            get { return "HUNK"; }
        }

        private static readonly string HunkSpanishDescription =
            "HUNK es un infiltrador de élite que porta un gran arsenal de armas obtenidas mediante " +
            "OSP (aprovisionamiento en el sitio).<color=#CCD3E0>\r\n\r\n" +
            "< ! > Usa tu Cuchillo de combate contra enemigos más débiles para conservar munición.\r\n\r\n" +
            "< ! > Asegurar la Muestra del virus G te otorgará una tarjeta, permitiéndote acceder a cajas de armas en cada escenario.\r\n\r\n" +
            "< ! > Usa Paso rápido cerca de un enemigo que ataque o de un proyectil para realizar una Esquiva perfecta; después, ejecuta un Contraataque con la Primaria.\r\n\r\n" +
            "< ! > La munición es escasa, pero puede encontrarse en cofres abiertos. Las cargas de Secundaria y la reducción de enfriamiento también afectan a la munición.\r\n\r\n" +
            "< ! > Esto es la guerra. Sobrevivir es tu responsabilidad.\r\n\r\n";

        // Traducción fiel del lore nativo del creador. No se reemplaza el inglés.
        private static readonly string HunkSpanishLore =
            "**INT. INSTALACIÓN ABANDONADA DE UMBRELLA - NOCHE**\n\n\n" +
            "(HUNK, el operativo silencioso y letal, está inmerso en un feroz combate contra una enorme A.B.O. " +
            "(Arma Bio-Orgánica). Esquiva hábilmente un zarpazo de la gigantesca garra de la criatura, rueda hasta " +
            "ponerse de pie y apunta con su subfusil. La A.B.O. ruge y carga contra HUNK sin contenerse.)\n\n" +
            "HUNK: (gruñe) Otro día más en la oficina.\n\n" +
            "(HUNK saca una granada cegadora y la lanza con precisión al rostro de la A.B.O. La criatura retrocede, " +
            "cegada por la intensa luz. Aprovechando el momento, HUNK se abalanza sobre ella y trepa por su pierna con " +
            "una agilidad sorprendente. Al llegar a la zona media de la criatura, desenfunda su cuchillo de combate.)\n\n" +
            "(La cámara enfoca de cerca el rostro horrorizado de la A.B.O. mientras se oye un fuerte «¡corte!».)\n\n" +
            "A.B.O.: ¡¡¡RAAAAGHHHHH!!!\n\n" +
            "(Suena un silbato deslizante cómico mientras la A.B.O. se desploma, agarrándose la entrepierna. La pantalla " +
            "tiembla con el impacto y escuchamos una serie de efectos exagerados: la bocina de un automóvil, el maullido " +
            "de un gato y cristales rompiéndose.)\n\n" +
            "SPECTRE (por radio): (Se estremece) Creo que hasta yo sentí eso.\n\n" +
            "(La cámara vuelve a HUNK, que mira directamente al objetivo con su característica mirada fría.)\n\n" +
            "HUNK: (impasible) Y decían que nunca daría la talla.\n\n" +
            "(La A.B.O. gime y rueda por el suelo dolorida. Se oye el clásico sonido de «boing».)\n\n" +
            "A.B.O.: ¡¡¡MIS... MIS MÓDULOS REGENERADORES!!!\n\n" +
            "(Corte al COMANDANTE observando desde el centro de mando, con una expresión a medio camino entre la diversión " +
            "y la exasperación.)\n\n" +
            "COMANDANTE: (suspira) No importa la misión, HUNK siempre sabe dónde golpear para que duela.";

        public override void RegisterInstalledLocalization(
            SurvivorInfo survivor,
            ManualLogSource logger
        )
        {
            if (survivor == null || !Matches(survivor))
            {
                return;
            }

            RegisterHunkSpanishTokens();
            RegisterHunkKeywordLiteralFallbacks();
            RegisterHunkKeywordTranslations(logger);
            RegisterHunkExactVirusKeywordTokens(logger);
            // HUNK usa PassiveSkill.keywordToken con ROB_HUNK_KEYWORD_VIRUS.
            // No instalamos el antiguo fallback de UI: el token combinado se localiza directamente.

            logger?.LogInfo(
                "[LOCALIZATION] Definition activa | RobHunkBody | " +
                "es-419 / es-ES + vista general + lore nativo traducido + skills + loadout HUNK + artefactos + cuchillos + skins + achievements"
            );
        }

        public override string ResolveSubtitle(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            return IsSpanishLanguage()
                ? "La Parca"
                : fallback ?? "";
        }

        public override string ResolveDescription(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            return IsSpanishLanguage()
                ? HunkSpanishDescription
                : fallback ?? "";
        }

        public override string ResolveLore(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            return IsSpanishLanguage()
                ? HunkSpanishLore
                : fallback ?? "";
        }

        private static void RegisterHunkSpanishTokens()
        {
            // ---------------------------------------------------------
            // IDENTIDAD / VISTA GENERAL / LORE
            // ---------------------------------------------------------
            RegisterSpanishToken("ROB_HUNK_BODY_SUBTITLE", "La Parca");
            RegisterSpanishToken("ROB_HUNK_BODY_DESCRIPTION", HunkSpanishDescription);

            // SurvivorContentProfile deriva ROB_HUNK_BODY_LORE a partir de
            // ROB_HUNK_BODY_NAME cuando el SurvivorDef no publica loreToken.
            RegisterSpanishToken("ROB_HUNK_BODY_LORE", HunkSpanishLore);

            RegisterHunkArtifactSpanishTokens();

            // ---------------------------------------------------------
            // PASIVA PRINCIPAL
            // ---------------------------------------------------------
            RegisterSpanishToken(
                "ROB_HUNK_BODY_PPASSIVE_NAME",
                "OSP (Aprovisionamiento en el sitio)"
            );
            RegisterSpanishSkillDescriptionToken(
                "ROB_HUNK_BODY_PPASSIVE_DESCRIPTION",
                "HUNK puede reunir <color=#FFFF00>Tarjetas U.C.</color> para abrir " +
                "<style=cIsDamage>cajas de armas</style> y ampliar su arsenal."
            );

            // ---------------------------------------------------------
            // HABILIDADES BASE
            // ---------------------------------------------------------
            RegisterSpanishToken("ROB_HUNK_BODY_PRIMARY_KNIFE_NAME", "Cuchillo de combate");
            RegisterSpanishSkillDescriptionToken(
                "ROB_HUNK_BODY_PRIMARY_KNIFE_DESCRIPTION",
                "<style=cIsUtility>Saqueo.</style> <style=cIsDamage>Acuchilla</style> a combatientes a corta distancia " +
                "e inflige <style=cIsDamage>200% de daño</style>, aplicando <style=cIsHealth>Mutilado</style>."
            );

            RegisterSpanishToken(
                "ROB_HUNK_BODY_PRIMARY_CQC_NAME",
                "CQC (Combate cuerpo a cuerpo)"
            );
            RegisterSpanishSkillDescriptionToken(
                "ROB_HUNK_BODY_PRIMARY_CQC_DESCRIPTION",
                "<style=cIsUtility>Saqueo.</style> <style=cIsDamage>Patea dos veces</style> e inflige " +
                "<style=cIsDamage>200% y 250% de daño</style>. Si conecta la <style=cIsUtility>segunda patada</style>, " +
                "continúa con un <style=cIsDamage>Contraataque</style>."
            );

            RegisterSpanishToken("ROB_HUNK_BODY_SECONDARY_AIM_NAME", "Puntería firme");
            RegisterSpanishSkillDescriptionToken(
                "ROB_HUNK_BODY_SECONDARY_AIM_DESCRIPTION",
                "Apunta y <style=cIsUtility>expón los puntos débiles de los enemigos</style>. " +
                "<style=cIsDamage>Usar la Primaria mientras apuntas dispara el arma que tengas equipada.</style>"
            );

            RegisterSpanishToken("ROB_HUNK_BODY_UTILITY_DODGE_NAME", "Paso rápido");
            RegisterSpanishSkillDescriptionToken(
                "ROB_HUNK_BODY_UTILITY_DODGE_DESCRIPTION",
                "<style=cIsUtility>Desplázate</style> una corta distancia. Si lo usas de forma preventiva para " +
                "<style=cIsUtility>evitar un ataque</style>, realiza una <style=cIsDamage>Esquiva perfecta</style>."
            );

            RegisterSpanishToken("ROB_HUNK_BODY_UTILITY_PARRY_NAME", "Parada");
            RegisterSpanishSkillDescriptionToken(
                "ROB_HUNK_BODY_UTILITY_PARRY_DESCRIPTION",
                "<style=cIsUtility>Bloquea</style> un ataque, <style=cIsDamage>anulando</style> el daño y " +
                "<style=cIsUtility>reembolsando el enfriamiento</style>. Si lo usas de forma preventiva, realiza una " +
                "<style=cIsDamage>Parada perfecta</style>."
            );

            RegisterSpanishToken("ROB_HUNK_BODY_SPECIAL_SWAP_NAME", "Legion");
            RegisterSpanishSkillDescriptionToken(
                "ROB_HUNK_BODY_SPECIAL_SWAP_DESCRIPTION",
                "<style=cIsUtility>Cambia</style> a otra <style=cIsDamage>arma</style>. Pulsa brevemente para volver " +
                "a tu <style=cIsUtility>última arma equipada</style>."
            );

            // ---------------------------------------------------------
            // PASIVAS / MODOS DE CARGA (EXTRA 1)
            // ---------------------------------------------------------
            RegisterSpanishToken("ROB_HUNK_BODY_PASSIVE_NAME", "Lucha encarnizada");
            RegisterSpanishSkillDescriptionToken(
                "ROB_HUNK_BODY_PASSIVE_DESCRIPTION",
                "Empieza con una <style=cIsDamage>LE 5</style> y un <style=cIsDamage>arma secundaria</style>. " +
                "Puedes <style=cIsDamage>rebuscar</style> en <style=cIsUtility>cofres</style> para encontrar munición."
            );

            RegisterSpanishToken("ROB_HUNK_BODY_RANDOMPASSIVE_NAME", "Nueva partida+");
            RegisterSpanishSkillDescriptionToken(
                "ROB_HUNK_BODY_RANDOMPASSIVE_DESCRIPTION",
                "Empieza con una <style=cIsDamage>LE 5</style> y un <style=cIsDamage>arma secundaria</style>. " +
                "Puedes <style=cIsDamage>rebuscar</style> en <style=cIsUtility>cofres</style> para encontrar munición. " +
                "<style=cIsHealth>El contenido de todas las cajas de armas se vuelve aleatorio.</style>"
            );

            RegisterSpanishToken("ROB_HUNK_BODY_PASSIVE2_NAME", "Amenaza inminente");
            RegisterSpanishSkillDescriptionToken(
                "ROB_HUNK_BODY_PASSIVE2_DESCRIPTION",
                "Empieza con un <style=cIsDamage>arsenal completo</style>, <style=cIsHealth>PERO</style> ya no puedes " +
                "<style=cIsHealth>obtener más munición</style>."
            );

            RegisterSpanishToken("ROB_HUNK_BODY_SHOTGUNPASSIVE_NAME", "Autoridad suprema");
            RegisterSpanishSkillDescriptionToken(
                "ROB_HUNK_BODY_SHOTGUNPASSIVE_DESCRIPTION",
                "Empieza <style=cIsHealth>ÚNICAMENTE</style> con una <style=cIsDamage>W-870</style>. Puedes " +
                "<style=cIsDamage>rebuscar</style> en <style=cIsUtility>cofres</style> para encontrar munición."
            );

            RegisterSpanishToken("ROB_HUNK_BODY_FLAMERPASSIVE_NAME", "Piromanía");
            RegisterSpanishSkillDescriptionToken(
                "ROB_HUNK_BODY_FLAMERPASSIVE_DESCRIPTION",
                "Empieza <style=cIsHealth>ÚNICAMENTE</style> con un <style=cIsDamage>Lanzallamas químico</style>. " +
                "Puedes <style=cIsDamage>rebuscar</style> en <style=cIsUtility>cofres</style> para encontrar munición. " +
                "<style=cIsHealth>Sólo aparece UNA caja de armas por escenario.</style>"
            );

            RegisterSpanishToken("ROB_HUNK_BODY_SMGPASSIVE_NAME", "Mercenario definitivo");
            RegisterSpanishSkillDescriptionToken(
                "ROB_HUNK_BODY_SMGPASSIVE_DESCRIPTION",
                "Empieza <style=cIsHealth>ÚNICAMENTE</style> con una <style=cIsDamage>LE 5 completamente mejorada</style>. " +
                "Puedes <style=cIsDamage>rebuscar</style> en <style=cIsUtility>cofres</style> para encontrar munición. " +
                "La <style=cIsDamage>Especial</style> se sustituye por <color=#C80202>Modo Caos</color>."
            );

            RegisterSpanishToken("ROB_HUNK_BODY_WPASSIVE_NAME", "Saturación global completa");
            RegisterSpanishSkillDescriptionToken(
                "ROB_HUNK_BODY_WPASSIVE_DESCRIPTION",
                "Empieza <style=cIsHealth>ÚNICAMENTE</style> con un <style=cIsDamage>arma secundaria</style>. Puedes " +
                "<style=cIsDamage>rebuscar</style> en <style=cIsUtility>cofres</style> para encontrar munición. " +
                "La <style=cIsDamage>Utilidad</style> se sustituye por <color=#C80202>Uroboros</color>."
            );

            // ---------------------------------------------------------
            // ARMAS EXPUESTAS POR EL LOADOUT AUDITADO (EXTRA 2)
            // HUNK usa algunas descripciones inglesas como token literal.
            // Registrar exactamente esa cadena como token permite traducirla
            // sin referenciar HunkMod.dll.
            // ---------------------------------------------------------
            RegisterSpanishLiteralToken(
                "Solid <style=cIsDamage>all-rounder</style> with a <style=cIsUtility>good ammo economy</style>.",
                "Un arma <style=cIsDamage>equilibrada</style> con una <style=cIsUtility>buena eficiencia de munición</style>."
            );
            RegisterSpanishLiteralToken(
                "<style=cIsDamage>Heavy hits</style>, but <style=cIsHealth>poor ammo economy</style>. <style=cIsUtility>Comes with a built-in Laser Sight.</style>",
                "<style=cIsDamage>Golpes potentes</style>, pero con <style=cIsHealth>mala eficiencia de munición</style>. " +
                "<style=cIsUtility>Incluye una mira láser integrada.</style>"
            );
            RegisterSpanishLiteralToken(
                "<style=cIsDamage>Medium damage</style> with an average <style=cIsUtility>ammo economy</style>.",
                "<style=cIsDamage>Daño medio</style> con una <style=cIsUtility>eficiencia de munición promedio</style>."
            );
            RegisterSpanishLiteralToken(
                "<style=cIsDamage>Unlimited ammo</style>, but <style=cIsHealth>pitiful damage</style>.",
                "<style=cIsDamage>Munición ilimitada</style>, pero <style=cIsHealth>daño lamentable</style>."
            );

            // Nombre propio de arma que sí se presenta como texto inglés.
            // En español de Resident Evil se conoce como Lanzadescargas.
            RegisterSpanishToken(
                "ROB_HUNK_WEAPON_SPARKSHOT_NAME",
                "Lanzadescargas"
            );

            // ---------------------------------------------------------
            // HERRAMIENTAS (EXTRA 3)
            // ---------------------------------------------------------
            RegisterSpanishToken("ROB_HUNK_WEAPON_SCANNER_NAME", "Escáner EMF");
            RegisterSpanishSkillDescriptionToken(
                "ROB_HUNK_WEAPON_SCANNER_DESC",
                "Un dispositivo capaz de localizar determinados aparatos electrónicos."
            );

            RegisterSpanishToken("ROB_HUNK_WEAPON_GRAPPLEGUN_NAME", "Gancho de agarre");
            RegisterSpanishSkillDescriptionToken(
                "ROB_HUNK_WEAPON_GRAPPLEGUN_DESC",
                "Dispara un gancho unido a una tirolina que te permite impulsarte hacia cualquier ubicación deseada."
            );

            RegisterSpanishToken("ROB_HUNK_JETPACK_NAME", "Mochila de salto experimental");
            RegisterSpanishSkillDescriptionToken(
                "ROB_HUNK_JETPACK_DESC",
                "Una herramienta fabricada por Umbrella que te permite impulsarte en cualquier dirección pulsando salto mientras estás en el aire."
            );

            RegisterSpanishToken("ROB_HUNK_SURVIVALKIT_NAME", "Kit de supervivencia de emergencia");
            RegisterSpanishSkillDescriptionToken(
                "ROB_HUNK_SURVIVALKIT_DESC",
                "Un kit de supervivencia que mejora tanto tu capacidad de rebuscar munición como la eficacia de tu cuchillo."
            );

            RegisterSpanishToken("ROB_HUNK_INFROCKET_NAME", "ATM-4");
            RegisterSpanishSkillDescriptionToken(
                "ROB_HUNK_INFROCKET_DESC",
                "Un lanzacohetes con <style=cIsHealing>munición infinita</style>. Recompensa del logro " +
                "<color=#FFFF00>Primer partidario</color>."
            );

            // ---------------------------------------------------------
            // VENTAJAS (EXTRA 4) — nombres/descripciones literales
            // ---------------------------------------------------------
            RegisterSpanishLiteralToken("No Perk", "Sin ventaja");
            RegisterSpanishLiteralToken("Play without a perk.", "Juega sin una ventaja.");

            RegisterSpanishLiteralToken("Marathon Runner", "Corredor de maratón");
            RegisterSpanishLiteralToken(
                "Sprint with <style=cIsUtility>incredible speed</style> after <style=cIsDamage>1 second</style>.",
                "Corre con <style=cIsUtility>una velocidad increíble</style> después de <style=cIsDamage>1 segundo</style>."
            );

            RegisterSpanishLiteralToken("Peak Physique", "Físico óptimo");
            RegisterSpanishLiteralToken(
                "Counterattacks '<style=cIsDamage>Critically Strike</style>' and restore <style=cIsHealing>5% max health</style>.",
                "Los Contraataques <style=cIsDamage>infligen golpes críticos</style> y restauran " +
                "<style=cIsHealing>5% de la salud máxima</style>."
            );

            RegisterSpanishLiteralToken("Evasive Expert", "Experto evasivo");
            RegisterSpanishLiteralToken(
                "Boost <style=cIsUtility>Perfect Dodge</style> window and range by <style=cIsDamage>50%</style> and <style=cIsHealing>invincibility</style> duration by <style=cIsDamage>100%</style>.",
                "Aumenta la ventana y el alcance de la <style=cIsUtility>Esquiva perfecta</style> un " +
                "<style=cIsDamage>50%</style> y la duración de la <style=cIsHealing>invencibilidad</style> un " +
                "<style=cIsDamage>100%</style>."
            );

            RegisterSpanishLiteralToken("Siege Ready", "Listo para el asedio");
            RegisterSpanishLiteralToken(
                "Boost <style=cIsUtility>reload speed</style> by <style=cIsDamage>75%</style> and <style=cIsUtility>weapon swap</style> speed by <style=cIsDamage>50%</style>.",
                "Aumenta la <style=cIsUtility>velocidad de recarga</style> un <style=cIsDamage>75%</style> y la " +
                "velocidad de <style=cIsUtility>cambio de arma</style> un <style=cIsDamage>50%</style>."
            );

            RegisterSpanishLiteralToken("Extreme Focus", "Concentración extrema");
            RegisterSpanishLiteralToken(
                "All <style=cIsDamage>ammo</style> picked up is for your <style=cIsUtility>currently equipped gun</style>.",
                "Toda la <style=cIsDamage>munición</style> que recojas será para tu " +
                "<style=cIsUtility>arma equipada actualmente</style>."
            );

            // ---------------------------------------------------------
            // CUCHILLOS (EXTRA 5)
            // ---------------------------------------------------------
            RegisterSpanishToken("ROB_HUNK_BODY_KNIFE_DEFAULT_NAME", "Cuchillo de combate");
            RegisterSpanishSkillDescriptionToken(
                "ROB_HUNK_BODY_KNIFE_DEFAULT_DESCRIPTION",
                "Un cuchillo estándar de grado militar. Seguro que resulta útil en un apuro."
            );

            RegisterSpanishToken("ROB_HUNK_BODY_KNIFE_BLOODY_NAME", "Hoja manchada");
            RegisterSpanishSkillDescriptionToken(
                "ROB_HUNK_BODY_KNIFE_BLOODY_DESCRIPTION",
                "Una hoja ensangrentada de filo serrado que ha desgarrado a numerosos oponentes."
            );

            RegisterSpanishToken("ROB_HUNK_BODY_KNIFE_HIDDEN_NAME", "Hoja oculta");
            RegisterSpanishSkillDescriptionToken(
                "ROB_HUNK_BODY_KNIFE_HIDDEN_DESCRIPTION",
                "Una hoja retráctil y discreta oculta en la manga. Ideal para operaciones de sigilo."
            );

            RegisterSpanishToken("ROB_HUNK_BODY_KNIFE_INFINITE_NAME", "Cuchillo de combate EX");
            RegisterSpanishSkillDescriptionToken(
                "ROB_HUNK_BODY_KNIFE_INFINITE_DESCRIPTION",
                "Un cuchillo militar tratado especialmente y endurecido hasta un grado increíble. Dicen que está hecho para durar de verdad."
            );

            RegisterSpanishToken("ROB_HUNK_BODY_KNIFE_WESKER_NAME", "Cuchillo personalizado");
            RegisterSpanishSkillDescriptionToken(
                "ROB_HUNK_BODY_KNIFE_WESKER_DESCRIPTION",
                "Una hoja feroz de filo serrado diseñada para facilitar el desgarro de tus oponentes."
            );

            RegisterSpanishToken("ROB_HUNK_BODY_KNIFE_MACHETE_NAME", "Machete");
            RegisterSpanishSkillDescriptionToken(
                "ROB_HUNK_BODY_KNIFE_MACHETE_DESCRIPTION",
                "Una enorme hoja destinada principalmente a tácticas de guerrilla."
            );

            RegisterSpanishToken("ROB_HUNK_BODY_KNIFE_RE4_NAME", "Cuchillo primigenio");
            RegisterSpanishSkillDescriptionToken(
                "ROB_HUNK_BODY_KNIFE_RE4_DESCRIPTION",
                "Un cuchillo forjado con posibilidades infinitas."
            );

            RegisterSpanishToken("ROB_HUNK_BODY_KNIFE_HOTDOGGER_NAME", "HOT DOGGER");
            RegisterSpanishSkillDescriptionToken(
                "ROB_HUNK_BODY_KNIFE_HOTDOGGER_DESCRIPTION",
                "Como su nombre indica, este cuchillo antiactivos biológicos desarrollado por Umbrella lo usan quienes disfrutan presumiendo."
            );

            RegisterSpanishToken("ROB_HUNK_BODY_KNIFE_STUNROD_NAME", "Vara aturdidora");
            RegisterSpanishSkillDescriptionToken(
                "ROB_HUNK_BODY_KNIFE_STUNROD_DESCRIPTION",
                "AVISO: En realidad no aturde."
            );

            RegisterSpanishToken("ROB_HUNK_BODY_KNIFE_KITCHEN_NAME", "Cuchillo de cocina");
            RegisterSpanishSkillDescriptionToken(
                "ROB_HUNK_BODY_KNIFE_KITCHEN_DESCRIPTION",
                "Un cuchillo hecho para cocinar. Su filo se mellará con facilidad."
            );

            // ---------------------------------------------------------
            // SKINS
            // ---------------------------------------------------------
            RegisterSpanishToken("ROB_HUNK_BODY_DEFAULT_SKIN_NAME", "Predeterminado");
            RegisterSpanishToken("ROB_HUNK_BODY_TOFU_SKIN_NAME", "Tofu");
            RegisterSpanishToken("ROB_HUNK_BODY_ENFORCER_SKIN_NAME", "Bulldozer");
            RegisterSpanishToken("ROB_HUNK_BODY_LIGHTWEIGHT_SKIN_NAME", "Ligero");
            RegisterSpanishToken("ROB_HUNK_BODY_GUERRILLA_SKIN_NAME", "Soldado");
            RegisterSpanishToken("ROB_HUNK_BODY_ROOKIE_SKIN_NAME", "Novato");
            RegisterSpanishToken("ROB_HUNK_BODY_SWAT_SKIN_NAME", "Pacificador");
            RegisterSpanishToken("ROB_HUNK_BODY_LEON_SKIN_NAME", "Especialista");
            RegisterSpanishToken("ROB_HUNK_BODY_LEONSHIRT_SKIN_NAME", "Especialista EX");
            RegisterSpanishToken(
                "ROB_HUNK_BODY_SUPER_SKIN_NAME",
                "<color=#FFFF00>Primer partidario</color>"
            );

            // ---------------------------------------------------------
            // ACHIEVEMENTS / UNLOCKS DEL CREADOR
            // ---------------------------------------------------------
            RegisterAchievement(
                "ACHIEVEMENT_ROB_HUNK_CQC_ACHIEVEMENT_ID_NAME",
                "HUNK: CQC (Combate cuerpo a cuerpo)",
                "ACHIEVEMENT_ROB_HUNK_CQC_ACHIEVEMENT_ID_DESCRIPTION",
                "Como HUNK, realiza 10 contraataques exitosos en un mismo escenario."
            );

            RegisterAchievement(
                "ACHIEVEMENT_ROB_HUNK_COMPLETION_UNLOCKABLE_ACHIEVEMENT_ID_NAME",
                "HUNK: La Parca",
                "ACHIEVEMENT_ROB_HUNK_COMPLETION_UNLOCKABLE_ACHIEVEMENT_ID_DESCRIPTION",
                "Como HUNK, termina el juego o sacrifícate."
            );

            RegisterAchievement(
                "ACHIEVEMENT_ROB_HUNK_SSG_ACHIEVEMENT_ID_NAME",
                "HUNK: Dominador",
                "ACHIEVEMENT_ROB_HUNK_SSG_ACHIEVEMENT_ID_DESCRIPTION",
                "Como HUNK, consigue la SSG."
            );

            RegisterAchievement(
                "ACHIEVEMENT_ROB_HUNK_FLAMER_ACHIEVEMENT_ID_NAME",
                "HUNK: Pirómano",
                "ACHIEVEMENT_ROB_HUNK_FLAMER_ACHIEVEMENT_ID_DESCRIPTION",
                "Como HUNK, mejora por completo el Lanzallamas químico."
            );

            RegisterAchievement(
                "ACHIEVEMENT_ROB_HUNK_JUMPPACK_ACHIEVEMENT_ID_NAME",
                "HUNK: Sólo queda subir",
                "ACHIEVEMENT_ROB_HUNK_JUMPPACK_ACHIEVEMENT_ID_DESCRIPTION",
                "Como HUNK, sé víctima de un accidente de paracaidismo."
            );

            RegisterAchievement(
                "ACHIEVEMENT_ROB_HUNK_SURVIVALKIT_ACHIEVEMENT_ID_NAME",
                "HUNK: Jodidamente sin suerte",
                "ACHIEVEMENT_ROB_HUNK_SURVIVALKIT_ACHIEVEMENT_ID_DESCRIPTION",
                "Como HUNK, agota toda tu munición en la Luna."
            );

            RegisterAchievement(
                "ACHIEVEMENT_ROB_HUNK_SUPPORTER_ACHIEVEMENT_ID_NAME",
                "HUNK: Primer partidario",
                "ACHIEVEMENT_ROB_HUNK_SUPPORTER_ACHIEVEMENT_ID_DESCRIPTION",
                "Juega con HUNK antes de su lanzamiento oficial."
            );

            RegisterAchievement(
                "ACHIEVEMENT_ROB_HUNK_MONSOON_UNLOCKABLE_ACHIEVEMENT_ID_NAME",
                "HUNK: Maestría",
                "ACHIEVEMENT_ROB_HUNK_MONSOON_UNLOCKABLE_ACHIEVEMENT_ID_DESCRIPTION",
                "Como HUNK, termina el juego o sacrifícate en Monzón."
            );

            RegisterAchievement(
                "ACHIEVEMENT_ROB_HUNK_INFINITEKNIFE_ACHIEVEMENT_ID_NAME",
                "HUNK: As de picas",
                "ACHIEVEMENT_ROB_HUNK_INFINITEKNIFE_ACHIEVEMENT_ID_DESCRIPTION",
                "Como HUNK, lleva dos Tarjetas de Picas al mismo tiempo."
            );

            RegisterAchievement(
                "ACHIEVEMENT_ROB_HUNK_WESKERKNIFE_UNLOCKABLE_ACHIEVEMENT_ID_NAME",
                "HUNK: Entusiasta de las armas de fuego",
                "ACHIEVEMENT_ROB_HUNK_WESKERKNIFE_UNLOCKABLE_ACHIEVEMENT_ID_DESCRIPTION",
                "Como HUNK, mejora un arma con un accesorio nuevo."
            );

            RegisterAchievement(
                "ACHIEVEMENT_ROB_HUNK_MACHETE_UNLOCKABLE_ACHIEVEMENT_ID_NAME",
                "HUNK: No necesito ninguna maldita arma",
                "ACHIEVEMENT_ROB_HUNK_MACHETE_UNLOCKABLE_ACHIEVEMENT_ID_DESCRIPTION",
                "Como HUNK, completa un escenario sin disparar ninguna arma de fuego."
            );

            RegisterAchievement(
                "ACHIEVEMENT_ROB_HUNK_RE4KNIFE_ACHIEVEMENT_ID_NAME",
                "HUNK: El camino por delante",
                "ACHIEVEMENT_ROB_HUNK_RE4KNIFE_ACHIEVEMENT_ID_DESCRIPTION",
                "Como HUNK, llega al escenario 6."
            );

            RegisterAchievement(
                "ACHIEVEMENT_ROB_HUNK_HOTDOGGER_ACHIEVEMENT_ID_NAME",
                "HUNK: Química básica",
                "ACHIEVEMENT_ROB_HUNK_HOTDOGGER_ACHIEVEMENT_ID_DESCRIPTION",
                "Como HUNK, consigue el Lanzallamas químico."
            );

            RegisterAchievement(
                "ACHIEVEMENT_ROB_HUNK_STUNROD_ACHIEVEMENT_ID_NAME",
                "HUNK: Oferta de tres por uno",
                "ACHIEVEMENT_ROB_HUNK_STUNROD_ACHIEVEMENT_ID_DESCRIPTION",
                "Como HUNK, consigue el Lanzadescargas."
            );

            RegisterAchievement(
                "ACHIEVEMENT_ROB_HUNK_TYPHOON_UNLOCKABLE_ACHIEVEMENT_ID_NAME",
                "HUNK: Gran Maestría",
                "ACHIEVEMENT_ROB_HUNK_TYPHOON_UNLOCKABLE_ACHIEVEMENT_ID_DESCRIPTION",
                "Como HUNK, termina el juego o sacrifícate en Tifón o Eclipse.\n" +
                "<color=#8888>(Cuenta cualquier dificultad Tifón o superior)</color>"
            );

            RegisterAchievement(
                "ACHIEVEMENT_ROB_HUNK_LIGHTWEIGHT_ACHIEVEMENT_ID_NAME",
                "HUNK: Minimalista",
                "ACHIEVEMENT_ROB_HUNK_LIGHTWEIGHT_ACHIEVEMENT_ID_DESCRIPTION",
                "Como HUNK, completa el escenario 1 sin objetos.\n" +
                "<color=#8888>(Las armas y herramientas no cuentan en tu contra)</color>"
            );

            RegisterAchievement(
                "ACHIEVEMENT_ROB_HUNK_GUERRILLA_ACHIEVEMENT_ID_NAME",
                "HUNK: Armado y listo",
                "ACHIEVEMENT_ROB_HUNK_GUERRILLA_ACHIEVEMENT_ID_DESCRIPTION",
                "Como HUNK, consigue un arsenal completo."
            );

            RegisterAchievement(
                "ACHIEVEMENT_ROB_HUNK_ROOKIE_ACHIEVEMENT_ID_NAME",
                "HUNK: Primer día de trabajo",
                "ACHIEVEMENT_ROB_HUNK_ROOKIE_ACHIEVEMENT_ID_DESCRIPTION",
                "Como HUNK, consigue un arma nueva."
            );

            RegisterAchievement(
                "ACHIEVEMENT_ROB_HUNK_SWAT_ACHIEVEMENT_ID_NAME",
                "HUNK: Guardián de la paz",
                "ACHIEVEMENT_ROB_HUNK_SWAT_ACHIEVEMENT_ID_DESCRIPTION",
                "Como HUNK, mejora por completo la MUP."
            );
        }

        // ---------------------------------------------------------
        // ARTEFACTOS PROPIOS DE HUNK
        // ---------------------------------------------------------
        // Estos ArtifactDef pertenecen al ContentPack com.rob.Hunk y
        // exponen tokens normales de Language. Los registramos directamente
        // en ES para que la pantalla de Artefactos no dependa del fallback
        // visual del Loadout.
        private static void RegisterHunkArtifactSpanishTokens()
        {
            RegisterSpanishToken(
                "ROB_ARTIFACT_PURSUER_NAME",
                "Artefacto del Perseguidor"
            );
            RegisterSpanishToken(
                "ROB_ARTIFACT_PURSUER_DESC",
                "Los jugadores comienzan la partida con una Placa S.T.A.R.S."
            );

            RegisterSpanishToken(
                "ROB_ARTIFACT_CONTAMINATION_NAME",
                "Artefacto de la Contaminación"
            );
            RegisterSpanishToken(
                "ROB_ARTIFACT_CONTAMINATION_DESC",
                "Cada jugador recibe un evento de Virus aleatorio en cada escenario."
            );

            // El artefacto del Perseguidor entrega este objeto al comenzar.
            // Localizamos al menos su nombre para que el término sea coherente
            // cuando aparezca en otras superficies del mod.
            RegisterSpanishToken(
                "ROB_HUNK_STARSBADGE_NAME",
                "Placa S.T.A.R.S."
            );
        }

        /// <summary>
        /// HunkMod.dll 1.5.1 define los tres tooltips de Virus mediante
        /// tokens propios que NO forman parte de los keywordTokens de los
        /// SkillDef auditados. Registrarlos por su identificador real evita
        /// modificar texto visible por frame y respeta el sistema Language.
        ///
        /// Tokens confirmados directamente en HunkMod.dll:
        /// ROB_HUNK_KEYWORD_GVIRUS
        /// ROB_HUNK_KEYWORD_TVIRUS
        /// ROB_HUNK_KEYWORD_CVIRUS
        /// </summary>
        private static void RegisterHunkExactVirusKeywordTokens(
            ManualLogSource logger
        )
        {
            // HunkMod.dll 1.5.1 construye el tooltip que vemos en OSP con un
            // PassiveSkill.keywordToken singular. En CreateSkills() asigna
            // exactamente ROB_HUNK_KEYWORD_VIRUS, y en AddTokens() su valor
            // es la concatenación G + "\n\n" + T + "\n\n" + C.
            // Por eso traducir sólo GVIRUS/TVIRUS/CVIRUS nunca afectaba ese panel.
            RegisterSpanishToken(
                "ROB_HUNK_KEYWORD_VIRUS",
                "<style=cKeywordName>G-Virus</style>" +
                "<style=cSub>Una vez por escenario, un monstruo aleatorio se vuelve " +
                "<color=#C678B4>Infectado</color>. Muta, " +
                "<style=cIsHealth>haciéndose más fuerte con el tiempo</style>, y al matarlo " +
                "suelta una <color=#FFFF00>Muestra de G-Virus</color>.</style>" +
                "\n\n" +
                "<style=cKeywordName>T-Virus</style>" +
                "<style=cSub>Una vez por escenario, varios monstruos se vuelven " +
                "<color=#0066FF>Infectados</color>. Esto los " +
                "<style=cIsHealth>revive</style> dos veces, y matar a todos te otorga " +
                "una <color=#FFFF00>Muestra de T-Virus</color>.</style>" +
                "\n\n" +
                "<style=cKeywordName>C-Virus</style>" +
                "<style=cSub>Una vez por escenario, varios monstruos se vuelven " +
                "<color=#FF0033>Infectados</color>. Al morir, estos monstruos entran en " +
                "<style=cIsDamage>frenesí</style> durante <style=cIsUtility>5 segundos</style> " +
                "y luego <style=cIsHealth>explotan</style>; matar a todos te otorga una " +
                "<color=#FFFF00>Muestra de C-Virus</color>.</style>"
            );

            // Se conservan también los tres tokens individuales porque el DLL
            // los registra y pueden aparecer en otras superficies del mod.
            RegisterSpanishToken(
                "ROB_HUNK_KEYWORD_GVIRUS",
                "<style=cKeywordName>G-Virus</style>" +
                "<style=cSub>Una vez por escenario, un monstruo aleatorio se vuelve " +
                "<color=#C678B4>Infectado</color>. Muta, " +
                "<style=cIsHealth>haciéndose más fuerte con el tiempo</style>, y al matarlo " +
                "suelta una <color=#FFFF00>Muestra de G-Virus</color>.</style>"
            );

            RegisterSpanishToken(
                "ROB_HUNK_KEYWORD_TVIRUS",
                "<style=cKeywordName>T-Virus</style>" +
                "<style=cSub>Una vez por escenario, varios monstruos se vuelven " +
                "<color=#0066FF>Infectados</color>. Esto los " +
                "<style=cIsHealth>revive</style> dos veces, y matar a todos te otorga " +
                "una <color=#FFFF00>Muestra de T-Virus</color>.</style>"
            );

            RegisterSpanishToken(
                "ROB_HUNK_KEYWORD_CVIRUS",
                "<style=cKeywordName>C-Virus</style>" +
                "<style=cSub>Una vez por escenario, varios monstruos se vuelven " +
                "<color=#FF0033>Infectados</color>. Al morir, estos monstruos entran en " +
                "<style=cIsDamage>frenesí</style> durante <style=cIsUtility>5 segundos</style> " +
                "y luego <style=cIsHealth>explotan</style>; matar a todos te otorga una " +
                "<color=#FFFF00>Muestra de C-Virus</color>.</style>"
            );

            logger?.LogInfo(
                "[HUNK LOCALIZATION] Tokens reales del DLL registrados | " +
                "ROB_HUNK_KEYWORD_VIRUS (combinado) + GVIRUS / TVIRUS / CVIRUS individuales."
            );
        }

        // =========================================================
        // KEYWORDS / PANELES LATERALES DEL LOADOUT DE HUNK
        // =========================================================
        /// <summary>
        /// Fallback para el panel personalizado de HUNK. Algunas entradas no
        /// están expuestas como keywordTokens de SkillDef y el mod utiliza
        /// cadenas literales como tokens. Registrarlas aquí cubre esa segunda
        /// ruta sin referencia directa a HunkMod.dll.
        ///
        /// Importante: NO añadimos corchetes. El panel de HUNK los dibuja por
        /// su cuenta; añadirlos aquí produce [[ Nombre ]].
        /// </summary>
        private static void RegisterHunkKeywordLiteralFallbacks()
        {
            RegisterSpanishLiteralToken("Perfect Dodge", "Esquiva perfecta");
            RegisterSpanishLiteralToken("Counterattack", "Contraataque");
            RegisterSpanishLiteralToken("Looting", "Saqueo");
            RegisterSpanishLiteralToken("Mangled", "Mutilado");
            RegisterSpanishLiteralToken("Perfect Parry", "Parada perfecta");

            RegisterSpanishLiteralToken(
                "Refund cooldown and become briefly invulnerable. Allows you to perform a Counterattack.",
                "Reembolsa el enfriamiento y te vuelve brevemente invulnerable. Te permite realizar un Contraataque."
            );
            RegisterSpanishLiteralToken(
                "Pressing primary lunges and performs a lethal counterattack on a nearby enemy.",
                "Al pulsar la Primaria, te abalanzas y ejecutas un contraataque letal contra un enemigo cercano."
            );
            RegisterSpanishLiteralToken(
                "Enemies slain with this skill have a small chance to drop ammo.",
                "Los enemigos abatidos con esta habilidad tienen una pequeña probabilidad de soltar munición."
            );
            RegisterSpanishLiteralToken(
                "Upon reaching 6 stacks of this debuff, enemies suffer an instant 2200% damage.",
                "Al alcanzar 6 acumulaciones de este perjuicio, los enemigos sufren al instante 2200% de daño."
            );

            RegisterSpanishLiteralToken("G-Virus", "G-Virus");
            RegisterSpanishLiteralToken("T-Virus", "T-Virus");
            RegisterSpanishLiteralToken("C-Virus", "C-Virus");
            RegisterSpanishLiteralToken(
                "Once every stage, a random monster becomes Infected. It mutates, growing stronger over time, and killing it drops a G-Virus Sample.",
                "Una vez por escenario, un monstruo aleatorio se vuelve Infectado. Muta, haciéndose más fuerte con el tiempo, y al matarlo suelta una Muestra de G-Virus."
            );
            RegisterSpanishLiteralToken(
                "Once every stage, several monsters become Infected. This revives them twice, and killing all of them nets you a T-Virus Sample.",
                "Una vez por escenario, varios monstruos se vuelven Infectados. Esto los revive dos veces, y matar a todos te otorga una Muestra de T-Virus."
            );
            RegisterSpanishLiteralToken(
                "Once every stage, several monsters become Infected. Upon death these monsters go on a frenzy for 5 seconds and then explode, and killing all of them nets you a C-Virus Sample.",
                "Una vez por escenario, varios monstruos se vuelven Infectados. Al morir entran en frenesí durante 5 segundos y luego explotan; matar a todos te otorga una Muestra de C-Virus."
            );
        }


        /// <summary>
        /// HUNK publica varios keywordTokens propios que el auditor genérico
        /// no mostraba por nombre (Perfect Dodge, Counterattack, Looting,
        /// Mangled y los Virus). Recorremos únicamente SkillDefs de HUNK,
        /// inspeccionamos sus keywordTokens y añadimos ES al token REAL que
        /// utiliza el creador. Así no necesitamos referenciar HunkMod.dll ni
        /// adivinar los identificadores internos.
        /// </summary>
        private static void RegisterHunkKeywordTranslations(
            ManualLogSource logger
        )
        {
            try
            {
                object definitions =
                    ReadStaticMember(
                        typeof(SkillCatalog),
                        "allSkillDefs"
                    ) ??
                    ReadStaticMember(
                        typeof(SkillCatalog),
                        "skillDefs"
                    );

                IEnumerable enumerable =
                    definitions as IEnumerable;

                if (enumerable == null)
                {
                    logger?.LogWarning(
                        "[HUNK LOCALIZATION] No se pudo enumerar SkillCatalog para localizar keywordTokens."
                    );

                    return;
                }

                int translated = 0;

                foreach (object rawSkill in enumerable)
                {
                    if (rawSkill == null)
                    {
                        continue;
                    }

                    string skillNameToken =
                        ReadStringMember(
                            rawSkill,
                            "skillNameToken"
                        );

                    string descriptionToken =
                        ReadStringMember(
                            rawSkill,
                            "skillDescriptionToken"
                        );

                    string internalName =
                        ReadStringMember(
                            rawSkill,
                            "name"
                        );

                    if (
                        !LooksLikeHunkSkill(
                            skillNameToken,
                            descriptionToken,
                            internalName
                        )
                    )
                    {
                        continue;
                    }

                    object keywords =
                        ReadMember(
                            rawSkill,
                            "keywordTokens"
                        );

                    IEnumerable keywordEnumerable =
                        keywords as IEnumerable;

                    if (keywordEnumerable == null)
                    {
                        continue;
                    }

                    foreach (object rawKeyword in keywordEnumerable)
                    {
                        string keywordToken =
                            rawKeyword as string;

                        if (string.IsNullOrWhiteSpace(keywordToken))
                        {
                            continue;
                        }

                        string resolved =
                            Language.GetString(
                                keywordToken
                            ) ?? "";

                        string spanish =
                            ResolveHunkKeywordSpanish(
                                keywordToken,
                                resolved
                            );

                        if (spanish == null)
                        {
                            continue;
                        }

                        RegisterSpanishToken(
                            keywordToken,
                            FormatLongSkillText(spanish)
                        );

                        translated++;
                    }
                }

                logger?.LogInfo(
                    "[HUNK LOCALIZATION] keywordTokens localizados dinámicamente: " +
                    translated + "."
                );
            }
            catch (Exception exception)
            {
                logger?.LogWarning(
                    "[HUNK LOCALIZATION] No se pudieron localizar los keywordTokens dinámicos. " +
                    exception.GetType().Name +
                    ": " +
                    exception.Message
                );
            }
        }

        private static bool LooksLikeHunkSkill(
            string nameToken,
            string descriptionToken,
            string internalName
        )
        {
            return
                ContainsIgnoreCase(nameToken, "ROB_HUNK") ||
                ContainsIgnoreCase(descriptionToken, "ROB_HUNK") ||
                ContainsIgnoreCase(internalName, "Hunk");
        }

        private static string ResolveHunkKeywordSpanish(
            string token,
            string resolved
        )
        {
            string source =
                (token ?? "") +
                "\n" +
                (resolved ?? "");

            if (
                ContainsIgnoreCase(source, "Perfect Dodge") &&
                ContainsIgnoreCase(source, "Refund cooldown")
            )
            {
                return
                    "<style=cKeywordName>Esquiva perfecta</style>" +
                    "<style=cSub>Reembolsa el enfriamiento y te vuelve brevemente " +
                    "<style=cIsUtility>invulnerable</style>. Te permite realizar un " +
                    "<style=cIsDamage>Contraataque</style>.</style>";
            }

            if (
                ContainsIgnoreCase(source, "Counterattack")
            )
            {
                return
                    "<style=cKeywordName>Contraataque</style>" +
                    "<style=cSub>Al pulsar la <style=cIsDamage>Primaria</style>, te abalanzas " +
                    "y ejecutas un <style=cIsDamage>contraataque letal</style> contra un enemigo cercano.</style>";
            }

            if (
                ContainsIgnoreCase(source, "Looting") &&
                ContainsIgnoreCase(source, "drop ammo")
            )
            {
                return
                    "<style=cKeywordName>Saqueo</style>" +
                    "<style=cSub>Los enemigos abatidos con esta habilidad tienen una " +
                    "pequeña probabilidad de <style=cIsUtility>soltar munición</style>.</style>";
            }

            if (
                ContainsIgnoreCase(source, "Mangled") &&
                ContainsIgnoreCase(source, "6 stacks")
            )
            {
                return
                    "<style=cKeywordName>Mutilado</style>" +
                    "<style=cSub>Al alcanzar <style=cIsUtility>6 acumulaciones</style> de este perjuicio, " +
                    "los enemigos sufren al instante <style=cIsDamage>2200% de daño</style>.</style>";
            }

            if (
                ContainsIgnoreCase(source, "G-Virus")
            )
            {
                return
                    "<style=cKeywordName>G-Virus</style>" +
                    "<style=cSub>Una vez por escenario, un monstruo aleatorio se vuelve " +
                    "<style=cIsHealth>Infectado</style>. Muta, haciéndose más fuerte con el tiempo, " +
                    "y al matarlo suelta una <style=cIsDamage>Muestra de G-Virus</style>.</style>";
            }

            if (
                ContainsIgnoreCase(source, "T-Virus")
            )
            {
                return
                    "<style=cKeywordName>T-Virus</style>" +
                    "<style=cSub>Una vez por escenario, varios monstruos se vuelven " +
                    "<style=cIsHealth>Infectados</style>. Esto los revive dos veces, y matar a todos " +
                    "te otorga una <style=cIsDamage>Muestra de T-Virus</style>.</style>";
            }

            if (
                ContainsIgnoreCase(source, "C-Virus")
            )
            {
                return
                    "<style=cKeywordName>C-Virus</style>" +
                    "<style=cSub>Una vez por escenario, varios monstruos se vuelven " +
                    "<style=cIsHealth>Infectados</style>. Al morir entran en frenesí durante " +
                    "<style=cIsUtility>5 segundos</style> y luego explotan; matar a todos te otorga " +
                    "una <style=cIsDamage>Muestra de C-Virus</style>.</style>";
            }

            // Parry se añadió después de los textos base y también utiliza
            // un panel propio. Sólo lo sustituimos cuando reconocemos su
            // descripción inequívocamente.
            if (
                ContainsIgnoreCase(source, "Perfect Parry") &&
                ContainsIgnoreCase(source, "800%")
            )
            {
                return
                    "<style=cKeywordName>Parada perfecta</style>" +
                    "<style=cSub>Inflige <style=cIsDamage>800% de daño</style> y aplica " +
                    "<style=cIsHealth>1 Mutilado</style>, aturde al atacante, destruye el proyectil " +
                    "que activó la parada y permite realizar un <style=cIsDamage>Contraataque</style>.</style>";
            }

            return null;
        }

        // =========================================================
        // PANEL PERSONALIZADO DE HUNK (VIRUS)
        // =========================================================
        /// <summary>
        /// HUNK construye parte del tooltip de OSP durante
        /// LoadoutPanelController.Rebuild y no todas esas cadenas pasan por
        /// LanguageAPI/keywordTokens. Por eso los cuatro keywords normales se
        /// localizaban, pero G/T/C-Virus seguían en inglés.
        ///
        /// Este hook se instala sólo cuando RobHunkBody está presente. Después
        /// de que HUNK termina su propio Rebuild, revisamos los componentes del
        /// panel y localizamos exclusivamente las tres descripciones de Virus.
        /// No hay referencia directa a HunkMod.dll y no se toca gameplay.
        /// </summary>
        private static void InstallHunkLoadoutPanelLocalization(
            ManualLogSource logger
        )
        {
            hunkRuntimeLogger = logger;

            if (hunkLoadoutPanelHookInstalled)
            {
                return;
            }

            On.RoR2.UI.LoadoutPanelController.Rebuild +=
                HunkLoadoutPanelController_Rebuild;

            hunkLoadoutPanelHookInstalled = true;

            logger?.LogInfo(
                "[HUNK LOCALIZATION] Fallback del panel personalizado instalado para G/T/C-Virus."
            );
        }

        private static void HunkLoadoutPanelController_Rebuild(
            On.RoR2.UI.LoadoutPanelController.orig_Rebuild orig,
            RoR2.UI.LoadoutPanelController self
        )
        {
            orig(self);

            if (self == null || !IsSpanishLanguage())
            {
                return;
            }

            try
            {
                int replacements =
                    LocalizeHunkPanelComponentStrings(self);

                if (
                    replacements > 0 &&
                    !hunkVirusPanelFixLogged
                )
                {
                    hunkVirusPanelFixLogged = true;

                    hunkRuntimeLogger?.LogInfo(
                        "[HUNK LOCALIZATION] Panel OSP localizado en runtime | " +
                        "G/T/C-Virus | Sustituciones: " + replacements + "."
                    );
                }
            }
            catch (Exception exception)
            {
                hunkRuntimeLogger?.LogWarning(
                    "[HUNK LOCALIZATION] No se pudo aplicar el fallback del panel OSP. " +
                    exception.GetType().Name + ": " + exception.Message
                );
            }
        }

        private static int LocalizeHunkPanelComponentStrings(
            RoR2.UI.LoadoutPanelController panel
        )
        {
            UnityEngine.Component[] components =
                panel.GetComponentsInChildren<UnityEngine.Component>(true);

            if (components == null || components.Length == 0)
            {
                return 0;
            }

            int replacements = 0;

            foreach (UnityEngine.Component component in components)
            {
                if (component == null)
                {
                    continue;
                }

                Type type = component.GetType();

                BindingFlags flags =
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic;

                FieldInfo[] fields = type.GetFields(flags);

                for (int i = 0; i < fields.Length; i++)
                {
                    FieldInfo field = fields[i];

                    if (
                        field == null ||
                        field.FieldType != typeof(string) ||
                        field.IsInitOnly ||
                        field.IsLiteral
                    )
                    {
                        continue;
                    }

                    try
                    {
                        string current = field.GetValue(component) as string;

                        if (
                            TryLocalizeHunkPanelStringMember(
                                current,
                                out string translated,
                                out bool registerAsToken
                            )
                        )
                        {
                            if (registerAsToken)
                            {
                                RegisterSpanishToken(current, translated);
                            }
                            else
                            {
                                field.SetValue(component, translated);
                            }

                            replacements++;
                        }
                    }
                    catch
                    {
                        // Un miembro de UI no debe bloquear el resto del panel.
                    }
                }

                PropertyInfo[] properties = type.GetProperties(flags);

                for (int i = 0; i < properties.Length; i++)
                {
                    PropertyInfo property = properties[i];

                    if (
                        property == null ||
                        property.PropertyType != typeof(string) ||
                        !property.CanRead ||
                        !property.CanWrite ||
                        property.GetIndexParameters().Length != 0
                    )
                    {
                        continue;
                    }

                    try
                    {
                        string current =
                            property.GetValue(component, null) as string;

                        if (
                            TryLocalizeHunkPanelStringMember(
                                current,
                                out string translated,
                                out bool registerAsToken
                            )
                        )
                        {
                            if (registerAsToken)
                            {
                                RegisterSpanishToken(current, translated);
                            }
                            else
                            {
                                property.SetValue(component, translated, null);
                            }

                            replacements++;
                        }
                    }
                    catch
                    {
                        // Algunas propiedades Unity no admiten lectura/escritura
                        // durante Rebuild; simplemente continuamos.
                    }
                }
            }

            return replacements;
        }

        private static bool TryLocalizeHunkPanelStringMember(
            string current,
            out string translated,
            out bool registerAsToken
        )
        {
            translated = null;
            registerAsToken = false;

            if (string.IsNullOrWhiteSpace(current))
            {
                return false;
            }

            // Ruta A: HUNK guarda el texto literal directamente en el
            // TooltipProvider/componente del botón.
            translated = TranslateHunkVirusPanelText(current);

            if (translated != null && translated != current)
            {
                return true;
            }

            // Ruta B: el componente guarda un token privado que el auditor no
            // ve. Resolvemos su valor; si el resultado es uno de los tres
            // tooltips, registramos español sobre ESE token real.
            string resolved = Language.GetString(current) ?? "";

            if (
                string.IsNullOrWhiteSpace(resolved) ||
                string.Equals(
                    resolved,
                    current,
                    StringComparison.Ordinal
                )
            )
            {
                translated = null;
                return false;
            }

            translated = TranslateHunkVirusPanelText(resolved);

            if (translated == null || translated == resolved)
            {
                translated = null;
                return false;
            }

            registerAsToken = true;
            return true;
        }

        private static string TranslateHunkVirusPanelText(
            string source
        )
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return null;
            }

            if (
                ContainsIgnoreCase(source, "Once every stage") &&
                ContainsIgnoreCase(source, "G-Virus Sample") &&
                !ContainsIgnoreCase(source, "T-Virus Sample") &&
                !ContainsIgnoreCase(source, "C-Virus Sample")
            )
            {
                return
                    "Una vez por escenario, un monstruo aleatorio se vuelve " +
                    "<style=cIsHealth>Infectado</style>. Muta, " +
                    "<style=cIsDamage>haciéndose más fuerte con el tiempo</style>, " +
                    "y al matarlo suelta una <style=cIsDamage>Muestra de G-Virus</style>.";
            }

            if (
                ContainsIgnoreCase(source, "Once every stage") &&
                ContainsIgnoreCase(source, "T-Virus Sample")
            )
            {
                return
                    "Una vez por escenario, varios monstruos se vuelven " +
                    "<style=cIsUtility>Infectados</style>. Esto los " +
                    "<style=cIsDamage>revive dos veces</style>, y matar a todos " +
                    "te otorga una <style=cIsDamage>Muestra de T-Virus</style>.";
            }

            if (
                ContainsIgnoreCase(source, "Once every stage") &&
                ContainsIgnoreCase(source, "C-Virus Sample")
            )
            {
                return
                    "Una vez por escenario, varios monstruos se vuelven " +
                    "<style=cIsHealth>Infectados</style>. Al morir entran en " +
                    "<style=cIsDamage>frenesí</style> durante " +
                    "<style=cIsUtility>5 segundos</style> y luego explotan; " +
                    "matar a todos te otorga una <style=cIsDamage>Muestra de C-Virus</style>.";
            }

            return null;
        }

        // =========================================================
        // FALLBACK FINAL DEL TOOLTIP VISIBLE DE LOS VIRUS
        // =========================================================
        /// <summary>
        /// HUNK rellena el bloque largo de G/T/C-Virus después de Rebuild.
        /// Por eso modificar componentes durante Rebuild no alcanza ese texto.
        /// Este fallback se limita a la pantalla de Loadout y revisa de forma
        /// espaciada los HGTextMeshProUGUI activos. Sólo sustituye un texto si
        /// reconoce inequívocamente los bloques ingleses de los Virus.
        /// </summary>
        private static void InstallHunkLiveVirusUiLocalization(
            ManualLogSource logger
        )
        {
            hunkRuntimeLogger = logger;

            if (hunkLiveVirusUiFallbackInstalled)
            {
                return;
            }

            RoR2Application.onUpdate += HunkLiveVirusUiUpdate;
            hunkLiveVirusUiFallbackInstalled = true;

            logger?.LogInfo(
                "[HUNK LOCALIZATION] Fallback visual activo para el tooltip visible de G/T/C-Virus."
            );
        }

        private static void HunkLiveVirusUiUpdate()
        {
            if (!IsSpanishLanguage())
            {
                return;
            }

            // No hace falta recorrer UI en cada frame. A 120 FPS esto equivale
            // aproximadamente a una comprobación cada 0,1 s.
            hunkLiveVirusUiFrameCounter++;

            if ((hunkLiveVirusUiFrameCounter % 12) != 0)
            {
                return;
            }

            LoadoutPanelController loadout =
                UnityEngine.Object.FindObjectOfType<LoadoutPanelController>();

            if (loadout == null)
            {
                return;
            }

            try
            {
                HGTextMeshProUGUI[] texts =
                    UnityEngine.Object.FindObjectsOfType<HGTextMeshProUGUI>();

                if (texts == null || texts.Length == 0)
                {
                    return;
                }

                int replacements = 0;

                for (int i = 0; i < texts.Length; i++)
                {
                    HGTextMeshProUGUI text = texts[i];

                    if (
                        text == null ||
                        text.gameObject == null ||
                        !text.gameObject.activeInHierarchy
                    )
                    {
                        continue;
                    }

                    string current = text.text;
                    string translated = TranslateVisibleHunkVirusTooltip(current);

                    if (
                        string.IsNullOrEmpty(translated) ||
                        string.Equals(
                            current,
                            translated,
                            StringComparison.Ordinal
                        )
                    )
                    {
                        continue;
                    }

                    text.text = translated;
                    replacements++;
                }

                if (replacements > 0 && !hunkLiveVirusUiFixLogged)
                {
                    hunkLiveVirusUiFixLogged = true;

                    hunkRuntimeLogger?.LogInfo(
                        "[HUNK LOCALIZATION] Tooltip visible de Virus localizado en runtime | " +
                        "Sustituciones: " + replacements + "."
                    );
                }
            }
            catch (Exception exception)
            {
                hunkRuntimeLogger?.LogWarning(
                    "[HUNK LOCALIZATION] No se pudo localizar el tooltip visible de Virus. " +
                    exception.GetType().Name + ": " + exception.Message
                );
            }
        }

        private static string TranslateVisibleHunkVirusTooltip(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return null;
            }

            bool hasStageText =
                ContainsIgnoreCase(source, "Once every stage");

            if (!hasStageText)
            {
                return null;
            }

            bool hasG = ContainsIgnoreCase(source, "G-Virus");
            bool hasT = ContainsIgnoreCase(source, "T-Virus");
            bool hasC = ContainsIgnoreCase(source, "C-Virus");

            // HUNK normalmente coloca los tres Virus dentro de un único bloque
            // de texto. Sustituir el bloque completo evita depender de tags de
            // color insertados entre palabras del inglés original.
            if (hasG && hasT && hasC)
            {
                return
                    "[ G-Virus ]\n" +
                    "Una vez por escenario, un monstruo aleatorio se vuelve " +
                    "<style=cIsHealth>Infectado</style>. Muta, " +
                    "<style=cIsDamage>haciéndose más fuerte con el tiempo</style>, " +
                    "y al matarlo suelta una <style=cIsDamage>Muestra de G-Virus</style>.\n\n" +
                    "[ T-Virus ]\n" +
                    "Una vez por escenario, varios monstruos se vuelven " +
                    "<style=cIsUtility>Infectados</style>. Esto los " +
                    "<style=cIsDamage>revive dos veces</style>, y matar a todos " +
                    "te otorga una <style=cIsDamage>Muestra de T-Virus</style>.\n\n" +
                    "[ C-Virus ]\n" +
                    "Una vez por escenario, varios monstruos se vuelven " +
                    "<style=cIsHealth>Infectados</style>. Al morir entran en " +
                    "<style=cIsDamage>frenesí</style> durante " +
                    "<style=cIsUtility>5 segundos</style> y luego explotan; " +
                    "matar a todos te otorga una <style=cIsDamage>Muestra de C-Virus</style>.";
            }

            // También cubrimos versiones futuras de HUNK que separen cada
            // Virus en un TMP distinto.
            if (hasG)
            {
                return
                    "[ G-Virus ]\n" +
                    "Una vez por escenario, un monstruo aleatorio se vuelve " +
                    "<style=cIsHealth>Infectado</style>. Muta, " +
                    "<style=cIsDamage>haciéndose más fuerte con el tiempo</style>, " +
                    "y al matarlo suelta una <style=cIsDamage>Muestra de G-Virus</style>.";
            }

            if (hasT)
            {
                return
                    "[ T-Virus ]\n" +
                    "Una vez por escenario, varios monstruos se vuelven " +
                    "<style=cIsUtility>Infectados</style>. Esto los " +
                    "<style=cIsDamage>revive dos veces</style>, y matar a todos " +
                    "te otorga una <style=cIsDamage>Muestra de T-Virus</style>.";
            }

            if (hasC)
            {
                return
                    "[ C-Virus ]\n" +
                    "Una vez por escenario, varios monstruos se vuelven " +
                    "<style=cIsHealth>Infectados</style>. Al morir entran en " +
                    "<style=cIsDamage>frenesí</style> durante " +
                    "<style=cIsUtility>5 segundos</style> y luego explotan; " +
                    "matar a todos te otorga una <style=cIsDamage>Muestra de C-Virus</style>.";
            }

            return null;
        }

        private static bool ContainsIgnoreCase(
            string text,
            string value
        )
        {
            return
                !string.IsNullOrEmpty(text) &&
                !string.IsNullOrEmpty(value) &&
                text.IndexOf(
                    value,
                    StringComparison.OrdinalIgnoreCase
                ) >= 0;
        }

        private static object ReadStaticMember(
            Type type,
            string memberName
        )
        {
            if (type == null || string.IsNullOrWhiteSpace(memberName))
            {
                return null;
            }

            BindingFlags flags =
                BindingFlags.Static |
                BindingFlags.Public |
                BindingFlags.NonPublic;

            FieldInfo field =
                type.GetField(
                    memberName,
                    flags
                );

            if (field != null)
            {
                return field.GetValue(null);
            }

            PropertyInfo property =
                type.GetProperty(
                    memberName,
                    flags
                );

            return property != null
                ? property.GetValue(null, null)
                : null;
        }

        private static object ReadMember(
            object instance,
            string memberName
        )
        {
            if (instance == null || string.IsNullOrWhiteSpace(memberName))
            {
                return null;
            }

            Type type =
                instance.GetType();

            BindingFlags flags =
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic;

            FieldInfo field =
                type.GetField(
                    memberName,
                    flags
                );

            if (field != null)
            {
                return field.GetValue(instance);
            }

            PropertyInfo property =
                type.GetProperty(
                    memberName,
                    flags
                );

            return property != null
                ? property.GetValue(instance, null)
                : null;
        }

        private static string ReadStringMember(
            object instance,
            string memberName
        )
        {
            object value =
                ReadMember(
                    instance,
                    memberName
                );

            return value != null
                ? value.ToString()
                : "";
        }

        private static void RegisterAchievement(
            string nameToken,
            string name,
            string descriptionToken,
            string description
        )
        {
            RegisterSpanishToken(nameToken, name);
            RegisterSpanishToken(descriptionToken, description);
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

        private static void RegisterSpanishLiteralToken(
            string englishLiteralToken,
            string spanish
        )
        {
            RegisterSpanishToken(
                englishLiteralToken,
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
                spanish == null
            )
            {
                return;
            }

            LanguageAPI.Add(token, spanish, "es-419");
            LanguageAPI.Add(token, spanish, "es-ES");
        }

        private static bool IsSpanishLanguage()
        {
            string languageName =
                Language.currentLanguageName ?? "";

            return
                languageName.StartsWith(
                    "es",
                    StringComparison.OrdinalIgnoreCase
                );
        }
    }
}
