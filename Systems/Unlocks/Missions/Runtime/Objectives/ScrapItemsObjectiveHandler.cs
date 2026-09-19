using System.Collections.Generic;

using BepInEx.Logging;
using RoR2;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    public static class ScrapItemsObjectiveHandler
    {
        private const string ObjectiveType = "ScrapItems";
        private static ManualLogSource logger;
        private static bool initialized;

        public static void Initialize(ManualLogSource log)
        {
            if (initialized) return;
            initialized = true;
            logger = log;

            GameplayEventHub.ScrapperProcessed += OnScrapperProcessed;
            UsuLog.Verbose(
                logger,
                "[MISSION V2] ScrapItems handler inicializado.");
        }

        private static void OnScrapperProcessed(
            ScrapperTransaction transaction
        )
        {
            if (
                transaction == null ||
                !NetworkServer.active ||
                Run.instance == null ||
                !MissionProgressRegistry.IsRunActive ||
                !MissionRuntimeActivityPlan.IsTypeActive(ObjectiveType) ||
                !MissionRuntimeCatalog.HasObjectiveType(ObjectiveType)
            )
            {
                return;
            }

            CharacterMaster master = transaction.Master;
            CharacterBody body = transaction.Body;
            int converted = transaction.AmountConverted;

            if (
                master == null ||
                master.inventory == null ||
                converted <= 0 ||
                MissionPlayerIdentity.GetNetworkUser(master) == null
            )
            {
                return;
            }

            IReadOnlyList<MissionObjectiveRuntimeBinding> bindings =
                MissionRuntimeCatalog.GetObjectives(ObjectiveType);

            for (int i = 0; i < bindings.Count; i++)
            {
                MissionObjectiveRuntimeBinding binding = bindings[i];
                if (binding == null) continue;

                CharacterBody effectiveBody = body ?? master.GetBody();

                MissionEventContext context = new MissionEventContext
                {
                    PlayerMaster = master,
                    PlayerBody = effectiveBody,
                    ActionBody = effectiveBody,
                    EventType = ObjectiveType
                };

                MissionCompositionProgressResult result =
                    MissionCompositionProgressEvaluator.AddProgress(
                        binding.MissionId,
                        binding.Mission,
                        binding.RouteIndex,
                        binding.ObjectiveIndex,
                        context,
                        converted
                    );

                GenericMissionDispatcher.HandleProgressResult(
                    binding,
                    result,
                    $"Scrap: +{converted}"
                );
            }
        }
    }
}
