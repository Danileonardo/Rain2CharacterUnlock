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
            // SI NO HAY MISIÓN DORMIDA NI PRESET, USAR GENÉRICA
            // =====================================================

            if (
                !restoredDormantMission &&
                !hasPreset
            )
            {
                challenge =
                    new SurvivorChallengeJson
                    {
                        Enabled =
                            true,

                        Name =
                            "Desafío de desbloqueo",

                        Description =
                            "",

                        Type =
                            "KillEnemies",

                        Parameters =
                            new JObject
                            {
                                ["amount"] =
                                    100
                            }
                    };
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
                    $"Challenge: KillEnemies 100"
                );
            }


            return entry;
        }


        // =========================================================
        // SINCRONIZAR SURVIVORS DETECTADOS
        // =========================================================

    }
}
