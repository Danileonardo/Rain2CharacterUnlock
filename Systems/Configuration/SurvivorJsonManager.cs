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
    public static class SurvivorJsonManager
    {
        public static SurvivorJsonFile CurrentConfig
        {
            get;
            private set;
        } = new SurvivorJsonFile();


        private static string ConfigDirectory =>
            System.IO.Path.Combine(
                BepInEx.Paths.ConfigPath,
                "UniversalSurvivorUnlocks"
            );


        private static string JsonPath =>
            System.IO.Path.Combine(
                ConfigDirectory,
                "Survivors.json"
            );


        private static string BackupPath =>
            System.IO.Path.Combine(
                ConfigDirectory,
                "Survivors.backup.json"
            );


        // =========================================================
        // CARGA INICIAL
        // =========================================================

        public static void LoadForStartup(
            ManualLogSource logger
        )
        {
            Directory.CreateDirectory(
                ConfigDirectory
            );


            if (
                !TryLoadExisting(
                    logger,
                    out SurvivorJsonFile file
                )
            )
            {
                return;
            }


            CurrentConfig =
                file;


            // =====================================================
            // MODO DE AUTORÍA
            // =====================================================
            //
            // Durante el desarrollo:
            //
            // SurvivorChallengePresets.cs
            //          ↓
            //     fuente de verdad
            //          ↓
            //     Survivors.json
            //
            // Esto permite cambiar libremente
            // nombre, descripción, tipo y parámetros
            // de los presets originales.
            //
            // Cuando AuthoringMode sea false,
            // esta sincronización NO ocurrirá.
            // =====================================================

            if (
                SurvivorChallengePresets
                    .AuthoringMode
            )
            {
                bool changed =
                    ApplyAuthoringPresets(
                        CurrentConfig,
                        logger
                    );


                if (changed)
                {
                    Save(
                        CurrentConfig,
                        logger
                    );
                }
            }


            logger.LogInfo(
                $"Configuración inicial cargada. " +
                $"Disponibles guardados: " +
                $"{CurrentConfig.AvailableSurvivors.Count}"
            );
        }


        // =========================================================
        // SINCRONIZAR PRESETS DURANTE AUTORÍA
        // =========================================================

        private static bool ApplyAuthoringPresets(
            SurvivorJsonFile file,
            ManualLogSource logger
        )
        {
            if (
                file == null ||
                !SurvivorChallengePresets.AuthoringMode
            )
            {
                return false;
            }


            bool changed =
                false;


            changed |=
                ApplyAuthoringPresetsToEntries(
                    file.AvailableSurvivors,
                    logger
                );


            changed |=
                ApplyAuthoringPresetsToEntries(
                    file.UnavailableSurvivors,
                    logger
                );


            return changed;
        }


        // =========================================================
        // SINCRONIZAR UNA COLECCIÓN DE SURVIVORS
        // =========================================================

        private static bool ApplyAuthoringPresetsToEntries(
            Dictionary<
                string,
                SurvivorJsonEntry
            > entries,
            ManualLogSource logger
        )
        {
            if (entries == null)
            {
                return false;
            }


            bool changed =
                false;


            foreach (
                KeyValuePair<
                    string,
                    SurvivorJsonEntry
                > pair
                in entries
            )
            {
                SurvivorJsonEntry entry =
                    pair.Value;


                if (entry == null)
                {
                    continue;
                }


                string bodyName =
                    !string.IsNullOrWhiteSpace(
                        entry.BodyName
                    )
                        ? entry.BodyName
                        : pair.Key;


                string contentPackIdentifier =
                    entry.Source;


                // =================================================
                // ¿USU TIENE UN PRESET PARA ESTE SURVIVOR?
                // =================================================

                if (
                    !SurvivorChallengePresets
                        .TryCreatePreset(
                            bodyName,
                            contentPackIdentifier,
                            out SurvivorChallengeJson preset
                        )
                )
                {
                    continue;
                }


                // =================================================
                // YA ESTÁ ACTUALIZADO
                // =================================================

                if (
                    ChallengesAreEqual(
                        entry.Challenge,
                        preset
                    )
                )
                {
                    continue;
                }


                // =================================================
                // ACTUALIZAR DESDE EL PRESET DE AUTORÍA
                // =================================================

                entry.Challenge =
                    preset;


                changed =
                    true;


                logger?.LogInfo(
                    $"Preset de autoría actualizado | " +
                    $"Body: {bodyName} | " +
                    $"Pack: {contentPackIdentifier} | " +
                    $"Nombre: {preset.Name} | " +
                    $"Tipo: {preset.Type}"
                );
            }


            return changed;
        }


        // =========================================================
        // COMPARAR DOS CHALLENGES
        // =========================================================

        private static bool ChallengesAreEqual(
            SurvivorChallengeJson first,
            SurvivorChallengeJson second
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
                JToken firstToken =
                    JToken.FromObject(
                        first
                    );


                JToken secondToken =
                    JToken.FromObject(
                        second
                    );


                return JToken.DeepEquals(
                    firstToken,
                    secondToken
                );
            }
            catch
            {
                /*
                 * Si por alguna razón no se pueden
                 * comparar, preferimos asumir que
                 * son diferentes durante autoría.
                 */
                return false;
            }
        }


        // =========================================================
        // BUSCAR UN SURVIVOR EN TODO EL JSON
        // =========================================================

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


            bool hasPreset =
                SurvivorChallengePresets
                    .TryCreatePreset(
                        bodyName,
                        contentPackIdentifier,
                        out challenge
                    );


            // =====================================================
            // SI NO HAY PRESET, USAR CONFIGURACIÓN GENÉRICA
            // =====================================================

            if (!hasPreset)
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
                            contentPackIdentifier
                        )
                            ? contentPackIdentifier
                            : !string.IsNullOrWhiteSpace(
                                sourceAssembly
                            )
                                ? sourceAssembly
                                : "Modded",

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

            if (hasPreset)
            {
                logger.LogInfo(
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
                logger.LogInfo(
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

        public static void Sync(
            List<SurvivorInfo> detectedSurvivors,
            ManualLogSource logger
        )
        {
            Directory.CreateDirectory(
                ConfigDirectory
            );


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


            SurvivorJsonFile newFile =
                new SurvivorJsonFile();


            /*
             * Survivors MOD actualmente instalados.
             */
            HashSet<string> detectedModdedIds =
                new HashSet<string>();


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
            // CONSERVAR MODS DESINSTALADOS
            // =====================================================

            foreach (
                KeyValuePair<
                    string,
                    SurvivorJsonEntry
                > pair
                in savedRecords
            )
            {
                string id =
                    pair.Key;


                if (
                    detectedOfficialIds.Contains(
                        id
                    )
                )
                {
                    continue;
                }


                if (
                    detectedModdedIds.Contains(
                        id
                    )
                )
                {
                    continue;
                }


                SurvivorJsonEntry entry =
                    pair.Value;


                if (entry == null)
                {
                    continue;
                }


                entry.Available =
                    false;


                entry.Status =
                    "MissingContent";


                entry.Reason =
                    "Survivor mod no detectado. " +
                    "El mod puede estar desactivado o desinstalado.";


                if (entry.Challenge == null)
                {
                    entry.Challenge =
                        new SurvivorChallengeJson();
                }


                newFile
                    .UnavailableSurvivors[
                        id
                    ] =
                    entry;
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
                $"{newFile.UnavailableSurvivors.Count}"
            );
        }


        // =========================================================
        // CARGAR JSON EXISTENTE
        // =========================================================

        private static bool TryLoadExisting(
            ManualLogSource logger,
            out SurvivorJsonFile file
        )
        {
            file =
                new SurvivorJsonFile();


            if (!File.Exists(JsonPath))
            {
                return true;
            }


            try
            {
                string json =
                    File.ReadAllText(
                        JsonPath,
                        Encoding.UTF8
                    );


                SurvivorJsonFile loaded =
                    JsonConvert
                        .DeserializeObject<
                            SurvivorJsonFile
                        >(
                            json
                        );


                if (loaded != null)
                {
                    file =
                        loaded;
                }


                if (
                    file.AvailableSurvivors == null
                )
                {
                    file.AvailableSurvivors =
                        new Dictionary<
                            string,
                            SurvivorJsonEntry
                        >();
                }


                if (
                    file.UnavailableSurvivors == null
                )
                {
                    file.UnavailableSurvivors =
                        new Dictionary<
                            string,
                            SurvivorJsonEntry
                        >();
                }


                return true;
            }
            catch (
                Exception exception
            )
            {
                logger.LogError(
                    "Survivors.json contiene un error y no será sobrescrito."
                );


                logger.LogError(
                    exception.Message
                );


                return false;
            }
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
        }


        // =========================================================
        // GUARDAR
        // =========================================================

        private static void Save(
            SurvivorJsonFile file,
            ManualLogSource logger
        )
        {
            try
            {
                if (
                    File.Exists(
                        JsonPath
                    )
                )
                {
                    File.Copy(
                        JsonPath,
                        BackupPath,
                        true
                    );
                }


                string json =
                    JsonConvert.SerializeObject(
                        file,
                        Formatting.Indented
                    );


                File.WriteAllText(
                    JsonPath,
                    json,
                    new UTF8Encoding(
                        false
                    )
                );


                logger.LogInfo(
                    "Survivors.json actualizado correctamente."
                );


                logger.LogInfo(
                    $"Disponibles en JSON: " +
                    $"{file.AvailableSurvivors.Count}"
                );


                logger.LogInfo(
                    $"No disponibles en JSON: " +
                    $"{file.UnavailableSurvivors.Count}"
                );


                logger.LogInfo(
                    $"Ruta: {JsonPath}"
                );
            }
            catch (
                Exception exception
            )
            {
                logger.LogError(
                    "No se pudo guardar Survivors.json."
                );


                logger.LogError(
                    exception.Message
                );
            }
        }
    }
}