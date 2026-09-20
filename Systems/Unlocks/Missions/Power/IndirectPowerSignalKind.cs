namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Señales de potencia indirecta que G4 Phase 1.2 puede inferir desde
    /// descripciones/runtime sin convertirlas todavía en tiers ni misiones.
    /// </summary>
    public enum IndirectPowerSignalKind
    {
        Unknown = 0,
        MaxHealthDamage = 10,
        ExecuteThreshold = 20,
        DamageAmplification = 30,
        AttackSpeedBoost = 40,
        CooldownAcceleration = 50,
        RecastOrRepeat = 60,
        StrongMitigation = 70,
        InvulnerabilityOrNegation = 80,
        ArsenalOrResourceAccess = 90,
        WeakPointExposure = 100,
        ReactiveCounter = 110,
        StackingOrScaling = 120,
        TeamReposition = 130,
        ModeOrSubskillAccess = 140,
        PersistentFlight = 150,
        ForcedMovementControl = 160,
        ArmorShredOrBypass = 170,
        TeamDamageAmplification = 180,
        TeamDefenseBuff = 190,
        AllyCooldownReset = 200,
        ResurrectionAccess = 210,
        MultiHitOrChannelScaling = 220
    }
}
