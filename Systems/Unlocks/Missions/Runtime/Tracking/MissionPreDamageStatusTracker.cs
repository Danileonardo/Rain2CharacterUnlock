using System;
using System.Collections.Generic;

using BepInEx.Logging;

using EntityStates;
using RoR2;

using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Captura los estados de la víctima inmediatamente ANTES de que
    /// TakeDamageProcess aplique el golpe. La captura vive en una pila
    /// durante el procesamiento del daño, por lo que onCharacterDeathGlobal
    /// puede consultar el estado previo del golpe mortal.
    /// </summary>
    public static class MissionPreDamageStatusTracker
    {
        private sealed class Snapshot
        {
            public CharacterBody TargetBody;
            public readonly HashSet<string> StatusIds =
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        private static readonly Stack<Snapshot> ActiveSnapshots =
            new Stack<Snapshot>();

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

            On.RoR2.HealthComponent.TakeDamageProcess += HealthComponent_TakeDamageProcess;
            Run.onRunStartGlobal += OnRunStart;
            Run.onRunDestroyGlobal += OnRunEnd;

            logger?.LogInfo("[MISSION V2] PreDamageStatus tracker inicializado.");
        }

        private static void OnRunStart(Run run)
        {
            ActiveSnapshots.Clear();
        }

        private static void OnRunEnd(Run run)
        {
            ActiveSnapshots.Clear();
        }

        private static void HealthComponent_TakeDamageProcess(
            On.RoR2.HealthComponent.orig_TakeDamageProcess orig,
            HealthComponent self,
            DamageInfo damageInfo
        )
        {
            if (
                !NetworkServer.active ||
                Run.instance == null ||
                self == null ||
                !MissionRuntimeActivityPlan.IsTypeActive("StatusPresent")
            )
            {
                orig(self, damageInfo);
                return;
            }

            // StatusPresent se usa como condición de acciones de jugador.
            // Evitamos snapshots para daño ambiental o peleas entre NPCs.
            CharacterMaster attackerMaster = null;
            if (damageInfo.attacker != null)
            {
                CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                attackerMaster = attackerBody?.master;
            }

            CharacterMaster playerOwner =
                PlayerOwnerResolver.ResolveOwningPlayerMaster(attackerMaster);

            if (playerOwner == null)
            {
                orig(self, damageInfo);
                return;
            }

            CharacterBody targetBody = self.body;
            Snapshot snapshot = Capture(targetBody);
            ActiveSnapshots.Push(snapshot);

            try
            {
                orig(self, damageInfo);
            }
            finally
            {
                if (ActiveSnapshots.Count > 0)
                {
                    ActiveSnapshots.Pop();
                }
            }
        }

        public static bool HasStatusBeforeCurrentDamage(
            CharacterBody targetBody,
            string statusId
        )
        {
            if (targetBody == null || string.IsNullOrWhiteSpace(statusId))
            {
                return false;
            }

            foreach (Snapshot snapshot in ActiveSnapshots)
            {
                if (
                    snapshot != null &&
                    ReferenceEquals(snapshot.TargetBody, targetBody) &&
                    snapshot.StatusIds.Contains(statusId.Trim())
                )
                {
                    return true;
                }
            }

            return false;
        }

        private static Snapshot Capture(CharacterBody body)
        {
            Snapshot snapshot = new Snapshot
            {
                TargetBody = body
            };

            if (body == null)
            {
                return snapshot;
            }

            BuffIndex[] activeBuffs = body.activeBuffsList;
            if (activeBuffs != null)
            {
                for (int i = 0; i < activeBuffs.Length; i++)
                {
                    BuffDef buffDef = BuffCatalog.GetBuffDef(activeBuffs[i]);
                    if (buffDef == null || body.GetBuffCount(buffDef) <= 0)
                    {
                        continue;
                    }

                    snapshot.StatusIds.Add("buff:" + buffDef.name);
                }
            }

            DotController dotController = DotController.FindDotController(body.gameObject);
            if (dotController != null && dotController.dotStackList != null)
            {
                for (int i = 0; i < dotController.dotStackList.Count; i++)
                {
                    var stack = dotController.dotStackList[i];
                    string dotName = stack.dotIndex.ToString();
                    snapshot.StatusIds.Add("dot:" + dotName);

                    var dotDef = DotController.GetDotDef(stack.dotIndex);
                    if (dotDef != null && dotDef.associatedBuff != null)
                    {
                        snapshot.StatusIds.Add("dot:" + dotDef.associatedBuff.name);
                    }
                }
            }

            EntityStateMachine stateMachine = null;
            SetStateOnHurt setStateOnHurt = body.GetComponent<SetStateOnHurt>();
            if (setStateOnHurt != null)
            {
                stateMachine = setStateOnHurt.targetStateMachine;
            }

            if (stateMachine != null && stateMachine.state != null)
            {
                if (stateMachine.state is FrozenState)
                {
                    snapshot.StatusIds.Add("cc:Freeze");
                }

                if (stateMachine.state is StunState)
                {
                    snapshot.StatusIds.Add("cc:Stun");
                }
            }

            return snapshot;
        }
    }
}
