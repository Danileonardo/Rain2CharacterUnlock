using System;
using System.Collections.Generic;

namespace UniversalSurvivorUnlocks
{
    public static class MissionEvaluator
    {
        // =========================================================
        // COMPATIBILIDAD: ¿ALGÚN OBJETIVO DE ESTA RUTA COINCIDE?
        // =========================================================
        //
        // Mantiene el contrato antiguo:
        //
        // MatchesEvent(route, context)
        //
        // pero ahora una ruta puede contener varios Objectives.
        //
        // Devuelve TRUE si:
        //
        // 1. todas las Conditions de la ruta son válidas,
        // 2. al menos un Objective corresponde al evento.
        //
        // NO significa que la ruta completa esté terminada.
        //
        // La finalización real será:
        //
        // TODOS los Objectives de la ruta completados = Route DONE
        //
        // y:
        //
        // CUALQUIER Route DONE = Mission DONE
        // =========================================================

        public static bool MatchesEvent(
            MissionRoute route,
            MissionEventContext context
        )
        {
            return TryGetFirstMatchingObjective(
                route,
                context,
                out _,
                out _
            );
        }


        // =========================================================
        // PRIMER OBJETIVO DE LA RUTA QUE COINCIDE
        // =========================================================

        public static bool TryGetFirstMatchingObjective(
            MissionRoute route,
            MissionEventContext context,
            out int objectiveIndex,
            out MissionObjective objective
        )
        {
            objectiveIndex =
                -1;

            objective =
                null;


            if (route == null)
            {
                return false;
            }


            if (
                !MissionConditionEvaluator
                    .AreSatisfied(
                        route.Conditions,
                        context
                    )
            )
            {
                return false;
            }


            IReadOnlyList<MissionObjective>
                objectives =
                    route.GetEffectiveObjectives();


            for (int i = 0; i < objectives.Count; i++)
            {
                MissionObjective candidate =
                    objectives[i];


                if (
                    !MatchesObjective(
                        candidate,
                        context
                    )
                )
                {
                    continue;
                }


                objectiveIndex =
                    i;

                objective =
                    candidate;


                return true;
            }


            return false;
        }


        // =========================================================
        // ¿ALGUNA RUTA DE LA MISIÓN ACEPTA EL EVENTO?
        // =========================================================
        //
        // Esto implementa la parte estructural:
        //
        // Route 1
        // OR
        // Route 2
        // OR
        // Route 3
        //
        // IMPORTANTE:
        //
        // Este método indica que el evento puede alimentar una ruta.
        // NO declara la misión completada por sí solo.
        // =========================================================

        public static bool MatchesAnyRoute(
            MissionDefinition mission,
            MissionEventContext context,
            out int routeIndex,
            out int objectiveIndex,
            out MissionObjective objective
        )
        {
            routeIndex =
                -1;

            objectiveIndex =
                -1;

            objective =
                null;


            if (
                mission == null ||
                mission.Routes == null
            )
            {
                return false;
            }


            for (
                int currentRouteIndex = 0;
                currentRouteIndex < mission.Routes.Count;
                currentRouteIndex++
            )
            {
                MissionRoute route =
                    mission.Routes[
                        currentRouteIndex
                    ];


                if (
                    !TryGetFirstMatchingObjective(
                        route,
                        context,
                        out int matchedObjectiveIndex,
                        out MissionObjective matchedObjective
                    )
                )
                {
                    continue;
                }


                routeIndex =
                    currentRouteIndex;

                objectiveIndex =
                    matchedObjectiveIndex;

                objective =
                    matchedObjective;


                return true;
            }


            return false;
        }


        // =========================================================
        // OBJETIVO INDIVIDUAL
        // =========================================================
        //
        // Se hace público para que el futuro router de progreso
        // pueda recorrer todos los objetivos sin crear listas
        // temporales en eventos frecuentes.
        // =========================================================

        public static bool MatchesObjective(
            MissionObjective objective,
            MissionEventContext context
        )
        {
            if (
                objective == null ||
                string.IsNullOrWhiteSpace(
                    objective.Type
                )
            )
            {
                return false;
            }


            switch (
                objective.Type
                    .Trim()
                    .ToLowerInvariant()
            )
            {
                case "kill":
                    return
                        KillObjectiveEvaluator
                            .Matches(
                                objective,
                                context
                            );


                /*
                 * Los próximos evaluadores entrarán aquí:
                 *
                 * ApplyStatusEffects
                 * HealHealth
                 * HoldItemStack
                 * CompleteStage
                 * KillWithSkillWhileStatus
                 *
                 * Cada tipo tendrá su evaluator genérico.
                 */


                default:
                    return false;
            }
        }
    }
}
