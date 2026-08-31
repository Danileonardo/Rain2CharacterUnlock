using RoR2;


namespace UniversalSurvivorUnlocks
{
    public class MissionEventContext
    {
        // Jugador al que pertenece la acción.
        public CharacterMaster PlayerMaster { get; set; }


        // Body actual del jugador.
        public CharacterBody PlayerBody { get; set; }


        // Entidad sobre la cual ocurrió la acción.
        //
        // Para Kill:
        // enemigo muerto.
        //
        // Para Heal:
        // aliado curado.
        //
        // etc.
        public CharacterBody TargetBody { get; set; }


        // Información del daño cuando corresponde.
        public DamageReport DamageReport { get; set; }
    }
}