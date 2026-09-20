using System;

using Newtonsoft.Json.Linq;

using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Condición OR genérica para una lista de pares:
    /// survivor + slot + skill.
    ///
    /// Permite que una sola ruta Mission V2 comparta el mismo contador entre
    /// distintos performers compatibles. G8 la utiliza para Execution, pero el
    /// formato no depende de Gwen ni de ningún mod concreto.
    /// </summary>
    public static class MissionAllowedPerformerSkillEvaluator
    {
        public static bool IsSatisfied(
            JObject parameters,
            MissionEventContext context
        )
        {
            if (
                parameters == null ||
                context == null ||
                context.PlayerBody == null
            )
            {
                return false;
            }

            JArray options =
                parameters["options"] as JArray;

            if (
                options == null ||
                options.Count == 0
            )
            {
                return false;
            }

            string currentBody =
                BodyCatalog.GetBodyName(
                    context.PlayerBody.bodyIndex
                );

            if (string.IsNullOrWhiteSpace(currentBody))
            {
                return false;
            }

            for (int i = 0; i < options.Count; i++)
            {
                JObject option =
                    options[i] as JObject;

                if (option == null)
                {
                    continue;
                }

                string requiredBody =
                    option.Value<string>("body")?.Trim() ?? "";

                if (
                    string.IsNullOrWhiteSpace(requiredBody) ||
                    !string.Equals(
                        currentBody,
                        requiredBody,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    continue;
                }

                if (
                    MissionRequiredSkillEvaluator.IsSatisfied(
                        option,
                        context
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
