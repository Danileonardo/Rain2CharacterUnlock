using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using Newtonsoft.Json;

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
            Path.Combine(
                Paths.ConfigPath,
                "UniversalSurvivorUnlocks"
            );

        private static string JsonPath =>
            Path.Combine(
                ConfigDirectory,
                "Survivors.json"
            );

        private static string BackupPath =>
            Path.Combine(
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

        if (!TryLoadExisting(
            logger,
            out SurvivorJsonFile file
        ))
        {
            return;
        }

        CurrentConfig = file;

        logger.LogInfo(
            $"Configuración inicial cargada. " +
            $"Disponibles guardados: " +
            $"{CurrentConfig.AvailableSurvivors.Count}"
        );
    }

        public static void Sync(
            List<SurvivorInfo> detectedSurvivors,
            ManualLogSource logger
        )
        {
            Directory.CreateDirectory(
                ConfigDirectory
            );

            SurvivorJsonFile existingFile;

            if (!TryLoadExisting(
                logger,
                out existingFile
            ))
            {
                return;
            }

            Dictionary<string, SurvivorJsonEntry> savedRecords =
                CombineExistingRecords(
                    existingFile
                );

            SurvivorJsonFile newFile =
                new SurvivorJsonFile();

            HashSet<string> detectedIds =
                new HashSet<string>();

            foreach (
                SurvivorInfo survivor
                in detectedSurvivors
            )
            {
                // Los ocultos y no seleccionables
                // NO aparecen en el JSON.
                if (
                    survivor.Status == SurvivorStatus.Hidden ||
                    survivor.Status == SurvivorStatus.NotSelectable
                )
                {
                    continue;
                }

                string id =
                    survivor.BodyName;

                if (
                    string.IsNullOrWhiteSpace(id) ||
                    id == "Sin Body"
                )
                {
                    continue;
                }

                detectedIds.Add(id);

                SurvivorJsonEntry entry;

                if (!savedRecords.TryGetValue(
                    id,
                    out entry
                ))
                {
                    entry =
                        new SurvivorJsonEntry();
                }

                // Actualizamos solamente datos detectados.
                // La challenge existente NO se reemplaza.
                entry.DisplayName =
                    survivor.DisplayName;

                entry.InternalName =
                    survivor.InternalName;

                entry.BodyName =
                    survivor.BodyName;

                entry.Source =
                    survivor.ExpansionName;

                entry.OriginalUnlock =
                    survivor.UnlockableName;

                if (entry.Challenge == null)
                {
                    entry.Challenge =
                        new SurvivorChallengeJson();
                }

                switch (survivor.Status)
                {
                    case SurvivorStatus.Available:

                        entry.Available = true;

                        entry.Status =
                            "Available";

                        entry.Reason = "";

                        newFile
                            .AvailableSurvivors[id] =
                            entry;

                        break;

                    case SurvivorStatus.DlcNotOwned:

                        entry.Available = false;

                        entry.Status =
                            "DlcNotOwned";

                        entry.Reason =
                            "Requires DLC: " +
                            survivor.ExpansionName;

                        newFile
                            .UnavailableSurvivors[id] =
                            entry;

                        break;
                }
            }

            // Si existía anteriormente pero ahora
            // no aparece en SurvivorCatalog,
            // conservamos todos sus datos.
            foreach (
                KeyValuePair<string, SurvivorJsonEntry> pair
                in savedRecords
            )
            {
                if (detectedIds.Contains(pair.Key))
                {
                    continue;
                }

                SurvivorJsonEntry entry =
                    pair.Value;

                entry.Available = false;

                entry.Status =
                    "MissingContent";

                entry.Reason =
                    "Survivor not detected. " +
                    "Its mod or required content may be disabled or uninstalled.";

                if (entry.Challenge == null)
                {
                    entry.Challenge =
                        new SurvivorChallengeJson();
                }

                newFile
                    .UnavailableSurvivors[pair.Key] =
                    entry;
            }

            SortEntries(newFile);

            CurrentConfig = newFile;

            Save(
                newFile,
                logger
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