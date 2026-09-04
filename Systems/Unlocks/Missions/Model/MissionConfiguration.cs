using System;
using Newtonsoft.Json;

namespace UniversalSurvivorUnlocks
{
    public class MissionConfiguration
    {
        // =========================================================
        // PROVEEDOR ACTIVO
        // =========================================================
        //
        // null:
        //     Configuración creada antes del sistema de proveedores.
        //     EffectiveProvider mantiene compatibilidad y deduce el
        //     proveedor a partir de Source.
        //
        // Original:
        //     USU deja que el contenido original controle el unlock.
        //
        // USU:
        //     USU controla el unlock mediante su sistema de misiones.
        //
        // Community:
        //     Reservado para presets aportados por terceros/comunidad.
        //
        // Custom:
        //     Misión creada o modificada por el jugador.
        //
        // =========================================================

        [JsonProperty(
            "provider",
            Order = 1,
            NullValueHandling = NullValueHandling.Ignore
        )]
        public UnlockProviderKind? Provider
        {
            get;
            set;
        }


        // =========================================================
        // ORIGEN DE LA SELECCIÓN
        // =========================================================
        //
        // AutomaticFallback:
        //     USU se activó automáticamente porque no se detectó un
        //     sistema original utilizable.
        //
        // UserSelected:
        //     El jugador eligió explícitamente el proveedor.
        //
        // Esta diferencia permitirá que, si un creador añade después
        // su propio sistema, sólo los fallbacks automáticos regresen
        // al Original. Las elecciones del jugador se conservarán.
        // =========================================================

        [JsonProperty(
            "selectionMode",
            Order = 2,
            NullValueHandling = NullValueHandling.Ignore
        )]
        public UnlockSelectionMode? SelectionMode
        {
            get;
            set;
        }


        // =========================================================
        // FUENTE ACTIVA DE MISIÓN
        // =========================================================
        //
        // Preset
        // Custom
        //
        // Preset:
        //     La misión se obtiene desde la biblioteca mediante
        //     BasePresetId.
        //
        // Custom:
        //     Se utiliza CustomMission.
        //
        // Provider y Source son conceptos diferentes:
        // Provider indica QUIÉN controla el desbloqueo.
        // Source indica DE DÓNDE sale la misión de USU.
        // =========================================================

        [JsonProperty("source", Order = 3)]
        public string Source
        {
            get;
            set;
        } =
            "Preset";


        // =========================================================
        // PRESET BASE
        // =========================================================
        //
        // Identificador permanente del preset.
        //
        // Una misión personalizada también conserva este valor
        // cuando fue creada clonando un preset oficial.
        // =========================================================

        [JsonProperty("basePresetId", Order = 4)]
        public string BasePresetId
        {
            get;
            set;
        } =
            "";


        // =========================================================
        // MISIÓN PERSONALIZADA
        // =========================================================
        //
        // Sólo se utiliza cuando Source == "Custom".
        //
        // Cuando Source == "Preset", debe permanecer null.
        // =========================================================

        [JsonProperty("customMission", Order = 5)]
        public MissionDefinition CustomMission
        {
            get;
            set;
        }


        // =========================================================
        // AYUDAS DE COMPATIBILIDAD / RUNTIME
        // =========================================================

        [JsonIgnore]
        public bool UsesCustomMission
        {
            get
            {
                return
                    Source == "Custom" &&
                    CustomMission != null;
            }
        }


        /// <summary>
        /// Resuelve el proveedor incluso para Survivors.json antiguos.
        ///
        /// Antes de 5G.1D no existía Provider. Si Source era Custom,
        /// se interpreta como Custom; cualquier otra MissionConfiguration
        /// existente pertenece al sistema de USU.
        /// </summary>
        [JsonIgnore]
        public UnlockProviderKind EffectiveProvider
        {
            get
            {
                if (Provider.HasValue)
                {
                    return Provider.Value;
                }

                if (
                    string.Equals(
                        Source,
                        "Custom",
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    return UnlockProviderKind.Custom;
                }

                return UnlockProviderKind.USU;
            }
        }


        /// <summary>
        /// Las configuraciones anteriores a 5G.1D se consideran fallbacks
        /// automáticos porque todavía no existía una selección de proveedor
        /// realizada desde la futura interfaz de USU.
        /// </summary>
        [JsonIgnore]
        public UnlockSelectionMode EffectiveSelectionMode
        {
            get
            {
                return
                    SelectionMode
                    ?? UnlockSelectionMode.AutomaticFallback;
            }
        }
    }
}
