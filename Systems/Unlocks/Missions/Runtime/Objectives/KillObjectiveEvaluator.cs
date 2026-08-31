using System;
using RoR2;


namespace UniversalSurvivorUnlocks
{
    public static class KillObjectiveEvaluator
    {
        public static bool Matches(
            MissionObjective objective,
            MissionEventContext context
        )
        {
            if (
                objective == null ||
                context == null
            )
            {
                return false;
            }


            if (
                !string.Equals(
                    objective.Type,
                    "Kill",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return false;
            }


            CharacterBody attackerBody =
                context.PlayerBody;


            CharacterBody victimBody =
                context.TargetBody;


            if (
                attackerBody == null ||
                victimBody == null
            )
            {
                return false;
            }


            MissionTarget target =
                objective.Target;


            if (target == null)
            {
                return false;
            }


            string category =
                target.Category?
                    .Trim()
                    .ToLowerInvariant()
                ?? "any";


            switch (category)
            {
                // =================================================
                // CUALQUIER OBJETIVO
                // =================================================

                case "any":
                    return true;


                // =================================================
                // ENEMIGO
                // =================================================

                case "enemy":
                case "anyenemy":
                    return IsEnemy(
                        attackerBody,
                        victimBody
                    );


                // =================================================
                // JEFE
                // =================================================

                case "boss":
                    return
                        IsEnemy(
                            attackerBody,
                            victimBody
                        ) &&
                        victimBody.isBoss;


                // =================================================
                // ÉLITE
                // =================================================

                case "elite":
                    return
                        IsEnemy(
                            attackerBody,
                            victimBody
                        ) &&
                        victimBody.isElite;


                // =================================================
                // BODY ESPECÍFICO
                // =================================================

                case "specificbody":
                    return
                        IsEnemy(
                            attackerBody,
                            victimBody
                        ) &&
                        MatchesBody(
                            victimBody,
                            target.Id
                        );

                // =================================================
                // JEFE ESPECÍFICO
                // =================================================
                //
                // Ejemplos:
                //
                // BrotherBody
                // SuperRoboBallBossBody
                // ScavBody
                //
                // Requiere:
                // - que sea enemigo;
                // - que el Body coincida;
                // - que el juego lo considere Boss.
                //
                // =================================================

                case "specificboss":
                    return
                        IsEnemy(
                            attackerBody,
                            victimBody
                        ) &&
                        victimBody.isBoss &&
                        MatchesBody(
                            victimBody,
                            target.Id
                        );


                default:
                    return false;
            }
        }


        private static bool IsEnemy(
            CharacterBody attacker,
            CharacterBody victim
        )
        {
            if (
                attacker?.teamComponent == null ||
                victim?.teamComponent == null
            )
            {
                return false;
            }


            TeamIndex attackerTeam =
                attacker.teamComponent.teamIndex;


            TeamIndex victimTeam =
                victim.teamComponent.teamIndex;


            TeamMask enemyTeams =
                TeamMask.GetEnemyTeams(
                    attackerTeam
                );


            return enemyTeams.HasTeam(
                victimTeam
            );
        }


        private static bool MatchesBody(
            CharacterBody body,
            string requiredBody
        )
        {
            if (
                body == null ||
                string.IsNullOrWhiteSpace(
                    requiredBody
                )
            )
            {
                return false;
            }


            string currentBody =
                BodyCatalog.GetBodyName(
                    body.bodyIndex
                );


            return string.Equals(
                currentBody,
                requiredBody,
                StringComparison.OrdinalIgnoreCase
            );
        }
    }
}