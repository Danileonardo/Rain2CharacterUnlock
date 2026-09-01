using System;
using BepInEx.Logging;
using RoR2;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace UniversalSurvivorUnlocks
{
    /*
     * =============================================================
     * MISSION STAGE RUNTIME TRACKER
     * =============================================================
     *
     * Sistema genérico y event-driven para detectar:
     *
     * 1. Inicio de un nuevo sector / escena jugable.
     * 2. Finalización del sector mediante teletransportador.
     *
     * NO hace polling.
     * NO conoce a Wooper.
     * NO conoce ningún preset.
     *
     * Expone dos eventos reutilizables:
     *
     *     StageStarted
     *     StageCompleted
     *
     * Además conecta el cambio de sector con:
     *
     *     MissionProgressRegistry.NotifyStageStarted()
     *
     * para resetear únicamente objetivos con:
     *
     *     MissionProgressResetScope.Stage
     *
     * =============================================================
     */
    public static class MissionStageRuntimeTracker
    {
        private static ManualLogSource logger;

        private static bool initialized;

        private static string currentStageName =
            "";

        private static int currentSceneHandle =
            int.MinValue;

        private static int stageSequence;

        private static int lastCompletedSceneHandle =
            int.MinValue;


        public static event Action<
            MissionStageEventContext
        > StageStarted;


        public static event Action<
            MissionStageEventContext
        > StageCompleted;


        public static string CurrentStageName =>
            currentStageName;


        public static int CurrentSceneHandle =>
            currentSceneHandle;


        public static int CurrentStageSequence =>
            stageSequence;


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


            /*
             * SceneDirector.Start existe en la versión actual
             * del juego y ya es utilizado por otros mods cargados.
             *
             * Lo usamos sólo como evento de entrada de sector.
             */
            On.RoR2.SceneDirector.Start +=
                SceneDirector_Start;


            /*
             * Este evento se dispara cuando el teletransportador
             * termina realmente su proceso.
             *
             * Es más apropiado para "finaliza el sector"
             * que simplemente comprobar carga al 99/100%.
             */
            TeleporterInteraction
                .onTeleporterFinishGlobal +=
                OnTeleporterFinishGlobal;


            Run.onRunStartGlobal +=
                OnRunStartGlobal;

            Run.onRunDestroyGlobal +=
                OnRunDestroyGlobal;


            logger?.LogInfo(
                "[MISSION STAGE] Sistema de sectores inicializado."
            );
        }


        // =========================================================
        // RUN
        // =========================================================

        private static void OnRunStartGlobal(
            Run run
        )
        {
            ResetRuntimeState();
        }


        private static void OnRunDestroyGlobal(
            Run run
        )
        {
            ResetRuntimeState();
        }


        private static void ResetRuntimeState()
        {
            currentStageName =
                "";

            currentSceneHandle =
                int.MinValue;

            stageSequence =
                0;

            lastCompletedSceneHandle =
                int.MinValue;
        }


        // =========================================================
        // NUEVO SECTOR
        // =========================================================

        private static void SceneDirector_Start(
            On.RoR2.SceneDirector.orig_Start orig,
            SceneDirector self
        )
        {
            /*
             * Primero dejamos que RoR2 ejecute su Start normal.
             */
            orig(
                self
            );


            if (
                !NetworkServer.active ||
                Run.instance == null
            )
            {
                return;
            }


            Scene scene =
                SceneManager.GetActiveScene();


            string stageName =
                NormalizeStageName(
                    scene.name
                );


            int sceneHandle =
                scene.handle;


            /*
             * Dedupe:
             *
             * Si por cualquier razón Start se ejecutara más de una
             * vez para la misma escena, NO reiniciamos progreso
             * Stage dos veces.
             */
            if (
                sceneHandle ==
                    currentSceneHandle &&
                string.Equals(
                    stageName,
                    currentStageName,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return;
            }


            currentStageName =
                stageName;

            currentSceneHandle =
                sceneHandle;

            stageSequence++;

            lastCompletedSceneHandle =
                int.MinValue;


            /*
             * Aquí conectamos finalmente el soporte Stage
             * preparado en el Paso 3.
             */
            MissionProgressRegistry
                .NotifyStageStarted();


            MissionStageEventContext context =
                new MissionStageEventContext(
                    currentStageName,
                    currentSceneHandle,
                    stageSequence
                );


            logger?.LogInfo(
                "[MISSION STAGE] START | " +
                $"Stage: {currentStageName} | " +
                $"SceneHandle: {currentSceneHandle} | " +
                $"Sequence: {stageSequence}"
            );


            StageStarted?.Invoke(
                context
            );
        }


        // =========================================================
        // TELEPORTER FINISH
        // =========================================================

        private static void OnTeleporterFinishGlobal(
            TeleporterInteraction teleporter
        )
        {
            if (
                !NetworkServer.active ||
                Run.instance == null
            )
            {
                return;
            }


            Scene scene =
                SceneManager.GetActiveScene();


            string stageName =
                NormalizeStageName(
                    scene.name
                );


            int sceneHandle =
                scene.handle;


            /*
             * Si el evento llega antes de que SceneDirector.Start
             * haya registrado algo, tomamos la escena activa.
             */
            if (
                string.IsNullOrWhiteSpace(
                    currentStageName
                )
            )
            {
                currentStageName =
                    stageName;

                currentSceneHandle =
                    sceneHandle;
            }


            /*
             * Dedupe:
             * una escena sólo puede generar una finalización válida.
             */
            if (
                lastCompletedSceneHandle ==
                    sceneHandle
            )
            {
                return;
            }


            lastCompletedSceneHandle =
                sceneHandle;


            MissionStageEventContext context =
                new MissionStageEventContext(
                    currentStageName,
                    currentSceneHandle,
                    stageSequence,
                    teleporter
                );


            logger?.LogInfo(
                "[MISSION STAGE] COMPLETE | " +
                $"Stage: {currentStageName} | " +
                $"SceneHandle: {currentSceneHandle} | " +
                $"Sequence: {stageSequence}"
            );


            StageCompleted?.Invoke(
                context
            );
        }


        // =========================================================
        // CONSULTAS
        // =========================================================

        public static bool IsCurrentStage(
            string requiredStage
        )
        {
            return StageNamesMatch(
                currentStageName,
                requiredStage
            );
        }


        public static bool StageNamesMatch(
            string actualStage,
            string requiredStage
        )
        {
            if (
                string.IsNullOrWhiteSpace(
                    requiredStage
                )
            )
            {
                return true;
            }


            return string.Equals(
                NormalizeStageName(
                    actualStage
                ),
                NormalizeStageName(
                    requiredStage
                ),
                StringComparison.OrdinalIgnoreCase
            );
        }


        public static string NormalizeStageName(
            string stageName
        )
        {
            if (stageName == null)
            {
                return "";
            }


            return stageName
                .Trim()
                .ToLowerInvariant();
        }
    }
}
