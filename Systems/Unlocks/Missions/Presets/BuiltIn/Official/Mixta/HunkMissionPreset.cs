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
            // Mixta: cada portador válido puede obtener el desbloqueo.
            // El handler invalida individualmente al portador si muere o la pierde.
            MissionObjective carryEscape = CreateFuelArrayCarryObjective("carry_sample_escape");
            MissionObjective escape = new MissionObjective
            {
                Id = "escape_moon",
                Type = "CompleteEnding",
                Amount = 1d,
                ResetScope = "Run",
                Parameters = new JObject { ["ending"] = "MoonEscape" }
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

            return MissionModeConfiguration.Mixed(CreateMissionPreset(
                MissionPresetIds.HunkOfficial,
                MissionPresetCategories.CharacterRecipe,
                "RobHunkBody",
                "La Parca No Falla",
                "Una orden: protege la Matriz de combustible;\nescapa de la Luna o sacrifícate en el Obelisco.",
                true,
                false,
                "PerPlayer",
                CreateRoute("hunk_escape", new MissionObjective[] { carryEscape, escape }, null, new MissionRules { SingleRun = true, ResetOnDeath = false }),
                CreateRoute("hunk_obliterate", new MissionObjective[] { carryObliterate, obliterate }, null, new MissionRules { SingleRun = true, ResetOnDeath = false })
            ));
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
