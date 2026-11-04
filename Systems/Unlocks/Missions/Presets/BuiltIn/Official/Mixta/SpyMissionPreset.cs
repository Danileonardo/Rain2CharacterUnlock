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

            return MissionModeConfiguration.Mixed(CreateMissionPreset(
                MissionPresetIds.SpyOfficial,
                MissionPresetCategories.CharacterRecipe,
                "SpyBody",
                "Sin que me veas venir",
                "Caballeros, la discreción tiene su precio;\nremata a un jefe por la espalda con Daga serrada.",
                true,
                false,
                "PerPlayer",
                CreateRoute(
                    "spy_main",
                    new MissionObjective[] { kill },
                    new MissionCondition[] { CreateRequiredSurvivorCondition("Bandit2Body") },
                    new MissionRules { SingleRun = true }
                )
            ));
        }
    }
}
