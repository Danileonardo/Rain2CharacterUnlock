using System;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    public static partial class BuiltInMissionPresetCatalog
    {
        // =========================================================
        // MISIÓN OFICIAL - SCOUT
        // =========================================================

        private static MissionPreset CreateScoutOfficial()
        {
            MissionObjective drinks = new MissionObjective
            {
                Id = "energy_drinks_8",
                Type = "HoldItemStack",
                Amount = 8d,
                ResetScope = "Run",
                Parameters = new JObject { ["item"] = "SprintBonus" }
            };

            MissionObjective teleporter = new MissionObjective
            {
                Id = "first_teleporter_fast",
                Type = "CompleteTeleporter",
                Amount = 1d,
                ResetScope = "Run"
            };

            AddObjectiveConditions(
                teleporter,
                CreateCondition("StageSequence", new JObject { ["sequence"] = 1 }),
                CreateCondition("TimeLimit", new JObject { ["seconds"] = 240d }),
                CreateCondition("NoItemPickup", new JObject())
            );

            return CreateMissionPreset(
                MissionPresetIds.ScoutOfficial,
                MissionPresetCategories.CharacterRecipe,
                "ScoutBody",
                "Sed Termonuclear",
                "Sacia tu sed con 8 Bebidas energéticas;\no completa el primer sector sin objetos en 4 min.",
                true,
                false,
                "PerPlayer",
                CreateRoute("scout_drinks", new MissionObjective[] { drinks }, null, new MissionRules { SingleRun = true }),
                CreateRoute("scout_batter_up", new MissionObjective[] { teleporter }, null, new MissionRules { SingleRun = true })
            );
        }
    }
}
