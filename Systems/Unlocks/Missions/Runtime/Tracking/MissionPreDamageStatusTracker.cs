using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

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
            public readonly HashSet<string> PoisonApplierKeys =
                new HashSet<string>(StringComparer.Ordinal);
        }

        private static readonly Stack<Snapshot> ActiveSnapshots =
            new Stack<Snapshot>();

        // Identidad de jugadores que hicieron daño real a un jefe durante
        // esta escena. Solo se consulta si una baja satisface un objetivo
        // Kill PerPlayer; tocar cualquier jefe no concede premios por sí solo.
        private static readonly Dictionary<CharacterBody, HashSet<string>> BossDamageContributors =
            new Dictionary<CharacterBody, HashSet<string>>();
        private const int MaxTrackedBosses = 128;

        internal static List<string> GetBossDamageContributorKeys(CharacterBody boss)
        {
            HashSet<string> keys;
            return boss != null && BossDamageContributors.TryGetValue(boss, out keys)
                ? new List<string>(keys) : new List<string>();
        }

        internal static void ForgetBossDamageContributors(CharacterBody boss)
        {
            if (boss != null) BossDamageContributors.Remove(boss);
        }

        // Algunas compilaciones notifican la baja después de salir de
        // TakeDamageProcess. Conservamos la autoría de la última observación
        // únicamente mientras haya veneno activo en esa víctima.
        private static readonly Dictionary<CharacterBody, HashSet<string>> LastPoisonByTarget =
            new Dictionary<CharacterBody, HashSet<string>>();

        // Evidencia adicional si un tick de Poison expone al autor antes de
        // que el DotController consolide varias aplicaciones.
        private static readonly Dictionary<CharacterBody, HashSet<string>> PoisonTickOwners =
            new Dictionary<CharacterBody, HashSet<string>>();

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
            MissionRunLifecycle.RunStarted += OnRunStart;
            MissionRunLifecycle.RunDestroyed += OnRunEnd;
            MissionStageRuntimeTracker.StageStarted += OnStageStarted;

            UsuLog.Verbose(
                logger,
                "[MISSION V2] PreDamageStatus tracker inicializado."
            );
        }

        private static void OnRunStart(Run run)
        {
            ActiveSnapshots.Clear();
            LastPoisonByTarget.Clear();
            PoisonTickOwners.Clear();
            BossDamageContributors.Clear();
        }

        private static void OnRunEnd(Run run)
        {
            ActiveSnapshots.Clear();
            LastPoisonByTarget.Clear();
            PoisonTickOwners.Clear();
            BossDamageContributors.Clear();
        }

        private static void OnStageStarted(MissionStageEventContext context)
        {
            LastPoisonByTarget.Clear();
            PoisonTickOwners.Clear();
            BossDamageContributors.Clear();
        }

        private static void HealthComponent_TakeDamageProcess(
            On.RoR2.HealthComponent.orig_TakeDamageProcess orig,
            HealthComponent self,
            DamageInfo damageInfo
        )
        {
            bool observeStatus = MissionRuntimeActivityPlan.IsTypeActive("StatusPresent");
            bool observeBossAssist = MissionRuntimeActivityPlan.IsTypeActive("Kill") &&
                self?.body != null && self.body.isBoss;
            if (!NetworkServer.active || Run.instance == null || self == null ||
                (!observeStatus && !observeBossAssist))
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
            if (observeBossAssist && damageInfo.damage > 0f)
            {
                MissionPlayerIdentity ownerIdentity;
                if (MissionPlayerIdentity.TryCreate(playerOwner, out ownerIdentity) &&
                    !string.IsNullOrWhiteSpace(ownerIdentity.Key))
                {
                    HashSet<string> contributors;
                    if (BossDamageContributors.TryGetValue(targetBody, out contributors) ||
                        BossDamageContributors.Count < MaxTrackedBosses)
                    {
                        if (contributors == null)
                        {
                            contributors = new HashSet<string>(StringComparer.Ordinal);
                            BossDamageContributors.Add(targetBody, contributors);
                        }
                        contributors.Add(ownerIdentity.Key);
                    }
                }
            }

            if (!observeStatus)
            {
                orig(self, damageInfo);
                return;
            }
            // Una aplicacion previa del veneno y una mordida posterior pueden
            // proceder de dos jugadores diferentes. Guardar autores que RoR2
            // explicita en los ticks, no suponer que el mordedor enveneno.
            if (targetBody != null && IsPoisonDamageTick(damageInfo))
            {
                MissionPlayerIdentity poisonIdentity;
                if (MissionPlayerIdentity.TryCreate(playerOwner, out poisonIdentity))
                {
                    HashSet<string> owners;
                    if (!PoisonTickOwners.TryGetValue(targetBody, out owners))
                    {
                        owners = new HashSet<string>(StringComparer.Ordinal);
                        PoisonTickOwners[targetBody] = owners;
                    }
                    owners.Add(poisonIdentity.Key);
                }
            }
            Snapshot snapshot = Capture(targetBody);
            if (targetBody != null)
            {
                if (snapshot.PoisonApplierKeys.Count > 0)
                    LastPoisonByTarget[targetBody] =
                        new HashSet<string>(snapshot.PoisonApplierKeys, StringComparer.Ordinal);
                else
                    LastPoisonByTarget.Remove(targetBody);
            }
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

        // Solo entrega identidad de un veneno observado ANTES del golpe letal.
        // Nunca atribuye al último atacante la propiedad del DoT.
        internal static List<string> GetPoisonAppliersBeforeCurrentDamage(CharacterBody targetBody)
        {
            var keys = new HashSet<string>(StringComparer.Ordinal);
            if (targetBody == null) return new List<string>();

            foreach (Snapshot snapshot in ActiveSnapshots)
            {
                if (snapshot == null || !ReferenceEquals(snapshot.TargetBody, targetBody))
                    continue;
                foreach (string key in snapshot.PoisonApplierKeys)
                    keys.Add(key);
                return new List<string>(keys);
            }

            HashSet<string> lastSeen;
            if (LastPoisonByTarget.TryGetValue(targetBody, out lastSeen))
            {
                foreach (string key in lastSeen) keys.Add(key);
                LastPoisonByTarget.Remove(targetBody);
            }
            return new List<string>(keys);
        }

        // Reflexion para evitar depender de un campo de DamageInfo que
        // puede cambiar entre versiones de Risk of Rain 2.
        private static bool IsPoisonDamageTick(DamageInfo info)
        {
            if (info == null) return false;
            try
            {
                Type t = info.GetType();
                const BindingFlags flags = BindingFlags.Instance |
                    BindingFlags.Public | BindingFlags.NonPublic;
                FieldInfo field = t.GetField("dotIndex", flags);
                object value = field != null ? field.GetValue(info) : null;
                if (field == null)
                {
                    PropertyInfo prop = t.GetProperty("dotIndex", flags);
                    value = prop?.GetValue(info, null);
                }
                return string.Equals(value?.ToString(), "Poison", StringComparison.OrdinalIgnoreCase);
            }
            catch { return false; }
        }

        // Los datos de DotStack pueden variar entre builds de RoR2.
        // Leemos solo el miembro de autoría conocido y fallamos cerrados.
        private static GameObject GetDotAttackerObject(object dotStack)
        {
            if (dotStack == null) return null;
            try
            {
                Type t = dotStack.GetType();
                const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                foreach (string name in new[] { "attackerObject", "attackerGameObject", "attacker" })
                {
                    FieldInfo field = t.GetField(name, flags);
                    object value = field != null ? field.GetValue(dotStack) : null;
                    if (field == null)
                    {
                        PropertyInfo property = t.GetProperty(name, flags);
                        value = property?.GetValue(dotStack, null);
                    }
                    if (value is GameObject obj) return obj;
                    if (value is Component component) return component.gameObject;
                }
            }
            catch (Exception) { /* Build incompatible: no se inventa contribuyente. */ }
            return null;
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

                    if (string.Equals(dotName, "Poison", StringComparison.OrdinalIgnoreCase))
                    {
                        GameObject source = GetDotAttackerObject(stack);
                        CharacterBody attackerBody = source == null ? null :
                            source.GetComponent<CharacterBody>();
                        CharacterMaster attackerMaster = attackerBody?.master ??
                            (source == null ? null : source.GetComponent<CharacterMaster>());
                        CharacterMaster owner = PlayerOwnerResolver.ResolveOwningPlayerMaster(
                            attackerMaster);
                        MissionPlayerIdentity identity;
                        if (owner != null && MissionPlayerIdentity.TryCreate(owner, out identity) &&
                            !string.IsNullOrWhiteSpace(identity.Key))
                            snapshot.PoisonApplierKeys.Add(identity.Key);
                    }

                    var dotDef = DotController.GetDotDef(stack.dotIndex);
                    if (dotDef != null && dotDef.associatedBuff != null)
                    {
                        snapshot.StatusIds.Add("dot:" + dotDef.associatedBuff.name);
                    }
                }
            }

            if (snapshot.StatusIds.Contains("dot:Poison"))
            {
                HashSet<string> owners;
                if (PoisonTickOwners.TryGetValue(body, out owners))
                    foreach (string key in owners) snapshot.PoisonApplierKeys.Add(key);
            }
            else
            {
                // Un veneno anterior ya expirado no participa en esta muerte.
                PoisonTickOwners.Remove(body);
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
