using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Logging;
using R2API;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>Native Logbook token registration for known USU additions.</summary>
    internal static class NewSurvivorDiaryTokenResolver
    {
        private static readonly Dictionary<string, object> Overlays =
            new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        internal static void Register(SurvivorInfo survivor, string bodyName,
            string spanish, string english, ManualLogSource logger)
        {
            if (survivor?.SurvivorDef == null) return;
            var tokens = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            object def = survivor.SurvivorDef;
            AddDerived(tokens, GetString(def, "displayNameToken"));
            Add(tokens, GetString(def, "loreToken"));
            if (survivor.SurvivorDef.bodyPrefab != null)
            {
                var body = survivor.SurvivorDef.bodyPrefab.GetComponent<CharacterBody>();
                AddDerived(tokens, GetString(body, "baseNameToken"));
            }
            string prefix = bodyName.EndsWith("Body", StringComparison.OrdinalIgnoreCase)
                ? bodyName.Substring(0, bodyName.Length - 4).ToUpperInvariant()
                : bodyName.ToUpperInvariant();
            Add(tokens, prefix + "_LORE");
            Add(tokens, prefix + "_BODY_LORE");
            foreach (string token in tokens)
            {
                AddOverlay(token, "en", english);
                AddOverlay(token, "es-419", spanish);
                AddOverlay(token, "es-ES", spanish);
            }
            UsuLog.Verbose(logger, "[USU DIARIO] " + bodyName +
                " tokens nativos candidatos: " + string.Join(", ", tokens));
        }

        private static void AddOverlay(string token, string language, string value)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(value)) return;
            string key = language + ":" + token;
            if (Overlays.ContainsKey(key)) return;
            Overlays.Add(key, LanguageAPI.AddOverlay(token, value, language));
        }
        private static void Add(HashSet<string> tokens, string token)
        {
            if (!string.IsNullOrWhiteSpace(token)) tokens.Add(token.Trim());
        }
        private static void AddDerived(HashSet<string> tokens, string nameToken)
        {
            if (string.IsNullOrWhiteSpace(nameToken)) return;
            if (nameToken.EndsWith("_NAME", StringComparison.OrdinalIgnoreCase))
                Add(tokens, nameToken.Substring(0, nameToken.Length - 5) + "_LORE");
            if (nameToken.EndsWith("_DISPLAYNAME", StringComparison.OrdinalIgnoreCase))
                Add(tokens, nameToken.Substring(0, nameToken.Length - 12) + "_LORE");
        }
        private static string GetString(object obj, string name)
        {
            if (obj == null) return "";
            const BindingFlags f = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            try
            {
                FieldInfo field = obj.GetType().GetField(name, f);
                if (field != null) return field.GetValue(obj) as string ?? "";
                PropertyInfo property = obj.GetType().GetProperty(name, f);
                if (property != null && property.GetIndexParameters().Length == 0)
                    return property.GetValue(obj, null) as string ?? "";
            }
            catch { }
            return "";
        }
    }
}
