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
        // Durante el desarrollo, los presets definidos aquí
        // son la fuente de verdad.
        //
        // Cuando cambiemos nombre, descripción, tipo o parámetros,
        // SurvivorJsonManager actualizará automáticamente la copia
        // guardada en Survivors.json.
        //
        // false:
        // Comportamiento pensado para la versión final.
        // El preset sólo se utilizará al crear/configurar por
        // primera vez al survivor.
        //
        // Más adelante, antes de publicar la versión definitiva,
        // cambiaremos este valor a false.
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
                    "Restaura un total de 5000 de salud a tu equipo\n" +
                    "durante una sola partida.",

                Type =
                    "HealHealth",

                Parameters =
                    new JObject
                    {
                        ["amount"] =
                            5000,

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
        // OBTENER PRESET PARA SURVIVOR CONOCIDO
        // =========================================================
        //
        // Este es el punto central.
        //
        // Cada survivor al que nosotros le creemos una misión
        // predeterminada debe agregarse aquí.
        //
        // Los survivors sin preset específico seguirán utilizando
        // el desafío genérico configurado por SurvivorJsonManager.
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
            // NO EXISTE PRESET ESPECÍFICO
            // =====================================================

            return false;
        }


        // =========================================================
        // COMPATIBILIDAD TEMPORAL
        // =========================================================
        //
        // SurvivorJsonManager todavía llama a este método.
        //
        // Ya NO realizamos migraciones del tipo:
        //
        // Guerrero de la llave
        // ->
        // Elegido de la Llave Espada
        //
        // porque durante el desarrollo los presets todavía
        // están siendo diseñados.
        //
        // En el siguiente paso modificaremos SurvivorJsonManager
        // para utilizar AuthoringMode y podremos eliminar
        // definitivamente este método.
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