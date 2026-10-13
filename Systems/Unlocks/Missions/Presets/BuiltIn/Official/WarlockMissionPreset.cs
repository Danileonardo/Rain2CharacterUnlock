using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    public static partial class BuiltInMissionPresetCatalog
    {
        private static MissionPreset CreateWarlockOfficial()
        {
            // Cualquier survivor puede reunir las 10 bajas de ImpBody.
            // La recompensa es exclusivamente la del proveedor USU activo.
            MissionObjective souls = CreateKillObjective(
                "warlock_ten_imp_souls", "SpecificBody", "ImpBody", 10d);
            souls.ResetScope = "Run";

            // El Señor de los diablillos debe morir DESPUÉS de las 10 bajas.
            // El requisito de orden está implementado en KillObjectiveHandler
            // desde la Fase A de Seamstress (afterObjectiveId).
            MissionObjective overlord = CreateKillObjective(
                "warlock_overlord_after_souls", "SpecificBoss", "ImpBossBody", 1d);
            overlord.ResetScope = "Run";
            overlord.Parameters = new JObject
            {
                ["afterObjectiveId"] = "warlock_ten_imp_souls"
            };

            return CreateMissionPreset(
                MissionPresetIds.WarlockOfficial,
                MissionPresetCategories.CharacterRecipe,
                "WarlockBody",
                "Tributo al trono",
                "Arrebata 10 almas de diablillos;\n" +
                "después vence a un Señor de los diablillos.",
                true, false, "Shared",
                CreateRoute("warlock_tribute_to_throne",
                    new[] { souls, overlord }, null,
                    new MissionRules { SingleRun = true }));
        }
    }
}
