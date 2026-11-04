using System;
using BepInEx.Logging;
using RoR2;
namespace UniversalSurvivorUnlocks
{
    public sealed class ShadowDefinition : SurvivorDefinition
    {
        // Verify installed survivor BodyName in the runtime log; never assume package GUID.
        public override string SourceIdentifier => "com.ds_gaming.Shadow";
        public override string BodyName => "ShadowBody";
        public override string DefinitionName => "Shadow";
        public override bool Matches(SurvivorInfo survivor) => survivor != null && survivor.IsModded &&
            string.Equals(survivor.BodyName, BodyName, StringComparison.OrdinalIgnoreCase);
        public override void RegisterInstalledLocalization(SurvivorInfo survivor, ManualLogSource logger)
        {
            if (Matches(survivor)) NewSurvivorDiaryTokenResolver.Register(survivor, BodyName,
                ShadowLore.SpanishLore, ShadowLore.EnglishLore, logger);
        }
        public override string ResolveLore(SurvivorContentProfile profile, string fallback)
        {
            string lang = Language.currentLanguageName ?? "";
            if (lang.StartsWith("es", StringComparison.OrdinalIgnoreCase)) return ShadowLore.SpanishLore;
            if (lang.StartsWith("en", StringComparison.OrdinalIgnoreCase)) return ShadowLore.EnglishLore;
            return string.IsNullOrWhiteSpace(fallback) ? ShadowLore.EnglishLore : fallback;
        }
    }
}
