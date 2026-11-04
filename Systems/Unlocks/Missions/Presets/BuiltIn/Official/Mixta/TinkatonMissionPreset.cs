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
            // Los dos prerrequisitos pueden completarse en cualquier orden,
            // pero la muerte del Ojo solo cuenta DESPUES de ambos.
            // Nunca recontar una muerte ocurrida anteriormente.
            eye.Parameters["afterObjectiveIds"] = new JArray(
                "scrap_6", "justice_demolisher");
            AddObjectiveConditions(
                eye,
                CreateCondition("FatalHit", new JObject())
            );

            return MissionModeConfiguration.Mixed(CreateMissionPreset(
                MissionPresetIds.TinkatonOfficial,
                MissionPresetCategories.CharacterRecipe,
                "TinkatonBody",
                "Forjada en Chatarra",
                "De 6 objetos reciclados nace tu gran martillo;\nten Justicia demoledora y vence un Ojo mecánico.",
                true,
                false,
                "PerPlayer",
                CreateRoute(
                    "tinkaton_main",
                    new MissionObjective[] { scrap, hammer, eye },
                    null,
                    new MissionRules { SingleRun = true }
                )
            ));
        }
    }
}
