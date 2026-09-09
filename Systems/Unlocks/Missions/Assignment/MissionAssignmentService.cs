using System;
using System.Collections.Generic;

using BepInEx.Logging;
using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Punto único para asignar el proveedor y la misión efectiva de un
    /// survivor.
    ///
    /// Paso 5G.1D-E:
    /// - valida presets realmente asignables;
    /// - permite seleccionar Original;
    /// - permite asignar un preset de USU;
    /// - permite activar/guardar una misión Custom;
    /// - permite restaurar el personaje completo a su política base;
    /// - elimina únicamente las copias Custom del jugador al restaurar;
    /// - persiste cada operación en Survivors.json con rollback;
    /// - solicita a MissionRuntimeRefreshService aplicar el provider/misión
    ///   inmediatamente cuando el runtime actual lo permite.
    /// </summary>
    public static class MissionAssignmentService
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


            // Política 5G.2C FIX1:
            // cuando el survivor ya posee un sistema de desbloqueo Original
            // del creador, USU no lo reemplaza directamente con un preset.
            // Las variantes del jugador se crearán como CUSTOM en 5H/5I.
            if (
                SurvivorUnlockManager.HasStoredOriginalUnlock(
                    entry
                )
            )
            {
                return MissionAssignmentResult.CreateFailure(
                    MissionAssignmentResultCodes.OriginalProviderProtected,
                    "Este personaje ya tiene un sistema de desbloqueo Original. " +
                    "USU lo respeta; crea una variante CUSTOM para modificarlo.",
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
        // ASIGNAR PRESET USU
        // =========================================================
        //
        // Asignar un preset USU sólo se permite cuando no existe un
        // proveedor Original del creador. En ese caso USU actúa como fallback
        // y la selección explícita pasa a UserSelected.
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

                // Un preset puede asignarse a cualquier survivor. No debemos
                // conservar la localizationKey del preset anterior del target,
                // porque mostraría el nombre/descripcion de otro personaje.
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


                if (
                    !missionChanged &&
                    !textChanged
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


        // =========================================================
        // SELECCIONAR ORIGINAL
        // =========================================================
        //
        // Original NO es una copia de una misión del creador.
        // Significa que USU dejará de controlar el desbloqueo cuando la capa
        // runtime de 5G.1D-E aplique el provider.
        //
        // Importante: no se borra Preset, CustomMission ni Mission. Quedan
        // dormidos para que el jugador pueda volver a seleccionarlos.
        // =========================================================

        public static MissionAssignmentResult SelectOriginal(
            string bodyName,
            ManualLogSource logger = null
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


                missionConfig.Provider =
                    UnlockProviderKind.Original;

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
                        "No se pudo guardar la selección del proveedor Original.",
                        normalizedBodyName
                    );
                }


                ApplyRuntimeChange(
                    normalizedBodyName,
                    logger,
                    "provider Original"
                );


                string displayName =
                    GetDisplayName(
                        entry,
                        normalizedBodyName
                    );


                logger?.LogInfo(
                    "[MISSION ASSIGNMENT] " +
                    $"Original seleccionado | " +
                    $"Body: {normalizedBodyName}"
                );


                return MissionAssignmentResult.CreateSuccess(
                    normalizedBodyName,
                    missionConfig.BasePresetId ?? "",
                    displayName,
                    "Original",
                    $"'{displayName}' ahora usa el comportamiento Original."
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
                    "[MISSION ASSIGNMENT] No se pudo seleccionar Original."
                );

                logger?.LogError(
                    exception.Message
                );


                return MissionAssignmentResult.CreateFailure(
                    MissionAssignmentResultCodes.Unknown,
                    "Ocurrió un error al seleccionar el proveedor Original.",
                    normalizedBodyName
                );
            }
        }


        // =========================================================
        // RESTAURAR PERSONAJE COMPLETO
        // =========================================================
        //
        // "Restaurar original" NO significa simplemente seleccionar el
        // provider Original. Es una restauración de la política completa del
        // personaje:
        //
        // 1) elimina la copia Custom perteneciente al jugador;
        // 2) recupera el preset oficial USU del personaje (o el fallback
        //    genérico si no existe uno oficial);
        // 3) vuelve a AutomaticFallback;
        // 4) si sabemos que existe un comportamiento Original utilizable,
        //    Original queda activo y la misión USU permanece dormida;
        // 5) si no conocemos un sistema Original utilizable, USU vuelve a ser
        //    el fallback automático.
        //
        // El overload sin OriginalUnlockState utiliza la información que ya
        // existe en Survivors.json. La detección universal/dinámica definitiva
        // se implementará posteriormente en la Fase 6.
        // =========================================================

        public static MissionAssignmentResult RestoreCharacter(
            string bodyName,
            ManualLogSource logger = null
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


            return RestoreCharacter(
                normalizedBodyName,
                ResolveStoredOriginalUnlockState(
                    entry
                ),
                logger
            );
        }


        /// <summary>
        /// Overload preparado para la futura detección universal de providers.
        /// Permite indicar de forma explícita si el comportamiento original
        /// posee unlock propio o si está desbloqueado por defecto.
        /// </summary>
        public static MissionAssignmentResult RestoreCharacter(
            string bodyName,
            OriginalUnlockState originalState,
            ManualLogSource logger = null
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
                bool hadCustomMission =
                    HasCustomPlayerData(
                        originalChallenge
                    );


                SurvivorChallengeJson restoredChallenge =
                    CreateDefaultFallbackChallenge(
                        normalizedBodyName,
                        entry
                    );


                MissionConfiguration missionConfig =
                    EnsureMissionConfiguration(
                        restoredChallenge
                    );


                // Restaurar personaje es la ÚNICA operación de esta fase que
                // destruye la copia Custom del jugador.
                missionConfig.CustomMission =
                    null;

                missionConfig.Source =
                    "Preset";

                missionConfig.SelectionMode =
                    UnlockSelectionMode.AutomaticFallback;


                bool useOriginal =
                    originalState == OriginalUnlockState.HasUnlockSystem ||
                    originalState == OriginalUnlockState.UnlockedByDefault;


                missionConfig.Provider =
                    useOriginal
                        ? UnlockProviderKind.Original
                        : UnlockProviderKind.USU;


                entry.Challenge =
                    restoredChallenge;


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
                        "No se pudo guardar la restauración completa del personaje.",
                        normalizedBodyName
                    );
                }


                ApplyRuntimeChange(
                    normalizedBodyName,
                    logger,
                    "restaurar personaje"
                );


                string displayName =
                    GetDisplayName(
                        entry,
                        normalizedBodyName
                    );


                string restoredPresetId =
                    missionConfig.BasePresetId ?? "";


                string restoredPresetName =
                    GetPresetDisplayNameOrFallback(
                        restoredPresetId
                    );


                string targetDescription =
                    useOriginal
                        ? originalState == OriginalUnlockState.UnlockedByDefault
                            ? "Original (desbloqueado desde el inicio)"
                            : "Original"
                        : "USU (fallback automático)";


                logger?.LogInfo(
                    "[MISSION ASSIGNMENT] " +
                    $"Personaje restaurado | " +
                    $"Body: {normalizedBodyName} | " +
                    $"Provider: {missionConfig.Provider} | " +
                    $"SelectionMode: {missionConfig.SelectionMode} | " +
                    $"OriginalState: {originalState} | " +
                    $"CustomRemoved: {hadCustomMission}"
                );


                return MissionAssignmentResult.CreateSuccess(
                    normalizedBodyName,
                    restoredPresetId,
                    displayName,
                    restoredPresetName,
                    hadCustomMission
                        ? $"'{displayName}' fue restaurado a {targetDescription} y su copia Custom fue eliminada."
                        : $"'{displayName}' fue restaurado a {targetDescription}."
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
                    "[MISSION ASSIGNMENT] No se pudo restaurar el personaje completo."
                );

                logger?.LogError(
                    exception.Message
                );


                return MissionAssignmentResult.CreateFailure(
                    MissionAssignmentResultCodes.Unknown,
                    "Ocurrió un error al restaurar el personaje completo.",
                    normalizedBodyName
                );
            }
        }


        // =========================================================
        // HELPERS INTERNOS
        // =========================================================

        private static SurvivorChallengeJson EnsureChallenge(
            SurvivorJsonEntry entry
        )
        {
            if (entry.Challenge == null)
            {
                entry.Challenge =
                    new SurvivorChallengeJson();
            }


            return entry.Challenge;
        }


        /// <summary>
        /// Crea MissionConfig sólo cuando una entrada legacy todavía no la
        /// posee. Si ya existe una Mission v2 embebida, se conserva como una
        /// copia Custom dormida para no perderla al seleccionar Original.
        /// </summary>
        private static MissionConfiguration EnsureMissionConfiguration(
            SurvivorChallengeJson challenge
        )
        {
            if (challenge.MissionConfig != null)
            {
                return challenge.MissionConfig;
            }


            MissionDefinition legacyMission =
                CloneMissionOrNull(
                    challenge.Mission
                );


            challenge.MissionConfig =
                new MissionConfiguration
                {
                    Provider =
                        UnlockProviderKind.USU,

                    SelectionMode =
                        UnlockSelectionMode.AutomaticFallback,

                    Source =
                        legacyMission != null
                            ? "Custom"
                            : "Preset",

                    BasePresetId =
                        "",

                    CustomMission =
                        legacyMission
                };


            return challenge.MissionConfig;
        }


        /// <summary>
        /// Reconstruye la configuración base de USU para el personaje.
        /// Los nueve personajes con preset oficial recuperan SU preset oficial,
        /// no el último preset que el jugador hubiese seleccionado.
        ///
        /// Para survivors sin preset oficial todavía se mantiene el fallback
        /// genérico histórico de USU: KillEnemies 100.
        /// </summary>
        private static SurvivorChallengeJson CreateDefaultFallbackChallenge(
            string bodyName,
            SurvivorJsonEntry entry
        )
        {
            string source =
                entry?.Source ?? "";


            if (
                SurvivorChallengePresets.TryCreatePreset(
                    bodyName,
                    source,
                    out SurvivorChallengeJson preset
                ) &&
                preset != null
            )
            {
                SurvivorChallengeJson clonedPreset =
                    CloneChallengeOrNull(
                        preset
                    );


                MissionConfiguration presetConfig =
                    EnsureMissionConfiguration(
                        clonedPreset
                    );


                presetConfig.Provider =
                    UnlockProviderKind.USU;

                presetConfig.SelectionMode =
                    UnlockSelectionMode.AutomaticFallback;

                presetConfig.Source =
                    "Preset";

                presetConfig.CustomMission =
                    null;


                return clonedPreset;
            }


            return new SurvivorChallengeJson
            {
                Enabled = true,

                Name =
                    "Desafío de desbloqueo",

                Description =
                    "",

                LocalizationKey =
                    "",

                LunarCoinReward =
                    0,

                Type =
                    "KillEnemies",

                Parameters =
                    new JObject
                    {
                        ["amount"] = 100
                    },

                MissionConfig =
                    new MissionConfiguration
                    {
                        Provider =
                            UnlockProviderKind.USU,

                        SelectionMode =
                            UnlockSelectionMode.AutomaticFallback,

                        Source =
                            "Preset",

                        BasePresetId =
                            "",

                        CustomMission =
                            null
                    },

                Mission =
                    null
            };
        }


        /// <summary>
        /// La detección de sistemas originales será ampliada en Fase 6.
        /// En 5G.1D-D sólo consideramos confirmado un sistema original cuando
        /// Survivors.json conserva un nombre de unlock original real.
        /// "Ninguno" permanece como Unknown y por seguridad vuelve a USU como
        /// fallback automático.
        /// </summary>
        private static OriginalUnlockState ResolveStoredOriginalUnlockState(
            SurvivorJsonEntry entry
        )
        {
            if (entry == null)
            {
                return OriginalUnlockState.Unknown;
            }


            string originalUnlock =
                entry.OriginalUnlock?.Trim() ?? "";


            if (
                string.IsNullOrWhiteSpace(
                    originalUnlock
                ) ||
                IsNoOriginalUnlockValue(
                    originalUnlock
                )
            )
            {
                return OriginalUnlockState.Unknown;
            }


            return OriginalUnlockState.HasUnlockSystem;
        }


        private static bool IsNoOriginalUnlockValue(
            string value
        )
        {
            return
                string.Equals(
                    value,
                    "Ninguno",
                    StringComparison.OrdinalIgnoreCase
                ) ||
                string.Equals(
                    value,
                    "None",
                    StringComparison.OrdinalIgnoreCase
                ) ||
                string.Equals(
                    value,
                    "Null",
                    StringComparison.OrdinalIgnoreCase
                );
        }


        private static bool HasCustomPlayerData(
            SurvivorChallengeJson challenge
        )
        {
            if (challenge == null)
            {
                return false;
            }


            MissionConfiguration config =
                challenge.MissionConfig;


            if (config == null)
            {
                // Una Mission v2 sin MissionConfig pertenece a una entrada
                // legacy. La restauración también reemplaza ese contenido.
                return challenge.Mission != null;
            }


            return
                config.CustomMission != null ||
                config.EffectiveProvider == UnlockProviderKind.Custom ||
                string.Equals(
                    config.Source,
                    "Custom",
                    StringComparison.OrdinalIgnoreCase
                );
        }


        private static string GetPresetDisplayNameOrFallback(
            string presetId
        )
        {
            if (
                !string.IsNullOrWhiteSpace(
                    presetId
                ) &&
                MissionPresetLibraryService.TryGetAssignableMissionPreset(
                    presetId,
                    out MissionPreset preset
                ) &&
                preset != null &&
                !string.IsNullOrWhiteSpace(
                    preset.Name
                )
            )
            {
                return preset.Name;
            }


            return "Fallback USU";
        }


        private static bool IsValidMission(
            MissionDefinition mission
        )
        {
            return
                mission != null &&
                mission.Routes != null &&
                mission.Routes.Count > 0;
        }


        private static string GetDisplayName(
            SurvivorJsonEntry entry,
            string fallback
        )
        {
            if (
                entry != null &&
                !string.IsNullOrWhiteSpace(
                    entry.DisplayName
                )
            )
            {
                return entry.DisplayName;
            }


            return fallback ?? "";
        }


        private static MissionDefinition CloneMissionOrNull(
            MissionDefinition mission
        )
        {
            if (mission == null)
            {
                return null;
            }


            JToken token =
                JToken.FromObject(
                    mission
                );


            return token.ToObject<
                MissionDefinition
            >();
        }


        private static SurvivorChallengeJson CloneChallengeOrNull(
            SurvivorChallengeJson challenge
        )
        {
            if (challenge == null)
            {
                return null;
            }


            JToken token =
                JToken.FromObject(
                    challenge
                );


            return token.ToObject<
                SurvivorChallengeJson
            >();
        }


        private static void RestoreChallenge(
            SurvivorJsonEntry entry,
            SurvivorChallengeJson originalChallenge
        )
        {
            if (entry == null)
            {
                return;
            }


            entry.Challenge =
                CloneChallengeOrNull(
                    originalChallenge
                );
        }


        private static void ApplyRuntimeChange(
            string bodyName,
            ManualLogSource logger,
            string reason
        )
        {
            try
            {
                MissionRuntimeRefreshService.ApplyBodyConfiguration(
                    bodyName,
                    logger,
                    reason
                );
            }
            catch (Exception exception)
            {
                // La persistencia ya fue completada. Un fallo de refresh no
                // debe destruir la selección guardada; se aplicará al reiniciar.
                logger?.LogWarning(
                    $"[MISSION ASSIGNMENT] Configuración guardada para " +
                    $"{bodyName}, pero el refresh runtime falló: " +
                    exception.Message
                );
            }
        }


        private static bool PersistOrRollback(
            SurvivorJsonEntry entry,
            SurvivorChallengeJson originalChallenge,
            ManualLogSource logger
        )
        {
            if (
                SurvivorJsonManager
                    .TrySaveCurrentConfig(
                        logger
                    )
            )
            {
                return true;
            }


            RestoreChallenge(
                entry,
                originalChallenge
            );


            return false;
        }
    }
}
