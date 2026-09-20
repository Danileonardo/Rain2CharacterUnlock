namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Evidencia diagnóstica G4 para una mecánica que altera la potencia del
    /// kit sin corresponder necesariamente a un efecto directo de G3.
    /// </summary>
    public sealed class IndirectPowerSignal
    {
        public IndirectPowerSignalKind Kind
        {
            get;
            set;
        } = IndirectPowerSignalKind.Unknown;

        public string SkillStableId
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

        /// <summary>
        /// Aporte bruto antes de normalizar contra la referencia vanilla.
        /// </summary>
        public float RawContribution
        {
            get;
            set;
        }
    }
}
