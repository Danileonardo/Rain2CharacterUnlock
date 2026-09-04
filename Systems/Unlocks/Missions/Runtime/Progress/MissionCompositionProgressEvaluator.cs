using System;
using System.Collections.Generic;
using RoR2;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    /*
     * =============================================================
     * MISSION COMPOSITION PROGRESS EVALUATOR
     * =============================================================
     *
     * Une:
     *
     * MissionDefinition
     * MissionRoute
     * MissionObjective
     *
     * con:
     *
     * MissionProgressRegistry
     *
     * SEMÁNTICA:
     *
     * Dentro de una ruta:
     *
     *     Objective A
     *     AND
     *     Objective B
     *     AND
     *     Objective C
     *
     * Entre rutas:
     *
     *     Route A
     *     OR
     *     Route B
     *
     * ProgressScope:
     *
     *     Shared
     *     PerPlayer
     *
     * Este archivo NO escucha eventos del juego.
     *
     * Los futuros Objective Handlers serán quienes digan:
     *
     *     "suma 1 a este objetivo"
     *
     * o:
     *
     *     "establece este objetivo en 7"
     *
     * o:
     *
     *     "marca CompleteStage"
     *
     * =============================================================
     */
    public static class MissionCompositionProgressEvaluator
    {
        // =========================================================
        // ADD
        // =========================================================

        public static MissionCompositionProgressResult AddProgress(
            string missionId,
            MissionDefinition mission,
            int routeIndex,
            int objectiveIndex,
            MissionEventContext context,
            double amount
        )
        {
            if (
                !TryResolve(
                    missionId,
                    mission,
                    routeIndex,
                    objectiveIndex,
                    context,
                    out ResolvedObjective resolved
                )
            )
            {
                return
                    MissionCompositionProgressResult
                        .Rejected();
            }


            bool objectiveWasCompleted =
                MissionProgressRegistry
                    .IsObjectiveCompleted(
                        resolved.ProgressScope,
                        resolved.NetworkUser,
                        missionId,
                        resolved.StorageObjectiveId
                    );


            bool routeWasCompleted =
                IsRouteCompletedInternal(
                    missionId,
                    mission,
                    routeIndex,
                    resolved.ProgressScope,
                    resolved.NetworkUser
                );


            bool missionWasCompleted =
                MissionProgressRegistry
                    .IsMissionCompleted(
                        resolved.ProgressScope,
                        resolved.NetworkUser,
                        missionId
                    );

            double previousValue =
                MissionProgressRegistry
                    .GetProgress(
                        resolved.ProgressScope,
                        resolved.NetworkUser,
                        missionId,
                        resolved.StorageObjectiveId
                    );

            double currentValue =
                MissionProgressRegistry
                    .AddProgress(
                        resolved.ProgressScope,
                        resolved.NetworkUser,
                        missionId,
                        resolved.StorageObjectiveId,
                        amount,
                        resolved.ResetScope
                    );


            bool objectiveCompleted =
                currentValue >=
                resolved.RequiredValue;


            MissionProgressRegistry
                .SetObjectiveCompleted(
                    resolved.ProgressScope,
                    resolved.NetworkUser,
                    missionId,
                    resolved.StorageObjectiveId,
                    objectiveCompleted,
                    resolved.ResetScope
                );


            return FinalizeResult(
                missionId,
                mission,
                resolved,
                routeWasCompleted,
                missionWasCompleted,
                objectiveWasCompleted,
                previousValue,
                currentValue
            );
        }


        // =========================================================
        // SET
        // =========================================================
        //
        // Especialmente útil para valores que pueden SUBIR o BAJAR:
        //
        // ApplyStatusEffects simultáneos.
        // HoldItemStack actual.
        //
        // =========================================================

        public static MissionCompositionProgressResult SetProgress(
            string missionId,
            MissionDefinition mission,
            int routeIndex,
            int objectiveIndex,
            MissionEventContext context,
            double value
        )
        {
            if (
                !TryResolve(
                    missionId,
                    mission,
                    routeIndex,
                    objectiveIndex,
                    context,
                    out ResolvedObjective resolved
                )
            )
            {
                return
                    MissionCompositionProgressResult
                        .Rejected();
            }


            bool objectiveWasCompleted =
                MissionProgressRegistry
                    .IsObjectiveCompleted(
                        resolved.ProgressScope,
                        resolved.NetworkUser,
                        missionId,
                        resolved.StorageObjectiveId
                    );


            bool routeWasCompleted =
                IsRouteCompletedInternal(
                    missionId,
                    mission,
                    routeIndex,
                    resolved.ProgressScope,
                    resolved.NetworkUser
                );


            bool missionWasCompleted =
                MissionProgressRegistry
                    .IsMissionCompleted(
                        resolved.ProgressScope,
                        resolved.NetworkUser,
                        missionId
                    );

            double previousValue =
                MissionProgressRegistry
                    .GetProgress(
                        resolved.ProgressScope,
                        resolved.NetworkUser,
                        missionId,
                        resolved.StorageObjectiveId
                    );

            double currentValue =
                MissionProgressRegistry
                    .SetProgress(
                        resolved.ProgressScope,
                        resolved.NetworkUser,
                        missionId,
                        resolved.StorageObjectiveId,
                        value,
                        resolved.ResetScope
                    );


            bool objectiveCompleted =
                currentValue >=
                resolved.RequiredValue;


            /*
             * Aquí sí permitimos volver a FALSE.
             *
             * Ejemplo:
             *
             * HoldItemStack:
             *
             * tenía 2
             * ↓
             * pierde/consume uno
             * ↓
             * vuelve a 1
             *
             * Si la misión completa todavía NO fue concedida,
             * el objetivo deja de estar satisfecho.
             */
            MissionProgressRegistry
                .SetObjectiveCompleted(
                    resolved.ProgressScope,
                    resolved.NetworkUser,
                    missionId,
                    resolved.StorageObjectiveId,
                    objectiveCompleted,
                    resolved.ResetScope
                );


            return FinalizeResult(
                missionId,
                mission,
                resolved,
                routeWasCompleted,
                missionWasCompleted,
                objectiveWasCompleted,
                previousValue,
                currentValue
            );
        }


        // =========================================================
        // BOOLEAN / COMPLETE
        // =========================================================

        public static MissionCompositionProgressResult
            MarkObjectiveCompleted(
                string missionId,
                MissionDefinition mission,
                int routeIndex,
                int objectiveIndex,
                MissionEventContext context
            )
        {
            if (
                !TryResolve(
                    missionId,
                    mission,
                    routeIndex,
                    objectiveIndex,
                    context,
                    out ResolvedObjective resolved
                )
            )
            {
                return
                    MissionCompositionProgressResult
                        .Rejected();
            }


            bool objectiveWasCompleted =
                MissionProgressRegistry
                    .IsObjectiveCompleted(
                        resolved.ProgressScope,
                        resolved.NetworkUser,
                        missionId,
                        resolved.StorageObjectiveId
                    );


            bool routeWasCompleted =
                IsRouteCompletedInternal(
                    missionId,
                    mission,
                    routeIndex,
                    resolved.ProgressScope,
                    resolved.NetworkUser
                );


            bool missionWasCompleted =
                MissionProgressRegistry
                    .IsMissionCompleted(
                        resolved.ProgressScope,
                        resolved.NetworkUser,
                        missionId
                    );


            MissionProgressRegistry
                .SetObjectiveCompleted(
                    resolved.ProgressScope,
                    resolved.NetworkUser,
                    missionId,
                    resolved.StorageObjectiveId,
                    true,
                    resolved.ResetScope
                );

            double previousValue =
                MissionProgressRegistry
                    .GetProgress(
                        resolved.ProgressScope,
                        resolved.NetworkUser,
                        missionId,
                        resolved.StorageObjectiveId
                    );

            /*
             * Para flags booleanos dejamos Value en el target
             * para que UI/debug puedan mostrar 1/1, etc.
             */
            double currentValue =
                MissionProgressRegistry
                    .SetProgress(
                        resolved.ProgressScope,
                        resolved.NetworkUser,
                        missionId,
                        resolved.StorageObjectiveId,
                        resolved.RequiredValue,
                        resolved.ResetScope
                    );


            return FinalizeResult(
                missionId,
                mission,
                resolved,
                routeWasCompleted,
                missionWasCompleted,
                objectiveWasCompleted,
                previousValue,
                currentValue
            );
        }


        // =========================================================
        // GET
        // =========================================================

        public static double GetProgress(
            string missionId,
            MissionDefinition mission,
            int routeIndex,
            int objectiveIndex,
            MissionEventContext context
        )
        {
            if (
                !TryResolveForRead(
                    missionId,
                    mission,
                    routeIndex,
                    objectiveIndex,
                    context,
                    out ResolvedObjective resolved
                )
            )
            {
                return 0d;
            }


            return
                MissionProgressRegistry
                    .GetProgress(
                        resolved.ProgressScope,
                        resolved.NetworkUser,
                        missionId,
                        resolved.StorageObjectiveId
                    );
        }


        public static bool IsObjectiveCompleted(
            string missionId,
            MissionDefinition mission,
            int routeIndex,
            int objectiveIndex,
            MissionEventContext context
        )
        {
            if (
                !TryResolveForRead(
                    missionId,
                    mission,
                    routeIndex,
                    objectiveIndex,
                    context,
                    out ResolvedObjective resolved
                )
            )
            {
                return false;
            }


            return
                MissionProgressRegistry
                    .IsObjectiveCompleted(
                        resolved.ProgressScope,
                        resolved.NetworkUser,
                        missionId,
                        resolved.StorageObjectiveId
                    );
        }


        public static bool IsRouteCompleted(
            string missionId,
            MissionDefinition mission,
            int routeIndex,
            MissionEventContext context
        )
        {
            MissionProgressScope progressScope =
                MissionProgressKeyBuilder
                    .GetProgressScope(
                        mission
                    );


            NetworkUser networkUser =
                ResolveNetworkUser(
                    progressScope,
                    context
                );


            if (
                progressScope ==
                    MissionProgressScope.PerPlayer &&
                networkUser == null
            )
            {
                return false;
            }


            return
                IsRouteCompletedInternal(
                    missionId,
                    mission,
                    routeIndex,
                    progressScope,
                    networkUser
                );
        }


        public static bool IsMissionCompleted(
            string missionId,
            MissionDefinition mission,
            MissionEventContext context
        )
        {
            MissionProgressScope progressScope =
                MissionProgressKeyBuilder
                    .GetProgressScope(
                        mission
                    );


            NetworkUser networkUser =
                ResolveNetworkUser(
                    progressScope,
                    context
                );


            if (
                progressScope ==
                    MissionProgressScope.PerPlayer &&
                networkUser == null
            )
            {
                return false;
            }


            return
                MissionProgressRegistry
                    .IsMissionCompleted(
                        progressScope,
                        networkUser,
                        missionId
                    );
        }


        // =========================================================
        // FINALIZE
        // =========================================================

        private static MissionCompositionProgressResult FinalizeResult(
            string missionId,
            MissionDefinition mission,
            ResolvedObjective resolved,
            bool routeWasCompleted,
            bool missionWasCompleted,
            bool objectiveWasCompleted,
            double previousValue,
            double currentValue
        )
        {
            bool objectiveCompleted =
                MissionProgressRegistry
                    .IsObjectiveCompleted(
                        resolved.ProgressScope,
                        resolved.NetworkUser,
                        missionId,
                        resolved.StorageObjectiveId
                    );


            bool routeCompleted =
                IsRouteCompletedInternal(
                    missionId,
                    mission,
                    resolved.RouteIndex,
                    resolved.ProgressScope,
                    resolved.NetworkUser
                );


            bool missionCompleted =
                missionWasCompleted;


            /*
             * OR ENTRE RUTAS:
             *
             * En el instante en que ESTA ruta queda completa,
             * la misión completa queda marcada.
             */
            if (routeCompleted)
            {
                MissionProgressRegistry
                    .SetMissionCompleted(
                        resolved.ProgressScope,
                        resolved.NetworkUser,
                        missionId,
                        true
                    );


                missionCompleted =
                    true;
            }
            else
            {
                missionCompleted =
                    MissionProgressRegistry
                        .IsMissionCompleted(
                            resolved.ProgressScope,
                            resolved.NetworkUser,
                            missionId
                        );
            }


            return new MissionCompositionProgressResult
            {
                Accepted =
                    true,

                RouteIndex =
                    resolved.RouteIndex,

                ObjectiveIndex =
                    resolved.ObjectiveIndex,

                RouteId =
                    resolved.RouteId,

                ObjectiveId =
                    resolved.ObjectiveId,

                StorageObjectiveId =
                    resolved.StorageObjectiveId,

                PreviousValue =
                    previousValue,

                CurrentValue =
                    currentValue,

                RequiredValue =
                    resolved.RequiredValue,

                ObjectiveCompleted =
                    objectiveCompleted,

                ObjectiveJustCompleted =
                    !objectiveWasCompleted &&
                    objectiveCompleted,

                RouteCompleted =
                    routeCompleted,

                RouteJustCompleted =
                    !routeWasCompleted &&
                    routeCompleted,

                MissionCompleted =
                    missionCompleted,

                MissionJustCompleted =
                    !missionWasCompleted &&
                    missionCompleted,

                ProgressScope =
                    resolved.ProgressScope,

                ResetScope =
                    resolved.ResetScope
            };
        }


        // =========================================================
        // ROUTE = TODOS LOS OBJECTIVES
        // =========================================================

        private static bool IsRouteCompletedInternal(
            string missionId,
            MissionDefinition mission,
            int routeIndex,
            MissionProgressScope progressScope,
            NetworkUser networkUser
        )
        {
            if (
                string.IsNullOrWhiteSpace(
                    missionId
                ) ||
                mission == null ||
                mission.Routes == null ||
                routeIndex < 0 ||
                routeIndex >=
                    mission.Routes.Count
            )
            {
                return false;
            }


            MissionRoute route =
                mission.Routes[
                    routeIndex
                ];


            if (route == null)
            {
                return false;
            }


            IReadOnlyList<MissionObjective>
                objectives =
                    route.GetEffectiveObjectives();


            if (
                objectives == null ||
                objectives.Count == 0
            )
            {
                return false;
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


                if (objective == null)
                {
                    return false;
                }


                string storageObjectiveId =
                    MissionProgressKeyBuilder
                        .GetStorageObjectiveId(
                            route,
                            routeIndex,
                            objective,
                            objectiveIndex
                        );


                if (
                    !MissionProgressRegistry
                        .IsObjectiveCompleted(
                            progressScope,
                            networkUser,
                            missionId,
                            storageObjectiveId
                        )
                )
                {
                    return false;
                }
            }


            return true;
        }


        // =========================================================
        // RESOLVE FOR WRITE
        // =========================================================

        private static bool TryResolve(
            string missionId,
            MissionDefinition mission,
            int routeIndex,
            int objectiveIndex,
            MissionEventContext context,
            out ResolvedObjective resolved
        )
        {
            resolved =
                null;


            if (
                !MissionProgressRegistry
                    .IsRunActive
            )
            {
                return false;
            }


            if (
                !TryResolveForRead(
                    missionId,
                    mission,
                    routeIndex,
                    objectiveIndex,
                    context,
                    out resolved
                )
            )
            {
                return false;
            }


            /*
             * Las Conditions se comprueban ANTES de escribir.
             *
             * Ejemplo:
             *
             * RequiredStage = Wetland
             *
             * Si el jugador hace una acción en otro mapa:
             * el progreso ni siquiera entra al Registry.
             */
            if (
                !MissionConditionEvaluator
                    .AreSatisfied(
                        resolved.Route.Conditions,
                        context
                    )
            )
            {
                resolved =
                    null;

                return false;
            }


            /*
             * Las Conditions del Objective sólo afectan a ESTA
             * acción. Esto permite composiciones como:
             *
             * Wooper:
             *
             * Kill x5
             *   + Bite
             *   + Poison
             *
             * AND HoldItemStack x2
             * AND CompleteStage
             *
             * sin que Bite/Poison se apliquen a los otros objetivos.
             */
            if (
                !MissionConditionEvaluator
                    .AreSatisfied(
                        resolved.Objective.Conditions,
                        context
                    )
            )
            {
                resolved =
                    null;

                return false;
            }


            return true;
        }


        // =========================================================
        // RESOLVE FOR READ
        // =========================================================

        private static bool TryResolveForRead(
            string missionId,
            MissionDefinition mission,
            int routeIndex,
            int objectiveIndex,
            MissionEventContext context,
            out ResolvedObjective resolved
        )
        {
            resolved =
                null;


            if (
                string.IsNullOrWhiteSpace(
                    missionId
                ) ||
                mission == null ||
                mission.Routes == null ||
                routeIndex < 0 ||
                routeIndex >=
                    mission.Routes.Count
            )
            {
                return false;
            }


            MissionRoute route =
                mission.Routes[
                    routeIndex
                ];


            if (route == null)
            {
                return false;
            }


            IReadOnlyList<MissionObjective>
                objectives =
                    route.GetEffectiveObjectives();


            if (
                objectives == null ||
                objectiveIndex < 0 ||
                objectiveIndex >=
                    objectives.Count
            )
            {
                return false;
            }


            MissionObjective objective =
                objectives[
                    objectiveIndex
                ];


            if (objective == null)
            {
                return false;
            }


            MissionProgressScope progressScope =
                MissionProgressKeyBuilder
                    .GetProgressScope(
                        mission
                    );


            NetworkUser networkUser =
                ResolveNetworkUser(
                    progressScope,
                    context
                );


            /*
             * Shared no necesita NetworkUser.
             *
             * PerPlayer sí.
             */
            if (
                progressScope ==
                    MissionProgressScope.PerPlayer &&
                networkUser == null
            )
            {
                return false;
            }


            double requiredValue =
                objective.Amount > 0d
                    ? objective.Amount
                    : 1d;


            resolved =
                new ResolvedObjective
                {
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
                            ),

                    RequiredValue =
                        requiredValue,

                    ProgressScope =
                        progressScope,

                    ResetScope =
                        MissionProgressKeyBuilder
                            .GetResetScope(
                                objective
                            ),

                    NetworkUser =
                        networkUser
                };


            return true;
        }


        // =========================================================
        // PLAYER
        // =========================================================

        private static NetworkUser ResolveNetworkUser(
            MissionProgressScope progressScope,
            MissionEventContext context
        )
        {
            if (
                progressScope ==
                    MissionProgressScope.Shared
            )
            {
                return null;
            }


            CharacterMaster master =
                context?.PlayerMaster;


            if (
                master == null &&
                context?.PlayerBody != null
            )
            {
                master =
                    context
                        .PlayerBody
                        .master;
            }


            if (master == null)
            {
                return null;
            }


            return
                MissionPlayerIdentity
                    .GetNetworkUser(
                        master
                    );
        }


        // =========================================================
        // INTERNAL
        // =========================================================

        private sealed class ResolvedObjective
        {
            public MissionRoute Route;

            public MissionObjective Objective;

            public int RouteIndex;

            public int ObjectiveIndex;

            public string RouteId;

            public string ObjectiveId;

            public string StorageObjectiveId;

            public double RequiredValue;

            public MissionProgressScope ProgressScope;

            public MissionProgressResetScope ResetScope;

            public NetworkUser NetworkUser;
        }
    }
}
