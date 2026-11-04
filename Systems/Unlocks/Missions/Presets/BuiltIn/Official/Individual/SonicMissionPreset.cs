using Newtonsoft.Json.Linq;
namespace UniversalSurvivorUnlocks
{
    public static partial class BuiltInMissionPresetCatalog
    {
        private static MissionPreset CreateSonicOfficial()
        {
            var hits = new MissionObjective { Id = "sonic_transport_distinct_30",
                Type = "SonicTransportHits", Amount = 30d, ResetScope = "Run",
                Parameters = new JObject { ["distinctTargets"] = true } };
            return MissionModeConfiguration.Individual(CreateMissionPreset(
                MissionPresetIds.SonicOfficial, MissionPresetCategories.CharacterRecipe,
                "SonicBody", "Rolling Around at the Speed of Sound",
                "Ni siquiera los robots de Eggman podrían seguirle el ritmo.\n" +
                "Arrolla a 30 enemigos con Modo Transporte en una partida.",
                true, false, "PerPlayer", CreateRoute("sonic_transport_roll",
                    new[] { hits }, new[] { CreateRequiredSurvivorCondition("ToolbotBody") },
                    new MissionRules { SingleRun = true })));
        }
    }
}
