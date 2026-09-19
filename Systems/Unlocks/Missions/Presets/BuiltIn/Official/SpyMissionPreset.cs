using System;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    public static partial class BuiltInMissionPresetCatalog
    {
        // =========================================================
        // MISIÓN OFICIAL - SPY
        // =========================================================

        private static MissionPreset CreateSpyOfficial()
        {
            MissionObjective kill = CreateKillObjective("spy_boss", "Boss", "", 1d);
            AddObjectiveConditions(
                kill,
                CreateRequiredSkillCondition("Secondary", "BANDIT2_SECONDARY_NAME"),
                CreateCondition("Backstab", new JObject()),
                CreateCondition("FatalHit", new JObject())
            );

            return CreateMissionPreset(
                MissionPresetIds.SpyOfficial,
                MissionPresetCategories.CharacterRecipe,
                "SpyBody",
                "Sin que me veas venir",
                "Que el jefe nunca vea venir tu golpe final;\nremátalo por detrás con Daga serrada - Bandit",
                true,
                false,
                "PerPlayer",
                CreateRoute(
                    "spy_main",
                    new MissionObjective[] { kill },
                    new MissionCondition[] { CreateRequiredSurvivorCondition("Bandit2Body") },
                    new MissionRules { SingleRun = true }
                )
            );
        }
    }
}
