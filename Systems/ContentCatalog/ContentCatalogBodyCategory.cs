namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Clasificación adicional para CharacterBody no-survivor.
    ///
    /// Importante:
    /// - Boss/Enemy siguen viviendo en ContentCatalogKind.
    /// - Umbra, Elite y TeleporterBoss son CONTEXTOS runtime, no cuerpos
    ///   diferentes. Se resuelven en ContentRuntimeContextTracker.
    /// </summary>
    public enum ContentCatalogBodyCategory
    {
        NotApplicable = 0,
        Enemy = 1,
        Champion = 2,
        Drone = 3,
        Summon = 4,
        Decoy = 5,
        Special = 6,
        Internal = 100
    }
}
