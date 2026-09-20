namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// G4 Phase 1.3.2 - señales cuantitativas extraídas de una skill.
    ///
    /// Estas señales describen magnitud/uptime, no sustituyen la taxonomía
    /// semántica de G3. G3 sigue diciendo QUÉ hace la habilidad; esta capa
    /// intenta estimar CUÁNTO aporta cuando el texto/runtime expone números.
    /// </summary>
    public enum SkillMagnitudeSignalKind
    {
        None = 0,
        DamagePercent = 1,
        MultiHit = 2,
        CriticalLeverage = 3,
        ExecutionLeverage = 4,
        MaxHealthDamagePercent = 5,
        HealingPercent = 6,
        BarrierPercent = 7,
        InvulnerabilityWindow = 8,
        InvisibilityWindow = 9,
        CrowdControlDuration = 10,
        CooldownEfficiency = 11,
        CooldownReset = 12,
        ResourceGain = 13,
        ResourceCost = 14,
        ScalingPerTarget = 15,
        ScalingPerStack = 16,
        AreaCoverage = 17,
        DamageMultiplier = 18,
        ConditionalSynergy = 19,
        AttackSpeedBonusPercent = 20,
        MoveSpeedBonusPercent = 21,
        ArmorBonusFlat = 22,
        ArmorReductionFlat = 23,
        DamageReductionPercent = 24
    }
}
