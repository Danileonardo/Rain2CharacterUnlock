namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Fuerza del encaje entre un survivor y una familia de misión.
    /// No equivale a ChallengeTier ni a CommunityPowerTier.
    /// </summary>
    public enum MissionTemplateCompatibilityLevel
    {
        Incompatible = 0,
        Weak = 1,
        Compatible = 2,
        Strong = 3,
        Preferred = 4
    }
}
