using BepInEx.Logging;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Integración independiente de The Knight. Se activa sólo con el mod y Body reales.
    /// Los diarios definitivos aprobados NO están en el material entregado:
    /// hasta recuperarlos, el Logbook original se conserva intacto.
    /// No se inventan pasivas, habilidades, skins ni líneas de narrativa.
    /// </summary>
    public sealed class TheKnightDefinition : SurvivorDefinition
    {
        public override string SourceIdentifier => "com.vibon.TheKnight";
        public override string BodyName => "KnightBody";
        public override string DefinitionName => "The Knight";

        public override void RegisterInstalledLocalization(SurvivorInfo survivor, ManualLogSource logger)
        {
            if (survivor == null || !Matches(survivor)) return;
            UsuLog.Verbose(logger, "[DEFINITION] KnightBody activo; lore original conservado hasta recuperar Diario I definitivo.");
        }

        public override string ResolveLore(SurvivorContentProfile profile, string fallback)
            => fallback ?? "";
    }
}
