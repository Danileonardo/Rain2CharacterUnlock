using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    /*
     * Snapshot temporal de las misiones efectivas
     * de una run.
     *
     * IMPORTANTE:
     * - No se guarda en disco.
     * - No reemplaza Survivors.json.
     * - El host crea la copia.
     * - Los clientes reciben exactamente esa copia.
     *
     * Guardamos cada SurvivorJsonEntry como JObject
     * para conservar campos presentes y futuros
     * (Mission System v2, parámetros nuevos, ExtraData, etc.)
     * sin tener que cambiar el protocolo de red cada vez.
     */
    public sealed class SessionMissionSnapshot
    {
        [JsonProperty("schemaVersion")]
        public int SchemaVersion
        {
            get;
            set;
        } = 1;


        [JsonProperty("missions")]
        public Dictionary<string, JObject> Missions
        {
            get;
            set;
        } =
            new Dictionary<string, JObject>();
    }
}
