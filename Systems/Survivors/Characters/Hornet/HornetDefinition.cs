using System;
using BepInEx.Logging;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    public sealed class HornetDefinition : SurvivorDefinition
    {
        public override string SourceIdentifier => "com.fanto.HornetSilksong";
        public override string BodyName => "HornetBody";
        public override string DefinitionName => "Hornet";

        // Mod content-pack IDs vary independently of Thunderstore package IDs.
        // Identity remains restricted to this exact, distinctive BodyName.
        public override bool Matches(SurvivorInfo survivor)
        {
            return survivor != null && survivor.IsModded &&
                string.Equals(survivor.BodyName, BodyName, StringComparison.OrdinalIgnoreCase);
        }

        public override void RegisterInstalledLocalization(SurvivorInfo survivor, ManualLogSource logger)
        {
            if (!Matches(survivor)) return;

            NewSurvivorDiaryTokenResolver.Register(survivor, BodyName,
                HornetLore.SpanishLore, HornetLore.EnglishLore, logger);
            HornetSkillLocalization.RegisterFor(survivor, logger);
        }

        public override string ResolveDescription(SurvivorContentProfile profile, string fallback)
        {
            // El panel Overview de USU y el selector nativo usan rutas distintas.
            string language = Language.currentLanguageName ?? "";
            return language.StartsWith("es", StringComparison.OrdinalIgnoreCase)
                ? HornetSkillLocalization.SpanishOverview
                : fallback ?? "";
        }

        public override string ResolveSubtitle(SurvivorContentProfile profile, string fallback)
        {
            string language = Language.currentLanguageName ?? "";
            return language.StartsWith("es", StringComparison.OrdinalIgnoreCase)
                ? "Princesa protectora de Hallownest"
                : fallback ?? "";
        }

        public override string ResolveLore(SurvivorContentProfile profile, string fallback)
        {
            string language = Language.currentLanguageName ?? "";
            if (language.StartsWith("es", StringComparison.OrdinalIgnoreCase))
                return HornetLore.SpanishLore;
            if (language.StartsWith("en", StringComparison.OrdinalIgnoreCase))
                return HornetLore.EnglishLore;
            return string.IsNullOrWhiteSpace(fallback) ? HornetLore.EnglishLore : fallback;
        }
    }
}
