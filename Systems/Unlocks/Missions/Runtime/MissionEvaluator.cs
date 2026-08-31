using System;


namespace UniversalSurvivorUnlocks
{
    public static class MissionEvaluator
    {
        // =========================================================
        // EVALUAR UNA ACCIÓN
        // =========================================================
        //
        // Devuelve TRUE cuando:
        //
        // 1. El evento corresponde al Objective.
        // 2. El Target es correcto.
        // 3. Todas las Conditions son válidas.
        //
        // TODAVÍA NO suma progreso.
        //
        // Esa responsabilidad irá en MissionProgressManager.
        // =========================================================

        public static bool MatchesEvent(
            MissionRoute route,
            MissionEventContext context
        )
        {
            if (
                route == null ||
                route.Objective == null
            )
            {
                return false;
            }


            bool objectiveMatched =
                MatchesObjective(
                    route.Objective,
                    context
                );


            if (!objectiveMatched)
            {
                return false;
            }


            return
                MissionConditionEvaluator
                    .AreSatisfied(
                        route.Conditions,
                        context
                    );
        }


        private static bool MatchesObjective(
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


                default:
                    return false;
            }
        }
    }
}