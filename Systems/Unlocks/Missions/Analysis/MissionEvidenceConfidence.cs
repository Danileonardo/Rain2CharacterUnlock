namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Confianza de una señal G3.
    ///
    /// High   = metadata/objeto runtime explícito.
    /// Medium = nombre/tipo/miembro runtime fuertemente indicativo.
    /// Low    = inferencia textual conservadora.
    /// </summary>
    public enum MissionEvidenceConfidence
    {
        Low = 0,
        Medium = 10,
        High = 20
    }
}
