using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    public static partial class BuiltInMissionPresetCatalog
    {
        private static MissionPreset CreateAH64Official()
        {
            // Los objetivos reales y persistentes están en AH64CampaignTracker.
            // Mission V2 continúa guardando el preset como un oficial auténtico,
            // pero el handler de finales NO debe desbloquearlo por accidente.
            // El sentinel nunca coincide con un final vanilla y la campaña
            // concede el logro sólo tras validar TODOS los requisitos.
            MissionObjective final = new MissionObjective
            {
                Id = "ah64_campaign_final_authorized",
                Type = "CompleteEnding",
                Amount = 1d,
                ResetScope = "Run",
                Parameters = new JObject
                {
                    ["ending"] = "__USU_AH64_CAMPAIGN_VALIDATION_ONLY__"
                }
            };

            return CreateMissionPreset(
                MissionPresetIds.AH64Official,
                MissionPresetCategories.CharacterRecipe,
                "AH64Body",
                "Protocolo 64: Dominio aéreo",
                "Completa la campaña aérea acumulativa;\n" +
                "después escapa de la Luna en Monzón.",
                true, false, "Shared",
                CreateRoute("ah64_protocol_64",
                    new[] { final }, null,
                    new MissionRules { SingleRun = false }));
        }
    }
}
