namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Señal secundaria de potencia percibida por comunidad.
    ///
    /// Unknown es obligatorio cuando no existen suficientes fuentes de
    /// potencia comparables. Esta señal nunca reemplaza el análisis local.
    /// </summary>
    public enum CommunityPowerTier
    {
        Unknown = 0,
        C = 1,
        B = 2,
        A = 3,
        S = 4,
        SPlus = 5
    }
}
