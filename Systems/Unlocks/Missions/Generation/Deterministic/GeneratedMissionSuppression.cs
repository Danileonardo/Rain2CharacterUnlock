namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Registro diagnóstico de un survivor que no necesita fallback G7 porque
    /// ya existe contenido autorado dentro de USU.
    /// </summary>
    public sealed class GeneratedMissionSuppression
    {
        public string SurvivorId
        {
            get;
            set;
        } = "";

        public string BodyName
        {
            get;
            set;
        } = "";

        public string DisplayName
        {
            get;
            set;
        } = "";

        public GeneratedMissionAuthorityKind AuthorityKind
        {
            get;
            set;
        } = GeneratedMissionAuthorityKind.None;

        public string Detail
        {
            get;
            set;
        } = "";
    }
}
