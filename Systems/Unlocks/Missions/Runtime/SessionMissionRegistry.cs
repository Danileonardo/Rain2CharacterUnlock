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
     * Fuente de verdad temporal para una run.
     *
     * SINGLEPLAYER:
     *     La máquina local es también servidor.
     *     Se crea un snapshot desde Survivors.json.
     *
     * MULTIPLAYER HOST:
     *     El host crea el snapshot desde SU configuración local.
     *     Ese snapshot queda congelado durante toda la run.
     *
     * MULTIPLAYER CLIENT:
     *     Recibe el snapshot del host y lo mantiene sólo en memoria.
     *
     * Al terminar la run:
     *     El snapshot se elimina.
     *
     * NUNCA se modifica Survivors.json de un cliente.
     * =============================================================
     */
    public static class SessionMissionRegistry
    {
        private static ManualLogSource logger;

        private static bool initialized;

        private static SessionMissionSnapshot activeSnapshot;

        private static bool activeSnapshotCameFromHost;


        // =========================================================
        // ESTADO
        // =========================================================

        public static bool HasSessionSnapshot =>
            activeSnapshot != null;


        public static bool SnapshotCameFromHost =>
            HasSessionSnapshot &&
            activeSnapshotCameFromHost;


        // =========================================================
        // INICIALIZAR
        // =========================================================

        public static void Initialize(
            ManualLogSource log
        )
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            logger = log;

            Run.onRunStartGlobal +=
                OnRunStartGlobal;

            Run.onRunDestroyGlobal +=
                OnRunDestroyGlobal;

            logger?.LogInfo(
                "[MISSION SESSION] Registro de misión de sesión inicializado."
            );
        }


        // =========================================================
        // COMIENZA UNA RUN
        // =========================================================

        private static void OnRunStartGlobal(
            Run run
        )
        {
            /*
             * En un cliente NO limpiamos aquí.
             *
             * El host podría enviar el paquete muy cerca del evento
             * de inicio de Run. Si el cliente limpiara aquí podríamos
             * borrar un snapshot recién recibido por una carrera
             * de eventos.
             *
             * La limpieza normal ocurre en onRunDestroyGlobal.
             */
            if (!NetworkServer.active)
            {
                logger?.LogInfo(
                    "[MISSION SESSION] Cliente esperando snapshot del host."
                );

                return;
            }


            /*
             * Host / singleplayer:
             * se congela la configuración efectiva al comenzar la run.
             */
            SessionMissionSnapshot snapshot =
                BuildSnapshotFromLocalConfig();


            ApplySnapshot(
                snapshot,
                cameFromHost: true
            );


            string json =
                JsonConvert.SerializeObject(
                    snapshot,
                    Formatting.None
                );


            logger?.LogInfo(
                $"[MISSION SESSION] Snapshot del host creado | " +
                $"Misiones: {snapshot.Missions.Count} | " +
                $"Bytes aprox.: {json.Length}"
            );


            LogImportantMission(
                snapshot,
                "WooperBody",
                "HOST"
            );


            /*
             * En singleplayer no habrá clientes remotos.
             * En multiplayer, R2API lo entregará a todos.
             */
            new SessionMissionSyncMessage(
                json
            )
            .Send(
                NetworkDestination.Clients
            );


            logger?.LogInfo(
                $"[MISSION SESSION] Snapshot enviado a clientes | " +
                $"Misiones: {snapshot.Missions.Count}"
            );
        }


        // =========================================================
        // TERMINA UNA RUN
        // =========================================================

        private static void OnRunDestroyGlobal(
            Run run
        )
        {
            ClearSessionSnapshot(
                "fin de run"
            );
        }


        // =========================================================
        // CREAR SNAPSHOT DESDE CONFIG LOCAL DEL HOST
        // =========================================================

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


                /*
                 * JObject.FromObject crea una representación
                 * independiente del objeto original.
                 *
                 * Luego DeepClone asegura que el snapshot de sesión
                 * no comparta referencias mutables con CurrentConfig.
                 */
                JObject entryJson =
                    JObject.FromObject(
                        entry
                    );


                snapshot.Missions[
                    bodyName
                ] =
                    (JObject)entryJson.DeepClone();
            }


            return snapshot;
        }


        // =========================================================
        // CLIENTE RECIBE SNAPSHOT DEL HOST
        // =========================================================

        public static void ReceiveHostSnapshot(
            string json
        )
        {
            /*
             * El host ya usa su propia copia local.
             * No debe reemplazarla con un paquete remoto.
             */
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


                ApplySnapshot(
                    snapshot,
                    cameFromHost: true
                );


                logger?.LogInfo(
                    $"[MISSION SESSION] Snapshot del host recibido | " +
                    $"Misiones: {snapshot.Missions.Count}"
                );


                LogImportantMission(
                    snapshot,
                    "WooperBody",
                    "CLIENTE"
                );
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


        // =========================================================
        // APLICAR SNAPSHOT EN MEMORIA
        // =========================================================

        private static void ApplySnapshot(
            SessionMissionSnapshot snapshot,
            bool cameFromHost
        )
        {
            if (snapshot == null)
            {
                activeSnapshot =
                    null;

                activeSnapshotCameFromHost =
                    false;

                return;
            }


            /*
             * Hacemos una segunda copia profunda.
             * De este modo ningún consumidor puede terminar
             * modificando accidentalmente el objeto recibido.
             */
            string json =
                JsonConvert.SerializeObject(
                    snapshot,
                    Formatting.None
                );


            activeSnapshot =
                JsonConvert
                    .DeserializeObject<
                        SessionMissionSnapshot
                    >(
                        json
                    )
                ?? new SessionMissionSnapshot();


            if (activeSnapshot.Missions == null)
            {
                activeSnapshot.Missions =
                    new Dictionary<
                        string,
                        JObject
                    >();
            }


            activeSnapshotCameFromHost =
                cameFromHost;
        }


        // =========================================================
        // OBTENER ENTRY EFECTIVA
        // =========================================================

        public static bool TryGetEffectiveEntry(
            string bodyName,
            out SurvivorJsonEntry entry
        )
        {
            entry =
                null;


            /*
             * 1. Durante una run:
             *    primero manda el snapshot de sesión.
             */
            if (
                TryGetSessionEntryJson(
                    bodyName,
                    out JObject sessionEntry
                )
            )
            {
                try
                {
                    entry =
                        sessionEntry
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
                        $"la entrada de sesión para {bodyName}."
                    );

                    logger?.LogError(
                        exception
                    );
                }
            }


            /*
             * 2. Menú / fuera de run / fallback:
             *    configuración local del jugador.
             */
            return TryGetLocalEntry(
                bodyName,
                out entry
            );
        }


        // =========================================================
        // OBTENER ENTRY COMO JSON
        // =========================================================

        public static bool TryGetEffectiveEntryJson(
            string bodyName,
            out JObject entryJson
        )
        {
            entryJson =
                null;


            if (
                TryGetSessionEntryJson(
                    bodyName,
                    out JObject sessionEntry
                )
            )
            {
                entryJson =
                    (JObject)
                    sessionEntry.DeepClone();

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


        // =========================================================
        // OBTENER CHALLENGE EFECTIVO COMO JSON
        // =========================================================

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


        // =========================================================
        // HELPERS DE TEXTO
        // =========================================================

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


        // =========================================================
        // ENTRY DEL SNAPSHOT
        // =========================================================

        private static bool TryGetSessionEntryJson(
            string bodyName,
            out JObject entryJson
        )
        {
            entryJson =
                null;


            if (
                activeSnapshot == null ||
                activeSnapshot.Missions == null ||
                string.IsNullOrWhiteSpace(
                    bodyName
                )
            )
            {
                return false;
            }


            if (
                !activeSnapshot.Missions
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


        // =========================================================
        // ENTRY LOCAL
        // =========================================================

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


        // =========================================================
        // LIMPIAR
        // =========================================================

        public static void ClearSessionSnapshot(
            string reason = ""
        )
        {
            bool hadSnapshot =
                activeSnapshot != null;


            activeSnapshot =
                null;

            activeSnapshotCameFromHost =
                false;


            if (hadSnapshot)
            {
                logger?.LogInfo(
                    $"[MISSION SESSION] Snapshot eliminado" +
                    $"{(
                        string.IsNullOrWhiteSpace(reason)
                            ? "."
                            : $" | Motivo: {reason}"
                    )}"
                );
            }
        }


        // =========================================================
        // LOG DE PRUEBA
        // =========================================================

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
                entry["challenge"] as JObject;


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
    }
}
