using System;
using System.Collections.Generic;

using BepInEx.Logging;

using RoR2;

using UnityEngine.Networking;


namespace UniversalSurvivorUnlocks
{
    /*
     * =============================================================
     * GENERIC MISSION DISPATCHER
     * =============================================================
     *
     * Punto común entre:
     *
     * Objective Handlers
     *          ↓
     * MissionCompositionProgressEvaluator
     *          ↓
     * SessionUnlockManager
     *
     * Los handlers NO conceden rewards directamente.
     *
     * Sólo informan:
     *
     *     MissionJustCompleted
     *
     * y este dispatcher realiza el reward de sesión.
     * =============================================================
     */
    public static class GenericMissionDispatcher
    {
        private static ManualLogSource logger;

        private static bool initialized;


        private static readonly HashSet<string>
            CompletedBodiesThisRun =
                new HashSet<string>(
                    StringComparer.Ordinal
                );


        // =========================================================
        // INITIALIZE
        // =========================================================

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


            /*
             * Trackers/contextos genéricos.
             */
            MissionSkillHistoryTracker.Initialize(log);
            MissionPreDamageStatusTracker.Initialize(log);

            /*
             * Objective Handlers genéricos.
             */
            CompleteStageObjectiveHandler.Initialize(log);
            CompleteTeleporterObjectiveHandler.Initialize(log);
            HoldItemStackObjectiveHandler.Initialize(log);
            PickupItemObjectiveHandler.Initialize(log);
            ScrapItemsObjectiveHandler.Initialize(log);
            KillObjectiveHandler.Initialize(log);
            BombingRunObjectiveHandler.Initialize(log);
            CarryEquipmentObjectiveHandler.Initialize(log);
            CompleteEndingObjectiveHandler.Initialize(log);
            DefeatUmbraWavesObjectiveHandler.Initialize(log);
            LeaveStageObjectiveHandler.Initialize(log);
            RecruitMinionsObjectiveHandler.Initialize(log);


            logger?.LogInfo(
                "[MISSION V2] GenericMissionDispatcher inicializado."
            );
        }


        private static void OnRunStartGlobal(
            Run run
        )
        {
            CompletedBodiesThisRun.Clear();
        }


        private static void OnRunDestroyGlobal(
            Run run
        )
        {
            CompletedBodiesThisRun.Clear();
        }


        // =========================================================
        // RESULTADO
        // =========================================================

        public static void HandleProgressResult(
            MissionObjectiveRuntimeBinding binding,
            MissionCompositionProgressResult result,
            string details = ""
        )
        {
            if (
                binding == null ||
                result == null ||
                !result.Accepted
            )
            {
                return;
            }


            string bindingBodyName =
                binding.BodyName;


            // Una misión ya cerrada en esta run no debe seguir
            // generando progreso ni ruido de log por otras rutas.
            if (
                !string.IsNullOrWhiteSpace(bindingBodyName) &&
                CompletedBodiesThisRun.Contains(bindingBodyName)
            )
            {
                return;
            }


            bool suppressProgressLogs =
                !string.IsNullOrWhiteSpace(bindingBodyName) &&
                SessionUnlockManager.AreAllLocalUsersUnlocked(
                    bindingBodyName
                );


            /*
            * =========================================================
            * LOGS DE PROGRESO
             * =========================================================
             *
             * Metas pequeñas:
             *     1 - 10      -> cada 1
             *
             * Metas medianas:
             *     11 - 99     -> cada 5
             *
             * Metas grandes:
             *     100 - 999   -> cada 100
             *
             * Metas enormes:
             *     1000+       -> cada 1000
             *
             * El objetivo completado SIEMPRE se muestra.
             * =========================================================
             */
            if (!suppressProgressLogs)
            {
                if (result.ObjectiveJustCompleted)
                {
                    logger?.LogInfo(
                        "[MISSION V2] OBJETIVO COMPLETADO | " +
                        $"Body: {binding.BodyName} | " +
                        $"Route: {result.RouteId} | " +
                        $"Objective: {result.ObjectiveId} | " +
                        $"{result.CurrentValue:0.##}/{result.RequiredValue:0.##}"
                    );
                }
                else if (ShouldLogProgress(result))
                {
                    logger?.LogInfo(
                        "[MISSION V2] PROGRESO | " +
                        $"Body: {binding.BodyName} | " +
                        $"Route: {result.RouteId} | " +
                        $"Objective: {result.ObjectiveId} | " +
                        $"{result.CurrentValue:0.##}/{result.RequiredValue:0.##}" +
                        (
                            string.IsNullOrWhiteSpace(details)
                                ? ""
                                : " | " + details
                        )
                    );
                }
            }


            if (!result.MissionJustCompleted)
            {
                return;
            }


            if (
                !NetworkServer.active ||
                Run.instance == null
            )
            {
                return;
            }


            string bodyName =
                bindingBodyName;


            if (
                string.IsNullOrWhiteSpace(
                    bodyName
                )
            )
            {
                return;
            }


            if (
                CompletedBodiesThisRun.Contains(
                    bodyName
                )
            )
            {
                return;
            }


            /*
             * Confirmamos el unlock antes de deduplicar.
             *
             * Si por un error de configuración el Unlockable todavía
             * no existe, una futura actualización válida dentro de
             * la misma run podrá volver a intentarlo.
             */
            if (
                !SurvivorUnlockManager
                    .TryGetCustomUnlockable(
                        bodyName,
                        out UnlockableDef unlockable
                    ) ||
                unlockable == null
            )
            {
                logger?.LogWarning(
                    "[MISSION V2] Misión terminada pero no existe " +
                    $"Unlockable USU | Body: {bodyName}"
                );

                return;
            }


            CompletedBodiesThisRun.Add(
                bodyName
            );


            if (!suppressProgressLogs)
            {
                logger?.LogInfo(
                    "[MISSION V2] MISIÓN COMPLETADA | " +
                    $"Body: {bodyName} | " +
                    $"Route: {result.RouteId}" +
                    (
                        string.IsNullOrWhiteSpace(
                            details
                        )
                            ? ""
                            : " | " + details
                    )
                );
            }


            /*
             * Reutilizamos el sistema de recompensa que ya fue
             * validado en multiplayer.
             *
             * NO duplicamos lógica de:
             *
             * - Achievement
             * - Unlockable
             * - cliente remoto
             * - persistencia
             */
            SessionUnlockManager
                .CompleteMission(
                    bodyName
                );
        }

        // =========================================================
        // POLÍTICA GLOBAL DE LOGS DE PROGRESO
        // =========================================================

        private static double GetProgressLogStep(
            double requiredValue
        )
        {
            if (requiredValue <= 10d)
            {
                return 1d;
            }


            if (requiredValue < 100d)
            {
                return 5d;
            }


            if (requiredValue < 1000d)
            {
                return 100d;
            }


            return 1000d;
        }


        private static bool ShouldLogProgress(
            MissionCompositionProgressResult result
        )
        {
            if (
                result == null ||
                !result.Accepted
            )
            {
                return false;
            }


            double previous =
                result.PreviousValue;

            double current =
                result.CurrentValue;

            double required =
                result.RequiredValue;


            /*
             * No mostramos retrocesos ni valores sin cambio.
             *
             * Especialmente importante para objetivos como
             * HoldItemStack, cuyos valores pueden subir y bajar.
             */
            if (current <= previous)
            {
                return false;
            }


            /*
             * La finalización se trata aparte mediante
             * ObjectiveJustCompleted.
             */
            if (current >= required)
            {
                return false;
            }


            double step =
                GetProgressLogStep(
                    required
                );


            long previousBucket =
                (long)Math.Floor(
                    previous / step
                );

            long currentBucket =
                (long)Math.Floor(
                    current / step
                );


            return
                currentBucket >
                previousBucket;
        }
    }
}
