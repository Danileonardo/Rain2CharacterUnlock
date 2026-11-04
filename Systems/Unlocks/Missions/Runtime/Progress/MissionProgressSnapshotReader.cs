using System;
using System.Collections.Generic;
using RoR2;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Lectura inmutable en el host para futuras presentaciones (Browser/HUD).
    /// No envia paquetes y NO confirma un desbloqueo; eso sigue en Rewards.
    /// </summary>
    public sealed class MissionObjectiveProgressSnapshot
    {
        public string ObjectiveId { get; private set; }
        public string Type { get; private set; }
        public double Value { get; private set; }
        public double RequiredValue { get; private set; }
        public bool Completed { get; private set; }
        public bool PrerequisitesMet { get; private set; }
        public MissionProgressResetScope ResetScope { get; private set; }

        internal MissionObjectiveProgressSnapshot(string id, string type,
            double value, double required, bool completed, bool prerequisitesMet,
            MissionProgressResetScope resetScope)
        {
            ObjectiveId = id;
            Type = type;
            Value = value;
            RequiredValue = required;
            Completed = completed;
            PrerequisitesMet = prerequisitesMet;
            ResetScope = resetScope;
        }
    }

    public sealed class MissionRouteProgressSnapshot
    {
        public string RouteId { get; private set; }
        public IReadOnlyList<MissionObjectiveProgressSnapshot> Objectives { get; private set; }
        public int RemainingObjectives { get; private set; }
        public bool Completed { get { return Objectives.Count > 0 && RemainingObjectives == 0; } }
        public string LastPendingObjectiveId { get; private set; }

        internal MissionRouteProgressSnapshot(string routeId,
            List<MissionObjectiveProgressSnapshot> objectives, int remaining,
            string lastPendingId)
        {
            RouteId = routeId;
            Objectives = objectives.AsReadOnly();
            RemainingObjectives = remaining;
            LastPendingObjectiveId = remaining == 1 ? lastPendingId : "";
        }
    }

    public sealed class MissionProgressSnapshot
    {
        public string MissionId { get; private set; }
        public MissionProgressScope Scope { get; private set; }
        public bool MissionCompleted { get; private set; }
        public IReadOnlyList<MissionRouteProgressSnapshot> Routes { get; private set; }
        // OR entre rutas: puede haber varias rutas incompletas, solo una
        // ruta necesita llegar a su ultimo objetivo para ser prioritaria.
        public string PriorityRouteId { get; private set; }
        public string PriorityObjectiveId { get; private set; }
        public bool HasOneRemainingObjective { get { return !string.IsNullOrEmpty(PriorityObjectiveId); } }

        internal MissionProgressSnapshot(string id, MissionProgressScope scope,
            bool completed, List<MissionRouteProgressSnapshot> routes,
            string priorityRouteId, string priorityObjectiveId)
        {
            MissionId = id;
            Scope = scope;
            MissionCompleted = completed;
            Routes = routes.AsReadOnly();
            PriorityRouteId = completed ? "" : priorityRouteId;
            PriorityObjectiveId = completed ? "" : priorityObjectiveId;
        }
    }

    public static class MissionProgressSnapshotReader
    {
        /// <summary>
        /// Solo lectura en servidor, sin escritura, eventos ni polling.
        /// Para PerPlayer exige usuario valido; para Shared usa null.
        /// El HUD cliente necesitara luego un transporte de snapshots
        /// incremental y controlado desde el host.
        /// </summary>
        public static MissionProgressSnapshot Read(
            string missionId, MissionDefinition mission, NetworkUser networkUser = null)
        {
            if (!NetworkServer.active || !MissionProgressRegistry.IsRunActive ||
                string.IsNullOrWhiteSpace(missionId) || mission?.Routes == null)
                return null;

            MissionProgressScope scope = MissionProgressKeyBuilder.GetProgressScope(mission);
            if (scope == MissionProgressScope.PerPlayer && networkUser == null)
                return null;
            NetworkUser owner = scope == MissionProgressScope.Shared ? null : networkUser;
            bool completed = MissionProgressRegistry.IsMissionCompleted(scope, owner, missionId);
            var routes = new List<MissionRouteProgressSnapshot>();
            string priorityRoute = "";
            string priorityObjective = "";

            for (int routeIndex = 0; routeIndex < mission.Routes.Count; routeIndex++)
            {
                MissionRoute route = mission.Routes[routeIndex];
                if (route == null) continue;
                IReadOnlyList<MissionObjective> objectives = route.GetEffectiveObjectives();
                var values = new List<MissionObjectiveProgressSnapshot>();
                int remaining = 0;
                string lastPending = "";

                for (int objectiveIndex = 0; objectiveIndex < objectives.Count; objectiveIndex++)
                {
                    MissionObjective objective = objectives[objectiveIndex];
                    if (objective == null) continue;
                    string id = MissionProgressKeyBuilder.GetObjectiveId(objective, objectiveIndex);
                    string storage = MissionProgressKeyBuilder.GetStorageObjectiveId(
                        route, routeIndex, objective, objectiveIndex);
                    double value = MissionProgressRegistry.GetProgress(scope, owner, missionId, storage);
                    bool isComplete = MissionProgressRegistry.IsObjectiveCompleted(
                        scope, owner, missionId, storage);
                    bool ready = MissionObjectivePrerequisiteEvaluator.AreSatisfied(
                        missionId, route, routeIndex, objective, scope, owner);
                    double required = objective.Amount > 0 ? objective.Amount : 1d;
                    values.Add(new MissionObjectiveProgressSnapshot(id, objective.Type,
                        value, required, isComplete, ready,
                        MissionProgressKeyBuilder.GetResetScope(objective)));
                    if (!isComplete)
                    {
                        remaining++;
                        lastPending = id;
                    }
                }

                string routeId = MissionProgressKeyBuilder.GetRouteId(route, routeIndex);
                routes.Add(new MissionRouteProgressSnapshot(
                    routeId, values, remaining, lastPending));

                if (!completed && remaining == 1 && values.Count > 0 &&
                    string.IsNullOrEmpty(priorityObjective))
                {
                    priorityRoute = routeId;
                    priorityObjective = lastPending;
                }
            }

            return new MissionProgressSnapshot(missionId, scope,
                completed, routes, priorityRoute, priorityObjective);
        }
    }
}
