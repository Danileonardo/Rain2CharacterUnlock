using System;
using System.Collections.Generic;

using BepInEx.Logging;
using Newtonsoft.Json.Linq;
using RoR2;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Objetivo de posesión de equipo configurable.
    ///
    /// Modo normal:
    ///     1 mientras el jugador lleva el equipo, 0 si no.
    ///
    /// Modo continuous/failOnLoss:
    ///     el jugador puede adquirirlo durante la run, pero una vez
    ///     adquirido, perderlo/inercambiarlo/consumirlo invalida ese
    ///     objetivo hasta la siguiente run.
    ///
    /// failOnDeath permite invalidar el intento al morir.
    /// </summary>
    public static class CarryEquipmentObjectiveHandler
    {
        private const string ObjectiveType = "CarryEquipment";

        private sealed class CarryState
        {
            public bool Acquired;
            public bool Invalidated;
        }

        private static readonly Dictionary<string, EquipmentIndex> EquipmentCache =
            new Dictionary<string, EquipmentIndex>(StringComparer.OrdinalIgnoreCase);

        private static readonly Dictionary<string, CarryState> StateByObjective =
            new Dictionary<string, CarryState>(StringComparer.Ordinal);

        private static ManualLogSource logger;
        private static bool initialized;

        public static void Initialize(ManualLogSource log)
        {
            if (initialized) return;
            initialized = true;
            logger = log;

            On.RoR2.CharacterMaster.OnInventoryChanged += CharacterMaster_OnInventoryChanged;
            GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeathGlobal;
            MissionStageRuntimeTracker.StageStarted += OnStageStarted;
            Run.onRunStartGlobal += OnRunStart;
            Run.onRunDestroyGlobal += OnRunEnd;

            logger?.LogInfo("[MISSION V2] CarryEquipment handler inicializado.");
        }

        private static void OnRunStart(Run run)
        {
            StateByObjective.Clear();
            EquipmentCache.Clear();
        }

        private static void OnRunEnd(Run run)
        {
            StateByObjective.Clear();
            EquipmentCache.Clear();
        }

        private static void CharacterMaster_OnInventoryChanged(
            On.RoR2.CharacterMaster.orig_OnInventoryChanged orig,
            CharacterMaster self
        )
        {
            orig(self);

            if (!NetworkServer.active || Run.instance == null || self == null)
            {
                return;
            }

            Evaluate(self);
        }

        private static void OnCharacterDeathGlobal(DamageReport report)
        {
            if (
                !NetworkServer.active ||
                Run.instance == null ||
                report?.victimMaster == null ||
                MissionPlayerIdentity.GetNetworkUser(report.victimMaster) == null ||
                !MissionRuntimeActivityPlan.IsTypeActive(ObjectiveType) ||
                !MissionRuntimeCatalog.HasObjectiveType(ObjectiveType)
            )
            {
                return;
            }

            CharacterMaster master = report.victimMaster;
            IReadOnlyList<MissionObjectiveRuntimeBinding> bindings =
                MissionRuntimeCatalog.GetObjectives(ObjectiveType);

            for (int i = 0; i < bindings.Count; i++)
            {
                MissionObjectiveRuntimeBinding binding = bindings[i];
                if (binding?.Objective == null || !ReadBool(binding.Objective.Parameters, "failOnDeath", false))
                {
                    continue;
                }

                CarryState state = GetState(binding, master);
                if (!state.Acquired || state.Invalidated)
                {
                    continue;
                }

                state.Invalidated = true;
                SetProgress(binding, master, 0d, "Muestra perdida por muerte");
            }
        }

        private static void OnStageStarted(MissionStageEventContext context)
        {
            if (!NetworkServer.active || Run.instance == null) return;

            foreach (PlayerCharacterMasterController controller in PlayerCharacterMasterController.instances)
            {
                if (controller?.master != null)
                {
                    Evaluate(controller.master);
                }
            }
        }

        private static void Evaluate(CharacterMaster master)
        {
            if (
                master == null ||
                master.inventory == null ||
                MissionPlayerIdentity.GetNetworkUser(master) == null ||
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
                if (binding?.Objective == null) continue;

                string equipmentName = ReadEquipment(binding.Objective.Parameters);
                if (!TryGetEquipmentIndex(equipmentName, out EquipmentIndex equipmentIndex))
                {
                    continue;
                }

                bool holding = master.inventory.currentEquipmentIndex == equipmentIndex;
                bool continuous = ReadBool(binding.Objective.Parameters, "continuous", false);
                bool failOnLoss = ReadBool(binding.Objective.Parameters, "failOnLoss", continuous);

                CarryState state = GetState(binding, master);

                if (state.Invalidated)
                {
                    SetProgress(binding, master, 0d, "Objetivo de transporte invalidado");
                    continue;
                }

                if (holding)
                {
                    state.Acquired = true;
                    SetProgress(binding, master, 1d, "Equipo requerido presente");
                    continue;
                }

                if (state.Acquired && failOnLoss)
                {
                    state.Invalidated = true;
                    SetProgress(binding, master, 0d, "Equipo requerido perdido");
                    continue;
                }

                SetProgress(binding, master, 0d, "Equipo requerido ausente");
            }
        }

        private static void SetProgress(
            MissionObjectiveRuntimeBinding binding,
            CharacterMaster master,
            double value,
            string details
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

            MissionCompositionProgressResult result =
                MissionCompositionProgressEvaluator.SetProgress(
                    binding.MissionId,
                    binding.Mission,
                    binding.RouteIndex,
                    binding.ObjectiveIndex,
                    context,
                    value
                );

            GenericMissionDispatcher.HandleProgressResult(binding, result, details);
        }

        private static CarryState GetState(
            MissionObjectiveRuntimeBinding binding,
            CharacterMaster master
        )
        {
            string key =
                (binding?.MissionId ?? "") + ":" +
                (binding?.RouteIndex ?? -1) + ":" +
                (binding?.ObjectiveIndex ?? -1) + ":" +
                master.GetInstanceID();

            if (!StateByObjective.TryGetValue(key, out CarryState state) || state == null)
            {
                state = new CarryState();
                StateByObjective[key] = state;
            }

            return state;
        }

        private static string ReadEquipment(JObject parameters)
        {
            return parameters?.Value<string>("equipment") ?? "";
        }

        private static bool ReadBool(JObject parameters, string key, bool defaultValue)
        {
            return parameters?.Value<bool?>(key) ?? defaultValue;
        }

        private static bool TryGetEquipmentIndex(string name, out EquipmentIndex index)
        {
            index = EquipmentIndex.None;
            if (string.IsNullOrWhiteSpace(name)) return false;

            if (EquipmentCache.TryGetValue(name, out index))
            {
                return index != EquipmentIndex.None;
            }

            index = EquipmentCatalog.FindEquipmentIndex(name.Trim());
            EquipmentCache[name] = index;
            return index != EquipmentIndex.None;
        }
    }
}
