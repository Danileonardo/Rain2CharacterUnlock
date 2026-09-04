using System;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Biblioteca integrada de bloques y recetas creadas por USU.
    ///
    /// NIVEL 1 - BLOQUES ATÓMICOS
    /// Objective / Condition
    ///
    /// NIVEL 2 - RECETAS
    /// Mission
    ///
    /// Una receta puede combinar bloques mediante:
    /// - varios Objectives dentro de una Route = AND;
    /// - varias Conditions dentro de una Route = AND;
    /// - varias Routes = OR.
    ///
    /// El catálogo usa factories para entregar copias nuevas y
    /// proteger siempre el preset original del creador.
    /// </summary>
    public static class BuiltInMissionPresetCatalog
    {
        private static readonly Dictionary<
            string,
            Func<MissionPreset>
        > Factories =
            new Dictionary<string, Func<MissionPreset>>(
                StringComparer.OrdinalIgnoreCase
            );


        static BuiltInMissionPresetCatalog()
        {
            RegisterBaseObjectivePresets();
            RegisterBaseConditionPresets();
            RegisterStatusPresets();
            RegisterAdvancedRuntimePresets();
            RegisterLegacyAndCreatorRecipes();
            RegisterOfficialCharacterRecipes();
        }


        // =========================================================
        // PUBLIC API
        // =========================================================

        public static bool Contains(
            string presetId
        )
        {
            return
                !string.IsNullOrWhiteSpace(presetId) &&
                Factories.ContainsKey(
                    presetId.Trim()
                );
        }


        public static MissionPreset Get(
            string presetId
        )
        {
            MissionPreset preset;

            return TryGet(
                presetId,
                out preset
            )
                ? preset
                : null;
        }


        public static bool TryGet(
            string presetId,
            out MissionPreset preset
        )
        {
            preset =
                null;


            if (
                string.IsNullOrWhiteSpace(
                    presetId
                )
            )
            {
                return false;
            }


            Func<MissionPreset> factory;


            if (
                !Factories.TryGetValue(
                    presetId.Trim(),
                    out factory
                ) ||
                factory == null
            )
            {
                return false;
            }


            preset =
                factory();


            return preset != null;
        }


        public static List<MissionPreset> GetAll()
        {
            List<MissionPreset> result =
                new List<MissionPreset>(
                    Factories.Count
                );


            foreach (
                KeyValuePair<string, Func<MissionPreset>> pair
                in Factories
            )
            {
                MissionPreset preset =
                    pair.Value != null
                        ? pair.Value()
                        : null;


                if (preset != null)
                {
                    result.Add(
                        preset
                    );
                }
            }


            return result;
        }


        public static List<MissionPreset> GetByKind(
            string kind
        )
        {
            return Filter(
                preset =>
                    string.Equals(
                        preset.Kind,
                        kind,
                        StringComparison.OrdinalIgnoreCase
                    )
            );
        }


        public static List<MissionPreset> GetByCategory(
            string category
        )
        {
            return Filter(
                preset =>
                    string.Equals(
                        preset.Category,
                        category,
                        StringComparison.OrdinalIgnoreCase
                    )
            );
        }


        public static List<MissionPreset> GetRuntimeSupported()
        {
            return Filter(
                preset =>
                    preset.RuntimeSupported
            );
        }


        private static List<MissionPreset> Filter(
            Func<MissionPreset, bool> predicate
        )
        {
            List<MissionPreset> all =
                GetAll();


            List<MissionPreset> result =
                new List<MissionPreset>();


            if (predicate == null)
            {
                return result;
            }


            for (
                int i = 0;
                i < all.Count;
                i++
            )
            {
                MissionPreset preset =
                    all[i];


                if (
                    preset != null &&
                    predicate(preset)
                )
                {
                    result.Add(
                        preset
                    );
                }
            }


            return result;
        }


        private static void Register(
            string presetId,
            Func<MissionPreset> factory
        )
        {
            if (
                string.IsNullOrWhiteSpace(
                    presetId
                )
            )
            {
                throw new ArgumentException(
                    "El presetId no puede estar vacío.",
                    nameof(presetId)
                );
            }


            if (factory == null)
            {
                throw new ArgumentNullException(
                    nameof(factory)
                );
            }


            string normalizedId =
                presetId.Trim();


            if (
                Factories.ContainsKey(
                    normalizedId
                )
            )
            {
                throw new InvalidOperationException(
                    "Preset integrado duplicado: " +
                    normalizedId
                );
            }


            Factories.Add(
                normalizedId,
                factory
            );
        }


        // =========================================================
        // REGISTRO - OBJETIVOS BASE
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
        }


        // =========================================================
        // REGISTRO - CONDICIONES BASE
        // =========================================================

        private static void RegisterBaseConditionPresets()
        {
            RegisterCondition(
                MissionPresetIds.Airborne,
                MissionPresetCategories.Movement,
                "Estar en el aire",
                "La acción sólo cuenta mientras el jugador está en el aire.",
                true,
                "Airborne",
                new JObject()
            );


            RegisterCondition(
                MissionPresetIds.Grounded,
                MissionPresetCategories.Movement,
                "Estar en el suelo",
                "La acción sólo cuenta mientras el jugador está en el suelo.",
                true,
                "Grounded",
                new JObject()
            );


            RegisterCondition(
                MissionPresetIds.RequiredSurvivor,
                MissionPresetCategories.Survivor,
                "Survivor requerido",
                "Exige uno o varios survivors seleccionados.",
                true,
                "RequiredSurvivor",
                new JObject
                {
                    ["bodies"] = new JArray()
                }
            );


            RegisterCondition(
                MissionPresetIds.RequiredStage,
                MissionPresetCategories.Stage,
                "Sector requerido",
                "Exige uno o varios sectores seleccionados.",
                true,
                "RequiredStage",
                new JObject
                {
                    ["stages"] = new JArray()
                }
            );


            RegisterCondition(
                MissionPresetIds.RequiredItem,
                MissionPresetCategories.Items,
                "Objeto requerido",
                "La acción exige tener un objeto y cantidad seleccionados.",
                false,
                "RequiredItem",
                new JObject
                {
                    ["item"] = "",
                    ["amount"] = 1
                }
            );


            RegisterCondition(
                MissionPresetIds.RequiredEquipment,
                MissionPresetCategories.Items,
                "Equipamiento requerido",
                "La acción exige llevar un equipamiento seleccionado.",
                false,
                "RequiredEquipment",
                new JObject
                {
                    ["equipment"] = ""
                }
            );


            RegisterCondition(
                MissionPresetIds.RequiredSkill,
                MissionPresetCategories.Skills,
                "Habilidad requerida",
                "La acción debe provenir de una habilidad seleccionada.",
                true,
                "RequiredSkill",
                new JObject
                {
                    ["slot"] = "",
                    ["skillToken"] = ""
                }
            );


            RegisterCondition(
                MissionPresetIds.Difficulty,
                MissionPresetCategories.Progression,
                "Dificultad requerida",
                "Exige una dificultad seleccionada o superior.",
                false,
                "Difficulty",
                new JObject
                {
                    ["difficulty"] = ""
                }
            );


            RegisterCondition(
                MissionPresetIds.HealthBelow,
                MissionPresetCategories.Restrictions,
                "Salud por debajo de",
                "La acción sólo cuenta bajo un porcentaje de salud.",
                false,
                "HealthBelow",
                new JObject
                {
                    ["fraction"] = 0.5d
                }
            );


            RegisterCondition(
                MissionPresetIds.TimeLimit,
                MissionPresetCategories.Restrictions,
                "Límite de tiempo",
                "La acción o ruta debe completarse dentro del tiempo configurado.",
                true,
                "TimeLimit",
                new JObject
                {
                    ["seconds"] = 120d,
                    ["scope"] = "Run"
                }
            );


            RegisterCondition(
                MissionPresetIds.DamageType,
                MissionPresetCategories.Damage,
                "Tipo de daño",
                "La acción debe utilizar el tipo de daño seleccionado.",
                true,
                "DamageType",
                new JObject
                {
                    ["damageType"] = ""
                }
            );


            RegisterCondition(
                MissionPresetIds.ExplosiveDamage,
                MissionPresetCategories.Damage,
                "Daño explosivo",
                "La acción debe provenir de una explosión válida.",
                true,
                "DamageType",
                new JObject
                {
                    ["damageType"] = "Explosive"
                }
            );


            RegisterCondition(
                MissionPresetIds.CriticalHit,
                MissionPresetCategories.HitRules,
                "Golpe crítico",
                "El golpe debe ser crítico.",
                true,
                "CriticalHit",
                new JObject()
            );


            RegisterCondition(
                MissionPresetIds.WeakPoint,
                MissionPresetCategories.HitRules,
                "Punto débil",
                "El impacto debe acertar un punto débil.",
                false,
                "WeakPoint",
                new JObject()
            );


            RegisterCondition(
                MissionPresetIds.Backstab,
                MissionPresetCategories.HitRules,
                "Ataque por la espalda",
                "El golpe debe cumplir la regla real de Backstab.",
                true,
                "Backstab",
                new JObject()
            );


            RegisterCondition(
                MissionPresetIds.FatalHit,
                MissionPresetCategories.HitRules,
                "Golpe mortal",
                "El golpe debe ser el que mata al objetivo.",
                true,
                "FatalHit",
                new JObject()
            );


            RegisterCondition(
                MissionPresetIds.MinimumDamage,
                MissionPresetCategories.Damage,
                "Daño mínimo",
                "El golpe debe causar al menos una cantidad configurada.",
                true,
                "MinimumDamage",
                new JObject
                {
                    ["damage"] = 1000d
                }
            );


            RegisterCondition(
                MissionPresetIds.NoDamage,
                MissionPresetCategories.Restrictions,
                "Sin recibir daño",
                "Reinicia o invalida el progreso al recibir daño.",
                false,
                "NoDamage",
                new JObject()
            );


            RegisterCondition(
                MissionPresetIds.NoHealing,
                MissionPresetCategories.Restrictions,
                "Sin curarse",
                "Reinicia o invalida el progreso al recibir curación.",
                false,
                "NoHealing",
                new JObject()
            );


            RegisterCondition(
                MissionPresetIds.NoDeath,
                MissionPresetCategories.Restrictions,
                "Sin morir",
                "La misión exige no morir durante el ámbito configurado.",
                false,
                "NoDeath",
                new JObject()
            );


            RegisterCondition(
                MissionPresetIds.NoItemPickup,
                MissionPresetCategories.Restrictions,
                "Sin recoger objetos",
                "Invalida la ruta si el jugador recoge un objeto.",
                true,
                "NoItemPickup",
                new JObject()
            );
        }


        // =========================================================
        // REGISTRO - ESTADOS
        // =========================================================

        private static void RegisterStatusPresets()
        {
            RegisterStatusCondition(
                MissionPresetIds.StatusSpecific,
                "Estado específico",
                "Specific",
                ""
            );


            RegisterStatusCondition(
                MissionPresetIds.StatusAnyValid,
                "Cualquier efecto válido",
                "AnyValid",
                ""
            );


            RegisterStatusCondition(
                MissionPresetIds.StatusAnyNegative,
                "Cualquier efecto negativo",
                "AnyNegative",
                ""
            );


            RegisterStatusCondition(
                MissionPresetIds.StatusAnyPositive,
                "Cualquier efecto positivo",
                "AnyPositive",
                ""
            );


            RegisterStatusCondition(
                MissionPresetIds.StatusAnyDot,
                "Cualquier DoT",
                "AnyDot",
                ""
            );


            RegisterStatusCondition(
                MissionPresetIds.StatusPoison,
                "Objetivo envenenado",
                "Specific",
                "dot:Poison"
            );


            RegisterStatusCondition(
                MissionPresetIds.StatusBurn,
                "Objetivo ardiendo",
                "Specific",
                "dot:Burn"
            );


            RegisterStatusCondition(
                MissionPresetIds.StatusBleed,
                "Objetivo sangrando",
                "Specific",
                "dot:Bleed"
            );


            RegisterStatusCondition(
                MissionPresetIds.StatusFreeze,
                "Objetivo congelado",
                "Specific",
                "cc:Freeze"
            );


            RegisterStatusCondition(
                MissionPresetIds.StatusStun,
                "Objetivo aturdido",
                "Specific",
                "cc:Stun"
            );
        }


        // =========================================================
        // REGISTRO - BLOQUES AVANZADOS YA SOPORTADOS POR RUNTIME
        // =========================================================

        private static void RegisterAdvancedRuntimePresets()
        {
            Register(
                MissionPresetIds.BombingRun,
                () => CreateObjectivePreset(
                    MissionPresetIds.BombingRun,
                    MissionPresetCategories.Combat,
                    "Bombardeo en el aire",
                    "Completa rondas de bajas explosivas antes de aterrizar.",
                    true,
                    new MissionObjective
                    {
                        Id = "bombing_run",
                        Type = "BombingRun",
                        Amount = 1d,
                        ResetScope = "Stage",
                        Parameters = new JObject
                        {
                            ["killsPerRun"] = 5,
                            ["requireLandingBetweenRuns"] = true,
                            ["countOwnedMinions"] = false
                        }
                    }
                )
            );

            Register(
                MissionPresetIds.CarryEquipment,
                () => CreateObjectivePreset(
                    MissionPresetIds.CarryEquipment,
                    MissionPresetCategories.Items,
                    "Transportar equipo",
                    "Conserva un equipo configurable hasta completar otro objetivo.",
                    true,
                    new MissionObjective
                    {
                        Id = "carry_equipment",
                        Type = "CarryEquipment",
                        Amount = 1d,
                        ResetScope = "Run",
                        Parameters = new JObject
                        {
                            ["equipment"] = "",
                            ["continuous"] = false,
                            ["failOnLoss"] = false,
                            ["failOnDeath"] = false
                        }
                    }
                )
            );

            Register(
                MissionPresetIds.CompleteEnding,
                () => CreateObjectivePreset(
                    MissionPresetIds.CompleteEnding,
                    MissionPresetCategories.Progression,
                    "Completar final",
                    "Termina la run mediante un final configurable.",
                    true,
                    new MissionObjective
                    {
                        Id = "complete_ending",
                        Type = "CompleteEnding",
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
                MissionPresetIds.DefeatUmbraWaves,
                () => CreateObjectivePreset(
                    MissionPresetIds.DefeatUmbraWaves,
                    MissionPresetCategories.Combat,
                    "Derrotar oleadas de Umbrae",
                    "Derrota oleadas completas de copias de los jugadores.",
                    true,
                    new MissionObjective
                    {
                        Id = "defeat_umbra_waves",
                        Type = "DefeatUmbraWaves",
                        Amount = 1d,
                        ResetScope = "Run",
                        Parameters = new JObject
                        {
                            ["stage"] = "artifactworld"
                        }
                    }
                )
            );

            Register(
                MissionPresetIds.LeaveStage,
                () => CreateObjectivePreset(
                    MissionPresetIds.LeaveStage,
                    MissionPresetCategories.Progression,
                    "Abandonar sector",
                    "Completa el objetivo al salir de un sector configurable.",
                    true,
                    new MissionObjective
                    {
                        Id = "leave_stage",
                        Type = "LeaveStage",
                        Amount = 1d,
                        ResetScope = "Run",
                        Parameters = new JObject
                        {
                            ["stage"] = ""
                        }
                    }
                )
            );

            // Estos bloques existían en 5B pero todavía figuraban como
            // pendientes. Desde 5F sus handlers ya están activos.
            // Re-registrarlos no es necesario: actualizamos las factories
            // originales mediante presets equivalentes de receta cuando se
            // usan. El editor puede consultar RuntimeSupported en estas
            // entradas avanzadas y en las recetas oficiales.

            RegisterCondition(
                MissionPresetIds.StageSequence,
                MissionPresetCategories.Stage,
                "Número de sector",
                "La acción sólo cuenta en una posición concreta de la run.",
                true,
                "StageSequence",
                new JObject { ["sequence"] = 1 }
            );

            RegisterCondition(
                MissionPresetIds.PriorSkillUsed,
                MissionPresetCategories.Skills,
                "Habilidad usada previamente",
                "Exige haber usado una habilidad antes de la acción final.",
                true,
                "PriorSkillUsed",
                new JObject
                {
                    ["slot"] = "",
                    ["skillToken"] = "",
                    ["withinSeconds"] = 8f
                }
            );

            RegisterCondition(
                MissionPresetIds.PartyHasSurvivor,
                MissionPresetCategories.Survivor,
                "Survivor presente en el grupo",
                "La ruta requiere que al menos un jugador use uno de los survivors permitidos.",
                true,
                "PartyHasSurvivor",
                new JObject { ["bodies"] = new JArray() }
            );

            RegisterCondition(
                MissionPresetIds.MinionsAlive,
                MissionPresetCategories.Allies,
                "Aliados reclutados vivos",
                "Exige conservar vivos una cantidad configurable de minions reclutados.",
                true,
                "MinionsAlive",
                new JObject
                {
                    ["amount"] = 1,
                    ["markerItem"] = "LemurianHarness"
                }
            );
        }


        // =========================================================
        // REGISTRO - RECETAS HISTÓRICAS / COMPATIBILIDAD
        // =========================================================
        //
        // Estas recetas se conservan para migración y referencia.
        // Todas deben tener IsLegacy = true. MissionPresetLibraryService
        // las mantiene fuera de la lista de nuevas misiones asignables.
        //
        // Las piezas reutilizables para el editor NO salen de estas
        // recetas completas: ya existen como Objective / Condition base.
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


        // =========================================================
        // REGISTRO - MISIONES OFICIALES ACTUALES
        // =========================================================

        private static void RegisterOfficialCharacterRecipes()
        {
            Register(MissionPresetIds.JhinOfficial, CreateJhinOfficial);
            Register(MissionPresetIds.SpyOfficial, CreateSpyOfficial);
            Register(MissionPresetIds.ScoutOfficial, CreateScoutOfficial);
            Register(MissionPresetIds.RocketOfficial, CreateRocketOfficial);
            Register(MissionPresetIds.HunkOfficial, CreateHunkOfficial);
            Register(MissionPresetIds.TinkatonOfficial, CreateTinkatonOfficial);
            Register(MissionPresetIds.WooperOfficial, CreateWooperOfficial);
            Register(MissionPresetIds.SoraOfficial, CreateSoraOfficial);
            Register(MissionPresetIds.RalseiOfficial, CreateRalseiOfficial);
        }


        // =========================================================
        // RECETAS
        // =========================================================
        // RECETAS OFICIALES - 9 PERSONAJES
        // =========================================================

        private static MissionPreset CreateJhinOfficial()
        {
            MissionObjective kill =
                CreateKillObjective(
                    "jhin_boss_kill",
                    "Boss",
                    "",
                    1d
                );


            AddObjectiveConditions(
                kill,

                CreateCondition(
                    "CriticalHit",
                    new JObject()
                ),

                CreateCondition(
                    "MinimumDamage",
                    new JObject
                    {
                        ["damage"] = 44444d
                    }
                ),

                CreateCondition(
                    "FatalHit",
                    new JObject()
                )
            );


            return CreateMissionPreset(
                MissionPresetIds.JhinOfficial,
                MissionPresetCategories.CharacterRecipe,
                "JhinBody",
                "El Cuarto Acto",
                "Convierte a un jefe en tu gran final;\nasesta un crítico mortal de 44.444 de daño o más.",
                true,
                false,
                "PerPlayer",

                CreateRoute(
                    "jhin_fourth_act",

                    new MissionObjective[]
                    {
                kill
                    },

                    null,

                    new MissionRules
                    {
                        SingleRun = true
                    }
                )
            );
        }

        private static MissionPreset CreateSpyOfficial()
        {
            MissionObjective kill = CreateKillObjective("spy_boss", "Boss", "", 1d);
            AddObjectiveConditions(
                kill,
                CreateRequiredSkillCondition("Secondary", "BANDIT2_SECONDARY_NAME"),
                CreateCondition("Backstab", new JObject()),
                CreateCondition("FatalHit", new JObject())
            );

            return CreateMissionPreset(
                MissionPresetIds.SpyOfficial,
                MissionPresetCategories.CharacterRecipe,
                "SpyBody",
                "Sin que me veas venir",
                "Que el jefe nunca vea venir tu golpe final;\nremátalo por detrás con Daga serrada - Bandit",
                true,
                false,
                "PerPlayer",
                CreateRoute(
                    "spy_main",
                    new MissionObjective[] { kill },
                    new MissionCondition[] { CreateRequiredSurvivorCondition("Bandit2Body") },
                    new MissionRules { SingleRun = true }
                )
            );
        }

        private static MissionPreset CreateScoutOfficial()
        {
            MissionObjective drinks = new MissionObjective
            {
                Id = "energy_drinks_8",
                Type = "HoldItemStack",
                Amount = 8d,
                ResetScope = "Run",
                Parameters = new JObject { ["item"] = "SprintBonus" }
            };

            MissionObjective teleporter = new MissionObjective
            {
                Id = "first_teleporter_fast",
                Type = "CompleteTeleporter",
                Amount = 1d,
                ResetScope = "Run"
            };

            AddObjectiveConditions(
                teleporter,
                CreateCondition("StageSequence", new JObject { ["sequence"] = 1 }),
                CreateCondition("TimeLimit", new JObject { ["seconds"] = 240d }),
                CreateCondition("NoItemPickup", new JObject())
            );

            return CreateMissionPreset(
                MissionPresetIds.ScoutOfficial,
                MissionPresetCategories.CharacterRecipe,
                "ScoutBody",
                "Sed Termonuclear",
                "Sacia tu sed con 8 Bebidas energéticas;\no completa el primer sector sin objetos en 4 min.",
                true,
                false,
                "PerPlayer",
                CreateRoute("scout_drinks", new MissionObjective[] { drinks }, null, new MissionRules { SingleRun = true }),
                CreateRoute("scout_batter_up", new MissionObjective[] { teleporter }, null, new MissionRules { SingleRun = true })
            );
        }

        private static MissionPreset CreateRocketOfficial()
        {
            MissionObjective bombing = new MissionObjective
            {
                Id = "bombing_runs_3",
                Type = "BombingRun",
                Amount = 3d,
                ResetScope = "Stage",
                Parameters = new JObject
                {
                    ["killsPerRun"] = 5,
                    ["requireLandingBetweenRuns"] = true,
                    ["countOwnedMinions"] = false
                }
            };

            return CreateMissionPreset(
                MissionPresetIds.RocketOfficial,
                MissionPresetCategories.CharacterRecipe,
                "RocketSurvivorBody",
                "La gravedad es opcional",
                "Haz llover explosiones desde el cielo;\nderriba 5 antes de caer; haz la hazaña 3 veces.",
                true,
                false,
                "PerPlayer",
                CreateRoute("rocket_main", new MissionObjective[] { bombing }, null, new MissionRules { SingleRun = true })
            );
        }

        private static MissionPreset CreateHunkOfficial()
        {
            MissionObjective carryEscape = CreateFuelArrayCarryObjective("carry_sample_escape");
            MissionObjective escape = new MissionObjective
            {
                Id = "escape_moon",
                Type = "CompleteEnding",
                Amount = 1d,
                ResetScope = "Run",
                Parameters = new JObject { ["ending"] = "Escape" }
            };

            MissionObjective carryObliterate = CreateFuelArrayCarryObjective("carry_sample_obliterate");
            MissionObjective obliterate = new MissionObjective
            {
                Id = "obliterate",
                Type = "CompleteEnding",
                Amount = 1d,
                ResetScope = "Run",
                Parameters = new JObject { ["ending"] = "Obliterate" }
            };

            return CreateMissionPreset(
                MissionPresetIds.HunkOfficial,
                MissionPresetCategories.CharacterRecipe,
                "RobHunkBody",
                "La Parca No Falla",
                "Protege la batería y sobrevive a toda costa;\nescapa de la Luna o sacrifícate en el Obelisco.",
                true,
                false,
                "PerPlayer",
                CreateRoute("hunk_escape", new MissionObjective[] { carryEscape, escape }, null, new MissionRules { SingleRun = true, ResetOnDeath = true }),
                CreateRoute("hunk_obliterate", new MissionObjective[] { carryObliterate, obliterate }, null, new MissionRules { SingleRun = true, ResetOnDeath = true })
            );
        }

        private static MissionObjective CreateFuelArrayCarryObjective(string id)
        {
            return new MissionObjective
            {
                Id = id,
                Type = "CarryEquipment",
                Amount = 1d,
                ResetScope = "Run",
                Parameters = new JObject
                {
                    ["equipment"] = "QuestVolatileBattery",
                    ["continuous"] = true,
                    ["failOnLoss"] = true,
                    ["failOnDeath"] = true
                }
            };
        }

        private static MissionPreset CreateTinkatonOfficial()
        {
            MissionObjective scrap = new MissionObjective
            {
                Id = "scrap_6",
                Type = "ScrapItems",
                Amount = 6d,
                ResetScope = "Run"
            };

            MissionObjective hammer = new MissionObjective
            {
                Id = "justice_demolisher",
                Type = "HoldItemStack",
                Amount = 1d,
                ResetScope = "Run",
                Parameters = new JObject { ["item"] = "ArmorReductionOnHit" }
            };

            MissionObjective eye = CreateKillObjective(
                "mechanical_eye",
                "SpecificBody",
                "SuperRoboBallBossBody|RoboBallBossBody",
                1d
            );
            AddObjectiveConditions(
                eye,
                CreateCondition("FatalHit", new JObject())
            );

            return CreateMissionPreset(
                MissionPresetIds.TinkatonOfficial,
                MissionPresetCategories.CharacterRecipe,
                "TinkatonBody",
                "Forjada en Chatarra",
                "Haz de 6 chatarras el inicio de tu gran golpe;\nten Justicia demoledora y vence un Ojo mecánico.",
                true,
                false,
                "PerPlayer",
                CreateRoute(
                    "tinkaton_main",
                    new MissionObjective[] { scrap, hammer, eye },
                    null,
                    new MissionRules { SingleRun = true }
                )
            );
        }

        private static MissionPreset CreateWooperOfficial()
        {
            MissionObjective bite = CreateKillObjective("poisoned_bite_20", "Enemy", "", 20d);
            bite.ResetScope = "Stage";
            AddObjectiveConditions(
                bite,
                CreateRequiredSkillCondition("Secondary", "CROCO_SECONDARY_ALT_NAME"),
                CreateCondition("StatusPresent", new JObject
                {
                    ["subject"] = "Target",
                    ["timing"] = "BeforeAction",
                    ["mode"] = "Specific",
                    ["statusId"] = "dot:Poison"
                })
            );

            return CreateMissionPreset(
                MissionPresetIds.WooperOfficial,
                MissionPresetCategories.CharacterRecipe,
                "WooperBody",
                "De vuelta al agua",
                "Haz de los Humedales tu hogar; marca territorio;\ncaza y muerde a 20 presas envenenadas - Acrid",
                true,
                false,
                "PerPlayer",
                CreateRoute(
                    "wooper_main",
                    new MissionObjective[] { bite },
                    new MissionCondition[]
                    {
                        CreateRequiredSurvivorCondition("CrocoBody"),
                        CreateCondition("RequiredStage", new JObject { ["stage"] = "foggyswamp" })
                    },
                    new MissionRules { SingleRun = true }
                )
            );
        }

        private static MissionPreset CreateSoraOfficial()
        {
            MissionObjective shadows = new MissionObjective
            {
                Id = "umbra_waves_3",
                Type = "DefeatUmbraWaves",
                Amount = 3d,
                ResetScope = "Run",
                Parameters = new JObject { ["stage"] = "artifactworld" }
            };

            MissionObjective leave = new MissionObjective
            {
                Id = "leave_ambry",
                Type = "LeaveStage",
                Amount = 1d,
                ResetScope = "Run",
                Parameters = new JObject { ["stage"] = "artifactworld" }
            };

            return CreateMissionPreset(
                MissionPresetIds.SoraOfficial,
                MissionPresetCategories.CharacterRecipe,
                "SoraBody",
                "Elegido de la Llave Espada",
                "Abre paso entre mundos en Baluarte de Ambry;\nvence a sombras y completa Venganza - Mercenary",
                true,
                false,
                "Shared",
                CreateRoute(
                    "sora_vengeance",
                    new MissionObjective[] { shadows, leave },
                    new MissionCondition[]
                    {
                        CreateCondition("PartyHasSurvivor", new JObject
                        {
                            ["bodies"] = new JArray("MercBody")
                        })
                    },
                    new MissionRules { SingleRun = true }
                )
            );
        }

        private static MissionPreset CreateRalseiOfficial()
        {
            MissionObjective recruit = new MissionObjective
            {
                Id = "devotion_friends_3",
                Type = "RecruitMinions",
                Amount = 3d,
                ResetScope = "Run",
                Target = new MissionTarget { Category = "Ally" },
                Parameters = new JObject { ["markerItem"] = "LemurianHarness" }
            };

            MissionObjective teleporter = new MissionObjective
            {
                Id = "portal_with_friends",
                Type = "CompleteTeleporter",
                Amount = 1d,
                ResetScope = "Run"
            };

            return CreateMissionPreset(
                MissionPresetIds.RalseiOfficial,
                MissionPresetCategories.CharacterRecipe,
                "RalseiBody",
                "El poder de la bondad",
                "Usa Devoción y reúne 3 nuevos amigos Lemurianos;\ncompleta el portal con ellos - Captain o Seeker",
                true,
                false,
                "PerPlayer",
                CreateRoute(
                    "ralsei_devotion",
                    new MissionObjective[] { recruit, teleporter },
                    new MissionCondition[] { CreateRequiredSurvivorCondition("CaptainBody", "SeekerBody") },
                    new MissionRules { SingleRun = true }
                )
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
                        ["damage"] = 44444d
                    }
                ),
                CreateCondition(
                    "FatalHit",
                    new JObject()
                )
            );


            return CreateMissionPreset(
                MissionPresetIds.BossCritical44444,
                MissionPresetCategories.Legacy,
                "JhinBody",
                "Crítico mortal de 44444",
                "Mata a un jefe con un crítico de al menos 44444 de daño.",
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


        private static MissionPreset CreateObjectivePreset(
            string presetId,
            string category,
            string name,
            string description,
            bool runtimeSupported,
            MissionObjective objective
        )
        {
            return new MissionPreset
            {
                PresetId = presetId,
                Kind = MissionPresetKinds.Objective,
                Category = category,
                Name = name,
                Description = description,
                RuntimeSupported = runtimeSupported,
                ObjectiveTemplate = objective
            };
        }


        private static MissionPreset CreateConditionPreset(
            string presetId,
            string category,
            string name,
            string description,
            bool runtimeSupported,
            MissionCondition condition
        )
        {
            return new MissionPreset
            {
                PresetId = presetId,
                Kind = MissionPresetKinds.Condition,
                Category = category,
                Name = name,
                Description = description,
                RuntimeSupported = runtimeSupported,
                ConditionTemplate = condition
            };
        }


        private static MissionPreset CreateMissionPreset(
            string presetId,
            string category,
            string targetBody,
            string name,
            string description,
            bool runtimeSupported,
            bool isLegacy,
            string progressScope,
            params MissionRoute[] routes
        )
        {
            MissionDefinition mission =
                new MissionDefinition
                {
                    SchemaVersion = 2,
                    ProgressScope = progressScope,
                    RewardScope = "Session"
                };


            if (routes != null)
            {
                for (
                    int i = 0;
                    i < routes.Length;
                    i++
                )
                {
                    if (routes[i] != null)
                    {
                        mission.Routes.Add(
                            routes[i]
                        );
                    }
                }
            }


            return new MissionPreset
            {
                PresetId = presetId,
                Kind = MissionPresetKinds.Mission,
                Category = category,
                TargetBody = targetBody ?? "",
                Name = name,
                Description = description,
                RuntimeSupported = runtimeSupported,
                IsLegacy = isLegacy,
                Mission = mission
            };
        }


        private static void RegisterCondition(
            string presetId,
            string category,
            string name,
            string description,
            bool runtimeSupported,
            string conditionType,
            JObject parameters
        )
        {
            Register(
                presetId,
                () => CreateConditionPreset(
                    presetId,
                    category,
                    name,
                    description,
                    runtimeSupported,
                    CreateCondition(
                        conditionType,
                        parameters
                    )
                )
            );
        }


        private static void RegisterStatusCondition(
            string presetId,
            string name,
            string mode,
            string statusId
        )
        {
            RegisterCondition(
                presetId,
                MissionPresetCategories.StatusEffects,
                name,
                "El objetivo debe tener el estado configurado antes de la acción.",
                true,
                "StatusPresent",
                new JObject
                {
                    ["subject"] = "Target",
                    ["timing"] = "BeforeAction",
                    ["mode"] = mode,
                    ["statusId"] = statusId
                }
            );
        }


        private static MissionPreset CreateApplyStatusObjectivePreset(
            string presetId,
            string name,
            string description,
            string mode,
            string statusId
        )
        {
            return CreateObjectivePreset(
                presetId,
                MissionPresetCategories.StatusEffects,
                name,
                description,
                false,
                CreateApplyStatusObjective(
                    "apply_status",
                    1d,
                    mode,
                    statusId
                )
            );
        }


        // =========================================================
        // HELPERS - MODELO
        // =========================================================

        private static MissionObjective CreateKillObjective(
            string id,
            string targetCategory,
            string targetId,
            double amount
        )
        {
            return new MissionObjective
            {
                Id = id,
                Type = "Kill",
                Amount = amount,
                ResetScope = "Run",
                Target = new MissionTarget
                {
                    Category = targetCategory,
                    Id = targetId ?? ""
                }
            };
        }


        private static MissionObjective CreateApplyStatusObjective(
            string id,
            double amount,
            string mode,
            string statusId
        )
        {
            return new MissionObjective
            {
                Id = id,
                Type = "ApplyStatusEffects",
                Amount = amount,
                ResetScope = "Run",
                Target = new MissionTarget
                {
                    Category = "Any"
                },
                Parameters = new JObject
                {
                    ["mode"] = mode,
                    ["statusId"] = statusId,
                    ["activeSimultaneously"] = true
                }
            };
        }


        private static MissionCondition CreateCondition(
            string type,
            JObject parameters
        )
        {
            return new MissionCondition
            {
                Type = type,
                Parameters = parameters ?? new JObject()
            };
        }


        private static MissionCondition CreateRequiredSurvivorCondition(
            params string[] bodies
        )
        {
            JArray bodyArray =
                new JArray();


            if (bodies != null)
            {
                for (
                    int i = 0;
                    i < bodies.Length;
                    i++
                )
                {
                    if (
                        !string.IsNullOrWhiteSpace(
                            bodies[i]
                        )
                    )
                    {
                        bodyArray.Add(
                            bodies[i]
                        );
                    }
                }
            }


            return CreateCondition(
                "RequiredSurvivor",
                new JObject
                {
                    ["bodies"] = bodyArray
                }
            );
        }


        private static MissionCondition CreateRequiredSkillCondition(
            string slot,
            string skillToken
        )
        {
            return CreateCondition(
                "RequiredSkill",
                new JObject
                {
                    ["slot"] = slot,
                    ["skillToken"] = skillToken
                }
            );
        }


        private static MissionObjective AddObjectiveConditions(
            MissionObjective objective,
            params MissionCondition[] conditions
        )
        {
            if (
                objective == null ||
                conditions == null
            )
            {
                return objective;
            }


            for (
                int i = 0;
                i < conditions.Length;
                i++
            )
            {
                if (conditions[i] != null)
                {
                    objective.Conditions.Add(
                        conditions[i]
                    );
                }
            }


            return objective;
        }


        private static MissionRoute CreateRoute(
            string routeId,
            IReadOnlyList<MissionObjective> objectives,
            IReadOnlyList<MissionCondition> conditions,
            MissionRules rules
        )
        {
            MissionRoute route =
                new MissionRoute
                {
                    Id = routeId,
                    Rules = rules ?? new MissionRules()
                };


            if (objectives != null)
            {
                for (
                    int i = 0;
                    i < objectives.Count;
                    i++
                )
                {
                    if (objectives[i] != null)
                    {
                        route.Objectives.Add(
                            objectives[i]
                        );
                    }
                }
            }


            if (conditions != null)
            {
                for (
                    int i = 0;
                    i < conditions.Count;
                    i++
                )
                {
                    if (conditions[i] != null)
                    {
                        route.Conditions.Add(
                            conditions[i]
                        );
                    }
                }
            }


            return route;
        }
    }
}
