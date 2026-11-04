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
            // El objetivo final es el Caminante de arcilla (Clay Dunestrider)
            // que aparece en el diario de Gwen. Solo cuenta si es jefe
            // del evento de teletransportador (no un spawn natural).
            var boss = CreateKillObjective("gwen_teleporter_boss", "SpecificTeleporterBoss", "ClayBossBody", 1d);
            boss.ResetScope = "Run";
            boss.Parameters = new JObject { ["requiredItem"] = "Dagger", ["requiredItemCount"] = 2 };
            return MissionModeConfiguration.Mixed(CreateMissionPreset(
                MissionPresetIds.GwenOfficial, MissionPresetCategories.CharacterRecipe,
                "GwenBody", "Entre hilos y agujas",
                "Dos Dagas ceremoniales, una última puntada;\nvence al Caminante de arcilla del portal.",
                true, false, "PerPlayer",
                CreateRoute("gwen_thread_and_needle", new[] { daggers, boss }, null,
                    new MissionRules { SingleRun = true })
            ));
        }
    }
}
