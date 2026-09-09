namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Tipos de contenido que el navegador visual de USU puede exponer.
    ///
    /// No todos tienen editor todavía. La enum se deja preparada desde
    /// 5G.2D-A para no cambiar IDs/persistencia cuando lleguen Skills/Skins.
    /// </summary>
    public enum ContentCatalogKind
    {
        Unknown = 0,
        Survivor = 1,
        Skill = 2,
        Item = 3,
        Equipment = 4,
        Enemy = 5,
        Boss = 6,
        Stage = 7,
        Interactable = 8,
        Skin = 9,
        Weapon = 10,
        Other = 100
    }
}
