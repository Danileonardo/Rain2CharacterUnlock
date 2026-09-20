namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Señales observables que G3 puede extraer de un survivor instalado.
    ///
    /// No representan dificultad ni poder. G4/G5 consumirán estas señales
    /// posteriormente; G3 sólo describe capacidades con evidencia runtime.
    /// </summary>
    public enum MissionProfileCapability
    {
        Unknown = 0,
        Damage = 10,
        Mobility = 20,
        Airborne = 30,
        StatusEffect = 40,
        SummonMinion = 50,
        Healing = 60,
        Defense = 70,
        SelfBuff = 75,
        ResourceMechanic = 80,
        CriticalHit = 90,
        Backstab = 100,
        Execution = 110
    }
}
