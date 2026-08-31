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


        private static bool IsSatisfied(
            MissionCondition condition,
            MissionEventContext context
        )
        {
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
                    return !IsAirborne(
                        context?.PlayerBody
                    );


                // =================================================
                // REQUIRED SURVIVOR
                // =================================================

                case "requiredsurvivor":
                    return IsRequiredSurvivor(
                        condition.Parameters,
                        context?.PlayerBody
                    );


                // =================================================
                // SIN CONDICIÓN CONOCIDA
                // =================================================
                //
                // IMPORTANTE:
                //
                // Una condición desconocida NO debe considerarse
                // automáticamente válida.
                //
                // De lo contrario una misión mal configurada podría
                // completarse ignorando parte de sus requisitos.
                // =================================================

                default:
                    return false;
            }
        }


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


            string requiredBody =
                parameters.Value<string>(
                    "body"
                );


            if (
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