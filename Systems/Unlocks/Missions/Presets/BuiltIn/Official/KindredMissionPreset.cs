using Newtonsoft.Json.Linq;
namespace UniversalSurvivorUnlocks
{
    public static partial class BuiltInMissionPresetCatalog
    {
        private static MissionPreset CreateKindredOfficial()
        {
            var hunt = CreateKillObjective("kindred_unique_boss_hunt", "Boss", "", 4d);
            hunt.ResetScope = "Run";
            hunt.Parameters = new JObject
            {
                ["requiredItem"] = "DeathMark", ["requiredItemCount"] = 1,
                ["distinctBossBodies"] = true
            };
            return CreateMissionPreset(
                MissionPresetIds.KindredOfficial, MissionPresetCategories.CharacterRecipe,
                "KindredBody", "La marca del destino",
                "Porta una Marca de muerte;\ncaza 4 tipos distintos de jefes en una partida.",
                true, false, "PerPlayer",
                CreateRoute("kindred_four_unique_prey", new[] { hunt }, null,
                    new MissionRules { SingleRun = true })
            );
        }
    }
}
