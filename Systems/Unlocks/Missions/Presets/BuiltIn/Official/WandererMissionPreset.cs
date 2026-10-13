using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    public static partial class BuiltInMissionPresetCatalog
    {
        private static MissionPreset CreateWandererOfficial()
        {
            // Three teleporter boss kills, only awarded when the killer is airborne.
            // Can be completed with another survivor, including in shared multiplayer.
            // SingleRun prevents cumulative progress across separate sessions.
            MissionObjective lastBreath = CreateKillObjective(
                "wanderer_airborne_teleporter_bosses_3", "TeleporterBoss", "", 3d);
            lastBreath.ResetScope = "Run";
            AddObjectiveConditions(lastBreath,
                CreateCondition("Airborne", new JObject()));

            return CreateMissionPreset(
                MissionPresetIds.WandererOfficial,
                MissionPresetCategories.CharacterRecipe,
                "UnforgivenBody",
                "Último aliento",
                "Derrota a 3 jefes de teletransportador\n" +
                "siendo tú quien aseste el golpe desde el aire.",
                true, false, "Shared",
                CreateRoute("wanderer_last_breath",
                    new[] { lastBreath }, null,
                    new MissionRules { SingleRun = true }));
        }
    }
}
