namespace UniversalSurvivorUnlocks
{
    public class MissionConfiguration
    {
        // =========================================================
        // FUENTE ACTIVA
        // =========================================================
        //
        // Preset
        // Custom
        //
        // =========================================================

        public string Source { get; set; } =
            "Preset";


        // Preset original asociado.
        //
        // Incluso una misión Custom puede conservar este valor
        // para saber desde qué preset fue creada.
        public string BasePresetId { get; set; } =
            "";


        // Sólo se utiliza cuando Source == "Custom".
        //
        // Cuando Source == "Preset", la misión se obtiene desde
        // la biblioteca oficial mediante BasePresetId.
        public MissionDefinition CustomMission { get; set; }


        public bool UsesCustomMission
        {
            get
            {
                return
                    Source == "Custom" &&
                    CustomMission != null;
            }
        }
    }
}