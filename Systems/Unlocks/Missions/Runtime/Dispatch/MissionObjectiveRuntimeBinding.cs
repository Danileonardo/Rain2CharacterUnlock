namespace UniversalSurvivorUnlocks
{
    /*
     * =============================================================
     * MISSION OBJECTIVE RUNTIME BINDING
     * =============================================================
     *
     * Representa un Objective ya resuelto dentro de:
     *
     * Survivor
     *   -> Mission
     *      -> Route
     *         -> Objective
     *
     * Se construye UNA VEZ al comenzar la run.
     *
     * Los handlers no necesitan:
     *
     * - releer JSON,
     * - buscar la ruta,
     * - buscar el índice,
     * - reconstruir IDs.
     * =============================================================
     */
    public sealed class MissionObjectiveRuntimeBinding
    {
        public string BodyName
        {
            get;
            internal set;
        } = "";


        /*
         * Actualmente el MissionId estable de una misión de
         * desbloqueo de survivor es el body que será desbloqueado.
         *
         * Ejemplo:
         *
         * WooperBody
         */
        public string MissionId
        {
            get;
            internal set;
        } = "";


        public MissionDefinition Mission
        {
            get;
            internal set;
        }


        public MissionRoute Route
        {
            get;
            internal set;
        }


        public MissionObjective Objective
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


        public string ObjectiveType =>
            Objective?.Type ?? "";
    }
}
