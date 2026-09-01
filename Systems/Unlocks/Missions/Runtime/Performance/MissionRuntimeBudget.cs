using System;
using System.Collections.Generic;
using BepInEx.Logging;
using RoR2;
using UnityEngine;

namespace UniversalSurvivorUnlocks
{
    /*
     * =============================================================
     * MISSION RUNTIME BUDGET
     * =============================================================
     *
     * Presupuesto para ESCÁNERES DE RESPALDO.
     *
     * NO usar para eventos autoritativos:
     *
     * - NO descartar golpes.
     * - NO descartar curaciones.
     * - NO descartar muertes.
     * - NO descartar pickups.
     *
     * Esos eventos deben procesarse siempre.
     *
     * Este presupuesto sirve para cosas como:
     *
     * - reconciliar buffs activos,
     * - verificar entidades que un evento pudo perder,
     * - inspeccionar una lista grande de cuerpos,
     * - sanity checks periódicos.
     *
     * Configuración inicial:
     *
     *     5 elementos
     *     por cada 0.10 segundos
     *     por canal
     *
     * Un scanner de 100 entidades, por ejemplo,
     * no procesa 100 de golpe.
     *
     * Las procesa gradualmente.
     * =============================================================
     */
    public static class MissionRuntimeBudget
    {
        private sealed class BudgetState
        {
            public float WindowStartedAt;

            public int Used;
        }


        private static ManualLogSource logger;

        private static bool initialized;


        private static readonly Dictionary<
            string,
            BudgetState
        > States =
            new Dictionary<
                string,
                BudgetState
            >(
                StringComparer.Ordinal
            );


        public const int DefaultMaxItemsPerWindow =
            5;


        public const float DefaultWindowSeconds =
            0.10f;


        public static void Initialize(
            ManualLogSource log
        )
        {
            if (initialized)
            {
                return;
            }


            initialized =
                true;

            logger =
                log;


            Run.onRunStartGlobal +=
                OnRunStartGlobal;

            Run.onRunDestroyGlobal +=
                OnRunDestroyGlobal;


            logger?.LogInfo(
                "[MISSION PERF] RuntimeBudget inicializado | " +
                $"Default: {DefaultMaxItemsPerWindow} items / " +
                $"{DefaultWindowSeconds:0.00}s."
            );
        }


        private static void OnRunStartGlobal(
            Run run
        )
        {
            Reset();
        }


        private static void OnRunDestroyGlobal(
            Run run
        )
        {
            Reset();
        }


        // =========================================================
        // CONSUMIR
        // =========================================================

        public static bool TryConsume(
            string channel,
            int amount = 1,
            int maxItemsPerWindow =
                DefaultMaxItemsPerWindow,
            float windowSeconds =
                DefaultWindowSeconds
        )
        {
            if (
                string.IsNullOrWhiteSpace(
                    channel
                )
            )
            {
                return false;
            }


            if (amount <= 0)
            {
                return true;
            }


            if (maxItemsPerWindow <= 0)
            {
                return false;
            }


            if (windowSeconds <= 0f)
            {
                windowSeconds =
                    DefaultWindowSeconds;
            }


            float now =
                Time.unscaledTime;


            if (
                !States.TryGetValue(
                    channel,
                    out BudgetState state
                ) ||
                state == null
            )
            {
                state =
                    new BudgetState
                    {
                        WindowStartedAt =
                            now,

                        Used =
                            0
                    };


                States[
                    channel
                ] =
                    state;
            }


            if (
                now -
                state.WindowStartedAt >=
                    windowSeconds
            )
            {
                state.WindowStartedAt =
                    now;

                state.Used =
                    0;
            }


            if (
                state.Used + amount >
                    maxItemsPerWindow
            )
            {
                return false;
            }


            state.Used +=
                amount;


            return true;
        }


        // =========================================================
        // DISPONIBLE
        // =========================================================

        public static int GetRemaining(
            string channel,
            int maxItemsPerWindow =
                DefaultMaxItemsPerWindow,
            float windowSeconds =
                DefaultWindowSeconds
        )
        {
            if (
                string.IsNullOrWhiteSpace(
                    channel
                ) ||
                maxItemsPerWindow <= 0
            )
            {
                return 0;
            }


            if (
                !States.TryGetValue(
                    channel,
                    out BudgetState state
                ) ||
                state == null
            )
            {
                return maxItemsPerWindow;
            }


            float now =
                Time.unscaledTime;


            if (
                now -
                state.WindowStartedAt >=
                    windowSeconds
            )
            {
                return maxItemsPerWindow;
            }


            return Math.Max(
                0,
                maxItemsPerWindow -
                state.Used
            );
        }


        public static void ResetChannel(
            string channel
        )
        {
            if (
                string.IsNullOrWhiteSpace(
                    channel
                )
            )
            {
                return;
            }


            States.Remove(
                channel
            );
        }


        public static void Reset()
        {
            States.Clear();
        }
    }
}
