using System;
using BepInEx.Logging;
using R2API;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Bayonetta Survivor (Psyche): only activates when the matching
    /// content pack and BayoBody are present; does not reference its DLL.
    /// </summary>
    public sealed class BayonettaDefinition : SurvivorDefinition
    {
        public override string SourceIdentifier => "com.psyche.BayoMod";
        public override string BodyName => "BayoBody";
        public override string DefinitionName => "Bayonetta";

        private const string LoreToken = "PSYCHE_BAYO_LORE";
        private static object englishOverlay;
        private static object spanishOverlay419;
        private static object spanishOverlayES;

        public override void RegisterInstalledLocalization(SurvivorInfo survivor, ManualLogSource logger)
        {
            if (survivor == null || !Matches(survivor)) return;
            // Preserve the native Logbook token and the already approved USU lore.
            if (englishOverlay == null)
                englishOverlay = LanguageAPI.AddOverlay(LoreToken, USUCommandoBayonettaEncounterGate.ResolveBayonetta(false), "en");
            if (spanishOverlay419 == null)
                spanishOverlay419 = LanguageAPI.AddOverlay(LoreToken, USUCommandoBayonettaEncounterGate.ResolveBayonetta(true), "es-419");
            if (spanishOverlayES == null)
                spanishOverlayES = LanguageAPI.AddOverlay(LoreToken, USUCommandoBayonettaEncounterGate.ResolveBayonetta(true), "es-ES");
            BayonettaSkillLocalization.RegisterFor(survivor, logger);
            BayonettaVerifiedLocalization.RegisterFor(survivor, logger);
            UsuLog.Verbose(logger, "[BAYONETTA] Diario I registrado en EN/ES (Logbook y USU Notes).");
        }


        public override string ResolveDescription(SurvivorContentProfile profile, string fallback)
        {
            string language = Language.currentLanguageName ?? "";
            if (language.StartsWith("es", StringComparison.OrdinalIgnoreCase))
                return BayonettaVerifiedLocalization.SpanishOverview;
            return fallback ?? "";
        }

        public override string ResolveLore(SurvivorContentProfile profile, string fallback)
        {
            string language = Language.currentLanguageName ?? "";
            if (language.StartsWith("es", StringComparison.OrdinalIgnoreCase))
                return USUCommandoBayonettaEncounterGate.ResolveBayonetta(true);
            if (language.StartsWith("en", StringComparison.OrdinalIgnoreCase))
                return USUCommandoBayonettaEncounterGate.ResolveBayonetta(false);
            return fallback ?? USUCommandoBayonettaEncounterGate.ResolveBayonetta(false);
        }
    }
}
