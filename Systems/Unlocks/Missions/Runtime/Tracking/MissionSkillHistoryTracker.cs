using System;
using System.Collections.Generic;

using BepInEx.Logging;

using RoR2;

using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Historial mínimo y reutilizable de la última habilidad activada
    /// por cada jugador. Se utiliza por condiciones como PriorSkillUsed,
    /// ResourceMastery y la atribución temporal de HealingSupport.
    /// No realiza polling.
    /// </summary>
    public static class MissionSkillHistoryTracker
    {
        private sealed class SkillUseInfo
        {
            public CharacterBody Body;
            public GenericSkill Skill;
            public string Slot = "";
            public string SkillToken = "";
            public string SkillName = "";
            public float RunTime;
        }

        private static readonly Dictionary<CharacterMaster, SkillUseInfo>
            LastUseByPlayer = new Dictionary<CharacterMaster, SkillUseInfo>();

        // Historial corto para atribuir efectos diferidos (por ejemplo curas
        // que ocurren varios ticks después de activar la skill).
        private static readonly List<MissionRecentSkillUse>
            RecentUses = new List<MissionRecentSkillUse>();

        private const float RecentHistoryRetentionSeconds = 12f;
        private const int MaxRecentHistoryEntries = 128;

        private static ManualLogSource logger;
        private static bool initialized;

        public static void Initialize(ManualLogSource log)
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            logger = log;

            GameplayEventHub.SkillActivationStarted += OnSkillActivated;
            MissionRunLifecycle.RunStarted += OnRunStart;
            MissionRunLifecycle.RunDestroyed += OnRunEnd;

            UsuLog.Verbose(
                logger,
                "[MISSION V2] SkillHistory tracker inicializado."
            );
        }

        private static void OnRunStart(Run run)
        {
            LastUseByPlayer.Clear();
            RecentUses.Clear();
        }

        private static void OnRunEnd(Run run)
        {
            LastUseByPlayer.Clear();
            RecentUses.Clear();
        }

        private static void OnSkillActivated(
            CharacterBody self,
            GenericSkill skill
        )
        {
            if (
                !NetworkServer.active ||
                Run.instance == null ||
                self == null ||
                skill == null
            )
            {
                return;
            }

            bool needed =
                MissionRuntimeActivityPlan.IsTypeActive("PriorSkillUsed") ||
                MissionRuntimeActivityPlan.IsTypeActive("UseSkill") ||
                MissionRuntimeActivityPlan.IsTypeActive("Heal");

            if (!needed)
            {
                return;
            }

            CharacterMaster master = self.master;

            if (
                master == null ||
                MissionPlayerIdentity.GetNetworkUser(master) == null
            )
            {
                return;
            }

            string slot = ResolveSlot(self, skill);
            string token = skill.skillDef != null
                ? skill.skillDef.skillNameToken ?? ""
                : "";
            string name = skill.skillDef != null
                ? skill.skillDef.skillName ?? ""
                : "";

            float runTime =
                Run.instance.GetRunStopwatch();

            LastUseByPlayer[master] = new SkillUseInfo
            {
                Body = self,
                Skill = skill,
                Slot = slot,
                SkillToken = token,
                SkillName = name,
                RunTime = runTime
            };

            RecentUses.Add(
                new MissionRecentSkillUse(
                    master,
                    self,
                    skill,
                    slot,
                    token,
                    name,
                    runTime
                )
            );

            PruneRecentUses(runTime);
        }

        public static bool WasUsedRecently(
            CharacterMaster master,
            string slot,
            string skillToken,
            float withinSeconds
        )
        {
            if (master == null || Run.instance == null)
            {
                return false;
            }

            if (!LastUseByPlayer.TryGetValue(master, out SkillUseInfo info) || info == null)
            {
                return false;
            }

            if (
                !string.IsNullOrWhiteSpace(slot) &&
                !string.Equals(info.Slot, slot.Trim(), StringComparison.OrdinalIgnoreCase)
            )
            {
                return false;
            }

            if (
                !string.IsNullOrWhiteSpace(skillToken) &&
                !string.Equals(info.SkillToken, skillToken.Trim(), StringComparison.Ordinal)
            )
            {
                return false;
            }

            float maxAge = withinSeconds > 0f ? withinSeconds : 8f;
            float age = Run.instance.GetRunStopwatch() - info.RunTime;

            return age >= 0f && age <= maxAge;
        }

        public static IReadOnlyList<MissionRecentSkillUse> GetRecentUses(
            float withinSeconds
        )
        {
            List<MissionRecentSkillUse> result =
                new List<MissionRecentSkillUse>();

            if (Run.instance == null)
            {
                return result;
            }

            float maxAge = withinSeconds > 0f ? withinSeconds : 8f;
            float now = Run.instance.GetRunStopwatch();

            PruneRecentUses(now);

            for (int i = RecentUses.Count - 1; i >= 0; i--)
            {
                MissionRecentSkillUse use = RecentUses[i];

                if (
                    use == null ||
                    use.PlayerMaster == null ||
                    use.PlayerBody == null ||
                    use.Skill == null
                )
                {
                    continue;
                }

                float age = now - use.RunTime;

                if (age < 0f || age > maxAge)
                {
                    continue;
                }

                result.Add(use);
            }

            // RecentUses se recorre al revés; el resultado ya está de más
            // reciente a más antiguo, que es el orden deseado para atribución.
            return result;
        }

        private static void PruneRecentUses(
            float now
        )
        {
            for (int i = RecentUses.Count - 1; i >= 0; i--)
            {
                MissionRecentSkillUse use = RecentUses[i];

                if (
                    use == null ||
                    now - use.RunTime > RecentHistoryRetentionSeconds
                )
                {
                    RecentUses.RemoveAt(i);
                }
            }

            if (RecentUses.Count <= MaxRecentHistoryEntries)
            {
                return;
            }

            int removeCount =
                RecentUses.Count - MaxRecentHistoryEntries;

            RecentUses.RemoveRange(
                0,
                removeCount
            );
        }

        private static string ResolveSlot(CharacterBody body, GenericSkill skill)
        {
            SkillLocator locator = body != null ? body.skillLocator : null;

            if (locator != null)
            {
                if (ReferenceEquals(locator.primary, skill)) return "Primary";
                if (ReferenceEquals(locator.secondary, skill)) return "Secondary";
                if (ReferenceEquals(locator.utility, skill)) return "Utility";
                if (ReferenceEquals(locator.special, skill)) return "Special";
            }

            return "";
        }
    }

    public sealed class MissionRecentSkillUse
    {
        public CharacterMaster PlayerMaster { get; }
        public CharacterBody PlayerBody { get; }
        public GenericSkill Skill { get; }
        public string Slot { get; }
        public string SkillToken { get; }
        public string SkillName { get; }
        public float RunTime { get; }

        public MissionRecentSkillUse(
            CharacterMaster playerMaster,
            CharacterBody playerBody,
            GenericSkill skill,
            string slot,
            string skillToken,
            string skillName,
            float runTime
        )
        {
            PlayerMaster = playerMaster;
            PlayerBody = playerBody;
            Skill = skill;
            Slot = slot ?? "";
            SkillToken = skillToken ?? "";
            SkillName = skillName ?? "";
            RunTime = runTime;
        }
    }
}
