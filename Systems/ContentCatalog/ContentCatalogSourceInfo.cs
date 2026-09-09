namespace UniversalSurvivorUnlocks
{
    public sealed class ContentCatalogSourceInfo
    {
        public string ContentPackIdentifier
        {
            get;
            set;
        } = "";

        public string AssemblyName
        {
            get;
            set;
        } = "";

        public ContentCatalogSourceKind Kind
        {
            get;
            set;
        } = ContentCatalogSourceKind.Unknown;

        public string DisplayLabel
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(AssemblyName))
                {
                    return AssemblyName;
                }

                if (!string.IsNullOrWhiteSpace(ContentPackIdentifier))
                {
                    return ContentPackIdentifier;
                }

                return Kind.ToString();
            }
        }
    }
}
