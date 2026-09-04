using System;
using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    public static class SurvivorChallengePresets
    {
        // =========================================================
        // MODO DE AUTORÍA
        // =========================================================
        // true durante desarrollo: estas definiciones actualizan
        // Survivors.json al iniciar. En publicación pública futura
        // se desactivará para respetar la copia editable del usuario.
        // =========================================================

        public const bool AuthoringMode = true;


        private static MissionDefinition GetRuntimeMissionOrNull(string presetId)
        {
            MissionPreset preset = BuiltInMissionPresetCatalog.Get(presetId);

            if (
                preset == null ||
                !preset.RuntimeSupported ||
                preset.Mission == null ||
                preset.Mission.Routes == null ||
                preset.Mission.Routes.Count == 0
            )
            {
                return null;
            }

            // El catálogo usa factories: esta instancia es una copia
            // segura y no modifica el original del creador.
            return preset.Mission;
        }

        private static MissionConfiguration
            CreatePresetMissionConfiguration(
                string presetId
        )
        {
            return new MissionConfiguration
            {
                // Los nueve presets oficiales actuales funcionan como
                // proveedor USU. Hasta que exista una elección explícita
                // del jugador desde la futura interfaz, se consideran un
                // fallback automático y no una selección manual.
                Provider =
                    UnlockProviderKind.USU,

                SelectionMode =
                    UnlockSelectionMode.AutomaticFallback,

                Source =
                    "Preset",

                BasePresetId =
                    presetId,

                CustomMission =
                    null
            };
        }


        // =========================================================
        // SORA
        // =========================================================

        public static SurvivorChallengeJson CreateSoraPreset()
        {
            return new SurvivorChallengeJson
            {
                Enabled = true,
                Name = "Elegido de la Llave Espada",
                LocalizationKey = SurvivorLocalizationKeys.Sora,
                Description =
                    "Abre paso entre mundos en Baluarte de Ambry;\n" +
                    "vence a sombras y completa Venganza - Mercenary",
                LunarCoinReward = 4,
                Type = "ApplyStatusEffects", // fallback legacy
                Parameters = new JObject
                {
                    ["amount"] = 100,
                    ["singleRun"] = true
                },

                MissionConfig =
                    CreatePresetMissionConfiguration(
                        MissionPresetIds.SoraOfficial
                ),

                Mission =
                    GetRuntimeMissionOrNull(
                        MissionPresetIds.SoraOfficial
                )
            };
        }


        // =========================================================
        // RALSEI
        // =========================================================

        public static SurvivorChallengeJson CreateRalseiPreset()
        {
            return new SurvivorChallengeJson
            {
                Enabled = true,
                Name = "El poder de la bondad",
                LocalizationKey = SurvivorLocalizationKeys.Ralsei,
                Description =
                    "Usa Devoción y reúne 3 nuevos amigos Lemurianos;\n" +
                    "completa el portal con ellos - Captain o Seeker",
                LunarCoinReward = 3,
                Type = "HealHealth", // fallback legacy
                Parameters = new JObject
                {
                    ["amount"] = 10000,
                    ["singleRun"] = true
                },
                
                        MissionConfig =
                    CreatePresetMissionConfiguration(
                        MissionPresetIds.RalseiOfficial
                ),

                Mission =
                    GetRuntimeMissionOrNull(
                        MissionPresetIds.RalseiOfficial
                )
            };
        }


        // =========================================================
        // JHIN
        // =========================================================

        public static SurvivorChallengeJson CreateJhinPreset()
        {
            return new SurvivorChallengeJson
            {
                Enabled = true,
                Name = "El Cuarto Acto",
                LocalizationKey = SurvivorLocalizationKeys.Jhin,
                Description =
                    "Convierte a un jefe en tu gran final;\n" +
                    "asesta un crítico mortal de 44.444 de daño o más.",
                LunarCoinReward = 4,
                Type = "BossCriticalKill", // fallback legacy
                Parameters = new JObject
                {
                    ["minimumDamage"] = 44444,
                    ["singleRun"] = true
                },
                MissionConfig =
                    CreatePresetMissionConfiguration(
                        MissionPresetIds.JhinOfficial
                    ),
                Mission =
                    GetRuntimeMissionOrNull(
                        MissionPresetIds.JhinOfficial
                    )
            };
        }


        // =========================================================
        // SCOUT
        // =========================================================

        public static SurvivorChallengeJson CreateScoutPreset()
        {
            return new SurvivorChallengeJson
            {
                Enabled = true,
                Name = "Sed Termonuclear",
                LocalizationKey = SurvivorLocalizationKeys.Scout,
                Description =
                    "Sacia tu sed con 8 Bebidas energéticas;\n" +
                    "o completa el primer sector sin objetos en 4 min.",
                LunarCoinReward = 2,
                Type = "HoldItemStack", // fallback legacy: 15 bebidas
                Parameters = new JObject
                {
                    ["item"] = "SprintBonus",
                    ["amount"] = 8,
                    ["singleRun"] = true
                },
                MissionConfig =
                    CreatePresetMissionConfiguration(
                        MissionPresetIds.ScoutOfficial
                    ),
                Mission =
                    GetRuntimeMissionOrNull(
                        MissionPresetIds.ScoutOfficial
                    )
            };
        }


        // =========================================================
        // SPY
        // =========================================================

        public static SurvivorChallengeJson CreateSpyPreset()
        {
            return new SurvivorChallengeJson
            {
                Enabled = true,
                Name = "Sin que me veas venir",
                LocalizationKey = SurvivorLocalizationKeys.Spy,
                Description =
                    "Que el jefe nunca vea venir tu golpe final;\n" +
                    "remátalo por detrás con Daga serrada - Bandit",
                LunarCoinReward = 3,
                Type = "BackstabBossKill", // fallback legacy
                Parameters = new JObject
                {
                    ["singleRun"] = true
                },
                MissionConfig =
                    CreatePresetMissionConfiguration(
                        MissionPresetIds.SpyOfficial
                    ),
                Mission =
                    GetRuntimeMissionOrNull(
                        MissionPresetIds.SpyOfficial
                    )
            };
        }


        // =========================================================
        // ROCKET
        // =========================================================

        public static SurvivorChallengeJson CreateRocketPreset()
        {
            return new SurvivorChallengeJson
            {
                Enabled = true,
                Name = "La gravedad es opcional",
                LocalizationKey = SurvivorLocalizationKeys.Rocket,
                Description =
                    "Haz llover explosiones desde el cielo;\n" +
                    "derriba 5 antes de caer; haz la hazaña 3 veces.",
                LunarCoinReward = 4,
                Type = "AirborneExplosionKills", // fallback legacy
                Parameters = new JObject
                {
                    ["amount"] = 8,
                    ["resetOnGround"] = true,
                    ["countOwnedMinions"] = false,
                    ["singleRun"] = true
                },
                MissionConfig =
                    CreatePresetMissionConfiguration(
                        MissionPresetIds.RocketOfficial
                    ),
                Mission =
                    GetRuntimeMissionOrNull(
                        MissionPresetIds.RocketOfficial
                    )
            };
        }


        // =========================================================
        // HUNK
        // =========================================================

        public static SurvivorChallengeJson CreateHunkPreset()
        {
            return new SurvivorChallengeJson
            {
                Enabled = true,
                Name = "La Parca No Falla",
                LocalizationKey = SurvivorLocalizationKeys.Hunk,
                Description =
                    "Protege la batería y sobrevive a toda costa;\n" +
                    "escapa de la Luna o sacrifícate en el Obelisco.",
                LunarCoinReward = 5,
                Type = "PrecisionExecutionStreak", // fallback legacy
                Parameters = new JObject
                {
                    ["railgunnerWeakPoints"] = 24,
                    ["banditLightsOutKills"] = 24,
                    ["singleRun"] = true
                },
                MissionConfig =
                    CreatePresetMissionConfiguration(
                        MissionPresetIds.HunkOfficial
                    ),
                Mission =
                    GetRuntimeMissionOrNull(
                        MissionPresetIds.HunkOfficial
                    )
            };
        }


        // =========================================================
        // TINKATON
        // =========================================================

        public static SurvivorChallengeJson CreateTinkatonPreset()
        {
            return new SurvivorChallengeJson
            {
                Enabled = true,
                Name = "Forjada en Chatarra",
                LocalizationKey = SurvivorLocalizationKeys.Tinkaton,
                Description =
                    "Haz de 6 chatarras el inicio de tu gran golpe;\n" +
                    "ten Justicia demoledora y vence un Ojo mecánico.",
                LunarCoinReward = 5,
                Type = "ScrapItemBossFinisher", // fallback legacy
                Parameters = new JObject
                {
                    ["scrapAmount"] = 6,
                    ["requiredBody"] = "ToolbotBody",
                    ["requiredItem"] = "ArmorReductionOnHit",
                    ["bossBody"] = "SuperRoboBallBossBody",
                    ["finalDamageSource"] = "Secondary",
                    ["requiredSecondarySkillToken"] = "TOOLBOT_SECONDARY_NAME",
                    ["singleRun"] = true
                },
                MissionConfig =
                    CreatePresetMissionConfiguration(
                        MissionPresetIds.TinkatonOfficial
                    ),
                Mission =
                    GetRuntimeMissionOrNull(
                        MissionPresetIds.TinkatonOfficial
                    )
            };
        }


        // =========================================================
        // WOOPER
        // =========================================================

        public static SurvivorChallengeJson CreateWooperPreset()
        {
            return new SurvivorChallengeJson
            {
                Enabled = true,
                Name = "De vuelta al agua",
                LocalizationKey = SurvivorLocalizationKeys.Wooper,
                Description =
                    "Haz de los Humedales tu hogar; marca territorio;\n" +
                    "caza y muerde a 20 presas envenenadas - Acrid",
                LunarCoinReward = 2,
                Type = "KillEnemies", // fallback legacy genérico
                Parameters = new JObject
                {
                    ["amount"] = 100,
                    ["singleRun"] = true
                },
                MissionConfig =
                    CreatePresetMissionConfiguration(
                        MissionPresetIds.WooperOfficial
                    ),
                Mission =
                    GetRuntimeMissionOrNull(
                        MissionPresetIds.WooperOfficial
                    )
            };
        }


        // =========================================================
        // IDENTIFICACIÓN DE MODS/PERSONAJES
        // =========================================================

        private static bool IsExact(
            string bodyName,
            string contentPackIdentifier,
            string requiredBody,
            string requiredPack
        )
        {
            return
                string.Equals(bodyName, requiredBody, StringComparison.Ordinal) &&
                string.Equals(contentPackIdentifier, requiredPack, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSora(string bodyName, string pack) =>
            IsExact(bodyName, pack, "SoraBody", "com.Dragonyck.Sora");

        private static bool IsRalsei(string bodyName, string pack) =>
            IsExact(bodyName, pack, "RalseiBody", "com.GodRayProd.RalseiMod");

        private static bool IsJhin(string bodyName, string pack) =>
            IsExact(bodyName, pack, "JhinBody", "com.seroronin.JhinMod");

        private static bool IsScout(string bodyName, string pack) =>
            IsExact(bodyName, pack, "ScoutBody", "com.kenko.Scout");

        private static bool IsSpy(string bodyName, string pack) =>
            IsExact(bodyName, pack, "SpyBody", "com.kenko.Spy");

        private static bool IsRocket(string bodyName, string pack) =>
            IsExact(bodyName, pack, "RocketSurvivorBody", "com.EnforcerGang.RocketSurvivor");

        private static bool IsHunk(string bodyName, string pack) =>
            IsExact(bodyName, pack, "RobHunkBody", "com.rob.Hunk");

        private static bool IsTinkaton(string bodyName, string pack) =>
            IsExact(bodyName, pack, "TinkatonBody", "com.Dragonyck.Tinkaton");

        private static bool IsWooper(string bodyName, string pack) =>
            IsExact(bodyName, pack, "WooperBody", "com.Dragonyck.Wooper");


        // =========================================================
        // OBTENER PRESET PARA SURVIVOR CONOCIDO
        // =========================================================

        public static bool TryCreatePreset(
            string bodyName,
            string contentPackIdentifier,
            out SurvivorChallengeJson challenge
        )
        {
            challenge = null;

            if (IsSora(bodyName, contentPackIdentifier))
            {
                challenge = CreateSoraPreset();
                return true;
            }

            if (IsRalsei(bodyName, contentPackIdentifier))
            {
                challenge = CreateRalseiPreset();
                return true;
            }

            if (IsJhin(bodyName, contentPackIdentifier))
            {
                challenge = CreateJhinPreset();
                return true;
            }

            if (IsScout(bodyName, contentPackIdentifier))
            {
                challenge = CreateScoutPreset();
                return true;
            }

            if (IsSpy(bodyName, contentPackIdentifier))
            {
                challenge = CreateSpyPreset();
                return true;
            }

            if (IsRocket(bodyName, contentPackIdentifier))
            {
                challenge = CreateRocketPreset();
                return true;
            }

            if (IsHunk(bodyName, contentPackIdentifier))
            {
                challenge = CreateHunkPreset();
                return true;
            }

            if (IsTinkaton(bodyName, contentPackIdentifier))
            {
                challenge = CreateTinkatonPreset();
                return true;
            }

            if (IsWooper(bodyName, contentPackIdentifier))
            {
                challenge = CreateWooperPreset();
                return true;
            }

            return false;
        }


        // =========================================================
        // COMPATIBILIDAD TEMPORAL / FUTURA
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
