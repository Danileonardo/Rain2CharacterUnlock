using System;
using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    public static class SurvivorChallengePresets
    {
        // =========================================================
        // MODO DE AUTORÍA
        // =========================================================
        //
        // true:
        // Los presets definidos aquí son la fuente de verdad
        // durante el desarrollo.
        //
        // Podemos modificar nombre, descripción, tipo y parámetros
        // libremente y Survivors.json se actualizará al iniciar.
        //
        // false:
        // Comportamiento futuro para la versión pública.
        // =========================================================

        public const bool AuthoringMode =
            true;


        // =========================================================
        // SORA
        // =========================================================

        public static SurvivorChallengeJson CreateSoraPreset()
        {
            return new SurvivorChallengeJson
            {
                Enabled =
                    true,

                Name =
                    "Elegido de la Llave Espada",

                Description =
                    "Mantén 100 efectos de estado válidos activos\n" +
                    "simultáneamente en una partida.",

                Type =
                    "ApplyStatusEffects",

                Parameters =
                    new JObject
                    {
                        ["amount"] =
                            100,

                        ["singleRun"] =
                            true
                    }
            };
        }


        // =========================================================
        // RALSEI
        // =========================================================

        public static SurvivorChallengeJson CreateRalseiPreset()
        {
            return new SurvivorChallengeJson
            {
                Enabled =
                    true,

                Name =
                    "Oración de Esperanza",

                Description =
                    "Restaura un total de 10000 de salud a tu equipo\n" +
                    "durante una sola partida.",

                Type =
                    "HealHealth",

                Parameters =
                    new JObject
                    {
                        ["amount"] =
                            10000,

                        ["singleRun"] =
                            true
                    }
            };
        }


        // =========================================================
        // JHIN
        // =========================================================

        public static SurvivorChallengeJson CreateJhinPreset()
        {
            return new SurvivorChallengeJson
            {
                Enabled =
                    true,

                Name =
                    "El Cuarto Acto",

                Description =
                    "Asesta el golpe final a un jefe con un crítico\n" +
                    "de 4444 de daño o más en una sola partida.",

                Type =
                    "BossCriticalKill",

                Parameters =
                    new JObject
                    {
                        ["minimumDamage"] =
                            4444,

                        ["singleRun"] =
                            true
                    }
            };
        }

        // =========================================================
        // SCOUT
        // =========================================================

        public static SurvivorChallengeJson CreateScoutPreset()
        {
            return new SurvivorChallengeJson
            {
                Enabled =
                    true,

                Name =
                    "Energía Atómica",

                Description =
                    "Acumula 15 Bebidas energéticas en total durante \n" +
                    "la partida.",

                Type =
                    "HoldItemStack",

                Parameters =
                    new JObject
                    {
                        ["item"] =
                            "SprintBonus",

                        ["amount"] =
                            15,

                        ["singleRun"] =
                            true
                    }
            };
        }

        // =========================================================
        // SPY
        // =========================================================

        public static SurvivorChallengeJson CreateSpyPreset()
        {
            return new SurvivorChallengeJson
            {
                Enabled =
                    true,

                Name =
                    "Sin que me veas venir",

                Description =
                    "Mata a un jefe apuñalándolo por la espalda\n" +
                    "con la Daga serrada - Bandit.",

                Type =
                    "BackstabBossKill",

                Parameters =
                    new JObject
                    {
                        ["singleRun"] =
                            true
                    }
            };
        }

        // =========================================================
        // ROCKET
        // =========================================================

        public static SurvivorChallengeJson CreateRocketPreset()
        {
            return new SurvivorChallengeJson
            {
                Enabled =
                    true,

                Name =
                    "La gravedad es opcional",

                Description =
                    "Mata a 15 enemigos con explosiones\n" +
                    "sin tocar el suelo.",

                Type =
                    "AirborneExplosionKills",

                Parameters =
                    new JObject
                    {
                        ["amount"] =
                            15,

                        ["resetOnGround"] =
                            true,

                        ["countOwnedMinions"] =
                            true,

                        ["singleRun"] =
                            true
                    }
            };
        }

        // =========================================================
        // HUNK
        // =========================================================

        public static SurvivorChallengeJson CreateHunkPreset()
        {
            return new SurvivorChallengeJson
            {
                Enabled =
                    true,

                Name =
                    "La Parca No Falla",

                Description =
                    "Logra 24 puntos débiles seguidos con Railgunner\n" +
                    "o 24 bajas seguidas con Luces fuera - Bandit.",

                Type =
                    "PrecisionExecutionStreak",

                Parameters =
                    new JObject
                    {
                        ["railgunnerWeakPoints"] =
                            24,

                        ["banditLightsOutKills"] =
                            24,

                        ["singleRun"] =
                            true
                    }
            };
        }

        // =========================================================
        // TINKATON
        // =========================================================

        public static SurvivorChallengeJson CreateTinkatonPreset()
        {
            return new SurvivorChallengeJson
            {
                Enabled =
                    true,

                Name =
                    "Forjada en Chatarra",

                Description =
                    "Recicla 6 objetos; ten Justicia demoledora y acaba\n" +
                    "con Unidad de Aleación con Bote explosivo - MUL-T.",

                Type =
                    "ScrapItemBossFinisher",

                Parameters =
                    new JObject
                    {
                        ["scrapAmount"] =
                            6,

                        ["requiredBody"] =
                            "ToolbotBody",

                        ["requiredItem"] =
                            "ArmorReductionOnHit",

                        ["bossBody"] =
                            "SuperRoboBallBossBody",

                        ["finalDamageSource"] =
                            "Secondary",

                        ["requiredSecondarySkillToken"] =
                            "TOOLBOT_SECONDARY_NAME",

                        ["singleRun"] =
                            true
                    }
            };
        }

        // =========================================================
        // ¿ES SORA?
        // =========================================================

        private static bool IsSora(
            string bodyName,
            string contentPackIdentifier
        )
        {
            return
                string.Equals(
                    bodyName,
                    "SoraBody",
                    StringComparison.Ordinal
                )
                &&
                string.Equals(
                    contentPackIdentifier,
                    "com.Dragonyck.Sora",
                    StringComparison.OrdinalIgnoreCase
                );
        }


        // =========================================================
        // ¿ES RALSEI?
        // =========================================================

        private static bool IsRalsei(
            string bodyName,
            string contentPackIdentifier
        )
        {
            return
                string.Equals(
                    bodyName,
                    "RalseiBody",
                    StringComparison.Ordinal
                )
                &&
                string.Equals(
                    contentPackIdentifier,
                    "com.GodRayProd.RalseiMod",
                    StringComparison.OrdinalIgnoreCase
                );
        }


        // =========================================================
        // ¿ES JHIN?
        // =========================================================

        private static bool IsJhin(
            string bodyName,
            string contentPackIdentifier
        )
        {
            return
                string.Equals(
                    bodyName,
                    "JhinBody",
                    StringComparison.Ordinal
                )
                &&
                string.Equals(
                    contentPackIdentifier,
                    "com.seroronin.JhinMod",
                    StringComparison.OrdinalIgnoreCase
                );
        }

        // =========================================================
        // ¿ES SCOUT?
        // =========================================================

        private static bool IsScout(
            string bodyName,
            string contentPackIdentifier
        )
        {
            return
                string.Equals(
                    bodyName,
                    "ScoutBody",
                    StringComparison.Ordinal
                )
                &&
                string.Equals(
                    contentPackIdentifier,
                    "com.kenko.Scout",
                    StringComparison.OrdinalIgnoreCase
                );
        }

        // =========================================================
        // ¿ES SPY?
        // =========================================================

        private static bool IsSpy(
            string bodyName,
            string contentPackIdentifier
        )
        {
            return
                string.Equals(
                    bodyName,
                    "SpyBody",
                    StringComparison.Ordinal
                )
                &&
                string.Equals(
                    contentPackIdentifier,
                    "com.kenko.Spy",
                    StringComparison.OrdinalIgnoreCase
                );
        }

        // =========================================================
        // ¿ES ROCKET?
        // =========================================================

        private static bool IsRocket(
            string bodyName,
            string contentPackIdentifier
        )
        {
            return
                string.Equals(
                    bodyName,
                    "RocketSurvivorBody",
                    StringComparison.Ordinal
                )
                &&
                string.Equals(
                    contentPackIdentifier,
                    "com.EnforcerGang.RocketSurvivor",
                    StringComparison.OrdinalIgnoreCase
                );
        }

        // =========================================================
        // ¿ES HUNK?
        // =========================================================

        private static bool IsHunk(
            string bodyName,
            string contentPackIdentifier
        )
        {
            return
                string.Equals(
                    bodyName,
                    "RobHunkBody",
                    StringComparison.Ordinal
                )
                &&
                string.Equals(
                    contentPackIdentifier,
                    "com.rob.Hunk",
                    StringComparison.OrdinalIgnoreCase
                );
        }

        // =========================================================
        // ¿ES TINKATON?
        // =========================================================

        private static bool IsTinkaton(
            string bodyName,
            string contentPackIdentifier
        )
        {
            return
                string.Equals(
                    bodyName,
                    "TinkatonBody",
                    StringComparison.Ordinal
                )
                &&
                string.Equals(
                    contentPackIdentifier,
                    "com.Dragonyck.Tinkaton",
                    StringComparison.OrdinalIgnoreCase
                );
        }

        // =========================================================
        // OBTENER PRESET PARA SURVIVOR CONOCIDO
        // =========================================================

        public static bool TryCreatePreset(
            string bodyName,
            string contentPackIdentifier,
            out SurvivorChallengeJson challenge
        )
        {
            challenge =
                null;


            // =====================================================
            // SORA
            // =====================================================

            if (
                IsSora(
                    bodyName,
                    contentPackIdentifier
                )
            )
            {
                challenge =
                    CreateSoraPreset();

                return true;
            }


            // =====================================================
            // RALSEI
            // =====================================================

            if (
                IsRalsei(
                    bodyName,
                    contentPackIdentifier
                )
            )
            {
                challenge =
                    CreateRalseiPreset();

                return true;
            }


            // =====================================================
            // JHIN
            // =====================================================

            if (
                IsJhin(
                    bodyName,
                    contentPackIdentifier
                )
            )
            {
                challenge =
                    CreateJhinPreset();

                return true;
            }

            // =====================================================
            // SCOUT
            // =====================================================

            if (
                IsScout(
                    bodyName,
                    contentPackIdentifier
                )
            )
            {
                challenge =
                    CreateScoutPreset();

                return true;
            }

            // =====================================================
            // SPY
            // =====================================================

            if (
                IsSpy(
                    bodyName,
                    contentPackIdentifier
                )
            )
            {
                challenge =
                    CreateSpyPreset();

                return true;
            }

            // =====================================================
            // ROCKET
            // =====================================================

            if (
                IsRocket(
                    bodyName,
                    contentPackIdentifier
                )
            )
            {
                challenge =
                    CreateRocketPreset();

                return true;
            }

            // =====================================================
            // HUNK
            // =====================================================

            if (
                IsHunk(
                    bodyName,
                    contentPackIdentifier
                )
            )
            {
                challenge =
                    CreateHunkPreset();

                return true;
            }

            // =====================================================
            // TINKATON
            // =====================================================

            if (
                IsTinkaton(
                    bodyName,
                    contentPackIdentifier
                )
            )
            {
                challenge =
                    CreateTinkatonPreset();

                return true;
            }

            // =====================================================
            // NO EXISTE PRESET ESPECÍFICO
            // =====================================================

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
            migratedChallenge =
                null;

            return false;
        }
    }
}