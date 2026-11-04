using System;
using System.Collections.Generic;
using System.Globalization;
using BepInEx.Logging;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
namespace UniversalSurvivorUnlocks
{
    /// <summary>Campaña terrestre independiente: host autoritativo y persistencia propia.</summary>
    internal static class CWTCampaignTracker
    {
        private static ManualLogSource logger;
        private static bool initialized, active;
        private static float lastTick, nextSave;
        private sealed class PositionState { public Vector3 Position; public bool Grounded; }
        private static readonly Dictionary<CharacterMaster, PositionState> positions =
            new Dictionary<CharacterMaster, PositionState>();
        private static readonly HashSet<CharacterMaster> contributorsThisRun =
            new HashSet<CharacterMaster>();
        internal static bool IsSpanish =>
            (Language.currentLanguageName ?? "").StartsWith("es", StringComparison.OrdinalIgnoreCase);
        internal static void Initialize(ManualLogSource log)
        {
            if (initialized) return;
            initialized = true;
            logger = log;
            CWTCampaignProgressStore.Initialize(log);
            MissionRunLifecycle.RunStarted += OnRunStarted;
            MissionRunLifecycle.RunDestroyed += OnRunDestroyed;
            GameplayEventHub.FiveHzTick += Tick;
            GameplayEventHub.CharacterDeath += OnDeath;
            On.RoR2.BlastAttack.Fire += OnBlastFire;
        }
        private static bool IsMissionSelected()
        {
            if (!NetworkServer.active || !CWTCampaignIdentity.Detected) return false;
            SurvivorJsonEntry entry;
            return SessionMissionRegistry.TryGetEffectiveEntry(CWTCampaignIdentity.BodyName, out entry) &&
                entry?.Challenge != null && entry.Challenge.Enabled &&
                SurvivorUnlockManager.RequiresCustomUnlock(entry) &&
                string.Equals(entry.Challenge.MissionConfig?.BasePresetId,
                    MissionPresetIds.CelestialWarTankOfficial, StringComparison.OrdinalIgnoreCase);
        }
        private static bool IsActive() => active && NetworkServer.active && Run.instance != null;
        private static void OnRunStarted(Run run)
        {
            active = IsMissionSelected();
            positions.Clear(); contributorsThisRun.Clear();
            lastTick = Time.unscaledTime; nextSave = lastTick + 10f;
        }
        private static void OnRunDestroyed(Run run)
        {
            CWTCampaignProgressStore.Save();
            active = false;
            positions.Clear(); contributorsThisRun.Clear();
        }
        private static bool IsGrounded(CharacterBody body) =>
            body != null && body.characterMotor != null && body.characterMotor.isGrounded;
        private static void Record(CharacterMaster master)
        {
            if (master == null || !PlayerOwnerResolver.IsPlayerMaster(master)) return;
            if (CWTCampaignProgressStore.RegisterContributor(master))
                contributorsThisRun.Add(master);
        }
        private static void Tick()
        {
            if (!IsActive()) return;
            float now = Time.unscaledTime;
            lastTick = now;
            string scene = SceneManager.GetActiveScene().name.ToLowerInvariant();
            foreach (PlayerCharacterMasterController controller in PlayerCharacterMasterController.instances)
            {
                CharacterMaster master = controller?.master;
                CharacterBody body = master?.GetBody();
                if (master == null || body == null) continue;
                Vector3 here = body.corePosition;
                bool grounded = IsGrounded(body);
                PositionState p;
                if (positions.TryGetValue(master, out p) && grounded && p.Grounded)
                {
                    float dist = Vector3.Distance(here, p.Position);
                    // Teleportaciones o reposicionamientos bruscos no cuentan.
                    if (dist >= 0.1f && dist <= 20f)
                    {
                        Record(master);
                        CWTCampaignProgressStore.AddGround(dist, scene);
                    }
                }
                positions[master] = new PositionState { Position = here, Grounded = grounded };
            }
            if (now >= nextSave) { nextSave = now + 10f; CWTCampaignProgressStore.Save(); }
        }
        private static BlastAttack.Result OnBlastFire(On.RoR2.BlastAttack.orig_Fire orig, BlastAttack blast)
        {
            // Fire siempre se ejecuta una sola vez. Se contabilizan impactos reales,
            // NO disparos, efectos visuales ni simples lanzamientos de proyectiles.
            BlastAttack.Result result = orig(blast);
            if (!IsActive() || blast == null || result.hitCount <= 0 || blast.attacker == null) return result;
            CharacterBody attacker = blast.attacker.GetComponent<CharacterBody>();
            CharacterMaster master = PlayerOwnerResolver.ResolveOwningPlayerMaster(attacker?.master);
            if (master == null) return result;
            Record(master);
            CWTCampaignProgressStore.AddBlastHits(result.hitCount);
            return result;
        }
        private static void OnDeath(DamageReport report)
        {
            if (!IsActive() || report?.victimBody == null) return;
            CharacterMaster master = PlayerOwnerResolver.ResolveOwningPlayerMaster(report.attackerMaster);
            if (master == null) return;
            CharacterBody player = master.GetBody();
            if (player == null) return;
            if (report.victimBody.teamComponent != null &&
                player.teamComponent != null &&
                report.victimBody.teamComponent.teamIndex == player.teamComponent.teamIndex) return;
            Record(master);
            if (Vector3.Distance(player.corePosition, report.victimBody.corePosition) <= 12f)
                CWTCampaignProgressStore.AddCloseKill();
            if (ContentRuntimeContextTracker.IsTeleporterBoss(report.victimBody))
                CWTCampaignProgressStore.AddBoss();
            // Final estricto: Carroñero NORMAL con mochila; el retorcido lunar
            // no sustituye a este objetivo. El final debe estar ya habilitado.
            string victimName = (report.victimBody.name ?? "").ToLowerInvariant();
            bool normalScavenger = victimName.StartsWith("scavbody") ||
                victimName.StartsWith("scavengerbody");
            if (!normalScavenger || !CWTCampaignProgressStore.ObjectivesComplete ||
                Run.instance.selectedDifficulty < DifficultyIndex.Hard) return;
            List<NetworkUser> eligible = new List<NetworkUser>();
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (PlayerCharacterMasterController controller in PlayerCharacterMasterController.instances)
            {
                CharacterMaster ally = controller?.master;
                if (ally == null || !contributorsThisRun.Contains(ally) ||
                    !CWTCampaignProgressStore.HasContributor(ally)) continue;
                CharacterBody b = ally.GetBody();
                if (b == null || Vector3.Distance(b.corePosition, report.victimBody.corePosition) > 90f) continue;
                NetworkUser user = MissionPlayerIdentity.GetNetworkUser(ally);
                MissionPlayerIdentity id;
                if (user?.gameObject == null || !MissionPlayerIdentity.TryCreate(user, out id) ||
                    !seen.Add(id.Key)) continue;
                eligible.Add(user);
            }
            if (eligible.Count == 0) return;
            CWTCampaignProgressStore.CompleteFinal();
            foreach (NetworkUser user in eligible)
                SessionUnlockManager.CompleteMissionForPlayer(CWTCampaignIdentity.BodyName, user);
            logger?.LogInfo("[CWT] Campaña completada; Billy desbloqueado para " + eligible.Count + " contribuyentes.");
        }
        internal static string BuildProgressText()
        {
            var s = CWTCampaignProgressStore.Current;
            string t = string.Format(CultureInfo.InvariantCulture,
                "{0:0.0}/100 km (Acueducto {1:0.0}/10, Delta {2:0.0}/10, Abisales {3:0.0}/10)\n" +
                "{4:N0}/10,000 impactos explosivos\n{5:N0}/1,000 bajas cercanas\n" +
                "{6:N0}/100 jefes del teletransportador\n",
                s.GroundMeters/1000d,s.AqueductMeters/1000d,s.DeltaMeters/1000d,
                s.AbyssMeters/1000d,s.ExplosiveHits,s.CloseKills,s.TeleporterBosses);
            return t + (s.FinalScavengerComplete ? "✓ " : (CWTCampaignProgressStore.ObjectivesComplete ? "Disponible: " : "Bloqueado: ")) +
                (IsSpanish ? "Carroñero normal en Monzón" : "Normal Scavenger on Monsoon");
        }
    }
}