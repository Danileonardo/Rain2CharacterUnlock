using System;
using System.IO;
using System.Text;
using BepInEx.Logging;
using Newtonsoft.Json;

namespace UniversalSurvivorUnlocks
{
    public static partial class SurvivorJsonManager
    {
        // =========================================================
        // GUARDAR CONFIGURACIÓN ACTUAL
        // =========================================================
        //
        // Punto público controlado para servicios como
        // MissionAssignmentService. La lógica real de serialización,
        // backup y ruta sigue encapsulada en Save(...).
        // =========================================================

        public static void SaveCurrentConfig(
            ManualLogSource logger
        )
        {
            TrySaveCurrentConfig(
                logger
            );
        }


        // =========================================================
        // GUARDAR CONFIGURACIÓN ACTUAL Y REPORTAR RESULTADO
        // =========================================================
        //
        // MissionAssignmentService necesita distinguir entre una
        // asignación válida y una asignación que no pudo persistirse.
        // El método anterior se conserva por compatibilidad.
        // =========================================================

        public static bool TrySaveCurrentConfig(
            ManualLogSource logger
        )
        {
            if (
                SurvivorPersistenceCompatibility
                    .IsReadOnly
            )
            {
                logger?.LogWarning(
                    "[PERSISTENCE] Guardado omitido: Survivors.json pertenece a un schema futuro y está protegido en modo sólo lectura."
                );

                return false;
            }


            if (CurrentConfig == null)
            {
                CurrentConfig =
                    new SurvivorJsonFile();
            }


            return Save(
                CurrentConfig,
                logger
            );
        }


        // =========================================================
        // GUARDAR
        // =========================================================
        //
        // El JSON completo se serializa primero a un archivo temporal.
        // Sólo cuando esa escritura termina correctamente se sustituye
        // Survivors.json. De esta forma una excepción durante la
        // serialización/escritura no deja truncado el archivo principal.
        //
        // Si ya existía un JSON, el reemplazo conserva además
        // Survivors.backup.json con la versión inmediatamente anterior.
        // =========================================================

        private static bool Save(
            SurvivorJsonFile file,
            ManualLogSource logger
        )
        {
            try
            {
                Directory.CreateDirectory(
                    ConfigDirectory
                );


                string json =
                    JsonConvert.SerializeObject(
                        file,
                        Formatting.Indented
                    );


                File.WriteAllText(
                    TempPath,
                    json,
                    new UTF8Encoding(
                        false
                    )
                );


                CommitStagedFile();


                UsuLog.Verbose(
                    logger,
                    "Survivors.json actualizado correctamente."
                );


                UsuLog.Verbose(
                    logger,
                    $"Disponibles en JSON: " +
                    $"{file.AvailableSurvivors.Count}"
                );


                UsuLog.Verbose(
                    logger,
                    $"No disponibles en JSON: " +
                    $"{file.UnavailableSurvivors.Count}"
                );


                UsuLog.Verbose(
                    logger,
                    $"Ruta: {JsonPath}"
                );


                return true;
            }
            catch (
                Exception exception
            )
            {
                logger?.LogError(
                    "No se pudo guardar Survivors.json."
                );


                logger?.LogError(
                    exception.Message
                );


                return false;
            }
            finally
            {
                TryDeleteTempFile();
            }
        }


        // =========================================================
        // CONFIRMAR ARCHIVO TEMPORAL
        // =========================================================

        private static void CommitStagedFile()
        {
            if (!File.Exists(TempPath))
            {
                throw new IOException(
                    "No existe el archivo temporal de Survivors.json."
                );
            }


            if (!File.Exists(JsonPath))
            {
                File.Move(
                    TempPath,
                    JsonPath
                );

                return;
            }


            File.Copy(
                JsonPath,
                BackupPath,
                true
            );


            try
            {
                File.Replace(
                    TempPath,
                    JsonPath,
                    null
                );


                return;
            }
            catch (
                PlatformNotSupportedException
            )
            {
                /*
                 * Algunos runtimes/plataformas no implementan File.Replace.
                 * El fallback mantiene el mismo contrato: primero backup,
                 * después sustituir el archivo principal.
                 */
            }
            catch (
                IOException
            )
            {
                /*
                 * Si File.Replace no está disponible para este filesystem,
                 * usamos el fallback seguro por copia.
                 */
            }


            if (!File.Exists(TempPath))
            {
                throw new IOException(
                    "No fue posible confirmar Survivors.json."
                );
            }


            File.Copy(
                TempPath,
                JsonPath,
                true
            );


            File.Delete(
                TempPath
            );
        }


        private static void TryDeleteTempFile()
        {
            try
            {
                if (File.Exists(TempPath))
                {
                    File.Delete(
                        TempPath
                    );
                }
            }
            catch
            {
                /*
                 * No ocultamos el resultado del guardado por no poder
                 * eliminar un temporal residual.
                 */
            }
        }
    }
}
