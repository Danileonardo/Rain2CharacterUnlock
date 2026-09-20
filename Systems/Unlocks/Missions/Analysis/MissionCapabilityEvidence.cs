namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Evidencia diagnóstica de por qué G3 marcó una capacidad.
    ///
    /// Source describe el origen técnico de la señal (texto, metadata, etc.).
    /// Detail se mantiene deliberadamente pequeño y nunca se persiste.
    /// </summary>
    public sealed class MissionCapabilityEvidence
    {
        public MissionProfileCapability Capability
        {
            get;
            set;
        }

        public string SkillStableId
        {
            get;
            set;
        } = "";

        public string Source
        {
            get;
            set;
        } = "";

        public string Detail
        {
            get;
            set;
        } = "";

        public MissionEvidenceConfidence Confidence
        {
            get;
            set;
        } = MissionEvidenceConfidence.Low;
    }
}
