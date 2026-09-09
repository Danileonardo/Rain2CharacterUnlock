using System;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Contextos que pertenecen a una INSTANCIA durante una run.
    ///
    /// No deben mezclarse con la identidad estática del BodyDef:
    /// un StoneTitanBody puede ser un Champion estático y, en una instancia
    /// concreta, ser además TeleporterBoss y/o Elite.
    /// </summary>
    [Flags]
    public enum ContentRuntimeTargetFlags
    {
        None = 0,
        TeleporterBoss = 1 << 0,
        Elite = 1 << 1,
        Umbra = 1 << 2,
        Summon = 1 << 3
    }
}
