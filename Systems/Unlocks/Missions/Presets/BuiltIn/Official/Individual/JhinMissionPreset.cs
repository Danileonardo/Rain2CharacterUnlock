using System;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    public static partial class BuiltInMissionPresetCatalog
    {
        // =========================================================
        // MISIÓN OFICIAL - JHIN
        // =========================================================

        private static MissionPreset CreateJhinOfficial()
        {
            MissionObjective kill =
                CreateKillObjective(
                    "jhin_boss_kill",
                    "TeleporterBoss",
                    "",
                    1d
                );


            AddObjectiveConditions(
                kill,

                CreateCondition(
                    "CriticalHit",
                    new JObject()
                ),

                CreateCondition(
                    "MinimumDamage",
                    new JObject
                    {
                        ["damage"] = 4444d
                    }
                ),

                CreateCondition(
                    "FatalHit",
                    new JObject()
                ),

                CreateExcludedSurvivorCondition(
                    "RailgunnerBody"
                )
            );


            return MissionModeConfiguration.Individual(CreateMissionPreset(
                MissionPresetIds.JhinOfficial,
                MissionPresetCategories.CharacterRecipe,
                "JhinBody",
                "El Cuarto Acto",
                "Que el cuarto acto sea una obra maestra;\nabate a un jefe con un crítico de 4.444+.",
                true,
                false,
                "PerPlayer",

                CreateRoute(
                    "jhin_fourth_act",

                    new MissionObjective[]
                    {
                kill
                    },

                    null,

                    new MissionRules
                    {
                        SingleRun = true
                    }
                )
            ));
        }
    }
}
