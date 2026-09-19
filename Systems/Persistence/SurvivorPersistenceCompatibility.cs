using System;
using System.Collections.Generic;
using BepInEx.Logging;
using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Compatibilidad y normalización no destructiva para Survivors.json.
    ///
    /// Esta capa no decide providers, presets ni progreso. Su única función es
    /// mantener legibles configuraciones antiguas, conservar campos que esta
    /// build todavía no conoce y evitar que una versión anterior sobrescriba
    /// un schema creado por una versión futura de USU.
    /// </summary>
    public static class SurvivorPersistenceCompatibility
    {
        public static bool IsReadOnly
        {
            get;
            private set;
        }


        public static bool NormalizeAfterLoad(
            SurvivorJsonFile file,
            ManualLogSource logger
        )
        {
            IsReadOnly = false;

            if (file == null)
            {
                return false;
            }

            bool changed = false;

            if (
                file.SchemaVersion <
                    SurvivorJsonFile.CurrentSchemaVersion
            )
            {
                file.SchemaVersion =
                    SurvivorJsonFile.CurrentSchemaVersion;

                changed = true;
            }
            else if (
                file.SchemaVersion >
                    SurvivorJsonFile.CurrentSchemaVersion
            )
            {
                IsReadOnly = true;

                logger?.LogWarning(
                    $"[PERSISTENCE] Survivors.json usa schema " +
                    $"{file.SchemaVersion}, superior al soportado " +
                    $"({SurvivorJsonFile.CurrentSchemaVersion}). " +
                    "Se cargará en modo sólo lectura para evitar pérdida de datos."
                );
            }

            SurvivorJsonFile defaults =
                new SurvivorJsonFile();

            if (file.Guide == null)
            {
                file.Guide =
                    new SurvivorJsonGuide();

                changed = true;
            }

            if (file.Guide.Title == null)
            {
                file.Guide.Title =
                    defaults.Guide.Title;

                changed = true;
            }

            if (file.Guide.Instructions == null)
            {
                file.Guide.Instructions =
                    defaults.Guide.Instructions;

                changed = true;
            }

            if (file.Guide.ExampleChallenge == null)
            {
                file.Guide.ExampleChallenge =
                    defaults.Guide.ExampleChallenge;

                changed = true;
            }

            changed |=
                NormalizeChallenge(
                    file.Guide.ExampleChallenge
                );

            if (file.AvailableTitle == null)
            {
                file.AvailableTitle =
                    defaults.AvailableTitle;

                changed = true;
            }

            if (file.AvailableMessage == null)
            {
                file.AvailableMessage =
                    defaults.AvailableMessage;

                changed = true;
            }

            if (file.UnavailableTitle == null)
            {
                file.UnavailableTitle =
                    defaults.UnavailableTitle;

                changed = true;
            }

            if (file.UnavailableMessage == null)
            {
                file.UnavailableMessage =
                    defaults.UnavailableMessage;

                changed = true;
            }

            if (file.AvailableSurvivors == null)
            {
                file.AvailableSurvivors =
                    new Dictionary<string, SurvivorJsonEntry>();

                changed = true;
            }

            if (file.UnavailableSurvivors == null)
            {
                file.UnavailableSurvivors =
                    new Dictionary<string, SurvivorJsonEntry>();

                changed = true;
            }

            if (file.DormantMissions == null)
            {
                file.DormantMissions =
                    new Dictionary<string, SurvivorMissionRecord>(
                        StringComparer.OrdinalIgnoreCase
                    );

                changed = true;
            }

            changed |=
                NormalizeDormantMissions(
                    file.DormantMissions
                );

            if (file.ExtraData == null)
            {
                file.ExtraData =
                    new Dictionary<string, JToken>();

                changed = true;
            }

            changed |=
                NormalizeEntries(
                    file.AvailableSurvivors
                );

            changed |=
                NormalizeEntries(
                    file.UnavailableSurvivors
                );

            if (changed)
            {
                UsuLog.Verbose(
                    logger,
                    "[PERSISTENCE] Survivors.json normalizado sin cambiar provider/preset del jugador."
                );
            }

            return changed;
        }


        public static void CopyFileMetadata(
            SurvivorJsonFile source,
            SurvivorJsonFile target
        )
        {
            if (
                source == null ||
                target == null
            )
            {
                return;
            }

            target.SchemaVersion =
                SurvivorJsonFile.CurrentSchemaVersion;

            target.Guide =
                source.Guide
                ?? new SurvivorJsonGuide();

            target.AvailableTitle =
                source.AvailableTitle
                ?? target.AvailableTitle;

            target.AvailableMessage =
                source.AvailableMessage
                ?? target.AvailableMessage;

            target.UnavailableTitle =
                source.UnavailableTitle
                ?? target.UnavailableTitle;

            target.UnavailableMessage =
                source.UnavailableMessage
                ?? target.UnavailableMessage;

            target.DormantMissions =
                new Dictionary<string, SurvivorMissionRecord>(
                    StringComparer.OrdinalIgnoreCase
                );

            target.ExtraData =
                source.ExtraData
                ?? new Dictionary<string, JToken>();
        }


        private static bool NormalizeEntries(
            Dictionary<string, SurvivorJsonEntry> entries
        )
        {
            if (entries == null)
            {
                return false;
            }

            bool changed = false;

            List<string> keys =
                new List<string>(
                    entries.Keys
                );

            foreach (string key in keys)
            {
                SurvivorJsonEntry entry =
                    entries[key];

                if (entry == null)
                {
                    entry =
                        new SurvivorJsonEntry
                        {
                            DisplayName = key ?? "",
                            InternalName = key ?? "",
                            BodyName = key ?? ""
                        };

                    entries[key] =
                        entry;

                    changed = true;
                }

                if (entry.DisplayName == null)
                {
                    entry.DisplayName = "";
                    changed = true;
                }

                if (entry.InternalName == null)
                {
                    entry.InternalName = "";
                    changed = true;
                }

                if (entry.BodyName == null)
                {
                    entry.BodyName =
                        key ?? "";

                    changed = true;
                }

                if (entry.Source == null)
                {
                    entry.Source = "";
                    changed = true;
                }

                if (entry.OriginalUnlock == null)
                {
                    entry.OriginalUnlock = "";
                    changed = true;
                }

                if (entry.Status == null)
                {
                    entry.Status = "";
                    changed = true;
                }

                if (entry.Reason == null)
                {
                    entry.Reason = "";
                    changed = true;
                }

                if (entry.Challenge == null)
                {
                    entry.Challenge =
                        new SurvivorChallengeJson();

                    changed = true;
                }

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

                    changed = true;
                }

                if (entry.ExtraData == null)
                {
                    entry.ExtraData =
                        new Dictionary<string, JToken>();

                    changed = true;
                }

                changed |=
                    NormalizeChallenge(
                        entry.Challenge
                    );
            }

            return changed;
        }


        private static bool NormalizeDormantMissions(
            Dictionary<string, SurvivorMissionRecord> records
        )
        {
            if (records == null)
            {
                return false;
            }

            bool changed = false;
            List<string> keys = new List<string>(records.Keys);

            foreach (string key in keys)
            {
                SurvivorMissionRecord record = records[key];

                if (record == null)
                {
                    records.Remove(key);
                    changed = true;
                    continue;
                }

                if (record.BodyName == null)
                {
                    record.BodyName = "";
                    changed = true;
                }

                if (record.Source == null)
                {
                    record.Source = "";
                    changed = true;
                }

                string stableId =
                    SurvivorIdentity.Build(
                        record.Source,
                        record.BodyName
                    );

                if (
                    string.IsNullOrWhiteSpace(record.SurvivorId) ||
                    (
                        SurvivorIdentity.IsBodyFallback(record.SurvivorId) &&
                        !SurvivorIdentity.IsBodyFallback(stableId)
                    )
                )
                {
                    record.SurvivorId = stableId;
                    changed = true;
                }

                if (!string.Equals(
                    record.State,
                    SurvivorMissionRecord.PendingContentState,
                    StringComparison.Ordinal
                ))
                {
                    record.State = SurvivorMissionRecord.PendingContentState;
                    changed = true;
                }

                if (record.Challenge == null)
                {
                    record.Challenge = new SurvivorChallengeJson();
                    changed = true;
                }

                changed |= NormalizeChallenge(record.Challenge);

                if (
                    !string.IsNullOrWhiteSpace(record.SurvivorId) &&
                    !string.Equals(key, record.SurvivorId, StringComparison.OrdinalIgnoreCase)
                )
                {
                    records.Remove(key);
                    records[record.SurvivorId] = record;
                    changed = true;
                }
            }

            return changed;
        }


        private static bool NormalizeChallenge(
            SurvivorChallengeJson challenge
        )
        {
            if (challenge == null)
            {
                return false;
            }

            bool changed = false;

            if (challenge.Name == null)
            {
                challenge.Name = "";
                changed = true;
            }

            if (challenge.Description == null)
            {
                challenge.Description = "";
                changed = true;
            }

            if (challenge.LocalizationKey == null)
            {
                challenge.LocalizationKey = "";
                changed = true;
            }

            if (challenge.Type == null)
            {
                challenge.Type = "Original";
                changed = true;
            }

            if (challenge.Parameters == null)
            {
                challenge.Parameters =
                    new JObject();

                changed = true;
            }

            if (challenge.ExtraData == null)
            {
                challenge.ExtraData =
                    new Dictionary<string, JToken>();

                changed = true;
            }

            MissionConfiguration missionConfig =
                challenge.MissionConfig;

            if (missionConfig != null)
            {
                if (missionConfig.Source == null)
                {
                    missionConfig.Source =
                        "Preset";

                    changed = true;
                }

                if (missionConfig.BasePresetId == null)
                {
                    missionConfig.BasePresetId =
                        "";

                    changed = true;
                }

                if (missionConfig.ExtraData == null)
                {
                    missionConfig.ExtraData =
                        new Dictionary<string, JToken>();

                    changed = true;
                }
            }

            return changed;
        }
    }
}
