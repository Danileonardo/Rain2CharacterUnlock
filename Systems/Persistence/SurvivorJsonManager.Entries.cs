using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    public static partial class SurvivorJsonManager
    {
        public static SurvivorJsonEntry GetEntryAnywhere(
            string bodyName
        )
        {
            if (
                string.IsNullOrWhiteSpace(
                    bodyName
                ) ||
                CurrentConfig == null
            )
            {
                return null;
            }


            if (
                CurrentConfig
                    .AvailableSurvivors == null
            )
            {
                CurrentConfig
                    .AvailableSurvivors =
                    new Dictionary<
                        string,
                        SurvivorJsonEntry
                    >();
            }


            if (
                CurrentConfig
                    .UnavailableSurvivors == null
            )
            {
                CurrentConfig
                    .UnavailableSurvivors =
                    new Dictionary<
                        string,
                        SurvivorJsonEntry
                    >();
            }


            if (
                CurrentConfig
                    .AvailableSurvivors
                    .TryGetValue(
                        bodyName,
                        out SurvivorJsonEntry availableEntry
                    )
            )
            {
                return availableEntry;
            }


            if (
                CurrentConfig
                    .UnavailableSurvivors
                    .TryGetValue(
                        bodyName,
                        out SurvivorJsonEntry unavailableEntry
                    )
            )
            {
                return unavailableEntry;
            }


            return null;
        }


        public static SurvivorJsonEntry FindEntryBySurvivorId(
            string survivorId
        )
        {
            if (
                string.IsNullOrWhiteSpace(
                    survivorId
                ) ||
                CurrentConfig == null
            )
            {
                return null;
            }

            SurvivorJsonEntry found =
                FindEntryBySurvivorId(
                    CurrentConfig.AvailableSurvivors,
                    survivorId
                );

            if (found != null)
            {
                return found;
            }

            return
                FindEntryBySurvivorId(
                    CurrentConfig.UnavailableSurvivors,
                    survivorId
                );
        }


        public static SurvivorMissionRecord GetMissionRecord(
            string bodyName
        )
        {
            SurvivorJsonEntry entry =
                GetEntryAnywhere(
                    bodyName
                );

            if (entry != null)
            {
                return
                    SurvivorMissionRecord.FromEntry(
                        bodyName,
                        entry
                    );
            }

            if (CurrentConfig?.DormantMissions != null)
            {
                foreach (
                    SurvivorMissionRecord record
                    in CurrentConfig.DormantMissions.Values
                )
                {
                    if (
                        record != null &&
                        string.Equals(
                            record.BodyName,
                            bodyName,
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    {
                        return record;
                    }
                }
            }

            return null;
        }


        private static SurvivorJsonEntry FindEntryBySurvivorId(
            Dictionary<string, SurvivorJsonEntry> entries,
            string survivorId
        )
        {
            if (entries == null)
            {
                return null;
            }

            foreach (SurvivorJsonEntry entry in entries.Values)
            {
                if (
                    entry != null &&
                    string.Equals(
                        entry.SurvivorId,
                        survivorId,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    return entry;
                }
            }

            return null;
        }


        // =========================================================
        // CREAR CONFIGURACIÓN AUTOMÁTICA
        // PARA UN SURVIVOR MODDED NUEVO
        // =========================================================

        public static SurvivorJsonEntry CreateAutomaticEntry(
            SurvivorDef survivor,
            string contentPackIdentifier,
            string sourceAssembly,
            ManualLogSource logger
        )
        {
            if (
                survivor == null ||
                survivor.bodyPrefab == null
            )
            {
                return null;
            }


            string bodyName =
                survivor.bodyPrefab.name;


            /*
             * Si ya existe, respetamos la entrada
             * almacenada.
             *
             * Durante AuthoringMode, los presets
             * conocidos son sincronizados aparte.
             */
            SurvivorJsonEntry existing =
                GetEntryAnywhere(
                    bodyName
                );


            if (existing != null)
            {
                return existing;
            }


            string resolvedSource =
                !string.IsNullOrWhiteSpace(
                    contentPackIdentifier
                )
                    ? contentPackIdentifier
                    : sourceAssembly;


            string stableId =
                SurvivorIdentity.Build(
                    resolvedSource,
                    bodyName
                );


            SurvivorMissionRecord dormantRecord =
                GetMissionRecordBySurvivorId(
                    stableId
                );


            bool restoredDormantMission =
                dormantRecord?.Challenge != null;


            // =====================================================
            // OBTENER NOMBRE
            // =====================================================

            string displayName =
                bodyName;


            try
            {
                if (
                    !string.IsNullOrWhiteSpace(
                        survivor.displayNameToken
                    )
                )
                {
                    string translatedName =
                        Language.GetString(
                            survivor.displayNameToken
                        );


                    if (
                        !string.IsNullOrWhiteSpace(
                            translatedName
                        ) &&
                        !string.Equals(
                            translatedName,
                            survivor.displayNameToken,
                            StringComparison.Ordinal
                        )
                    )
                    {
                        displayName =
                            translatedName;
                    }
                }
            }
            catch
            {
                /*
                 * Durante esta fase temprana
                 * el sistema de idioma puede
                 * todavía no estar listo.
                 */
            }


            /*
             * Fallback:
             *
             * SoraBody -> Sora
             * RalseiBody -> Ralsei
             */
            if (
                displayName == bodyName &&
                bodyName.EndsWith(
                    "Body",
                    StringComparison.Ordinal
                )
            )
            {
                displayName =
                    bodyName.Substring(
                        0,
                        bodyName.Length - 4
                    );
            }


            // =====================================================
            // BUSCAR PRESET ESPECÍFICO
            // =====================================================

            SurvivorChallengeJson challenge;


            bool hasPreset = false;


            if (restoredDormantMission)
            {
                challenge =
                    dormantRecord.Challenge;
            }
            else
            {
                hasPreset =
                    SurvivorChallengePresets
                        .TryCreatePreset(
                            bodyName,
                            contentPackIdentifier,
                            out challenge
                        );
            }


            // =====================================================
            // SI NO HAY MISIÓN DORMIDA NI PRESET, DEJAR PENDIENTE
            // PARA GENERATED
            // =====================================================
            //
            // Desde G8 Phase 1.5 ya no se crea el fallback histórico
            // KillEnemies 100. G7/G8 lo reemplazará por una Generated
            // Mission V2 una vez confirmado que tampoco existe Original.
            // =====================================================

            if (
                !restoredDormantMission &&
                !hasPreset
            )
            {
                challenge =
                    CreateGeneratedPendingChallenge();
            }


            // =====================================================
            // CREAR ENTRY AUTOMÁTICA
            // =====================================================

            SurvivorJsonEntry entry =
                new SurvivorJsonEntry
                {
                    DisplayName =
                        displayName,

                    InternalName =
                        !string.IsNullOrWhiteSpace(
                            survivor.cachedName
                        )
                            ? survivor.cachedName
                            : bodyName,

                    BodyName =
                        bodyName,

                    Source =
                        !string.IsNullOrWhiteSpace(
                            resolvedSource
                        )
                            ? resolvedSource
                            : "Modded",

                    SurvivorId =
                        stableId,

                    OriginalUnlock =
                        "Ninguno",

                    Available =
                        true,

                    Status =
                        "Available",

                    Reason =
                        "",

                    Challenge =
                        challenge
                };


            // =====================================================
            // ASEGURAR DICCIONARIOS
            // =====================================================

            if (CurrentConfig == null)
            {
                CurrentConfig =
                    new SurvivorJsonFile();
            }


            if (
                CurrentConfig
                    .AvailableSurvivors == null
            )
            {
                CurrentConfig
                    .AvailableSurvivors =
                    new Dictionary<
                        string,
                        SurvivorJsonEntry
                    >();
            }


            if (
                CurrentConfig
                    .UnavailableSurvivors == null
            )
            {
                CurrentConfig
                    .UnavailableSurvivors =
                    new Dictionary<
                        string,
                        SurvivorJsonEntry
                    >();
            }


            /*
             * Si estaba marcado como no disponible,
             * lo retiramos de esa sección.
             */
            CurrentConfig
                .UnavailableSurvivors
                .Remove(
                    bodyName
                );


            /*
             * Se agrega solamente en memoria.
             *
             * Sync() escribirá posteriormente
             * Survivors.json.
             */
            CurrentConfig
                .AvailableSurvivors[
                    bodyName
                ] =
                entry;


            // =====================================================
            // LOG
            // =====================================================

            if (restoredDormantMission)
            {
                UsuLog.Verbose(
                    logger,
                    $"Misión dormida rehidratada en memoria | " +
                    $"Survivor: {displayName} | " +
                    $"Body: {bodyName} | " +
                    $"SurvivorId: {stableId}"
                );
            }
            else if (hasPreset)
            {
                UsuLog.Verbose(
                    logger,
                    $"Preset de desbloqueo aplicado | " +
                    $"Survivor: {displayName} | " +
                    $"Body: {bodyName} | " +
                    $"Pack: {entry.Source} | " +
                    $"Challenge: {challenge.Type} | " +
                    $"Nombre: {challenge.Name}"
                );
            }
            else
            {
                UsuLog.Verbose(
                    logger,
                    $"Configuración automática creada en memoria | " +
                    $"Survivor: {displayName} | " +
                    $"Body: {bodyName} | " +
                    $"Pack: {entry.Source} | " +
                    $"Challenge: Generated pending"
                );
            }


            return entry;
        }


        /// <summary>
        /// Estado neutro previo a la generación. No representa una misión
        /// jugable y no es Custom ni Preset. G7/G8 lo reemplaza por una
        /// Generated Mission V2 dentro del mismo Survivors.json.
        /// </summary>
        internal static SurvivorChallengeJson CreateGeneratedPendingChallenge()
        {
            return new SurvivorChallengeJson
            {
                Enabled = true,
                Name = "",
                Description = "",
                LocalizationKey = "",
                LunarCoinReward = 0,
                Type = "GeneratedPending",
                Parameters = new JObject(),
                MissionConfig = null,
                Mission = null
            };
        }


        /// <summary>
        /// Contenedor neutro para survivors cuyo contenido Original es la
        /// autoridad. Se conserva para permitir futuras alternativas USU/Custom
        /// sin inventar detrás una misión automática de 100 bajas.
        /// </summary>
        internal static SurvivorChallengeJson CreateOriginalPlaceholderChallenge()
        {
            return new SurvivorChallengeJson
            {
                Enabled = false,
                Name = "",
                Description = "",
                LocalizationKey = "",
                LunarCoinReward = 0,
                Type = "Original",
                Parameters = new JObject(),
                MissionConfig = null,
                Mission = null
            };
        }


        /// <summary>
        /// Reconoce exclusivamente el fallback automático histórico exacto.
        /// Una Custom real de 100 bajas posee MissionConfig/Source y no entra.
        /// </summary>
        internal static bool IsHistoricalKillEnemies100Fallback(
            SurvivorChallengeJson challenge
        )
        {
            if (
                challenge == null ||
                challenge.Mission != null ||
                challenge.MissionConfig != null ||
                !string.Equals(
                    challenge.Type,
                    "KillEnemies",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return false;
            }

            int amount =
                challenge.Parameters?.Value<int?>("amount")
                ?? 0;

            if (amount != 100)
            {
                return false;
            }

            string name =
                challenge.Name?.Trim() ?? "";

            string description =
                challenge.Description?.Trim() ?? "";

            string localizationKey =
                challenge.LocalizationKey?.Trim() ?? "";

            bool defaultName =
                string.IsNullOrWhiteSpace(name) ||
                string.Equals(
                    name,
                    "Desafío de desbloqueo",
                    StringComparison.OrdinalIgnoreCase
                );

            return
                challenge.Enabled &&
                defaultName &&
                string.IsNullOrWhiteSpace(description) &&
                string.IsNullOrWhiteSpace(localizationKey) &&
                challenge.LunarCoinReward == 0;
        }


        // =========================================================
        // SINCRONIZAR SURVIVORS DETECTADOS
        // =========================================================

    }
}
