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
        public static void Sync(
            List<SurvivorInfo> detectedSurvivors,
            ManualLogSource logger
        )
        {
            Directory.CreateDirectory(
                ConfigDirectory
            );


            if (
                SurvivorPersistenceCompatibility
                    .IsReadOnly
            )
            {
                logger?.LogWarning(
                    "[PERSISTENCE] Sync omitido: Survivors.json pertenece a un schema futuro y está protegido en modo sólo lectura."
                );

                return;
            }


            /*
             * NO volvemos a leer Survivors.json.
             *
             * CurrentConfig puede contener survivors
             * descubiertos durante GenerateContentPackAsync
             * antes de que hayan sido escritos al disco.
             */
            SurvivorJsonFile existingFile =
                CurrentConfig
                ?? new SurvivorJsonFile();


            if (
                existingFile
                    .AvailableSurvivors == null
            )
            {
                existingFile
                    .AvailableSurvivors =
                    new Dictionary<
                        string,
                        SurvivorJsonEntry
                    >();
            }


            if (
                existingFile
                    .UnavailableSurvivors == null
            )
            {
                existingFile
                    .UnavailableSurvivors =
                    new Dictionary<
                        string,
                        SurvivorJsonEntry
                    >();
            }


            /*
             * Recuperamos las configuraciones anteriores.
             *
             * Esto nos permite conservar una misión
             * si un survivor modded desaparece y
             * vuelve después.
             */
            Dictionary<
                string,
                SurvivorJsonEntry
            > savedRecords =
                CombineExistingRecords(
                    existingFile
                );


            Dictionary<
                string,
                SurvivorMissionRecord
            > savedMissionRecords =
                BuildMissionRecordIndex(
                    existingFile
                );


            SurvivorJsonFile newFile =
                new SurvivorJsonFile();


            SurvivorPersistenceCompatibility
                .CopyFileMetadata(
                    existingFile,
                    newFile
                );


            /*
             * Survivors MOD actualmente instalados.
             */
            HashSet<string> detectedModdedIds =
                new HashSet<string>();


            HashSet<string> detectedStableIds =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase
                );


            /*
             * Survivors oficiales detectados.
             *
             * Sirve para eliminar del JSON
             * cualquier registro antiguo de
             * Vanilla o DLC.
             */
            HashSet<string> detectedOfficialIds =
                new HashSet<string>();


            // =====================================================
            // DETECTAR Y CLASIFICAR
            // =====================================================

            foreach (
                SurvivorInfo survivor
                in detectedSurvivors
            )
            {
                if (survivor == null)
                {
                    continue;
                }


                string id =
                    survivor.BodyName;


                if (
                    string.IsNullOrWhiteSpace(
                        id
                    ) ||
                    id == "Sin Body" ||
                    id == "UnknownBody"
                )
                {
                    continue;
                }


                // =================================================
                // CONTENIDO OFICIAL
                // =================================================

                if (!survivor.IsModded)
                {
                    detectedOfficialIds.Add(
                        id
                    );

                    continue;
                }


                // =================================================
                // CONTENIDO MODDED
                // =================================================

                detectedModdedIds.Add(
                    id
                );


                /*
                 * Survivors ocultos o no seleccionables
                 * tampoco se configuran.
                 */
                if (
                    survivor.Status ==
                        SurvivorStatus.Hidden ||
                    survivor.Status ==
                        SurvivorStatus.NotSelectable
                )
                {
                    continue;
                }


                SurvivorJsonEntry entry;


                /*
                 * Si ya existía una configuración,
                 * la recuperamos.
                 */
                if (
                    !savedRecords.TryGetValue(
                        id,
                        out entry
                    )
                )
                {
                    entry =
                        new SurvivorJsonEntry();
                }


                // =================================================
                // ACTUALIZAR METADATOS DEL MOD
                // =================================================

                entry.DisplayName =
                    survivor.DisplayName;


                entry.InternalName =
                    survivor.InternalName;


                entry.BodyName =
                    survivor.BodyName;


                if (
                    !string.IsNullOrWhiteSpace(
                        survivor.ContentPackIdentifier
                    )
                )
                {
                    entry.Source =
                        survivor.ContentPackIdentifier;
                }
                else if (
                    !string.IsNullOrWhiteSpace(
                        survivor.SourceAssembly
                    )
                )
                {
                    entry.Source =
                        survivor.SourceAssembly;
                }
                else
                {
                    entry.Source =
                        "Modded";
                }


                entry.OriginalUnlock =
                    survivor.UnlockableName;


                string stableId =
                    SurvivorIdentity.Build(
                        entry.Source,
                        entry.BodyName
                    );

                if (
                    string.IsNullOrWhiteSpace(
                        entry.SurvivorId
                    ) ||
                    (
                        SurvivorIdentity.IsBodyFallback(
                            entry.SurvivorId
                        ) &&
                        !SurvivorIdentity.IsBodyFallback(
                            stableId
                        )
                    )
                )
                {
                    entry.SurvivorId =
                        stableId;
                }


                if (!string.IsNullOrWhiteSpace(stableId))
                {
                    detectedStableIds.Add(stableId);
                }


                SurvivorMissionRecord restoredMission =
                    TakeMissionRecord(
                        savedMissionRecords,
                        stableId,
                        id
                    );


                restoredMission?.ApplyMissionTo(
                    entry
                );


                if (entry.Challenge == null)
                {
                    entry.Challenge =
                        new SurvivorChallengeJson();
                }


                // =================================================
                // SURVIVOR MOD DISPONIBLE
                // =================================================

                if (
                    survivor.Status ==
                    SurvivorStatus.Available
                )
                {
                    entry.Available =
                        true;

                    entry.Status =
                        "Available";

                    entry.Reason =
                        "";


                    newFile
                        .AvailableSurvivors[
                            id
                        ] =
                        entry;


                    continue;
                }


                // =================================================
                // SURVIVOR MOD NO DISPONIBLE
                // =================================================

                entry.Available =
                    false;


                entry.Status =
                    survivor.Status.ToString();


                if (
                    survivor.Status ==
                    SurvivorStatus.DlcNotOwned
                )
                {
                    entry.Reason =
                        "Requires DLC: " +
                        survivor.ExpansionName;
                }
                else
                {
                    entry.Reason =
                        "Survivor actualmente no disponible.";
                }


                newFile
                    .UnavailableSurvivors[
                        id
                    ] =
                    entry;
            }


            // =====================================================
            // PREPARAR OFFICIAL AUNQUE EL MOD NO ESTÉ INSTALADO
            // =====================================================
            //
            // Una misión Official conocida por USU puede quedar preparada como
            // PendingContent sin inventar ningún dato runtime del survivor. Si ya
            // existía una Custom/Generated/configuración del jugador, se conserva
            // el registro previo y jamás se reemplaza silenciosamente.
            // =====================================================

            List<OfficialMissionPresetDescriptor> officialDescriptors =
                OfficialMissionPresetRegistry.GetAll();


            for (int i = 0; i < officialDescriptors.Count; i++)
            {
                OfficialMissionPresetDescriptor descriptor =
                    officialDescriptors[i];

                if (descriptor == null)
                {
                    continue;
                }

                string officialStableId =
                    SurvivorIdentity.Build(
                        descriptor.ContentPackIdentifier,
                        descriptor.BodyName
                    );

                if (
                    string.IsNullOrWhiteSpace(officialStableId) ||
                    detectedStableIds.Contains(officialStableId)
                )
                {
                    continue;
                }

                if (savedMissionRecords.ContainsKey(officialStableId))
                {
                    continue;
                }

                if (
                    !SurvivorChallengePresets.TryCreatePreset(
                        descriptor.BodyName,
                        descriptor.ContentPackIdentifier,
                        out SurvivorChallengeJson officialChallenge
                    ) ||
                    officialChallenge == null
                )
                {
                    continue;
                }

                savedMissionRecords[officialStableId] =
                    new SurvivorMissionRecord
                    {
                        SurvivorId = officialStableId,
                        BodyName = descriptor.BodyName,
                        Source = descriptor.ContentPackIdentifier,
                        State = SurvivorMissionRecord.PendingContentState,
                        Challenge = officialChallenge
                    };
            }


            // =====================================================
            // CONTENIDO AUSENTE -> REGISTRO MISSION-ONLY
            // =====================================================

            foreach (
                KeyValuePair<
                    string,
                    SurvivorMissionRecord
                > pair
                in savedMissionRecords
            )
            {
                SurvivorMissionRecord record =
                    pair.Value;

                if (record == null)
                {
                    continue;
                }

                if (
                    detectedOfficialIds.Contains(record.BodyName) ||
                    detectedModdedIds.Contains(record.BodyName)
                )
                {
                    continue;
                }

                NormalizeMissionRecord(record);

                if (
                    string.IsNullOrWhiteSpace(record.SurvivorId)
                )
                {
                    continue;
                }

                newFile.DormantMissions[record.SurvivorId] =
                    record;
            }


            // =====================================================
            // MODO DE AUTORÍA
            // =====================================================
            //
            // Aquí hacemos una segunda sincronización.
            //
            // Esto es útil porque durante Sync()
            // acabamos de actualizar Source usando el
            // ContentPackIdentifier real del survivor.
            //
            // De esta manera, aunque un JSON antiguo
            // tuviera un Source diferente, el preset
            // puede ser reconocido correctamente.
            // =====================================================

            if (
                SurvivorChallengePresets
                    .AuthoringMode
            )
            {
                ApplyAuthoringPresets(
                    newFile,
                    logger
                );
            }


            // =====================================================
            // TERMINAR
            // =====================================================

            SortEntries(
                newFile
            );


            CurrentConfig =
                newFile;


            Save(
                newFile,
                logger
            );


            logger.LogInfo(
                $"JSON modded sincronizado | " +
                $"Disponibles: " +
                $"{newFile.AvailableSurvivors.Count} | " +
                $"No disponibles: " +
                $"{newFile.UnavailableSurvivors.Count} | " +
                $"PendingContent: " +
                $"{CountDormantMissionRecords(newFile)}"
            );


            UsuLog.Verbose(
                logger,
                $"[SURVIVOR IDENTITY] Persistencia reconciliada | " +
                $"Con ID estable: " +
                $"{CountStableIdentities(newFile)}"
            );
        }


        // =========================================================
        // CARGAR JSON EXISTENTE
        // =========================================================



        private static int CountStableIdentities(
            SurvivorJsonFile file
        )
        {
            if (file == null)
            {
                return 0;
            }

            int count = 0;

            if (file.AvailableSurvivors != null)
            {
                foreach (SurvivorJsonEntry entry in file.AvailableSurvivors.Values)
                {
                    if (
                        entry != null &&
                        !string.IsNullOrWhiteSpace(
                            entry.SurvivorId
                        )
                    )
                    {
                        count++;
                    }
                }
            }

            if (file.UnavailableSurvivors != null)
            {
                foreach (SurvivorJsonEntry entry in file.UnavailableSurvivors.Values)
                {
                    if (
                        entry != null &&
                        !string.IsNullOrWhiteSpace(
                            entry.SurvivorId
                        )
                    )
                    {
                        count++;
                    }
                }
            }

            if (file.DormantMissions != null)
            {
                foreach (SurvivorMissionRecord record in file.DormantMissions.Values)
                {
                    if (
                        record != null &&
                        !string.IsNullOrWhiteSpace(
                            record.SurvivorId
                        )
                    )
                    {
                        count++;
                    }
                }
            }

            return count;
        }


        // =========================================================
        // COMBINAR REGISTROS EXISTENTES
        // =========================================================

        private static Dictionary<
            string,
            SurvivorJsonEntry
        > CombineExistingRecords(
            SurvivorJsonFile file
        )
        {
            Dictionary<
                string,
                SurvivorJsonEntry
            > records =
                new Dictionary<
                    string,
                    SurvivorJsonEntry
                >();


            foreach (
                KeyValuePair<
                    string,
                    SurvivorJsonEntry
                > pair
                in file.AvailableSurvivors
            )
            {
                records[
                    pair.Key
                ] =
                    pair.Value;
            }


            foreach (
                KeyValuePair<
                    string,
                    SurvivorJsonEntry
                > pair
                in file.UnavailableSurvivors
            )
            {
                records[
                    pair.Key
                ] =
                    pair.Value;
            }


            return records;
        }


        // =========================================================
        // ORDENAR
        // =========================================================

        private static void SortEntries(
            SurvivorJsonFile file
        )
        {
            file.AvailableSurvivors =
                file.AvailableSurvivors
                    .OrderBy(
                        pair =>
                            pair.Value.DisplayName
                    )
                    .ToDictionary(
                        pair =>
                            pair.Key,
                        pair =>
                            pair.Value
                    );


            file.UnavailableSurvivors =
                file.UnavailableSurvivors
                    .OrderBy(
                        pair =>
                            pair.Value.DisplayName
                    )
                    .ToDictionary(
                        pair =>
                            pair.Key,
                        pair =>
                            pair.Value
                    );


            file.DormantMissions =
                (file.DormantMissions
                    ?? new Dictionary<string, SurvivorMissionRecord>())
                    .OrderBy(
                        pair =>
                            pair.Value?.BodyName
                            ?? pair.Key
                    )
                    .ToDictionary(
                        pair =>
                            pair.Key,
                        pair =>
                            pair.Value,
                        StringComparer.OrdinalIgnoreCase
                    );
        }
    }
}
