using System;
using System.Collections.Generic;

using BepInEx.Logging;
using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    public static partial class MissionAssignmentService
    {
        // =========================================================
        // VALIDAR ASIGNACIÓN DE PRESET
        // =========================================================

        public static MissionAssignmentResult ValidatePresetAssignment(
            string bodyName,
            string presetId
        )
        {
            string normalizedBodyName =
                bodyName?.Trim() ?? "";

            string normalizedPresetId =
                presetId?.Trim() ?? "";


            if (
                string.IsNullOrWhiteSpace(
                    normalizedBodyName
                )
            )
            {
                return MissionAssignmentResult.CreateFailure(
                    MissionAssignmentResultCodes.InvalidBodyName,
                    "El BodyName del survivor no puede estar vacío."
                );
            }


            SurvivorJsonEntry entry =
                SurvivorJsonManager.GetEntryAnywhere(
                    normalizedBodyName
                );


            if (entry == null)
            {
                return MissionAssignmentResult.CreateFailure(
                    MissionAssignmentResultCodes.SurvivorNotFound,
                    $"No existe una configuración de USU para '{normalizedBodyName}'.",
                    normalizedBodyName,
                    normalizedPresetId
                );
            }


            if (
                string.IsNullOrWhiteSpace(
                    normalizedPresetId
                )
            )
            {
                return MissionAssignmentResult.CreateFailure(
                    MissionAssignmentResultCodes.InvalidPresetId,
                    "El PresetId no puede estar vacío.",
                    normalizedBodyName
                );
            }


            // Política 0.3.1:
            // Original sigue protegido frente a presets arbitrarios. La única
            // excepción general es la misión Official USU propia del survivor,
            // que puede coexistir y activarse sólo por elección explícita.
            if (
                !MissionProviderSelectionPolicy.CanAssignUsuPreset(
                    entry,
                    normalizedPresetId
                )
            )
            {
                return MissionAssignmentResult.CreateFailure(
                    MissionAssignmentResultCodes.OriginalProviderProtected,
                    "Este personaje ya tiene un sistema de desbloqueo Original. " +
                    "Sólo puede alternarse a su misión Official USU propia o a una Custom guardada.",
                    normalizedBodyName,
                    normalizedPresetId
                );
            }


            if (
                !MissionPresetLibraryService
                    .TryGetAssignableMissionPreset(
                        normalizedPresetId,
                        out MissionPreset preset
                    )
            )
            {
                return MissionAssignmentResult.CreateFailure(
                    MissionAssignmentResultCodes.PresetNotAssignable,
                    $"El preset '{normalizedPresetId}' no existe o no puede asignarse.",
                    normalizedBodyName,
                    normalizedPresetId
                );
            }


            if (
                !IsValidMission(
                    preset.Mission
                )
            )
            {
                return MissionAssignmentResult.CreateFailure(
                    MissionAssignmentResultCodes.InvalidMission,
                    $"El preset '{normalizedPresetId}' no contiene una misión v2 válida.",
                    normalizedBodyName,
                    normalizedPresetId
                );
            }


            string displayName =
                GetDisplayName(
                    entry,
                    normalizedBodyName
                );


            string presetName =
                !string.IsNullOrWhiteSpace(
                    preset.Name
                )
                    ? preset.Name
                    : normalizedPresetId;


            return MissionAssignmentResult.CreateSuccess(
                normalizedBodyName,
                normalizedPresetId,
                displayName,
                presetName,
                $"'{presetName}' puede asignarse a '{displayName}'."
            );
        }


        // =========================================================
        // SELECCIONAR ALTERNATIVA USU OFICIAL
        // =========================================================
        //
        // Algunos survivors pueden declarar explícitamente una misión USU
        // alternativa aun teniendo un unlock Original. El caso actual es
        // Miner: Original permanece intacto, pero el jugador puede alternar
        // directamente a la misión oficial USU "Excavación".
        // =========================================================

        public static MissionAssignmentResult SelectOfficialUsuAlternative(
            string bodyName,
            ManualLogSource logger = null
        )
        {
            string normalizedBodyName =
                bodyName?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(normalizedBodyName))
            {
                return MissionAssignmentResult.CreateFailure(
                    MissionAssignmentResultCodes.InvalidBodyName,
                    "El BodyName del survivor no puede estar vacío."
                );
            }

            SurvivorJsonEntry entry =
                SurvivorJsonManager.GetEntryAnywhere(
                    normalizedBodyName
                );

            if (entry == null)
            {
                return MissionAssignmentResult.CreateFailure(
                    MissionAssignmentResultCodes.SurvivorNotFound,
                    $"No existe una configuración de USU para '{normalizedBodyName}'.",
                    normalizedBodyName
                );
            }

            if (
                !MissionProviderSelectionPolicy.TryResolveDirectOfficialUsuDescriptor(
                    entry,
                    out OfficialMissionPresetDescriptor descriptor
                )
            )
            {
                return MissionAssignmentResult.CreateFailure(
                    MissionAssignmentResultCodes.OriginalProviderProtected,
                    "Este personaje no tiene una alternativa USU oficial directa.",
                    normalizedBodyName
                );
            }

            return AssignPreset(
                normalizedBodyName,
                descriptor.PresetId,
                logger
            );
        }
    }
}
