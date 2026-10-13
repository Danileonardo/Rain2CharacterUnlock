using System;
using System.Collections.Generic;

using BepInEx.Logging;
using Newtonsoft.Json.Linq;
using RoR2;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Generic Mission v2 objective extracted from the official Acolyte unlock
    /// pattern: an interaction consumes one or more real inventory items.
    ///
    /// The primitive is deliberately generic and does not encode Acolyte,
    /// Hallowed Concepts, or a guessed "healing item" category.
    /// </summary>
    public static class ConsumeItemOnInteractionObjectiveHandler
    {
        private const string ObjectiveType = "ConsumeItemOnInteraction";
        private static ManualLogSource logger;
        private static bool initialized;

        public static void Initialize(ManualLogSource log)
        {
            if (initialized) return;
            initialized = true;
            logger = log;
            GameplayEventHub.ItemConsumedOnInteraction += OnItemConsumed;
            UsuLog.Verbose(logger, "[MISSION V2] ConsumeItemOnInteraction handler inicializado.");
        }

        private static void OnItemConsumed(GameplayConsumedItemInteractionEvent evt)
        {
            GameplayInteractionEvent interaction = evt?.Interaction;
            if (!NetworkServer.active || Run.instance == null || interaction == null ||
                interaction.PlayerMaster == null || interaction.PlayerBody == null ||
                evt.AmountConsumed <= 0)
                return;

            if (!MissionRuntimeActivityPlan.IsTypeActive(ObjectiveType) ||
                !MissionRuntimeCatalog.HasObjectiveType(ObjectiveType))
                return;

            if (MissionPlayerIdentity.GetNetworkUser(interaction.PlayerMaster) == null)
                return;

            ItemDef itemDef = ItemCatalog.GetItemDef(evt.ItemIndex);
            string itemName = itemDef?.name ?? "";
            string itemToken = itemDef?.nameToken ?? "";

            IReadOnlyList<MissionObjectiveRuntimeBinding> bindings =
                MissionRuntimeCatalog.GetObjectives(ObjectiveType);

            for (int i = 0; i < bindings.Count; i++)
            {
                MissionObjectiveRuntimeBinding binding = bindings[i];
                MissionObjective objective = binding?.Objective;
                if (objective == null ||
                    !MatchesTarget(objective.Parameters, interaction) ||
                    !MatchesItem(objective.Parameters, itemName, itemToken))
                    continue;

                MissionEventContext context = new MissionEventContext
                {
                    PlayerMaster = interaction.PlayerMaster,
                    PlayerBody = interaction.PlayerBody,
                    ActionBody = interaction.PlayerBody,
                    TargetBody = interaction.TargetBody,
                    EventType = ObjectiveType
                };

                MissionCompositionProgressResult result =
                    MissionCompositionProgressEvaluator.AddProgress(
                        binding.MissionId,
                        binding.Mission,
                        binding.RouteIndex,
                        binding.ObjectiveIndex,
                        context,
                        evt.AmountConsumed
                    );

                GenericMissionDispatcher.HandleProgressResult(
                    binding,
                    result,
                    "Objeto consumido al interactuar: " +
                    (string.IsNullOrWhiteSpace(itemName) ? evt.ItemIndex.ToString() : itemName)
                );
            }
        }

        private static bool MatchesItem(JObject parameters, string itemName, string itemToken)
        {
            string expected = ReadString(parameters, "item", "itemName", "itemToken");
            if (string.IsNullOrWhiteSpace(expected))
                return true; // any actually consumed item, but only for a configured target

            string[] allowed = expected.Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < allowed.Length; i++)
            {
                string candidate = allowed[i].Trim();
                if (string.Equals(candidate, itemName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(candidate, itemToken, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private static bool MatchesTarget(JObject parameters, GameplayInteractionEvent interaction)
        {
            string expectedObject = ReadString(parameters, "objectName", "targetObject", "interactable");
            string expectedBody = ReadString(parameters, "body", "targetBody", "bodyName");
            string expectedComponent = ReadString(parameters, "componentType", "component");

            // Safety: a target is mandatory. We never count arbitrary inventory
            // losses that happen during unrelated interactions.
            if (string.IsNullOrWhiteSpace(expectedObject) &&
                string.IsNullOrWhiteSpace(expectedBody) &&
                string.IsNullOrWhiteSpace(expectedComponent))
                return false;

            if (!string.IsNullOrWhiteSpace(expectedObject) &&
                !MatchesName(expectedObject, interaction.TargetObjectName,
                    parameters?.Value<string>("nameMatch")))
                return false;

            if (!string.IsNullOrWhiteSpace(expectedBody) &&
                !string.Equals(expectedBody.Trim(), interaction.TargetBodyName ?? "",
                    StringComparison.OrdinalIgnoreCase))
                return false;

            if (!string.IsNullOrWhiteSpace(expectedComponent) &&
                !HasComponentType(interaction.ComponentTypeNames, expectedComponent))
                return false;

            return true;
        }

        private static bool MatchesName(string expected, string actual, string mode)
        {
            expected = NormalizeObjectName(expected);
            actual = NormalizeObjectName(actual);
            if (string.IsNullOrWhiteSpace(expected) || string.IsNullOrWhiteSpace(actual))
                return false;

            string normalizedMode = string.IsNullOrWhiteSpace(mode)
                ? "exact" : mode.Trim().ToLowerInvariant();

            if (normalizedMode == "contains")
                return actual.IndexOf(expected, StringComparison.OrdinalIgnoreCase) >= 0;
            if (normalizedMode == "startswith" || normalizedMode == "prefix")
                return actual.StartsWith(expected, StringComparison.OrdinalIgnoreCase);
            return string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase);
        }

        private static bool HasComponentType(IReadOnlyList<string> types, string expected)
        {
            if (types == null || string.IsNullOrWhiteSpace(expected)) return false;
            expected = expected.Trim();
            for (int i = 0; i < types.Count; i++)
            {
                string current = types[i];
                if (string.IsNullOrWhiteSpace(current)) continue;
                if (string.Equals(current, expected, StringComparison.OrdinalIgnoreCase)) return true;
                int lastDot = current.LastIndexOf('.');
                string shortName = lastDot >= 0 && lastDot + 1 < current.Length
                    ? current.Substring(lastDot + 1) : current;
                if (string.Equals(shortName, expected, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        private static string NormalizeObjectName(string value)
        {
            string result = (value ?? "").Trim();
            const string clone = "(Clone)";
            if (result.EndsWith(clone, StringComparison.OrdinalIgnoreCase))
                result = result.Substring(0, result.Length - clone.Length).Trim();
            return result;
        }

        private static string ReadString(JObject parameters, params string[] keys)
        {
            if (parameters == null || keys == null) return "";
            for (int i = 0; i < keys.Length; i++)
            {
                string value = parameters.Value<string>(keys[i]);
                if (!string.IsNullOrWhiteSpace(value)) return value.Trim();
            }
            return "";
        }
    }
}
