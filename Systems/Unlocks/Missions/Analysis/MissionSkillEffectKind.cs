namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Semántica descriptiva de una habilidad instalada.
    ///
    /// No representa poder ni dificultad. G3 sólo describe qué parece hacer
    /// la skill usando evidencia runtime/textual; G4/G5 decidirán después cómo
    /// consumir esta información.
    /// </summary>
    public enum MissionSkillEffectKind
    {
        Unknown = 0,
        Damage = 10,
        Healing = 20,
        StatusEffect = 30,
        CrowdControl = 40,
        Mobility = 50,
        Airborne = 55,
        Defense = 60,
        SelfBuff = 65,
        SummonMinion = 70,
        Resource = 80,
        CriticalHit = 90,
        Backstab = 100,
        Execution = 110,
        Projectile = 120,
        AreaEffect = 130,
        Melee = 140,
        Utility = 150
    }
}
