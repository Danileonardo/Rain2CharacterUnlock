using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Requisitos previos universales para objetivos de la MISMA ruta.
    /// Compatible con afterObjectiveId (presets existentes) y
    /// afterObjectiveIds (lista AND para el futuro editor CUSTOM).
    /// No cambia las recompensas, los IDs ni el estado de progreso.
    /// </summary>
    public static class MissionObjectivePrerequisiteEvaluator
    {
        public static List<string> GetRequiredObjectiveIds(MissionObjective objective)
        {
            var ids = new List<string>();
            if (objective == null || objective.Parameters == null)
                return ids;

            AddId(ids, objective.Parameters.Value<string>("afterObjectiveId"));

            JToken multiple = objective.Parameters["afterObjectiveIds"];
            if (multiple is JArray array)
            {
                foreach (JToken entry in array)
                {
                    if (entry != null && entry.Type == JTokenType.String)
                        AddId(ids, entry.Value<string>());
                }
            }

            return ids;
        }

        private static void AddId(List<string> ids, string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return;
            id = id.Trim();
            for (int i = 0; i < ids.Count; i++)
                if (string.Equals(ids[i], id, StringComparison.OrdinalIgnoreCase))
                    return;
            ids.Add(id);
        }

        /// <summary>
        /// true cuando no hay dependencias, o cuando TODAS se han completado.
        /// Si se declara un ID inexistente, falla cerrado: no se concede progreso.
        /// Los objetivos que pueden perderse (p. ej. HoldItemStack) se consultan
        /// en tiempo real mediante IsObjectiveCompleted.
        /// </summary>
        public static bool AreSatisfied(
            string missionId,
            MissionRoute route,
            int routeIndex,
            MissionObjective objective,
            MissionProgressScope progressScope,
            NetworkUser networkUser)
        {
            if (objective == null) return false;
            List<string> prerequisites = GetRequiredObjectiveIds(objective);
            if (prerequisites.Count == 0) return true;
            if (string.IsNullOrWhiteSpace(missionId) || route == null)
                return false;
            if (progressScope == MissionProgressScope.PerPlayer && networkUser == null)
                return false;

            IReadOnlyList<MissionObjective> objectives = route.GetEffectiveObjectives();
            if (objectives == null || objectives.Count == 0) return false;

            foreach (string requiredId in prerequisites)
            {
                bool found = false;
                for (int i = 0; i < objectives.Count; i++)
                {
                    MissionObjective candidate = objectives[i];
                    if (candidate == null || ReferenceEquals(candidate, objective) ||
                        !string.Equals(candidate.Id, requiredId, StringComparison.OrdinalIgnoreCase))
                        continue;

                    found = true;
                    string storageId = MissionProgressKeyBuilder.GetStorageObjectiveId(
                        route, routeIndex, candidate, i);
                    if (!MissionProgressRegistry.IsObjectiveCompleted(
                        progressScope, networkUser, missionId, storageId))
                        return false;
                    break;
                }
                if (!found) return false;
            }
            return true;
        }
    }
}
