using System;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    public static partial class BuiltInMissionPresetCatalog
    {
        // =========================================================
        // MISIÓN OFICIAL - WOOPER
        // =========================================================

        private static MissionPreset CreateWooperOfficial()
        {
            MissionObjective bite = CreateKillObjective("poisoned_bite_20", "Enemy", "", 20d);
            bite.ResetScope = "Stage";
            AddObjectiveConditions(
                bite,
                CreateRequiredSkillCondition("Secondary", "CROCO_SECONDARY_ALT_NAME"),
                CreateCondition("StatusPresent", new JObject
                {
                    ["subject"] = "Target",
                    ["timing"] = "BeforeAction",
                    ["mode"] = "Specific",
                    ["statusId"] = "dot:Poison"
                })
            );

            return CreateMissionPreset(
                MissionPresetIds.WooperOfficial,
                MissionPresetCategories.CharacterRecipe,
                "WooperBody",
                "De vuelta al agua",
                "Haz de los Humedales tu hogar; marca territorio;\ncaza y muerde a 20 presas envenenadas - Acrid",
                true,
                false,
                "PerPlayer",
                CreateRoute(
                    "wooper_main",
                    new MissionObjective[] { bite },
                    new MissionCondition[]
                    {
                        CreateRequiredSurvivorCondition("CrocoBody"),
                        CreateCondition("RequiredStage", new JObject { ["stage"] = "foggyswamp" })
                    },
                    new MissionRules { SingleRun = true }
                )
            );
        }
    }
}
