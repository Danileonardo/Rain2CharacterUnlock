using System;
using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    public static class SurvivorChallengePresets
    {
        // =========================================================
        // SORA
        // =========================================================

        public static SurvivorChallengeJson CreateSoraPreset()
        {
            return new SurvivorChallengeJson
            {
                Enabled = true,

                Name = "Guerrero de la llave",

                Type = "ApplyStatusEffects",

                Parameters = new JObject
                {
                    ["amount"] = 100,
                    ["singleRun"] = true
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
                Enabled = true,

                Name = "Oración de Esperanza",

                Type = "HealHealth",

                Parameters = new JObject
                {
                    ["amount"] = 5000,
                    ["singleRun"] = true
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
        // OBTENER PRESET PARA SURVIVOR CONOCIDO
        // =========================================================

        public static bool TryCreatePreset(
            string bodyName,
            string contentPackIdentifier,
            out SurvivorChallengeJson challenge
        )
        {
            challenge = null;


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
            // NO EXISTE PRESET ESPECÍFICO
            // =====================================================

            return false;
        }


        // =========================================================
        // MIGRAR PRESETS AUTOMÁTICOS ANTIGUOS
        // =========================================================

        public static bool TryMigrateLegacyPreset(
            string bodyName,
            string contentPackIdentifier,
            SurvivorChallengeJson currentChallenge,
            out SurvivorChallengeJson migratedChallenge
        )
        {
            migratedChallenge = null;


            // =====================================================
            // MIGRACIÓN DEL PRESET ANTERIOR DE RALSEI
            //
            // Rey Sanador
            // HealHealth
            // amount = 5000
            // singleRun = true
            //
            // ->
            //
            // Oración de Esperanza
            // HealHealth
            // amount = 5000
            // singleRun = true
            // =====================================================

            if (
                IsRalsei(
                    bodyName,
                    contentPackIdentifier
                )
                &&
                IsPreviousRalseiPreset(
                    currentChallenge
                )
            )
            {
                migratedChallenge =
                    CreateRalseiPreset();

                return true;
            }


            /*
             * A partir de aquí comprobamos el
             * challenge genérico automático usado
             * por versiones antiguas:
             *
             * Desafío de desbloqueo
             * KillEnemies
             * amount = 100
             *
             * Si el usuario modificó cualquier cosa,
             * NO se toca.
             */
            if (
                !IsLegacyAutomaticChallenge(
                    currentChallenge
                )
            )
            {
                return false;
            }


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
                migratedChallenge =
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
                migratedChallenge =
                    CreateRalseiPreset();

                return true;
            }


            // =====================================================
            // NO HAY MIGRACIÓN CONOCIDA
            // =====================================================

            return false;
        }


        // =========================================================
        // ¿ES EL PRESET ANTERIOR DE RALSEI?
        // =========================================================

        private static bool IsPreviousRalseiPreset(
            SurvivorChallengeJson challenge
        )
        {
            if (challenge == null)
            {
                return false;
            }


            if (!challenge.Enabled)
            {
                return false;
            }


            if (
                !string.Equals(
                    challenge.Name,
                    "Rey Sanador",
                    StringComparison.Ordinal
                )
            )
            {
                return false;
            }


            if (
                !string.Equals(
                    challenge.Type,
                    "HealHealth",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return false;
            }


            if (challenge.Parameters == null)
            {
                return false;
            }


            /*
             * El preset anterior tenía exactamente:
             *
             * amount
             * singleRun
             *
             * Si aparecen parámetros adicionales,
             * asumimos que el usuario lo personalizó.
             */
            int parameterCount = 0;

            foreach (
                JProperty property
                in challenge.Parameters.Properties()
            )
            {
                parameterCount++;
            }


            if (parameterCount != 2)
            {
                return false;
            }


            JToken amountToken =
                challenge.Parameters["amount"];


            JToken singleRunToken =
                challenge.Parameters["singleRun"];


            if (
                amountToken == null ||
                singleRunToken == null
            )
            {
                return false;
            }


            if (
                !int.TryParse(
                    amountToken.ToString(),
                    out int amount
                )
            )
            {
                return false;
            }


            if (amount != 5000)
            {
                return false;
            }


            if (
                !bool.TryParse(
                    singleRunToken.ToString(),
                    out bool singleRun
                )
            )
            {
                return false;
            }


            if (!singleRun)
            {
                return false;
            }


            /*
             * Si tiene propiedades desconocidas
             * agregadas por el usuario,
             * tampoco migramos automáticamente.
             */
            if (
                challenge.ExtraData != null &&
                challenge.ExtraData.Count > 0
            )
            {
                return false;
            }


            return true;
        }


        // =========================================================
        // ¿ES EL CHALLENGE GENÉRICO ANTIGUO?
        // =========================================================

        private static bool IsLegacyAutomaticChallenge(
            SurvivorChallengeJson challenge
        )
        {
            if (challenge == null)
            {
                return false;
            }


            if (!challenge.Enabled)
            {
                return false;
            }


            if (
                !string.Equals(
                    challenge.Name,
                    "Desafío de desbloqueo",
                    StringComparison.Ordinal
                )
            )
            {
                return false;
            }


            if (
                !string.Equals(
                    challenge.Type,
                    "KillEnemies",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return false;
            }


            if (challenge.Parameters == null)
            {
                return false;
            }


            /*
             * Si tiene parámetros adicionales,
             * asumimos que el usuario modificó
             * la configuración.
             */
            int parameterCount = 0;

            foreach (
                JProperty property
                in challenge.Parameters.Properties()
            )
            {
                parameterCount++;
            }


            if (parameterCount != 1)
            {
                return false;
            }


            JToken amountToken =
                challenge.Parameters["amount"];


            if (amountToken == null)
            {
                return false;
            }


            if (
                !int.TryParse(
                    amountToken.ToString(),
                    out int amount
                )
            )
            {
                return false;
            }


            if (amount != 100)
            {
                return false;
            }


            /*
             * Si existen propiedades desconocidas
             * agregadas por el usuario,
             * tampoco migramos automáticamente.
             */
            if (
                challenge.ExtraData != null &&
                challenge.ExtraData.Count > 0
            )
            {
                return false;
            }


            return true;
        }
    }
}