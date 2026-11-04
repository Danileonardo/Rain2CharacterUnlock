using System;
using System.Collections.Generic;
using RoR2;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Lectura de progreso para Browser/HUD futuro. Sin escritura ni red.
    /// Los datos son una fotografía de la run AUTORITATIVA en el host;
    /// un HUD de clientes requerirá transporte/sincronización posterior.
    /// No sustituye el AH64CampaignProgressStore persistente.
    /// </summary>
    public sealed class MissionObjectiveCaptureSnapshot
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string ResetScope { get; set; }
        public double Current { get; set; }
        public double Required { get; set; }
        public bool Completed { get; set; }
        public bool PrerequisitesSatisfied { get; set; }
        public List<string> AfterObjectiveIds { get; set; } = new List<string>();
    }

    public sealed class MissionRouteCaptureSnapshot
    {
        public string RouteId { get; set; }
        public List<MissionObjectiveCaptureSnapshot> Objectives { get; set; }
            = new List<MissionObjectiveCaptureSnapshot>();
        public int RemainingObjectives { get; set; }
        public bool Completed { get; set; }
        public bool LastObjectivePending { get { return RemainingObjectives == 1; } }
        public string LastPendingObjectiveId { get; set; }
    }

    public sealed class MissionCaptureSnapshot
    {
        public string MissionId { get; set; }
        public MissionProgressScope Scope { get; set; }
        public bool Completed { get; set; }
        public List<MissionRouteCaptureSnapshot> Routes { get; set; }
            = new List<MissionRouteCaptureSnapshot>();
    }

    public static class MissionProgressSnapshotService
    {
        /// <summary>
        /// Devuelve null fuera de una run del host o sin identidad PerPlayer.
        /// No crea estados de progreso ni altera misiones.
        /// El último objetivo se calcula por ruta; una misión con rutas OR
        /// podrá elegir en la UI la ruta que el jugador esté siguiendo.
        /// </summary>
        public static MissionCaptureSnapshot Capture(
            string missionId,
            MissionDefinition mission,
            MissionEventContext context)
        {
            if (!NetworkServer.active || !MissionProgressRegistry.IsRunActive ||
                string.IsNullOrWhiteSpace(missionId) || mission?.Routes == null)
                return null;

            MissionProgressScope scope = MissionProgressKeyBuilder.GetProgressScope(mission);
            NetworkUser networkUser = null;
            if (scope == MissionProgressScope.PerPlayer)
            {
                CharacterMaster master = context?.PlayerMaster ?? context?.PlayerBody?.master;
                networkUser = master == null ? null : MissionPlayerIdentity.GetNetworkUser(master);
                if (networkUser == null) return null;
            }

            var snapshot = new MissionCaptureSnapshot
            {
                MissionId = missionId,
                Scope = scope,
                Completed = MissionProgressRegistry.IsMissionCompleted(
                    scope, networkUser, missionId)
            };

            for (int routeIndex = 0; routeIndex < mission.Routes.Count; routeIndex++)
            {
                MissionRoute route = mission.Routes[routeIndex];
                if (route == null) continue;
                IReadOnlyList<MissionObjective> objectives = route.GetEffectiveObjectives();
                var routeView = new MissionRouteCaptureSnapshot
                {
                    RouteId = MissionProgressKeyBuilder.GetRouteId(route, routeIndex)
                };
                for (int i = 0; i < objectives.Count; i++)
                {
                    MissionObjective objective = objectives[i];
                    if (objective == null) continue;
                    string storageId = MissionProgressKeyBuilder.GetStorageObjectiveId(
                        route, routeIndex, objective, i);
                    bool completed = MissionProgressRegistry.IsObjectiveCompleted(
                        scope, networkUser, missionId, storageId);
                    routeView.Objectives.Add(new MissionObjectiveCaptureSnapshot
                    {
                        Id = MissionProgressKeyBuilder.GetObjectiveId(objective, i),
                        Type = objective.Type,
                        ResetScope = objective.ResetScope,
                        Current = MissionProgressRegistry.GetProgress(
                            scope, networkUser, missionId, storageId),
                        Required = objective.Amount > 0d ? objective.Amount : 1d,
                        Completed = completed,
                        PrerequisitesSatisfied = MissionObjectivePrerequisiteEvaluator.AreSatisfied(
                            missionId, route, routeIndex, objective, scope, networkUser),
                        AfterObjectiveIds = MissionObjectivePrerequisiteEvaluator.GetRequiredObjectiveIds(objective)
                    });
                    if (!completed)
                    {
                        routeView.RemainingObjectives++;
                        routeView.LastPendingObjectiveId =
                            MissionProgressKeyBuilder.GetObjectiveId(objective, i);
                    }
                }
                routeView.Completed = routeView.Objectives.Count > 0 &&
                    routeView.RemainingObjectives == 0;
                if (routeView.RemainingObjectives != 1)
                    routeView.LastPendingObjectiveId = null;
                snapshot.Routes.Add(routeView);
            }
            return snapshot;
        }
    }
}
