using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    public static partial class BuiltInMissionPresetCatalog
    {
        private static MissionPreset CreateBansheeOfficial()
        {
            // The airborne condition is evaluated on the owning player's CharacterBody.
            // Kills by a grounded teammate do not advance this objective.
            MissionObjective aerialHunt = CreateKillObjective(
                "banshee_airborne_enemies", "Enemy", "", 20d);
            aerialHunt.ResetScope = "Run";
            AddObjectiveConditions(
                aerialHunt, CreateCondition("Airborne", new JObject()));

            // One teleporter boss killed AFTER the 20th aerial enemy;
            // the killer must also be airborne at the final blow.
            MissionObjective aerialBoss = CreateKillObjective(
                "banshee_airborne_teleporter_boss", "TeleporterBoss", "", 1d);
            aerialBoss.ResetScope = "Run";
            aerialBoss.Parameters = new JObject
            {
                ["afterObjectiveId"] = "banshee_airborne_enemies"
            };
            AddObjectiveConditions(
                aerialBoss, CreateCondition("Airborne", new JObject()));

            return CreateMissionPreset(
                MissionPresetIds.BansheeOfficial,
                MissionPresetCategories.CharacterRecipe,
                "BansheeBody",
                "Presagio desde las alturas",
                "Tras 20 bajas en el aire, derrota en el aire\n" +
                "a un jefe del teletransportador.",
                true, false, "Shared",
                CreateRoute(
                    "banshee_omen_from_above",
                    new[] { aerialHunt, aerialBoss }, null,
                    new MissionRules { SingleRun = true }));
        }
    }
}
