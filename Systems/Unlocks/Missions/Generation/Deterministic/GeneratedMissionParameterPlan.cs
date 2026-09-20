namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Blueprint temporal de parámetros de G7.
    ///
    /// No es todavía MissionDefinition ni crea trackers. G8/G9 podrán
    /// materializar/validar estos valores cuando se conecte persistencia.
    /// </summary>
    public sealed class GeneratedMissionParameterPlan
    {
        public string ObjectiveKind
        {
            get;
            set;
        } = "";

        public int Amount
        {
            get;
            set;
        }

        public int RepeatCount
        {
            get;
            set;
        } = 1;

        public float TimeLimitSeconds
        {
            get;
            set;
        }

        public string ResetScope
        {
            get;
            set;
        } = "Run";

        public string PerformerPolicy
        {
            get;
            set;
        } = "AnyCompatibleUnlockedAlternative";

        public string Notes
        {
            get;
            set;
        } = "";
    }
}
