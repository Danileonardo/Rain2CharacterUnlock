using System;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    public static partial class BuiltInMissionPresetCatalog
    {
        // =========================================================
        // BLOQUES - CONDICIONES BASE
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
    }
}
