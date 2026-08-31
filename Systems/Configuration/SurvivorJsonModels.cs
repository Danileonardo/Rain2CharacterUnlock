using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    public class SurvivorJsonFile
    {
        [JsonProperty("_guide", Order = 1)]
        public SurvivorJsonGuide Guide
        {
            get;
            set;
        } = new SurvivorJsonGuide();


        [JsonProperty("schemaVersion", Order = 2)]
        public int SchemaVersion
        {
            get;
            set;
        } = 1;


        [JsonProperty("_availableTitle", Order = 3)]
        public string AvailableTitle
        {
            get;
            set;
        } =
            "==================== PERSONAJES HABILITADOS ====================";


        [JsonProperty("_availableMessage", Order = 4)]
        public string AvailableMessage
        {
            get;
            set;
        } =
            "Estos personajes están disponibles actualmente y pueden recibir una misión de desbloqueo personalizada.";


        [JsonProperty("availableSurvivors", Order = 5)]
        public Dictionary<string, SurvivorJsonEntry> AvailableSurvivors
        {
            get;
            set;
        } =
            new Dictionary<string, SurvivorJsonEntry>();


        [JsonProperty("_unavailableTitle", Order = 6)]
        public string UnavailableTitle
        {
            get;
            set;
        } =
            "================== PERSONAJES NO HABILITADOS ==================";


        [JsonProperty("_unavailableMessage", Order = 7)]
        public string UnavailableMessage
        {
            get;
            set;
        } =
            "Estos personajes no están disponibles actualmente. " +
            "Su configuración se conserva, pero sus misiones no pueden " +
            "activarse hasta que vuelva a estar disponible su DLC, mod o dependencia.";


        [JsonProperty("unavailableSurvivors", Order = 8)]
        public Dictionary<string, SurvivorJsonEntry> UnavailableSurvivors
        {
            get;
            set;
        } =
            new Dictionary<string, SurvivorJsonEntry>();
    }


    // =============================================================
    // GUÍA DEL JSON
    // =============================================================

    public class SurvivorJsonGuide
    {
        [JsonProperty("title", Order = 1)]
        public string Title
        {
            get;
            set;
        } =
            "================ UNIVERSAL SURVIVOR UNLOCKS ================";


        [JsonProperty("instructions", Order = 2)]
        public List<string> Instructions
        {
            get;
            set;
        } =
            new List<string>
            {
                "Las misiones personalizadas se configuran dentro del bloque challenge de cada personaje habilitado.",
                "enabled = false mantiene desactivada la misión personalizada.",
                "enabled = true permite utilizar la misión configurada.",
                "name indica el nombre visible de la misión.",
                "description indica la descripción visible de la misión.",
                "type indica el tipo de misión.",
                "parameters contiene las condiciones específicas de esa misión.",
                "Los tipos de misión disponibles se implementarán progresivamente durante el desarrollo del mod.",
                "No es necesario modificar displayName, internalName, bodyName, source, status ni originalUnlock."
            };


        [JsonProperty("exampleChallenge", Order = 3)]
        public SurvivorChallengeJson ExampleChallenge
        {
            get;
            set;
        } =
            new SurvivorChallengeJson
            {
                Enabled = true,

                Name =
                    "Desafío de desbloqueo",

                Description =
                    "",

                Type =
                    "KillEnemies",

                Parameters =
                    new JObject
                    {
                        ["amount"] = 100
                    }
            };
    }


    // =============================================================
    // ENTRADA DE SURVIVOR
    // =============================================================

    public class SurvivorJsonEntry
    {
        [JsonProperty("displayName", Order = 1)]
        public string DisplayName
        {
            get;
            set;
        } = "";


        [JsonProperty("internalName", Order = 2)]
        public string InternalName
        {
            get;
            set;
        } = "";


        [JsonProperty("bodyName", Order = 3)]
        public string BodyName
        {
            get;
            set;
        } = "";


        [JsonProperty("source", Order = 4)]
        public string Source
        {
            get;
            set;
        } = "";


        [JsonProperty("originalUnlock", Order = 5)]
        public string OriginalUnlock
        {
            get;
            set;
        } = "";


        [JsonProperty("available", Order = 6)]
        public bool Available
        {
            get;
            set;
        }


        [JsonProperty("status", Order = 7)]
        public string Status
        {
            get;
            set;
        } = "";


        [JsonProperty("reason", Order = 8)]
        public string Reason
        {
            get;
            set;
        } = "";


        [JsonProperty("challenge", Order = 9)]
        public SurvivorChallengeJson Challenge
        {
            get;
            set;
        } =
            new SurvivorChallengeJson();


        [JsonExtensionData]
        public IDictionary<string, JToken> ExtraData
        {
            get;
            set;
        } =
            new Dictionary<string, JToken>();
    }


    // =============================================================
    // CONFIGURACIÓN DE MISIÓN
    // =============================================================

    public class SurvivorChallengeJson
    {
        [JsonProperty("enabled", Order = 1)]
        public bool Enabled
        {
            get;
            set;
        } = false;


        [JsonProperty("name", Order = 2)]
        public string Name
        {
            get;
            set;
        } = "";


        [JsonProperty("description", Order = 3)]
        public string Description
        {
            get;
            set;
        } = "";

        // =====================================================
        // LEGACY / SCHEMA V1
        // =====================================================

        [JsonProperty("type", Order = 4)]
        public string Type
        {
            get;
            set;
        } =
            "Original";


        [JsonProperty("parameters", Order = 5)]
        public JObject Parameters
        {
            get;
            set;
        } =
            new JObject();

        // =========================================================
        // MISSION SCHEMA V2
        // =========================================================
        //
        // Mientras Mission sea null, se utiliza el sistema
        // Type + Parameters actual.
        //
        // Esto permite migrar gradualmente los presets existentes
        // sin romper Survivors.json antiguos.
        // =========================================================

        public MissionDefinition Mission { get; set; }


        [JsonExtensionData]
        public IDictionary<string, JToken> ExtraData
        {
            get;
            set;
        } =
            new Dictionary<string, JToken>();
    }
}