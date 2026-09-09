namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Estado del contenido respecto del perfil local.
    ///
    /// 5G.2D-A construye el catálogo y deja el contrato preparado.
    /// 5G.2D-B cruzará estos registros con LogBook/UserProfile para resolver
    /// Discovered/Undiscovered sin afectar el runtime de las misiones.
    /// </summary>
    public enum ContentCatalogDiscoveryState
    {
        Unknown = 0,
        Discovered = 1,
        Undiscovered = 2,
        NotApplicable = 3
    }
}
