using System;
using System.Collections.Generic;
using BepInEx.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    /*
     * =============================================================
     * SESSION MISSION REGISTRY
     * =============================================================
     *
     * Mantiene DOS capas temporales:
     *
     * 1. LobbySnapshot
     *    - Configuración efectiva del host antes de iniciar la run.
     *    - Es la que debe ver la selección de personajes.
     *
     * 2. RunSnapshot
     *    - Copia congelada al comenzar la run.
     *    - Tiene prioridad absoluta mientras la run existe.
     *
     * Prioridad de lectura:
     *
     * RunSnapshot
     *     ↓
     * LobbySnapshot
     *     ↓
     * Survivors.json local
     *
     * El cliente NUNCA escribe el snapshot del host en disco.
     * =============================================================
     */
    public static class SessionMissionRegistry
    {
        private static ManualLogSource logger;

        private static bool initialized;

        private static SessionMissionSnapshot
            lobbySnapshot;

        private static SessionMissionSnapshot
            runSnapshot;

        private static bool
            lobbySnapshotCameFromHost;

        private static bool
            runSnapshotCameFromHost;


        public static event Action
            EffectiveSnapshotChanged;


        public static bool HasLobbySnapshot =>
            lobbySnapshot != null;


        public static bool HasRunSnapshot =>
            runSnapshot != null;


        public static bool LobbySnapshotCameFromHost =>
            HasLobbySnapshot &&
            lobbySnapshotCameFromHost;


        public static bool RunSnapshotCameFromHost =>
            HasRunSnapshot &&
            runSnapshotCameFromHost;


        public static void Initialize(
            ManualLogSource log
        )
        {
            if (initialized)
            {
                return;
            }


            initialized =
                true;

            logger =
                log;


            Run.onRunStartGlobal +=
                OnRunStartGlobal;

            Run.onRunDestroyGlobal +=
                OnRunDestroyGlobal;


            logger?.LogInfo(
                "[MISSION SESSION] Registro Lobby/Run inicializado."
            );
        }


        public static void
            RefreshLobbySnapshotAndBroadcast(
                string reason = ""
            )
        {
            if (!NetworkServer.active)
            {
                return;
            }


            if (Run.instance != null)
            {
                logger?.LogInfo(
                    "[MISSION LOBBY] Actualización ignorada: " +
                    "ya existe una run activa."
                );

                return;
            }


            SessionMissionSnapshot snapshot =
                BuildSnapshotFromLocalConfig();


            ApplyLobbySnapshot(
                snapshot,
                cameFromHost: true
            );


            string json =
                SerializeSnapshot(
                    snapshot
                );


            logger?.LogInfo(
                $"[MISSION LOBBY] Snapshot del host creado | " +
                $"Misiones: {snapshot.Missions.Count} | " +
                $"Bytes aprox.: {json.Length}" +
                FormatReason(
                    reason
                )
            );


            LogImportantMission(
                snapshot,
                "WooperBody",
                "LOBBY HOST"
            );


            int fragmentCount =
                SessionMissionChunkTransport
                    .SendSnapshotToClients(
                        json,
                        isRunSnapshot: false
                    );


            logger?.LogInfo(
                $"[MISSION LOBBY] Snapshot fragmentado despachado | " +
                $"Misiones: {snapshot.Missions.Count} | " +
                $"Fragmentos: {fragmentCount}"
            );
        }


        private static void OnRunStartGlobal(
            Run run
        )
        {
            if (!NetworkServer.active)
            {
                logger?.LogInfo(
                    "[MISSION RUN] Cliente esperando snapshot " +
                    "congelado del host."
                );

                return;
            }


            SessionMissionSnapshot source =
                lobbySnapshot
                ?? BuildSnapshotFromLocalConfig();


            SessionMissionSnapshot frozen =
                CloneSnapshot(
                    source
                );


            ApplyRunSnapshot(
                frozen,
                cameFromHost: true
            );


            string json =
                SerializeSnapshot(
                    frozen
                );


            logger?.LogInfo(
                $"[MISSION RUN] Snapshot congelado por el host | " +
                $"Misiones: {frozen.Missions.Count} | " +
                $"Bytes aprox.: {json.Length} | " +
                $"Fuente: " +
                $"{(lobbySnapshot != null ? "LobbySnapshot" : "LocalConfig")}"
            );


            LogImportantMission(
                frozen,
                "WooperBody",
                "RUN HOST"
            );


            int fragmentCount =
                SessionMissionChunkTransport
                    .SendSnapshotToClients(
                        json,
                        isRunSnapshot: true
                    );


            logger?.LogInfo(
                $"[MISSION RUN] Snapshot congelado fragmentado despachado | " +
                $"Misiones: {frozen.Missions.Count} | " +
                $"Fragmentos: {fragmentCount}"
            );
        }


        private static void OnRunDestroyGlobal(
            Run run
        )
        {
            ClearRunSnapshot(
                "fin de run"
            );
        }


        private static SessionMissionSnapshot
            BuildSnapshotFromLocalConfig()
        {
            SessionMissionSnapshot snapshot =
                new SessionMissionSnapshot();


            SurvivorJsonFile config =
                SurvivorJsonManager.CurrentConfig;


            if (
                config == null ||
                config.AvailableSurvivors == null
            )
            {
                logger?.LogWarning(
                    "[MISSION SESSION] No existe configuración local disponible."
                );

                return snapshot;
            }


            foreach (
                KeyValuePair<
                    string,
                    SurvivorJsonEntry
                > pair
                in config.AvailableSurvivors
            )
            {
                string bodyName =
                    pair.Key;


                SurvivorJsonEntry entry =
                    pair.Value;


                if (
                    string.IsNullOrWhiteSpace(
                        bodyName
                    ) ||
                    entry == null
                )
                {
                    continue;
                }


                JObject entryJson =
                    JObject.FromObject(
                        entry
                    );


                snapshot.Missions[
                    bodyName
                ] =
                    (JObject)
                    entryJson.DeepClone();
            }


            return snapshot;
        }


        public static void ReceiveHostSnapshot(
            string json,
            bool isRunSnapshot
        )
        {
            if (NetworkServer.active)
            {
                return;
            }


            if (string.IsNullOrWhiteSpace(json))
            {
                logger?.LogWarning(
                    "[MISSION SESSION] Se recibió un snapshot vacío."
                );

                return;
            }


            try
            {
                SessionMissionSnapshot snapshot =
                    JsonConvert
                        .DeserializeObject<
                            SessionMissionSnapshot
                        >(
                            json
                        );


                if (
                    snapshot == null ||
                    snapshot.Missions == null
                )
                {
                    logger?.LogWarning(
                        "[MISSION SESSION] Snapshot del host inválido."
                    );

                    return;
                }


                if (isRunSnapshot)
                {
                    ApplyRunSnapshot(
                        snapshot,
                        cameFromHost: true
                    );


                    logger?.LogInfo(
                        $"[MISSION RUN] Snapshot congelado del host recibido | " +
                        $"Misiones: {snapshot.Missions.Count}"
                    );


                    LogImportantMission(
                        snapshot,
                        "WooperBody",
                        "RUN CLIENTE"
                    );
                }
                else
                {
                    ApplyLobbySnapshot(
                        snapshot,
                        cameFromHost: true
                    );


                    logger?.LogInfo(
                        $"[MISSION LOBBY] Snapshot del host recibido | " +
                        $"Misiones: {snapshot.Missions.Count}"
                    );


                    LogImportantMission(
                        snapshot,
                        "WooperBody",
                        "LOBBY CLIENTE"
                    );
                }
            }
            catch (Exception exception)
            {
                logger?.LogError(
                    "[MISSION SESSION] Error al leer snapshot del host."
                );

                logger?.LogError(
                    exception
                );
            }
        }


        private static void ApplyLobbySnapshot(
            SessionMissionSnapshot snapshot,
            bool cameFromHost
        )
        {
            lobbySnapshot =
                CloneSnapshot(
                    snapshot
                );


            lobbySnapshotCameFromHost =
                lobbySnapshot != null &&
                cameFromHost;


            NotifyEffectiveSnapshotChanged();
        }


        private static void ApplyRunSnapshot(
            SessionMissionSnapshot snapshot,
            bool cameFromHost
        )
        {
            runSnapshot =
                CloneSnapshot(
                    snapshot
                );


            runSnapshotCameFromHost =
                runSnapshot != null &&
                cameFromHost;


            NotifyEffectiveSnapshotChanged();
        }


        private static SessionMissionSnapshot
            CloneSnapshot(
                SessionMissionSnapshot snapshot
            )
        {
            if (snapshot == null)
            {
                return null;
            }


            string json =
                SerializeSnapshot(
                    snapshot
                );


            SessionMissionSnapshot clone =
                JsonConvert
                    .DeserializeObject<
                        SessionMissionSnapshot
                    >(
                        json
                    )
                ?? new SessionMissionSnapshot();


            if (clone.Missions == null)
            {
                clone.Missions =
                    new Dictionary<
                        string,
                        JObject
                    >();
            }


            return clone;
        }


        private static string SerializeSnapshot(
            SessionMissionSnapshot snapshot
        )
        {
            return JsonConvert.SerializeObject(
                snapshot
                ?? new SessionMissionSnapshot(),
                Formatting.None
            );
        }


        public static bool TryGetEffectiveEntry(
            string bodyName,
            out SurvivorJsonEntry entry
        )
        {
            entry =
                null;


            if (
                TryGetEffectiveEntryJson(
                    bodyName,
                    out JObject entryJson
                )
            )
            {
                try
                {
                    entry =
                        entryJson
                            .ToObject<
                                SurvivorJsonEntry
                            >();


                    return
                        entry != null;
                }
                catch (Exception exception)
                {
                    logger?.LogError(
                        $"[MISSION SESSION] No se pudo convertir " +
                        $"la entrada efectiva para {bodyName}."
                    );

                    logger?.LogError(
                        exception
                    );
                }
            }


            return false;
        }


        public static bool TryGetEffectiveEntryJson(
            string bodyName,
            out JObject entryJson
        )
        {
            entryJson =
                null;


            if (
                TryGetSnapshotEntryJson(
                    runSnapshot,
                    bodyName,
                    out JObject runEntry
                )
            )
            {
                entryJson =
                    (JObject)
                    runEntry.DeepClone();

                return true;
            }


            if (
                TryGetSnapshotEntryJson(
                    lobbySnapshot,
                    bodyName,
                    out JObject lobbyEntry
                )
            )
            {
                entryJson =
                    (JObject)
                    lobbyEntry.DeepClone();

                return true;
            }


            if (
                TryGetLocalEntry(
                    bodyName,
                    out SurvivorJsonEntry localEntry
                )
            )
            {
                entryJson =
                    JObject.FromObject(
                        localEntry
                    );

                return true;
            }


            return false;
        }


        public static JObject GetEffectiveChallengeJson(
            string bodyName
        )
        {
            if (
                !TryGetEffectiveEntryJson(
                    bodyName,
                    out JObject entryJson
                )
            )
            {
                return null;
            }


            JObject challenge =
                entryJson[
                    "challenge"
                ] as JObject;


            if (challenge == null)
            {
                return null;
            }


            return
                (JObject)
                challenge.DeepClone();
        }


        public static string GetEffectiveMissionName(
            string bodyName
        )
        {
            JObject challenge =
                GetEffectiveChallengeJson(
                    bodyName
                );


            return
                challenge?["name"]?
                    .ToString()
                ?? "";
        }


        public static string GetEffectiveMissionDescription(
            string bodyName
        )
        {
            JObject challenge =
                GetEffectiveChallengeJson(
                    bodyName
                );


            return
                challenge?["description"]?
                    .ToString()
                ?? "";
        }


        public static string GetEffectiveMissionType(
            string bodyName
        )
        {
            JObject challenge =
                GetEffectiveChallengeJson(
                    bodyName
                );


            return
                challenge?["type"]?
                    .ToString()
                ?? "";
        }


        private static bool TryGetSnapshotEntryJson(
            SessionMissionSnapshot snapshot,
            string bodyName,
            out JObject entryJson
        )
        {
            entryJson =
                null;


            if (
                snapshot == null ||
                snapshot.Missions == null ||
                string.IsNullOrWhiteSpace(
                    bodyName
                )
            )
            {
                return false;
            }


            if (
                !snapshot.Missions
                    .TryGetValue(
                        bodyName,
                        out JObject stored
                    ) ||
                stored == null
            )
            {
                return false;
            }


            entryJson =
                stored;

            return true;
        }


        private static bool TryGetLocalEntry(
            string bodyName,
            out SurvivorJsonEntry entry
        )
        {
            entry =
                null;


            if (string.IsNullOrWhiteSpace(bodyName))
            {
                return false;
            }


            SurvivorJsonFile config =
                SurvivorJsonManager.CurrentConfig;


            if (config == null)
            {
                return false;
            }


            if (
                config.AvailableSurvivors != null &&
                config.AvailableSurvivors
                    .TryGetValue(
                        bodyName,
                        out entry
                    ) &&
                entry != null
            )
            {
                return true;
            }


            if (
                config.UnavailableSurvivors != null &&
                config.UnavailableSurvivors
                    .TryGetValue(
                        bodyName,
                        out entry
                    ) &&
                entry != null
            )
            {
                return true;
            }


            entry =
                null;

            return false;
        }


        public static void ClearRunSnapshot(
            string reason = ""
        )
        {
            bool hadSnapshot =
                runSnapshot != null;


            runSnapshot =
                null;

            runSnapshotCameFromHost =
                false;


            if (hadSnapshot)
            {
                logger?.LogInfo(
                    "[MISSION RUN] Snapshot eliminado" +
                    FormatReason(
                        reason
                    )
                );


                NotifyEffectiveSnapshotChanged();
            }
        }


        public static void ClearLobbySnapshot(
            string reason = ""
        )
        {
            bool hadSnapshot =
                lobbySnapshot != null;


            lobbySnapshot =
                null;

            lobbySnapshotCameFromHost =
                false;


            if (hadSnapshot)
            {
                logger?.LogInfo(
                    "[MISSION LOBBY] Snapshot eliminado" +
                    FormatReason(
                        reason
                    )
                );


                NotifyEffectiveSnapshotChanged();
            }
        }


        public static void ClearSessionSnapshot(
            string reason = ""
        )
        {
            ClearRunSnapshot(
                reason
            );
        }


        private static void NotifyEffectiveSnapshotChanged()
        {
            try
            {
                EffectiveSnapshotChanged?.Invoke();
            }
            catch (Exception exception)
            {
                logger?.LogError(
                    "[MISSION SESSION] Error notificando cambio de snapshot."
                );

                logger?.LogError(
                    exception
                );
            }
        }


        private static void LogImportantMission(
            SessionMissionSnapshot snapshot,
            string bodyName,
            string side
        )
        {
            if (
                snapshot == null ||
                snapshot.Missions == null ||
                !snapshot.Missions
                    .TryGetValue(
                        bodyName,
                        out JObject entry
                    ) ||
                entry == null
            )
            {
                logger?.LogInfo(
                    $"[MISSION SESSION] {side} | " +
                    $"{bodyName} no está presente en el snapshot."
                );

                return;
            }


            JObject challenge =
                entry[
                    "challenge"
                ] as JObject;


            string missionName =
                challenge?["name"]?
                    .ToString()
                ?? "";


            string missionDescription =
                challenge?["description"]?
                    .ToString()
                ?? "";


            string missionType =
                challenge?["type"]?
                    .ToString()
                ?? "";


            missionDescription =
                missionDescription
                    .Replace(
                        "\r",
                        " "
                    )
                    .Replace(
                        "\n",
                        " / "
                    );


            logger?.LogInfo(
                $"[MISSION SESSION] {side} | " +
                $"Body: {bodyName} | " +
                $"Nombre: {missionName} | " +
                $"Tipo: {missionType} | " +
                $"Descripción: {missionDescription}"
            );
        }


        private static string FormatReason(
            string reason
        )
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return ".";
            }


            return
                $" | Motivo: {reason}";
        }
    }
}
