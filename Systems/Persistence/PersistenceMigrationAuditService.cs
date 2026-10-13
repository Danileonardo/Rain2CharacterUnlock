using System;
using System.Collections.Generic;
using BepInEx.Logging;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// C5 - Auditoría pasiva de la migración de persistencia para el RC 0.3.1.
    ///
    /// No modifica Survivors.json. Verifica que el archivo que ya pasó por
    /// NormalizeAfterLoad + Sync conserve una forma compatible con los cuatro
    /// proveedores y con los slots dormidos introducidos durante G8.
    /// </summary>
    public static class PersistenceMigrationAuditService
    {
        public static void Audit(
            ManualLogSource logger
        )
        {
            SurvivorJsonFile file =
                SurvivorJsonManager.CurrentConfig;

            if (file == null)
            {
                logger?.LogWarning(
                    "[PERSISTENCE MIGRATION] C5 sin configuración cargada; auditoría omitida."
                );

                return;
            }

            int available = 0;
            int unavailable = 0;
            int dormant =
                file.DormantMissions?.Count ?? 0;

            int missionConfigs = 0;
            int providerExplicit = 0;
            int providerLegacy = 0;
            int generatedActive = 0;
            int generatedDormant = 0;
            int customActive = 0;
            int customDormant = 0;
            int usuRemembered = 0;
            int invalid = 0;

            AuditEntries(
                file.AvailableSurvivors,
                ref available,
                ref missionConfigs,
                ref providerExplicit,
                ref providerLegacy,
                ref generatedActive,
                ref generatedDormant,
                ref customActive,
                ref customDormant,
                ref usuRemembered,
                ref invalid
            );

            AuditEntries(
                file.UnavailableSurvivors,
                ref unavailable,
                ref missionConfigs,
                ref providerExplicit,
                ref providerLegacy,
                ref generatedActive,
                ref generatedDormant,
                ref customActive,
                ref customDormant,
                ref usuRemembered,
                ref invalid
            );

            string status =
                invalid == 0
                    ? "OK"
                    : "REVIEW";

            logger?.LogInfo(
                $"[PERSISTENCE MIGRATION] C5 {status} | " +
                $"Schema: {file.SchemaVersion}/{SurvivorJsonFile.CurrentSchemaVersion} | " +
                $"Disponibles: {available} | No disponibles: {unavailable} | Dormant: {dormant} | " +
                $"MissionConfig: {missionConfigs} | Provider explícito: {providerExplicit} | " +
                $"Legacy compatible: {providerLegacy} | Generated activa/dormida: {generatedActive}/{generatedDormant} | " +
                $"Custom activa/dormida: {customActive}/{customDormant} | USU recordado: {usuRemembered} | " +
                $"Inconsistencias: {invalid} | ReadOnly: {SurvivorPersistenceCompatibility.IsReadOnly}"
            );
        }


        private static void AuditEntries(
            Dictionary<string, SurvivorJsonEntry> entries,
            ref int entryCount,
            ref int missionConfigs,
            ref int providerExplicit,
            ref int providerLegacy,
            ref int generatedActive,
            ref int generatedDormant,
            ref int customActive,
            ref int customDormant,
            ref int usuRemembered,
            ref int invalid
        )
        {
            if (entries == null)
            {
                return;
            }

            foreach (
                KeyValuePair<string, SurvivorJsonEntry> pair
                in entries
            )
            {
                SurvivorJsonEntry entry =
                    pair.Value;

                if (entry == null)
                {
                    invalid++;
                    continue;
                }

                entryCount++;

                SurvivorChallengeJson challenge =
                    entry.Challenge;

                if (challenge == null)
                {
                    invalid++;
                    continue;
                }

                MissionConfiguration config =
                    challenge.MissionConfig;

                if (config == null)
                {
                    // Configuración pre-provider válida: el runtime todavía
                    // puede resolverla por Type/Parameters legacy.
                    providerLegacy++;
                    continue;
                }

                missionConfigs++;

                if (config.Provider.HasValue)
                {
                    providerExplicit++;
                }
                else
                {
                    providerLegacy++;
                }

                if (
                    string.Equals(
                        config.Source,
                        "Generated",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    generatedActive++;

                    if (
                        config.GeneratedMission == null &&
                        challenge.Mission == null
                    )
                    {
                        invalid++;
                    }
                }
                else if (config.GeneratedMission != null)
                {
                    generatedDormant++;
                }

                if (
                    string.Equals(
                        config.Source,
                        "Custom",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    customActive++;

                    if (
                        config.CustomMission == null &&
                        config.CustomMissionSnapshot?.Mission == null &&
                        challenge.Mission == null
                    )
                    {
                        invalid++;
                    }
                }
                else if (
                    config.CustomMission != null ||
                    config.CustomMissionSnapshot != null
                )
                {
                    customDormant++;
                }

                if (!string.IsNullOrWhiteSpace(config.UsuPresetId))
                {
                    usuRemembered++;
                }
            }
        }
    }
}
