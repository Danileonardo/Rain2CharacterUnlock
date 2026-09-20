namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Una observación comunitaria individual, investigada y curada offline.
    ///
    /// El DLL nunca descarga ni consulta SourceReference. Se conserva sólo
    /// como metadato auditable para poder reconstruir por qué una observación
    /// fue normalizada a C/B/A/S/S+.
    /// </summary>
    public sealed class CommunityPowerSource
    {
        public string SurvivorId
        {
            get;
            set;
        } = "";

        public string SourceId
        {
            get;
            set;
        } = "";

        public CommunityPowerSourceKind Kind
        {
            get;
            set;
        } = CommunityPowerSourceKind.Unknown;

        public CommunityPowerTier Tier
        {
            get;
            set;
        } = CommunityPowerTier.Unknown;

        /// <summary>
        /// Identificador de hilo/lista/publicación independiente. Varias
        /// opiniones del mismo hilo pueden registrarse para conservar el
        /// desacuerdo, pero ese hilo cuenta una sola vez como fuente
        /// independiente.
        /// </summary>
        public string IndependentGroupId
        {
            get;
            set;
        } = "";

        public string SourceTitle
        {
            get;
            set;
        } = "";

        /// <summary>
        /// URL o referencia humana. Nunca se abre desde runtime.
        /// </summary>
        public string SourceReference
        {
            get;
            set;
        } = "";

        /// <summary>
        /// Resumen breve de lo que realmente afirma la fuente.
        /// </summary>
        public string EvidenceLabel
        {
            get;
            set;
        } = "";

        /// <summary>
        /// Regla usada para transformar la observación a nuestro vocabulario:
        /// balanced -> A, strong/overtuned -> S, broken -> S+, etc.
        /// </summary>
        public string NormalizationBasis
        {
            get;
            set;
        } = "";

        /// <summary>
        /// Contexto de evaluación: General, Monsoon, Eclipse, etc.
        /// </summary>
        public string Context
        {
            get;
            set;
        } = "General";

        public string ObservedVersion
        {
            get;
            set;
        } = "";

        public string PublishedDate
        {
            get;
            set;
        } = "";

        /// <summary>
        /// Calidad/comparabilidad editorial 0..1. Opiniones antiguas,
        /// ambiguas o sin versión conocida pesan menos que evaluaciones
        /// explícitas y recientes de balance/potencia.
        /// </summary>
        public float Weight
        {
            get;
            set;
        } = 1f;

        /// <summary>
        /// Sólo evidencia cuyo criterio sea potencia/rendimiento/balance entra
        /// en CommunityPower. Diversión, estética o popularidad quedan fuera.
        /// </summary>
        public bool IsPowerFocused
        {
            get;
            set;
        } = true;
    }
}
