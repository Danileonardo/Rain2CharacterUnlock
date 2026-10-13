namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Contenido autorado que puede existir antes de que G7 ofrezca un
    /// fallback generado. G7 nunca reemplaza silenciosamente estas fuentes.
    /// </summary>
    public enum GeneratedMissionAuthorityKind
    {
        None = 0,
        UsuOfficialPreset = 10,
        UsuCustom = 20,
        ExistingUsuMission = 30,
        OriginalUnlock = 40
    }
}
