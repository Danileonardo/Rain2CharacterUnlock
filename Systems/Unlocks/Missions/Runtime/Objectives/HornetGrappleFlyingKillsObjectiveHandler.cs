using System;
using System.Collections.Generic;
using BepInEx.Logging;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Tras un enganche de Loader confirmado mediante su estado de balanceo,
    /// suma bajas de criaturas voladoras realmente suspendidas en Siren's Call.
    /// Aterrizar cierra sólo la ventana activa, nunca el total de la run.
    /// </summary>
    public static class HornetGrappleFlyingKillsObjectiveHandler
    {
        private const string ObjectiveType = "HornetGrappleFlyingKills";
        private static bool initialized;
        private static readonly Dictionary<CharacterMaster, float> PendingHook =
            new Dictionary<CharacterMaster, float>();
        private static readonly HashSet<CharacterMaster> HookedInAir =
            new HashSet<CharacterMaster>();
        // Lista conservadora de enemigos voladores nativos: nunca contar
        // enemigos terrestres solo porque hayan sido lanzados por el aire.
        private static readonly HashSet<string> FlyerBodies =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
                "VultureBody", "JellyfishBody", "WispBody", "GreaterWispBody",
                "LunarWispBody", "RoboBallMiniBody", "RoboBallBossBody",
                "FlyingVerminBody", "VoidJailerBody", "VoidBarnacleBody",
                "GipBody", "SolusAmalgamatorBody"
            };

        public static void Initialize(ManualLogSource logger)
        {
            if (initialized) return;
            initialized = true;
            GameplayEventHub.SkillActivated += OnSkillActivated;
            GameplayEventHub.FixedUpdateTick += TrackHookState;
            GameplayEventHub.CharacterDeath += OnDeath;
            MissionRunLifecycle.RunStarted += Reset;
            MissionRunLifecycle.RunDestroyed += Reset;
            MissionStageRuntimeTracker.StageStarted += _ => { PendingHook.Clear(); HookedInAir.Clear(); };
        }

        private static void Reset(Run _) { PendingHook.Clear(); HookedInAir.Clear(); }
        private static bool Active => NetworkServer.active && Run.instance != null &&
            MissionProgressRegistry.IsRunActive &&
            MissionRuntimeActivityPlan.IsTypeActive(ObjectiveType) &&
            MissionRuntimeCatalog.HasObjectiveType(ObjectiveType);
        private static bool IsLoader(CharacterBody body) =>
            body != null && body.bodyIndex == BodyCatalog.FindBodyIndex("LoaderBody") &&
            body.master != null && MissionPlayerIdentity.GetNetworkUser(body.master) != null;
        private static bool InSirenCall() => string.Equals(
            MissionStageRuntimeTracker.CurrentStageName, "shipgraveyard",
            StringComparison.OrdinalIgnoreCase);

        private static void OnSkillActivated(CharacterBody body, GenericSkill skill)
        {
            if (!Active || !InSirenCall() || !IsLoader(body) ||
                body.skillLocator == null || skill != body.skillLocator.secondary) return;
            // Disparo de gancho: todavía NO se reconoce un agarre válido.
            PendingHook[body.master] = Time.fixedTime;
        }

        private static void TrackHookState()
        {
            if (!Active) return;
            if (!InSirenCall()) { PendingHook.Clear(); HookedInAir.Clear(); return; }
            var expired = new List<CharacterMaster>();
            foreach (var pair in PendingHook)
            {
                CharacterBody body = pair.Key?.GetBody();
                if (body == null || body.characterMotor == null || body.characterMotor.isGrounded ||
                    Time.fixedTime - pair.Value > 4f)
                {
                    expired.Add(pair.Key);
                    continue;
                }
                // Ambos ganchos (Grapple Fist / Spiked Fist) llevan a un
                // estado de balanceo de Loader solo después de conectarse.
                // No basta haber pulsado la secundaria ni estar saltando.
                EntityStateMachine[] machines = body.GetComponents<EntityStateMachine>();
                for (int i = 0; i < machines.Length; i++)
                {
                    object state = machines[i]?.state;
                    string name = state?.GetType().FullName ?? "";
                    if (name.IndexOf("Loader", StringComparison.OrdinalIgnoreCase) >= 0 &&
                        (name.IndexOf("Swing", StringComparison.OrdinalIgnoreCase) >= 0 ||
                         name.IndexOf("Grappl", StringComparison.OrdinalIgnoreCase) >= 0) &&
                        name.IndexOf("Fire", StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        HookedInAir.Add(pair.Key);
                        expired.Add(pair.Key);
                        break;
                    }
                }
            }
            foreach (CharacterMaster master in expired) PendingHook.Remove(master);
            var landed = new List<CharacterMaster>();
            foreach (CharacterMaster master in HookedInAir)
            {
                CharacterBody b = master?.GetBody();
                if (b?.characterMotor == null || b.characterMotor.isGrounded ||
                    !IsLoader(b)) landed.Add(master);
            }
            foreach (CharacterMaster master in landed) HookedInAir.Remove(master);
        }

        private static bool IsFlyingNow(CharacterBody enemy)
        {
            if (enemy == null || enemy.teamComponent == null ||
                enemy.teamComponent.teamIndex != TeamIndex.Monster) return false;
            string bodyName = BodyCatalog.GetBodyName(enemy.bodyIndex);
            if (!FlyerBodies.Contains(bodyName)) return false;
            if (enemy.characterMotor != null && enemy.characterMotor.isGrounded) return false;
            // Un Buitre de aleación posado en el suelo NO es una baja aérea.
            // Este filtro físico se aplica también a enemigos flotantes en reposo.
            return !Physics.Raycast(enemy.corePosition, Vector3.down, 2f,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        }

        private static void OnDeath(DamageReport report)
        {
            if (!Active || !InSirenCall() || report == null ||
                !IsFlyingNow(report.victimBody) || report.attackerMaster == null) return;
            CharacterMaster killer = PlayerOwnerResolver.ResolveOwningPlayerMaster(report.attackerMaster);
            CharacterBody loader = killer?.GetBody();
            if (!IsLoader(loader) || loader.characterMotor == null ||
                loader.characterMotor.isGrounded || !HookedInAir.Contains(killer)) return;
            foreach (var binding in MissionRuntimeCatalog.GetObjectives(ObjectiveType))
            {
                if (binding?.Mission == null || binding.Objective == null ||
                    !string.Equals(binding.BodyName, "HornetBody", StringComparison.OrdinalIgnoreCase)) continue;
                var context = new MissionEventContext {
                    PlayerMaster = killer, PlayerBody = loader, ActionBody = loader,
                    TargetBody = report.victimBody, EventType = ObjectiveType
                };
                var result = MissionCompositionProgressEvaluator.AddProgress(
                    binding.MissionId, binding.Mission, binding.RouteIndex,
                    binding.ObjectiveIndex, context, 1d);
                GenericMissionDispatcher.HandleProgressResult(binding, result,
                    "Gancho confirmado + Loader aérea + objetivo volador", killedBody: report.victimBody);
            }
        }
    }
}
