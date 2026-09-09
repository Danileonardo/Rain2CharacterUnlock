namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Fuente técnica del UnlockableDef que está asociado al contenido.
    ///
    /// IMPORTANTE:
    /// Esto NO reemplaza el provider Original / USU / Custom de
    /// MissionAssignmentService. En 5G.2D-C sólo necesitamos distinguir
    /// contenido libre, unlock del creador y unlock administrado por USU.
    /// El provider exacto se enlazará al HUD en 5H/5I.
    /// </summary>
    public enum SurvivorContentUnlockSource
    {
        Unknown = 0,
        UnlockedByDefault = 1,
        Original = 2,
        UsuManaged = 3
    }
}
