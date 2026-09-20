namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Clasificación diagnóstica de cómo participa una entrada de skill en
    /// el kit del survivor. No modifica loadouts ni gameplay.
    /// </summary>
    public enum MissionSkillUsageKind
    {
        Active = 0,
        Passive = 10,
        AuxiliaryActive = 20,
        LoadoutOption = 30
    }
}
