using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    public static partial class BuiltInMissionPresetCatalog
    {
        private static MissionPreset CreateBayonettaOfficial()
        {
            var dodgeKills = new MissionObjective
            {
                Id = "bayonetta_dodge_kills_40",
                Type = "BayonettaDodgeKills",
                Amount = 40d,
                ResetScope = "Run",
                Parameters = new JObject
                {
                    ["requiredSurvivor"] = "CommandoBody",
                    ["secondsAfterDodge"] = 4d,
                    ["damagePenalty"] = 2d
                }
            };

            return MissionModeConfiguration.Individual(CreateMissionPreset(
                MissionPresetIds.BayonettaOfficial,
                MissionPresetCategories.CharacterRecipe,
                "BayoBody",
                "Un baile sin heridas",
                "Que tus pistolas marquen el ritmo de la danza;\n" +
                "tras esquivar, vence a 40; cada golpe resta 2.",
                true, false, "PerPlayer",
                CreateRoute(
                    "bayonetta_dodge_dance",
                    new[] { dodgeKills },
                    new[] { CreateRequiredSurvivorCondition("CommandoBody") },
                    new MissionRules { SingleRun = true }
                )
            ));
        }
    }
}
