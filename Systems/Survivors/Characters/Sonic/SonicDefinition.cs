using System;
using BepInEx.Logging;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    public sealed class SonicDefinition : SurvivorDefinition
    {
        // Content pack GUID may differ from Thunderstore identifier.
        // Do not assume it is an authoritative installation check.
        public override string SourceIdentifier => "com.ds_gaming.Sonic";
        public override string BodyName => "SonicBody";
        public override string DefinitionName => "Sonic";
        public override bool Matches(SurvivorInfo survivor) => survivor != null &&
            survivor.IsModded && string.Equals(survivor.BodyName, BodyName, StringComparison.OrdinalIgnoreCase);
        public override void RegisterInstalledLocalization(SurvivorInfo survivor, ManualLogSource logger)
        {
            if (Matches(survivor)) NewSurvivorDiaryTokenResolver.Register(survivor, BodyName,
                USUMultSonicEncounterGate.ResolveSonic(true), USUMultSonicEncounterGate.ResolveSonic(false), logger);
        }
        public override string ResolveLore(SurvivorContentProfile profile, string fallback)
        {
            string lang = Language.currentLanguageName ?? "";
            if (lang.StartsWith("es", StringComparison.OrdinalIgnoreCase)) return USUMultSonicEncounterGate.ResolveSonic(true);
            if (lang.StartsWith("en", StringComparison.OrdinalIgnoreCase)) return USUMultSonicEncounterGate.ResolveSonic(false);
            return string.IsNullOrWhiteSpace(fallback) ? USUMultSonicEncounterGate.ResolveSonic(false) : fallback;
        }
    }
}
