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

            GameplayEventHub.InventoryChanged += CharacterMaster_OnInventoryChanged;
            GameplayEventHub.CharacterDeath += OnCharacterDeathGlobal;
            MissionStageRuntimeTracker.StageStarted += OnStageStarted;
            MissionRunLifecycle.RunStarted += OnRunStart;
            MissionRunLifecycle.RunDestroyed += OnRunEnd;

            UsuLog.Verbose(
                logger,
                "[MISSION V2] CarryEquipment handler inicializado.");
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
            CharacterMaster self
        )
        {
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

        public static void ResetMission(
            string missionId
        )
        {
            if (string.IsNullOrWhiteSpace(missionId))
            {
                return;
            }


            string prefix =
                missionId.Trim() + ":";


            List<string> keysToRemove =
                new List<string>();


            foreach (string key in StateByObjective.Keys)
            {
                if (
                    key.StartsWith(
                        prefix,
                        StringComparison.Ordinal
                    )
                )
                {
                    keysToRemove.Add(key);
                }
            }


            foreach (string key in keysToRemove)
            {
                StateByObjective.Remove(key);
            }
        }


        private static void SetProgress(
            MissionObjectiveRuntimeBinding binding,
            CharacterMaster master,
            double value,
            string details
        )
        {
            // En progreso compartido, un inventario ajeno sin batería
            // no debe anular a un compañero que sí la transporta.
            // No fusionamos estados de los portadores: quien haya perdido
            // la batería o haya muerto sigue invalidado individualmente.
            if (binding?.Mission != null &&
                MissionProgressKeyBuilder.GetProgressScope(binding.Mission) ==
                    MissionProgressScope.Shared)
            {
                value = HasValidSharedCarrier(binding) ? 1d : 0d;
            }

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

        // Un objetivo Shared de transporte existe mientras AL MENOS UNO
        // de los jugadores lleve el equipo sin haber invalidado su intento.
        // Las muertes y pérdidas siguen siendo personales, no del equipo.
        private static bool HasValidSharedCarrier(
            MissionObjectiveRuntimeBinding binding
        )
        {
            if (binding?.Objective == null) return false;

            foreach (PlayerCharacterMasterController controller in
                PlayerCharacterMasterController.instances)
            {
                CharacterMaster candidate = controller?.master;
                if (candidate?.inventory == null ||
                    MissionPlayerIdentity.GetNetworkUser(candidate) == null)
                {
                    continue;
                }

                if (IsValidCarrier(binding, candidate)) return true;
            }

            return false;
        }

        // Disponible para la política Mixta de HUNK. Verifica la misma
        // condición que el objetivo Shared, pero por jugador y por ruta.
        internal static bool IsValidCarrier(
            MissionObjectiveRuntimeBinding binding,
            CharacterMaster master)
        {
            if (binding?.Objective == null || master?.inventory == null ||
                MissionPlayerIdentity.GetNetworkUser(master) == null ||
                !TryGetEquipmentIndex(
                    ReadEquipment(binding.Objective.Parameters),
                    out EquipmentIndex equipment))
                return false;

            CarryState state = GetState(binding, master);
            if (state.Invalidated ||
                master.inventory.currentEquipmentIndex != equipment)
                return false;

            CharacterBody body = master.GetBody();
            if (body?.healthComponent != null && !body.healthComponent.alive)
                return false;

            // Equipamiento detectado inicialmente por un stage start es válido,
            // pero un portador invalidado nunca recupera su elegibilidad.
            state.Acquired = true;
            return true;
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
