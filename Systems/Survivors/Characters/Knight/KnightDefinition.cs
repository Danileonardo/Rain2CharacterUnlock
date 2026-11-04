using System;
using BepInEx.Logging;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// The Knight (vibon). Registro separado de mission y lore.
    /// Los textos nativos fueron contrastados con KnightMod.dll.
    /// </summary>
    public sealed class KnightDefinition : SurvivorDefinition
    {
        public override string SourceIdentifier => "com.fanto.TheKnight";
        public override string BodyName => "KnightBody";
        public override string DefinitionName => "Knight";

        public override bool Matches(SurvivorInfo survivor)
        {
            return survivor != null && survivor.IsModded &&
                string.Equals(survivor.BodyName, BodyName, StringComparison.OrdinalIgnoreCase);
        }

        public override void RegisterInstalledLocalization(SurvivorInfo survivor, ManualLogSource logger)
        {
            if (!Matches(survivor)) return;

            // Sustituye el lore antiguo de Knight por el encuentro condicional.
            NewSurvivorDiaryTokenResolver.Register(survivor, BodyName,
                USURexKnightEncounterGate.ResolveKnight(true), USURexKnightEncounterGate.ResolveKnight(false), logger);

            // El traductor también registra el Overview nativo exacto del mod,
            // habilidades y skins sin modificar ningún SkillDef o SkinDef.
            KnightSkillLocalization.RegisterFor(survivor, logger);
        }

        public override string ResolveSubtitle(SurvivorContentProfile profile, string fallback)
        {
            return KnightSkillLocalization.IsSpanish()
                ? "Receptáculo del Abismo" : fallback ?? "";
        }

        public override string ResolveDescription(SurvivorContentProfile profile, string fallback)
        {
            return KnightSkillLocalization.IsSpanish()
                ? KnightSkillLocalization.SpanishOverview : fallback ?? "";
        }

        public override string ResolveLore(SurvivorContentProfile profile, string fallback)
        {
            string language = Language.currentLanguageName ?? "";
            if (language.StartsWith("es", StringComparison.OrdinalIgnoreCase))
                return USURexKnightEncounterGate.ResolveKnight(true);
            if (language.StartsWith("en", StringComparison.OrdinalIgnoreCase))
                return USURexKnightEncounterGate.ResolveKnight(false);
            return USURexKnightEncounterGate.ResolveKnight(false);
        }
    }
}
