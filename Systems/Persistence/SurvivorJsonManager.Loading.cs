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


            bool migrationChanged =
                SurvivorPersistenceCompatibility
                    .NormalizeAfterLoad(
                        file,
                        logger
                    );


            CurrentConfig =
                file;


            // =====================================================
            // C5 - MIGRACIÓN 0.3.0 -> 0.3.1
            // =====================================================
            //
            // NormalizeAfterLoad ya era no destructivo, pero una
            // normalización de schema podía quedar sólo en memoria hasta
            // que otra operación provocara un Save posterior. Para el RC
            // hacemos explícita la persistencia de la migración apenas se
            // carga el archivo, conservando el backup atómico habitual.
            //
            // Un schema futuro sigue protegido por IsReadOnly y jamás se
            // sobrescribe desde una build anterior.
            // =====================================================

            if (
                migrationChanged &&
                !SurvivorPersistenceCompatibility.IsReadOnly
            )
            {
                bool migrationSaved =
                    Save(
                        CurrentConfig,
                        logger
                    );

                if (migrationSaved)
                {
                    logger?.LogInfo(
                        "[PERSISTENCE MIGRATION] Normalización persistida | " +
                        $"Schema: {CurrentConfig.SchemaVersion} | " +
                        "Backup previo: preservado cuando existía Survivors.json."
                    );
                }
                else
                {
                    logger?.LogWarning(
                        "[PERSISTENCE MIGRATION] La normalización quedó sólo en memoria porque no pudo persistirse."
                    );
                }
            }


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
                    .AuthoringMode &&
                !SurvivorPersistenceCompatibility
                    .IsReadOnly
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
    }
}
