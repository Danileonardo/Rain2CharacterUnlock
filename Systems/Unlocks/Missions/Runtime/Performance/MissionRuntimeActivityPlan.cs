using System;
using System.Collections.Generic;
using BepInEx.Logging;
using Newtonsoft.Json.Linq;
using RoR2;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    /*
     * =============================================================
     * MISSION RUNTIME ACTIVITY PLAN
     * =============================================================
     *
     * Objetivo:
     *
     * Evitar que cada tracker tenga que volver a:
     * - leer JSON,
     * - recorrer todas las misiones,
     * - interpretar rutas,
     * - buscar tipos de objetivo,
     *
     * cada vez que ocurre un golpe, curación, baja, buff, etc.
     *
     * El plan se reconstruye UNA VEZ al comenzar la run.
     *
     * Después los trackers sólo preguntan:
     *
     *     IsTypeActive("ApplyStatusEffects")
     *
     * o:
     *
     *     IsTypeActiveForBody(
     *         "WooperBody",
     *         "KillWithSkillWhileStatus"
     *     )
     *
     * Eso es únicamente una consulta a HashSet.
     *
     * IMPORTANTE:
     * Este sistema no elimina eventos.
     * Sólo permite que trackers innecesarios salgan inmediatamente.
     * =============================================================
     */
    public static class MissionRuntimeActivityPlan
    {
        private static ManualLogSource logger;

        private static bool initialized;

        private static bool ready;


        private static readonly HashSet<string>
            ActiveTypes =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase
                );


        private static readonly Dictionary<
            string,
            HashSet<string>
        > ActiveTypesByBody =
            new Dictionary<
                string,
                HashSet<string>
            >(
                StringComparer.OrdinalIgnoreCase
            );


        public static bool IsReady =>
            ready;


        public static int ActiveTypeCount =>
            ActiveTypes.Count;


        public static int ActiveBodyCount =>
            ActiveTypesByBody.Count;


        // =========================================================
        // INITIALIZE
        // =========================================================

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
                "[MISSION PERF] ActivityPlan inicializado."
            );
        }


        // =========================================================
        // RUN
        // =========================================================

        private static void OnRunStartGlobal(
            Run run
        )
        {
            if (!NetworkServer.active)
            {
                return;
            }


            Rebuild();
        }


        private static void OnRunDestroyGlobal(
            Run run
        )
        {
            Clear();
        }


        // =========================================================
        // REBUILD
        // =========================================================

        public static void Rebuild()
        {
            Clear();


            if (!NetworkServer.active)
            {
                return;
            }


            SurvivorJsonFile config =
                SurvivorJsonManager.CurrentConfig;


            if (
                config == null ||
                config.AvailableSurvivors == null
            )
            {
                logger?.LogWarning(
                    "[MISSION PERF] ActivityPlan sin configuración disponible."
                );

                return;
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


                if (
                    string.IsNullOrWhiteSpace(
                        bodyName
                    )
                )
                {
                    continue;
                }


                /*
                 * Muy importante:
                 * pedimos la entrada EFECTIVA a SessionMissionRegistry.
                 *
                 * Durante una run la prioridad es:
                 *
                 * RunSnapshot
                 *     ↓
                 * LobbySnapshot
                 *     ↓
                 * local
                 *
                 * Así el ActivityPlan usa la misma misión congelada
                 * que el resto del sistema.
                 */
                if (
                    !SessionMissionRegistry
                        .TryGetEffectiveEntryJson(
                            bodyName,
                            out JObject entryJson
                        ) ||
                    entryJson == null
                )
                {
                    continue;
                }


                JObject challenge =
                    GetObjectCaseInsensitive(
                        entryJson,
                        "challenge"
                    );


                if (challenge == null)
                {
                    continue;
                }


                if (
                    IsExplicitlyDisabled(
                        challenge
                    )
                )
                {
                    continue;
                }


                HashSet<string> bodyTypes =
                    new HashSet<string>(
                        StringComparer.OrdinalIgnoreCase
                    );


                CollectTypesRecursive(
                    challenge,
                    bodyTypes
                );


                if (bodyTypes.Count == 0)
                {
                    continue;
                }


                ActiveTypesByBody[
                    bodyName
                ] =
                    bodyTypes;


                foreach (
                    string type
                    in bodyTypes
                )
                {
                    ActiveTypes.Add(
                        type
                    );
                }
            }


            ready =
                true;


            logger?.LogInfo(
                "[MISSION PERF] ActivityPlan construido | " +
                $"Bodies activos: {ActiveBodyCount} | " +
                $"Tipos activos: {ActiveTypeCount}"
            );


            if (
                ActiveTypes.Count > 0
            )
            {
                logger?.LogInfo(
                    "[MISSION PERF] Tipos requeridos | " +
                    string.Join(
                        ", ",
                        ActiveTypes
                    )
                );
            }
        }


        // =========================================================
        // CONSULTAS BARATAS
        // =========================================================

        public static bool IsTypeActive(
            string missionType
        )
        {
            if (
                !ready ||
                string.IsNullOrWhiteSpace(
                    missionType
                )
            )
            {
                return false;
            }


            return ActiveTypes.Contains(
                missionType
            );
        }


        public static bool IsAnyTypeActive(
            params string[] missionTypes
        )
        {
            if (
                !ready ||
                missionTypes == null
            )
            {
                return false;
            }


            foreach (
                string missionType
                in missionTypes
            )
            {
                if (
                    !string.IsNullOrWhiteSpace(
                        missionType
                    ) &&
                    ActiveTypes.Contains(
                        missionType
                    )
                )
                {
                    return true;
                }
            }


            return false;
        }


        public static bool IsTypeActiveForBody(
            string bodyName,
            string missionType
        )
        {
            if (
                !ready ||
                string.IsNullOrWhiteSpace(
                    bodyName
                ) ||
                string.IsNullOrWhiteSpace(
                    missionType
                )
            )
            {
                return false;
            }


            return
                ActiveTypesByBody
                    .TryGetValue(
                        bodyName,
                        out HashSet<string> types
                    ) &&
                types != null &&
                types.Contains(
                    missionType
                );
        }


        // =========================================================
        // TYPE DISCOVERY
        // =========================================================

        /*
         * El Mission System actual usa challenge.type.
         *
         * El Mission System v2 puede contener:
         *
         * challenge
         * └── routes
         *     └── objectives
         *         └── type
         *
         * Por eso buscamos propiedades "type"
         * recursivamente UNA sola vez al iniciar la run.
         */
        private static void CollectTypesRecursive(
            JToken token,
            HashSet<string> output
        )
        {
            if (
                token == null ||
                output == null
            )
            {
                return;
            }


            JObject objectToken =
                token as JObject;


            if (objectToken != null)
            {
                foreach (
                    JProperty property
                    in objectToken.Properties()
                )
                {
                    if (
                        string.Equals(
                            property.Name,
                            "type",
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    {
                        string type =
                            property.Value?
                                .ToString()
                                .Trim();


                        if (
                            !string.IsNullOrWhiteSpace(
                                type
                            )
                        )
                        {
                            output.Add(
                                type
                            );
                        }
                    }


                    CollectTypesRecursive(
                        property.Value,
                        output
                    );
                }


                return;
            }


            JArray array =
                token as JArray;


            if (array != null)
            {
                foreach (
                    JToken child
                    in array
                )
                {
                    CollectTypesRecursive(
                        child,
                        output
                    );
                }
            }
        }


        private static bool IsExplicitlyDisabled(
            JObject challenge
        )
        {
            JToken enabled =
                GetTokenCaseInsensitive(
                    challenge,
                    "enabled"
                );


            if (
                enabled != null &&
                enabled.Type ==
                    JTokenType.Boolean
            )
            {
                return
                    enabled.Value<bool>() == false;
            }


            return false;
        }


        private static JObject
            GetObjectCaseInsensitive(
                JObject source,
                string propertyName
            )
        {
            return
                GetTokenCaseInsensitive(
                    source,
                    propertyName
                )
                as JObject;
        }


        private static JToken
            GetTokenCaseInsensitive(
                JObject source,
                string propertyName
            )
        {
            if (
                source == null ||
                string.IsNullOrWhiteSpace(
                    propertyName
                )
            )
            {
                return null;
            }


            foreach (
                JProperty property
                in source.Properties()
            )
            {
                if (
                    string.Equals(
                        property.Name,
                        propertyName,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    return property.Value;
                }
            }


            return null;
        }


        private static void Clear()
        {
            ActiveTypes.Clear();

            ActiveTypesByBody.Clear();

            ready =
                false;
        }
    }
}
