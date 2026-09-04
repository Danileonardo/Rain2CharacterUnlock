using System;
using System.Collections.Generic;

using BepInEx.Logging;

using RoR2;

using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Historial mínimo y reutilizable de la última habilidad activada
    /// por cada jugador. Se utiliza por condiciones como PriorSkillUsed.
    /// No realiza polling.
    /// </summary>
    public static class MissionSkillHistoryTracker
    {
        private sealed class SkillUseInfo
        {
            public string Slot = "";
            public string SkillToken = "";
            public float RunTime;
        }

        private static readonly Dictionary<CharacterMaster, SkillUseInfo>
            LastUseByPlayer = new Dictionary<CharacterMaster, SkillUseInfo>();

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

            On.RoR2.CharacterBody.OnSkillActivated += CharacterBody_OnSkillActivated;
            Run.onRunStartGlobal += OnRunStart;
            Run.onRunDestroyGlobal += OnRunEnd;

            logger?.LogInfo("[MISSION V2] SkillHistory tracker inicializado.");
        }

        private static void OnRunStart(Run run)
        {
            LastUseByPlayer.Clear();
        }

        private static void OnRunEnd(Run run)
        {
            LastUseByPlayer.Clear();
        }

        private static void CharacterBody_OnSkillActivated(
            On.RoR2.CharacterBody.orig_OnSkillActivated orig,
            CharacterBody self,
            GenericSkill skill
        )
        {
            orig(self, skill);

            if (
                !NetworkServer.active ||
                Run.instance == null ||
                self == null ||
                skill == null ||
                !MissionRuntimeActivityPlan.IsTypeActive("PriorSkillUsed")
            )
            {
                return;
            }

            CharacterMaster master = self.master;
            if (master == null || MissionPlayerIdentity.GetNetworkUser(master) == null)
            {
                return;
            }

            string slot = ResolveSlot(self, skill);
            string token = skill.skillDef != null
                ? skill.skillDef.skillNameToken ?? ""
                : "";

            LastUseByPlayer[master] = new SkillUseInfo
            {
                Slot = slot,
                SkillToken = token,
                RunTime = Run.instance.GetRunStopwatch()
            };
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
}
