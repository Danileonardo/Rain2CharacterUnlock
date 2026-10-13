using System;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    public static partial class BuiltInMissionPresetCatalog
    {
        // =========================================================
        // BLOQUES - OBJETIVOS BASE
        // =========================================================

        private static void RegisterBaseObjectivePresets()
        {
            Register(
                MissionPresetIds.KillEnemies,
                () => CreateObjectivePreset(
                    MissionPresetIds.KillEnemies,
                    MissionPresetCategories.Combat,
                    "Matar enemigos",
                    "Derrota una cantidad configurable de enemigos.",
                    true,
                    CreateKillObjective(
                        "kill_enemies",
                        "Enemy",
                        "",
                        1d
                    )
                )
            );


            Register(
                MissionPresetIds.KillElite,
                () => CreateObjectivePreset(
                    MissionPresetIds.KillElite,
                    MissionPresetCategories.Combat,
                    "Matar élites",
                    "Derrota una cantidad configurable de enemigos élite.",
                    true,
                    CreateKillObjective(
                        "kill_elites",
                        "Elite",
                        "",
                        1d
                    )
                )
            );


            Register(
                MissionPresetIds.KillBoss,
                () => CreateObjectivePreset(
                    MissionPresetIds.KillBoss,
                    MissionPresetCategories.Combat,
                    "Matar jefes",
                    "Derrota una cantidad configurable de jefes.",
                    true,
                    CreateKillObjective(
                        "kill_bosses",
                        "Boss",
                        "",
                        1d
                    )
                )
            );


            Register(
                MissionPresetIds.KillSpecificBody,
                () => CreateObjectivePreset(
                    MissionPresetIds.KillSpecificBody,
                    MissionPresetCategories.Combat,
                    "Matar enemigo específico",
                    "Derrota un Body concreto seleccionado desde el catálogo.",
                    true,
                    CreateKillObjective(
                        "kill_specific_body",
                        "SpecificBody",
                        "",
                        1d
                    )
                )
            );


            Register(
                MissionPresetIds.HitTarget,
                () => CreateObjectivePreset(
                    MissionPresetIds.HitTarget,
                    MissionPresetCategories.Combat,
                    "Golpear objetivo",
                    "Registra impactos sobre un objetivo configurable.",
                    false,
                    new MissionObjective
                    {
                        Id = "hit_target",
                        Type = "Hit",
                        Amount = 1d,
                        ResetScope = "Run",
                        Target = new MissionTarget
                        {
                            Category = "Enemy"
                        }
                    }
                )
            );


            Register(
                MissionPresetIds.ReachLevel,
                () => CreateObjectivePreset(
                    MissionPresetIds.ReachLevel,
                    MissionPresetCategories.Progression,
                    "Alcanzar nivel",
                    "Alcanza un nivel configurable durante la partida.",
                    false,
                    new MissionObjective
                    {
                        Id = "reach_level",
                        Type = "ReachLevel",
                        Amount = 1d,
                        ResetScope = "Run",
                        Parameters = new JObject
                        {
                            ["level"] = 1
                        }
                    }
                )
            );


            Register(
                MissionPresetIds.ReachStage,
                () => CreateObjectivePreset(
                    MissionPresetIds.ReachStage,
                    MissionPresetCategories.Progression,
                    "Alcanzar fase",
                    "Alcanza un número de fase configurable.",
                    false,
                    new MissionObjective
                    {
                        Id = "reach_stage",
                        Type = "ReachStage",
                        Amount = 1d,
                        ResetScope = "Run",
                        Parameters = new JObject
                        {
                            ["stage"] = 1
                        }
                    }
                )
            );


            Register(
                MissionPresetIds.CompleteStage,
                () => CreateObjectivePreset(
                    MissionPresetIds.CompleteStage,
                    MissionPresetCategories.Progression,
                    "Completar sector",
                    "Completa el sector actual o uno seleccionado.",
                    true,
                    new MissionObjective
                    {
                        Id = "complete_stage",
                        Type = "CompleteStage",
                        Amount = 1d,
                        ResetScope = "Stage",
                        Target = new MissionTarget
                        {
                            Category = "Any"
                        }
                    }
                )
            );


            Register(
                MissionPresetIds.CompleteTeleporter,
                () => CreateObjectivePreset(
                    MissionPresetIds.CompleteTeleporter,
                    MissionPresetCategories.Progression,
                    "Completar teletransportador",
                    "Completa una cantidad configurable de teletransportadores.",
                    true,
                    new MissionObjective
                    {
                        Id = "complete_teleporter",
                        Type = "CompleteTeleporter",
                        Amount = 1d,
                        ResetScope = "Run"
                    }
                )
            );


            Register(
                MissionPresetIds.CompleteRun,
                () => CreateObjectivePreset(
                    MissionPresetIds.CompleteRun,
                    MissionPresetCategories.Progression,
                    "Completar partida",
                    "Completa una run mediante un final configurable.",
                    false,
                    new MissionObjective
                    {
                        Id = "complete_run",
                        Type = "CompleteRun",
                        Amount = 1d,
                        ResetScope = "Run",
                        Parameters = new JObject
                        {
                            ["ending"] = "Any"
                        }
                    }
                )
            );


            Register(
                MissionPresetIds.HealHealth,
                () => CreateObjectivePreset(
                    MissionPresetIds.HealHealth,
                    MissionPresetCategories.Healing,
                    "Curar salud",
                    "Acumula una cantidad configurable de curación.",
                    false,
                    new MissionObjective
                    {
                        Id = "heal_health",
                        Type = "HealHealth",
                        Amount = 1000d,
                        ResetScope = "Run",
                        Target = new MissionTarget
                        {
                            Category = "Ally"
                        }
                    }
                )
            );


            Register(
                MissionPresetIds.ApplyStatusEffects,
                () => CreateApplyStatusObjectivePreset(
                    MissionPresetIds.ApplyStatusEffects,
                    "Aplicar efectos de estado",
                    "Cuenta cualquier efecto de estado válido.",
                    "AnyValid",
                    ""
                )
            );


            Register(
                MissionPresetIds.ApplyNegativeStatusEffects,
                () => CreateApplyStatusObjectivePreset(
                    MissionPresetIds.ApplyNegativeStatusEffects,
                    "Aplicar efectos negativos",
                    "Cuenta debuffs, DoT y control negativo válidos.",
                    "AnyNegative",
                    ""
                )
            );


            Register(
                MissionPresetIds.ApplyPositiveStatusEffects,
                () => CreateApplyStatusObjectivePreset(
                    MissionPresetIds.ApplyPositiveStatusEffects,
                    "Aplicar efectos positivos",
                    "Cuenta buffs positivos válidos sobre aliados.",
                    "AnyPositive",
                    ""
                )
            );


            Register(
                MissionPresetIds.ApplyDotEffects,
                () => CreateApplyStatusObjectivePreset(
                    MissionPresetIds.ApplyDotEffects,
                    "Aplicar daño prolongado",
                    "Cuenta efectos DoT válidos como Burn, Bleed o Poison.",
                    "AnyDot",
                    ""
                )
            );


            Register(
                MissionPresetIds.ApplyPoison,
                () => CreateApplyStatusObjectivePreset(
                    MissionPresetIds.ApplyPoison,
                    "Aplicar Veneno",
                    "Cuenta aplicaciones o estados activos de Poison.",
                    "Specific",
                    "dot:Poison"
                )
            );


            Register(
                MissionPresetIds.HoldItemStack,
                () => CreateObjectivePreset(
                    MissionPresetIds.HoldItemStack,
                    MissionPresetCategories.Items,
                    "Tener objeto",
                    "Mantén una cantidad configurable de un objeto.",
                    true,
                    new MissionObjective
                    {
                        Id = "hold_item_stack",
                        Type = "HoldItemStack",
                        Amount = 1d,
                        ResetScope = "Run",
                        Parameters = new JObject
                        {
                            ["item"] = ""
                        }
                    }
                )
            );


            Register(
                MissionPresetIds.PickupItem,
                () => CreateObjectivePreset(
                    MissionPresetIds.PickupItem,
                    MissionPresetCategories.Items,
                    "Recoger objeto",
                    "Recoge una cantidad configurable de un objeto seleccionado.",
                    true,
                    new MissionObjective
                    {
                        Id = "pickup_item",
                        Type = "PickupItem",
                        Amount = 1d,
                        ResetScope = "Run",
                        Parameters = new JObject
                        {
                            ["item"] = ""
                        }
                    }
                )
            );


            Register(
                MissionPresetIds.ScrapItems,
                () => CreateObjectivePreset(
                    MissionPresetIds.ScrapItems,
                    MissionPresetCategories.Items,
                    "Convertir objetos en chatarra",
                    "Convierte una cantidad configurable de objetos en Scrap.",
                    true,
                    new MissionObjective
                    {
                        Id = "scrap_items",
                        Type = "ScrapItems",
                        Amount = 1d,
                        ResetScope = "Run"
                    }
                )
            );


            Register(
                MissionPresetIds.UseSkill,
                () => CreateObjectivePreset(
                    MissionPresetIds.UseSkill,
                    MissionPresetCategories.Skills,
                    "Usar habilidad",
                    "Usa una habilidad seleccionada una cantidad configurable.",
                    false,
                    new MissionObjective
                    {
                        Id = "use_skill",
                        Type = "UseSkill",
                        Amount = 1d,
                        ResetScope = "Run",
                        Parameters = new JObject
                        {
                            ["slot"] = "",
                            ["skillToken"] = ""
                        }
                    }
                )
            );


            Register(
                MissionPresetIds.RecruitMinions,
                () => CreateObjectivePreset(
                    MissionPresetIds.RecruitMinions,
                    MissionPresetCategories.Allies,
                    "Reclutar aliados",
                    "Recluta una cantidad configurable de aliados/minions.",
                    true,
                    new MissionObjective
                    {
                        Id = "recruit_minions",
                        Type = "RecruitMinions",
                        Amount = 1d,
                        ResetScope = "Run",
                        Target = new MissionTarget
                        {
                            Category = "Ally"
                        }
                    }
                )
            );


            Register(
                MissionPresetIds.InteractWithTarget,
                () => CreateObjectivePreset(
                    MissionPresetIds.InteractWithTarget,
                    MissionPresetCategories.Interaction,
                    "Interactuar con objetivo",
                    "Interactúa con un objetivo o interactuable concreto.",
                    true,
                    new MissionObjective
                    {
                        Id = "interact_with_target",
                        Type = "InteractWithTarget",
                        Amount = 1d,
                        ResetScope = "Run",
                        Target = new MissionTarget
                        {
                            Category = "Any"
                        },
                        Parameters = new JObject
                        {
                            ["objectName"] = "",
                            ["body"] = "",
                            ["componentType"] = "",
                            ["nameMatch"] = "Exact"
                        }
                    }
                )
            );


            Register(
                MissionPresetIds.ConsumeItemOnInteraction,
                () => CreateObjectivePreset(
                    MissionPresetIds.ConsumeItemOnInteraction,
                    MissionPresetCategories.Interaction,
                    "Entregar objeto al interactuar",
                    "Consume un objeto real del inventario al interactuar con un objetivo concreto.",
                    true,
                    new MissionObjective
                    {
                        Id = "consume_item_on_interaction",
                        Type = "ConsumeItemOnInteraction",
                        Amount = 1d,
                        ResetScope = "Run",
                        Target = new MissionTarget
                        {
                            Category = "Any"
                        },
                        Parameters = new JObject
                        {
                            ["objectName"] = "",
                            ["body"] = "",
                            ["componentType"] = "",
                            ["nameMatch"] = "Exact",
                            ["item"] = ""
                        }
                    }
                )
            );
        }
    }
}
