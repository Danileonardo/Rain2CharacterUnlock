namespace UniversalSurvivorUnlocks
{
    public class MissionRules
    {
        // El progreso se reinicia al comenzar una nueva run.
        public bool SingleRun { get; set; } =
            true;


        // Si la muerte del jugador reinicia el progreso.
        //
        // No significa "no morir".
        //
        // Esa condición podrá existir por separado.
        public bool ResetOnDeath { get; set; } =
            false;


        // Permite guardar aquí una semántica futura para streaks.
        //
        // Ejemplo:
        // HUNK puede utilizar un sistema donde fallar la acción
        // concreta reinicie solamente la racha.
        public bool IsStreak { get; set; } =
            false;
    }
}