using System;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    public static partial class BuiltInMissionPresetCatalog
    {
        // =========================================================
        // RECETAS LEGACY / COMPATIBILIDAD
        // =========================================================

        private static void RegisterLegacyAndCreatorRecipes()
        {
            Register(
                MissionPresetIds.DefaultKillEnemies100,
                CreateDefaultKillEnemies100
            );

            Register(
                MissionPresetIds.StatusEffects100,
                CreateStatusEffects100
            );

            Register(
                MissionPresetIds.TeamHealing10000,
                CreateTeamHealing10000
            );

            Register(
                MissionPresetIds.BossCritical44444,
                CreateBossCritical44444
            );

            Register(
                MissionPresetIds.EnergyDrinks15,
                CreateEnergyDrinks15
            );

            Register(
                MissionPresetIds.BossBackstab,
                CreateBossBackstab
            );

            Register(
                MissionPresetIds.AirborneExplosionKills15,
                CreateAirborneExplosionKills15
            );

            Register(
                MissionPresetIds.RailgunnerWeakPoints24,
                CreateRailgunnerWeakPoints24
            );

            Register(
                MissionPresetIds.BanditLightsOutKills24,
                CreateBanditLightsOutKills24
            );

            Register(
                MissionPresetIds.ScrapItems6,
                CreateScrapItems6
            );

            Register(
                MissionPresetIds.ShatteringJustice1,
                CreateShatteringJustice1
            );

            Register(
                MissionPresetIds.AlloyBlastCanisterFinisher,
                CreateAlloyBlastCanisterFinisher
            );
        }

        private static MissionPreset CreateDefaultKillEnemies100()
        {
            return CreateMissionPreset(
                MissionPresetIds.DefaultKillEnemies100,
                MissionPresetCategories.Legacy,
                "",
                "Desafío genérico: 100 enemigos",
                "Derrota 100 enemigos durante una partida.",
                true,
                true,
                "PerPlayer",
                CreateRoute(
                    "kill_enemies_100",
                    new MissionObjective[]
                    {
                        CreateKillObjective(
                            "kill_enemies",
                            "Enemy",
                            "",
                            100d
                        )
                    },
                    null,
                    null
                )
            );
        }

        private static MissionPreset CreateStatusEffects100()
        {
            MissionObjective objective =
                CreateApplyStatusObjective(
                    "status_effects",
                    100d,
                    "AnyValid",
                    ""
                );


            objective.Parameters[
                "activeSimultaneously"
            ] =
                true;


            return CreateMissionPreset(
                MissionPresetIds.StatusEffects100,
                MissionPresetCategories.Legacy,
                "SoraBody",
                "100 efectos simultáneos",
                "Mantén 100 efectos de estado válidos activos simultáneamente.",
                false,
                true,
                "Shared",
                CreateRoute(
                    "status_effects_100",
                    new MissionObjective[]
                    {
                        objective
                    },
                    null,
                    null
                )
            );
        }

        private static MissionPreset CreateTeamHealing10000()
        {
            return CreateMissionPreset(
                MissionPresetIds.TeamHealing10000,
                MissionPresetCategories.Legacy,
                "RalseiBody",
                "Curación de equipo 10000",
                "Restaura 10000 de salud total al equipo en una partida.",
                false,
                true,
                "Shared",
                CreateRoute(
                    "team_healing_10000",
                    new MissionObjective[]
                    {
                        new MissionObjective
                        {
                            Id = "team_healing",
                            Type = "HealHealth",
                            Amount = 10000d,
                            ResetScope = "Run",
                            Target = new MissionTarget
                            {
                                Category = "Ally"
                            }
                        }
                    },
                    null,
                    null
                )
            );
        }

        private static MissionPreset CreateBossCritical44444()
        {
            MissionObjective objective =
                CreateKillObjective(
                    "boss_kill",
                    "Boss",
                    "",
                    1d
                );


            AddObjectiveConditions(
                objective,
                CreateCondition(
                    "CriticalHit",
                    new JObject()
                ),
                CreateCondition(
                    "MinimumDamage",
                    new JObject
                    {
                        ["damage"] = 4444d
                    }
                ),
                CreateCondition(
                    "FatalHit",
                    new JObject()
                ),
                CreateExcludedSurvivorCondition(
                    "RailgunnerBody"
                )
            );


            return CreateMissionPreset(
                MissionPresetIds.BossCritical44444,
                MissionPresetCategories.Legacy,
                "JhinBody",
                "Crítico mortal de 4444",
                "Mata a un jefe con un crítico de al menos 4444 de daño.",
                true,
                true,
                "PerPlayer",
                CreateRoute(
                    "boss_critical_44444",
                    new MissionObjective[]
                    {
                        objective
                    },
                    null,
                    null
                )
            );
        }

        private static MissionPreset CreateEnergyDrinks15()
        {
            return CreateMissionPreset(
                MissionPresetIds.EnergyDrinks15,
                MissionPresetCategories.Legacy,
                "ScoutBody",
                "15 Bebidas energéticas",
                "Mantén 15 Bebidas energéticas durante una partida.",
                true,
                true,
                "PerPlayer",
                CreateRoute(
                    "energy_drinks_15",
                    new MissionObjective[]
                    {
                        new MissionObjective
                        {
                            Id = "energy_drinks",
                            Type = "HoldItemStack",
                            Amount = 15d,
                            ResetScope = "Run",
                            Parameters = new JObject
                            {
                                ["item"] = "SprintBonus"
                            }
                        }
                    },
                    null,
                    null
                )
            );
        }

        private static MissionPreset CreateBossBackstab()
        {
            MissionObjective objective =
                CreateKillObjective(
                    "boss_kill",
                    "Boss",
                    "",
                    1d
                );


            AddObjectiveConditions(
                objective,
                CreateRequiredSkillCondition(
                    "Secondary",
                    "BANDIT2_SECONDARY_NAME"
                ),
                CreateCondition(
                    "Backstab",
                    new JObject()
                ),
                CreateCondition(
                    "CriticalHit",
                    new JObject()
                ),
                CreateCondition(
                    "FatalHit",
                    new JObject()
                )
            );


            return CreateMissionPreset(
                MissionPresetIds.BossBackstab,
                MissionPresetCategories.Legacy,
                "SpyBody",
                "Backstab mortal a un jefe",
                "Mata a un jefe con Daga serrada mediante un Backstab de Bandit.",
                true,
                true,
                "PerPlayer",
                CreateRoute(
                    "boss_backstab",
                    new MissionObjective[]
                    {
                        objective
                    },
                    new MissionCondition[]
                    {
                        CreateRequiredSurvivorCondition(
                            "Bandit2Body"
                        )
                    },
                    null
                )
            );
        }

        private static MissionPreset CreateAirborneExplosionKills15()
        {
            MissionObjective objective =
                CreateKillObjective(
                    "airborne_explosion_kills",
                    "Enemy",
                    "",
                    15d
                );


            objective.Parameters[
                "countOwnedMinions"
            ] =
                true;


            objective.Parameters[
                "resetOnGround"
            ] =
                true;


            AddObjectiveConditions(
                objective,
                CreateCondition(
                    "Airborne",
                    new JObject()
                ),
                CreateCondition(
                    "DamageType",
                    new JObject
                    {
                        ["damageType"] = "Explosive"
                    }
                )
            );


            return CreateMissionPreset(
                MissionPresetIds.AirborneExplosionKills15,
                MissionPresetCategories.Legacy,
                "RocketSurvivorBody",
                "15 bajas explosivas en el aire",
                "Consigue 15 bajas explosivas sin tocar el suelo.",
                false,
                true,
                "PerPlayer",
                CreateRoute(
                    "airborne_explosion_kills_15",
                    new MissionObjective[]
                    {
                        objective
                    },
                    null,
                    new MissionRules
                    {
                        SingleRun = true,
                        IsStreak = true
                    }
                )
            );
        }

        private static MissionPreset CreateRailgunnerWeakPoints24()
        {
            MissionObjective objective =
                new MissionObjective
                {
                    Id = "weak_point_hits",
                    Type = "Hit",
                    Amount = 24d,
                    ResetScope = "Run",
                    Target = new MissionTarget
                    {
                        Category = "Enemy"
                    }
                };


            AddObjectiveConditions(
                objective,
                CreateCondition(
                    "WeakPoint",
                    new JObject()
                )
            );


            return CreateMissionPreset(
                MissionPresetIds.RailgunnerWeakPoints24,
                MissionPresetCategories.Legacy,
                "RobHunkBody",
                "24 puntos débiles - Railgunner",
                "Acierta 24 puntos débiles consecutivos con Railgunner.",
                false,
                true,
                "PerPlayer",
                CreateRoute(
                    "railgunner_weak_points_24",
                    new MissionObjective[]
                    {
                        objective
                    },
                    new MissionCondition[]
                    {
                        CreateRequiredSurvivorCondition(
                            "RailgunnerBody"
                        )
                    },
                    new MissionRules
                    {
                        SingleRun = true,
                        IsStreak = true
                    }
                )
            );
        }

        private static MissionPreset CreateBanditLightsOutKills24()
        {
            MissionObjective objective =
                CreateKillObjective(
                    "lights_out_kills",
                    "Enemy",
                    "",
                    24d
                );


            AddObjectiveConditions(
                objective,
                CreateRequiredSkillCondition(
                    "Special",
                    "BANDIT2_SPECIAL_NAME"
                ),
                CreateCondition(
                    "FatalHit",
                    new JObject()
                )
            );


            return CreateMissionPreset(
                MissionPresetIds.BanditLightsOutKills24,
                MissionPresetCategories.Legacy,
                "RobHunkBody",
                "24 bajas con Luces fuera - Bandit",
                "Consigue 24 bajas consecutivas con Luces fuera de Bandit.",
                false,
                true,
                "PerPlayer",
                CreateRoute(
                    "bandit_lights_out_24",
                    new MissionObjective[]
                    {
                        objective
                    },
                    new MissionCondition[]
                    {
                        CreateRequiredSurvivorCondition(
                            "Bandit2Body"
                        )
                    },
                    new MissionRules
                    {
                        SingleRun = true,
                        IsStreak = true
                    }
                )
            );
        }

        private static MissionPreset CreateScrapItems6()
        {
            return CreateMissionPreset(
                MissionPresetIds.ScrapItems6,
                MissionPresetCategories.Legacy,
                "TinkatonBody",
                "Convertir 6 objetos en chatarra",
                "Convierte 6 objetos en Scrap durante la partida.",
                true,
                true,
                "PerPlayer",
                CreateRoute(
                    "scrap_items_6",
                    new MissionObjective[]
                    {
                        new MissionObjective
                        {
                            Id = "scrap_items",
                            Type = "ScrapItems",
                            Amount = 6d,
                            ResetScope = "Run"
                        }
                    },
                    null,
                    null
                )
            );
        }

        private static MissionPreset CreateShatteringJustice1()
        {
            return CreateMissionPreset(
                MissionPresetIds.ShatteringJustice1,
                MissionPresetCategories.Legacy,
                "TinkatonBody",
                "Tener Justicia demoledora",
                "Mantén 1 Justicia demoledora en el inventario.",
                true,
                true,
                "PerPlayer",
                CreateRoute(
                    "shattering_justice_1",
                    new MissionObjective[]
                    {
                        new MissionObjective
                        {
                            Id = "shattering_justice",
                            Type = "HoldItemStack",
                            Amount = 1d,
                            ResetScope = "Run",
                            Parameters = new JObject
                            {
                                ["item"] = "ArmorReductionOnHit"
                            }
                        }
                    },
                    null,
                    null
                )
            );
        }

        private static MissionPreset CreateAlloyBlastCanisterFinisher()
        {
            MissionObjective objective =
                CreateKillObjective(
                    "alloy_unit_kill",
                    "SpecificBoss",
                    "SuperRoboBallBossBody",
                    1d
                );


            AddObjectiveConditions(
                objective,
                CreateRequiredSkillCondition(
                    "Secondary",
                    "TOOLBOT_SECONDARY_NAME"
                ),
                CreateCondition(
                    "FatalHit",
                    new JObject()
                )
            );


            return CreateMissionPreset(
                MissionPresetIds.AlloyBlastCanisterFinisher,
                MissionPresetCategories.Legacy,
                "TinkatonBody",
                "Unidad de Aleación con Bote explosivo",
                "Mata a la Unidad de Aleación con Bote explosivo de MUL-T.",
                true,
                true,
                "PerPlayer",
                CreateRoute(
                    "alloy_blast_canister_finisher",
                    new MissionObjective[]
                    {
                        objective
                    },
                    new MissionCondition[]
                    {
                        CreateRequiredSurvivorCondition(
                            "ToolbotBody"
                        )
                    },
                    null
                )
            );
        }
    }
}
