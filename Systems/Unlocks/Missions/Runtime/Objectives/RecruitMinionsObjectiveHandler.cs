using System;
using System.Collections.Generic;

using BepInEx.Logging;
using Newtonsoft.Json.Linq;
using RoR2;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Detecta minions nuevos mediante un item marcador en su inventario.
    /// Devotion usa LemurianHarness, pero el handler es configurable.
    /// </summary>
    public static class RecruitMinionsObjectiveHandler
    {
        private const string ObjectiveType = "RecruitMinions";

        private static readonly Dictionary<string, ItemIndex> ItemCache =
            new Dictionary<string, ItemIndex>(StringComparer.OrdinalIgnoreCase);

        private static readonly Dictionary<CharacterMaster, HashSet<CharacterMaster>>
            RecruitedByOwner = new Dictionary<CharacterMaster, HashSet<CharacterMaster>>();

        private static ManualLogSource logger;
        private static bool initialized;

        public static void Initialize(ManualLogSource log)
        {
            if (initialized) return;
            initialized = true;
            logger = log;

            On.RoR2.CharacterMaster.OnInventoryChanged += CharacterMaster_OnInventoryChanged;
            Run.onRunStartGlobal += run => RecruitedByOwner.Clear();
            Run.onRunDestroyGlobal += run => RecruitedByOwner.Clear();

            logger?.LogInfo("[MISSION V2] RecruitMinions handler inicializado.");

            if (TryGetItemIndex("LemurianHarness", out ItemIndex devotionMarker))
            {
                logger?.LogInfo(
                    "[MISSION V2] RecruitMinions marcador Devoción resuelto | " +
                    $"LemurianHarness: {devotionMarker}"
                );
            }
            else
            {
                logger?.LogInfo(
                    "[MISSION V2] RecruitMinions: LemurianHarness no está " +
                    "disponible en ItemCatalog; fallback " +
                    "DevotedLemurianController activo."
                );
            }
        }

        private static void CharacterMaster_OnInventoryChanged(
            On.RoR2.CharacterMaster.orig_OnInventoryChanged orig,
            CharacterMaster self
        )
        {
            orig(self);

            if (
                !NetworkServer.active ||
                Run.instance == null ||
                self == null ||
                self.inventory == null ||
                self.minionOwnership == null ||
                self.minionOwnership.ownerMaster == null ||
                !MissionRuntimeActivityPlan.IsTypeActive(ObjectiveType) ||
                !MissionRuntimeCatalog.HasObjectiveType(ObjectiveType)
            )
            {
                return;
            }

            CharacterMaster owner = self.minionOwnership.ownerMaster;
            if (MissionPlayerIdentity.GetNetworkUser(owner) == null)
            {
                return;
            }

            IReadOnlyList<MissionObjectiveRuntimeBinding> bindings =
                MissionRuntimeCatalog.GetObjectives(ObjectiveType);

            for (int i = 0; i < bindings.Count; i++)
            {
                MissionObjectiveRuntimeBinding binding = bindings[i];
                string markerItem = ReadMarker(binding?.Objective?.Parameters);
                if (!MatchesRecruitMarker(self, markerItem))
                {
                    continue;
                }

                HashSet<CharacterMaster> set = GetOrCreateSet(owner);
                if (!set.Add(self))
                {
                    continue;
                }

                CharacterBody ownerBody = owner.GetBody();
                MissionEventContext context = new MissionEventContext
                {
                    PlayerMaster = owner,
                    PlayerBody = ownerBody,
                    ActionBody = ownerBody,
                    TargetBody = self.GetBody(),
                    EventType = ObjectiveType
                };

                MissionCompositionProgressResult result =
                    MissionCompositionProgressEvaluator.AddProgress(
                        binding.MissionId,
                        binding.Mission,
                        binding.RouteIndex,
                        binding.ObjectiveIndex,
                        context,
                        1d
                    );

                GenericMissionDispatcher.HandleProgressResult(
                    binding,
                    result,
                    "Nuevo minion reclutado"
                );
            }
        }

        public static int CountAliveMinions(CharacterMaster owner, string markerItem)
        {
            if (owner == null)
            {
                return 0;
            }

            if (!RecruitedByOwner.TryGetValue(owner, out HashSet<CharacterMaster> set) || set == null)
            {
                return 0;
            }

            int count = 0;
            foreach (CharacterMaster minion in set)
            {
                if (minion == null) continue;
                if (!MatchesRecruitMarker(minion, markerItem)) continue;

                CharacterBody body = minion.GetBody();
                if (body?.healthComponent != null && body.healthComponent.alive)
                {
                    count++;
                }
            }

            return count;
        }

        private static HashSet<CharacterMaster> GetOrCreateSet(CharacterMaster owner)
        {
            if (!RecruitedByOwner.TryGetValue(owner, out HashSet<CharacterMaster> set) || set == null)
            {
                set = new HashSet<CharacterMaster>();
                RecruitedByOwner[owner] = set;
            }

            return set;
        }

        private static string ReadMarker(JObject parameters)
        {
            return
                parameters?.Value<string>("markerItem") ??
                parameters?.Value<string>("minionItem") ??
                "LemurianHarness";
        }

        private static bool MatchesRecruitMarker(
            CharacterMaster minion,
            string markerItem
        )
        {
            if (minion == null)
            {
                return false;
            }

            if (
                minion.inventory != null &&
                TryGetItemIndex(markerItem, out ItemIndex markerIndex) &&
                minion.inventory.GetItemCountPermanent(markerIndex) > 0
            )
            {
                return true;
            }

            // LemurianHarness es el marcador vanilla de los seguidores
            // creados por el Artefacto de Devoción. Algunas instalaciones
            // pueden no exponer su referencia estática aunque el controlador
            // de Devoción sí exista. En ese caso usamos el componente que
            // identifica directamente a un Lemuriano devoto.
            return
                IsLemurianHarnessMarker(markerItem) &&
                minion.GetComponent<DevotedLemurianController>() != null;
        }

        private static bool IsLemurianHarnessMarker(string markerItem)
        {
            return
                string.Equals(
                    markerItem?.Trim(),
                    "LemurianHarness",
                    StringComparison.OrdinalIgnoreCase
                );
        }

        private static bool TryGetItemIndex(string name, out ItemIndex index)
        {
            index = ItemIndex.None;
            if (string.IsNullOrWhiteSpace(name)) return false;

            string trimmedName = name.Trim();

            if (ItemCache.TryGetValue(trimmedName, out index))
            {
                return index != ItemIndex.None;
            }

            index = ItemCatalog.FindItemIndex(trimmedName);

            if (index == ItemIndex.None)
            {
                string expectedNameToken =
                    "ITEM_" +
                    trimmedName.Replace("_", "").ToUpperInvariant() +
                    "_NAME";

                int itemCount = ItemCatalog.itemCount;
                for (int i = 0; i < itemCount; i++)
                {
                    ItemIndex candidateIndex = (ItemIndex)i;
                    ItemDef candidate = ItemCatalog.GetItemDef(candidateIndex);
                    if (candidate == null) continue;

                    if (
                        string.Equals(
                            candidate.name,
                            trimmedName,
                            StringComparison.OrdinalIgnoreCase
                        ) ||
                        string.Equals(
                            candidate.nameToken,
                            expectedNameToken,
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    {
                        index = candidateIndex;
                        break;
                    }
                }
            }

            // Sólo cacheamos resoluciones válidas. Si el catálogo todavía
            // no estaba listo, una consulta futura podrá intentarlo de nuevo.
            if (index != ItemIndex.None)
            {
                ItemCache[trimmedName] = index;
                return true;
            }

            return false;
        }
    }
}
