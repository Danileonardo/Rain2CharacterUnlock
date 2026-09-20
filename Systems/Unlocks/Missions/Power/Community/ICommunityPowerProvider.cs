namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Provider desacoplado para señal de comunidad.
    /// </summary>
    public interface ICommunityPowerProvider
    {
        bool TryGet(
            string survivorId,
            out CommunityPowerAssessment assessment
        );
    }
}
