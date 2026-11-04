using System;
using System.Collections.Generic;
using BepInEx.Logging;
using RoR2;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Bayonetta unlock: Commando kills within 4 seconds of his utility
    /// dodge/slide, -2 when taking a valid hit. Progress is per player/run.
    /// Server authoritative. No perfect-dodge requirement or DLC items.
    /// </summary>
    public static class BayonettaDodgeKillsObjectiveHandler
    {
        private const string ObjectiveType = "BayonettaDodgeKills";
        private static readonly Dictionary<CharacterMaster, float> DodgeTimes =
            new Dictionary<CharacterMaster, float>();
        private static bool initialized;
        private static ManualLogSource logger;

        public static void Initialize(ManualLogSource log)
        {
            if (initialized) return;
            initialized = true;
            logger = log;
            GameplayEventHub.SkillActivated += OnSkillActivated;
            GameplayEventHub.CharacterDeath += OnEnemyKilled;
            On.RoR2.HealthComponent.TakeDamageProcess += OnTakeDamage;
            MissionRunLifecycle.RunStarted += Reset;
            MissionRunLifecycle.RunDestroyed += Reset;
        }

        private static void Reset(Run unused) { DodgeTimes.Clear(); }

        private static bool IsMissionActive()
        {
            return NetworkServer.active && Run.instance != null &&
                MissionProgressRegistry.IsRunActive &&
                MissionRuntimeActivityPlan.IsTypeActive(ObjectiveType) &&
                MissionRuntimeCatalog.HasObjectiveType(ObjectiveType);
        }

        private static bool IsPlayerCommando(CharacterBody body)
        {
            return body != null && body.master != null &&
                body.bodyIndex == BodyCatalog.FindBodyIndex("CommandoBody") &&
                MissionPlayerIdentity.GetNetworkUser(body.master) != null;
        }

        private static void OnSkillActivated(CharacterBody body, GenericSkill skill)
        {
            if (!IsMissionActive() || !IsPlayerCommando(body) ||
                body.skillLocator == null || skill == null ||
                skill != body.skillLocator.utility)
                return;
            // Commando utility is Tactical Dive (roll) or Tactical Slide.
            DodgeTimes[body.master] = Run.instance.GetRunStopwatch();
        }

        private static void OnEnemyKilled(DamageReport report)
        {
            if (!IsMissionActive() || report == null || report.victimBody == null ||
                report.victimBody.teamComponent == null ||
                report.victimBody.teamComponent.teamIndex != TeamIndex.Monster)
                return;
            CharacterBody attacker = report.attackerBody;
            if (!IsPlayerCommando(attacker)) return;
            CharacterMaster owner = attacker.master;
            if (!DodgeTimes.TryGetValue(owner, out float start)) return;
            float age = Run.instance.GetRunStopwatch() - start;
            if (age < 0f) return;
            Apply(owner, attacker, false, age);
        }

        private static void OnTakeDamage(
            On.RoR2.HealthComponent.orig_TakeDamageProcess orig,
            HealthComponent self, DamageInfo info)
        {
            // The game's damage calculation must always occur unchanged.
            orig(self, info);
            if (!IsMissionActive() || self == null || info == null ||
                info.rejected || info.damage <= 0f ||
                !IsPlayerCommando(self.body))
                return;
            Apply(self.body.master, self.body, true, 0f);
        }

        private static void Apply(CharacterMaster owner, CharacterBody commando,
            bool tookDamage, float ageSinceDodge)
        {
            if (owner == null || commando == null) return;
            var bindings = MissionRuntimeCatalog.GetObjectives(ObjectiveType);
            for (int i = 0; i < bindings.Count; i++)
            {
                var binding = bindings[i];
                if (binding == null || binding.Mission == null || binding.Objective == null ||
                    !string.Equals(binding.BodyName, "BayoBody", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (MissionProgressRegistry.IsMissionCompleted(
                    MissionProgressScope.PerPlayer,
                    MissionPlayerIdentity.GetNetworkUser(owner), binding.MissionId))
                    continue;

                float window = (float)(binding.Objective.Parameters?
                    .Value<double?>("secondsAfterDodge") ?? 4d);
                if (!tookDamage && ageSinceDodge > window) continue;
                double penalty = binding.Objective.Parameters?
                    .Value<double?>("damagePenalty") ?? 2d;

                var context = new MissionEventContext
                {
                    PlayerMaster = owner,
                    PlayerBody = commando,
                    ActionBody = commando,
                    TargetBody = commando,
                    EventType = ObjectiveType
                };

                // SetProgress on penalties clamps to zero and avoids negative scores.
                // Unique storage IDs and scopes remain owned by Mission v2.
                MissionCompositionProgressResult result;
                if (tookDamage)
                {
                    // The current score is maintained by the mission engine.
                    // Negative AddProgress is allowed; the evaluator then
                    // clamps it in this custom handler through SetProgress.
                    string progressId = binding.StorageObjectiveId;
                    var user = MissionPlayerIdentity.GetNetworkUser(owner);
                    // An existing route with a single objective uses this ID.
                    double current = MissionProgressRegistry.GetProgress(
                        MissionProgressScope.PerPlayer, user,
                        binding.MissionId, progressId);
                    double next = Math.Max(0d, current - Math.Max(0d, penalty));
                    result = MissionCompositionProgressEvaluator.SetProgress(
                        binding.MissionId, binding.Mission,
                        binding.RouteIndex, binding.ObjectiveIndex, context, next);
                }
                else
                {
                    result = MissionCompositionProgressEvaluator.AddProgress(
                        binding.MissionId, binding.Mission,
                        binding.RouteIndex, binding.ObjectiveIndex, context, 1d);
                }

                GenericMissionDispatcher.HandleProgressResult(binding, result,
                    tookDamage ? "Bayonetta: daño recibido (-2)" :
                                  "Bayonetta: baja tras esquiva (+1)");
            }
        }
    }
}
