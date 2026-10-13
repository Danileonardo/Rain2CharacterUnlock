using System;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using Newtonsoft.Json;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Estado de la campaña AH-64, independiente de las misiones por run.
    /// Archivo propio: BepInEx/config/UniversalSurvivorUnlocks/AH64Campaign.json
    /// Nunca altera Survivors.json ni el estado de ningún otro personaje.
    /// El host es la autoridad de esta primera implementación: los clientes
    /// no deben sobrescribir el guardado local del host.
    /// </summary>
    internal static class AH64CampaignProgressStore
    {
        internal const float FlightGoal = 64f * 60f;
        internal const int BulletGoal = 6400;
        internal const int MissileGoal = 640;
        internal const int BossGoal = 64;

        internal sealed class SaveData
        {
            public int SchemaVersion = 1;
            public float FlightSeconds;
            public int BulletShots;
            public int MissilesLaunched;
            public int TeleporterBosses;
            public bool FinalEscapeComplete;
        }

        private static ManualLogSource logger;
        private static SaveData state = new SaveData();
        private static bool initialized;
        private static bool dirty;

        internal static SaveData Current
        {
            get { Initialize(null); return state; }
        }

        private static string FilePath
        {
            get { return Path.Combine(Paths.ConfigPath,
                "UniversalSurvivorUnlocks", "AH64Campaign.json"); }
        }

        internal static void Initialize(ManualLogSource log)
        {
            if (logger == null && log != null) logger = log;
            if (initialized) return;
            initialized = true;
            string file = FilePath;
            if (!File.Exists(file)) return;
            try
            {
                SaveData loaded = JsonConvert.DeserializeObject<SaveData>(
                    File.ReadAllText(file));
                if (loaded == null || loaded.SchemaVersion != 1)
                {
                    logger?.LogWarning("[AH64 CAMPAIGN] Guardado no compatible; " +
                        "se conserva el archivo sin sobrescribirlo.");
                    writable = false;
                    return;
                }
                // Datos corruptos o desproporcionados jamás liberan objetivos.
                state = loaded;
                state.FlightSeconds = Clamp(loaded.FlightSeconds, FlightGoal);
                state.BulletShots = Clamp(loaded.BulletShots, BulletGoal);
                state.MissilesLaunched = Clamp(loaded.MissilesLaunched, MissileGoal);
                state.TeleporterBosses = Clamp(loaded.TeleporterBosses, BossGoal);
                logger?.LogInfo("[AH64 CAMPAIGN] Progreso entre partidas cargado.");
            }
            catch (Exception ex)
            {
                writable = false;
                logger?.LogWarning("[AH64 CAMPAIGN] JSON ilegible, no se sobrescribirá: " + ex.Message);
            }
        }

        private static bool writable = true;
        private static float Clamp(float value, float max)
        { return float.IsNaN(value) || float.IsInfinity(value) ? 0f : Math.Max(0f, Math.Min(max, value)); }
        private static int Clamp(int value, int max)
        { return Math.Max(0, Math.Min(max, value)); }

        internal static bool ObjectivesComplete =>
            state.FlightSeconds >= FlightGoal &&
            state.BulletShots >= BulletGoal &&
            state.MissilesLaunched >= MissileGoal &&
            state.TeleporterBosses >= BossGoal;

        internal static bool AddFlight(float seconds)
        {
            if (seconds <= 0 || state.FlightSeconds >= FlightGoal) return false;
            state.FlightSeconds = Clamp(state.FlightSeconds + seconds, FlightGoal);
            dirty = true; return true;
        }
        internal static bool AddBullets(int bullets)
        {
            if (bullets <= 0 || state.BulletShots >= BulletGoal) return false;
            state.BulletShots = Clamp(state.BulletShots + bullets, BulletGoal);
            dirty = true; return true;
        }
        internal static bool AddMissiles(int missiles)
        {
            if (missiles <= 0 || state.MissilesLaunched >= MissileGoal) return false;
            state.MissilesLaunched = Clamp(state.MissilesLaunched + missiles, MissileGoal);
            dirty = true; return true;
        }
        internal static bool AddBoss()
        {
            if (state.TeleporterBosses >= BossGoal) return false;
            state.TeleporterBosses++;
            dirty = true; return true;
        }
        internal static void CompleteFinal()
        {
            state.FinalEscapeComplete = true;
            dirty = true;
            Save();
        }
        internal static void Save()
        {
            if (!dirty || !writable) return;
            string path = FilePath;
            string temp = path + ".tmp";
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(temp, JsonConvert.SerializeObject(state, Formatting.Indented));
                if (File.Exists(path))
                {
                    // Sustitución atómica NTFS, recuperable con .bak.
                    File.Replace(temp, path, path + ".bak");
                }
                else File.Move(temp, path);
                dirty = false;
            }
            catch (Exception ex)
            {
                logger?.LogWarning("[AH64 CAMPAIGN] No se pudo persistir: " + ex.Message);
            }
        }
    }
}
