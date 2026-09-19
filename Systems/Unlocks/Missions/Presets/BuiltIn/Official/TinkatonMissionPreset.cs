using System;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    public static partial class BuiltInMissionPresetCatalog
    {
        // =========================================================
        // MISIÓN OFICIAL - TINKATON
        // =========================================================

        private static MissionPreset CreateTinkatonOfficial()
        {
            MissionObjective scrap = new MissionObjective
            {
                Id = "scrap_6",
                Type = "ScrapItems",
                Amount = 6d,
                ResetScope = "Run"
            };

            MissionObjective hammer = new MissionObjective
            {
                Id = "justice_demolisher",
                Type = "HoldItemStack",
                Amount = 1d,
                ResetScope = "Run",
                Parameters = new JObject { ["item"] = "ArmorReductionOnHit" }
            };

            MissionObjective eye = CreateKillObjective(
                "mechanical_eye",
                "SpecificBody",
                "SuperRoboBallBossBody|RoboBallBossBody",
                1d
            );
            AddObjectiveConditions(
                eye,
                CreateCondition("FatalHit", new JObject())
            );

            return CreateMissionPreset(
                MissionPresetIds.TinkatonOfficial,
                MissionPresetCategories.CharacterRecipe,
                "TinkatonBody",
                "Forjada en Chatarra",
                "Haz de 6 chatarras el inicio de tu gran golpe;\nten Justicia demoledora y vence un Ojo mecánico.",
                true,
                false,
                "PerPlayer",
                CreateRoute(
                    "tinkaton_main",
                    new MissionObjective[] { scrap, hammer, eye },
                    null,
                    new MissionRules { SingleRun = true }
                )
            );
        }
    }
}
