using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    public class MissionObjective
    {
        // =========================================================
        // ID ESTABLE
        // =========================================================
        //
        // Ejemplos:
        //
        // poisoned_bite_kills
        // bison_steak_stack
        // complete_stage
        //
        // No debe depender del texto visible ni del idioma.
        // =========================================================
        public string Id { get; set; } =
            "";


        // =========================================================
        // TIPO
        // =========================================================
        public string Type { get; set; } =
            "";


        // Cantidad necesaria.
        public double Amount { get; set; } =
            1d;


        // =========================================================
        // RESET SCOPE
        // =========================================================
        //
        // Run:
        //     conserva progreso durante toda la run.
        //
        // Stage:
        //     MissionProgressRegistry lo reinicia al comenzar
        //     un nuevo sector.
        //
        // Se guarda como string para que el JSON/editor siga siendo
        // extensible y legible.
        //
        // Presets antiguos que no tengan este campo usarán Run.
        // =========================================================
        public string ResetScope { get; set; } =
            "Run";


        // Qué entidad debe recibir/cumplir el objetivo.
        public MissionTarget Target { get; set; } =
            new MissionTarget();


        // =========================================================
        // CONDITIONS DEL OBJETIVO
        // =========================================================
        //
        // Estas condiciones sólo afectan a ESTA acción.
        //
        // Ejemplo Wooper:
        //
        // Kill x5
        //   + RequiredSkill = Bite
        //   + StatusPresent = Poison
        //
        // mientras HoldItemStack x2 y CompleteStage NO heredan
        // esas condiciones.
        //
        // Las condiciones globales de la ruta continúan viviendo en
        // MissionRoute.Conditions.
        // =========================================================
        public List<MissionCondition> Conditions { get; set; } =
            new List<MissionCondition>();


        // Parámetros particulares del tipo.
        public JObject Parameters { get; set; } =
            new JObject();
    }
}
