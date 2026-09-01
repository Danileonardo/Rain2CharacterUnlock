using RoR2;

namespace UniversalSurvivorUnlocks
{
    /*
     * =============================================================
     * SERVICIO GENÉRICO:
     * RequiredStage + CompleteStage
     * =============================================================
     *
     * Este archivo NO escucha eventos por sí mismo.
     *
     * Su función es dar a los futuros evaluadores/objetivos
     * una API única para:
     *
     * - verificar el sector actual,
     * - marcar CompleteStage para un jugador,
     * - marcar CompleteStage compartido,
     * - consultar si quedó completado.
     *
     * En el siguiente paso conectaremos este servicio con
     * Mission System v2 / presets.
     * =============================================================
     */
    public static class MissionStageObjectiveService
    {
        public const string DefaultCompleteStageObjectiveId =
            "CompleteStage";


        // =========================================================
        // REQUIRED STAGE
        // =========================================================

        public static bool MatchesRequiredStage(
            string requiredStage
        )
        {
            return MissionStageRuntimeTracker
                .IsCurrentStage(
                    requiredStage
                );
        }


        public static bool MatchesRequiredStage(
            MissionStageEventContext context,
            string requiredStage
        )
        {
            if (context == null)
            {
                return false;
            }


            return MissionStageRuntimeTracker
                .StageNamesMatch(
                    context.StageName,
                    requiredStage
                );
        }


        // =========================================================
        // COMPLETE STAGE - PER PLAYER
        // =========================================================

        public static bool MarkCompleteStageForPlayer(
            NetworkUser networkUser,
            string missionId,
            string requiredStage = "",
            string objectiveId =
                DefaultCompleteStageObjectiveId
        )
        {
            if (
                networkUser == null ||
                string.IsNullOrWhiteSpace(
                    missionId
                ) ||
                !MatchesRequiredStage(
                    requiredStage
                )
            )
            {
                return false;
            }


            return MissionProgressRegistry
                .SetObjectiveCompleted(
                    MissionProgressScope.PerPlayer,
                    networkUser,
                    missionId,
                    objectiveId,
                    true,
                    MissionProgressResetScope.Stage
                );
        }


        public static bool IsCompleteStageForPlayer(
            NetworkUser networkUser,
            string missionId,
            string objectiveId =
                DefaultCompleteStageObjectiveId
        )
        {
            if (
                networkUser == null ||
                string.IsNullOrWhiteSpace(
                    missionId
                )
            )
            {
                return false;
            }


            return MissionProgressRegistry
                .IsObjectiveCompleted(
                    MissionProgressScope.PerPlayer,
                    networkUser,
                    missionId,
                    objectiveId
                );
        }


        // =========================================================
        // COMPLETE STAGE - SHARED
        // =========================================================

        public static bool MarkCompleteStageShared(
            string missionId,
            string requiredStage = "",
            string objectiveId =
                DefaultCompleteStageObjectiveId
        )
        {
            if (
                string.IsNullOrWhiteSpace(
                    missionId
                ) ||
                !MatchesRequiredStage(
                    requiredStage
                )
            )
            {
                return false;
            }


            return MissionProgressRegistry
                .SetObjectiveCompleted(
                    MissionProgressScope.Shared,
                    null,
                    missionId,
                    objectiveId,
                    true,
                    MissionProgressResetScope.Stage
                );
        }


        public static bool IsCompleteStageShared(
            string missionId,
            string objectiveId =
                DefaultCompleteStageObjectiveId
        )
        {
            if (
                string.IsNullOrWhiteSpace(
                    missionId
                )
            )
            {
                return false;
            }


            return MissionProgressRegistry
                .IsObjectiveCompleted(
                    MissionProgressScope.Shared,
                    null,
                    missionId,
                    objectiveId
                );
        }
    }
}
