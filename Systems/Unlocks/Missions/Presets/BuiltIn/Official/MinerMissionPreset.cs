using System;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    public static partial class BuiltInMissionPresetCatalog
    {
        // =========================================================
        // MISIÓN OFICIAL - MINER
        // =========================================================

        private static MissionPreset CreateMinerOfficial()
        {
            // Adaptación estable de "Excavation" (RoR/RoRR).
            //
            // El Direseeker integrado de MinerUnearthed 1.10.0 está
            // deshabilitado actualmente, así que USU no debe depender de
            // que ese boss o el mod standalone estén instalados.
            //
            // Abyssal Depths es la continuación/adaptación de Magma
            // Barracks en RoR2. Derrotar al jefe real del teletransportador
            // mantiene el espíritu de "despejar el camino" sin RNG de un
            // enemigo específico ni hooks sobre el achievement del creador.
            MissionObjective teleporterBoss =
                CreateKillObjective(
                    "miner_abyssal_boss",
                    "TeleporterBoss",
                    "",
                    1d
                );


            teleporterBoss.ResetScope =
                "Stage";


            AddObjectiveConditions(
                teleporterBoss,
                CreateCondition(
                    "FatalHit",
                    new JObject()
                )
            );


            return CreateMissionPreset(
                MissionPresetIds.MinerOfficial,
                MissionPresetCategories.CharacterRecipe,
                "MinerBody",
                "Excavación",
                "Desciende a las Profundidades Abisales;\nvence al jefe del portal y despeja el camino.",
                true,
                false,
                "PerPlayer",

                CreateRoute(
                    "miner_excavation",

                    new MissionObjective[]
                    {
                        teleporterBoss
                    },

                    new MissionCondition[]
                    {
                        CreateCondition(
                            "RequiredStage",
                            new JObject
                            {
                                ["stage"] = "dampcavesimple"
                            }
                        )
                    },

                    new MissionRules
                    {
                        SingleRun = true
                    }
                )
            );
        }
    }
}
