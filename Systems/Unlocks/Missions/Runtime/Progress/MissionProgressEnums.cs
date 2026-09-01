namespace UniversalSurvivorUnlocks
{
    /*
     * =============================================================
     * ALCANCE DEL PROGRESO
     * =============================================================
     *
     * Shared:
     *     Todos los jugadores alimentan el mismo estado.
     *
     * PerPlayer:
     *     Cada jugador mantiene su propio estado.
     *
     * Wooper utilizará PerPlayer.
     * =============================================================
     */
    public enum MissionProgressScope
    {
        Shared = 0,
        PerPlayer = 1
    }


    /*
     * =============================================================
     * MOMENTO EN QUE SE REINICIA UN OBJETIVO
     * =============================================================
     *
     * Run:
     *     Permanece durante toda la run.
     *
     * Stage:
     *     Se reinicia cuando el sistema de misiones notifique
     *     que comenzó un nuevo sector.
     *
     * El registro YA soporta Stage.
     * La notificación automática se conectará en el Paso 4.
     * =============================================================
     */
    public enum MissionProgressResetScope
    {
        Run = 0,
        Stage = 1
    }
}
