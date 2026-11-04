using System;
using System.Collections.Generic;
using BepInEx.Logging;
using RoR2;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    /// <summary>Counts one distinct victim per player/run while MUL-T is in its Transport dash state.</summary>
    public static class SonicTransportHitsObjectiveHandler
    {
        private const string ObjectiveType = "SonicTransportHits";
        private static bool initialized;
        private static readonly Dictionary<string, HashSet<int>> Seen = new Dictionary<string, HashSet<int>>();
        public static void Initialize(ManualLogSource log)
        {
            if (initialized) return;
            initialized = true;
            On.RoR2.HealthComponent.TakeDamageProcess += OnTakeDamage;
            MissionRunLifecycle.RunStarted += Reset;
            MissionRunLifecycle.RunDestroyed += Reset;
        }
        private static void Reset(Run run) { Seen.Clear(); }
        private static bool IsTransportDash(CharacterBody body)
        {
            if (body == null) return false;
            var machines = body.GetComponents<EntityStateMachine>();
            foreach (var machine in machines)
            {
                string state = machine?.state?.GetType().FullName ?? "";
                // Vanilla MUL-T Transport Mode / ToolbotDash, never other mobility skills.
                if (state.IndexOf("Toolbot", StringComparison.OrdinalIgnoreCase) >= 0 &&
                    (state.IndexOf("ToolbotDash", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     state.IndexOf("Transport", StringComparison.OrdinalIgnoreCase) >= 0)) return true;
            }
            return false;
        }
        private static void OnTakeDamage(On.RoR2.HealthComponent.orig_TakeDamageProcess orig,
            HealthComponent self, DamageInfo hit)
        {
            CharacterBody attacker = hit?.attacker != null ? hit.attacker.GetComponent<CharacterBody>() : null;
            CharacterBody victim = self?.body;
            bool valid = NetworkServer.active && Run.instance != null && MissionProgressRegistry.IsRunActive &&
                MissionRuntimeActivityPlan.IsTypeActive(ObjectiveType) && MissionRuntimeCatalog.HasObjectiveType(ObjectiveType) &&
                attacker != null && attacker.master != null && victim != null &&
                attacker.bodyIndex == BodyCatalog.FindBodyIndex("ToolbotBody") &&
                MissionPlayerIdentity.GetNetworkUser(attacker.master) != null && IsTransportDash(attacker) &&
                attacker.teamComponent != null && victim.teamComponent != null &&
                attacker.teamComponent.teamIndex != victim.teamComponent.teamIndex &&
                hit.damage > 0f && !hit.rejected;
            orig(self, hit);
            if (!valid || hit.rejected) return;
            foreach (var binding in MissionRuntimeCatalog.GetObjectives(ObjectiveType))
            {
                if (binding?.Mission == null || binding.Objective == null ||
                    !string.Equals(binding.BodyName, "SonicBody", StringComparison.OrdinalIgnoreCase)) continue;
                string key = binding.MissionId + ":" + binding.RouteIndex + ":" +
                    binding.ObjectiveIndex + ":" + attacker.master.GetInstanceID();
                if (!Seen.TryGetValue(key, out var seen)) { seen = new HashSet<int>(); Seen[key] = seen; }
                int id = victim.GetInstanceID();
                if (seen.Contains(id)) continue;
                var context = new MissionEventContext { PlayerMaster = attacker.master,
                    PlayerBody = attacker, ActionBody = attacker, TargetBody = victim, EventType = ObjectiveType };
                var result = MissionCompositionProgressEvaluator.AddProgress(binding.MissionId,
                    binding.Mission, binding.RouteIndex, binding.ObjectiveIndex, context, 1d);
                if (result == null || !result.Accepted) continue;
                seen.Add(id);
                GenericMissionDispatcher.HandleProgressResult(binding, result,
                    "MUL-T Transport Mode: distinct enemy hit");
            }
        }
    }
}
