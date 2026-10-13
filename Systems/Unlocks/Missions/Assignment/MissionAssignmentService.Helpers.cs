using System;
using System.Collections.Generic;

using BepInEx.Logging;
using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    public static partial class MissionAssignmentService
    {
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
        /// Para survivors sin preset oficial ya no se inventa KillEnemies 100.
        /// Se vuelve a GeneratedPending para que G7/G8 regenere Mission V2.
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

                presetConfig.UsuPresetId =
                    presetConfig.BasePresetId ?? "";

                presetConfig.CustomMission =
                    null;


                return clonedPreset;
            }


            return
                SurvivorJsonManager
                    .CreateGeneratedPendingChallenge();
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
                config.CustomMissionSnapshot != null ||
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
