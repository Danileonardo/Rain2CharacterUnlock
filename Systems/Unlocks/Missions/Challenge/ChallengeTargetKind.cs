namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Tipo de contenido al que puede pertenecer un challenge futuro.
    ///
    /// G5 Phase 1 sólo resuelve Survivor. Los demás valores quedan como
    /// contrato explícito para la futura expansión a skills, skins, armas u
    /// otros desbloqueables sin mezclar sus reglas de dificultad.
    /// </summary>
    public enum ChallengeTargetKind
    {
        Survivor = 0,
        Skill = 1,
        Skin = 2,
        Weapon = 3,
        Other = 4
    }
}
