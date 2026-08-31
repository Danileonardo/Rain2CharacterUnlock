using Newtonsoft.Json.Linq;


namespace UniversalSurvivorUnlocks
{
    public class MissionObjective
    {
        // =========================================================
        // TIPO DE OBJETIVO
        // =========================================================
        //
        // Ejemplos futuros:
        //
        // Kill
        // DealDamage
        // Heal
        // CompleteStage
        // CompleteTeleporter
        // CollectItem
        // HoldItem
        // Survive
        // Interact
        //
        // Lo dejamos como string a propósito para poder agregar
        // nuevos tipos sin cambiar el formato del JSON.
        // =========================================================

        public string Type { get; set; } =
            "";


        // Cantidad necesaria.
        //
        // double permite:
        //
        // Kill = 25
        // Heal = 10000
        // Damage = 44444.5
        //
        // aunque algunos objetivos solamente aceptarán enteros.
        public double Amount { get; set; } =
            1d;


        // Qué entidad debe recibir/cumplir el objetivo.
        public MissionTarget Target { get; set; } =
            new MissionTarget();


        // Parámetros particulares de objetivos futuros.
        //
        // Ejemplo Multikill:
        //
        // {
        //     "windowSeconds": 1.0
        // }
        //
        // Así no necesitamos modificar esta clase cada vez
        // que aparezca un objetivo nuevo.
        public JObject Parameters { get; set; } =
            new JObject();
    }
}