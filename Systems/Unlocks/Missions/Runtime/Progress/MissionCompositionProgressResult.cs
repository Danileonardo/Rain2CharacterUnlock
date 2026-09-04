namespace UniversalSurvivorUnlocks
{
    /*
     * =============================================================
     * RESULTADO DE UNA ACTUALIZACIÓN DE PROGRESO COMPUESTO
     * =============================================================
     *
     * Permite que los futuros Objective Handlers sepan:
     *
     * - si el evento fue aceptado,
     * - cuánto progreso quedó,
     * - si terminó el objetivo,
     * - si terminó la ruta,
     * - si terminó la misión,
     * - si la misión terminó JUSTO con este evento.
     *
     * Ese último flag será el que después conectaremos con
     * SessionUnlockManager.
     * =============================================================
     */
    public sealed class MissionCompositionProgressResult
    {
        public bool Accepted
        {
            get;
            internal set;
        }


        public int RouteIndex
        {
            get;
            internal set;
        } = -1;


        public int ObjectiveIndex
        {
            get;
            internal set;
        } = -1;


        public string RouteId
        {
            get;
            internal set;
        } = "";


        public string ObjectiveId
        {
            get;
            internal set;
        } = "";


        public string StorageObjectiveId
        {
            get;
            internal set;
        } = "";

        public double PreviousValue
        {
            get;
            internal set;
        }

        public double CurrentValue
        {
            get;
            internal set;
        }


        public double RequiredValue
        {
            get;
            internal set;
        }


        public bool ObjectiveCompleted
        {
            get;
            internal set;
        }


        public bool ObjectiveJustCompleted
        {
            get;
            internal set;
        }


        public bool RouteCompleted
        {
            get;
            internal set;
        }


        public bool RouteJustCompleted
        {
            get;
            internal set;
        }


        public bool MissionCompleted
        {
            get;
            internal set;
        }


        public bool MissionJustCompleted
        {
            get;
            internal set;
        }


        public MissionProgressScope ProgressScope
        {
            get;
            internal set;
        } =
            MissionProgressScope.PerPlayer;


        public MissionProgressResetScope ResetScope
        {
            get;
            internal set;
        } =
            MissionProgressResetScope.Run;


        public static MissionCompositionProgressResult Rejected()
        {
            return new MissionCompositionProgressResult
            {
                Accepted =
                    false
            };
        }
    }
}
