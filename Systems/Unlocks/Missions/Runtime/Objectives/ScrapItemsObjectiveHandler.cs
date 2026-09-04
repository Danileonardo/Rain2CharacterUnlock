using System;
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

            On.RoR2.ScrapperController.BeginScrapping_UniquePickup += OnBeginScrapping;
            logger?.LogInfo("[MISSION V2] ScrapItems handler inicializado.");
        }

        private static void OnBeginScrapping(
            On.RoR2.ScrapperController.orig_BeginScrapping_UniquePickup orig,
            ScrapperController self,
            UniquePickup pickupToTake
        )
        {
            if (
                !NetworkServer.active ||
                Run.instance == null ||
                !MissionProgressRegistry.IsRunActive ||
                !MissionRuntimeActivityPlan.IsTypeActive(ObjectiveType) ||
                !MissionRuntimeCatalog.HasObjectiveType(ObjectiveType)
            )
            {
                orig(self, pickupToTake);
                return;
            }

            CharacterBody body = null;
            CharacterMaster master = null;
            ItemIndex itemIndex = ItemIndex.None;
            int before = 0;

            try
            {
                PickupDef pickupDef = PickupCatalog.GetPickupDef(pickupToTake.pickupIndex);
                if (
                    pickupDef != null &&
                    pickupDef.itemIndex != ItemIndex.None &&
                    self != null &&
                    self.interactor != null
                )
                {
                    body = self.interactor.GetComponent<CharacterBody>();
                    master = body?.master;
                    itemIndex = pickupDef.itemIndex;

                    if (
                        master != null &&
                        master.inventory != null &&
                        MissionPlayerIdentity.GetNetworkUser(master) != null
                    )
                    {
                        before = master.inventory.GetItemCountPermanent(itemIndex);
                    }
                }
            }
            catch (Exception ex)
            {
                logger?.LogWarning("[MISSION V2] ScrapItems no pudo leer estado previo | " + ex.Message);
            }

            orig(self, pickupToTake);

            if (master == null || master.inventory == null || itemIndex == ItemIndex.None || before <= 0)
            {
                return;
            }

            int after = master.inventory.GetItemCountPermanent(itemIndex);
            int converted = before - after;
            if (converted <= 0)
            {
                return;
            }

            IReadOnlyList<MissionObjectiveRuntimeBinding> bindings =
                MissionRuntimeCatalog.GetObjectives(ObjectiveType);

            for (int i = 0; i < bindings.Count; i++)
            {
                MissionObjectiveRuntimeBinding binding = bindings[i];
                if (binding == null) continue;

                MissionEventContext context = new MissionEventContext
                {
                    PlayerMaster = master,
                    PlayerBody = body ?? master.GetBody(),
                    ActionBody = body ?? master.GetBody(),
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
