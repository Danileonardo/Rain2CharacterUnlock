using System;
using System.Collections.Generic;
using BepInEx.Logging;
using RoR2;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Aplica inmediatamente una selección persistida de provider/misión al
    /// runtime de Risk of Rain 2.
    ///
    /// MissionAssignmentService sigue siendo la fachada pública que decide y
    /// guarda. Esta clase sólo refleja esa decisión en SurvivorDef, snapshots,
    /// catálogo de objetivos, progreso temporal y selector.
    /// </summary>
    public static class MissionRuntimeRefreshService
    {
        private static ManualLogSource logger;
        private static bool initialized;

        private static readonly Dictionary<
            string,
            UnlockProviderKind
        > LastAppliedProviders =
            new Dictionary<
                string,
                UnlockProviderKind
            >(StringComparer.OrdinalIgnoreCase);


        public static void Initialize(
            ManualLogSource log
        )
        {
            if (initialized)
            {
                return;
            }


            initialized =
                true;

            logger =
                log;


            UsuLog.Verbose(
                logger,
                "MissionRuntimeRefreshService inicializado."
            );
        }


        /// <summary>
        /// Devuelve true cuando el cambio pudo aplicarse inmediatamente.
        /// Si false, la configuración ya puede estar guardada pero el cambio
        /// necesitará una futura sesión/reinicio para tomar efecto.
        /// </summary>
        public static bool ApplyBodyConfiguration(
            string bodyName,
            ManualLogSource log = null,
            string reason = ""
        )
        {
            ManualLogSource activeLogger =
                log ?? logger;


            string normalizedBodyName =
                bodyName?.Trim() ?? "";


            if (string.IsNullOrWhiteSpace(normalizedBodyName))
            {
                return false;
            }


            SurvivorJsonEntry entry =
                SurvivorJsonManager.GetEntryAnywhere(
                    normalizedBodyName
                );


            if (entry == null)
            {
                activeLogger?.LogWarning(
                    $"[MISSION RUNTIME] No existe configuración para " +
                    $"{normalizedBodyName}."
                );

                return false;
            }


            /*
             * En multiplayer el host es autoridad. Un cliente no puede
             * sustituir localmente el UnlockableDef durante una sesión de red
             * porque quedaría desincronizado del snapshot efectivo del host.
             */
            if (
                NetworkClient.active &&
                !NetworkServer.active
            )
            {
                activeLogger?.LogWarning(
                    $"[MISSION RUNTIME] Cambio guardado para " +
                    $"{normalizedBodyName}, pero el runtime del cliente " +
                    $"lo controla el host."
                );

                return false;
            }


            if (
                !TryResolveSurvivor(
                    normalizedBodyName,
                    out SurvivorDef survivorDef
                ) ||
                survivorDef == null
            )
            {
                activeLogger?.LogWarning(
                    $"[MISSION RUNTIME] SurvivorDef no encontrado para " +
                    $"{normalizedBodyName}. El cambio se aplicará en la " +
                    $"próxima carga."
                );

                return false;
            }


            UnlockProviderKind previousProvider =
                ResolvePreviousRuntimeProvider(
                    normalizedBodyName,
                    survivorDef,
                    entry
                );


            UnlockableDef currentUnlock =
                survivorDef.unlockableDef;


            if (
                currentUnlock == null ||
                !SurvivorUnlockManager.IsCustomUnlock(
                    currentUnlock
                )
            )
            {
                SurvivorUnlockManager.RememberOriginalUnlock(
                    survivorDef,
                    currentUnlock
                );
            }


            bool originalAvailable =
                SurvivorUnlockManager.HasOriginalUnlockAvailable(
                    survivorDef,
                    entry
                );


            bool useCustomUnlock =
                SurvivorUnlockManager.RequiresCustomUnlock(
                    entry,
                    originalAvailable
                );


            if (useCustomUnlock)
            {
                if (
                    !SurvivorUnlockManager.TryGetCustomUnlockable(
                        normalizedBodyName,
                        out UnlockableDef customUnlock
                    ) ||
                    customUnlock == null
                )
                {
                    activeLogger?.LogWarning(
                        $"[MISSION RUNTIME] No existe un UnlockableDef USU " +
                        $"preparado para {normalizedBodyName}. La selección " +
                        $"quedó guardada y se aplicará al reiniciar."
                    );

                    return false;
                }


                SurvivorUnlockManager.AssignEarlyCustomUnlock(
                    survivorDef,
                    customUnlock,
                    activeLogger
                );


                SurvivorUnlockManager.ApplyCustomAchievementIconForSurvivor(
                    survivorDef,
                    normalizedBodyName,
                    activeLogger
                );
            }
            else
            {
                SurvivorUnlockManager.RestoreOriginalUnlock(
                    survivorDef
                );
            }


            RefreshMissionState(
                normalizedBodyName,
                reason
            );


            SurvivorLockedPortraitManager.RefreshAll(
                normalizedBodyName
            );


            UnlockProviderKind currentProvider =
                SurvivorUnlockManager.IsCustomUnlock(
                    survivorDef.unlockableDef
                )
                    ? ResolveConfiguredProvider(entry)
                    : UnlockProviderKind.Original;


            LastAppliedProviders[
                normalizedBodyName
            ] = currentProvider;


            activeLogger?.LogInfo(
                $"[MISSION RUNTIME] Provider aplicado | " +
                $"Body: {normalizedBodyName} | " +
                $"{previousProvider} -> {currentProvider}" +
                FormatReason(reason)
            );


            return true;
        }


        private static UnlockProviderKind ResolvePreviousRuntimeProvider(
            string bodyName,
            SurvivorDef survivorDef,
            SurvivorJsonEntry entry
        )
        {
            if (
                !string.IsNullOrWhiteSpace(bodyName) &&
                LastAppliedProviders.TryGetValue(
                    bodyName,
                    out UnlockProviderKind remembered
                )
            )
            {
                return remembered;
            }


            if (
                survivorDef == null ||
                !SurvivorUnlockManager.IsCustomUnlock(
                    survivorDef.unlockableDef
                )
            )
            {
                return UnlockProviderKind.Original;
            }


            /*
             * Si esta es la primera transición de la sesión y el runtime ya
             * tenía un UnlockableDef USU, intentamos conservar la variante
             * configurada. Si la configuración acaba de ser cambiada a
             * Original todavía sabemos al menos que el runtime anterior era
             * controlado por USU, por lo que usamos USU como fallback de log.
             */
            UnlockProviderKind configured =
                ResolveConfiguredProvider(entry);


            if (configured != UnlockProviderKind.Original)
            {
                return configured;
            }


            return UnlockProviderKind.USU;
        }


        private static void RefreshMissionState(
            string bodyName,
            string reason
        )
        {
            if (!NetworkServer.active)
            {
                return;
            }


            if (Run.instance != null)
            {
                /*
                 * El MissionId actual es el BodyName. El progreso de la misión
                 * anterior debe desaparecer antes de reconstruir el catálogo.
                 */
                MissionProgressRegistry.ResetMission(
                    bodyName
                );


                GenericMissionDispatcher.ResetBody(
                    bodyName
                );


                BombingRunObjectiveHandler.ResetMission(
                    bodyName
                );

                CarryEquipmentObjectiveHandler.ResetMission(
                    bodyName
                );

                DefeatUmbraWavesObjectiveHandler.ResetMission(
                    bodyName
                );


                SessionMissionRegistry
                    .RefreshRunSnapshotAndBroadcast(
                        string.IsNullOrWhiteSpace(reason)
                            ? $"runtime {bodyName}"
                            : reason
                    );


                MissionRuntimeActivityPlan.Rebuild();
                MissionRuntimeCatalog.Rebuild();


                return;
            }


            SessionMissionRegistry
                .RefreshLobbySnapshotAndBroadcast(
                    string.IsNullOrWhiteSpace(reason)
                        ? $"runtime {bodyName}"
                        : reason
                );
        }


        private static bool TryResolveSurvivor(
            string bodyName,
            out SurvivorDef survivorDef
        )
        {
            if (
                ModdedSurvivorRegistry.TryGetSurvivorByBodyName(
                    bodyName,
                    out survivorDef
                ) &&
                survivorDef != null
            )
            {
                return true;
            }


            survivorDef =
                null;


            SurvivorDef[] catalog =
                SurvivorCatalog.survivorDefs;


            if (catalog == null)
            {
                return false;
            }


            foreach (
                SurvivorDef candidate
                in catalog
            )
            {
                if (
                    candidate == null ||
                    candidate.bodyPrefab == null
                )
                {
                    continue;
                }


                if (
                    string.Equals(
                        candidate.bodyPrefab.name,
                        bodyName,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    survivorDef =
                        candidate;

                    return true;
                }
            }


            return false;
        }


        private static UnlockProviderKind ResolveConfiguredProvider(
            SurvivorJsonEntry entry
        )
        {
            return
                entry?.Challenge?.MissionConfig != null
                    ? entry.Challenge.MissionConfig.EffectiveProvider
                    : UnlockProviderKind.USU;
        }


        private static string FormatReason(
            string reason
        )
        {
            return
                string.IsNullOrWhiteSpace(reason)
                    ? ""
                    : " | Motivo: " + reason.Trim();
        }
    }
}
