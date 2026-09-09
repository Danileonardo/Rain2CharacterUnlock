using System;

using BepInEx.Logging;
using R2API;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Integración curada de Ralsei.
    ///
    /// Autoridad USU para:
    /// - localización visible es-419 / es-ES;
    /// - pasiva, skills y keywords publicadas por el mod creador;
    /// - skins y achievements visibles;
    /// - metadata del logro original "Pacifist" cuando se consulta;
    /// - lore / Logbook porque el creador registra GRP_RALSEI_LORE vacío.
    ///
    /// La misión oficial USU "El poder de la bondad" continúa en el sistema
    /// de misiones actual y se migrará en una fase posterior.
    ///
    /// No referencia RalseiMod.dll. Todo se aplica mediante identificadores y
    /// tokens públicos una vez que el Registry confirma que el survivor está
    /// realmente instalado.
    /// </summary>
    public sealed class RalseiDefinition : SurvivorDefinition
    {
        public override string SourceIdentifier
        {
            get
            {
                return "com.GodRayProd.RalseiMod";
            }
        }

        public override string BodyName
        {
            get
            {
                return "RalseiBody";
            }
        }

        public override string DefinitionName
        {
            get
            {
                return "Ralsei";
            }
        }


        private static readonly string RalseiSpanishDescription =
            "Las capacidades de apoyo de Ralsei pueden unir a cualquier equipo, ¡amigo o enemigo!\n\n" +
            "< ! > Elige con cuidado a los enemigos que pacificas; sólo puedes mantener 3 aliados pacificados a la vez. " +
            "Los enemigos pacificados no otorgarán dinero, así que tenlo en cuenta.\n\n" +
            "< ! > Lanzar Pacificar sobre los jefes puede ser una buena forma de debilitarlos para que tus aliados obtengan ventaja. " +
            "No descartes la habilidad sólo porque no pueda perdonar a los jefes.\n\n" +
            "< ! > Tu primaria es mucho más potente si omites el último golpe del combo, pero no puede aplicar debilitaciones sin él. " +
            "Decide cuándo completar el combo y cuándo atacar en ráfagas.";


        private static readonly string RalseiSpanishLore =
            "Registro recuperado de una fuente sin correspondencia conocida.\n\n" +
            "El pequeño príncipe apareció hablando de profecías, fuentes oscuras y un mundo que no figuraba en ningún mapa. " +
            "No parecía alarmado por encontrarse tan lejos de casa. Si acaso, parecía más preocupado por quienes habían llegado antes que él.\n\n" +
            "Ralsei insiste en que casi cualquier enfrentamiento puede terminar sin una muerte. Donde otros ven criaturas hostiles, " +
            "él ve futuros compañeros. Ha sido observado curando heridas, interponiendo barreras y hablando con enemigos hasta que " +
            "éstos abandonan las armas y, en ocasiones, cambian de bando por completo.\n\n" +
            "Sus métodos resultan extraños en Petrichor V. La supervivencia aquí rara vez recompensa la misericordia.\n\n" +
            "Aun así, sus nuevos aliados continúan siguiéndolo.\n\n" +
            "Quizá porque sabe algo que los demás han olvidado: que ser capaz de destruir a un enemigo no significa que sea necesario hacerlo.\n\n" +
            "Y mientras exista alguien dispuesto a escuchar, el príncipe del Mundo Oscuro parece decidido a seguir intentándolo.";


        private static readonly string RalseiEnglishLore =
            "Record recovered from a source with no known match.\n\n" +
            "The little prince appeared speaking of prophecies, dark fountains, and a world that did not appear on any map. " +
            "He did not seem alarmed to find himself so far from home. If anything, he seemed more concerned about those who had arrived before him.\n\n" +
            "Ralsei insists that almost any confrontation can end without a death. Where others see hostile creatures, he sees future companions. " +
            "He has been observed healing wounds, raising barriers, and speaking with enemies until they lay down their weapons and, at times, " +
            "change sides entirely.\n\n" +
            "His methods are strange on Petrichor V. Survival here rarely rewards mercy.\n\n" +
            "Even so, his new allies continue to follow him.\n\n" +
            "Perhaps because he knows something the others have forgotten: being able to destroy an enemy does not mean it is necessary to do so.\n\n" +
            "And as long as someone is willing to listen, the prince of the Dark World seems determined to keep trying.";


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

            // El creador aporta inglés válido para el resto de su contenido,
            // pero GRP_RALSEI_LORE está vacío. Sólo completamos ese hueco en EN.
            RegisterEnglishLoreFallback();

            RegisterRalseiSpanishTokens();
            RegisterRalseiPassiveSpanish(
                survivor,
                logger
            );

            logger?.LogInfo(
                "[LOCALIZATION] Definition activa | " +
                "RalseiBody | es-419 / es-ES + pasiva + skills + keywords + lore"
            );
        }


        public override string ResolveSubtitle(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            return IsSpanishLanguage()
                ? "Príncipe del Mundo Oscuro"
                : fallback ?? "";
        }


        public override string ResolveDescription(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            return IsSpanishLanguage()
                ? RalseiSpanishDescription
                : fallback ?? "";
        }


        public override string ResolveLore(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            if (IsSpanishLanguage())
            {
                return RalseiSpanishLore;
            }

            if (
                IsEnglishLanguage() &&
                IsMissingLoreText(fallback)
            )
            {
                return RalseiEnglishLore;
            }

            return fallback ?? "";
        }


        public override string ResolveOriginalMissionName(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            return IsSpanishLanguage()
                ? "Pacifista"
                : fallback ?? "";
        }


        public override string ResolveOriginalMissionDescription(
            SurvivorContentProfile profile,
            string fallback
        )
        {
            return IsSpanishLanguage()
                ? "Completa un sector sin matar a ningún enemigo que no sea jefe."
                : fallback ?? "";
        }


        private static void RegisterEnglishLoreFallback()
        {
            // El archivo de idioma del creador registra GRP_RALSEI_LORE con
            // una cadena vacía. USU aporta únicamente el contenido faltante.
            LanguageAPI.Add(
                "GRP_RALSEI_LORE",
                RalseiEnglishLore,
                "en"
            );
        }


        private static void RegisterRalseiSpanishTokens()
        {
            // Survivor / Logbook
            RegisterSpanishToken(
                "GRP_RALSEI_SUBTITLE",
                "Príncipe del Mundo Oscuro"
            );

            RegisterSpanishToken(
                "GRP_RALSEI_DESCRIPTION",
                RalseiSpanishDescription
            );

            RegisterSpanishToken(
                "GRP_RALSEI_LORE",
                RalseiSpanishLore
            );

            RegisterSpanishToken(
                "GRP_RALSEI_OUTRO_FLAVOR",
                "...y así partió, brillando con ESPERANZA."
            );

            RegisterSpanishToken(
                "GRP_RALSEI_OUTRO_FAILURE",
                "...y así desapareció, un susurro de leyenda desvaneciéndose en la oscuridad."
            );

            // Nombre potencialmente visible del cuerpo invocado.
            RegisterSpanishToken(
                "GRP_DUMMY_NAME",
                "Muñeco"
            );

            // Pasiva. La descripción también se registra de forma dinámica
            // usando los tokens reales publicados por SkillLocator.passiveSkill.
            RegisterSpanishToken(
                "GRP_RALSEI_PASSIVE_NAME",
                "Puntos de tensión: Arcana"
            );

            RegisterSpanishToken(
                "GRP_RALSEI_PASSIVE_DESCRIPTION",
                "<style=cIsUtility>Bloquear</style> ataques o <style=cIsUtility>Enredar</style> enemigos " +
                "<style=cIsDamage>reduce los tiempos de recarga de tus habilidades</style>."
            );

            // Primarias
            RegisterSpanishToken(
                "GRPSKILLSCARFRANGE",
                "Látigo de hilos"
            );

            RegisterSpanishToken(
                "GRPSKILLSCARFRANGE_DESCRIPTION",
                "Usa tu bufanda para lanzar <style=cIsUtility>hilos perforantes</style> que infligen " +
                "<style=cIsDamage>130% de daño</style>. Cada <style=cIsDamage>4.º</style> ataque " +
                "<style=cIsUtility>Enreda</style> a los enemigos e inflige <style=cIsDamage>180% de daño</style>."
            );

            RegisterSpanishToken(
                "GRPSKILLSCARFSHORT",
                "Corte de hilos"
            );

            RegisterSpanishToken(
                "GRPSKILLSCARFSHORT_DESCRIPTION",
                "Azota con tu bufanda a los enemigos cercanos e inflige <style=cIsDamage>180% de daño</style>. " +
                "Cada <style=cIsDamage>4.º</style> ataque gira a tu alrededor, <style=cIsUtility>Enredando</style> " +
                "a los enemigos e infligiendo <style=cIsDamage>400% de daño</style>."
            );

            // Secundarias
            RegisterSpanishToken(
                "GRPSKILLHEALSPELL",
                "Plegaria curativa"
            );

            RegisterSpanishToken(
                "GRPSKILLHEALSPELL_DESCRIPTION",
                "Lanza un <style=cIsHealing>hechizo curativo</style> sobre ti y todos los aliados en un radio de " +
                "<style=cIsUtility>35 m</style>, restaurando <style=cIsHealing>10% de salud</style> y otorgando " +
                "<style=cIsHealing>Regeneración</style> durante <style=cIsHealing>1</style> segundo. Mantener esta habilidad " +
                "durante 1,5 segundos <style=cIsDamage>consume todas las cargas</style> y teletransporta a tus aliados hasta ti, " +
                "<style=cIsHealing>curándolos un 10% más 20 de salud fija</style>, cantidad que aumenta con los niveles."
            );

            RegisterSpanishToken(
                "GRPSKILLGUARDSPELL",
                "Guardia mullida"
            );

            RegisterSpanishToken(
                "GRPSKILLGUARDSPELL_DESCRIPTION",
                "Lanza un <style=cIsUtility>hechizo protector</style> sobre ti y todos los aliados en un radio de " +
                "<style=cIsUtility>35 m</style>, otorgando <style=cIsUtility>30% de probabilidad de bloqueo</style> durante " +
                "<style=cIsUtility>6</style> segundos. Mantener esta habilidad durante 1,5 segundos " +
                "<style=cIsDamage>consume todas las cargas</style> y teletransporta a tus aliados hasta ti, " +
                "<style=cIsHealing>curándolos un 10% más 20 de salud fija</style>, cantidad que aumenta con los niveles."
            );

            // Utilidad
            RegisterSpanishToken(
                "GRPSKILLLIFTPRAYER",
                "Ascender"
            );

            RegisterSpanishToken(
                "GRPSKILLLIFTPRAYER_DESCRIPTION",
                "Asciende <style=cIsUtility>a gran altura</style> y después desciende lentamente " +
                "<style=cIsUtility>flotando</style>. <style=cIsDamage>El efecto de flotación termina al tocar el suelo " +
                "o al volver a activar la habilidad</style>."
            );

            // Especiales
            RegisterSpanishToken(
                "GRPSKILLPACIFY",
                "Pacificar"
            );

            RegisterSpanishToken(
                "GRPSKILLPACIFY_DESCRIPTION",
                "Tiene <style=cIsUtility>2</style> cargas. Lanza un hechizo de <style=cIsUtility>Sueño</style> sobre un enemigo " +
                "con menos del <style=cIsUtility>50% de salud</style>. Los enemigos pacificados " +
                "<style=cIsDamage>se convierten en aliados Potenciados</style> tras <style=cIsUtility>5</style> s. Máximo: 3."
            );

            RegisterSpanishToken(
                "GRPSKILLRALSEIDUMMYSKILL",
                "Muñeco de práctica"
            );

            RegisterSpanishToken(
                "GRPSKILLRALSEIDUMMYSKILL_DESCRIPTION",
                "Lanza un muñeco de Ralsei que <style=cIsUtility>atrae los ataques enemigos</style> y, al morir, " +
                "<style=cIsDamage>Aturde y Fatiga</style> e inflige <style=cIsDamage>1000% de daño</style>. " +
                "Periódicamente <style=cIsHealing>Potencia</style> a los aliados cercanos."
            );

            // Keywords
            RegisterSpanishToken(
                "GRP_KEYWORD_TANGLE",
                "<style=cKeywordName>Enredado</style><style=cSub>Reduce la armadura en <style=cIsUtility>-20</style> " +
                "y la velocidad de movimiento en <style=cIsUtility>-40%</style> durante 10 segundos. " +
                "<style=cIsDamage>Los objetivos Enredados tienen prioridad para tus aliados</style>.</style>"
            );

            RegisterSpanishToken(
                "GRP_KEYWORD_EMPOWER",
                "<style=cKeywordName>Potenciado</style><style=cSub>Obtén +100% de <style=cIsDamage>velocidad de ataque</style>, " +
                "+30% de <style=cIsDamage>velocidad de movimiento</style>, -50% de <style=cIsUtility>tiempo de recarga</style>, " +
                "+20 de <style=cIsUtility>armadura</style> y +2 de <style=cIsHealing>regeneración de salud base por segundo</style>. " +
                "Puede acumularse.</style>"
            );

            RegisterSpanishToken(
                "GRP_KEYWORD_SLEEP",
                "<style=cKeywordName>Dormido</style><style=cSub><style=cIsDamage>Perdona</style> a un enemigo que no sea jefe, " +
                "retirándolo del combate <style=cIsHealth>SIN activar efectos al matar</style>. Los jefes pasan a estar " +
                "<style=cIsUtility>Fatigados</style> durante 0 s en su lugar.</style>"
            );

            RegisterSpanishToken(
                "GRP_KEYWORD_FATIGUE",
                "<style=cKeywordName>Fatigado</style><style=cSub>Reduce la armadura en <style=cIsUtility>-60</style> " +
                "y la velocidad de ataque en <style=cIsUtility>-80%</style>.</style>"
            );

            // Skins
            RegisterSpanishToken(
                "GRP_RALSEI_DEFAULT_SKIN_NAME",
                "Predeterminado"
            );

            RegisterSpanishToken(
                "GRP_RALSEI_MASTERY_SKIN_NAME",
                "Pelado"
            );

            RegisterSpanishToken(
                "GRP_RALSEI_NIKO_SKIN_NAME",
                "Solsticio"
            );

            // Achievements / unlockables del creador
            RegisterSpanishToken(
                "ACHIEVEMENT_GRP_RALSEI_MASTERYACHIEVEMENT_NAME",
                "Ralsei: Maestría"
            );

            RegisterSpanishToken(
                "ACHIEVEMENT_GRP_RALSEI_MASTERYACHIEVEMENT_DESCRIPTION",
                "Como Ralsei, completa el juego u oblítérate en Monzón."
            );

            RegisterSpanishToken(
                "ACHIEVEMENT_GRP_RALSEI_PACIFISTACHIEVEMENT_NAME",
                "Ralsei: Pacifista verdadero"
            );

            RegisterSpanishToken(
                "ACHIEVEMENT_GRP_RALSEI_PACIFISTACHIEVEMENT_DESCRIPTION",
                "Como Ralsei, completa el juego u oblítérate sin matar a ningún enemigo que no sea jefe."
            );

            RegisterSpanishToken(
                "ACHIEVEMENT_GRP_RALSEI_UNLOCKACHIEVEMENT_NAME",
                "Pacifista"
            );

            RegisterSpanishToken(
                "ACHIEVEMENT_GRP_RALSEI_UNLOCKACHIEVEMENT_DESCRIPTION",
                "Completa un sector sin matar a ningún enemigo que no sea jefe."
            );

            RegisterSpanishToken(
                "ACHIEVEMENT_GRP_RALSEI_OOPSACHIEVEMENT_NAME",
                "Ralsei: Ups"
            );

            RegisterSpanishToken(
                "ACHIEVEMENT_GRP_RALSEI_OOPSACHIEVEMENT_DESCRIPTION",
                "Como Ralsei, muere a manos de uno de tus propios súbditos."
            );

            RegisterSpanishToken(
                "ACHIEVEMENT_GRP_RALSEI_KINGHEALERACHIEVEMENT_NAME",
                "Ralsei: Rey sanador"
            );

            RegisterSpanishToken(
                "ACHIEVEMENT_GRP_RALSEI_KINGHEALERACHIEVEMENT_DESCRIPTION",
                "Como Ralsei, aplica más de 1.000.000 de puntos de curación en una sola partida."
            );

            RegisterSpanishToken(
                "ACHIEVEMENT_GRP_RALSEI_POWERFULFRIENDSACHIEVEMENT_NAME",
                "Ralsei: Conexiones poderosas"
            );

            RegisterSpanishToken(
                "ACHIEVEMENT_GRP_RALSEI_POWERFULFRIENDSACHIEVEMENT_DESCRIPTION",
                "Como Ralsei, ten al mismo tiempo como aliados a un Carroñero, una Umbra y un Élite raro."
            );
        }


        private static void RegisterRalseiPassiveSpanish(
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
                    "RalseiBody | SkillLocator.passiveSkill no disponible"
                );

                return;
            }

            if (!string.IsNullOrWhiteSpace(nameToken))
            {
                RegisterSpanishToken(
                    nameToken,
                    "Puntos de tensión: Arcana"
                );
            }

            if (!string.IsNullOrWhiteSpace(descriptionToken))
            {
                RegisterSpanishToken(
                    descriptionToken,
                    "<style=cIsUtility>Bloquear</style> ataques o <style=cIsUtility>Enredar</style> enemigos " +
                    "<style=cIsDamage>reduce los tiempos de recarga de tus habilidades</style>."
                );
            }

            logger?.LogInfo(
                "[LOCALIZATION] Pasiva localizada | " +
                "RalseiBody | NameToken: " +
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
