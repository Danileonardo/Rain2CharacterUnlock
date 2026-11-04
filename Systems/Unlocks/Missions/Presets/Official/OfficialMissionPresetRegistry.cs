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
                "DarthVaderBody", "com.PopcornFactory.DarthVaderMod",
                MissionPresetIds.DarthVaderOfficial, SurvivorLocalizationKeys.DarthVader, 5, true
            ),
            new OfficialMissionPresetDescriptor(
                "KnightBody", "com.fanto.TheKnight",
                MissionPresetIds.KnightOfficial, SurvivorLocalizationKeys.Knight, 4, true
            ),
            new OfficialMissionPresetDescriptor(
                "ShadowBody", "com.ds_gaming.Shadow",
                MissionPresetIds.ShadowOfficial, SurvivorLocalizationKeys.Shadow, 4, true
            ),
            new OfficialMissionPresetDescriptor(
                "SonicBody", "com.ds_gaming.Sonic",
                MissionPresetIds.SonicOfficial, SurvivorLocalizationKeys.Sonic, 4, true
            ),
            new OfficialMissionPresetDescriptor(
                "HornetBody", "com.fanto.HornetSilksong",
                MissionPresetIds.HornetOfficial, SurvivorLocalizationKeys.Hornet, 4, true
            ),
            new OfficialMissionPresetDescriptor(
                "IreliaBody", "com.slendykray.BladedancerMod",
                MissionPresetIds.BladedancerOfficial,
                SurvivorLocalizationKeys.Bladedancer, 0
            ),
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
                "RobUltronBody", "com.rob.Ultron",
                MissionPresetIds.HelPOfficial, SurvivorLocalizationKeys.HelP, 3
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
                "GwenBody", "com.Dragonyck.Gwen",
                MissionPresetIds.GwenOfficial, SurvivorLocalizationKeys.Gwen, 3
            ),
            new OfficialMissionPresetDescriptor(
                "KindredBody", "com.Dragonyck.Kindred",
                MissionPresetIds.KindredOfficial, SurvivorLocalizationKeys.Kindred, 4
            ),
            new OfficialMissionPresetDescriptor(
                "SeamstressBody", "com.kenko.Seamstress",
                MissionPresetIds.SeamstressOfficial, SurvivorLocalizationKeys.Seamstress, 3
            ),
            new OfficialMissionPresetDescriptor(
                "BansheeBody",
                "com.kenko.Banshee",
                MissionPresetIds.BansheeOfficial,
                SurvivorLocalizationKeys.Banshee,
                3
            ),
            new OfficialMissionPresetDescriptor(
                "UnforgivenBody",
                "com.kenko.Unforgiven",
                MissionPresetIds.WandererOfficial,
                SurvivorLocalizationKeys.Wanderer,
                4
            ),
            new OfficialMissionPresetDescriptor(
                "WarlockBody",
                "com.kenko.Warlock",
                MissionPresetIds.WarlockOfficial,
                SurvivorLocalizationKeys.Warlock,
                4,
                true // alternativa explícita; conservar Original por defecto
            ),
            new OfficialMissionPresetDescriptor(
                "BayoBody",
                "com.psyche.BayoMod",
                MissionPresetIds.BayonettaOfficial,
                SurvivorLocalizationKeys.Bayonetta,
                4
            ),
            new OfficialMissionPresetDescriptor(
                "AH64Body",
                "com.JohnstonStu.AH64",
                MissionPresetIds.AH64Official,
                SurvivorLocalizationKeys.AH64,
                10
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

        private static OfficialMissionPresetDescriptor CWTDescriptor =>
            CWTCampaignIdentity.Detected
                ? new OfficialMissionPresetDescriptor(CWTCampaignIdentity.BodyName,
                    CWTCampaignIdentity.ContentPackIdentifier,
                    MissionPresetIds.CelestialWarTankOfficial,
                    SurvivorLocalizationKeys.CelestialWarTank, 10)
                : null;

        public static int Count => Entries.Length + (CWTDescriptor != null ? 1 : 0);

        public static List<OfficialMissionPresetDescriptor> GetAll()
        {
            var result = new List<OfficialMissionPresetDescriptor>(Entries);
            if (CWTDescriptor != null) result.Add(CWTDescriptor);
            return result;
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

            if (CWTDescriptor != null && string.Equals(
                    CWTDescriptor.PresetId, normalizedPresetId,
                    StringComparison.OrdinalIgnoreCase))
            {
                descriptor = CWTDescriptor;
                return true;
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

            // ContentPackIdentifier is not the Thunderstore package name.
            // These three characters can therefore resolve their USU preset
            // using the distinctive body while retaining the detected pack ID.
            if (string.Equals(normalizedBody, "DarthVaderBody", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalizedBody, "KnightBody", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalizedBody, "HornetBody", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalizedBody, "SonicBody", StringComparison.OrdinalIgnoreCase))
            {
                for (int i = 0; i < Entries.Length; i++)
                {
                    var candidate = Entries[i];
                    if (candidate != null && string.Equals(candidate.BodyName,
                        normalizedBody, StringComparison.OrdinalIgnoreCase))
                    {
                        descriptor = new OfficialMissionPresetDescriptor(normalizedBody, normalizedPack,
                            candidate.PresetId, candidate.LocalizationKey,
                            candidate.LunarCoinReward, candidate.AllowDirectUsuWithOriginal);
                        return true;
                    }
                }
            }

            if (CWTDescriptor != null &&
                string.Equals(CWTDescriptor.BodyName, normalizedBody, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(CWTDescriptor.ContentPackIdentifier, normalizedPack, StringComparison.OrdinalIgnoreCase))
            {
                descriptor = CWTDescriptor;
                return true;
            }
            return false;
        }
    }
}
