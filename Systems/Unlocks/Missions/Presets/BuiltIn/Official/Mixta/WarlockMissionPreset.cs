using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    public static partial class BuiltInMissionPresetCatalog
    {
        private static MissionPreset CreateWarlockOfficial()
        {
            // 500 BAJAS ANTES del encuentro infernal. Compartidas en la run;
            // no se cuentan bajas dentro de ninguno de los dos mapas rojos.
            // El progreso persiste al cambiar de sector, incluso si no toca
            // uno de los dos mapas objetivo en el sector 4 o 5.
            MissionObjective souls = CreateKillObjective(
                "warlock_five_hundred_souls", "Enemy", "", 500d);
            souls.ResetScope = "Run";
            AddObjectiveConditions(souls,
                CreateCondition("ExcludedStage", new JObject
                {
                    ["stages"] = new JArray("dampcavesimple", "helminthroost")
                }));

            // El Señor NO cuenta si muere antes de completar las 500 bajas.
            // Se admite un Señor de los diablillos real, sea o no del portal.
            // El jugador debe matarlo en UNA de las dos escenas indicadas.
            MissionObjective overlord = CreateKillObjective(
                "warlock_overlord_after_500", "SpecificBoss", "ImpBossBody", 1d);
            overlord.ResetScope = "Run";
            overlord.Parameters = new JObject
            {
                ["afterObjectiveId"] = "warlock_five_hundred_souls"
            };
            AddObjectiveConditions(overlord,
                CreateCondition("RequiredStage", new JObject
                {
                    ["stages"] = new JArray("dampcavesimple", "helminthroost")
                }));

            // No requieren estar los tres: basta Bandit, Huntress o Captain.
            // Los demás pueden contribuir a las 500 bajas en multijugador.
            return MissionModeConfiguration.Mixed(CreateMissionPreset(
                MissionPresetIds.WarlockOfficial,
                MissionPresetCategories.CharacterRecipe,
                "WarlockBody",
                "Bajo tres miradas",
                "Antes del infierno, reúne 500 almas;\n" +
                "allí vence a un Señor de los diablillos.",
                true, false, "Shared",
                CreateRoute("warlock_three_watchers",
                    new[] { souls, overlord },
                    new[]
                    {
                        CreateCondition("PartyHasSurvivor", new JObject
                        {
                            ["bodies"] = new JArray("Bandit2Body", "HuntressBody", "CaptainBody")
                        })
                    },
                    new MissionRules { SingleRun = true })));
        }
    }
}
