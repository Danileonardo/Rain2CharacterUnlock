using System;
using System.Collections.Generic;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// G2 — Persistencia mission-only para contenido ausente.
    ///
    /// Esta capa trabaja exclusivamente con identidad mínima y Challenge.
    /// Nunca conserva datos runtime del survivor ni intenta construir un
    /// ContentProfile cuando el mod no está instalado.
    /// </summary>
    public static partial class SurvivorJsonManager
    {
        public static SurvivorMissionRecord GetDormantMissionRecord(
            string survivorId
        )
        {
            if (
                string.IsNullOrWhiteSpace(survivorId) ||
                CurrentConfig?.DormantMissions == null
            )
            {
                return null;
            }

            return
                CurrentConfig.DormantMissions.TryGetValue(
                    survivorId,
                    out SurvivorMissionRecord record
                )
                    ? record
                    : null;
        }


        public static SurvivorMissionRecord GetMissionRecordBySurvivorId(
            string survivorId
        )
        {
            if (string.IsNullOrWhiteSpace(survivorId))
            {
                return null;
            }

            SurvivorJsonEntry entry =
                FindEntryBySurvivorId(
                    survivorId
                );

            if (entry != null)
            {
                return
                    SurvivorMissionRecord.FromEntry(
                        entry.BodyName,
                        entry
                    );
            }

            return
                GetDormantMissionRecord(
                    survivorId
                );
        }


        private static Dictionary<string, SurvivorMissionRecord>
            BuildMissionRecordIndex(
                SurvivorJsonFile file
            )
        {
            Dictionary<string, SurvivorMissionRecord> records =
                new Dictionary<string, SurvivorMissionRecord>(
                    StringComparer.OrdinalIgnoreCase
                );

            if (file == null)
            {
                return records;
            }

            // Indexamos primero entradas activas/no disponibles del schema
            // anterior. Durante Sync sólo se consumirá la misión y todos los
            // metadatos runtime se reconstruirán desde el contenido real.
            AddEntryMissionRecords(
                records,
                file.AvailableSurvivors
            );

            AddEntryMissionRecords(
                records,
                file.UnavailableSurvivors
            );

            // El registro dormido es la fuente autoritativa para contenido que
            // estuvo ausente. Se agrega al final para no quedar sobrescrito por
            // una entrada automática creada durante el bootstrap del retorno.
            AddDormantRecords(
                records,
                file.DormantMissions
            );

            return records;
        }


        private static void AddEntryMissionRecords(
            Dictionary<string, SurvivorMissionRecord> target,
            Dictionary<string, SurvivorJsonEntry> entries
        )
        {
            if (
                target == null ||
                entries == null
            )
            {
                return;
            }

            foreach (
                KeyValuePair<string, SurvivorJsonEntry> pair
                in entries
            )
            {
                SurvivorMissionRecord record =
                    SurvivorMissionRecord.FromEntry(
                        pair.Key,
                        pair.Value
                    );

                AddMissionRecord(
                    target,
                    record
                );
            }
        }


        private static void AddDormantRecords(
            Dictionary<string, SurvivorMissionRecord> target,
            Dictionary<string, SurvivorMissionRecord> source
        )
        {
            if (
                target == null ||
                source == null
            )
            {
                return;
            }

            foreach (
                KeyValuePair<string, SurvivorMissionRecord> pair
                in source
            )
            {
                SurvivorMissionRecord record = pair.Value;

                if (record == null)
                {
                    continue;
                }

                if (
                    string.IsNullOrWhiteSpace(record.SurvivorId)
                )
                {
                    record.SurvivorId = pair.Key ?? "";
                }

                AddMissionRecord(
                    target,
                    record
                );
            }
        }


        private static void AddMissionRecord(
            Dictionary<string, SurvivorMissionRecord> target,
            SurvivorMissionRecord record
        )
        {
            if (
                target == null ||
                record == null
            )
            {
                return;
            }

            NormalizeMissionRecord(record);

            if (
                string.IsNullOrWhiteSpace(record.SurvivorId)
            )
            {
                return;
            }

            target[record.SurvivorId] = record;
        }


        private static SurvivorMissionRecord TakeMissionRecord(
            Dictionary<string, SurvivorMissionRecord> records,
            string survivorId,
            string bodyName
        )
        {
            if (records == null)
            {
                return null;
            }

            if (
                !string.IsNullOrWhiteSpace(survivorId) &&
                records.TryGetValue(
                    survivorId,
                    out SurvivorMissionRecord exact
                )
            )
            {
                records.Remove(survivorId);
                return exact;
            }

            // Compatibilidad transitoria con IDs fallback de G1 / schema v1.
            // Sólo se acepta el mismo Body; nunca se empareja por nombre visible.
            string matchedKey = null;
            SurvivorMissionRecord matched = null;

            foreach (
                KeyValuePair<string, SurvivorMissionRecord> pair
                in records
            )
            {
                SurvivorMissionRecord candidate = pair.Value;

                if (
                    candidate == null ||
                    !string.Equals(
                        candidate.BodyName,
                        bodyName,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    continue;
                }

                if (
                    SurvivorIdentity.IsBodyFallback(candidate.SurvivorId) ||
                    string.IsNullOrWhiteSpace(candidate.Source)
                )
                {
                    matchedKey = pair.Key;
                    matched = candidate;
                    break;
                }
            }

            if (matched != null)
            {
                records.Remove(matchedKey);
            }

            return matched;
        }


        private static bool IsMissingContentEntry(
            SurvivorJsonEntry entry
        )
        {
            return
                entry != null &&
                (
                    string.Equals(
                        entry.Status,
                        "MissingContent",
                        StringComparison.OrdinalIgnoreCase
                    ) ||
                    string.Equals(
                        entry.Status,
                        SurvivorMissionRecord.PendingContentState,
                        StringComparison.OrdinalIgnoreCase
                    )
                );
        }


        private static void NormalizeMissionRecord(
            SurvivorMissionRecord record
        )
        {
            if (record == null)
            {
                return;
            }

            record.BodyName = record.BodyName ?? "";
            record.Source = record.Source ?? "";
            record.State = SurvivorMissionRecord.PendingContentState;

            string canonicalId =
                SurvivorIdentity.Build(
                    record.Source,
                    record.BodyName
                );

            if (
                string.IsNullOrWhiteSpace(record.SurvivorId) ||
                (
                    SurvivorIdentity.IsBodyFallback(record.SurvivorId) &&
                    !SurvivorIdentity.IsBodyFallback(canonicalId)
                )
            )
            {
                record.SurvivorId = canonicalId;
            }

            if (record.Challenge == null)
            {
                record.Challenge =
                    new SurvivorChallengeJson();
            }
        }


        private static int CountDormantMissionRecords(
            SurvivorJsonFile file
        )
        {
            return
                file?.DormantMissions?.Count
                ?? 0;
        }
    }
}
