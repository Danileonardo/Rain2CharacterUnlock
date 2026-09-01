using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    /*
     * =============================================================
     * SESSION MISSION SNAPSHOT
     * =============================================================
     *
     * Copia temporal de las misiones efectivas.
     *
     * Se usa para dos momentos distintos:
     *
     * LOBBY
     *     Configuración del host visible antes de iniciar la run.
     *
     * RUN
     *     Copia congelada de la configuración del lobby.
     *
     * IMPORTANTE:
     * - No se guarda en disco.
     * - No reemplaza Survivors.json.
     * - Cada entrada se conserva como JObject.
     *
     * Esto permite transportar campos presentes y futuros
     * sin tener que modificar el protocolo por cada nuevo
     * tipo de misión del creador.
     * =============================================================
     */
    public sealed class SessionMissionSnapshot
    {
        [JsonProperty("schemaVersion")]
        public int SchemaVersion
        {
            get;
            set;
        } = 2;


        [JsonProperty("missions")]
        public Dictionary<string, JObject> Missions
        {
            get;
            set;
        } =
            new Dictionary<string, JObject>();
    }
}
