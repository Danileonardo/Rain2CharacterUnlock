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
                Id = "vengeance_waves_3",
                Type = "DefeatUmbraWaves",
                Amount = 3d,
                ResetScope = "Run",
                // Identifica Umbrae por InvadingDoppelganger y acumula oleadas,
                // no muertes individuales. El handler existente agrupa las
                // bajas de un enfrentamiento según los jugadores presentes.
                // La ruta oficial restringe el progreso a las cuatro
                // variantes de Baluarte de Ambry (incluyendo DLC).
                // Cada oleada equivale a las Umbrae de los jugadores presentes.
                Parameters = new JObject { ["countIndividualUmbrae"] = false }
            };

            return MissionModeConfiguration.Mixed(CreateMissionPreset(
                MissionPresetIds.SoraOfficial,
                MissionPresetCategories.CharacterRecipe,
                "SoraBody",
                "Elegido de la Llave Espada",
                "Cruza la oscuridad del Baluarte de Ambry;\nvence 3 oleadas de sombras de Venganza.",
                true,
                false,
                "Shared",
                CreateRoute(
                    "sora_vengeance",
                    new MissionObjective[] { shadows },
                    new MissionCondition[]
                    {
                        CreateCondition("PartyHasSurvivor", new JObject
                        {
                            ["bodies"] = new JArray("MercBody")
                        }),
                        // Se admiten todas las variantes oficiales del
                        // Baluarte de Ambry, no las invasiones exteriores.
                        CreateCondition("RequiredStage", new JObject
                        {
                            ["stages"] = new JArray(
                                "artifactworld",
                                "artifactworld01",
                                "artifactworld02",
                                "artifactworld03")
                        })
                    },
                    new MissionRules { SingleRun = true }
                )
            ));
        }
    }
}
