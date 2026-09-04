using System.Collections.Generic;

namespace UniversalSurvivorUnlocks
{
    public class MissionPreset
    {
        // ID técnico permanente.
        // Nunca debe depender del idioma o nombre visible.
        public string PresetId { get; set; } =
            "";


        // Objective / Condition / Mission.
        public string Kind { get; set; } =
            MissionPresetKinds.Mission;


        // Categoría estable utilizada por la biblioteca/editor.
        public string Category { get; set; } =
            "";


        // Etiquetas auxiliares para búsqueda/filtros futuros.
        public List<string> Tags { get; set; } =
            new List<string>();


        // Survivor para el cual fue diseñado originalmente.
        // Vacío = preset genérico.
        public string TargetBody { get; set; } =
            "";


        public string Name { get; set; } =
            "";


        public string Description { get; set; } =
            "";


        // true:
        // el Mission Schema v2 actual ya puede ejecutar esta pieza.
        //
        // false:
        // la pieza forma parte del diseño del editor, pero todavía
        // necesita handler/evaluador v2 antes de poder activarse.
        public bool RuntimeSupported { get; set; } =
            false;


        // Indica que proviene de una mecánica/desafío antiguo.
        // No significa que deba eliminarse: se conserva para
        // compatibilidad/migración, pero la biblioteca no lo ofrece
        // como una nueva misión asignable.
        public bool IsLegacy { get; set; } =
            false;


        // =========================================================
        // TEMPLATE ATÓMICO: OBJECTIVE
        // =========================================================
        // Sólo se utiliza cuando Kind == Objective.
        public MissionObjective ObjectiveTemplate { get; set; }


        // =========================================================
        // TEMPLATE ATÓMICO: CONDITION
        // =========================================================
        // Sólo se utiliza cuando Kind == Condition.
        public MissionCondition ConditionTemplate { get; set; }


        // =========================================================
        // RECETA COMPLETA
        // =========================================================
        // Sólo se utiliza cuando Kind == Mission.
        //
        // Las recetas antiguas y las futuras misiones oficiales
        // viven aquí, pero siguen construidas con objetivos y
        // condiciones granulares.
        public MissionDefinition Mission { get; set; }
    }
}
