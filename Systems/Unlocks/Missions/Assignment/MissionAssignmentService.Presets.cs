using System;
using System.Collections.Generic;

using BepInEx.Logging;
using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    public static partial class MissionAssignmentService
    {
        // =========================================================
        // ASIGNAR PRESET USU
        // =========================================================
        //
        // Asignar un preset USU se permite normalmente cuando no existe
        // un proveedor Original del creador. Los únicos casos con Original
        // que pueden usar USU directamente deben estar declarados de forma
        // explícita en OfficialMissionPresetRegistry.
        //
        // Miner usa esta excepción para tener una misión USU independiente
        // sin modificar ni interceptar su achievement Original.
        //
        // Si existía una CustomMission, se conserva dormida. Cambiar de
        // proveedor NO equivale a restaurar/borrar la configuración Custom.
        // =========================================================

        public static MissionAssignmentResult AssignPreset(
            string bodyName,
            string presetId,
            ManualLogSource logger = null
        )
        {
            MissionAssignmentResult validation =
                ValidatePresetAssignment(
                    bodyName,
                    presetId
                );


            if (!validation.Success)
            {
                return validation;
            }


            string normalizedBodyName =
                validation.BodyName;

            string normalizedPresetId =
                validation.PresetId;


            SurvivorJsonEntry entry =
                SurvivorJsonManager.GetEntryAnywhere(
                    normalizedBodyName
                );


            if (
                entry == null ||
                !MissionPresetLibraryService
                    .TryGetAssignableMissionPreset(
                        normalizedPresetId,
                        out MissionPreset preset
                    )
            )
            {
                return MissionAssignmentResult.CreateFailure(
                    MissionAssignmentResultCodes.PresetNotAssignable,
                    $"El preset '{normalizedPresetId}' dejó de estar disponible antes de guardarse.",
                    normalizedBodyName,
                    normalizedPresetId
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


                // Antes de abandonar la fuente activa, conserva sus slots.
                // Esto permite volver a Generated/USU/Custom sin destruir la
                // alternativa anterior.
                MissionProviderSlotStore.CaptureActiveSlots(
                    challenge
                );


                // La misión custom se conserva aunque quede inactiva.
                MissionDefinition preservedCustomMission =
                    CloneMissionOrNull(
                        missionConfig.CustomMission
                    );


                MissionDefinition runtimeMission =
                    CloneMissionOrNull(
                        preset.Mission
                    );


                if (
                    !IsValidMission(
                        runtimeMission
                    )
                )
                {
                    RestoreChallenge(
                        entry,
                        originalChallenge
                    );

                    return MissionAssignmentResult.CreateFailure(
                        MissionAssignmentResultCodes.InvalidMission,
                        $"No se pudo preparar una copia válida del preset '{normalizedPresetId}'.",
                        normalizedBodyName,
                        normalizedPresetId
                    );
                }


                missionConfig.Provider =
                    UnlockProviderKind.USU;

                missionConfig.SelectionMode =
                    UnlockSelectionMode.UserSelected;

                missionConfig.Source =
                    "Preset";

                missionConfig.BasePresetId =
                    normalizedPresetId;

                missionConfig.UsuPresetId =
                    normalizedPresetId;

                missionConfig.CustomMission =
                    preservedCustomMission;


                challenge.Mission =
                    runtimeMission;

                challenge.Enabled =
                    true;

                challenge.Name =
                    preset.Name ?? "";

                challenge.Description =
                    preset.Description ?? "";

                NormalizeMissionV2CompatibilityMetadata(
                    challenge
                );

                // Un preset puede asignarse a cualquier survivor. Sólo
                // conservamos localización oficial integrada cuando el preset
                // corresponde realmente al survivor destino. Para asignación
                // cruzada la clave queda vacía para no mostrar textos de otro
                // personaje.
                if (
                    OfficialMissionPresetRegistry.TryResolve(
                        normalizedBodyName,
                        entry.Source,
                        out OfficialMissionPresetDescriptor officialDescriptor
                    ) &&
                    officialDescriptor != null &&
                    string.Equals(
                        officialDescriptor.PresetId,
                        normalizedPresetId,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    challenge.LocalizationKey =
                        officialDescriptor.LocalizationKey;

                    // Al volver de Generated, restaurar las monedas de
                    // este preset. Antes quedaba el 0 de la misión generada.
                    challenge.LunarCoinReward =
                        officialDescriptor.LunarCoinReward;
                }
                else
                {
                    challenge.LocalizationKey =
                        "";

                    // Preset cruzado/no oficial: no heredar monedas de la
                    // misión anterior de otro survivor.
                    challenge.LunarCoinReward = 0;
                }


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
                        $"No se pudo guardar la asignación del preset '{normalizedPresetId}'.",
                        normalizedBodyName,
                        normalizedPresetId
                    );
                }


                ApplyRuntimeChange(
                    normalizedBodyName,
                    logger,
                    $"preset USU {normalizedPresetId}"
                );


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


                logger?.LogInfo(
                    "[MISSION ASSIGNMENT] " +
                    $"USU seleccionado | " +
                    $"Body: {normalizedBodyName} | " +
                    $"Preset: {normalizedPresetId}"
                );


                return MissionAssignmentResult.CreateSuccess(
                    normalizedBodyName,
                    normalizedPresetId,
                    displayName,
                    presetName,
                    $"'{displayName}' ahora usa el preset USU '{presetName}'."
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
                    "[MISSION ASSIGNMENT] No se pudo asignar el preset USU."
                );

                logger?.LogError(
                    exception.Message
                );


                return MissionAssignmentResult.CreateFailure(
                    MissionAssignmentResultCodes.Unknown,
                    $"Ocurrió un error al asignar el preset '{normalizedPresetId}'.",
                    normalizedBodyName,
                    normalizedPresetId
                );
            }
        }


        // =========================================================
        // SINCRONIZAR SNAPSHOTS DE PRESETS ASIGNADOS
        // =========================================================
        //
        // Desde 5G.1D-F, BasePresetId es la autoridad cuando
        // MissionConfig.Source == "Preset".
        //
        // Survivors.json conserva una copia de Mission/Name/Description para
        // persistencia legible y compatibilidad, pero esa copia no puede
        // quedar congelada para siempre. Si USU actualiza un preset oficial,
        // cualquier survivor que siga referenciándolo debe recibir la nueva
        // versión en el siguiente arranque.
        //
        // IMPORTANTE:
        // - Provider y SelectionMode NO cambian.
        // - CustomMission dormida NO se elimina ni modifica.
        // - Source == "Custom" jamás se toca automáticamente.
        // - LocalizationKey se conserva: los presets oficiales de su propio
        //   survivor pueden seguir usando localización, mientras que una
        //   asignación cruzada mantiene la clave vacía que puso AssignPreset.
        // =========================================================

        public static int RefreshAssignedPresetSnapshots(
            ManualLogSource logger = null
        )
        {
            SurvivorJsonFile file =
                SurvivorJsonManager.CurrentConfig;


            if (file == null)
            {
                return 0;
            }


            int updated =
                0;


            updated +=
                RefreshAssignedPresetSnapshotsInEntries(
                    file.AvailableSurvivors,
                    logger
                );


            updated +=
                RefreshAssignedPresetSnapshotsInEntries(
                    file.UnavailableSurvivors,
                    logger
                );


            updated +=
                RefreshAssignedPresetSnapshotsInDormantMissions(
                    file.DormantMissions,
                    logger
                );


            if (updated <= 0)
            {
                return 0;
            }


            if (
                !SurvivorJsonManager.TrySaveCurrentConfig(
                    logger
                )
            )
            {
                logger?.LogWarning(
                    "[MISSION PRESET SYNC] Los snapshots fueron " +
                    "actualizados en memoria, pero Survivors.json no pudo " +
                    "guardarse. Se volverá a intentar en el próximo arranque."
                );

                return updated;
            }


            logger?.LogInfo(
                "[MISSION PRESET SYNC] " +
                $"Snapshots actualizados: {updated}"
            );


            return updated;
        }


        private static int RefreshAssignedPresetSnapshotsInEntries(
            Dictionary<string, SurvivorJsonEntry> entries,
            ManualLogSource logger
        )
        {
            if (entries == null)
            {
                return 0;
            }


            int updated =
                0;


            foreach (
                KeyValuePair<string, SurvivorJsonEntry> pair
                in entries
            )
            {
                SurvivorJsonEntry entry =
                    pair.Value;


                SurvivorChallengeJson challenge =
                    entry?.Challenge;


                MissionConfiguration missionConfig =
                    challenge?.MissionConfig;


                if (
                    challenge == null ||
                    missionConfig == null ||
                    !string.Equals(
                        missionConfig.Source,
                        "Preset",
                        StringComparison.OrdinalIgnoreCase
                    ) ||
                    string.IsNullOrWhiteSpace(
                        missionConfig.BasePresetId
                    )
                )
                {
                    continue;
                }


                string presetId =
                    missionConfig.BasePresetId.Trim();


                if (
                    !MissionPresetLibraryService
                        .TryGetAssignableMissionPreset(
                            presetId,
                            out MissionPreset preset
                        ) ||
                    preset == null
                )
                {
                    // No destruimos la copia persistida si el preset deja de
                    // estar disponible temporalmente o cambia de catálogo.
                    UsuLog.Verbose(
                        logger,
                        "[MISSION PRESET SYNC] Preset no disponible; " +
                        "se conserva el snapshot actual | " +
                        $"Body: {entry?.BodyName ?? pair.Key} | " +
                        $"Preset: {presetId}"
                    );

                    continue;
                }


                MissionDefinition refreshedMission =
                    CloneMissionOrNull(
                        preset.Mission
                    );


                if (
                    !IsValidMission(
                        refreshedMission
                    )
                )
                {
                    logger?.LogWarning(
                        "[MISSION PRESET SYNC] Se ignoró un preset sin " +
                        "Mission v2 válida | " +
                        $"Body: {entry?.BodyName ?? pair.Key} | " +
                        $"Preset: {presetId}"
                    );

                    continue;
                }


                string refreshedName =
                    preset.Name ?? "";


                string refreshedDescription =
                    preset.Description ?? "";


                bool missionChanged =
                    !MissionDefinitionsAreEqual(
                        challenge.Mission,
                        refreshedMission
                    );


                bool textChanged =
                    !string.Equals(
                        challenge.Name ?? "",
                        refreshedName,
                        StringComparison.Ordinal
                    ) ||
                    !string.Equals(
                        challenge.Description ?? "",
                        refreshedDescription,
                        StringComparison.Ordinal
                    );


                bool metadataChanged =
                    !HasMissionV2CompatibilityMetadata(
                        challenge
                    );


                if (
                    !missionChanged &&
                    !textChanged &&
                    !metadataChanged
                )
                {
                    continue;
                }


                // MissionConfig es deliberadamente la MISMA instancia.
                // Sólo se refresca el snapshot de la misión oficial.
                challenge.Mission =
                    refreshedMission;

                challenge.Name =
                    refreshedName;

                challenge.Description =
                    refreshedDescription;

                NormalizeMissionV2CompatibilityMetadata(
                    challenge
                );


                updated++;


                logger?.LogInfo(
                    "[MISSION PRESET SYNC] Preset refrescado | " +
                    $"Body: {entry?.BodyName ?? pair.Key} | " +
                    $"Preset: {presetId} | " +
                    $"Provider: {missionConfig.EffectiveProvider} | " +
                    $"Selection: {missionConfig.EffectiveSelectionMode}"
                );
            }


            return updated;
        }


        private static int RefreshAssignedPresetSnapshotsInDormantMissions(
            Dictionary<string, SurvivorMissionRecord> records,
            ManualLogSource logger
        )
        {
            if (records == null)
            {
                return 0;
            }

            int updated = 0;

            foreach (
                KeyValuePair<string, SurvivorMissionRecord> pair
                in records
            )
            {
                SurvivorMissionRecord record = pair.Value;
                SurvivorChallengeJson challenge = record?.Challenge;
                MissionConfiguration missionConfig = challenge?.MissionConfig;

                if (
                    challenge == null ||
                    missionConfig == null ||
                    !string.Equals(
                        missionConfig.Source,
                        "Preset",
                        StringComparison.OrdinalIgnoreCase
                    ) ||
                    string.IsNullOrWhiteSpace(missionConfig.BasePresetId)
                )
                {
                    continue;
                }

                string presetId = missionConfig.BasePresetId.Trim();

                if (
                    !MissionPresetLibraryService
                        .TryGetAssignableMissionPreset(
                            presetId,
                            out MissionPreset preset
                        ) ||
                    preset == null
                )
                {
                    UsuLog.Verbose(
                        logger,
                        "[MISSION PRESET SYNC] Preset dormido no disponible; " +
                        "se conserva el snapshot actual | " +
                        $"Body: {record?.BodyName ?? pair.Key} | " +
                        $"Preset: {presetId}"
                    );

                    continue;
                }

                MissionDefinition refreshedMission =
                    CloneMissionOrNull(
                        preset.Mission
                    );

                if (!IsValidMission(refreshedMission))
                {
                    logger?.LogWarning(
                        "[MISSION PRESET SYNC] Se ignoró un preset dormido " +
                        "sin Mission v2 válida | " +
                        $"Body: {record?.BodyName ?? pair.Key} | " +
                        $"Preset: {presetId}"
                    );

                    continue;
                }

                string refreshedName = preset.Name ?? "";
                string refreshedDescription = preset.Description ?? "";

                bool missionChanged =
                    !MissionDefinitionsAreEqual(
                        challenge.Mission,
                        refreshedMission
                    );

                bool textChanged =
                    !string.Equals(
                        challenge.Name ?? "",
                        refreshedName,
                        StringComparison.Ordinal
                    ) ||
                    !string.Equals(
                        challenge.Description ?? "",
                        refreshedDescription,
                        StringComparison.Ordinal
                    );

                bool metadataChanged =
                    !HasMissionV2CompatibilityMetadata(
                        challenge
                    );

                if (
                    !missionChanged &&
                    !textChanged &&
                    !metadataChanged
                )
                {
                    continue;
                }

                challenge.Mission = refreshedMission;
                challenge.Name = refreshedName;
                challenge.Description = refreshedDescription;

                NormalizeMissionV2CompatibilityMetadata(
                    challenge
                );

                updated++;

                logger?.LogInfo(
                    "[MISSION PRESET SYNC] Preset dormido refrescado | " +
                    $"Body: {record?.BodyName ?? pair.Key} | " +
                    $"Preset: {presetId} | " +
                    $"Provider: {missionConfig.EffectiveProvider} | " +
                    $"Selection: {missionConfig.EffectiveSelectionMode}"
                );
            }

            return updated;
        }


        private static bool HasMissionV2CompatibilityMetadata(
            SurvivorChallengeJson challenge
        )
        {
            if (challenge == null)
            {
                return false;
            }

            return
                string.Equals(
                    challenge.Type,
                    SurvivorChallengePresets.MissionV2ChallengeType,
                    StringComparison.OrdinalIgnoreCase
                ) &&
                challenge.Parameters != null &&
                !challenge.Parameters.HasValues;
        }


        private static void NormalizeMissionV2CompatibilityMetadata(
            SurvivorChallengeJson challenge
        )
        {
            if (challenge == null)
            {
                return;
            }

            challenge.Type =
                SurvivorChallengePresets.MissionV2ChallengeType;

            challenge.Parameters =
                new JObject();
        }


        private static bool MissionDefinitionsAreEqual(
            MissionDefinition first,
            MissionDefinition second
        )
        {
            if (
                first == null &&
                second == null
            )
            {
                return true;
            }


            if (
                first == null ||
                second == null
            )
            {
                return false;
            }


            try
            {
                return JToken.DeepEquals(
                    JToken.FromObject(first),
                    JToken.FromObject(second)
                );
            }
            catch
            {
                return false;
            }
        }
    }
}
