using System;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    public static partial class BuiltInMissionPresetCatalog
    {
        // =========================================================
        // MISIÓN OFICIAL - SORA
        // =========================================================

        private static MissionPreset CreateSoraOfficial()
        {
            MissionObjective shadows = new MissionObjective
            {
                Id = "umbra_waves_3",
                Type = "DefeatUmbraWaves",
                Amount = 3d,
                ResetScope = "Run",
                Parameters = new JObject { ["stage"] = "artifactworld" }
            };

            MissionObjective leave = new MissionObjective
            {
                Id = "leave_ambry",
                Type = "LeaveStage",
                Amount = 1d,
                ResetScope = "Run",
                Parameters = new JObject { ["stage"] = "artifactworld" }
            };

            return CreateMissionPreset(
                MissionPresetIds.SoraOfficial,
                MissionPresetCategories.CharacterRecipe,
                "SoraBody",
                "Elegido de la Llave Espada",
                "Abre paso entre mundos en Baluarte de Ambry;\nvence a sombras y completa Venganza - Mercenary",
                true,
                false,
                "Shared",
                CreateRoute(
                    "sora_vengeance",
                    new MissionObjective[] { shadows, leave },
                    new MissionCondition[]
                    {
                        CreateCondition("PartyHasSurvivor", new JObject
                        {
                            ["bodies"] = new JArray("MercBody")
                        })
                    },
                    new MissionRules { SingleRun = true }
                )
            );
        }
    }
}
