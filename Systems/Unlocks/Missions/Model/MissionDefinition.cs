using System.Collections.Generic;


namespace UniversalSurvivorUnlocks
{
    public class MissionDefinition
    {
        // Nueva generación del formato de misiones.
        //
        // SchemaVersion 1:
        // Type + Parameters antiguos.
        //
        // SchemaVersion 2:
        // Objective + Conditions + Routes.
        public int SchemaVersion { get; set; } = 2;


        // =========================================================
        // PROGRESO MULTIPLAYER
        // =========================================================
        //
        // PerPlayer:
        // cada jugador mantiene su propio progreso.
        //
        // Shared:
        // todos los jugadores contribuyen al mismo progreso.
        //
        // Ejemplos:
        //
        // HUNK:
        // PerPlayer
        //
        // Ralsei:
        // Shared
        //
        public string ProgressScope { get; set; } =
            "PerPlayer";


        // =========================================================
        // RECOMPENSA
        // =========================================================
        //
        // Actualmente USU utiliza recompensa de sesión:
        // si una ruta válida se completa, se desbloquea para todos
        // los perfiles conectados que todavía no lo tengan.
        // =========================================================

        public string RewardScope { get; set; } =
            "Session";


        // =========================================================
        // RUTAS
        // =========================================================
        //
        // Dentro de una ruta:
        //
        // Objective
        // +
        // Condition
        // +
        // Condition
        //
        // funcionan como AND.
        //
        // Entre distintas rutas:
        //
        // Route A
        // OR
        // Route B
        //
        // Ejemplo HUNK:
        //
        // Route A = Railgunner + weak points
        // OR
        // Route B = Bandit + Lights Out
        //
        // =========================================================

        public List<MissionRoute> Routes { get; set; } =
            new List<MissionRoute>();
    }
}