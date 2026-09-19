using System;
using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    public static class SurvivorChallengePresets
    {
        // =========================================================
        // MODO DE AUTORÍA
        // =========================================================
        // true sólo durante desarrollo local: las definiciones oficiales
        // pueden reescribir snapshots automáticos en Survivors.json.
        // En builds públicas debe permanecer false. Los snapshots oficiales
        // asignados se refrescan por BasePresetId mediante el sincronizador
        // normal, sin activar el modo de autoría.
        // =========================================================

        public static readonly bool AuthoringMode = false;

        /// <summary>
        /// Marcador neutral para un SurvivorChallengeJson cuya lógica real
        /// vive en Mission Schema v2.
        ///
        /// Antes, las misiones oficiales conservaban Type/Parameters
        /// de desafíos históricos como fallback. Eso hacía posible que, si
        /// un snapshot perdía Mission por cualquier motivo, el router legacy
        /// interpretara accidentalmente una misión antigua como vigente.
        ///
        /// Los presets legacy NO se eliminan: continúan en
        /// BuiltInMissionPresetCatalog con IsLegacy=true. Este marcador sólo
        /// evita que la asignación oficial nueva se haga pasar por uno de
        /// esos desafíos antiguos.
        /// </summary>
        public const string MissionV2ChallengeType = "MissionV2";


        private static MissionDefinition GetRuntimeMissionOrThrow(
            string presetId
        )
        {
            MissionPreset preset =
                BuiltInMissionPresetCatalog.Get(presetId);

            if (
                preset == null ||
                preset.IsLegacy ||
                !preset.RuntimeSupported ||
                preset.Mission == null ||
                preset.Mission.Routes == null ||
                preset.Mission.Routes.Count == 0
            )
            {
                throw new InvalidOperationException(
                    $"El preset oficial '{presetId}' no contiene una Mission v2 válida."
                );
            }

            // El catálogo usa factories: esta instancia es una copia
            // segura y no modifica el original del creador.
            return preset.Mission;
        }


        private static MissionConfiguration CreatePresetMissionConfiguration(
            string presetId
        )
        {
            return new MissionConfiguration
            {
                // Los presets oficiales preparados automáticamente siguen
                // siendo fallback. Si existe Original, Original conserva el
                // control hasta que el jugador elija USU explícitamente.
                Provider = UnlockProviderKind.USU,
                SelectionMode = UnlockSelectionMode.AutomaticFallback,
                Source = "Preset",
                BasePresetId = presetId,
                CustomMission = null
            };
        }


        private static SurvivorChallengeJson CreateOfficialPreset(
            string presetId
        )
        {
            if (
                !OfficialMissionPresetRegistry.TryGetByPresetId(
                    presetId,
                    out OfficialMissionPresetDescriptor descriptor
                ) ||
                descriptor == null
            )
            {
                throw new InvalidOperationException(
                    $"'{presetId}' no está registrado como misión oficial de USU."
                );
            }

            MissionPreset preset =
                BuiltInMissionPresetCatalog.Get(descriptor.PresetId);

            MissionDefinition mission =
                GetRuntimeMissionOrThrow(descriptor.PresetId);

            return new SurvivorChallengeJson
            {
                Enabled = true,
                Name = preset?.Name ?? "",
                LocalizationKey = descriptor.LocalizationKey,
                Description = preset?.Description ?? "",
                LunarCoinReward = descriptor.LunarCoinReward,

                // La lógica oficial pertenece exclusivamente a Mission v2.
                // Las recetas históricas siguen guardadas en la biblioteca,
                // pero ya no son un fallback implícito de este challenge.
                Type = MissionV2ChallengeType,
                Parameters = new JObject(),

                MissionConfig =
                    CreatePresetMissionConfiguration(
                        descriptor.PresetId
                    ),

                Mission = mission
            };
        }


        // =========================================================
        // API HISTÓRICA - SE CONSERVA PARA NO ROMPER LLAMADAS
        // =========================================================

        public static SurvivorChallengeJson CreateSoraPreset() =>
            CreateOfficialPreset(MissionPresetIds.SoraOfficial);

        public static SurvivorChallengeJson CreateRalseiPreset() =>
            CreateOfficialPreset(MissionPresetIds.RalseiOfficial);

        public static SurvivorChallengeJson CreateJhinPreset() =>
            CreateOfficialPreset(MissionPresetIds.JhinOfficial);

        public static SurvivorChallengeJson CreateScoutPreset() =>
            CreateOfficialPreset(MissionPresetIds.ScoutOfficial);

        public static SurvivorChallengeJson CreateSpyPreset() =>
            CreateOfficialPreset(MissionPresetIds.SpyOfficial);

        public static SurvivorChallengeJson CreateRocketPreset() =>
            CreateOfficialPreset(MissionPresetIds.RocketOfficial);

        public static SurvivorChallengeJson CreateHunkPreset() =>
            CreateOfficialPreset(MissionPresetIds.HunkOfficial);

        public static SurvivorChallengeJson CreateTinkatonPreset() =>
            CreateOfficialPreset(MissionPresetIds.TinkatonOfficial);

        public static SurvivorChallengeJson CreateWooperPreset() =>
            CreateOfficialPreset(MissionPresetIds.WooperOfficial);

        public static SurvivorChallengeJson CreateMinerPreset() =>
            CreateOfficialPreset(MissionPresetIds.MinerOfficial);


        // =========================================================
        // OBTENER PRESET OFICIAL PARA SURVIVOR CONOCIDO
        // =========================================================

        public static bool TryCreatePreset(
            string bodyName,
            string contentPackIdentifier,
            out SurvivorChallengeJson challenge
        )
        {
            challenge = null;

            if (
                !OfficialMissionPresetRegistry.TryResolve(
                    bodyName,
                    contentPackIdentifier,
                    out OfficialMissionPresetDescriptor descriptor
                ) ||
                descriptor == null
            )
            {
                return false;
            }

            challenge = CreateOfficialPreset(descriptor.PresetId);
            return true;
        }


        // =========================================================
        // COMPATIBILIDAD TEMPORAL / FUTURA
        // =========================================================
        // La migración de recetas legacy a CUSTOM/editor se hará en su
        // fase correspondiente. No convertirlas automáticamente aquí:
        // son plantillas históricas válidas que deben conservarse.
        // =========================================================

        public static bool TryMigrateLegacyPreset(
            string bodyName,
            string contentPackIdentifier,
            SurvivorChallengeJson currentChallenge,
            out SurvivorChallengeJson migratedChallenge
        )
        {
            migratedChallenge = null;
            return false;
        }
    }
}
