using System;
using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    /*
     * =============================================================
     * IDS Y SCOPES PARA MISSION PROGRESS
     * =============================================================
     *
     * El registro de progreso guarda:
     *
     * MissionId
     *     └── ObjectiveId
     *
     * Como una misión ahora puede tener múltiples rutas OR,
     * el ObjectiveId de almacenamiento incluye también la ruta:
     *
     *     route_id::objective_id
     *
     * Así:
     *
     * RailgunnerRoute::streak
     *
     * y:
     *
     * BanditRoute::streak
     *
     * nunca comparten accidentalmente el mismo contador.
     * =============================================================
     */
    public static class MissionProgressKeyBuilder
    {
        public static string GetRouteId(
            MissionRoute route,
            int routeIndex
        )
        {
            if (
                route != null &&
                !string.IsNullOrWhiteSpace(
                    route.Id
                )
            )
            {
                return route.Id.Trim();
            }


            /*
             * Sólo para presets legacy sin ID.
             *
             * Los presets nuevos/editor deben guardar siempre
             * un ID estable real.
             */
            return
                "legacy_route_" +
                Math.Max(
                    0,
                    routeIndex
                );
        }


        public static string GetObjectiveId(
            MissionObjective objective,
            int objectiveIndex
        )
        {
            if (
                objective != null &&
                !string.IsNullOrWhiteSpace(
                    objective.Id
                )
            )
            {
                return objective.Id.Trim();
            }


            return
                "legacy_objective_" +
                Math.Max(
                    0,
                    objectiveIndex
                );
        }


        public static string GetStorageObjectiveId(
            MissionRoute route,
            int routeIndex,
            MissionObjective objective,
            int objectiveIndex
        )
        {
            return
                GetRouteId(
                    route,
                    routeIndex
                ) +
                "::" +
                GetObjectiveId(
                    objective,
                    objectiveIndex
                );
        }


        // =========================================================
        // PROGRESS SCOPE
        // =========================================================

        public static MissionProgressScope GetProgressScope(
            MissionDefinition mission
        )
        {
            if (
                mission != null &&
                string.Equals(
                    mission.ProgressScope,
                    "Shared",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return
                    MissionProgressScope.Shared;
            }


            return
                MissionProgressScope.PerPlayer;
        }


        // =========================================================
        // RESET SCOPE
        // =========================================================

        public static MissionProgressResetScope GetResetScope(
            MissionObjective objective
        )
        {
            if (objective == null)
            {
                return
                    MissionProgressResetScope.Run;
            }


            if (
                string.Equals(
                    objective.ResetScope,
                    "Stage",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return
                    MissionProgressResetScope.Stage;
            }


            /*
             * Compatibilidad adicional:
             *
             * Durante prototipos anteriores podía resultar cómodo
             * guardar resetScope dentro de Parameters.
             *
             * Si existe, también lo respetamos.
             */
            JObject parameters =
                objective.Parameters;


            if (parameters != null)
            {
                string parameterScope =
                    parameters.Value<string>(
                        "resetScope"
                    );


                if (
                    string.Equals(
                        parameterScope,
                        "Stage",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    return
                        MissionProgressResetScope.Stage;
                }
            }


            return
                MissionProgressResetScope.Run;
        }
    }
}
