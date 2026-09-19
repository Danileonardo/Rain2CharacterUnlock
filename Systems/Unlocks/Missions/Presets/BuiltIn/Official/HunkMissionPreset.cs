using System;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    public static partial class BuiltInMissionPresetCatalog
    {
        // =========================================================
        // MISIÓN OFICIAL - HUNK
        // =========================================================

        private static MissionPreset CreateHunkOfficial()
        {
            MissionObjective carryEscape = CreateFuelArrayCarryObjective("carry_sample_escape");
            MissionObjective escape = new MissionObjective
            {
                Id = "escape_moon",
                Type = "CompleteEnding",
                Amount = 1d,
                ResetScope = "Run",
                Parameters = new JObject { ["ending"] = "Escape" }
            };

            MissionObjective carryObliterate = CreateFuelArrayCarryObjective("carry_sample_obliterate");
            MissionObjective obliterate = new MissionObjective
            {
                Id = "obliterate",
                Type = "CompleteEnding",
                Amount = 1d,
                ResetScope = "Run",
                Parameters = new JObject { ["ending"] = "Obliterate" }
            };

            return CreateMissionPreset(
                MissionPresetIds.HunkOfficial,
                MissionPresetCategories.CharacterRecipe,
                "RobHunkBody",
                "La Parca No Falla",
                "Protege la batería y sobrevive a toda costa;\nescapa de la Luna o sacrifícate en el Obelisco.",
                true,
                false,
                "PerPlayer",
                CreateRoute("hunk_escape", new MissionObjective[] { carryEscape, escape }, null, new MissionRules { SingleRun = true, ResetOnDeath = true }),
                CreateRoute("hunk_obliterate", new MissionObjective[] { carryObliterate, obliterate }, null, new MissionRules { SingleRun = true, ResetOnDeath = true })
            );
        }

        private static MissionObjective CreateFuelArrayCarryObjective(string id)
        {
            return new MissionObjective
            {
                Id = id,
                Type = "CarryEquipment",
                Amount = 1d,
                ResetScope = "Run",
                Parameters = new JObject
                {
                    ["equipment"] = "QuestVolatileBattery",
                    ["continuous"] = true,
                    ["failOnLoss"] = true,
                    ["failOnDeath"] = true
                }
            };
        }
    }
}
