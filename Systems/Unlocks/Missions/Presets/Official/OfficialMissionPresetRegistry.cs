using System;
using System.Collections.Generic;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Descriptor canónico de una misión oficial creada por USU.
    ///
    /// Esta tabla NO contiene las recetas legacy. Su única función es
    /// responder de forma inequívoca a dos preguntas:
    /// - qué preset es la misión oficial actual de cada survivor;
    /// - qué presets pueden llamarse "oficiales" en la biblioteca.
    ///
    /// Las 12 recetas históricas siguen viviendo en
    /// BuiltInMissionPresetCatalog con IsLegacy=true y permanecen
    /// disponibles como plantillas para el futuro editor.
    /// </summary>
    public sealed class OfficialMissionPresetDescriptor
    {
        public string BodyName { get; }
        public string ContentPackIdentifier { get; }
        public string PresetId { get; }
        public string LocalizationKey { get; }
        public int LunarCoinReward { get; }

        /// <summary>
        /// Excepción explícita y acotada a la política "Original primero".
        ///
        /// true significa que USU puede ofrecer SU PROPIA misión como
        /// alternativa directa aunque el survivor también tenga un unlock
        /// Original. El Original no se modifica ni se borra: sólo cambia el
        /// provider activo cuando el jugador elige la alternativa USU.
        /// </summary>
        public bool AllowDirectUsuWithOriginal { get; }

        internal OfficialMissionPresetDescriptor(
            string bodyName,
            string contentPackIdentifier,
            string presetId,
            string localizationKey,
            int lunarCoinReward
        )
            : this(
                bodyName,
                contentPackIdentifier,
                presetId,
                localizationKey,
                lunarCoinReward,
                false
            )
        {
        }

        internal OfficialMissionPresetDescriptor(
            string bodyName,
            string contentPackIdentifier,
            string presetId,
            string localizationKey,
            int lunarCoinReward,
            bool allowDirectUsuWithOriginal
        )
        {
            BodyName = bodyName ?? "";
            ContentPackIdentifier = contentPackIdentifier ?? "";
            PresetId = presetId ?? "";
            LocalizationKey = localizationKey ?? "";
            LunarCoinReward = lunarCoinReward;
            AllowDirectUsuWithOriginal = allowDirectUsuWithOriginal;
        }
    }

    public static class OfficialMissionPresetRegistry
    {
        private static readonly OfficialMissionPresetDescriptor[] Entries =
        {
            new OfficialMissionPresetDescriptor(
                "SoraBody",
                "com.Dragonyck.Sora",
                MissionPresetIds.SoraOfficial,
                SurvivorLocalizationKeys.Sora,
                4
            ),
            new OfficialMissionPresetDescriptor(
                "RalseiBody",
                "com.GodRayProd.RalseiMod",
                MissionPresetIds.RalseiOfficial,
                SurvivorLocalizationKeys.Ralsei,
                3
            ),
            new OfficialMissionPresetDescriptor(
                "JhinBody",
                "com.seroronin.JhinMod",
                MissionPresetIds.JhinOfficial,
                SurvivorLocalizationKeys.Jhin,
                4
            ),
            new OfficialMissionPresetDescriptor(
                "ScoutBody",
                "com.kenko.Scout",
                MissionPresetIds.ScoutOfficial,
                SurvivorLocalizationKeys.Scout,
                2
            ),
            new OfficialMissionPresetDescriptor(
                "SpyBody",
                "com.kenko.Spy",
                MissionPresetIds.SpyOfficial,
                SurvivorLocalizationKeys.Spy,
                3
            ),
            new OfficialMissionPresetDescriptor(
                "RocketSurvivorBody",
                "com.EnforcerGang.RocketSurvivor",
                MissionPresetIds.RocketOfficial,
                SurvivorLocalizationKeys.Rocket,
                4
            ),
            new OfficialMissionPresetDescriptor(
                "RobHunkBody",
                "com.rob.Hunk",
                MissionPresetIds.HunkOfficial,
                SurvivorLocalizationKeys.Hunk,
                5
            ),
            new OfficialMissionPresetDescriptor(
                "TinkatonBody",
                "com.Dragonyck.Tinkaton",
                MissionPresetIds.TinkatonOfficial,
                SurvivorLocalizationKeys.Tinkaton,
                5
            ),
            new OfficialMissionPresetDescriptor(
                "WooperBody",
                "com.Dragonyck.Wooper",
                MissionPresetIds.WooperOfficial,
                SurvivorLocalizationKeys.Wooper,
                2
            ),
            new OfficialMissionPresetDescriptor(
                "MinerBody",
                "DiggerUnearthed.DiggerContent",
                MissionPresetIds.MinerOfficial,
                SurvivorLocalizationKeys.Miner,
                3,
                true
            )
        };

        public static int Count => Entries.Length;

        public static List<OfficialMissionPresetDescriptor> GetAll()
        {
            return new List<OfficialMissionPresetDescriptor>(Entries);
        }

        public static bool IsOfficialPresetId(string presetId)
        {
            return TryGetByPresetId(presetId, out _);
        }

        public static bool AllowsDirectUsuWithOriginal(
            string bodyName,
            string contentPackIdentifier,
            string presetId
        )
        {
            if (
                !TryResolve(
                    bodyName,
                    contentPackIdentifier,
                    out OfficialMissionPresetDescriptor descriptor
                ) ||
                descriptor == null ||
                !descriptor.AllowDirectUsuWithOriginal
            )
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(presetId))
            {
                return true;
            }

            return string.Equals(
                descriptor.PresetId,
                presetId.Trim(),
                StringComparison.OrdinalIgnoreCase
            );
        }

        public static bool TryGetByPresetId(
            string presetId,
            out OfficialMissionPresetDescriptor descriptor
        )
        {
            descriptor = null;

            string normalizedPresetId = presetId?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(normalizedPresetId))
            {
                return false;
            }

            for (int i = 0; i < Entries.Length; i++)
            {
                OfficialMissionPresetDescriptor candidate = Entries[i];

                if (
                    candidate != null &&
                    string.Equals(
                        candidate.PresetId,
                        normalizedPresetId,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    descriptor = candidate;
                    return true;
                }
            }

            return false;
        }

        public static bool TryResolve(
            string bodyName,
            string contentPackIdentifier,
            out OfficialMissionPresetDescriptor descriptor
        )
        {
            descriptor = null;

            string normalizedBody = bodyName?.Trim() ?? "";
            string normalizedPack = contentPackIdentifier?.Trim() ?? "";

            if (
                string.IsNullOrWhiteSpace(normalizedBody) ||
                string.IsNullOrWhiteSpace(normalizedPack)
            )
            {
                return false;
            }

            for (int i = 0; i < Entries.Length; i++)
            {
                OfficialMissionPresetDescriptor candidate = Entries[i];
                if (candidate == null)
                {
                    continue;
                }

                if (
                    string.Equals(
                        candidate.BodyName,
                        normalizedBody,
                        StringComparison.Ordinal
                    ) &&
                    string.Equals(
                        candidate.ContentPackIdentifier,
                        normalizedPack,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    descriptor = candidate;
                    return true;
                }
            }

            return false;
        }
    }
}
