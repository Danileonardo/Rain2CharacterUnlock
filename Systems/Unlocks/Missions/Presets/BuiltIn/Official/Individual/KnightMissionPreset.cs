using Newtonsoft.Json.Linq;
namespace UniversalSurvivorUnlocks
{
    public static partial class BuiltInMissionPresetCatalog
    {
        private static MissionPreset CreateKnightOfficial()
        {
            var skulls = new MissionObjective
            {
                Id = "knight_nkuhana_40", Type = "KnightSkullLaunches", Amount = 40d,
                ResetScope = "Run",
                Parameters = new JObject { ["requiredItem"] = "Infusion",
                    ["requiredItemCount"] = 4, ["requiredLegendary"] = "NovaOnHeal" }
            };
            return MissionModeConfiguration.Individual(CreateMissionPreset(
                MissionPresetIds.KnightOfficial, MissionPresetCategories.CharacterRecipe,
                "KnightBody", "Más allá del caparazón",
                "Cuatro fragmentos dan vida a una máscara;\n" +
                "con 4 Infusiones, lanza 40 cráneos de N'kuhana.",
                true, false, "PerPlayer", CreateRoute("knight_mask_soul",
                    new[] { skulls }, new[] { CreateRequiredSurvivorCondition("TreebotBody") },
                    new MissionRules { SingleRun = true })));
        }
    }
}
