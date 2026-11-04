using System;
using System.Collections.Generic;
using BepInEx.Logging;
using RoR2;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Solo el HOST contabiliza un impacto real de Blinding Assault
    /// sobre una víctima con MercExpose ANTES de consumir el debuff.
    /// Un mismo enemigo no puede contarse dos veces durante la run.
    /// Una habilidad diferente, daño de item/proc y golpes de aliados
    /// no dan progreso a Mercenary.
    /// </summary>
    public static class ExposeDashHitsObjectiveHandler
    {
        private const string ObjectiveType = "ExposeDashHits";
        private static bool initialized;
        private static ManualLogSource logger;
        private static readonly Dictionary<string, HashSet<int>> SeenByOwner =
            new Dictionary<string, HashSet<int>>(StringComparer.Ordinal);

        public static void Initialize(ManualLogSource log)
        {
            if (initialized) return;
            initialized = true;
            logger = log;
            On.RoR2.HealthComponent.TakeDamageProcess += OnTakeDamage;
            MissionRunLifecycle.RunStarted += Reset;
            MissionRunLifecycle.RunDestroyed += Reset;
        }

        private static void Reset(Run run) { SeenByOwner.Clear(); }

        private static bool IsExposed(CharacterBody victim)
        {
            BuffIndex[] buffs = victim?.activeBuffsList;
            if (buffs == null) return false;
            for (int i = 0; i < buffs.Length; i++)
            {
                BuffDef def = BuffCatalog.GetBuffDef(buffs[i]);
                if (def != null &&
                    string.Equals(def.name, "MercExpose", StringComparison.OrdinalIgnoreCase) &&
                    victim.GetBuffCount(def) > 0)
                    return true;
            }
            return false;
        }

        private static bool IsBlindingAssault(CharacterBody merc)
        {
            if (merc?.skillLocator?.utility?.skillDef == null)
                return false;
            // Focused Assault, the alternative utility, cannot qualify.
            string skillToken = merc.skillLocator.utility.skillDef.skillNameToken;
            if (!string.Equals(skillToken, "MERC_UTILITY_NAME", StringComparison.OrdinalIgnoreCase))
                return false;

            EntityStateMachine[] machines = merc.GetComponents<EntityStateMachine>();
            for (int i = 0; i < machines.Length; i++)
            {
                Type type = machines[i]?.state?.GetType();
                if (type == null || !string.Equals(type.Namespace, "EntityStates.Merc", StringComparison.Ordinal))
                    continue;
                if (type.Name == "Assaulter" || type.Name == "Assaulter2")
                    return true;
            }
            return false;
        }

        private static void OnTakeDamage(
            On.RoR2.HealthComponent.orig_TakeDamageProcess orig,
            HealthComponent self, DamageInfo hit)
        {
            if (!NetworkServer.active || Run.instance == null ||
                !MissionProgressRegistry.IsRunActive || self == null || hit == null ||
                !MissionRuntimeActivityPlan.IsTypeActive(ObjectiveType) ||
                !MissionRuntimeCatalog.HasObjectiveType(ObjectiveType))
            {
                orig(self, hit);
                return;
            }

            // Snapshot antes de que TakeDamageProcess retire Expose.
            CharacterBody victim = self.body;
            CharacterBody attacker = hit.attacker != null
                ? hit.attacker.GetComponent<CharacterBody>() : null;
            bool candidate = victim != null && attacker != null &&
                victim.teamComponent != null && attacker.teamComponent != null &&
                victim.teamComponent.teamIndex != attacker.teamComponent.teamIndex &&
                attacker.bodyIndex == BodyCatalog.FindBodyIndex("MercBody") &&
                IsExposed(victim) && IsBlindingAssault(attacker) &&
                hit.damage > 0f && !hit.rejected;

            // La fuente de verdad del daño permanece siempre intacta.
            orig(self, hit);

            if (!candidate || hit.rejected || attacker == null || victim == null)
                return;
            CharacterMaster master = attacker.master;
            if (master == null || MissionPlayerIdentity.GetNetworkUser(master) == null)
                return;

            IReadOnlyList<MissionObjectiveRuntimeBinding> bindings =
                MissionRuntimeCatalog.GetObjectives(ObjectiveType);
            for (int i = 0; i < bindings.Count; i++)
            {
                MissionObjectiveRuntimeBinding binding = bindings[i];
                if (binding?.Mission == null || binding.Objective == null) continue;
                string itemName = binding.Objective.Parameters?.Value<string>("requiredItem") ?? "Dagger";
                int quantity = binding.Objective.Parameters?.Value<int?>("requiredItemCount") ?? 1;
                ItemIndex item = ItemCatalog.FindItemIndex(itemName);
                if (item == ItemIndex.None || attacker.inventory == null ||
                    attacker.inventory.GetItemCountPermanent(item) < Math.Max(1, quantity))
                    continue;

                // InstanceID distingue enemigos concretos (no especies).
                string key = binding.MissionId + ":" + binding.RouteIndex + ":" +
                             binding.ObjectiveIndex + ":" + master.GetInstanceID();
                HashSet<int> seen;
                if (!SeenByOwner.TryGetValue(key, out seen))
                {
                    seen = new HashSet<int>();
                    SeenByOwner[key] = seen;
                }
                int targetId = victim.GetInstanceID();
                if (seen.Contains(targetId)) continue;

                MissionEventContext context = new MissionEventContext
                {
                    PlayerMaster = master,
                    PlayerBody = attacker,
                    ActionBody = attacker,
                    TargetBody = victim,
                    EventType = ObjectiveType
                };
                MissionCompositionProgressResult progress =
                    MissionCompositionProgressEvaluator.AddProgress(
                        binding.MissionId, binding.Mission,
                        binding.RouteIndex, binding.ObjectiveIndex, context, 1d);
                if (progress == null || !progress.Accepted) continue;
                seen.Add(targetId);
                GenericMissionDispatcher.HandleProgressResult(binding, progress,
                    "Expose + Blinding Assault + Ceremonial Dagger");
            }
        }
    }
}
