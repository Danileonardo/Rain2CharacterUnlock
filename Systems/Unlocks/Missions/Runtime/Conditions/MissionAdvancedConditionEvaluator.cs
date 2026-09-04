using System;
using Newtonsoft.Json.Linq;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    public static class MissionAdvancedConditionEvaluator
    {
        public static bool IsTimeLimitSatisfied(JObject parameters)
        {
            if (Run.instance == null || parameters == null) return false;

            double seconds = parameters.Value<double?>("seconds") ?? 0d;
            if (seconds <= 0d) return false;

            return Run.instance.GetRunStopwatch() <= seconds;
        }

        public static bool IsStageSequenceSatisfied(JObject parameters)
        {
            if (parameters == null) return false;

            int current = MissionStageRuntimeTracker.CurrentStageSequence;
            int exact = parameters.Value<int?>("sequence") ?? 0;
            int min = parameters.Value<int?>("min") ?? 0;
            int max = parameters.Value<int?>("max") ?? 0;

            if (exact > 0) return current == exact;
            if (min > 0 && current < min) return false;
            if (max > 0 && current > max) return false;

            return min > 0 || max > 0;
        }

        public static bool HasNoItemPickup(MissionEventContext context)
        {
            CharacterMaster master = context?.PlayerMaster ?? context?.PlayerBody?.master;
            return master != null && !PickupItemObjectiveHandler.HasPickedAnyItem(master);
        }

        public static bool WasPriorSkillUsed(JObject parameters, MissionEventContext context)
        {
            CharacterMaster master = context?.PlayerMaster ?? context?.PlayerBody?.master;
            if (master == null || parameters == null) return false;

            string slot = parameters.Value<string>("slot") ?? "";
            string token = parameters.Value<string>("skillToken") ?? "";
            float within = parameters.Value<float?>("withinSeconds") ?? 8f;

            return MissionSkillHistoryTracker.WasUsedRecently(master, slot, token, within);
        }

        public static bool PartyHasSurvivor(JObject parameters)
        {
            if (parameters == null) return false;

            JArray bodies = parameters["bodies"] as JArray;
            string single = parameters.Value<string>("body") ?? "";

            foreach (PlayerCharacterMasterController controller in PlayerCharacterMasterController.instances)
            {
                CharacterBody body = controller?.master?.GetBody();
                if (body == null) continue;

                string bodyName = BodyCatalog.GetBodyName(body.bodyIndex);
                if (string.IsNullOrWhiteSpace(bodyName)) continue;

                if (
                    !string.IsNullOrWhiteSpace(single) &&
                    string.Equals(bodyName, single.Trim(), StringComparison.OrdinalIgnoreCase)
                )
                {
                    return true;
                }

                if (bodies != null)
                {
                    for (int i = 0; i < bodies.Count; i++)
                    {
                        string allowed = bodies[i]?.ToString();
                        if (
                            !string.IsNullOrWhiteSpace(allowed) &&
                            string.Equals(bodyName, allowed.Trim(), StringComparison.OrdinalIgnoreCase)
                        )
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static bool HasRequiredMinionsAlive(JObject parameters, MissionEventContext context)
        {
            CharacterMaster master = context?.PlayerMaster ?? context?.PlayerBody?.master;
            if (master == null || parameters == null) return false;

            int amount = parameters.Value<int?>("amount") ?? 1;
            string markerItem =
                parameters.Value<string>("markerItem") ??
                parameters.Value<string>("minionItem") ??
                "LemurianHarness";

            return RecruitMinionsObjectiveHandler.CountAliveMinions(master, markerItem) >= Math.Max(1, amount);
        }
    }
}
