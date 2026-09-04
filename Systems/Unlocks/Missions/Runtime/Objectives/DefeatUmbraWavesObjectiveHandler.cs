using System;
using System.Collections.Generic;

using BepInEx.Logging;
using RoR2;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Convierte las muertes de Umbrae del Artefacto de Venganza en
    /// progreso por oleadas completas. Una oleada equivale a una Umbra
    /// por jugador presente en ese momento.
    /// </summary>
    public static class DefeatUmbraWavesObjectiveHandler
    {
        private const string ObjectiveType = "DefeatUmbraWaves";
        private static readonly Dictionary<string, int> KillsInWave =
            new Dictionary<string, int>(StringComparer.Ordinal);

        private static ManualLogSource logger;
        private static bool initialized;
        private static ItemIndex invadingDoppelganger = ItemIndex.None;

        public static void Initialize(ManualLogSource log)
        {
            if (initialized) return;
            initialized = true;
            logger = log;

            GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeath;
            MissionStageRuntimeTracker.StageStarted += OnStageStarted;
            Run.onRunStartGlobal += run => KillsInWave.Clear();
            Run.onRunDestroyGlobal += run => KillsInWave.Clear();

            logger?.LogInfo("[MISSION V2] DefeatUmbraWaves handler inicializado.");
        }

        private static void OnStageStarted(MissionStageEventContext context)
        {
            KillsInWave.Clear();
        }

        private static void OnCharacterDeath(DamageReport report)
        {
            if (
                !NetworkServer.active ||
                Run.instance == null ||
                report?.victimMaster == null ||
                !MissionRuntimeActivityPlan.IsTypeActive(ObjectiveType) ||
                !MissionRuntimeCatalog.HasObjectiveType(ObjectiveType)
            )
            {
                return;
            }

            if (invadingDoppelganger == ItemIndex.None)
            {
                invadingDoppelganger = ItemCatalog.FindItemIndex("InvadingDoppelganger");
            }

            if (
                invadingDoppelganger == ItemIndex.None ||
                report.victimMaster.inventory == null ||
                report.victimMaster.inventory.GetItemCountPermanent(invadingDoppelganger) <= 0
            )
            {
                return;
            }

            IReadOnlyList<MissionObjectiveRuntimeBinding> bindings =
                MissionRuntimeCatalog.GetObjectives(ObjectiveType);

            int expectedInWave = Math.Max(1, PlayerCharacterMasterController.instances.Count);

            for (int i = 0; i < bindings.Count; i++)
            {
                MissionObjectiveRuntimeBinding binding = bindings[i];
                if (binding == null) continue;

                string requiredStage = binding.Objective?.Parameters?.Value<string>("stage") ?? "";
                if (
                    !string.IsNullOrWhiteSpace(requiredStage) &&
                    !string.Equals(
                        MissionStageRuntimeTracker.CurrentStageName,
                        requiredStage.Trim(),
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    continue;
                }

                string key = binding.MissionId + "|" + binding.StorageObjectiveId;
                int current = KillsInWave.TryGetValue(key, out int stored) ? stored + 1 : 1;
                KillsInWave[key] = current;

                if (current < expectedInWave)
                {
                    continue;
                }

                KillsInWave[key] = 0;

                CharacterMaster actorMaster = ResolvePlayerMaster(report) ?? FirstPlayerMaster();
                if (actorMaster == null) continue;

                CharacterBody actorBody = actorMaster.GetBody();
                MissionEventContext context = new MissionEventContext
                {
                    PlayerMaster = actorMaster,
                    PlayerBody = actorBody,
                    ActionBody = report.attackerBody ?? actorBody,
                    TargetBody = report.victimBody,
                    DamageReport = report,
                    EventType = ObjectiveType,
                    IsFatalHit = true
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
                    $"Oleada de sombras completada ({expectedInWave} Umbrae)"
                );
            }
        }

        private static CharacterMaster ResolvePlayerMaster(DamageReport report)
        {
            CharacterMaster master = report?.attackerMaster;
            if (master == null) return null;

            CharacterMaster owner = PlayerOwnerResolver.ResolveOwningPlayerMaster(master);
            return owner ?? (MissionPlayerIdentity.GetNetworkUser(master) != null ? master : null);
        }

        private static CharacterMaster FirstPlayerMaster()
        {
            foreach (PlayerCharacterMasterController controller in PlayerCharacterMasterController.instances)
            {
                if (controller?.master != null) return controller.master;
            }

            return null;
        }
    }
}
