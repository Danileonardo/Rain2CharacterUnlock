using System;
using System.IO;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using Newtonsoft.Json;
using RoR2;
namespace UniversalSurvivorUnlocks
{
    internal static class CWTCampaignProgressStore
    {
        internal const double GroundGoal = 100000d;
        internal const double StageGoal = 10000d;
        internal const int BlastGoal = 10000;
        internal const int CloseGoal = 1000;
        internal const int BossGoal = 100;
        internal sealed class SaveData
        {
            public int SchemaVersion = 1;
            public double GroundMeters;
            public double AqueductMeters;
            public double DeltaMeters;
            public double AbyssMeters;
            public int ExplosiveHits;
            public int CloseKills;
            public int TeleporterBosses;
            public bool FinalScavengerComplete;
            public HashSet<string> ParticipantKeys = new HashSet<string>(StringComparer.Ordinal);
        }
        private static SaveData state = new SaveData();
        private static bool initialized, dirty, writable = true;
        private static ManualLogSource logger;
        internal static SaveData Current { get { Initialize(null); return state; } }
        internal static bool ObjectivesComplete =>
            state.GroundMeters >= GroundGoal && state.AqueductMeters >= StageGoal &&
            state.DeltaMeters >= StageGoal && state.AbyssMeters >= StageGoal &&
            state.ExplosiveHits >= BlastGoal && state.CloseKills >= CloseGoal &&
            state.TeleporterBosses >= BossGoal;
        private static string FilePath => System.IO.Path.Combine(Paths.ConfigPath,
            "UniversalSurvivorUnlocks", "CWTCampaign.json");
        private static double Clamp(double v, double max) =>
            double.IsNaN(v) || double.IsInfinity(v) ? 0d : Math.Max(0d, Math.Min(max, v));
        private static int Clamp(int v, int max) => Math.Max(0, Math.Min(max, v));
        internal static void Initialize(ManualLogSource log)
        {
            if (logger == null && log != null) logger = log;
            if (initialized) return;
            initialized = true;
            if (!File.Exists(FilePath)) return;
            try
            {
                var loaded = JsonConvert.DeserializeObject<SaveData>(File.ReadAllText(FilePath));
                if (loaded == null || loaded.SchemaVersion != 1)
                {
                    writable = false;
                    logger?.LogWarning("[CWT] Versión de guardado desconocida: no se sobrescribirá.");
                    return;
                }
                state = loaded;
                state.GroundMeters = Clamp(state.GroundMeters, GroundGoal);
                state.AqueductMeters = Clamp(state.AqueductMeters, StageGoal);
                state.DeltaMeters = Clamp(state.DeltaMeters, StageGoal);
                state.AbyssMeters = Clamp(state.AbyssMeters, StageGoal);
                state.ExplosiveHits = Clamp(state.ExplosiveHits, BlastGoal);
                state.CloseKills = Clamp(state.CloseKills, CloseGoal);
                state.TeleporterBosses = Clamp(state.TeleporterBosses, BossGoal);
                if (state.ParticipantKeys == null)
                    state.ParticipantKeys = new HashSet<string>(StringComparer.Ordinal);
            }
            catch (Exception ex)
            {
                writable = false;
                logger?.LogWarning("[CWT] Guardado ilegible; no se sobrescribirá: " + ex.Message);
            }
        }
        internal static bool RegisterContributor(CharacterMaster master)
        {
            if (master == null) return false;
            NetworkUser user = MissionPlayerIdentity.GetNetworkUser(master);
            MissionPlayerIdentity id;
            if (user == null || !MissionPlayerIdentity.TryCreate(user, out id) ||
                string.IsNullOrWhiteSpace(id.PersistentKey)) return false;
            if (state.ParticipantKeys.Add(id.PersistentKey)) dirty = true;
            return true;
        }
        internal static bool HasContributor(CharacterMaster master)
        {
            if (master == null || state.ParticipantKeys == null) return false;
            NetworkUser user = MissionPlayerIdentity.GetNetworkUser(master);
            MissionPlayerIdentity id;
            return user != null && MissionPlayerIdentity.TryCreate(user, out id) &&
                state.ParticipantKeys.Contains(id.PersistentKey);
        }
        internal static void AddGround(double meters, string scene)
        {
            if (meters <= 0d) return;
            state.GroundMeters = Clamp(state.GroundMeters + meters, GroundGoal);
            if (scene == "goolake") state.AqueductMeters = Clamp(state.AqueductMeters + meters, StageGoal);
            if (scene == "frozenwall") state.DeltaMeters = Clamp(state.DeltaMeters + meters, StageGoal);
            if (scene == "dampcavesimple") state.AbyssMeters = Clamp(state.AbyssMeters + meters, StageGoal);
            dirty = true;
        }
        internal static void AddBlastHits(int n) { if (n > 0 && state.ExplosiveHits < BlastGoal)
            { state.ExplosiveHits = Clamp(state.ExplosiveHits + n, BlastGoal); dirty = true; } }
        internal static void AddCloseKill() { if (state.CloseKills < CloseGoal) { state.CloseKills++; dirty = true; } }
        internal static void AddBoss() { if (state.TeleporterBosses < BossGoal) { state.TeleporterBosses++; dirty = true; } }
        internal static void CompleteFinal() { state.FinalScavengerComplete = true; dirty = true; Save(); }
        internal static void Save()
        {
            if (!dirty || !writable) return;
            string path = FilePath, temp = path + ".tmp";
            try
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
                File.WriteAllText(temp, JsonConvert.SerializeObject(state, Formatting.Indented));
                if (File.Exists(path)) File.Replace(temp, path, path + ".bak");
                else File.Move(temp, path);
                dirty = false;
            }
            catch (Exception ex) { logger?.LogWarning("[CWT] Error guardando campaña: " + ex.Message); }
        }
    }
}