using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Describe qué comportamiento original se ha detectado para un objetivo.
    ///
    /// En este paso sólo se define el modelo. La detección real se añadirá en
    /// una fase posterior para survivors y después para skills, skins y armas.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OriginalUnlockState
    {
        /// <summary>
        /// Todavía no se ha podido determinar con seguridad el comportamiento.
        /// </summary>
        Unknown,

        /// <summary>
        /// El autor/mod posee un sistema de desbloqueo propio.
        /// </summary>
        HasUnlockSystem,

        /// <summary>
        /// El comportamiento original es que el contenido esté disponible
        /// desde el inicio, sin misión de desbloqueo.
        /// </summary>
        UnlockedByDefault
    }
}
