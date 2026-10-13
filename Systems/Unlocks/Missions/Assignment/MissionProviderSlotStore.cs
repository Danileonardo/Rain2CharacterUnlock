using System;

using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Mantiene las fuentes dormidas que deben coexistir en la fila:
    /// Original - USU - Personalizada - Generada.
    ///
    /// Original vive fuera de MissionConfig. USU se recuerda por preset ID,
    /// Custom por CustomMission y Generated por GeneratedMissionSnapshot.
    /// </summary>
    public static class MissionProviderSlotStore
    {
        public static bool HasCustomMission(
            SurvivorChallengeJson challenge
        )
        {
            MissionConfiguration config =
                challenge?.MissionConfig;

            return
                IsValidMission(config?.CustomMission) ||
                IsValidMission(config?.CustomMissionSnapshot?.Mission);
        }


        public static bool HasGeneratedMission(
            SurvivorChallengeJson challenge
        )
        {
            MissionConfiguration config =
                challenge?.MissionConfig;

            if (config == null)
            {
                return false;
            }

            if (
                string.Equals(
                    config.Source,
                    "Generated",
                    StringComparison.OrdinalIgnoreCase
                ) &&
                IsValidMission(challenge.Mission)
            )
            {
                return true;
            }

            return IsValidGeneratedSnapshot(
                config.GeneratedMission
            );
        }


        public static bool IsGeneratedActiveSource(
            SurvivorChallengeJson challenge
        )
        {
            return
                challenge?.MissionConfig != null &&
                string.Equals(
                    challenge.MissionConfig.Source,
                    "Generated",
                    StringComparison.OrdinalIgnoreCase
                );
        }


        public static bool IsPresetActiveSource(
            SurvivorChallengeJson challenge
        )
        {
            return
                challenge?.MissionConfig != null &&
                string.Equals(
                    challenge.MissionConfig.Source,
                    "Preset",
                    StringComparison.OrdinalIgnoreCase
                );
        }


        public static bool IsCustomActiveSource(
            SurvivorChallengeJson challenge
        )
        {
            return
                challenge?.MissionConfig != null &&
                string.Equals(
                    challenge.MissionConfig.Source,
                    "Custom",
                    StringComparison.OrdinalIgnoreCase
                );
        }


        /// <summary>
        /// Recuerda el preset USU activo antes de abandonar Source=Preset.
        /// </summary>
        public static void CaptureActiveUsuPreset(
            SurvivorChallengeJson challenge
        )
        {
            MissionConfiguration config =
                challenge?.MissionConfig;

            if (
                config == null ||
                !string.Equals(
                    config.Source,
                    "Preset",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return;
            }

            string presetId =
                config.BasePresetId?.Trim() ?? "";

            if (
                string.IsNullOrWhiteSpace(presetId) ||
                presetId.StartsWith(
                    "generated.",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return;
            }

            config.UsuPresetId =
                presetId;
        }


        /// <summary>
        /// Captura la Generated activa en un slot dormido antes de cambiar a
        /// otra fuente. No toca una snapshot válida ya existente cuando la
        /// fuente activa no es Generated.
        /// </summary>
        public static void CaptureActiveGenerated(
            SurvivorChallengeJson challenge
        )
        {
            MissionConfiguration config =
                challenge?.MissionConfig;

            if (
                config == null ||
                !string.Equals(
                    config.Source,
                    "Generated",
                    StringComparison.OrdinalIgnoreCase
                ) ||
                !IsValidMission(challenge.Mission)
            )
            {
                return;
            }

            config.GeneratedMission =
                CreateGeneratedSnapshot(
                    challenge
                );
        }


        public static void CaptureActiveCustom(
            SurvivorChallengeJson challenge
        )
        {
            MissionConfiguration config =
                challenge?.MissionConfig;

            if (
                config == null ||
                !string.Equals(
                    config.Source,
                    "Custom",
                    StringComparison.OrdinalIgnoreCase
                ) ||
                !IsValidMission(challenge.Mission)
            )
            {
                return;
            }

            config.CustomMission =
                CloneMission(challenge.Mission);

            config.CustomMissionSnapshot =
                new CustomMissionSnapshot
                {
                    Enabled = challenge.Enabled,
                    Name = challenge.Name ?? "",
                    Description = challenge.Description ?? "",
                    LocalizationKey = challenge.LocalizationKey ?? "",
                    LunarCoinReward = challenge.LunarCoinReward,
                    Type = challenge.Type ?? "MissionV2",
                    Parameters = CloneJObject(challenge.Parameters),
                    Mission = CloneMission(challenge.Mission),
                    BasePresetId = config.BasePresetId ?? ""
                };
        }


        public static void CaptureActiveSlots(
            SurvivorChallengeJson challenge
        )
        {
            CaptureActiveUsuPreset(challenge);
            CaptureActiveGenerated(challenge);
            CaptureActiveCustom(challenge);
        }


        /// <summary>
        /// Restaura presentación + Mission V2 de una Custom dormida.
        /// </summary>
        public static bool RestoreCustom(
            SurvivorChallengeJson challenge
        )
        {
            MissionConfiguration config =
                challenge?.MissionConfig;

            if (challenge == null || config == null)
            {
                return false;
            }

            CustomMissionSnapshot snapshot =
                config.CustomMissionSnapshot;

            MissionDefinition mission =
                IsValidMission(config.CustomMission)
                    ? CloneMission(config.CustomMission)
                    : CloneMission(snapshot?.Mission);

            if (!IsValidMission(mission))
            {
                return false;
            }

            if (snapshot != null)
            {
                challenge.Enabled = snapshot.Enabled;
                challenge.Name = snapshot.Name ?? "";
                challenge.Description = snapshot.Description ?? "";
                challenge.LocalizationKey = snapshot.LocalizationKey ?? "";
                challenge.LunarCoinReward = snapshot.LunarCoinReward;
                challenge.Type = string.IsNullOrWhiteSpace(snapshot.Type)
                    ? "MissionV2"
                    : snapshot.Type;
                challenge.Parameters = CloneJObject(snapshot.Parameters);
            }
            else
            {
                challenge.Enabled = true;
                challenge.LocalizationKey = "";
                challenge.Type = "MissionV2";
            }

            challenge.Mission = mission;
            config.Provider = UnlockProviderKind.Custom;
            config.SelectionMode = UnlockSelectionMode.UserSelected;
            config.Source = "Custom";
            config.CustomMission = CloneMission(mission);

            if (
                snapshot != null &&
                !string.IsNullOrWhiteSpace(snapshot.BasePresetId)
            )
            {
                config.BasePresetId = snapshot.BasePresetId;
            }

            CaptureActiveCustom(challenge);
            return true;
        }


        /// <summary>
        /// Restaura la Generated dormida sobre el challenge activo, sin borrar
        /// CustomMission ni el preset USU recordado.
        /// </summary>
        public static bool RestoreGenerated(
            SurvivorChallengeJson challenge
        )
        {
            MissionConfiguration config =
                challenge?.MissionConfig;

            if (challenge == null || config == null)
            {
                return false;
            }

            GeneratedMissionSnapshot snapshot =
                config.GeneratedMission;

            if (
                !IsValidGeneratedSnapshot(snapshot)
            )
            {
                if (
                    string.Equals(
                        config.Source,
                        "Generated",
                        StringComparison.OrdinalIgnoreCase
                    ) &&
                    IsValidMission(challenge.Mission)
                )
                {
                    CaptureActiveGenerated(challenge);
                    snapshot = config.GeneratedMission;
                }
            }

            if (!IsValidGeneratedSnapshot(snapshot))
            {
                return false;
            }

            challenge.Enabled = snapshot.Enabled;
            challenge.Name = snapshot.Name ?? "";
            challenge.Description = snapshot.Description ?? "";
            challenge.LocalizationKey = snapshot.LocalizationKey ?? "";
            challenge.LunarCoinReward = snapshot.LunarCoinReward;
            challenge.Type = string.IsNullOrWhiteSpace(snapshot.Type)
                ? "GeneratedMission"
                : snapshot.Type;
            challenge.Parameters = CloneJObject(snapshot.Parameters);
            challenge.Mission = CloneMission(snapshot.Mission);

            config.Provider = UnlockProviderKind.USU;
            config.SelectionMode = UnlockSelectionMode.UserSelected;
            config.Source = "Generated";
            config.BasePresetId = snapshot.GeneratedId ?? "";

            // El slot Generated queda actualizado con una copia limpia de lo
            // que acabamos de restaurar. Custom/USU permanecen dormidos.
            config.GeneratedMission =
                CreateGeneratedSnapshot(challenge);

            GeneratedMissionPersistenceFingerprint
                .Stamp(challenge);

            return true;
        }


        /// <summary>
        /// Resuelve el preset USU recordado independientemente de la fuente
        /// activa. Compatibilidad: en JSON antiguos usa BasePresetId cuando no
        /// corresponde a una Generated.
        /// </summary>
        public static string ResolveRememberedUsuPresetId(
            SurvivorChallengeJson challenge
        )
        {
            MissionConfiguration config =
                challenge?.MissionConfig;

            if (config == null)
            {
                return "";
            }

            string explicitId =
                config.UsuPresetId?.Trim() ?? "";

            if (!string.IsNullOrWhiteSpace(explicitId))
            {
                return explicitId;
            }

            string baseId =
                config.BasePresetId?.Trim() ?? "";

            if (
                string.IsNullOrWhiteSpace(baseId) ||
                baseId.StartsWith(
                    "generated.",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return "";
            }

            return baseId;
        }


        public static void RememberUsuPresetId(
            SurvivorChallengeJson challenge,
            string presetId
        )
        {
            MissionConfiguration config =
                challenge?.MissionConfig;

            string normalized =
                presetId?.Trim() ?? "";

            if (
                config == null ||
                string.IsNullOrWhiteSpace(normalized)
            )
            {
                return;
            }

            config.UsuPresetId = normalized;
        }


        public static GeneratedMissionSnapshot CreateGeneratedSnapshot(
            SurvivorChallengeJson challenge
        )
        {
            if (
                challenge == null ||
                !IsValidMission(challenge.Mission)
            )
            {
                return null;
            }

            string generatedId =
                challenge.MissionConfig?.BasePresetId?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(generatedId))
            {
                generatedId =
                    challenge.Parameters?
                        .Value<string>("generatedMissionId")?
                        .Trim()
                    ?? "";
            }

            return new GeneratedMissionSnapshot
            {
                Enabled = challenge.Enabled,
                Name = challenge.Name ?? "",
                Description = challenge.Description ?? "",
                LocalizationKey = challenge.LocalizationKey ?? "",
                LunarCoinReward = challenge.LunarCoinReward,
                Type = challenge.Type ?? "GeneratedMission",
                Parameters = CloneJObject(challenge.Parameters),
                Mission = CloneMission(challenge.Mission),
                GeneratedId = generatedId
            };
        }


        private static bool IsValidGeneratedSnapshot(
            GeneratedMissionSnapshot snapshot
        )
        {
            return
                snapshot != null &&
                IsValidMission(snapshot.Mission);
        }


        private static bool IsValidMission(
            MissionDefinition mission
        )
        {
            return
                mission != null &&
                mission.Routes != null &&
                mission.Routes.Count > 0;
        }


        private static MissionDefinition CloneMission(
            MissionDefinition mission
        )
        {
            if (mission == null)
            {
                return null;
            }

            return
                JToken
                    .FromObject(mission)
                    .ToObject<MissionDefinition>();
        }


        private static JObject CloneJObject(
            JObject value
        )
        {
            if (value == null)
            {
                return new JObject();
            }

            return
                JToken
                    .FromObject(value)
                    .ToObject<JObject>()
                ?? new JObject();
        }
    }
}
