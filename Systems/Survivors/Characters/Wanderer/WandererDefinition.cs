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
    /// No cambia mecánicas, desbloqueos, rewards, providers ni DLL del autor.
    /// El lore visible en EN/ES es una adaptación narrativa de USU.
    /// </summary>
    public sealed class WandererDefinition : SurvivorDefinition
    {
        public override string SourceIdentifier { get { return "com.kenko.Unforgiven"; } }
        public override string BodyName { get { return "UnforgivenBody"; } }
        public override string DefinitionName { get { return "Wanderer"; } }

        // Vista general: traducción fiel de consejos registrados en los tokens nativos.
        private static readonly string SpanishOverview = WandererLocalization.SpanishOverview;

        // Diario I (USU): monólogo narrativo ES/EN, inspirado en las técnicas del
        // mod original de tsuyoikenko, pero sin afirmar que Wanderer sea Yasuo.
        // No se modifica la DLL del creador ni sus otros recursos originales.
        private static readonly string SpanishLore = WandererLore.SpanishLore;

        private static readonly string EnglishLore = WandererLore.EnglishLore;

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
            RegisterEnglishToken("KENKO_UNFORGIVEN_LORE", EnglishLore);

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

            UsuLog.Verbose(logger, "[LOCALIZATION] Definition activa | UnforgivenBody | " +
                "ES Vista general + pasiva + 4 habilidades + skin Torbellino + Maestría + Diario I USU ES/EN y Notas");
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
            // El Diario I USU se presenta en los dos idiomas admitidos.
            return IsSpanish() ? SpanishLore : EnglishLore;
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

        private static void RegisterEnglishToken(string token, string translation)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(translation)) return;
            LanguageAPI.Add(token, translation, "en");
        }
    }
}
