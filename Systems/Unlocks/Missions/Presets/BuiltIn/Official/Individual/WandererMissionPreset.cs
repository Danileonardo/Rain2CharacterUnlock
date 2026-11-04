using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    public static partial class BuiltInMissionPresetCatalog
    {
        private static MissionPreset CreateWandererOfficial()
        {
            // El ejecutor de cada uno de los tres golpes finales debe ser Mercenary
            // (MercBody), en el aire. Los jefes son aleatorios y pueden repetirse.
            // Progreso por run; no requiere que el jefe esté en el aire.
            MissionObjective lastBreath = CreateKillObjective(
                "wanderer_airborne_teleporter_bosses_3", "TeleporterBoss", "", 3d);
            lastBreath.ResetScope = "Run";
            AddObjectiveConditions(lastBreath,
                CreateCondition("Airborne", new JObject()));

            return MissionModeConfiguration.Individual(CreateMissionPreset(
                MissionPresetIds.WandererOfficial,
                MissionPresetCategories.CharacterRecipe,
                "UnforgivenBody",
                "Último aliento",
                "El viento reclama tres últimos alientos;\n" +
                "remata a 3 jefes del portal desde el aire.",
                true, false, "PerPlayer",
                CreateRoute("wanderer_last_breath",
                    new[] { lastBreath },
                    new[] { CreateRequiredSurvivorCondition("MercBody") },
                    new MissionRules { SingleRun = true })));
        }
    }
}
