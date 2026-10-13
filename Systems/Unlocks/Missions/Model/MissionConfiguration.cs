using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Copia dormida de una misión Generated administrada por USU.
    ///
    /// No contiene MissionConfig para evitar recursión. Permite alternar
    /// Generated &lt;-&gt; USU/Custom/Original sin destruir la misión generada
    /// anterior, su nonce ni su presentación.
    /// </summary>
    public sealed class GeneratedMissionSnapshot
    {
        [JsonProperty("enabled", Order = 1)]
        public bool Enabled { get; set; } = true;

        [JsonProperty("name", Order = 2)]
        public string Name { get; set; } = "";

        [JsonProperty("description", Order = 3)]
        public string Description { get; set; } = "";

        [JsonProperty("localizationKey", Order = 4)]
        public string LocalizationKey { get; set; } = "";

        [JsonProperty("lunarCoinReward", Order = 5)]
        public int LunarCoinReward { get; set; } = 0;

        [JsonProperty("type", Order = 6)]
        public string Type { get; set; } = "GeneratedMission";

        [JsonProperty("parameters", Order = 7)]
        public JObject Parameters { get; set; } = new JObject();

        [JsonProperty("mission", Order = 8)]
        public MissionDefinition Mission { get; set; }

        [JsonProperty("generatedId", Order = 9)]
        public string GeneratedId { get; set; } = "";

        [JsonExtensionData]
        public IDictionary<string, JToken> ExtraData { get; set; } =
            new Dictionary<string, JToken>();
    }


    /// <summary>
    /// Snapshot completo de presentación/metadata de una misión Custom dormida.
    /// CustomMission se conserva como campo canónico de compatibilidad; este
    /// snapshot permite restaurar también nombre, descripción y parámetros al
    /// alternar entre fuentes.
    /// </summary>
    public sealed class CustomMissionSnapshot
    {
        [JsonProperty("enabled", Order = 1)]
        public bool Enabled { get; set; } = true;

        [JsonProperty("name", Order = 2)]
        public string Name { get; set; } = "";

        [JsonProperty("description", Order = 3)]
        public string Description { get; set; } = "";

        [JsonProperty("localizationKey", Order = 4)]
        public string LocalizationKey { get; set; } = "";

        [JsonProperty("lunarCoinReward", Order = 5)]
        public int LunarCoinReward { get; set; } = 0;

        [JsonProperty("type", Order = 6)]
        public string Type { get; set; } = "MissionV2";

        [JsonProperty("parameters", Order = 7)]
        public JObject Parameters { get; set; } = new JObject();

        [JsonProperty("mission", Order = 8)]
        public MissionDefinition Mission { get; set; }

        [JsonProperty("basePresetId", Order = 9)]
        public string BasePresetId { get; set; } = "";

        [JsonExtensionData]
        public IDictionary<string, JToken> ExtraData { get; set; } =
            new Dictionary<string, JToken>();
    }


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
        // Generated
        // Custom
        //
        // Preset:
        //     La misión se obtiene desde la biblioteca mediante
        //     BasePresetId.
        //
        // Generated:
        //     Mission fue creada por G7/G8 y se refresca automáticamente
        //     mientras el jugador no la convierta en Custom.
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
        // SLOT USU RECORDADO
        // =========================================================
        //
        // Conserva el último preset USU seleccionable incluso cuando la
        // fuente activa pasa a Generated o Custom. Así el botón USU puede
        // volver a activarlo sin destruir las demás fuentes.
        // =========================================================

        [JsonProperty("usuPresetId", Order = 5)]
        public string UsuPresetId
        {
            get;
            set;
        } =
            "";


        // =========================================================
        // SLOT GENERATED DORMIDO
        // =========================================================
        //
        // Copia administrada de la última Generated. Sólo se usa para poder
        // volver a Generated después de seleccionar USU/Custom/Original.
        // No es una Custom y puede volver a actualizarse/rerollearse cuando
        // Generated recupere la autoridad.
        // =========================================================

        [JsonProperty(
            "generatedMission",
            Order = 6,
            NullValueHandling = NullValueHandling.Ignore
        )]
        public GeneratedMissionSnapshot GeneratedMission
        {
            get;
            set;
        }


        // Sólo se usa para el modo temporal de pruebas 0.3.1. Cuando existe
        // una alternativa Original/USU, el usuario puede borrar el slot
        // Generated y evitar que vuelva a recrearse automáticamente.
        [JsonProperty(
            "generatedSuppressed",
            Order = 7,
            DefaultValueHandling = DefaultValueHandling.Ignore
        )]
        public bool GeneratedSuppressed
        {
            get;
            set;
        }


        [JsonProperty(
            "customMissionSnapshot",
            Order = 8,
            NullValueHandling = NullValueHandling.Ignore
        )]
        public CustomMissionSnapshot CustomMissionSnapshot
        {
            get;
            set;
        }


        // =========================================================
        // MISIÓN PERSONALIZADA
        // =========================================================
        //
        // Sólo se utiliza cuando Source == "Custom".
        //
        // Cuando Source == "Preset" puede conservar una copia dormida.
        // Cambiar de proveedor no debe borrar la misión personalizada.
        // RestoreCharacter es la operación que elimina esa copia al ejecutar
        // "Restaurar original" para el personaje completo.
        // =========================================================

        [JsonProperty("customMission", Order = 9)]
        public MissionDefinition CustomMission
        {
            get;
            set;
        }


        // =========================================================
        // CAMPOS DESCONOCIDOS / COMPATIBILIDAD HACIA ADELANTE
        // =========================================================
        //
        // Conserva propiedades de missionConfig que una versión más
        // nueva pueda haber escrito y esta build todavía no conozca.
        // =========================================================

        [JsonExtensionData]
        public IDictionary<string, JToken> ExtraData
        {
            get;
            set;
        } =
            new Dictionary<string, JToken>();


        // =========================================================
        // AYUDAS DE COMPATIBILIDAD / RUNTIME
        // =========================================================

        [JsonIgnore]
        public bool UsesGeneratedMission
        {
            get
            {
                return string.Equals(
                    Source,
                    "Generated",
                    StringComparison.OrdinalIgnoreCase
                );
            }
        }


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
