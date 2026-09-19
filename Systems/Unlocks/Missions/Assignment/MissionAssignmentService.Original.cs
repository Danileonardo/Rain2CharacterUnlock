using System;
using System.Collections.Generic;

using BepInEx.Logging;
using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    public static partial class MissionAssignmentService
    {
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
                UnlockProviderPolicy.ResolveStoredOriginalState(
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


                missionConfig.Provider =
                    UnlockProviderPolicy.ResolveProviderAfterRestore(
                        originalState
                    );


                bool useOriginal =
                    missionConfig.Provider == UnlockProviderKind.Original;


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
    }
}
