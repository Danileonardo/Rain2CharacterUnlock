using System;
using Newtonsoft.Json.Linq;
using System.Reflection;
using BepInEx.Logging;
using RoR2;
using RoR2.Orbs;
using UnityEngine;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Cuenta solamente DevilOrb REALES entregados a OrbManager por N'kuhana.
    /// Nada de contar curaciones, impactos, muertes o disparos estimados.
    /// Autoridad server, misión Shared con participantes personales validados.
    /// </summary>
    public static class KnightNkuhanaSkullsObjectiveHandler
    {
        private const string ObjectiveType = "KnightNkuhanaSkulls";
        private static bool initialized;
        private static ManualLogSource logger;

        public static void Initialize(ManualLogSource log)
        {
            if (initialized) return;
            initialized = true;
            logger = log;
            On.RoR2.Orbs.OrbManager.AddOrb += OnOrbAdded;
        }

        private static void OnOrbAdded(On.RoR2.Orbs.OrbManager.orig_AddOrb orig,
            OrbManager self, Orb orb)
        {
            // Observamos el evento verdadero, sin alterar el proceso de RoR2.
            orig(self, orb);
            if (!NetworkServer.active || Run.instance == null ||
                !MissionProgressRegistry.IsRunActive ||
                !MissionRuntimeActivityPlan.IsTypeActive(ObjectiveType) ||
                !(orb is DevilOrb) ||
                !MissionRuntimeCatalog.HasObjectiveType(ObjectiveType)) return;

            // Versiones de RoR2 exponen la fuente mediante diferentes miembros.
            // Sin una fuente identificable se descarta el evento (no inventamos autoría).
            CharacterMaster owner = ResolveOwner(orb);
            CharacterBody body = owner?.GetBody();
            if (body == null || body.bodyIndex != BodyCatalog.FindBodyIndex("TreebotBody") ||
                MissionPlayerIdentity.GetNetworkUser(owner) == null || body.inventory == null)
                return;

            var bindings = MissionRuntimeCatalog.GetObjectives(ObjectiveType);
            for (int i = 0; i < bindings.Count; i++)
            {
                var binding = bindings[i];
                if (binding?.Mission == null || binding.Objective == null ||
                    !string.Equals(binding.BodyName, "KnightBody", StringComparison.OrdinalIgnoreCase))
                    continue;
                var p = binding.Objective.Parameters;
                string infusionName = p?.Value<string>("infusion") ?? "Infusion";
                string opinionName = p?.Value<string>("opinion") ?? "NovaOnHeal";
                int required = Math.Max(1, p?.Value<int?>("infusionCount") ?? 4);
                ItemIndex infusion = ItemCatalog.FindItemIndex(infusionName);
                ItemIndex opinion = ItemCatalog.FindItemIndex(opinionName);
                if (infusion == ItemIndex.None || opinion == ItemIndex.None ||
                    body.inventory.GetItemCountPermanent(infusion) < required ||
                    body.inventory.GetItemCountPermanent(opinion) < 1) continue;

                var ctx = new MissionEventContext {
                    PlayerMaster = owner, PlayerBody = body, ActionBody = body,
                    TargetBody = body, EventType = ObjectiveType
                };
                var result = MissionCompositionProgressEvaluator.AddProgress(
                    binding.MissionId, binding.Mission, binding.RouteIndex,
                    binding.ObjectiveIndex, ctx, 1d);
                GenericMissionDispatcher.HandleProgressResult(binding, result,
                    "N'kuhana: DevilOrb emitido (confirmado)");
            }
        }

        private static CharacterMaster ResolveOwner(Orb orb)
        {
            object source = ReadMember(orb, "attacker") ?? ReadMember(orb, "source") ??
                ReadMember(orb, "attackerBody") ?? ReadMember(orb, "attackerMaster") ??
                ReadMember(orb, "owner");
            if (source is CharacterMaster master) return PlayerOwnerResolver.ResolveOwningPlayerMaster(master);
            if (source is CharacterBody body) return PlayerOwnerResolver.ResolveOwningPlayerMaster(body.master);
            if (source is GameObject go)
            {
                CharacterBody b = go.GetComponent<CharacterBody>();
                if (b != null) return PlayerOwnerResolver.ResolveOwningPlayerMaster(b.master);
                CharacterMaster m = go.GetComponent<CharacterMaster>();
                return m != null ? PlayerOwnerResolver.ResolveOwningPlayerMaster(m) : null;
            }
            if (source is Component component)
                return ResolveFromComponent(component);
            return null;
        }

        private static CharacterMaster ResolveFromComponent(Component component)
        {
            if (component == null) return null;
            CharacterBody b = component.GetComponent<CharacterBody>();
            return b != null ? PlayerOwnerResolver.ResolveOwningPlayerMaster(b.master) : null;
        }

        private static object ReadMember(object target, string name)
        {
            if (target == null) return null;
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            try
            {
                Type type = target.GetType();
                while (type != null)
                {
                    FieldInfo field = type.GetField(name, flags | BindingFlags.DeclaredOnly);
                    if (field != null) return field.GetValue(target);
                    PropertyInfo prop = type.GetProperty(name, flags | BindingFlags.DeclaredOnly);
                    if (prop != null && prop.GetIndexParameters().Length == 0)
                        return prop.GetValue(target, null);
                    type = type.BaseType;
                }
            }
            catch (Exception ex) { logger?.LogWarning("[KNIGHT] Orbe sin fuente recuperable: " + ex.Message); }
            return null;
        }
    }
}
