using Newtonsoft.Json.Linq;
namespace UniversalSurvivorUnlocks
{
    public static partial class BuiltInMissionPresetCatalog
    {
        private static MissionPreset CreateShadowOfficial()
        {
            var nail = new MissionObjective { Id = "shadow_power_nail_30", Type = "ShadowPowerWeaponHits",
                Amount = 30d, ResetScope = "Run", Parameters = new JObject { ["weapon"] = "nail" } };
            var rebar = new MissionObjective { Id = "shadow_power_rebar_10", Type = "ShadowPowerWeaponHits",
                Amount = 10d, ResetScope = "Run", Parameters = new JObject { ["weapon"] = "rebar" } };
            return MissionModeConfiguration.Individual(CreateMissionPreset(
                MissionPresetIds.ShadowOfficial, MissionPresetCategories.CharacterRecipe,
                "ShadowBody", "Protocolo Omega",
                "Un arsenal digno de Omega requiere control;\n" +
                "en Modo de Poder, alcanza 30 con clavos y 10 con barras.",
                true, false, "PerPlayer", CreateRoute("shadow_power_arsenal",
                    new[] { nail, rebar }, new[] { CreateRequiredSurvivorCondition("ToolbotBody") },
                    new MissionRules { SingleRun = true })));
        }
    }
}
