using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Explica por qué un proveedor terminó seleccionado.
    ///
    /// Esta diferencia permite que USU entregue temporalmente un desbloqueo
    /// cuando el autor todavía no posee uno y, si aparece uno original en una
    /// actualización futura, pueda devolver el control automáticamente sin
    /// ignorar una elección explícita del jugador.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum UnlockSelectionMode
    {
        /// <summary>
        /// USU fue seleccionado automáticamente como fallback porque no se
        /// detectó un sistema original utilizable.
        /// </summary>
        AutomaticFallback,

        /// <summary>
        /// El jugador seleccionó explícitamente este proveedor.
        /// Las futuras detecciones no deben reemplazar su elección en silencio.
        /// </summary>
        UserSelected
    }
}
