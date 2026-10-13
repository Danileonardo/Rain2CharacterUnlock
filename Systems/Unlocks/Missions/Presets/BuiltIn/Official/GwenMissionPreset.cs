using Newtonsoft.Json.Linq;
namespace UniversalSurvivorUnlocks
{
    public static partial class BuiltInMissionPresetCatalog
    {
        private static MissionPreset CreateGwenOfficial()
        {
            var daggers = new MissionObjective
            {
                Id = "gwen_two_daggers", Type = "HoldItemStack", Amount = 2d,
                ResetScope = "Run", Parameters = new JObject { ["item"] = "Dagger" }
            };
            var boss = CreateKillObjective("gwen_teleporter_boss", "TeleporterBoss", "", 1d);
            boss.ResetScope = "Run";
            boss.Parameters = new JObject { ["requiredItem"] = "Dagger", ["requiredItemCount"] = 2 };
            return CreateMissionPreset(
                MissionPresetIds.GwenOfficial, MissionPresetCategories.CharacterRecipe,
                "GwenBody", "Entre hilos y agujas",
                "Consigue 2 Dagas ceremoniales;\nvence a un jefe de teletransportador con ellas.",
                true, false, "PerPlayer",
                CreateRoute("gwen_thread_and_needle", new[] { daggers, boss }, null,
                    new MissionRules { SingleRun = true })
            );
        }
    }
}
