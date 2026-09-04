using System;
using System.Collections.Generic;

using BepInEx.Logging;

using RoR2;

using UnityEngine.Networking;


namespace UniversalSurvivorUnlocks
{
    /*
     * =============================================================
     * MISSION RUNTIME CATALOG
     * =============================================================
     *
     * Cache de Mission Schema v2 para la run actual.
     *
     * FUENTE:
     *
     * SessionMissionRegistry
     *
     * Por tanto respeta:
     *
     * RunSnapshot
     *   >
     * LobbySnapshot
     *   >
     * configuración local
     *
     * Sólo se construye en el HOST.
     *
     * Una vez construido, un handler puede preguntar:
     *
     * GetObjectives("HoldItemStack")
     *
     * y recibe únicamente los objetivos relevantes.
     *
     * No se vuelve a recorrer Survivors.json por cada evento.
     * =============================================================
     */
    public static class MissionRuntimeCatalog
    {
        private static ManualLogSource logger;

        private static bool initialized;

        private static bool ready;


        private static readonly List<
            MissionObjectiveRuntimeBinding
        > AllObjectives =
            new List<
                MissionObjectiveRuntimeBinding
            >();


        private static readonly Dictionary<
            string,
            List<MissionObjectiveRuntimeBinding>
        > ObjectivesByType =
            new Dictionary<
                string,
                List<MissionObjectiveRuntimeBinding>
            >(
                StringComparer.OrdinalIgnoreCase
            );


        public static event Action Rebuilt;


        public static bool IsReady =>
            ready;


        public static int ObjectiveCount =>
            AllObjectives.Count;


        public static int ObjectiveTypeCount =>
            ObjectivesByType.Count;


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
                "[MISSION V2] RuntimeCatalog inicializado."
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
                    "[MISSION V2] RuntimeCatalog sin configuración disponible."
                );

                ready =
                    true;

                Rebuilt?.Invoke();

                return;
            }


            int missionCount =
                0;


            foreach (
                KeyValuePair<
                    string,
                    SurvivorJsonEntry
                > localPair
                in config.AvailableSurvivors
            )
            {
                string lookupBodyName =
                    localPair.Key;


                if (
                    string.IsNullOrWhiteSpace(
                        lookupBodyName
                    )
                )
                {
                    continue;
                }


                /*
                 * Nunca usamos directamente la entrada local para
                 * la misión de la run.
                 *
                 * SessionMissionRegistry nos devuelve la entrada
                 * efectiva congelada.
                 */
                if (
                    !SessionMissionRegistry
                        .TryGetEffectiveEntry(
                            lookupBodyName,
                            out SurvivorJsonEntry entry
                        ) ||
                    entry == null ||
                    entry.Challenge == null
                )
                {
                    continue;
                }


                if (!entry.Challenge.Enabled)
                {
                    continue;
                }


                MissionDefinition mission =
                    entry.Challenge.Mission;


                /*
                 * Mission == null:
                 * sigue siendo Schema V1 / legacy.
                 *
                 * Lo continúa manejando ChallengeCompletionRouter.
                 */
                if (
                    mission == null ||
                    mission.Routes == null ||
                    mission.Routes.Count == 0
                )
                {
                    continue;
                }


                string bodyName =
                    !string.IsNullOrWhiteSpace(
                        entry.BodyName
                    )
                        ? entry.BodyName.Trim()
                        : lookupBodyName.Trim();


                /*
                 * El dispatcher v2 sólo debe trabajar con
                 * desbloqueos administrados realmente por USU.
                 */
                if (
                    !SurvivorUnlockManager
                        .TryGetCustomUnlockable(
                            bodyName,
                            out UnlockableDef unlockable
                        ) ||
                    unlockable == null
                )
                {
                    continue;
                }


                int addedForMission =
                    0;


                for (
                    int routeIndex = 0;
                    routeIndex <
                        mission.Routes.Count;
                    routeIndex++
                )
                {
                    MissionRoute route =
                        mission.Routes[
                            routeIndex
                        ];


                    if (route == null)
                    {
                        continue;
                    }


                    IReadOnlyList<MissionObjective>
                        objectives =
                            route
                                .GetEffectiveObjectives();


                    if (
                        objectives == null ||
                        objectives.Count == 0
                    )
                    {
                        continue;
                    }


                    for (
                        int objectiveIndex = 0;
                        objectiveIndex <
                            objectives.Count;
                        objectiveIndex++
                    )
                    {
                        MissionObjective objective =
                            objectives[
                                objectiveIndex
                            ];


                        if (
                            objective == null ||
                            string.IsNullOrWhiteSpace(
                                objective.Type
                            )
                        )
                        {
                            continue;
                        }


                        MissionObjectiveRuntimeBinding
                            binding =
                                new MissionObjectiveRuntimeBinding
                                {
                                    BodyName =
                                        bodyName,

                                    MissionId =
                                        bodyName,

                                    Mission =
                                        mission,

                                    Route =
                                        route,

                                    Objective =
                                        objective,

                                    RouteIndex =
                                        routeIndex,

                                    ObjectiveIndex =
                                        objectiveIndex,

                                    RouteId =
                                        MissionProgressKeyBuilder
                                            .GetRouteId(
                                                route,
                                                routeIndex
                                            ),

                                    ObjectiveId =
                                        MissionProgressKeyBuilder
                                            .GetObjectiveId(
                                                objective,
                                                objectiveIndex
                                            ),

                                    StorageObjectiveId =
                                        MissionProgressKeyBuilder
                                            .GetStorageObjectiveId(
                                                route,
                                                routeIndex,
                                                objective,
                                                objectiveIndex
                                            )
                                };


                        AllObjectives.Add(
                            binding
                        );


                        if (
                            !ObjectivesByType
                                .TryGetValue(
                                    objective.Type,
                                    out List<
                                        MissionObjectiveRuntimeBinding
                                    > typeList
                                )
                        )
                        {
                            typeList =
                                new List<
                                    MissionObjectiveRuntimeBinding
                                >();


                            ObjectivesByType[
                                objective.Type
                            ] =
                                typeList;
                        }


                        typeList.Add(
                            binding
                        );


                        addedForMission++;
                    }
                }


                if (addedForMission > 0)
                {
                    missionCount++;
                }
            }


            ready =
                true;


            logger?.LogInfo(
                "[MISSION V2] RuntimeCatalog construido | " +
                $"Misiones V2: {missionCount} | " +
                $"Objetivos: {ObjectiveCount} | " +
                $"Tipos: {ObjectiveTypeCount}"
            );


            if (ObjectiveTypeCount > 0)
            {
                logger?.LogInfo(
                    "[MISSION V2] Tipos activos | " +
                    string.Join(
                        ", ",
                        ObjectivesByType.Keys
                    )
                );
            }


            Rebuilt?.Invoke();
        }


        // =========================================================
        // QUERY
        // =========================================================

        public static IReadOnlyList<
            MissionObjectiveRuntimeBinding
        > GetObjectives(
            string objectiveType
        )
        {
            if (
                !ready ||
                string.IsNullOrWhiteSpace(
                    objectiveType
                )
            )
            {
                return
                    Array.Empty<
                        MissionObjectiveRuntimeBinding
                    >();
            }


            if (
                ObjectivesByType
                    .TryGetValue(
                        objectiveType,
                        out List<
                            MissionObjectiveRuntimeBinding
                        > list
                    ) &&
                list != null
            )
            {
                return list;
            }


            return
                Array.Empty<
                    MissionObjectiveRuntimeBinding
                >();
        }


        public static bool HasObjectiveType(
            string objectiveType
        )
        {
            return
                ready &&
                !string.IsNullOrWhiteSpace(
                    objectiveType
                ) &&
                ObjectivesByType.ContainsKey(
                    objectiveType
                );
        }


        // =========================================================
        // CLEAR
        // =========================================================

        private static void Clear()
        {
            AllObjectives.Clear();

            ObjectivesByType.Clear();

            ready =
                false;
        }
    }
}
