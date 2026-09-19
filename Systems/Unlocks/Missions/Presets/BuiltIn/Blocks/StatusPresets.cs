using System;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    public static partial class BuiltInMissionPresetCatalog
    {
        // =========================================================
        // BLOQUES - ESTADOS
        // =========================================================

        private static void RegisterStatusPresets()
        {
            RegisterStatusCondition(
                MissionPresetIds.StatusSpecific,
                "Estado específico",
                "Specific",
                ""
            );


            RegisterStatusCondition(
                MissionPresetIds.StatusAnyValid,
                "Cualquier efecto válido",
                "AnyValid",
                ""
            );


            RegisterStatusCondition(
                MissionPresetIds.StatusAnyNegative,
                "Cualquier efecto negativo",
                "AnyNegative",
                ""
            );


            RegisterStatusCondition(
                MissionPresetIds.StatusAnyPositive,
                "Cualquier efecto positivo",
                "AnyPositive",
                ""
            );


            RegisterStatusCondition(
                MissionPresetIds.StatusAnyDot,
                "Cualquier DoT",
                "AnyDot",
                ""
            );


            RegisterStatusCondition(
                MissionPresetIds.StatusPoison,
                "Objetivo envenenado",
                "Specific",
                "dot:Poison"
            );


            RegisterStatusCondition(
                MissionPresetIds.StatusBurn,
                "Objetivo ardiendo",
                "Specific",
                "dot:Burn"
            );


            RegisterStatusCondition(
                MissionPresetIds.StatusBleed,
                "Objetivo sangrando",
                "Specific",
                "dot:Bleed"
            );


            RegisterStatusCondition(
                MissionPresetIds.StatusFreeze,
                "Objetivo congelado",
                "Specific",
                "cc:Freeze"
            );


            RegisterStatusCondition(
                MissionPresetIds.StatusStun,
                "Objetivo aturdido",
                "Specific",
                "cc:Stun"
            );
        }
    }
}
