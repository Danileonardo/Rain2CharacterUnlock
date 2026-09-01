using System;
using System.Collections.Generic;
using BepInEx.Logging;
using RoR2;
using UnityEngine;

namespace UniversalSurvivorUnlocks
{
    /*
     * =============================================================
     * MISSION LOG LIMITER
     * =============================================================
     *
     * Reduce spam SIN afectar la lógica de progreso.
     *
     * Ejemplo:
     *
     * progreso interno:
     *
     *     1
     *     2
     *     3
     *     4
     *     5
     *     6
     *     ...
     *
     * log:
     *
     *     5
     *     10
     *     15
     *
     * También permite limitar logs detallados por tiempo.
     *
     * IMPORTANTE:
     * Este archivo sólo provee la API.
     * Trackers existentes se migrarán uno por uno.
     * =============================================================
     */
    public static class MissionLogLimiter
    {
        private static ManualLogSource logger;

        private static bool initialized;


        private static readonly Dictionary<
            string,
            long
        > LastMilestoneBucket =
            new Dictionary<
                string,
                long
            >(
                StringComparer.Ordinal
            );


        private static readonly Dictionary<
            string,
            float
        > LastLogTime =
            new Dictionary<
                string,
                float
            >(
                StringComparer.Ordinal
            );


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
                "[MISSION PERF] LogLimiter inicializado."
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
        // HITOS
        // =========================================================

        /*
         * Ejemplo:
         *
         * step = 5
         *
         * current:
         * 1  -> false
         * 4  -> false
         * 5  -> true
         * 6  -> false
         * 10 -> true
         *
         * Si el valor baja, NO genera spam.
         *
         * El tracker puede loguear resets importantes de forma
         * explícita cuando corresponda.
         */
        public static bool ShouldLogMilestone(
            string key,
            double current,
            double step = 5d
        )
        {
            if (
                string.IsNullOrWhiteSpace(
                    key
                ) ||
                step <= 0d ||
                current < 0d
            )
            {
                return false;
            }


            long bucket =
                (long)Math.Floor(
                    current / step
                );


            if (bucket <= 0)
            {
                return false;
            }


            if (
                LastMilestoneBucket.TryGetValue(
                    key,
                    out long previousBucket
                )
            )
            {
                if (
                    bucket <=
                        previousBucket
                )
                {
                    return false;
                }
            }


            LastMilestoneBucket[
                key
            ] =
                bucket;


            return true;
        }


        // =========================================================
        // RATE LIMIT
        // =========================================================

        public static bool ShouldLogRateLimited(
            string key,
            float minimumSeconds
        )
        {
            if (
                string.IsNullOrWhiteSpace(
                    key
                )
            )
            {
                return false;
            }


            if (minimumSeconds <= 0f)
            {
                return true;
            }


            float now =
                Time.unscaledTime;


            if (
                LastLogTime.TryGetValue(
                    key,
                    out float last
                ) &&
                now - last <
                    minimumSeconds
            )
            {
                return false;
            }


            LastLogTime[
                key
            ] =
                now;


            return true;
        }


        public static void ResetKey(
            string key
        )
        {
            if (
                string.IsNullOrWhiteSpace(
                    key
                )
            )
            {
                return;
            }


            LastMilestoneBucket.Remove(
                key
            );

            LastLogTime.Remove(
                key
            );
        }


        public static void Reset()
        {
            LastMilestoneBucket.Clear();

            LastLogTime.Clear();
        }
    }
}
