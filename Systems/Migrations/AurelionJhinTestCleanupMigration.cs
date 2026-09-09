using System;
using System.Collections.Generic;

using BepInEx.Logging;
using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Migración de desarrollo de 5G.1D-F.
    ///
    /// AurelionSolBody fue utilizado deliberadamente como sujeto de prueba
    /// para Original <-> USU y recibió temporalmente el preset oficial de
    /// Jhin. Esa asignación no debe quedar como valor de producción.
    ///
    /// La migración:
    /// - sólo se evalúa una vez por Survivors.json;
    /// - sólo restaura si todavía encuentra EXACTAMENTE la asignación de
    ///   prueba Aurelion -> creator.official.jhin.fourth_act;
    /// - utiliza MissionAssignmentService.RestoreCharacter para respetar la
    ///   arquitectura normal de providers;
    /// - marca la entrada para no interferir nunca con una asignación futura
    ///   que el jugador haga voluntariamente desde la UI.
    ///
    /// Esta clase puede eliminarse cuando 5G.1D-F quede cerrado.
    /// </summary>
    public static class AurelionJhinTestCleanupMigration
    {
        private const string BodyName =
            "AurelionSolBody";


        private const string MigrationMarker =
            "_usuMigration_5G1DF_AurelionJhinTestCleaned";


        public static bool Apply(
            ManualLogSource logger = null
        )
        {
            SurvivorJsonEntry entry =
                SurvivorJsonManager.GetEntryAnywhere(
                    BodyName
                );


            if (entry == null)
            {
                return false;
            }


            EnsureExtraData(
                entry
            );


            if (
                HasCompletedMarker(
                    entry
                )
            )
            {
                return false;
            }


            MissionConfiguration missionConfig =
                entry.Challenge?.MissionConfig;


            bool isKnownTestAssignment =
                missionConfig != null &&
                string.Equals(
                    missionConfig.Source,
                    "Preset",
                    StringComparison.OrdinalIgnoreCase
                ) &&
                string.Equals(
                    missionConfig.BasePresetId?.Trim() ?? "",
                    MissionPresetIds.JhinOfficial,
                    StringComparison.OrdinalIgnoreCase
                );


            if (!isKnownTestAssignment)
            {
                MarkCompleted(
                    entry
                );


                SurvivorJsonManager.TrySaveCurrentConfig(
                    logger
                );


                UsuLog.Verbose(
                    logger,
                    "[MIGRATION 5G.1D-F] Aurelion no conserva la " +
                    "asignación de prueba de Jhin; migración marcada como " +
                    "completada sin cambios."
                );


                return false;
            }


            MissionAssignmentResult result =
                MissionAssignmentService.RestoreCharacter(
                    BodyName,
                    logger
                );


            if (
                result == null ||
                !result.Success
            )
            {
                logger?.LogWarning(
                    "[MIGRATION 5G.1D-F] No se pudo restaurar Aurelion " +
                    "desde la asignación temporal de Jhin. Se reintentará " +
                    "en el próximo arranque."
                );


                return false;
            }


            // RestoreCharacter conserva la misma SurvivorJsonEntry, pero la
            // recuperamos de nuevo para no depender de ese detalle interno.
            entry =
                SurvivorJsonManager.GetEntryAnywhere(
                    BodyName
                );


            if (entry == null)
            {
                return true;
            }


            EnsureExtraData(
                entry
            );


            MarkCompleted(
                entry
            );


            SurvivorJsonManager.TrySaveCurrentConfig(
                logger
            );


            logger?.LogInfo(
                "[MIGRATION 5G.1D-F] Aurelion restaurado desde el preset " +
                "temporal de Jhin a su comportamiento original."
            );


            return true;
        }


        private static void EnsureExtraData(
            SurvivorJsonEntry entry
        )
        {
            if (entry.ExtraData == null)
            {
                entry.ExtraData =
                    new Dictionary<string, JToken>();
            }
        }


        private static bool HasCompletedMarker(
            SurvivorJsonEntry entry
        )
        {
            if (
                entry?.ExtraData == null ||
                !entry.ExtraData.TryGetValue(
                    MigrationMarker,
                    out JToken value
                ) ||
                value == null
            )
            {
                return false;
            }


            try
            {
                return value.Value<bool>();
            }
            catch
            {
                return false;
            }
        }


        private static void MarkCompleted(
            SurvivorJsonEntry entry
        )
        {
            EnsureExtraData(
                entry
            );


            entry.ExtraData[
                MigrationMarker
            ] =
                new JValue(true);
        }
    }
}
