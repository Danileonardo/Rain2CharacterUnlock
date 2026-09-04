using System.Collections.Generic;

using BepInEx.Logging;
using RoR2;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    public static class CompleteTeleporterObjectiveHandler
    {
        private const string ObjectiveType = "CompleteTeleporter";
        private static ManualLogSource logger;
        private static bool initialized;

        public static void Initialize(ManualLogSource log)
        {
            if (initialized) return;
            initialized = true;
            logger = log;

            MissionStageRuntimeTracker.StageCompleted += OnStageCompleted;
            logger?.LogInfo("[MISSION V2] CompleteTeleporter handler inicializado.");
        }

        private static void OnStageCompleted(MissionStageEventContext stageContext)
        {
            if (
                !NetworkServer.active ||
                Run.instance == null ||
                !MissionProgressRegistry.IsRunActive ||
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
                if (binding == null || binding.Mission == null) continue;

                MissionProgressScope scope = MissionProgressKeyBuilder.GetProgressScope(binding.Mission);

                if (scope == MissionProgressScope.Shared)
                {
                    // En shared probamos con cada jugador hasta encontrar un
                    // contexto que satisfaga las condiciones de la ruta.
                    foreach (PlayerCharacterMasterController controller in PlayerCharacterMasterController.instances)
                    {
                        CharacterMaster master = controller?.master;
                        if (master == null) continue;

                        MissionCompositionProgressResult result = Add(binding, master);
                        if (!result.Accepted) continue;

                        GenericMissionDispatcher.HandleProgressResult(
                            binding,
                            result,
                            $"Teleporter: {stageContext?.StageName ?? ""}"
                        );
                        break;
                    }
                }
                else
                {
                    foreach (PlayerCharacterMasterController controller in PlayerCharacterMasterController.instances)
                    {
                        CharacterMaster master = controller?.master;
                        if (master == null) continue;

                        MissionCompositionProgressResult result = Add(binding, master);
                        GenericMissionDispatcher.HandleProgressResult(
                            binding,
                            result,
                            $"Teleporter: {stageContext?.StageName ?? ""}"
                        );
                    }
                }
            }
        }

        private static MissionCompositionProgressResult Add(
            MissionObjectiveRuntimeBinding binding,
            CharacterMaster master
        )
        {
            CharacterBody body = master?.GetBody();
            MissionEventContext context = new MissionEventContext
            {
                PlayerMaster = master,
                PlayerBody = body,
                ActionBody = body,
                EventType = ObjectiveType
            };

            return MissionCompositionProgressEvaluator.AddProgress(
                binding.MissionId,
                binding.Mission,
                binding.RouteIndex,
                binding.ObjectiveIndex,
                context,
                1d
            );
        }
    }
}
