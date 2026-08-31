namespace UniversalSurvivorUnlocks
{
    public class MissionPreset
    {
        // ID técnico permanente.
        //
        // Nunca debe depender del idioma o nombre visible.
        public string PresetId { get; set; } =
            "";


        // Survivor para el cual fue diseñado originalmente.
        public string TargetBody { get; set; } =
            "";


        public string Name { get; set; } =
            "";


        public string Description { get; set; } =
            "";


        // Definición original de la misión.
        //
        // El usuario NO debe modificar esta instancia.
        public MissionDefinition Mission { get; set; } =
            new MissionDefinition();
    }
}