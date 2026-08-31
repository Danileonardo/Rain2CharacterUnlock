namespace UniversalSurvivorUnlocks
{
    public class MissionTarget
    {
        // =========================================================
        // CATEGORÍA
        // =========================================================
        //
        // Ejemplos:
        //
        // AnyEnemy
        // Enemy
        // Elite
        // Boss
        // TeleporterBoss
        // SpecificBody
        // Ally
        // Self
        //
        // =========================================================

        public string Category { get; set; } =
            "Any";


        // ID interno estable del contenido.
        //
        // IMPORTANTE:
        //
        // Aquí jamás guardaremos:
        //
        // "Lemuriano"
        // "Mithrix"
        //
        // sino:
        //
        // "LemurianBody"
        // "BrotherBody"
        //
        // El RuntimeCatalog futuro se encargará del nombre
        // localizado y de la imagen.
        public string Id { get; set; } =
            "";
    }
}