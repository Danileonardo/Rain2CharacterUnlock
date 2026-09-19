using System;
using System.Collections.Generic;

using BepInEx.Logging;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Validación final de la autoridad de misiones integrada en USU.
    ///
    /// No modifica configuración, providers ni runtime. Su objetivo es detectar
    /// regresiones estructurales después de la limpieza: una misión oficial
    /// perdida/duplicada, un Legacy expuesto como asignable, un descriptor que
    /// ya no coincide con su Definition o una excepción Original ↔ USU que se
    /// haya extendido accidentalmente a otro survivor.
    /// </summary>
    public static class MissionAuthorityIntegrityValidator
    {
        private const int ExpectedOfficialCount = 10;
        private const int ExpectedLegacyCount = 12;

        public static bool Validate(
            ManualLogSource logger,
            IReadOnlyList<MissionPreset> official,
            IReadOnlyList<MissionPreset> legacy
        )
        {
            if (logger == null)
            {
                return false;
            }

            bool valid = true;

            List<OfficialMissionPresetDescriptor> descriptors =
                OfficialMissionPresetRegistry.GetAll();

            if (descriptors.Count != ExpectedOfficialCount)
            {
                valid = false;
                logger.LogWarning(
                    "[MISSION AUTHORITY] Cantidad oficial inesperada | " +
                    $"Esperada: {ExpectedOfficialCount} | Registry: {descriptors.Count}"
                );
            }

            if (official == null || official.Count != ExpectedOfficialCount)
            {
                valid = false;
                logger.LogWarning(
                    "[MISSION AUTHORITY] Biblioteca oficial incompleta | " +
                    $"Esperada: {ExpectedOfficialCount} | Visible: {official?.Count ?? 0}"
                );
            }

            if (legacy == null || legacy.Count != ExpectedLegacyCount)
            {
                valid = false;
                logger.LogWarning(
                    "[MISSION AUTHORITY] Cantidad Legacy inesperada | " +
                    $"Esperada: {ExpectedLegacyCount} | Encontrada: {legacy?.Count ?? 0}"
                );
            }

            HashSet<string> officialPresetIds =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            HashSet<string> officialBodies =
                new HashSet<string>(StringComparer.Ordinal);

            HashSet<string> officialBindings =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            int directUsuAlternatives = 0;

            for (int i = 0; i < descriptors.Count; i++)
            {
                OfficialMissionPresetDescriptor descriptor = descriptors[i];

                if (descriptor == null)
                {
                    valid = false;
                    logger.LogWarning(
                        "[MISSION AUTHORITY] Descriptor oficial null."
                    );
                    continue;
                }

                string presetId = descriptor.PresetId?.Trim() ?? "";
                string bodyName = descriptor.BodyName?.Trim() ?? "";
                string source = descriptor.ContentPackIdentifier?.Trim() ?? "";
                string localizationKey = descriptor.LocalizationKey?.Trim() ?? "";

                if (
                    string.IsNullOrWhiteSpace(presetId) ||
                    string.IsNullOrWhiteSpace(bodyName) ||
                    string.IsNullOrWhiteSpace(source) ||
                    string.IsNullOrWhiteSpace(localizationKey)
                )
                {
                    valid = false;
                    logger.LogWarning(
                        "[MISSION AUTHORITY] Descriptor oficial incompleto | " +
                        $"Body: {bodyName} | Preset: {presetId} | Source: {source}"
                    );
                    continue;
                }

                if (!officialPresetIds.Add(presetId))
                {
                    valid = false;
                    logger.LogWarning(
                        "[MISSION AUTHORITY] Preset oficial duplicado | " +
                        presetId
                    );
                }

                if (!officialBodies.Add(bodyName))
                {
                    valid = false;
                    logger.LogWarning(
                        "[MISSION AUTHORITY] Body oficial duplicado | " +
                        bodyName
                    );
                }

                string bindingKey = bodyName + "|" + source;
                if (!officialBindings.Add(bindingKey))
                {
                    valid = false;
                    logger.LogWarning(
                        "[MISSION AUTHORITY] Binding Body/Source duplicado | " +
                        bindingKey
                    );
                }

                if (
                    descriptor.LunarCoinReward < 0 ||
                    descriptor.LunarCoinReward > 10
                )
                {
                    valid = false;
                    logger.LogWarning(
                        "[MISSION AUTHORITY] Recompensa fuera de rango | " +
                        $"Body: {bodyName} | Reward: {descriptor.LunarCoinReward}"
                    );
                }

                MissionPreset preset =
                    BuiltInMissionPresetCatalog.Get(presetId);

                bool presetValid =
                    preset != null &&
                    MissionPresetLibraryService.IsAssignable(preset) &&
                    !preset.IsLegacy &&
                    string.Equals(
                        preset.Category,
                        MissionPresetCategories.CharacterRecipe,
                        StringComparison.OrdinalIgnoreCase
                    ) &&
                    string.Equals(
                        preset.TargetBody ?? "",
                        bodyName,
                        StringComparison.Ordinal
                    );

                if (!presetValid)
                {
                    valid = false;
                    logger.LogWarning(
                        "[MISSION AUTHORITY] Misión oficial inválida o desalineada | " +
                        $"Body: {bodyName} | Preset: {presetId}"
                    );
                }
                else if (!ValidateVisibleDescription(preset, logger))
                {
                    valid = false;
                }

                if (!HasMatchingDefinition(bodyName, source))
                {
                    valid = false;
                    logger.LogWarning(
                        "[MISSION AUTHORITY] Descriptor sin Definition coincidente | " +
                        $"Body: {bodyName} | Source: {source}"
                    );
                }

                if (!ValidatePreparedChallenge(descriptor, logger))
                {
                    valid = false;
                }

                if (descriptor.AllowDirectUsuWithOriginal)
                {
                    directUsuAlternatives++;

                    if (
                        !string.Equals(
                            bodyName,
                            "MinerBody",
                            StringComparison.Ordinal
                        ) ||
                        !string.Equals(
                            presetId,
                            MissionPresetIds.MinerOfficial,
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    {
                        valid = false;
                        logger.LogWarning(
                            "[MISSION AUTHORITY] Excepción Original ↔ USU inesperada | " +
                            $"Body: {bodyName} | Preset: {presetId}"
                        );
                    }
                }
            }

            if (directUsuAlternatives != 1)
            {
                valid = false;
                logger.LogWarning(
                    "[MISSION AUTHORITY] Excepciones directas Original ↔ USU inesperadas | " +
                    $"Esperadas: 1 (Miner) | Encontradas: {directUsuAlternatives}"
                );
            }

            if (!ValidateLegacyIsolation(legacy, logger))
            {
                valid = false;
            }

            if (valid)
            {
                logger.LogInfo(
                    "[MISSION AUTHORITY] Integridad final OK | " +
                    $"Oficiales: {ExpectedOfficialCount} | " +
                    $"Legacy: {ExpectedLegacyCount} | " +
                    "Original ↔ USU directo: Miner"
                );
            }
            else
            {
                logger.LogWarning(
                    "[MISSION AUTHORITY] Integridad final INCOMPLETA. " +
                    "Revisar warnings anteriores antes de continuar."
                );
            }

            return valid;
        }

        private static bool ValidatePreparedChallenge(
            OfficialMissionPresetDescriptor descriptor,
            ManualLogSource logger
        )
        {
            try
            {
                if (
                    !SurvivorChallengePresets.TryCreatePreset(
                        descriptor.BodyName,
                        descriptor.ContentPackIdentifier,
                        out SurvivorChallengeJson challenge
                    ) ||
                    challenge == null
                )
                {
                    logger.LogWarning(
                        "[MISSION AUTHORITY] No se pudo preparar challenge oficial | " +
                        $"Body: {descriptor.BodyName} | Preset: {descriptor.PresetId}"
                    );
                    return false;
                }

                MissionConfiguration missionConfig = challenge.MissionConfig;

                bool valid =
                    challenge.Enabled &&
                    string.Equals(
                        challenge.Type,
                        SurvivorChallengePresets.MissionV2ChallengeType,
                        StringComparison.Ordinal
                    ) &&
                    challenge.Mission != null &&
                    missionConfig != null &&
                    missionConfig.Provider == UnlockProviderKind.USU &&
                    missionConfig.SelectionMode == UnlockSelectionMode.AutomaticFallback &&
                    string.Equals(
                        missionConfig.Source,
                        "Preset",
                        StringComparison.OrdinalIgnoreCase
                    ) &&
                    string.Equals(
                        missionConfig.BasePresetId,
                        descriptor.PresetId,
                        StringComparison.OrdinalIgnoreCase
                    ) &&
                    string.Equals(
                        challenge.LocalizationKey,
                        descriptor.LocalizationKey,
                        StringComparison.OrdinalIgnoreCase
                    ) &&
                    challenge.LunarCoinReward == descriptor.LunarCoinReward &&
                    SurvivorLocalization.UsesBuiltInLocalization(
                        descriptor.BodyName,
                        challenge
                    );

                if (!valid)
                {
                    logger.LogWarning(
                        "[MISSION AUTHORITY] Challenge preparado desalineado | " +
                        $"Body: {descriptor.BodyName} | Preset: {descriptor.PresetId}"
                    );
                }

                return valid;
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    "[MISSION AUTHORITY] Excepción validando challenge oficial | " +
                    $"Body: {descriptor.BodyName} | Preset: {descriptor.PresetId} | " +
                    ex.Message
                );
                return false;
            }
        }

        private static bool ValidateVisibleDescription(
            MissionPreset preset,
            ManualLogSource logger
        )
        {
            if (preset == null)
            {
                return false;
            }

            string description = preset.Description ?? "";
            string[] lines = description.Replace("\r", "").Split('\n');

            bool valid =
                lines.Length > 0 &&
                lines.Length <= 2;

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Length > 50)
                {
                    valid = false;
                }
            }

            if (!valid)
            {
                logger.LogWarning(
                    "[MISSION AUTHORITY] Descripción oficial fuera del contrato visual | " +
                    $"Preset: {preset.PresetId} | Líneas: {lines.Length} | " +
                    "Máximo: 2 líneas / 50 caracteres por línea"
                );
            }

            return valid;
        }

        private static bool HasMatchingDefinition(
            string bodyName,
            string source
        )
        {
            IReadOnlyList<SurvivorDefinition> definitions =
                SurvivorDefinitionRegistry.Registered;

            int matches = 0;

            for (int i = 0; i < definitions.Count; i++)
            {
                SurvivorDefinition definition = definitions[i];

                if (
                    definition != null &&
                    string.Equals(
                        definition.BodyName,
                        bodyName,
                        StringComparison.Ordinal
                    ) &&
                    string.Equals(
                        definition.SourceIdentifier,
                        source,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    matches++;
                }
            }

            return matches == 1;
        }

        private static bool ValidateLegacyIsolation(
            IReadOnlyList<MissionPreset> legacy,
            ManualLogSource logger
        )
        {
            if (legacy == null)
            {
                return false;
            }

            bool valid = true;
            HashSet<string> legacyIds =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < legacy.Count; i++)
            {
                MissionPreset preset = legacy[i];

                if (
                    preset == null ||
                    string.IsNullOrWhiteSpace(preset.PresetId) ||
                    !preset.IsLegacy ||
                    MissionPresetLibraryService.IsAssignable(preset) ||
                    OfficialMissionPresetRegistry.IsOfficialPresetId(
                        preset?.PresetId
                    )
                )
                {
                    valid = false;
                    logger.LogWarning(
                        "[MISSION AUTHORITY] Preset Legacy fuera de aislamiento | " +
                        $"Preset: {preset?.PresetId ?? "<null>"}"
                    );
                    continue;
                }

                if (!legacyIds.Add(preset.PresetId.Trim()))
                {
                    valid = false;
                    logger.LogWarning(
                        "[MISSION AUTHORITY] ID Legacy duplicado | " +
                        preset.PresetId
                    );
                }
            }

            return valid;
        }
    }
}
