using System.Collections.Generic;


namespace UniversalSurvivorUnlocks
{
    public class MissionRoute
    {
        // Qué debe hacer el jugador.
        //
        // Ejemplos:
        // Kill
        // DealDamage
        // Heal
        // CompleteStage
        public MissionObjective Objective { get; set; }


        // Requisitos adicionales.
        //
        // Todos los elementos de esta lista deben cumplirse.
        //
        // Ejemplo:
        //
        // Kill
        // +
        // Airborne
        // +
        // RequiredSurvivor = Commando
        public List<MissionCondition> Conditions { get; set; } =
            new List<MissionCondition>();


        // Reglas generales de esta ruta.
        public MissionRules Rules { get; set; } =
            new MissionRules();
    }
}