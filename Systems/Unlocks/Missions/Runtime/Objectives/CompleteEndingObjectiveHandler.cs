using System;
using System.Collections.Generic;

using BepInEx.Logging;
using Newtonsoft.Json.Linq;
using RoR2;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    public static class CompleteEndingObjectiveHandler
    {
        private const string ObjectiveType = "CompleteEnding";
        private static ManualLogSource logger;
        private static bool initialized;

        public static void Initialize(ManualLogSource log)
        {
            if (initialized) return;
            initialized = true;
            logger = log;

            Run.onServerGameOver += OnServerGameOver;
            logger?.LogInfo("[MISSION V2] CompleteEnding handler inicializado.");
        }

        private static void OnServerGameOver(Run run, GameEndingDef gameEndingDef)
        {
            if (
                !NetworkServer.active ||
                run == null ||
                gameEndingDef == null ||
                !MissionRuntimeActivityPlan.IsTypeActive(ObjectiveType) ||
                !MissionRuntimeCatalog.HasObjectiveType(ObjectiveType)
            )
            {
                return;
            }

            string endingName = gameEndingDef.cachedName ?? "";
            IReadOnlyList<MissionObjectiveRuntimeBinding> bindings =
                MissionRuntimeCatalog.GetObjectives(ObjectiveType);

            for (int i = 0; i < bindings.Count; i++)
            {
                MissionObjectiveRuntimeBinding binding = bindings[i];
                string required = binding?.Objective?.Parameters?.Value<string>("ending") ?? "";

                if (!MatchesEnding(required, endingName, gameEndingDef))
                {
                    continue;
                }

                foreach (PlayerCharacterMasterController controller in PlayerCharacterMasterController.instances)
                {
                    CharacterMaster master = controller?.master;
                    if (master == null) continue;

                    CharacterBody body = master.GetBody();
                    MissionEventContext context = new MissionEventContext
                    {
                        PlayerMaster = master,
                        PlayerBody = body,
                        ActionBody = body,
                        EventType = ObjectiveType
                    };

                    MissionCompositionProgressResult result =
                        MissionCompositionProgressEvaluator.MarkObjectiveCompleted(
                            binding.MissionId,
                            binding.Mission,
                            binding.RouteIndex,
                            binding.ObjectiveIndex,
                            context
                        );

                    if (!result.Accepted) continue;

                    GenericMissionDispatcher.HandleProgressResult(
                        binding,
                        result,
                        "Ending: " + endingName
                    );

                    if (result.ProgressScope == MissionProgressScope.Shared)
                    {
                        break;
                    }
                }
            }
        }

        private static bool MatchesEnding(
            string required,
            string endingName,
            GameEndingDef endingDef
        )
        {
            if (string.IsNullOrWhiteSpace(required)) return true;

            string normalized = required.Trim().ToLowerInvariant();
            string actual = (endingName ?? "").ToLowerInvariant();

            if (normalized == "escape" || normalized == "main")
            {
                return endingDef.isWin && !actual.Contains("obliter");
            }

            if (normalized == "obliterate" || normalized == "obliteration")
            {
                return actual.Contains("obliter");
            }

            return string.Equals(required.Trim(), endingName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
