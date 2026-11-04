using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    public static partial class BuiltInMissionPresetCatalog
    {
        private static MissionPreset CreateSeamstressOfficial()
        {
            // Pulso crítico: REX consume salud al utilizar su habilidad secundaria.
            // Todos los objetivos deben ocurrir durante la misma partida
            // Y en Profundidades Abisales; no exige un jefe específico.
            var spendHealth = new MissionObjective
            {
                Id = "seamstress_rex_spend_health",
                Type = "UseSkill",
                Amount = 1d,
                ResetScope = "Run"
            };
            AddObjectiveConditions(
                spendHealth,
                CreateRequiredSurvivorCondition("TreebotBody"),
                CreateCondition("RequiredSkill", new JObject
                {
                    ["slot"] = "Secondary"
                })
            );

            // HealthComponent.Heal informa la curación efectiva.
            // El handler limita esta variante a autocuración real de REX,
            // sin regeneración, tras usar una habilidad de coste vital.
            // HealthComponent.Heal no expone su causa exacta: véase INSTRUCCIONES.
            var restoredHealth = new MissionObjective
            {
                Id = "seamstress_rex_heal_1000",
                Type = "Heal",
                Amount = 1000d,
                ResetScope = "Run",
                Parameters = new JObject
                {
                    ["afterObjectiveId"] = "seamstress_rex_spend_health",
                    ["selfHealingOnly"] = true
                }
            };
            AddObjectiveConditions(
                restoredHealth,
                CreateRequiredSurvivorCondition("TreebotBody"),
                CreateCondition("AllowedPerformerSkill", new JObject
                {
                    ["options"] = new JArray(
                        new JObject { ["body"] = "TreebotBody", ["slot"] = "Primary" },
                        new JObject { ["body"] = "TreebotBody", ["slot"] = "Utility" },
                        new JObject { ["body"] = "TreebotBody", ["slot"] = "Special" }
                    )
                })
            );

            var boss = CreateKillObjective(
                "seamstress_boss_after_healing",
                "TeleporterBoss", "", 1d);
            boss.ResetScope = "Run";
            boss.Parameters = new JObject
            {
                ["afterObjectiveId"] = "seamstress_rex_heal_1000"
            };

            return MissionModeConfiguration.Mixed(CreateMissionPreset(
                MissionPresetIds.SeamstressOfficial,
                MissionPresetCategories.CharacterRecipe,
                "SeamstressBody",
                "Pulso crítico",
                "Un pulso prestado resiste las Profundidades;\n" +
                "gasta vida, cura 1.000 PV y vence al jefe.",
                true, false, "Shared",
                CreateRoute(
                    "seamstress_critical_pulse",
                    new[] { spendHealth, restoredHealth, boss },
                    new[]
                    {
                        CreateCondition("RequiredStage", new JObject
                        {
                            ["stage"] = "dampcavesimple"
                        })
                    },
                    new MissionRules { SingleRun = true })
            ));
        }
    }
}
