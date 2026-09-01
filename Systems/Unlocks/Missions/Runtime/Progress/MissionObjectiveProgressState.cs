using System;

namespace UniversalSurvivorUnlocks
{
    /*
     * =============================================================
     * ESTADO DE UN OBJETIVO INDIVIDUAL
     * =============================================================
     *
     * Ejemplos futuros:
     *
     * Wooper:
     *     poisonedBiteKills
     *         Value = 4
     *
     *     holdBisonSteak
     *         Value = 2
     *
     *     completeStage
     *         Completed = true
     *
     * HUNK, si algún día se migra:
     *     railgunnerStreak
     *         Value = 17
     *
     * Este archivo NO conoce ningún personaje ni tracker.
     * =============================================================
     */
    public sealed class MissionObjectiveProgressState
    {
        public string ObjectiveId
        {
            get;
            private set;
        }


        public double Value
        {
            get;
            private set;
        }


        public bool Completed
        {
            get;
            private set;
        }


        public MissionProgressResetScope ResetScope
        {
            get;
            private set;
        }


        public MissionObjectiveProgressState(
            string objectiveId,
            MissionProgressResetScope resetScope
        )
        {
            ObjectiveId =
                objectiveId ?? "";

            ResetScope =
                resetScope;

            Value =
                0d;

            Completed =
                false;
        }


        public double Add(
            double amount
        )
        {
            Value +=
                amount;


            return Value;
        }


        public double SetValue(
            double value
        )
        {
            Value =
                value;


            return Value;
        }


        public void SetCompleted(
            bool completed
        )
        {
            Completed =
                completed;
        }


        /*
         * Si un mismo objetivo vuelve a declararse con Scope Stage
         * después de haber nacido como Run (o viceversa),
         * conservamos el dato pero actualizamos su política.
         *
         * Esto permite que el futuro editor reconstruya una misión
         * sin necesitar recrear manualmente todos los estados.
         */
        public void SetResetScope(
            MissionProgressResetScope resetScope
        )
        {
            ResetScope =
                resetScope;
        }


        public void Reset()
        {
            Value =
                0d;

            Completed =
                false;
        }
    }
}
