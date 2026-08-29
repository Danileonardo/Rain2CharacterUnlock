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
            //
            // Mod original:
            // com.Dragonyck.Sora
            //
            // Integración:
            // Universal Survivor Unlocks
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
            // NO EXISTE PRESET ESPECÍFICO
            // =====================================================

            return false;
        }


        // =========================================================
        // MIGRAR PRESET AUTOMÁTICO ANTIGUO
        // =========================================================

        public static bool TryMigrateLegacyPreset(
            string bodyName,
            string contentPackIdentifier,
            SurvivorChallengeJson currentChallenge,
            out SurvivorChallengeJson migratedChallenge
        )
        {
            migratedChallenge = null;


            /*
             * Solamente migramos configuraciones
             * pertenecientes a Sora.
             */
            if (
                !IsSora(
                    bodyName,
                    contentPackIdentifier
                )
            )
            {
                return false;
            }


            /*
             * Sólo reemplazamos el challenge genérico
             * exacto creado automáticamente por las
             * versiones anteriores.
             *
             * Si el usuario modificó cualquier cosa,
             * no se toca.
             */
            if (
                !IsLegacyAutomaticChallenge(
                    currentChallenge
                )
            )
            {
                return false;
            }


            migratedChallenge =
                CreateSoraPreset();


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
             * agregadas por el usuario, tampoco
             * hacemos una migración automática.
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