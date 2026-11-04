using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Presentación (no ejecución) de restricciones reales de Mission V2.
    /// Cada ruta es OR; ninguna etiqueta puede exigir algo que no sea
    /// obligatorio para todas las rutas disponibles.
    /// </summary>
    internal static class MissionRequiredSurvivorDisplay
    {
        internal static List<string> GetCommonRequiredBodies(MissionDefinition mission)
            => GetAlwaysRequiredBodies(mission, "RequiredSurvivor");

        internal static List<string> GetCommonRequiredTeamBodies(MissionDefinition mission)
            => GetAlwaysRequiredBodies(mission, "PartyHasSurvivor");

        private static List<string> GetAlwaysRequiredBodies(MissionDefinition mission, string type)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (mission?.Routes == null || mission.Routes.Count == 0)
                return new List<string>();
            foreach (MissionRoute route in mission.Routes)
            {
                if (route == null) return new List<string>();
                var current = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                CollectBodies(route.Conditions, type, current);
                foreach (MissionObjective objective in route.GetEffectiveObjectives())
                    CollectBodies(objective?.Conditions, type, current);
                // One unconstrained OR path means this category is not mandatory.
                if (current.Count == 0) return new List<string>();
                result.UnionWith(current);
            }
            var names = new List<string>(result);
            names.Sort(StringComparer.OrdinalIgnoreCase);
            return names;
        }

        internal static List<string> GetCommonExcludedBodies(MissionDefinition mission)
        {
            var common = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (mission?.Routes == null || mission.Routes.Count == 0)
                return new List<string>();
            bool first = true;
            foreach (MissionRoute route in mission.Routes)
            {
                if (route == null) return new List<string>();
                var excluded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                CollectBodies(route.Conditions, "ExcludedSurvivor", excluded);
                foreach (MissionObjective objective in route.GetEffectiveObjectives())
                    CollectBodies(objective?.Conditions, "ExcludedSurvivor", excluded);
                if (first) { common.UnionWith(excluded); first = false; }
                else common.IntersectWith(excluded);
            }
            var names = new List<string>(common);
            names.Sort(StringComparer.OrdinalIgnoreCase);
            return names;
        }

        internal static bool ShowAnySurvivorForApprovedMissions(MissionDefinition mission)
        {
            if (mission?.Routes == null || mission.Routes.Count == 0)
                return false;
            foreach (MissionRoute route in mission.Routes)
            {
                if (route == null) return false;
                var required = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                CollectBodies(route.Conditions, "RequiredSurvivor", required);
                CollectBodies(route.Conditions, "PartyHasSurvivor", required);
                if (required.Count > 0) return false;
                foreach (MissionObjective obj in route.GetEffectiveObjectives())
                {
                    CollectBodies(obj?.Conditions, "RequiredSurvivor", required);
                    CollectBodies(obj?.Conditions, "PartyHasSurvivor", required);
                    // G8 generated missions carry a hidden performer/skill restriction.
                    if (obj?.Conditions != null)
                        foreach (MissionCondition c in obj.Conditions)
                            if (c?.Type != null && c.Type.IndexOf("AllowedPerformer", StringComparison.OrdinalIgnoreCase) >= 0)
                                return false;
                }
                if (required.Count > 0) return false;
            }
            return true;
        }

        internal static string GetCommonRequiredStageNames(MissionDefinition mission)
        {
            if (mission?.Routes == null || mission.Routes.Count == 0) return "";
            var all = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (MissionRoute route in mission.Routes)
            {
                if (route == null) return "";
                HashSet<string> selected = null;
                if (!ApplyStages(route.Conditions, ref selected)) return "";
                foreach (MissionObjective obj in route.GetEffectiveObjectives())
                    if (!ApplyStages(obj?.Conditions, ref selected)) return "";
                if (selected == null || selected.Count == 0) return "";
                all.UnionWith(selected);
            }
            if (all.Count == 0) return "";
            if (all.Count == 4 && all.Contains("artifactworld") && all.Contains("artifactworld01") &&
                all.Contains("artifactworld02") && all.Contains("artifactworld03"))
                return Language.GetString(SurvivorLocalization.CharacterSelectBulwarksAmbryNameToken);
            var labels = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string stage in all)
            {
                if (stage.Equals("foggyswamp", StringComparison.OrdinalIgnoreCase))
                    labels.Add(Language.GetString(SurvivorLocalization.CharacterSelectWetlandsNameToken));
                else if (stage.Equals("dampcavesimple", StringComparison.OrdinalIgnoreCase))
                    labels.Add(Language.GetString(SurvivorLocalization.CharacterSelectAbyssalNameToken));
                else if (stage.StartsWith("artifactworld", StringComparison.OrdinalIgnoreCase))
                    labels.Add(Language.GetString(SurvivorLocalization.CharacterSelectBulwarksAmbryNameToken));
                else if (stage.Equals("helminthroost", StringComparison.OrdinalIgnoreCase))
                    labels.Add("Helminth Hatchery");
                else labels.Add(stage);
            }
            var sorted = new List<string>(labels);
            sorted.Sort(StringComparer.OrdinalIgnoreCase);
            return string.Join(" / ", sorted.ToArray());
        }

        private static bool ApplyStages(IList<MissionCondition> conditions, ref HashSet<string> selected)
        {
            if (conditions == null) return true;
            foreach (MissionCondition condition in conditions)
            {
                if (condition == null || !string.Equals(condition.Type, "RequiredStage", StringComparison.OrdinalIgnoreCase))
                    continue;
                var p = condition.Parameters;
                if (p == null) return false;
                var current = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                string single = p.Value<string>("stage");
                if (!string.IsNullOrWhiteSpace(single)) current.Add(single.Trim());
                if (p["stages"] is JArray list)
                    foreach (JToken entry in list)
                        if (!string.IsNullOrWhiteSpace(entry?.ToString())) current.Add(entry.ToString().Trim());
                if (current.Count == 0) return false;
                if (selected == null) selected = current;
                else selected.IntersectWith(current);
                if (selected.Count == 0) return false;
            }
            return true;
        }

        private static void CollectBodies(IList<MissionCondition> conditions, string type, HashSet<string> destination)
        {
            if (conditions == null) return;
            foreach (MissionCondition condition in conditions)
            {
                if (condition == null || condition.Parameters == null ||
                    !string.Equals(condition.Type, type, StringComparison.OrdinalIgnoreCase)) continue;
                string single = condition.Parameters.Value<string>("body");
                if (!string.IsNullOrWhiteSpace(single)) destination.Add(single.Trim());
                if (condition.Parameters["bodies"] is JArray list)
                    foreach (JToken token in list)
                        if (!string.IsNullOrWhiteSpace(token?.ToString())) destination.Add(token.ToString().Trim());
            }
        }
    }
}
