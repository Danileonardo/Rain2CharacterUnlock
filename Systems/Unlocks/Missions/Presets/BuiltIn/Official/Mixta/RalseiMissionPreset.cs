using System;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    public static partial class BuiltInMissionPresetCatalog
    {
        // =========================================================
        // MISIÓN OFICIAL - RALSEI
        // =========================================================

        private static MissionPreset CreateRalseiOfficial()
        {
            MissionObjective recruit = new MissionObjective
            {
                Id = "devotion_friends_3",
                Type = "RecruitMinions",
                Amount = 3d,
                ResetScope = "Run",
                Target = new MissionTarget { Category = "Ally" },
                Parameters = new JObject { ["markerItem"] = "LemurianHarness" }
            };

            MissionObjective teleporter = new MissionObjective
            {
                Id = "portal_with_friends",
                Type = "CompleteTeleporter",
                Amount = 1d,
                ResetScope = "Run"
            };

            // Se eliminaron expresamente los requisitos de conservar vivos
            // a los Lemurianos. Cuentan todos los reclutamientos del equipo,
            // incluso cuando un devoto muere antes de activar el portal.

            return MissionModeConfiguration.Mixed(CreateMissionPreset(
                MissionPresetIds.RalseiOfficial,
                MissionPresetCategories.CharacterRecipe,
                "RalseiBody",
                "El poder de la bondad",
                "La Devoción reúne a los extraviados;\nrecluta 3 Lemurianos y completa el portal.",
                true,
                false,
                "PerPlayer",
                CreateRoute(
                    "ralsei_devotion",
                    new MissionObjective[] { recruit, teleporter },
                    null,
                    new MissionRules { SingleRun = true }
                )
            ));
        }
    }
}
