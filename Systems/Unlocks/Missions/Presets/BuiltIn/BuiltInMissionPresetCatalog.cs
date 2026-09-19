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
    public static partial class BuiltInMissionPresetCatalog
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


        private static MissionCondition CreateExcludedSurvivorCondition(
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
                "ExcludedSurvivor",
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
