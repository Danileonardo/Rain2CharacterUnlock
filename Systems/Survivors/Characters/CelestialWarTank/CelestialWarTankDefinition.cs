using BepInEx.Logging;
using R2API;
namespace UniversalSurvivorUnlocks
{
    // Integra el personaje sin modificar el diario original cómico de Billy.
    public sealed class CelestialWarTankDefinition : SurvivorDefinition
    {
        public override string SourceIdentifier { get { return CWTCampaignIdentity.ContentPackIdentifier; } }
        public override string BodyName { get { return CWTCampaignIdentity.BodyName; } }
        public override string DefinitionName { get { return "Celestial War Tank (Billy)"; } }
        public override bool Matches(SurvivorInfo survivor) { return CWTCampaignIdentity.Observe(survivor); }
        public override void RegisterInstalledLocalization(SurvivorInfo survivor, ManualLogSource logger)
        {
            if (survivor == null || !Matches(survivor)) return;
            string p = SurvivorLocalization.GetOfficialTokenPrefix(survivor.BodyName);
            LanguageAPI.Add(p + "_ACHIEVEMENT_NAME", "Campaña de acero: Dominio terrestre", "es-419");
            LanguageAPI.Add(p + "_ACHIEVEMENT_DESCRIPTION", "Que ningún terreno detenga al acero;\ndomina 4 frentes y vence al Carroñero.", "es-419");
            LanguageAPI.Add(p + "_ACHIEVEMENT_NAME", "Campaña de acero: Dominio terrestre", "es-ES");
            LanguageAPI.Add(p + "_ACHIEVEMENT_DESCRIPTION", "Que ningún terreno detenga al acero;\ndomina 4 frentes y vence al Carroñero.", "es-ES");
            LanguageAPI.Add(p + "_ACHIEVEMENT_NAME", "Steel Campaign: Land Supremacy", "en");
            LanguageAPI.Add(p + "_ACHIEVEMENT_DESCRIPTION", "Let no terrain stop the steel;\nconquer four fronts and defeat a Scavenger.", "en");
        }
    }
}