using System;
using System.Collections.Generic;
using System.Globalization;
using BepInEx.Logging;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Sólo AH-64. Acumulación duradera de vuelo, balas, misiles y jefes.
    /// Ningún otro objetivo/progreso Mission V2 se modifica.
    /// </summary>
    internal static class AH64CampaignTracker
    {
        private const string Body = "AH64Body";
        private static ManualLogSource logger;
        private static bool initialized;
        private static float lastTickTime;
        private static float nextSaveTime;
        private static bool campaignActiveForRun;
        private static readonly Dictionary<CharacterMaster,float> airborneTime =
            new Dictionary<CharacterMaster,float>();

        internal static bool IsSpanish =>
            (Language.currentLanguageName ?? "").StartsWith("es", StringComparison.OrdinalIgnoreCase);

        internal static void Initialize(ManualLogSource log)
        {
            if (initialized) return;
            initialized = true;
            logger = log;
            AH64CampaignProgressStore.Initialize(log);
            MissionRunLifecycle.RunStarted += OnRunStarted;
            MissionRunLifecycle.RunDestroyed += OnRunDestroyed;
            GameplayEventHub.FiveHzTick += Tick;
            GameplayEventHub.CharacterDeath += OnDeath;
            On.RoR2.BulletAttack.Fire += OnBulletFire;
            On.RoR2.Projectile.ProjectileController.Start += OnProjectileStart;
            Run.onServerGameOver += OnGameOver;
            logger?.LogInfo("[AH64 CAMPAIGN] Tracker persistente inicializado.");
        }

        private static bool IsActive()
        {
            return campaignActiveForRun && NetworkServer.active && Run.instance != null;
        }

        private static bool IsMissionSelected()
        {
            if (!NetworkServer.active) return false;
            if (!SessionMissionRegistry.TryGetEffectiveEntry(Body,
                out SurvivorJsonEntry entry) || entry?.Challenge == null ||
                !entry.Challenge.Enabled ||
                !SurvivorUnlockManager.RequiresCustomUnlock(entry)) return false;
            return string.Equals(entry.Challenge.MissionConfig?.BasePresetId,
                MissionPresetIds.AH64Official, StringComparison.OrdinalIgnoreCase);
        }

        private static void OnRunStarted(Run run)
        {
            // El host congela la selección del lobby al comenzar la run.
            // Este único acceso evita deserializar la misión con cada disparo.
            campaignActiveForRun = IsMissionSelected();
            airborneTime.Clear();
            lastTickTime = Time.unscaledTime;
            nextSaveTime = Time.unscaledTime + 10f;
            AH64CampaignProgressStore.Initialize(logger);
        }
        private static void OnRunDestroyed(Run run)
        {
            AH64CampaignProgressStore.Save();
            campaignActiveForRun = false;
            airborneTime.Clear();
        }

        private static bool IsPlayerAirborne(CharacterBody body)
        {
            // Los drones no cuentan como jugadores. Se exige estar
            // realmente fuera de tierra, no simplemente pulsar Salto.
            if (body == null || body.master == null ||
                !PlayerOwnerResolver.IsPlayerMaster(body.master)) return false;
            CharacterMotor motor = body.characterMotor;
            if (motor != null && motor.isGrounded) return false;
            // Distancia libre inferior: evita saltitos junto al suelo.
            // Si se usa una plataforma móvil, el detector es conservador.
            RaycastHit hit;
            if (Physics.Raycast(body.corePosition, Vector3.down,
                out hit, 3f, LayerIndex.world.mask)) return false;
            return true;
        }

        private static void Tick()
        {
            if (!IsActive()) return;
            float now = Time.unscaledTime;
            float elapsed = Mathf.Clamp(now - lastTickTime, 0f, 0.35f);
            lastTickTime = now;
            if (AH64CampaignProgressStore.Current.FlightSeconds <
                AH64CampaignProgressStore.FlightGoal)
            {
                foreach (PlayerCharacterMasterController controller in
                    PlayerCharacterMasterController.instances)
                {
                    CharacterMaster master = controller?.master;
                    CharacterBody body = master?.GetBody();
                    if (master == null || body == null) continue;
                    if (!IsPlayerAirborne(body))
                    {
                        airborneTime.Remove(master);
                        continue;
                    }
                    float prev = 0f;
                    airborneTime.TryGetValue(master, out prev);
                    airborneTime[master] = prev + elapsed;
                    // Dos segundos de vuelo continuo validan el tramo.
                    if (prev >= 2f)
                        AH64CampaignProgressStore.AddFlight(elapsed);
                }
            }
            if (now >= nextSaveTime)
            {
                nextSaveTime = now + 10f;
                AH64CampaignProgressStore.Save();
            }
        }

        private static void OnBulletFire(On.RoR2.BulletAttack.orig_Fire orig,
            BulletAttack bullet)
        {
            // Invocamos siempre el original, sin alterar balística.
            if (IsActive() && bullet != null && bullet.owner != null)
            {
                CharacterBody body = bullet.owner.GetComponent<CharacterBody>();
                if (IsPlayerAirborne(body))
                {
                    // Se cuentan disparos/proyectiles, no daños infligidos.
                    int shots = Math.Max(1, Math.Min(100, (int)bullet.bulletCount));
                    AH64CampaignProgressStore.AddBullets(shots);
                }
            }
            orig(bullet);
        }

        private static void OnProjectileStart(
            On.RoR2.Projectile.ProjectileController.orig_Start orig,
            RoR2.Projectile.ProjectileController projectile)
        {
            orig(projectile);
            if (!IsActive() || projectile == null || projectile.owner == null) return;
            string name = (projectile.gameObject.name ?? "").ToLowerInvariant();
            if (!(name.Contains("missile") || name.Contains("rocket") ||
                  name.Contains("hellfire") || name.Contains("longbow") ||
                  name.Contains("hydra"))) return;
            CharacterBody body = projectile.owner.GetComponent<CharacterBody>();
            if (IsPlayerAirborne(body))
                AH64CampaignProgressStore.AddMissiles(1);
        }

        private static void OnDeath(DamageReport report)
        {
            if (!IsActive() || report?.victimBody == null ||
                !ContentRuntimeContextTracker.IsTeleporterBoss(report.victimBody))
                return;
            CharacterMaster master = PlayerOwnerResolver.ResolveOwningPlayerMaster(
                report.attackerMaster);
            CharacterBody body = master?.GetBody();
            if (IsPlayerAirborne(body)) AH64CampaignProgressStore.AddBoss();
        }

        private static void OnGameOver(Run run, GameEndingDef ending)
        {
            if (!IsActive() || ending == null ||
                AH64CampaignProgressStore.Current.FinalEscapeComplete ||
                !AH64CampaignProgressStore.ObjectivesComplete ||
                !ending.isWin ||
                (ending.cachedName ?? "").IndexOf("obliter",
                    StringComparison.OrdinalIgnoreCase) >= 0 ||
                run.selectedDifficulty < DifficultyIndex.Hard)
                return;

            // Sólo la misión oficial efectiva puede conceder el unlock.
            AH64CampaignProgressStore.CompleteFinal();
            logger?.LogInfo("[AH64 CAMPAIGN] Protocolo 64 completado, " +
                "concesión mediante el sistema nativo de USU.");
            SessionUnlockManager.CompleteMission(Body);
        }

        private static string Bar(float count, float max, int segments = 18)
        {
            int filled = (int)Math.Floor(segments * Math.Max(0,
                Math.Min(1, count / max)));
            return "<color=#77C7F7>" + new string('■', filled) + "</color>" +
                   "<color=#777777>" + new string('□', segments-filled) + "</color>";
        }
        private static string Row(string name, double current, int required, bool time = false)
        {
            int n = Math.Min(required, Math.Max(0, (int)Math.Floor(current)));
            string value = time
                ? (Math.Min(current, required) / 60d).ToString("0.0", CultureInfo.InvariantCulture) +
                    "/" + (required / 60).ToString(CultureInfo.InvariantCulture) + " min"
                : n.ToString("N0", CultureInfo.InvariantCulture) + "/" +
                    required.ToString("N0", CultureInfo.InvariantCulture);
            string check = n >= required ? " <color=#78DE97>✓</color>" : "";
            return "<b>" + name + "</b>  " + value + check + "\n" +
                Bar(n, required) + "\n";
        }
        internal static string BuildProgressText()
        {
            var s = AH64CampaignProgressStore.Current;
            string text = Row(IsSpanish ? "Tiempo de vuelo" : "Flight time",
                s.FlightSeconds, (int)AH64CampaignProgressStore.FlightGoal, true) + "\n" +
                Row(IsSpanish ? "Balas disparadas en vuelo" : "Airborne bullet shots",
                    s.BulletShots, AH64CampaignProgressStore.BulletGoal) + "\n" +
                Row(IsSpanish ? "Misiles y cohetes lanzados en vuelo" : "Airborne missile/rocket launches",
                    s.MissilesLaunched, AH64CampaignProgressStore.MissileGoal) + "\n" +
                Row(IsSpanish ? "Jefes del teletransportador (en vuelo)" : "Airborne teleporter bosses",
                    s.TeleporterBosses, AH64CampaignProgressStore.BossGoal) + "\n" +
                "<b>" + (IsSpanish ? "Operación final: escapar en Monzón" :
                    "Final operation: escape on Monsoon") + "</b>  " +
                (s.FinalEscapeComplete ? "<color=#78DE97>✓</color>" :
                    (AH64CampaignProgressStore.ObjectivesComplete ?
                        (IsSpanish ? "Disponible" : "Ready") :
                        (IsSpanish ? "Bloqueada" : "Locked")));
            text += "\n<color=#A7A7A7>" + (IsSpanish ?
                "Progreso acumulativo. No se reinicia al terminar una partida. " +
                "Cuenta acciones del servidor/host." :
                "Persistent across runs. Counts server/host actions.") + "</color>";
            return text;
        }
    }
}
