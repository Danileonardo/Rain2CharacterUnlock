namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Evidencia cuantitativa puntual observada en una habilidad.
    /// </summary>
    public sealed class SkillMagnitudeSignal
    {
        public SkillMagnitudeSignalKind Kind
        {
            get;
            set;
        } = SkillMagnitudeSignalKind.None;

        public float Value
        {
            get;
            set;
        }

        public MissionEvidenceConfidence Confidence
        {
            get;
            set;
        } = MissionEvidenceConfidence.Low;

        public string Detail
        {
            get;
            set;
        } = "";
    }
}
