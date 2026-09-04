using System;
using System.Collections.Generic;

using BepInEx.Logging;
using RoR2;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    public static class LeaveStageObjectiveHandler
    {
        private const string ObjectiveType = "LeaveStage";
        private static string previousStage = "";
        private static ManualLogSource logger;
        private static bool initialized;

        public static void Initialize(ManualLogSource log)
        {
            if (initialized) return;
            initialized = true;
            logger = log;

            MissionStageRuntimeTracker.StageStarted += OnStageStarted;
            Run.onRunStartGlobal += run => previousStage = "";
            Run.onRunDestroyGlobal += run => previousStage = "";

            logger?.LogInfo("[MISSION V2] LeaveStage handler inicializado.");
        }

        private static void OnStageStarted(MissionStageEventContext stageContext)
        {
            if (!NetworkServer.active || Run.instance == null)
            {
                previousStage = stageContext?.StageName ?? "";
                return;
            }

            string leftStage = previousStage;
            previousStage = stageContext?.StageName ?? "";

            if (
                string.IsNullOrWhiteSpace(leftStage) ||
                !MissionRuntimeActivityPlan.IsTypeActive(ObjectiveType) ||
                !MissionRuntimeCatalog.HasObjectiveType(ObjectiveType)
            )
            {
                return;
            }

            IReadOnlyList<MissionObjectiveRuntimeBinding> bindings =
                MissionRuntimeCatalog.GetObjectives(ObjectiveType);

            for (int i = 0; i < bindings.Count; i++)
            {
                MissionObjectiveRuntimeBinding binding = bindings[i];
                string requiredStage = binding?.Objective?.Parameters?.Value<string>("stage") ?? "";

                if (
                    !string.IsNullOrWhiteSpace(requiredStage) &&
                    !string.Equals(leftStage, requiredStage.Trim(), StringComparison.OrdinalIgnoreCase)
                )
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
                    GenericMissionDispatcher.HandleProgressResult(binding, result, "Stage abandonado: " + leftStage);

                    if (result.ProgressScope == MissionProgressScope.Shared) break;
                }
            }
        }
    }
}
