using System;
using System.Collections.Generic;
using BepInEx.Logging;
using Newtonsoft.Json.Linq;
using RoR2;
using UnityEngine.Networking;
namespace UniversalSurvivorUnlocks
{
    /// <summary>Conservative damage attribution: only recognized primary projectiles while Dual Wield is active.</summary>
    public static class ShadowPowerWeaponHitsObjectiveHandler
    {
        private const string ObjectiveType = "ShadowPowerWeaponHits";
        private static bool initialized;
        private static readonly Dictionary<string, HashSet<int>> Seen = new Dictionary<string, HashSet<int>>();
        public static void Initialize(ManualLogSource log)
        {
            if (initialized) return;
            initialized = true;
            On.RoR2.HealthComponent.TakeDamageProcess += OnDamage;
            MissionRunLifecycle.RunStarted += Reset;
            MissionRunLifecycle.RunDestroyed += Reset;
        }
        private static void Reset(Run run) { Seen.Clear(); }
        private static bool InPowerMode(CharacterBody body)
        {
            if (body == null) return false;
            foreach (var machine in body.GetComponents<EntityStateMachine>())
            {
                string state = machine?.state?.GetType().FullName ?? "";
                if (state.IndexOf("Toolbot", StringComparison.OrdinalIgnoreCase) >= 0 &&
                    (state.IndexOf("DualWield", StringComparison.OrdinalIgnoreCase) >= 0 ||
                     state.IndexOf("PowerMode", StringComparison.OrdinalIgnoreCase) >= 0)) return true;
            }
            return false;
        }
        private static string Weapon(DamageInfo info)
        {
            // Names are a conservative fallback and MUST be verified in a live log.
            string name = info?.inflictor != null ? info.inflictor.name ?? "" : "";
            if (name.IndexOf("Rebar", StringComparison.OrdinalIgnoreCase) >= 0) return "rebar";
            if (name.IndexOf("Nail", StringComparison.OrdinalIgnoreCase) >= 0) return "nail";
            return ""; // Do not miscredit procs, unidentified hits or other weapons.
        }
        private static void OnDamage(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo info)
        {
            CharacterBody attacker = info?.attacker != null ? info.attacker.GetComponent<CharacterBody>() : null;
            CharacterBody victim = self?.body;
            string weapon = Weapon(info);
            bool valid = NetworkServer.active && Run.instance != null && MissionProgressRegistry.IsRunActive &&
                MissionRuntimeActivityPlan.IsTypeActive(ObjectiveType) && MissionRuntimeCatalog.HasObjectiveType(ObjectiveType) &&
                attacker != null && attacker.master != null && victim != null &&
                attacker.bodyIndex == BodyCatalog.FindBodyIndex("ToolbotBody") &&
                MissionPlayerIdentity.GetNetworkUser(attacker.master) != null && InPowerMode(attacker) &&
                attacker.teamComponent != null && victim.teamComponent != null &&
                attacker.teamComponent.teamIndex != victim.teamComponent.teamIndex &&
                info.damage > 0f && !info.rejected && weapon.Length > 0;
            orig(self, info);
            if (!valid || info.rejected) return;
            foreach (var binding in MissionRuntimeCatalog.GetObjectives(ObjectiveType))
            {
                if (binding?.Mission == null || binding.Objective == null ||
                    !string.Equals(binding.BodyName, "ShadowBody", StringComparison.OrdinalIgnoreCase)) continue;
                string required = binding.Objective.Parameters?.Value<string>("weapon") ?? "";
                if (!string.Equals(required, weapon, StringComparison.OrdinalIgnoreCase)) continue;
                string key = binding.MissionId + ":" + binding.RouteIndex + ":" +
                    binding.ObjectiveIndex + ":" + attacker.master.GetInstanceID();
                if (!Seen.TryGetValue(key, out var seen)) { seen = new HashSet<int>(); Seen[key] = seen; }
                int id = victim.GetInstanceID();
                if (seen.Contains(id)) continue;
                var ctx = new MissionEventContext { PlayerMaster = attacker.master,
                    PlayerBody = attacker, ActionBody = attacker, TargetBody = victim, EventType = ObjectiveType };
                var result = MissionCompositionProgressEvaluator.AddProgress(binding.MissionId,
                    binding.Mission, binding.RouteIndex, binding.ObjectiveIndex, ctx, 1d);
                if (result == null || !result.Accepted) continue;
                seen.Add(id);
                GenericMissionDispatcher.HandleProgressResult(binding, result, "MUL-T Power Mode weapon hit");
            }
        }
    }
}
