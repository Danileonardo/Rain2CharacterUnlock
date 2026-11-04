using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    public static partial class BuiltInMissionPresetCatalog
    {
        private static MissionPreset CreateHelPOfficial()
        {
            // Cada requisito es un objetivo de Mission V2 independiente
            // con progreso personal durante UNA partida.
            var healParts = new MissionObjective
            {
                Id = "help_desmantelar_2_drones_sanadores",
                Type = "HelPScrapHealing", Amount = 2d, ResetScope = "Run"
            };
            var attackParts = new MissionObjective
            {
                Id = "help_desmantelar_2_drones_artilleros",
                Type = "HelPScrapGunner", Amount = 2d, ResetScope = "Run"
            };
            var emergencyRepair = new MissionObjective
            {
                Id = "help_reparar_1_dron_emergencia",
                Type = "HelPRepairEmergency", Amount = 1d, ResetScope = "Run"
            };
            var emergencyHeal = new MissionObjective
            {
                Id = "help_activar_curacion_dron_emergencia",
                Type = "HelPEmergencyBeam", Amount = 1d, ResetScope = "Run"
            };

            return MissionModeConfiguration.Individual(CreateMissionPreset(
                MissionPresetIds.HelPOfficial,
                MissionPresetCategories.CharacterRecipe,
                "RobUltronBody",
                "Protocolo HEL-P: Reconstrucción",
                "Renace: desguaza 2 drones médicos y 2 artilleros;\n" +
                "repara un dron de emergencia y activa su cura.",
                true, false, "PerPlayer",
                CreateRoute("help_reconstruction_personal_v2",
                    new[] { healParts, attackParts, emergencyRepair, emergencyHeal },
                    // Engineer debe realizar la reconstrucción y activar el dron.
                    // La ruta individual (PerPlayer) atribuye todas las acciones
                    // a quien completa la misión: EngiBody, no otro compañero.
                    new[] { CreateRequiredSurvivorCondition("EngiBody") },
                    new MissionRules { SingleRun = true })
            ));
        }
    }
}
