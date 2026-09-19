using System;
using System.Collections.Generic;

using BepInEx.Logging;
using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    public static partial class MissionAssignmentService
    {
        // =========================================================
        // ASIGNAR / ACTIVAR CUSTOM
        // =========================================================
        //
        // customMission != null:
        //     guarda esa nueva copia Custom y la activa.
        //
        // customMission == null:
        //     reactiva la CustomMission que ya estuviera conservada en
        //     MissionConfig. Esto permite cambiar USU -> Custom sin perder
        //     la copia personalizada anterior.
        //
        // basePresetId es opcional. Si está vacío se conserva el ID previo.
        // =========================================================

        public static MissionAssignmentResult AssignCustom(
            string bodyName,
            MissionDefinition customMission,
            ManualLogSource logger = null,
            string basePresetId = ""
        )
        {
            string normalizedBodyName =
                bodyName?.Trim() ?? "";


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


                MissionDefinition missionToUse =
                    customMission != null
                        ? CloneMissionOrNull(
                            customMission
                        )
                        : CloneMissionOrNull(
                            missionConfig.CustomMission
                        );


                if (
                    !IsValidMission(
                        missionToUse
                    )
                )
                {
                    RestoreChallenge(
                        entry,
                        originalChallenge
                    );

                    return MissionAssignmentResult.CreateFailure(
                        MissionAssignmentResultCodes.CustomMissionNotAvailable,
                        "No existe una misión Custom válida para activar.",
                        normalizedBodyName
                    );
                }


                missionConfig.Provider =
                    UnlockProviderKind.Custom;

                missionConfig.SelectionMode =
                    UnlockSelectionMode.UserSelected;

                missionConfig.Source =
                    "Custom";


                string normalizedBasePresetId =
                    basePresetId?.Trim() ?? "";


                if (
                    !string.IsNullOrWhiteSpace(
                        normalizedBasePresetId
                    )
                )
                {
                    missionConfig.BasePresetId =
                        normalizedBasePresetId;
                }


                missionConfig.CustomMission =
                    CloneMissionOrNull(
                        missionToUse
                    );


                challenge.Mission =
                    CloneMissionOrNull(
                        missionToUse
                    );

                challenge.Enabled =
                    true;

                NormalizeMissionV2CompatibilityMetadata(
                    challenge
                );

                // Custom usa texto literal de Survivors.json. El editor de
                // nombre/descripcion llega después; aquí sólo evitamos que
                // una localizationKey oficial pise esos textos.
                challenge.LocalizationKey =
                    "";


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
                        "No se pudo guardar la misión Custom seleccionada.",
                        normalizedBodyName
                    );
                }


                ApplyRuntimeChange(
                    normalizedBodyName,
                    logger,
                    "misión Custom"
                );


                string displayName =
                    GetDisplayName(
                        entry,
                        normalizedBodyName
                    );

                string customName =
                    !string.IsNullOrWhiteSpace(
                        challenge.Name
                    )
                        ? challenge.Name
                        : "Misión personalizada";


                logger?.LogInfo(
                    "[MISSION ASSIGNMENT] " +
                    $"Custom seleccionado | " +
                    $"Body: {normalizedBodyName} | " +
                    $"BasePreset: {missionConfig.BasePresetId}"
                );


                return MissionAssignmentResult.CreateSuccess(
                    normalizedBodyName,
                    missionConfig.BasePresetId ?? "",
                    displayName,
                    customName,
                    $"'{displayName}' ahora usa su misión Custom."
                );
            }
            catch (
                Exception exception
            )
            {
                RestoreChallenge(
                    entry,
                    originalChallenge
                );


                logger?.LogError(
                    "[MISSION ASSIGNMENT] No se pudo asignar la misión Custom."
                );

                logger?.LogError(
                    exception.Message
                );


                return MissionAssignmentResult.CreateFailure(
                    MissionAssignmentResultCodes.Unknown,
                    "Ocurrió un error al asignar la misión Custom.",
                    normalizedBodyName
                );
            }
        }


        /// <summary>
        /// Reactiva la copia Custom ya almacenada para el survivor.
        /// </summary>
        public static MissionAssignmentResult SelectCustom(
            string bodyName,
            ManualLogSource logger = null
        )
        {
            return AssignCustom(
                bodyName,
                null,
                logger,
                ""
            );
        }
    }
}
