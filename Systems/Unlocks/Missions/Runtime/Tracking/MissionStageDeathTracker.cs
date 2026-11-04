using System;
using System.Collections.Generic;
using BepInEx.Logging;
using RoR2;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    /// <summary>Muerte por jugador, reiniciada al cambiar de sector.</summary>
    internal static class MissionStageDeathTracker
    {
        private static bool initialized;
        private static ManualLogSource logger;
        private static readonly HashSet<string> DeadThisStage =
            new HashSet<string>(StringComparer.Ordinal);

        internal static void Initialize(ManualLogSource log)
        {
            if (initialized) return;
            initialized = true;
            logger = log;
            MissionRunLifecycle.RunStarted += OnRun;
            MissionRunLifecycle.RunDestroyed += OnRun;
            MissionStageRuntimeTracker.StageStarted += OnStage;
            GameplayEventHub.CharacterDeath += OnDeath;
        }
        private static void OnRun(Run run) { DeadThisStage.Clear(); }
        private static void OnStage(MissionStageEventContext context) { DeadThisStage.Clear(); }

        private static void OnDeath(DamageReport report)
        {
            if (!NetworkServer.active || Run.instance == null || report?.victimMaster == null)
                return;
            NetworkUser user = MissionPlayerIdentity.GetNetworkUser(report.victimMaster);
            MissionPlayerIdentity identity;
            if (user == null || !MissionPlayerIdentity.TryCreate(user, out identity)) return;
            if (DeadThisStage.Add(identity.Key) && UsuLog.VerboseEnabled)
                UsuLog.Verbose(logger, "[MISSION SCOUT AUDIT] Muerte personal | Jugador: " +
                    user.userName + " | Identity: " + identity.Key +
                    " | NoDeath: false solo para este jugador");
        }

        internal static bool HasNotDied(MissionEventContext context)
        {
            CharacterMaster master = context?.PlayerMaster ?? context?.PlayerBody?.master;
            MissionPlayerIdentity identity;
            return master != null && MissionPlayerIdentity.TryCreate(master, out identity)
                && !DeadThisStage.Contains(identity.Key);
        }
    }
}
