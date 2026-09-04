using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    public static class MissionConditionEvaluator
    {
        public static bool AreSatisfied(
            IReadOnlyList<MissionCondition> conditions,
            MissionEventContext context
        )
        {
            if (conditions == null)
            {
                return true;
            }


            for (int i = 0; i < conditions.Count; i++)
            {
                MissionCondition condition =
                    conditions[i];


                if (
                    condition == null ||
                    string.IsNullOrWhiteSpace(
                        condition.Type
                    )
                )
                {
                    continue;
                }


                if (
                    !IsSatisfied(
                        condition,
                        context
                    )
                )
                {
                    return false;
                }
            }


            return true;
        }


        public static bool IsSatisfied(
            MissionCondition condition,
            MissionEventContext context
        )
        {
            if (
                condition == null ||
                string.IsNullOrWhiteSpace(
                    condition.Type
                )
            )
            {
                return false;
            }


            string type =
                condition.Type
                    .Trim()
                    .ToLowerInvariant();


            switch (type)
            {
                // =================================================
                // AIRBORNE
                // =================================================
                case "airborne":
                    return IsAirborne(
                        context?.PlayerBody
                    );


                // =================================================
                // GROUNDED
                // =================================================
                case "grounded":
                    return IsGrounded(
                        context?.PlayerBody
                    );


                // =================================================
                // REQUIRED SURVIVOR
                // =================================================
                //
                // Compatibilidad:
                //
                // ANTIGUO:
                //
                // {
                //     "body": "CrocoBody"
                // }
                //
                // NUEVO - OR:
                //
                // {
                //     "bodies": [
                //         "CrocoBody",
                //         "TreebotBody"
                //     ]
                // }
                //
                // También se permite conservar "body" junto con
                // "bodies"; cualquier coincidencia es válida.
                // =================================================
                case "requiredsurvivor":
                    return IsRequiredSurvivor(
                        condition.Parameters,
                        context?.PlayerBody
                    );


                // =================================================
                // REQUIRED STAGE
                // =================================================
                //
                // ANTIGUO / SIMPLE:
                //
                // {
                //     "stage": "foggyswamp"
                // }
                //
                // NUEVO - OR:
                //
                // {
                //     "stages": [
                //         "foggyswamp",
                //         "goolake"
                //     ]
                // }
                //
                // =================================================
                case "requiredstage":
                    return IsRequiredStage(
                        condition.Parameters
                    );


                // =================================================
                // REQUIRED SKILL
                // =================================================
                case "requiredskill":
                    return MissionRequiredSkillEvaluator
                        .IsSatisfied(
                            condition.Parameters,
                            context
                        );


                // =================================================
                // STATUS PRESENT
                // =================================================
                case "statuspresent":
                    return MissionStatusPresentEvaluator
                        .IsSatisfied(
                            condition.Parameters,
                            context
                        );


                // =================================================
                // HIT / DAMAGE CONDITIONS
                // =================================================
                case "criticalhit":
                    return MissionCombatConditionEvaluator
                        .IsCriticalHit(
                            context
                        );


                case "fatalhit":
                    return MissionCombatConditionEvaluator
                        .IsFatalHit(
                            context
                        );


                case "minimumdamage":
                    return MissionCombatConditionEvaluator
                        .HasMinimumDamage(
                            condition.Parameters,
                            context
                        );


                case "damagetype":
                    return MissionCombatConditionEvaluator
                        .MatchesDamageType(
                            condition.Parameters,
                            context
                        );


                case "backstab":
                    return MissionCombatConditionEvaluator
                        .IsBackstab(
                            context
                        );


                // =================================================
                // ADVANCED / COMPOSITION CONDITIONS
                // =================================================
                case "timelimit":
                    return MissionAdvancedConditionEvaluator
                        .IsTimeLimitSatisfied(condition.Parameters);

                case "stagesequence":
                    return MissionAdvancedConditionEvaluator
                        .IsStageSequenceSatisfied(condition.Parameters);

                case "noitempickup":
                    return MissionAdvancedConditionEvaluator
                        .HasNoItemPickup(context);

                case "priorskillused":
                    return MissionAdvancedConditionEvaluator
                        .WasPriorSkillUsed(condition.Parameters, context);

                case "partyhassurvivor":
                    return MissionAdvancedConditionEvaluator
                        .PartyHasSurvivor(condition.Parameters);

                case "minionsalive":
                    return MissionAdvancedConditionEvaluator
                        .HasRequiredMinionsAlive(condition.Parameters, context);


                // =================================================
                // DESCONOCIDA
                // =================================================
                //
                // Nunca considerar una condición desconocida válida.
                // =================================================
                default:
                    return false;
            }
        }


        // =========================================================
        // AIRBORNE
        // =========================================================

        private static bool IsAirborne(
            CharacterBody body
        )
        {
            if (body == null)
            {
                return false;
            }


            CharacterMotor motor =
                body.characterMotor;


            if (motor == null)
            {
                return false;
            }


            return !motor.isGrounded;
        }


        private static bool IsGrounded(
            CharacterBody body
        )
        {
            if (
                body == null ||
                body.characterMotor == null
            )
            {
                return false;
            }


            return body.characterMotor.isGrounded;
        }


        // =========================================================
        // REQUIRED SURVIVOR
        // =========================================================

        private static bool IsRequiredSurvivor(
            JObject parameters,
            CharacterBody body
        )
        {
            if (
                parameters == null ||
                body == null
            )
            {
                return false;
            }


            string currentBody =
                BodyCatalog.GetBodyName(
                    body.bodyIndex
                );


            if (
                string.IsNullOrWhiteSpace(
                    currentBody
                )
            )
            {
                return false;
            }


            return MatchesAnyConfiguredValue(
                parameters,
                "body",
                "bodies",
                currentBody
            );
        }


        // =========================================================
        // REQUIRED STAGE
        // =========================================================

        private static bool IsRequiredStage(
            JObject parameters
        )
        {
            if (parameters == null)
            {
                return false;
            }


            string currentStage =
                MissionStageRuntimeTracker
                    .CurrentStageName;


            if (
                string.IsNullOrWhiteSpace(
                    currentStage
                )
            )
            {
                return false;
            }


            return MatchesAnyConfiguredValue(
                parameters,
                "stage",
                "stages",
                currentStage
            );
        }


        // =========================================================
        // ANY OF
        // =========================================================
        //
        // Se reutiliza para:
        //
        // body / bodies
        // stage / stages
        //
        // La semántica es:
        //
        // A OR B OR C
        //
        // =========================================================

        private static bool MatchesAnyConfiguredValue(
            JObject parameters,
            string singleProperty,
            string multipleProperty,
            string currentValue
        )
        {
            if (
                parameters == null ||
                string.IsNullOrWhiteSpace(
                    currentValue
                )
            )
            {
                return false;
            }


            // -----------------------------------------------------
            // VALOR ÚNICO LEGACY
            // -----------------------------------------------------

            string singleValue =
                parameters.Value<string>(
                    singleProperty
                );


            if (
                !string.IsNullOrWhiteSpace(
                    singleValue
                ) &&
                string.Equals(
                    currentValue,
                    singleValue,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return true;
            }


            // -----------------------------------------------------
            // LISTA ANY OF
            // -----------------------------------------------------

            JToken multipleToken =
                parameters[
                    multipleProperty
                ];


            JArray values =
                multipleToken as JArray;


            if (values == null)
            {
                return false;
            }


            for (int i = 0; i < values.Count; i++)
            {
                string candidate =
                    values[i]?
                        .ToString()
                        .Trim();


                if (
                    string.IsNullOrWhiteSpace(
                        candidate
                    )
                )
                {
                    continue;
                }


                if (
                    string.Equals(
                        currentValue,
                        candidate,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    return true;
                }
            }


            return false;
        }
    }
}
