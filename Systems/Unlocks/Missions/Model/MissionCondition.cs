using Newtonsoft.Json.Linq;


namespace UniversalSurvivorUnlocks
{
    public class MissionCondition
    {
        // =========================================================
        // TIPO
        // =========================================================
        //
        // Ejemplos:
        //
        // Airborne
        // Grounded
        // RequiredSurvivor
        // RequiredSkill
        // RequiredItem
        // RequiredEquipment
        // RequiredStage
        // CriticalHit
        // WeakPoint
        // Backstab
        // HealthBelow
        // NoDamage
        // NoHealing
        // NoDeath
        //
        // =========================================================

        public string Type { get; set; } =
            "";


        // Parámetros específicos de la condición.
        //
        // Ejemplo:
        //
        // RequiredSurvivor:
        //
        // {
        //     "body": "CommandoBody"
        // }
        //
        //
        // RequiredSkill:
        //
        // {
        //     "slot": "Special",
        //     "skillToken": "BANDIT2_SPECIAL_NAME"
        // }
        //
        //
        // RequiredItem:
        //
        // {
        //     "item": "SprintBonus",
        //     "amount": 5
        // }
        //
        public JObject Parameters { get; set; } =
            new JObject();
    }
}