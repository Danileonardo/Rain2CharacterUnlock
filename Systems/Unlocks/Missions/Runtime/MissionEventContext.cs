using RoR2;

namespace UniversalSurvivorUnlocks
{
    public class MissionEventContext
    {
        // Jugador al que pertenece la acción.
        public CharacterMaster PlayerMaster { get; set; }


        // Body actual del jugador propietario.
        public CharacterBody PlayerBody { get; set; }


        // Body que realizó físicamente la acción.
        //
        // Normalmente coincide con PlayerBody.
        // Para drones/minions puede ser diferente.
        //
        // Las condiciones de habilidad/backstab deben mirar este body,
        // no al propietario remoto del daño.
        public CharacterBody ActionBody { get; set; }


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


        // =========================================================
        // METADATOS DEL EVENTO
        // =========================================================
        //
        // El evaluador de condiciones no debe asumir que todo contexto
        // proviene de un Kill. Los futuros handlers Hit/Heal/UseSkill
        // podrán poblar estas banderas de forma explícita.
        // =========================================================

        public string EventType { get; set; } =
            "";


        public bool IsFatalHit { get; set; }


        // True únicamente cuando la acción ocurrió dentro de un
        // BlastAttack real observado por ExplosionKillTracker.
        public bool IsExplosiveDamage { get; set; }


        // BlastAttack asociado cuando existe.
        public BlastAttack BlastAttack { get; set; }
    }
}
