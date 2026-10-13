using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Logging;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// C2.5 - Structural/semantic inspection of official survivor unlock
    /// implementations. Read-only: it inventories meaningful members from
    /// Achievement and server-tracker types so new reusable Mission V2
    /// mechanics can be based on runtime evidence rather than guesses.
    /// </summary>
    public static class VanillaUnlockSemanticInspectionService
    {
        private static readonly string[] SemanticTerms =
        {
            "kill", "death", "defeat", "boss", "teleporter", "stage",
            "scene", "run", "time", "difficulty", "item", "inventory",
            "equipment", "interact", "shrine", "altar", "heal", "health",
            "damage", "skill", "ability", "minion", "drone", "turret",
            "level", "jump", "air", "fall", "speed", "distance", "gold",
            "money", "loop", "ending", "obliterate", "portal", "artifact",
            "buff", "debuff", "dot", "elite", "champion", "acolyte",
            "page", "ritual", "anima", "harvester", "revive", "consume"
        };

        public static void Audit(
            IReadOnlyList<SurvivorInfo> survivors,
            ManualLogSource logger
        )
        {
            if (survivors == null)
            {
                return;
            }

            int inspected = 0;
            int withTracker = 0;
            int semanticMembers = 0;

            for (int i = 0; i < survivors.Count; i++)
            {
                SurvivorInfo survivor = survivors[i];
                if (
                    survivor == null ||
                    survivor.SurvivorDef == null ||
                    survivor.IsModded ||
                    survivor.SurvivorDef.unlockableDef == null
                )
                {
                    continue;
                }

                AchievementDef achievement =
                    FindAchievementForUnlockable(
                        survivor.SurvivorDef.unlockableDef
                    );

                if (achievement == null)
                {
                    continue;
                }

                inspected++;
                if (achievement.serverTrackerType != null)
                {
                    withTracker++;
                }

                List<string> members = new List<string>();
                CollectSemanticMembers(achievement.type, "Achievement", members);
                CollectSemanticMembers(achievement.serverTrackerType, "Tracker", members);
                semanticMembers += members.Count;

                UsuLog.Verbose(
                    logger,
                    "[VANILLA UNLOCK SEMANTICS] " +
                    SafeBodyName(survivor) +
                    " | Achievement: " +
                    (achievement.identifier ?? "") +
                    " | AchievementType: " + TypeName(achievement.type) +
                    " | TrackerType: " + TypeName(achievement.serverTrackerType) +
                    " | Signature: " +
                    (members.Count > 0
                        ? string.Join(",", members.ToArray())
                        : "<none>")
                );
            }

            UsuLog.Diagnostic(
                logger,
                "[VANILLA UNLOCK SEMANTICS] C2.5 inspección estructural completada | " +
                "Achievements: " + inspected +
                " | Con server tracker: " + withTracker +
                " | Miembros semánticos: " + semanticMembers +
                " | Runtime mutation: no | Autoridad modificada: no"
            );
        }

        private static void CollectSemanticMembers(
            Type type,
            string prefix,
            List<string> output
        )
        {
            if (type == null || output == null)
            {
                return;
            }

            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            Type cursor = type;
            int depth = 0;

            while (cursor != null && depth < 4)
            {
                BindingFlags flags =
                    BindingFlags.Instance |
                    BindingFlags.Static |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly;

                try
                {
                    FieldInfo[] fields = cursor.GetFields(flags);
                    for (int i = 0; i < fields.Length; i++)
                    {
                        AddIfSemantic(prefix + ".F:" + fields[i].Name, seen, output);
                    }

                    PropertyInfo[] properties = cursor.GetProperties(flags);
                    for (int i = 0; i < properties.Length; i++)
                    {
                        AddIfSemantic(prefix + ".P:" + properties[i].Name, seen, output);
                    }

                    MethodInfo[] methods = cursor.GetMethods(flags);
                    for (int i = 0; i < methods.Length; i++)
                    {
                        if (methods[i].IsSpecialName)
                        {
                            continue;
                        }

                        AddIfSemantic(prefix + ".M:" + methods[i].Name, seen, output);
                    }
                }
                catch
                {
                    // Diagnostic only. Some runtime-generated types can reject
                    // reflective enumeration; skipping them must not affect USU.
                }

                cursor = cursor.BaseType;
                depth++;
            }
        }

        private static void AddIfSemantic(
            string value,
            HashSet<string> seen,
            List<string> output
        )
        {
            if (string.IsNullOrWhiteSpace(value) || output.Count >= 24)
            {
                return;
            }

            string lowered = value.ToLowerInvariant();
            bool semantic = false;

            for (int i = 0; i < SemanticTerms.Length; i++)
            {
                if (lowered.IndexOf(SemanticTerms[i], StringComparison.Ordinal) >= 0)
                {
                    semantic = true;
                    break;
                }
            }

            if (semantic && seen.Add(value))
            {
                output.Add(value);
            }
        }

        private static AchievementDef FindAchievementForUnlockable(
            UnlockableDef unlockable
        )
        {
            if (unlockable == null || string.IsNullOrWhiteSpace(unlockable.cachedName))
            {
                return null;
            }

            try
            {
                IEnumerable definitions = AchievementManager.achievementDefs as IEnumerable;
                if (definitions == null)
                {
                    return null;
                }

                foreach (object raw in definitions)
                {
                    AchievementDef achievement = raw as AchievementDef;
                    if (achievement == null)
                    {
                        continue;
                    }

                    if (
                        string.Equals(
                            achievement.unlockableRewardIdentifier,
                            unlockable.cachedName,
                            StringComparison.Ordinal
                        )
                    )
                    {
                        return achievement;
                    }
                }
            }
            catch
            {
            }

            return null;
        }

        private static string SafeBodyName(SurvivorInfo survivor)
        {
            return survivor != null && !string.IsNullOrWhiteSpace(survivor.BodyName)
                ? survivor.BodyName
                : "<unknown>";
        }

        private static string TypeName(Type type)
        {
            return type != null
                ? (type.FullName ?? type.Name ?? "")
                : "<none>";
        }
    }
}
