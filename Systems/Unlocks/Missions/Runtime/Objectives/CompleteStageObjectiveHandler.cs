using System.Collections.Generic;

using BepInEx.Logging;

using RoR2;

using UnityEngine.Networking;


namespace UniversalSurvivorUnlocks
{
    /*
     * =============================================================
     * COMPLETE STAGE OBJECTIVE HANDLER
     * =============================================================
     *
     * Event-driven.
     *
     * Escucha:
     *
     * MissionStageRuntimeTracker.StageCompleted
     *
     * NO hace polling.
     * NO escanea enemigos.
     * NO conoce a Wooper.
     * =============================================================
     */
    public static class CompleteStageObjectiveHandler
    {
        private const string ObjectiveType =
            "CompleteStage";


        private static ManualLogSource logger;

        private static bool initialized;


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


            MissionStageRuntimeTracker
                .StageCompleted +=
                OnStageCompleted;


            logger?.LogInfo(
                "[MISSION V2] CompleteStage handler inicializado."
            );
        }


        // =========================================================
        // STAGE COMPLETE
        // =========================================================

        private static void OnStageCompleted(
            MissionStageEventContext stageContext
        )
        {
            if (
                !NetworkServer.active ||
                Run.instance == null ||
                !MissionProgressRegistry.IsRunActive
            )
            {
                return;
            }


            /*
             * Filtro barato.
             *
             * Si ninguna misión efectiva utiliza CompleteStage:
             * salimos inmediatamente.
             */
            if (
                !MissionRuntimeActivityPlan
                    .IsTypeActive(
                        ObjectiveType
                    ) ||
                !MissionRuntimeCatalog
                    .HasObjectiveType(
                        ObjectiveType
                    )
            )
            {
                return;
            }


            IReadOnlyList<
                MissionObjectiveRuntimeBinding
            > bindings =
                MissionRuntimeCatalog
                    .GetObjectives(
                        ObjectiveType
                    );


            for (
                int bindingIndex = 0;
                bindingIndex < bindings.Count;
                bindingIndex++
            )
            {
                MissionObjectiveRuntimeBinding
                    binding =
                        bindings[
                            bindingIndex
                        ];


                if (
                    binding == null ||
                    binding.Mission == null
                )
                {
                    continue;
                }


                MissionProgressScope progressScope =
                    MissionProgressKeyBuilder
                        .GetProgressScope(
                            binding.Mission
                        );


                /*
                 * SHARED:
                 *
                 * Basta con que exista un jugador elegible para
                 * esta ruta.
                 *
                 * Ejemplo:
                 *
                 * RequiredSurvivor = Acrid OR Rex
                 *
                 * Si cualquiera está presente y cumple las demás
                 * conditions, el CompleteStage shared puede marcarse.
                 */
                if (
                    progressScope ==
                        MissionProgressScope.Shared
                )
                {
                    TryCompleteShared(
                        binding,
                        stageContext
                    );

                    continue;
                }


                /*
                 * PER PLAYER:
                 *
                 * Cada jugador recibe su propio CompleteStage.
                 */
                foreach (
                    PlayerCharacterMasterController controller
                    in PlayerCharacterMasterController.instances
                )
                {
                    CharacterMaster master =
                        controller?.master;


                    if (master == null)
                    {
                        continue;
                    }


                    TryCompleteForPlayer(
                        binding,
                        master,
                        stageContext
                    );
                }
            }
        }


        // =========================================================
        // SHARED
        // =========================================================

        private static void TryCompleteShared(
            MissionObjectiveRuntimeBinding binding,
            MissionStageEventContext stageContext
        )
        {
            foreach (
                PlayerCharacterMasterController controller
                in PlayerCharacterMasterController.instances
            )
            {
                CharacterMaster master =
                    controller?.master;


                if (master == null)
                {
                    continue;
                }


                MissionEventContext context =
                    BuildContext(
                        master
                    );


                /*
                 * El CompositionEvaluator comprobará todas las
                 * conditions de la ruta antes de escribir.
                 */
                MissionCompositionProgressResult
                    result =
                        MissionCompositionProgressEvaluator
                            .MarkObjectiveCompleted(
                                binding.MissionId,
                                binding.Mission,
                                binding.RouteIndex,
                                binding.ObjectiveIndex,
                                context
                            );


                if (!result.Accepted)
                {
                    continue;
                }


                GenericMissionDispatcher
                    .HandleProgressResult(
                        binding,
                        result,
                        $"Stage: {stageContext?.StageName ?? ""}"
                    );


                /*
                 * El estado es Shared.
                 *
                 * No necesitamos volver a marcar el mismo objetivo
                 * utilizando otro jugador.
                 */
                return;
            }
        }


        // =========================================================
        // PER PLAYER
        // =========================================================

        private static void TryCompleteForPlayer(
            MissionObjectiveRuntimeBinding binding,
            CharacterMaster master,
            MissionStageEventContext stageContext
        )
        {
            MissionEventContext context =
                BuildContext(
                    master
                );


            MissionCompositionProgressResult result =
                MissionCompositionProgressEvaluator
                    .MarkObjectiveCompleted(
                        binding.MissionId,
                        binding.Mission,
                        binding.RouteIndex,
                        binding.ObjectiveIndex,
                        context
                    );


            GenericMissionDispatcher
                .HandleProgressResult(
                    binding,
                    result,
                    $"Stage: {stageContext?.StageName ?? ""}"
                );
        }


        private static MissionEventContext BuildContext(
            CharacterMaster master
        )
        {
            return new MissionEventContext
            {
                PlayerMaster =
                    master,

                PlayerBody =
                    master?.GetBody()
            };
        }
    }
}
