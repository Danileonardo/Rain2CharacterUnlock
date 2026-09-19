namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Entrada descubierta por el catálogo de efectos de estado.
    /// Permite que el editor futuro liste buffs/debuffs/DoT reales
    /// registrados por vanilla, DLC y mods.
    /// </summary>
    public class MissionStatusEffectDescriptor
    {
        // ID estable dentro de USU.
        // Ejemplos:
        // buff:bdWeak
        // dot:Poison
        // cc:Freeze
        public string Id { get; set; } =
            "";

        // PositiveBuff / NegativeBuff / Dot / CrowdControl.
        public string Family { get; set; } =
            "";

        // Nombre interno descubierto en RoR2.
        public string InternalName { get; set; } =
            "";

        // Por ahora puede coincidir con InternalName.
        // El editor podrá localizarlo con Language más adelante.
        public string DisplayName { get; set; } =
            "";

        public bool IsPositive { get; set; } =
            false;

        public bool IsNegative { get; set; } =
            false;
    }
}
