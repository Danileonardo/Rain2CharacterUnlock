using System.Collections.Generic;

namespace UniversalSurvivorUnlocks
{
    /*
     * =============================================================
     * PROGRESO DE UNA MISIÓN
     * =============================================================
     *
     * MissionId será normalmente el body que se desbloquea:
     *
     *     "WooperBody"
     *
     * Dentro se guardan objetivos independientes:
     *
     *     "RequiredSurvivor"
     *     "RequiredStage"
     *     "PoisonedBiteKills"
     *     "BisonSteakStack"
     *     "CompleteStage"
     *
     * Los nombres definitivos de objetivos los decidirá
     * Mission System v2 / el creador.
     * =============================================================
     */
    public sealed class MissionProgressState
    {
        public string MissionId
        {
            get;
            private set;
        }


        public bool Completed
        {
            get;
            private set;
        }


        public Dictionary<
            string,
            MissionObjectiveProgressState
        > Objectives
        {
            get;
            private set;
        }


        public MissionProgressState(
            string missionId
        )
        {
            MissionId =
                missionId ?? "";

            Objectives =
                new Dictionary<
                    string,
                    MissionObjectiveProgressState
                >();

            Completed =
                false;
        }


        public MissionObjectiveProgressState
            GetOrCreateObjective(
                string objectiveId,
                MissionProgressResetScope resetScope
            )
        {
            if (
                string.IsNullOrWhiteSpace(
                    objectiveId
                )
            )
            {
                return null;
            }


            if (
                Objectives.TryGetValue(
                    objectiveId,
                    out MissionObjectiveProgressState state
                ) &&
                state != null
            )
            {
                state.SetResetScope(
                    resetScope
                );


                return state;
            }


            state =
                new MissionObjectiveProgressState(
                    objectiveId,
                    resetScope
                );


            Objectives[
                objectiveId
            ] =
                state;


            return state;
        }


        public bool TryGetObjective(
            string objectiveId,
            out MissionObjectiveProgressState state
        )
        {
            state =
                null;


            if (
                string.IsNullOrWhiteSpace(
                    objectiveId
                )
            )
            {
                return false;
            }


            return Objectives.TryGetValue(
                objectiveId,
                out state
            ) &&
            state != null;
        }


        public void SetCompleted(
            bool completed
        )
        {
            Completed =
                completed;
        }


        // =========================================================
        // RESET DE OBJETIVOS DE STAGE
        // =========================================================

        public int ResetStageScopedObjectives()
        {
            int resetCount =
                0;


            foreach (
                MissionObjectiveProgressState state
                in Objectives.Values
            )
            {
                if (
                    state == null ||
                    state.ResetScope !=
                        MissionProgressResetScope.Stage
                )
                {
                    continue;
                }


                state.Reset();

                resetCount++;
            }


            return resetCount;
        }
    }
}
