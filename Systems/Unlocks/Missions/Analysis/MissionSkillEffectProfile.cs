namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Efecto semántico observado en una skill.
    ///
    /// Detail conserva la pista concreta (por ejemplo damageCoefficient=4,
    /// BuffDef: Soaked o una keyword localizada) sin intentar convertirla en
    /// una fórmula de gameplay universal.
    /// </summary>
    public sealed class MissionSkillEffectProfile
    {
        public MissionSkillEffectKind Kind
        {
            get;
            set;
        } = MissionSkillEffectKind.Unknown;

        public MissionEvidenceConfidence Confidence
        {
            get;
            set;
        } = MissionEvidenceConfidence.Low;

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
    }
}
