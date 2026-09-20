namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Dimensiones locales que G4 Phase 1 calcula desde el MissionProfile
    /// construido por G3.
    ///
    /// No son ChallengeTier ni CommunityPowerTier. Sólo describen cuánto
    /// sobresale el kit observado respecto de una referencia vanilla runtime.
    /// </summary>
    public enum LocalPowerDimension
    {
        Offense = 10,
        Sustain = 20,
        Defense = 30,
        Mobility = 40,
        Control = 50,
        Utility = 60
    }
}
