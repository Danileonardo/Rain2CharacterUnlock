using System;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    public static partial class BuiltInMissionPresetCatalog
    {
        // =========================================================
        // MISIÓN OFICIAL - ROCKET
        // =========================================================

        private static MissionPreset CreateRocketOfficial()
        {
            MissionObjective bombing = new MissionObjective
            {
                Id = "bombing_runs_3",
                Type = "BombingRun",
                Amount = 3d,
                ResetScope = "Stage",
                Parameters = new JObject
                {
                    ["killsPerRun"] = 5,
                    ["requireLandingBetweenRuns"] = true,
                    ["countOwnedMinions"] = false
                }
            };

            return CreateMissionPreset(
                MissionPresetIds.RocketOfficial,
                MissionPresetCategories.CharacterRecipe,
                "RocketSurvivorBody",
                "La gravedad es opcional",
                "Haz llover explosiones desde el cielo;\nderriba 5 antes de caer; haz la hazaña 3 veces.",
                true,
                false,
                "PerPlayer",
                CreateRoute("rocket_main", new MissionObjective[] { bombing }, null, new MissionRules { SingleRun = true })
            );
        }
    }
}
