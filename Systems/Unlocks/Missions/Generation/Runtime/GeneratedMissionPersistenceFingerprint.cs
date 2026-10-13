using System;

using BepInEx.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Marca el snapshot exacto que G8 escribió para una misión Generated.
    ///
    /// Si el jugador modifica ese bloque directamente en Survivors.json,
    /// el siguiente Sync detecta la diferencia y lo promueve a Custom antes
    /// de que el generador pueda sobrescribirlo. Así Generated y Custom usan
    /// el mismo JSON sin perder las ediciones del jugador.
    /// </summary>
    public static class GeneratedMissionPersistenceFingerprint
    {
        private const string SnapshotHashKey =
            "generatedSnapshotHash";


        public static void Stamp(
            SurvivorChallengeJson challenge
        )
        {
            if (challenge == null)
            {
                return;
            }

            if (challenge.Parameters == null)
            {
                challenge.Parameters =
                    new JObject();
            }

            challenge.Parameters[SnapshotHashKey] =
                Calculate(challenge);
        }


        public static bool TryPromoteEditedGeneratedToCustom(
            SurvivorChallengeJson challenge,
            string bodyName,
            ManualLogSource logger = null
        )
        {
            MissionConfiguration config =
                challenge?.MissionConfig;

            if (
                challenge == null ||
                config == null ||
                !string.Equals(
                    config.Source,
                    "Generated",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return false;
            }

            string expected =
                challenge.Parameters?
                    .Value<string>(SnapshotHashKey)?
                    .Trim()
                ?? "";

            // Generated antiguas de una build previa a Phase 1.5 no tienen
            // sello. Se dejan al generador para migración automática.
            if (string.IsNullOrWhiteSpace(expected))
            {
                return false;
            }

            string actual =
                Calculate(challenge);

            if (
                string.Equals(
                    expected,
                    actual,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return false;
            }

            MissionDefinition editedMission =
                CloneMission(challenge.Mission);

            if (
                editedMission == null ||
                editedMission.Routes == null ||
                editedMission.Routes.Count == 0
            )
            {
                logger?.LogWarning(
                    "[GENERATED CUSTOM] Se detectaron cambios manuales en " +
                    $"{bodyName}, pero Mission V2 no es válida. " +
                    "Se conserva Source=Generated para evitar perder el unlock."
                );

                return false;
            }

            config.Provider =
                UnlockProviderKind.Custom;

            config.SelectionMode =
                UnlockSelectionMode.UserSelected;

            config.Source =
                "Custom";

            config.CustomMission =
                CloneMission(editedMission);

            challenge.Type =
                "MissionV2";

            challenge.LocalizationKey =
                "";

            challenge.Parameters?.Remove(
                SnapshotHashKey
            );

            logger?.LogInfo(
                "[GENERATED CUSTOM] Edición detectada y preservada como Custom | " +
                $"Body: {bodyName} | BaseGenerated: {config.BasePresetId ?? ""}"
            );

            return true;
        }


        public static string Calculate(
            SurvivorChallengeJson challenge
        )
        {
            if (challenge == null)
            {
                return "";
            }

            JObject json =
                JObject.FromObject(challenge);

            if (json["parameters"] is JObject parameters)
            {
                parameters.Remove(
                    SnapshotHashKey
                );
            }

            // Los slots dormidos no forman parte del contenido Generated
            // activo. Cambiar de provider, guardar una Custom o recordar un
            // preset USU no debe interpretarse como edición manual de la
            // Generated actual.
            if (json["missionConfig"] is JObject missionConfig)
            {
                missionConfig.Remove("provider");
                missionConfig.Remove("selectionMode");
                missionConfig.Remove("usuPresetId");
                missionConfig.Remove("customMission");
                missionConfig.Remove("customMissionSnapshot");
                missionConfig.Remove("generatedMission");
            }

            // RoR2/BepInEx puede cargar una versión de Newtonsoft.Json
            // más antigua que no expone JToken.ToString(Formatting).
            // JsonConvert.SerializeObject(object, Formatting) sí forma parte
            // de la API disponible en el runtime del juego y produce el mismo
            // JSON compacto que necesitamos para el fingerprint.
            string material =
                JsonConvert.SerializeObject(
                    json,
                    Formatting.None
                );

            return
                DeterministicMissionGenerator
                    .StableHash(material)
                    .ToString("x8");
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
                JObject
                    .FromObject(mission)
                    .ToObject<MissionDefinition>();
        }
    }
}
