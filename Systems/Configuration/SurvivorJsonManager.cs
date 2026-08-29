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
            // MIGRACIONES DE PRESETS CONOCIDOS
            // =====================================================

            ApplyKnownPresetMigrations(
                CurrentConfig,
                logger
            );


            logger.LogInfo(
                $"Configuración inicial cargada. " +
                $"Disponibles guardados: " +
                $"{CurrentConfig.AvailableSurvivors.Count}"
            );
        }

        // =========================================================
        // MIGRAR PRESETS ANTIGUOS
        // =========================================================

        private static void ApplyKnownPresetMigrations(
            SurvivorJsonFile file,
            ManualLogSource logger
        )
        {
            if (file == null)
            {
                return;
            }


            int migrated =
                0;


            migrated +=
                ApplyKnownPresetMigrationsToEntries(
                    file.AvailableSurvivors,
                    logger
                );


            migrated +=
                ApplyKnownPresetMigrationsToEntries(
                    file.UnavailableSurvivors,
                    logger
                );


            if (migrated <= 0)
            {
                return;
            }


            /*
             * Guardamos inmediatamente.
             *
             * Esto es importante porque después de
             * LoadForStartup() se registran los
             * UnlockableDef / AchievementDef.
             *
             * Necesitamos que ya utilicen la nueva
             * configuración durante este mismo arranque.
             */
            Save(
                file,
                logger
            );


            logger.LogInfo(
                $"Migraciones de presets aplicadas: {migrated}"
            );
        }


        // =========================================================
        // MIGRAR UNA COLECCIÓN DE SURVIVORS
        // =========================================================

        private static int ApplyKnownPresetMigrationsToEntries(
            Dictionary<
                string,
                SurvivorJsonEntry
            > entries,
            ManualLogSource logger
        )
        {
            if (entries == null)
            {
                return 0;
            }


            int migrated =
                0;


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


                if (
                    !SurvivorChallengePresets
                        .TryMigrateLegacyPreset(
                            bodyName,
                            contentPackIdentifier,
                            entry.Challenge,
                            out SurvivorChallengeJson migratedChallenge
                        )
                )
                {
                    continue;
                }


                entry.Challenge =
                    migratedChallenge;


                migrated++;


                logger.LogInfo(
                    $"Preset legado migrado | " +
                    $"Body: {bodyName} | " +
                    $"Pack: {contentPackIdentifier} | " +
                    $"Nuevo challenge: " +
                    $"{migratedChallenge.Name} / " +
                    $"{migratedChallenge.Type}"
                );
            }


            return migrated;
        }

        // =========================================================
        // BUSCAR UN SURVIVOR EN TODO EL JSON
        // =========================================================

        public static SurvivorJsonEntry GetEntryAnywhere(
            string bodyName
        )
        {
            if (
                string.IsNullOrWhiteSpace(bodyName) ||
                CurrentConfig == null
            )
            {
                return null;
            }


            if (CurrentConfig.AvailableSurvivors == null)
            {
                CurrentConfig.AvailableSurvivors =
                    new Dictionary<string, SurvivorJsonEntry>();
            }


            if (CurrentConfig.UnavailableSurvivors == null)
            {
                CurrentConfig.UnavailableSurvivors =
                    new Dictionary<string, SurvivorJsonEntry>();
            }


            if (
                CurrentConfig.AvailableSurvivors.TryGetValue(
                    bodyName,
                    out SurvivorJsonEntry availableEntry
                )
            )
            {
                return availableEntry;
            }


            if (
                CurrentConfig.UnavailableSurvivors.TryGetValue(
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
             * Si el survivor ya existe en el JSON,
             * respetamos completamente su configuración.
             *
             * Esto significa que un preset NUNCA
             * sobrescribirá una configuración que
             * el usuario ya tenga.
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
             * SoraBody -> Sora
             * PaladinBody -> Paladin
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


            if (CurrentConfig.AvailableSurvivors == null)
            {
                CurrentConfig.AvailableSurvivors =
                    new Dictionary<
                        string,
                        SurvivorJsonEntry
                    >();
            }


            if (CurrentConfig.UnavailableSurvivors == null)
            {
                CurrentConfig.UnavailableSurvivors =
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


            if (existingFile.AvailableSurvivors == null)
            {
                existingFile.AvailableSurvivors =
                    new Dictionary<
                        string,
                        SurvivorJsonEntry
                    >();
            }


            if (existingFile.UnavailableSurvivors == null)
            {
                existingFile.UnavailableSurvivors =
                    new Dictionary<
                        string,
                        SurvivorJsonEntry
                    >();
            }

            /*
             * Recuperamos las configuraciones anteriores.
             *
             * Esto nos permite conservar una misión si un
             * survivor modded desaparece y vuelve después.
             */
            Dictionary<string, SurvivorJsonEntry> savedRecords =
                CombineExistingRecords(
                    existingFile
                );

            SurvivorJsonFile newFile =
                new SurvivorJsonFile();

            /*
             * Survivors MOD que están actualmente instalados.
             */
            HashSet<string> detectedModdedIds =
                new HashSet<string>();

            /*
             * Survivors oficiales detectados.
             *
             * Sirve para eliminar del JSON cualquier registro
             * antiguo de Vanilla o DLC.
             */
            HashSet<string> detectedOfficialIds =
                new HashSet<string>();


            // =========================================================
            // DETECTAR Y CLASIFICAR
            // =========================================================

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
                    string.IsNullOrWhiteSpace(id) ||
                    id == "Sin Body" ||
                    id == "UnknownBody"
                )
                {
                    continue;
                }


                // =====================================================
                // CONTENIDO OFICIAL
                // =====================================================

                if (!survivor.IsModded)
                {
                    /*
                     * Vanilla y DLC se registran solamente
                     * para saber que deben eliminarse de
                     * cualquier JSON antiguo.
                     *
                     * NO los agregamos al nuevo JSON.
                     */
                    detectedOfficialIds.Add(
                        id
                    );

                    continue;
                }


                // =====================================================
                // CONTENIDO MODDED
                // =====================================================

                detectedModdedIds.Add(
                    id
                );


                /*
                 * Si un mod crea un SurvivorDef oculto o
                 * no seleccionable tampoco queremos
                 * configurarlo.
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
                 *
                 * Esto conserva la misión editada
                 * por el usuario.
                 */
                if (!savedRecords.TryGetValue(
                    id,
                    out entry
                ))
                {
                    entry =
                        new SurvivorJsonEntry();
                }


                // =====================================================
                // ACTUALIZAR METADATOS DEL MOD
                // =====================================================

                entry.DisplayName =
                    survivor.DisplayName;

                entry.InternalName =
                    survivor.InternalName;

                entry.BodyName =
                    survivor.BodyName;


                /*
                 * Ya NO usamos ExpansionName.
                 *
                 * Para personajes modded usamos el
                 * identificador real de su ContentPack.
                 *
                 * Ejemplo:
                 * com.Dragonyck.Sora
                 */
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


                // =====================================================
                // SURVIVOR MOD DISPONIBLE
                // =====================================================

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
                        .AvailableSurvivors[id] =
                        entry;

                    continue;
                }


                // =====================================================
                // SURVIVOR MOD NO DISPONIBLE
                // =====================================================

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
                    .UnavailableSurvivors[id] =
                    entry;
            }


            // =========================================================
            // CONSERVAR MODS QUE FUERON DESINSTALADOS
            // =========================================================

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


                /*
                 * Si es un survivor oficial conocido,
                 * lo descartamos definitivamente.
                 */
                if (
                    detectedOfficialIds.Contains(
                        id
                    )
                )
                {
                    continue;
                }


                /*
                 * Si el survivor mod sigue instalado,
                 * ya fue procesado arriba.
                 */
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


                /*
                 * Este registro no pertenece actualmente
                 * a ningún survivor oficial ni a ningún
                 * survivor mod instalado.
                 *
                 * Lo consideramos contenido modded
                 * que posiblemente fue desinstalado.
                 *
                 * Conservamos su configuración.
                 */
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
                    .UnavailableSurvivors[id] =
                    entry;
            }


            // =========================================================
            // TERMINAR
            // =========================================================

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
                        .DeserializeObject<SurvivorJsonFile>(
                            json
                        );

                if (loaded != null)
                {
                    file = loaded;
                }

                if (file.AvailableSurvivors == null)
                {
                    file.AvailableSurvivors =
                        new Dictionary<string, SurvivorJsonEntry>();
                }

                if (file.UnavailableSurvivors == null)
                {
                    file.UnavailableSurvivors =
                        new Dictionary<string, SurvivorJsonEntry>();
                }

                return true;
            }
            catch (Exception exception)
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

        private static Dictionary<string, SurvivorJsonEntry>
            CombineExistingRecords(
                SurvivorJsonFile file
            )
        {
            Dictionary<string, SurvivorJsonEntry> records =
                new Dictionary<string, SurvivorJsonEntry>();

            foreach (
                KeyValuePair<string, SurvivorJsonEntry> pair
                in file.AvailableSurvivors
            )
            {
                records[pair.Key] =
                    pair.Value;
            }

            foreach (
                KeyValuePair<string, SurvivorJsonEntry> pair
                in file.UnavailableSurvivors
            )
            {
                records[pair.Key] =
                    pair.Value;
            }

            return records;
        }

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
                        pair => pair.Key,
                        pair => pair.Value
                    );

            file.UnavailableSurvivors =
                file.UnavailableSurvivors
                    .OrderBy(
                        pair =>
                            pair.Value.DisplayName
                    )
                    .ToDictionary(
                        pair => pair.Key,
                        pair => pair.Value
                    );
        }

        private static void Save(
            SurvivorJsonFile file,
            ManualLogSource logger
        )
        {
            try
            {
                if (File.Exists(JsonPath))
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
                    new UTF8Encoding(false)
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
            catch (Exception exception)
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