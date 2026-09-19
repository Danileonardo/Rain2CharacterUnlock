using System;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;

namespace UniversalSurvivorUnlocks
{
    public static partial class BuiltInMissionPresetCatalog
    {
        // =========================================================
        // BLOQUES - RUNTIME AVANZADO
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
    }
}
