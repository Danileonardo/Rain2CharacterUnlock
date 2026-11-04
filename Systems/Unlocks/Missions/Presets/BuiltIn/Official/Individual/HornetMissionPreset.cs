using Newtonsoft.Json.Linq;
namespace UniversalSurvivorUnlocks
{
    public static partial class BuiltInMissionPresetCatalog
    {
        private static MissionPreset CreateHornetOfficial()
        {
            var aerialKills = new MissionObjective
            {
                Id = "hornet_flying_enemies_9", Type = "HornetFlyingHookKills", Amount = 9d,
                ResetScope = "Run", Parameters = new JObject { ["requireSuccessfulGrapple"] = true }
            };
            return MissionModeConfiguration.Individual(CreateMissionPreset(
                MissionPresetIds.HornetOfficial, MissionPresetCategories.CharacterRecipe,
                "HornetBody", "El telar del viento",
                "Que nueve hilos dibujen rutas por el cielo;\n" +
                "tras el Gancho, vence a 9 enemigos voladores.",
                true, false, "PerPlayer", CreateRoute("hornet_silk_and_steel",
                    new[] { aerialKills }, new MissionCondition[]
                    {
                        CreateRequiredSurvivorCondition("LoaderBody"),
                        CreateCondition("RequiredStage", new JObject { ["stage"] = "shipgraveyard" })
                    }, new MissionRules { SingleRun = true })));
        }
    }
}
