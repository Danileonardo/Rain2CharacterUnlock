using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Indica quién controla actualmente el desbloqueo de un objetivo.
    ///
    /// Esta base se utilizará primero para survivors y, más adelante,
    /// podrá reutilizarse para skills, skins, armas y otros desbloqueables.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum UnlockProviderKind
    {
        /// <summary>
        /// Se utiliza exactamente el comportamiento definido por el mod
        /// o contenido original. USU no debe reemplazarlo ni destruirlo.
        /// </summary>
        Original,

        /// <summary>
        /// El desbloqueo es administrado por Universal Survivor Unlocks.
        /// Puede ser un preset oficial de USU o un fallback automático.
        /// </summary>
        USU,

        /// <summary>
        /// El desbloqueo procede de una biblioteca/preset aportado por
        /// terceros o por la comunidad.
        /// </summary>
        Community,

        /// <summary>
        /// El desbloqueo utiliza una misión creada o editada por el jugador.
        /// </summary>
        Custom
    }
}
