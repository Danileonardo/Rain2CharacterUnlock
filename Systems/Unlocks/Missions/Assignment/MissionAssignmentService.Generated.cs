using System;

using BepInEx.Logging;

namespace UniversalSurvivorUnlocks
{
    public static partial class MissionAssignmentService
    {
        /// <summary>
        /// Reactiva la misión Generated dormida sin convertirla a Custom ni
        /// reemplazarla por un preset oficial de USU.
        ///
        /// Se utiliza desde la UI cuando la fila de providers se separa en:
        /// Original - USU - Custom - Generated.
        /// </summary>
        public static MissionAssignmentResult SelectGenerated(
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

            SurvivorChallengeJson originalChallenge =
                CloneChallengeOrNull(
                    entry.Challenge
                );

            try
            {
                SurvivorChallengeJson challenge =
                    EnsureChallenge(
                        entry
                    );

                MissionConfiguration missionConfig =
                    EnsureMissionConfiguration(
                        challenge
                    );

                // Conserva cualquier fuente activa antes de volver a
                // Generated: preset USU, Custom o Generated actual.
                MissionProviderSlotStore.CaptureActiveSlots(
                    challenge
                );

                if (
                    !string.Equals(
                        missionConfig.Source,
                        "Generated",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    if (!MissionProviderSlotStore.RestoreGenerated(challenge))
                    {
                        return MissionAssignmentResult.CreateFailure(
                            MissionAssignmentResultCodes.PresetNotAssignable,
                            "No existe una misión Generated persistida para reactivar.",
                            normalizedBodyName
                        );
                    }

                    missionConfig =
                        EnsureMissionConfiguration(
                            challenge
                        );
                }
                else
                {
                    MissionProviderSlotStore.CaptureActiveGenerated(
                        challenge
                    );
                }

                missionConfig.Provider =
                    UnlockProviderKind.USU;

                missionConfig.SelectionMode =
                    UnlockSelectionMode.UserSelected;

                if (
                    !PersistOrRollback(
                        entry,
                        originalChallenge,
                        logger
                    )
                )
                {
                    return MissionAssignmentResult.CreateFailure(
                        MissionAssignmentResultCodes.PersistenceFailed,
                        "No se pudo guardar la selección del proveedor Generated.",
                        normalizedBodyName
                    );
                }

                ApplyRuntimeChange(
                    normalizedBodyName,
                    logger,
                    "provider Generated"
                );

                string displayName =
                    GetDisplayName(
                        entry,
                        normalizedBodyName
                    );

                logger?.LogInfo(
                    "[MISSION ASSIGNMENT] " +
                    $"Generated seleccionada | " +
                    $"Body: {normalizedBodyName}"
                );

                return MissionAssignmentResult.CreateSuccess(
                    normalizedBodyName,
                    missionConfig.BasePresetId ?? "",
                    displayName,
                    "Generated",
                    $"'{displayName}' ahora usa la misión Generated persistida."
                );
            }
            catch (Exception exception)
            {
                RestoreChallenge(
                    entry,
                    originalChallenge
                );

                logger?.LogError(
                    "[MISSION ASSIGNMENT] No se pudo seleccionar Generated."
                );

                logger?.LogError(
                    exception.Message
                );

                return MissionAssignmentResult.CreateFailure(
                    MissionAssignmentResultCodes.Unknown,
                    "Ocurrió un error al seleccionar el proveedor Generated.",
                    normalizedBodyName
                );
            }
        }
    }
}
