using System;
using BepInEx.Logging;
using R2API;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Wanderer (internal Unforgiven), com.kenko.Unforgiven.
    /// Los tokens exactos de habilidades/pasiva/maestría proceden de la
    /// auditoría de localización USU del mod instalado (2026-10-11).
    /// No cambia mecánicas, desbloqueos, rewards, providers ni EN original.
    /// </summary>
    public sealed class WandererDefinition : SurvivorDefinition
    {
        public override string SourceIdentifier { get { return "com.kenko.Unforgiven"; } }
        public override string BodyName { get { return "UnforgivenBody"; } }
        public override string DefinitionName { get { return "Wanderer"; } }

        // Vista general: traducción fiel de consejos registrados en los tokens nativos.
        private static readonly string SpanishOverview = "Wanderer es un superviviente fácil de comenzar a usar, pero difícil de dominar. Sus cancelaciones de animación permiten encadenar ataques y mantener un daño constante.<color=#CCD3E0>\n\n< ! > Intercala Tempestad de acero entre Golpes veloces para maximizar el daño.\n\n< ! > Vigila las acumulaciones de Tempestad de acero: su tornado puede acabar con grupos enteros de enemigos.\n\n< ! > Usar Tempestad de acero durante Hoja envolvente hace que el ataque gire y cause daño en área.\n\n< ! > Hoja aérea: prepara Tempestad de acero durante Hoja envolvente y ejecuta Último aliento antes de que termine el giro. El giro viajará contigo hasta el objetivo. Si impacta, también proporciona una acumulación de Tormenta creciente.\n\n";

        // Lore del creador EN: texto humorístico sobre Yasuo; no se borra.
        // Esta narración ES es una ADAPTACIÓN DE USU, no canon nuevo del autor.
        private static readonly string SpanishLore = "EXPEDIENTE RECUPERADO — SUJETO SIN IDENTIFICAR\nProcedencia: desconocida\nClasificación: anomalía cinética / armamento de filo\nEstado del informe: inconcluso\n\nLa patrulla encontró los cuerpos antes que las huellas. Ninguna señal de explosivos, ninguna munición abandonada. Solo cortes: uno en cada punto vulnerable, como si alguien hubiera calculado de antemano el lugar exacto en que caerían sus enemigos.\n\nLa primera explicación fue una emboscada. La segunda, un fallo en los equipos de vigilancia. La tercera no llegó a presentarse: el viento había borrado los rastros antes de que terminara el reconocimiento.\n\nUn testigo aseguró que vio a un viajero armado con una espada. Otro afirmó que eran dos. El registro de una cámara recogió una figura en el borde del desfiladero; en el fotograma siguiente ya estaba detrás de su adversario. No se detectó ningún dispositivo de teletransporte.\n\nLos informes de combate describen un patrón imposible de reproducir. El sujeto avanzaba con golpes veloces, interrumpía sus propios movimientos para enlazar nuevas estocadas y reunía corrientes de aire en torno a la hoja. Cuando la presión alcanzaba su límite, un tornado atravesaba el terreno y levantaba del suelo a cuanto encontraba a su paso.\n\nSin embargo, la observación más extraña no fue el ataque. Fue su defensa. Mientras continuó moviéndose, una fuerza invisible parecía protegerlo; al recibir un golpe directo, aquella protección se quebró y otra barrera ocupó su lugar. La patrulla anotó que detenerse habría sido su única debilidad. Nunca logró comprobarlo.\n\nEl enfrentamiento terminó en el extremo oriental del desfiladero. La última criatura fue elevada por una corriente repentina. El viajero alzó la vista y desapareció. Un instante después, ambos estaban suspendidos sobre el vacío. Hubo un destello de acero. La criatura cayó sola.\n\nCuando la patrulla regresó, encontró una empuñadura rota envuelta en tela y una inscripción que nadie supo atribuir a idioma conocido. La traducción más aceptada decía: «El último aliento no siempre es el final».\n\nUna cuarta versión del testimonio menciona que el viajero se detuvo, miró hacia la cámara y esperó a que cambiara el viento. No se incluyó en el informe oficial.\n\nANOTACIÓN FINAL: se deniega la persecución. Los instrumentos continúan indicando una dirección de viento distinta en cada lectura.\n\n[FIN DEL REGISTRO]";

        public override void RegisterInstalledLocalization(SurvivorInfo survivor, ManualLogSource logger)
        {
            if (survivor == null || !Matches(survivor)) return;
            RegisterSpanishToken("KENKO_UNFORGIVEN_NAME", "Wanderer");
            RegisterSpanishToken("KENKO_UNFORGIVEN_SUBTITLE", "El Imperdonado");
            RegisterSpanishToken("KENKO_UNFORGIVEN_DESCRIPTION", SpanishOverview);

            // Token de lore nativo: el Logbook NO llama a ResolveLore().
            // El catálogo de RoR2 deriva *_LORE desde KENKO_UNFORGIVEN_NAME.
            // Resolver solo cubre Notas dentro del browser de USU.
            RegisterSpanishToken("KENKO_UNFORGIVEN_LORE", SpanishLore);

            // Pasiva: Way of the Wind.
            RegisterSpanishToken("KENKO_UNFORGIVEN_PASSIVE_NAME", "Senda del viento");
            RegisterSpanishSkill("KENKO_UNFORGIVEN_PASSIVE_DESCRIPTION",
                "Acumula un <style=cIsHealing>escudo</style> pasivo mientras te mueves. " +
                "Al recibir un golpe con las acumulaciones al máximo, el <style=cIsHealing>escudo</style> " +
                "se rompe y otorga una <style=cIsHealing>barrera</style>. " +
                "La <style=cIsDamage>probabilidad de golpe crítico</style> se multiplica por <style=cIsDamage>1,5</style>.");

            // Principal: Swift Strikes.
            RegisterSpanishToken("KENKO_UNFORGIVEN_PRIMARY_SWING_NAME", "Golpes veloces");
            RegisterSpanishSkill("KENKO_UNFORGIVEN_PRIMARY_SWING_DESCRIPTION",
                "Ataca con un corte frontal que inflige <style=cIsDamage>240% de daño</style>.");

            // Secundaria: Steel Tempest.
            RegisterSpanishToken("KENKO_UNFORGIVEN_SECONDARY_STEEL_NAME", "Tempestad de acero");
            RegisterSpanishSkill("KENKO_UNFORGIVEN_SECONDARY_STEEL_DESCRIPTION",
                "<style=cIsUtility>Rápida</style>. Estocada frontal que inflige " +
                "<style=cIsDamage>260% de daño</style>. Al impactar, obtienes una acumulación de " +
                "<color=#FFBF66>Tormenta creciente</color>. Con 2 acumulaciones, tu siguiente " +
                "<color=#FFBF66>Tempestad de acero</color> lanza un tornado que inflige " +
                "<style=cIsDamage>400% de daño</style>.");

            // Utilidad: Sweeping Blade.
            RegisterSpanishToken("KENKO_UNFORGIVEN_UTILITY_SWEEP_NAME", "Hoja envolvente");
            RegisterSpanishSkill("KENKO_UNFORGIVEN_UTILITY_SWEEP_DESCRIPTION",
                "Te desplazas hacia un enemigo e infliges <style=cIsDamage>150% de daño</style>. " +
                "Tras cada desplazamiento, el siguiente inflige <style=cIsDamage>50% de daño</style> " +
                "adicional, hasta un máximo de <style=cIsDamage>200% de daño adicional</style>.");

            // Especial: Last Breath.
            RegisterSpanishToken("KENKO_UNFORGIVEN_SPECIAL_BREATH_NAME", "Último aliento");
            RegisterSpanishSkill("KENKO_UNFORGIVEN_SPECIAL_BREATH_DESCRIPTION",
                "Te teletransportas a un enemigo <style=cIsUtility>en el aire</style>, seleccionado " +
                "o cercano, y lo atacas rápidamente para infligir " +
                "<style=cIsDamage>2 × 250% + 800% de daño</style>. Obtienes " +
                "<style=cIsDamage>penetración de armadura</style> adicional durante 6 segundos.");

            // Única skin alternativa y logro nativo identificados en la auditoría.
            // Solo localiza el logro del autor; no altera su condición de desbloqueo.
            RegisterSpanishToken("KENKO_UNFORGIVEN_MASTERY_SKIN_NAME", "Torbellino");
            RegisterSpanishToken("ACHIEVEMENT_KENKO_UNFORGIVEN_MASTERYACHIEVEMENT_NAME", "Wanderer: Maestría");
            RegisterSpanishToken("ACHIEVEMENT_KENKO_UNFORGIVEN_MASTERYACHIEVEMENT_DESCRIPTION",
                "Como Wanderer, termina el juego o sacrifícate en el Obelisco en dificultad Monzón.");

            logger?.LogInfo("[LOCALIZATION] Definition activa | UnforgivenBody | " +
                "ES Vista general + pasiva + 4 habilidades + skin Torbellino + Maestría + lore nativo y Notas USU; EN original preservado");
        }

        public override string ResolveSubtitle(SurvivorContentProfile profile, string fallback)
        {
            return IsSpanish() ? "El Imperdonado" : fallback ?? "";
        }

        public override string ResolveDescription(SurvivorContentProfile profile, string fallback)
        {
            return IsSpanish() ? SpanishOverview : fallback ?? "";
        }

        public override string ResolveLore(SurvivorContentProfile profile, string fallback)
        {
            // No tocar el texto humorístico original publicado en inglés.
            return IsSpanish() ? SpanishLore : fallback ?? "";
        }

        private static bool IsSpanish()
        {
            return (Language.currentLanguageName ?? "")
                .StartsWith("es", StringComparison.OrdinalIgnoreCase);
        }

        private static void RegisterSpanishSkill(string token, string translation)
        {
            RegisterSpanishToken(token, FormatLongSkillText(translation));
        }

        private static void RegisterSpanishToken(string token, string translation)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(translation)) return;
            LanguageAPI.Add(token, translation, "es-419");
            LanguageAPI.Add(token, translation, "es-ES");
        }
    }
}
