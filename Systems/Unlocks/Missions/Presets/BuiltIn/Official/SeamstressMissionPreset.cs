using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    public static partial class BuiltInMissionPresetCatalog
    {
        private static MissionPreset CreateSeamstressOfficial()
        {
            // Tres compras reales en una misma instancia de ShrineBlood.
            // No basta con usar tres santuarios distintos.
            var shrine = new MissionObjective
            {
                Id = "seamstress_three_sacrifices",
                Type = "InteractWithTarget",
                Amount = 3d,
                ResetScope = "Run",
                Parameters = new JObject
                {
                    ["bloodShrine"] = true,
                    ["sameTargetInstance"] = true
                }
            };

            var boss = CreateKillObjective(
                "seamstress_boss_after_sacrifice",
                "TeleporterBoss", "", 1d);
            boss.ResetScope = "Run";
            boss.Parameters = new JObject
            {
                // El jefe debe morir DESPUÉS del tercer sacrificio.
                ["afterObjectiveId"] = "seamstress_three_sacrifices"
            };

            return CreateMissionPreset(
                MissionPresetIds.SeamstressOfficial,
                MissionPresetCategories.CharacterRecipe,
                "SeamstressBody",
                "Hilo de sangre",
                "Agota un Santuario de sangre con 3 sacrificios;\n" +
                "después vence a un jefe de teletransportador.",
                true, false, "Shared",
                CreateRoute(
                    "seamstress_crimson_thread",
                    new[] { shrine, boss }, null,
                    new MissionRules { SingleRun = true })
            );
        }
    }
}
