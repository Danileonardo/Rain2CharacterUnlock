using Newtonsoft.Json.Linq;
namespace UniversalSurvivorUnlocks
{
    public static partial class BuiltInMissionPresetCatalog
    {
        private static MissionPreset CreateDarthVaderOfficial()
        {
            var kills = new MissionObjective
            {
                Id = "vader_cube_66", Type = "VaderCubeKills", Amount = 66d,
                ResetScope = "Run",
                Parameters = new JObject { ["requiredItem"] = "NearbyDamageBonus",
                                           ["requiredItemCount"] = 10,
                                           ["requiredEquipment"] = "Blackhole" }
            };
            return MissionModeConfiguration.Individual(CreateMissionPreset(
                MissionPresetIds.DarthVaderOfficial, MissionPresetCategories.CharacterRecipe,
                "DarthVaderBody", "Contraorden 66",
                "Con la FUERZA de atracción del Cubo Primordial;\n" +
                "acaba con 66 usando 10 Cristales de enfoque Kyber.",
                true, false, "PerPlayer", CreateRoute("vader_cube_66_route",
                    new[] { kills }, new[] { CreateRequiredSurvivorCondition("MercBody") },
                    new MissionRules { SingleRun = true })));
        }
    }
}
