using System.Collections.Generic;

namespace UniversalSurvivorUnlocks
{
    public class MissionRoute
    {
        // =========================================================
        // ID ESTABLE DE LA RUTA
        // =========================================================
        //
        // Ejemplos:
        //
        // acrid_route
        // rex_route
        // railgunner_route
        // bandit_route
        //
        // Permite identificar una ruta aunque el usuario cambie
        // el orden visual dentro del editor.
        // =========================================================
        public string Id { get; set; } =
            "";


        // =========================================================
        // FORMATO LEGACY / COMPATIBILIDAD
        // =========================================================
        //
        // Schema v2 original permitía solamente:
        //
        // Route
        // └── Objective
        //
        // NO eliminamos esta propiedad porque presets antiguos
        // pueden seguir utilizándola.
        //
        // Cuando Objectives contiene uno o más elementos,
        // Objectives tiene prioridad sobre Objective.
        // =========================================================
        public MissionObjective Objective { get; set; }


        // =========================================================
        // OBJETIVOS DE LA RUTA
        // =========================================================
        //
        // TODOS los objetivos de una misma ruta deben completarse.
        //
        // Es decir:
        //
        // Objective A
        // AND
        // Objective B
        // AND
        // Objective C
        //
        // Ejemplo Wooper:
        //
        // KillWithSkillWhileStatus
        // AND
        // HoldItemStack
        // AND
        // CompleteStage
        //
        // =========================================================
        public List<MissionObjective> Objectives { get; set; } =
            new List<MissionObjective>();


        // =========================================================
        // CONDICIONES DE LA RUTA
        // =========================================================
        //
        // Todas deben cumplirse.
        //
        // Ejemplo:
        //
        // RequiredSurvivor = Acrid OR Rex
        // AND
        // RequiredStage = Wetland
        //
        // El OR de personajes NO se representa creando dos
        // RequiredSurvivor separados, porque dos condiciones dentro
        // de la misma ruta son AND.
        //
        // Se representa mediante:
        //
        // RequiredSurvivor
        // {
        //     "bodies": [
        //         "CrocoBody",
        //         "TreebotBody"
        //     ]
        // }
        //
        // =========================================================
        public List<MissionCondition> Conditions { get; set; } =
            new List<MissionCondition>();


        public MissionRules Rules { get; set; } =
            new MissionRules();


        // =========================================================
        // OBJETIVOS EFECTIVOS
        // =========================================================
        //
        // Compatibilidad automática:
        //
        // 1. Si existe Objectives[] con contenido:
        //      usamos Objectives[].
        //
        // 2. Si no:
        //      usamos Objective legacy.
        //
        // Esto permite migrar los presets uno por uno.
        // =========================================================
        public IReadOnlyList<MissionObjective>
            GetEffectiveObjectives()
        {
            if (
                Objectives != null &&
                Objectives.Count > 0
            )
            {
                return Objectives;
            }


            if (Objective != null)
            {
                return new MissionObjective[]
                {
                    Objective
                };
            }


            return new MissionObjective[0];
        }
    }
}
