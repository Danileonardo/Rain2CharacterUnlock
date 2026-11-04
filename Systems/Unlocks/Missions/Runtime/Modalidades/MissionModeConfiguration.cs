using System;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// La ubicación en carpetas es organizativa. El modo se registra
    /// de manera explícita en cada MissionDefinition y viaja en snapshots
    /// host/cliente. No se infiere de los nombres de archivo.
    /// </summary>
    public enum MissionUnlockMode
    {
        Cooperative,
        Individual,
        Mixed
    }

    public static class MissionModeConfiguration
    {
        public static MissionUnlockMode GetMode(MissionDefinition mission)
        {
            string scope = mission?.RewardScope?.Trim();
            if (string.Equals(scope, "Individual", StringComparison.OrdinalIgnoreCase))
                return MissionUnlockMode.Individual;
            if (string.Equals(scope, "Mixed", StringComparison.OrdinalIgnoreCase))
                return MissionUnlockMode.Mixed;
            // Legacy, Custom, Generated y Cooperativa conservan Session.
            return MissionUnlockMode.Cooperative;
        }

        public static MissionPreset Individual(MissionPreset preset)
        {
            if (preset?.Mission != null)
            {
                preset.Mission.ProgressScope = "PerPlayer";
                preset.Mission.RewardScope = "Individual";
            }
            return preset;
        }

        public static MissionPreset Cooperative(MissionPreset preset)
        {
            if (preset?.Mission != null)
            {
                preset.Mission.ProgressScope = "Shared";
                preset.Mission.RewardScope = "Session";
            }
            return preset;
        }

        // Mixto: metas comunes + elegibilidad personal registrada por el host.
        // Nunca difundir la recompensa de sesión a los espectadores.
        public static MissionPreset Mixed(MissionPreset preset)
        {
            if (preset?.Mission != null)
            {
                preset.Mission.ProgressScope = "Shared";
                preset.Mission.RewardScope = "Mixed";
            }
            return preset;
        }
    }
}
