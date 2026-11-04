using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    public static partial class BuiltInMissionPresetCatalog
    {
        // "El filo de los olvidados". Cuenta impactos verificados sobre
        // 12 enemigos distintos Exposed con Blinding Assault y una
        // Daga ceremonial, con progreso individual por run.
        private static MissionPreset CreateBladedancerOfficial()
        {
            MissionObjective exposedDashHits = new MissionObjective
            {
                Id = "bladedancer_exposed_dash_unique_12",
                Type = "ExposeDashHits",
                Amount = 12d,
                ResetScope = "Run",
                Parameters = new JObject
                {
                    ["requiredItem"] = "Dagger",
                    ["requiredItemCount"] = 1
                }
            };

            return MissionModeConfiguration.Individual(CreateMissionPreset(
                MissionPresetIds.BladedancerOfficial,
                MissionPresetCategories.CharacterRecipe,
                "IreliaBody",
                "El filo de los olvidados",
                "Que la Daga ceremonial acompañe tu danza;\n" +
                "usa Asalto cegador en 12 enemigos expuestos.",
                true, false, "PerPlayer",
                CreateRoute(
                    "bladedancer_edge_of_navori",
                    new[] { exposedDashHits },
                    new[] { CreateRequiredSurvivorCondition("MercBody") },
                    new MissionRules { SingleRun = true }
                )
            ));
        }
    }
}
