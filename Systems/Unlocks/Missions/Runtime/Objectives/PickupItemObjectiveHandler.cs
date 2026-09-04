using System;
using System.Collections.Generic;

using BepInEx.Logging;
using Newtonsoft.Json.Linq;
using RoR2;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Detecta aumentos reales de inventario. Sirve para PickupItem y
    /// también mantiene la bandera NoItemPickup por jugador.
    /// No realiza polling; sólo reacciona a OnInventoryChanged.
    /// </summary>
    public static class PickupItemObjectiveHandler
    {
        private const string ObjectiveType = "PickupItem";

        private sealed class InventoryState
        {
            public int TotalPermanentItems;
            public readonly Dictionary<ItemIndex, int> Counts =
                new Dictionary<ItemIndex, int>();
            public bool HasPickedAnyItem;
        }

        private static readonly Dictionary<CharacterMaster, InventoryState>
            StateByPlayer = new Dictionary<CharacterMaster, InventoryState>();

        private static readonly Dictionary<string, ItemIndex> ItemIndexCache =
            new Dictionary<string, ItemIndex>(StringComparer.OrdinalIgnoreCase);

        private static ManualLogSource logger;
        private static bool initialized;

        public static void Initialize(ManualLogSource log)
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            logger = log;

            On.RoR2.CharacterMaster.OnInventoryChanged += CharacterMaster_OnInventoryChanged;
            Run.onRunStartGlobal += OnRunStart;
            Run.onRunDestroyGlobal += OnRunEnd;

            MissionStageRuntimeTracker.StageStarted += OnStageStarted;

            logger?.LogInfo("[MISSION V2] PickupItem/NoItemPickup handler inicializado.");
        }

        private static void OnRunStart(Run run)
        {
            StateByPlayer.Clear();
        }

        private static void OnRunEnd(Run run)
        {
            StateByPlayer.Clear();
        }

        private static void OnStageStarted(MissionStageEventContext context)
        {
            if (!NetworkServer.active || Run.instance == null)
            {
                return;
            }

            // Para rutas como Scout: la restricción se evalúa dentro
            // del primer sector. Reiniciamos el indicador en cada stage.
            foreach (PlayerCharacterMasterController controller in PlayerCharacterMasterController.instances)
            {
                CharacterMaster master = controller?.master;
                if (master == null || master.inventory == null)
                {
                    continue;
                }

                InventoryState state = GetOrCreateState(master);
                Snapshot(master, state, resetPickupFlag: true);
            }
        }

        private static void CharacterMaster_OnInventoryChanged(
            On.RoR2.CharacterMaster.orig_OnInventoryChanged orig,
            CharacterMaster self
        )
        {
            orig(self);

            if (!NetworkServer.active || Run.instance == null || self == null || self.inventory == null)
            {
                return;
            }

            if (MissionPlayerIdentity.GetNetworkUser(self) == null)
            {
                return;
            }

            InventoryState state = GetOrCreateState(self);

            // Primera observación = baseline. Evita considerar como pickup
            // los items internos con los que un survivor aparece al spawn.
            if (state.Counts.Count == 0 && state.TotalPermanentItems == 0)
            {
                Snapshot(self, state, resetPickupFlag: false);
                return;
            }

            int newTotal = CountTotalPermanentItems(self.inventory);
            if (newTotal > state.TotalPermanentItems)
            {
                state.HasPickedAnyItem = true;
            }

            if (
                MissionRuntimeActivityPlan.IsTypeActive(ObjectiveType) &&
                MissionRuntimeCatalog.HasObjectiveType(ObjectiveType)
            )
            {
                EvaluatePickupObjectives(self, state);
            }

            Snapshot(self, state, resetPickupFlag: false);
        }

        public static bool HasPickedAnyItem(CharacterMaster master)
        {
            return
                master != null &&
                StateByPlayer.TryGetValue(master, out InventoryState state) &&
                state != null &&
                state.HasPickedAnyItem;
        }

        private static void EvaluatePickupObjectives(CharacterMaster master, InventoryState state)
        {
            IReadOnlyList<MissionObjectiveRuntimeBinding> bindings =
                MissionRuntimeCatalog.GetObjectives(ObjectiveType);

            for (int i = 0; i < bindings.Count; i++)
            {
                MissionObjectiveRuntimeBinding binding = bindings[i];
                if (binding?.Objective?.Parameters == null)
                {
                    continue;
                }

                string itemName = ReadItemName(binding.Objective.Parameters);
                if (!TryGetItemIndex(itemName, out ItemIndex itemIndex))
                {
                    continue;
                }

                int oldCount = state.Counts.TryGetValue(itemIndex, out int stored)
                    ? stored
                    : master.inventory.GetItemCountPermanent(itemIndex);

                int newCount = master.inventory.GetItemCountPermanent(itemIndex);
                int gained = newCount - oldCount;

                if (gained <= 0)
                {
                    continue;
                }

                MissionEventContext context = BuildContext(master);
                MissionCompositionProgressResult result =
                    MissionCompositionProgressEvaluator.AddProgress(
                        binding.MissionId,
                        binding.Mission,
                        binding.RouteIndex,
                        binding.ObjectiveIndex,
                        context,
                        gained
                    );

                GenericMissionDispatcher.HandleProgressResult(
                    binding,
                    result,
                    $"Pickup {itemName}: +{gained}"
                );
            }
        }

        private static InventoryState GetOrCreateState(CharacterMaster master)
        {
            if (!StateByPlayer.TryGetValue(master, out InventoryState state) || state == null)
            {
                state = new InventoryState();
                StateByPlayer[master] = state;
            }

            return state;
        }

        private static void Snapshot(
            CharacterMaster master,
            InventoryState state,
            bool resetPickupFlag
        )
        {
            if (master == null || master.inventory == null || state == null)
            {
                return;
            }

            state.Counts.Clear();

            int itemCount = ItemCatalog.itemCount;
            for (int i = 0; i < itemCount; i++)
            {
                ItemIndex index = (ItemIndex)i;
                int count = master.inventory.GetItemCountPermanent(index);
                if (count > 0)
                {
                    state.Counts[index] = count;
                }
            }

            state.TotalPermanentItems = CountTotalPermanentItems(master.inventory);

            if (resetPickupFlag)
            {
                state.HasPickedAnyItem = false;
            }
        }

        private static int CountTotalPermanentItems(Inventory inventory)
        {
            if (inventory == null)
            {
                return 0;
            }

            int total = 0;
            int itemCount = ItemCatalog.itemCount;

            for (int i = 0; i < itemCount; i++)
            {
                total += inventory.GetItemCountPermanent((ItemIndex)i);
            }

            return total;
        }

        private static bool TryGetItemIndex(string itemName, out ItemIndex itemIndex)
        {
            itemIndex = ItemIndex.None;

            if (string.IsNullOrWhiteSpace(itemName))
            {
                return false;
            }

            if (ItemIndexCache.TryGetValue(itemName, out itemIndex))
            {
                return itemIndex != ItemIndex.None;
            }

            itemIndex = ItemCatalog.FindItemIndex(itemName.Trim());
            ItemIndexCache[itemName] = itemIndex;
            return itemIndex != ItemIndex.None;
        }

        private static string ReadItemName(JObject parameters)
        {
            return
                parameters?.Value<string>("item") ??
                parameters?.Value<string>("itemName") ??
                parameters?.Value<string>("itemInternalName") ??
                "";
        }

        private static MissionEventContext BuildContext(CharacterMaster master)
        {
            return new MissionEventContext
            {
                PlayerMaster = master,
                PlayerBody = master?.GetBody(),
                ActionBody = master?.GetBody(),
                EventType = ObjectiveType
            };
        }
    }
}
