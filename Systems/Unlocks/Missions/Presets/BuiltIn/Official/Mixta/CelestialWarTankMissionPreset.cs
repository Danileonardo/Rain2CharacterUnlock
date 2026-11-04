using Newtonsoft.Json.Linq;
namespace UniversalSurvivorUnlocks
{
    public static partial class BuiltInMissionPresetCatalog
    {
        private static MissionPreset CreateCelestialWarTankOfficial()
        {
            // Sólo el tracker de campaña puede validar al Carroñero.
            // El sentinel evita desbloqueos accidentales del handler genérico.
            var sentinel = new MissionObjective
            {
                Id = "cwt_campaign_final_authorized", Type = "CompleteEnding",
                Amount = 1d, ResetScope = "Run",
                Parameters = new JObject { ["ending"] = "__USU_CWT_CAMPAIGN_VALIDATION_ONLY__" }
            };
            return MissionModeConfiguration.Mixed(CreateMissionPreset(
                MissionPresetIds.CelestialWarTankOfficial,
                MissionPresetCategories.CharacterRecipe,
                CWTCampaignIdentity.BodyName,
                "Campaña de acero: Dominio terrestre",
                "Que ningún terreno detenga al acero;\ndomina 4 frentes y vence al Carroñero.",
                true, false, "Shared",
                CreateRoute("cwt_steel_campaign", new[] { sentinel }, null,
                    new MissionRules { SingleRun = false })));
        }
    }
}