using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Logging;
using RoR2;
using RoR2.Orbs;
using UnityEngine;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Dedicated objective types for the Knight and Hornet USU recipes.
    /// None of them directly grants unlocks or changes player damage.
    /// The generic mission dispatcher performs the usual host-authoritative grant.
    /// </summary>
    public static class ThreeSurvivorObjectiveHandlers
    {
        private const string KnightType = "KnightSkullLaunches";
        private const string HornetType = "HornetFlyingHookKills";
        private static bool initialized;
        private static ManualLogSource logger;
        // Diagnóstico limitado: nunca registrar un mensaje por cada orbe.
        private static bool knightUnresolvedSourceReported;
        private static bool knightFirstSkullReported;
        private static readonly Dictionary<CharacterMaster, HookState> Hooks =
            new Dictionary<CharacterMaster, HookState>();

        private sealed class HookState
        {
            public GenericSkill Skill;
            public float ActivatedAt;
            public int StockAtLaunch;
            public bool Confirmed;
            public bool TookFlight;
            public float ConnectedAt;
        }

        public static void Initialize(ManualLogSource log)
        {
            if (initialized) return;
            initialized = true;
            logger = log;
            GameplayEventHub.CharacterDeath += OnCharacterDeath;
            // Antes de ejecutar la activación: conservar el stock real de ese lanzamiento.
            GameplayEventHub.SkillActivationStarted += OnSkillActivationStarted;
            GameplayEventHub.FixedUpdateTick += OnFixedUpdate;
            On.RoR2.Orbs.OrbManager.AddOrb += OnAddOrb;
            MissionRunLifecycle.RunStarted += Reset;
            MissionRunLifecycle.RunDestroyed += Reset;
            UsuLog.Verbose(logger, "[USU] Knight, Hornet handlers initialized; Vader uses its dedicated area tracker");
        }

        private static void Reset(Run unused)
        {
            Hooks.Clear();
            knightUnresolvedSourceReported = false;
            knightFirstSkullReported = false;
        }
        private static bool Active(string type)
        {
            return NetworkServer.active && Run.instance != null &&
                MissionProgressRegistry.IsRunActive &&
                MissionRuntimeActivityPlan.IsTypeActive(type) &&
                MissionRuntimeCatalog.HasObjectiveType(type);
        }
        private static bool IsBody(CharacterBody body, string name)
        {
            return body != null && body.master != null &&
                body.bodyIndex == BodyCatalog.FindBodyIndex(name);
        }
        private static bool HasItem(CharacterBody body, string name, int count)
        {
            if (body?.inventory == null) return false;
            ItemIndex index = ItemCatalog.FindItemIndex(name);
            return index != ItemIndex.None &&
                body.inventory.GetItemCountPermanent(index) >= count;
        }

        // Opinión de N'kuhana emite RoR2.Orbs.DevilOrb. Registramos la emisión
        // al entregarse el orbe al OrbManager, sin esperar impacto ni muerte.
        // Este es el ÚNICO rastreador activo para KnightSkullLaunches.
        private static void OnAddOrb(On.RoR2.Orbs.OrbManager.orig_AddOrb orig,
            OrbManager self, Orb orb)
        {
            // Primero dejamos que RoR2 procese el orbe; si falla, no sumamos.
            orig(self, orb);
            if (!Active(KnightType) || !(orb is DevilOrb)) return;

            // 'attacker' suele ser un GameObject, pero algunos orbes/mods pueden
            // exponer Component, CharacterBody o CharacterMaster. No inferimos
            // el dueño por proximidad, porque eso sumaría cráneos ajenos.
            CharacterBody body = ResolveSkullOwnerBody(orb);
            if (body == null)
            {
                if (!knightUnresolvedSourceReported)
                {
                    knightUnresolvedSourceReported = true;
                    logger?.LogWarning("[USU Knight] DevilOrb sin atacante identificable. " +
                        "No se suma progreso; revisar el origen del orbe en esta versión de RoR2.");
                }
                return;
            }

            if (!IsBody(body, "TreebotBody") ||
                MissionPlayerIdentity.GetNetworkUser(body.master) == null ||
                !HasItem(body, "Infusion", 4) ||
                !HasItem(body, "NovaOnHeal", 1)) return;

            if (!knightFirstSkullReported)
            {
                knightFirstSkullReported = true;
                logger?.LogInfo("[USU Knight] Primer DevilOrb válido de REX detectado " +
                    "(4 Infusiones + Opinión de N'kuhana). El progreso lo controla Mission V2.");
            }

            AddProgress(KnightType, "KnightBody", body.master, body,
                null, "REX: N'kuhana spectral skull launched");
        }

        private static CharacterBody ResolveSkullOwnerBody(Orb orb)
        {
            // No usamos origen espacial ni el objetivo: podrían pertenecer a
            // otros jugadores. Solo admitimos referencias expresas al atacante.
            object source = ReadOrbMember(orb, "attacker") ??
                ReadOrbMember(orb, "attackerBody") ??
                ReadOrbMember(orb, "attackerMaster") ??
                ReadOrbMember(orb, "source") ??
                ReadOrbMember(orb, "owner");
            if (source is CharacterBody body) return body;
            if (source is CharacterMaster master) return master.GetBody();
            if (source is Component component)
            {
                CharacterBody componentBody = component.GetComponent<CharacterBody>() ??
                    component.GetComponentInParent<CharacterBody>();
                if (componentBody != null) return componentBody;
                CharacterMaster componentMaster = component.GetComponent<CharacterMaster>() ??
                    component.GetComponentInParent<CharacterMaster>();
                return componentMaster != null ? componentMaster.GetBody() : null;
            }
            if (source is GameObject go)
            {
                CharacterBody goBody = go.GetComponent<CharacterBody>() ??
                    go.GetComponentInParent<CharacterBody>();
                if (goBody != null) return goBody;
                CharacterMaster goMaster = go.GetComponent<CharacterMaster>() ??
                    go.GetComponentInParent<CharacterMaster>();
                return goMaster != null ? goMaster.GetBody() : null;
            }
            return null;
        }

        private static object ReadOrbMember(object source, string name)
        {
            if (source == null) return null;
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            try
            {
                // Buscar también en clases base para cubrir miembros heredados.
                for (Type type = source.GetType(); type != null; type = type.BaseType)
                {
                    FieldInfo field = type.GetField(name, flags);
                    if (field != null) return field.GetValue(source);
                    PropertyInfo property = type.GetProperty(name, flags);
                    if (property != null && property.GetIndexParameters().Length == 0)
                        return property.GetValue(source, null);
                }
            }
            catch (Exception ex)
            {
                if (!knightUnresolvedSourceReported)
                {
                    knightUnresolvedSourceReported = true;
                    logger?.LogWarning("[USU Knight] No se pudo leer la autoría del orbe: " + ex.Message);
                }
            }
            return null;
        }

        // En Loader vanilla, ambos ganchos gastan su carga secundaria al
        // ENGANCHAR con una superficie, enemigo, dron, etc.; un fallo no la gasta.
        // Registrar el stock antes del lanzamiento evita confundir una carga
        // gastada anteriormente con el enganche del intento actual.
        private static void OnSkillActivationStarted(CharacterBody body, GenericSkill skill)
        {
            if (!Active(HornetType) || !InSirenCall() ||
                !IsBody(body, "LoaderBody") || body.skillLocator == null ||
                skill == null || skill != body.skillLocator.secondary) return;
            if (MissionPlayerIdentity.GetNetworkUser(body.master) == null) return;

            Hooks[body.master] = new HookState
            {
                Skill = skill,
                StockAtLaunch = skill.stock,
                ActivatedAt = Run.instance.GetRunStopwatch(),
                Confirmed = false,
                TookFlight = false
            };
        }

        private static bool InSirenCall()
        {
            return string.Equals(MissionStageRuntimeTracker.CurrentStageName,
                "shipgraveyard", StringComparison.OrdinalIgnoreCase);
        }

        private static void OnFixedUpdate()
        {
            if (!Active(HornetType) || !InSirenCall())
            {
                Hooks.Clear();
                return;
            }
            if (Hooks.Count == 0) return;

            float now = Run.instance.GetRunStopwatch();
            var gone = new List<CharacterMaster>();
            foreach (var pair in Hooks)
            {
                CharacterBody body = pair.Key != null ? pair.Key.GetBody() : null;
                HookState state = pair.Value;
                if (body == null || !IsBody(body, "LoaderBody") ||
                    body.characterMotor == null || body.skillLocator == null ||
                    state.Skill != body.skillLocator.secondary)
                {
                    gone.Add(pair.Key);
                    continue;
                }

                if (!state.Confirmed)
                {
                    // La conexión puede ser con PARED/SUELO/ESTRUCTURA/ENEMIGO:
                    // solo importa la carga efectivamente consumida por el enganche.
                    if (state.StockAtLaunch > 0 &&
                        state.Skill.stock < state.StockAtLaunch)
                    {
                        state.Confirmed = true;
                        state.ConnectedAt = now;
                    }
                    else if (now - state.ActivatedAt > 6f)
                    {
                        // El proyectil falló o no conectó: nunca abrir ventana.
                        gone.Add(pair.Key);
                        continue;
                    }
                }

                if (!state.Confirmed) continue;
                if (!body.characterMotor.isGrounded)
                {
                    // Puede soltarse del gancho o usar cualquier ataque.
                    // La ventana dura hasta ATERRIZAR, sin expiración arbitraria.
                    state.TookFlight = true;
                }
                else if (state.TookFlight || now - state.ConnectedAt > 1.5f)
                {
                    // Si conectó desde el suelo, damos un margen para despegar.
                    // Después de haber volado, tocar tierra siempre cierra la ventana.
                    gone.Add(pair.Key);
                }
            }
            foreach (var key in gone) Hooks.Remove(key);
        }

        private static readonly HashSet<string> FlyingEnemyBodies = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase)
        {
            "VultureBody", "WispBody", "GreaterWispBody", "JellyfishBody",
            "LunarWispBody", "RoboBallMiniBody", "RoboBallBossBody",
            "FlyingVerminBody", "FlyingVerminBodySmall", "VoidJailerBody"
        };
        private static bool IsAirborneFlyingEnemy(CharacterBody target)
        {
            if (target == null || target.teamComponent == null ||
                target.teamComponent.teamIndex != TeamIndex.Monster) return false;
            if (!FlyingEnemyBodies.Contains(target.name.Replace("(Clone)", "").Trim()))
            {
                // BodyCatalog body index is preferable to prefab name when renamed.
                string catalogName = BodyCatalog.GetBodyName(target.bodyIndex);
                if (!FlyingEnemyBodies.Contains(catalogName)) return false;
            }
            if (target.characterMotor != null && target.characterMotor.isGrounded) return false;
            RaycastHit hit;
            if (Physics.Raycast(target.corePosition, Vector3.down, out hit,
                2f, LayerIndex.world.mask)) return false;
            return true;
        }
        private static void OnCharacterDeath(DamageReport report)
        {
            if (report?.victimBody == null || report.victimBody.teamComponent == null ||
                report.victimBody.teamComponent.teamIndex != TeamIndex.Monster) return;
            CharacterMaster attackerMaster =
                PlayerOwnerResolver.ResolveOwningPlayerMaster(report.attackerMaster);
            CharacterBody attacker = attackerMaster?.GetBody();
            if (attacker == null || MissionPlayerIdentity.GetNetworkUser(attackerMaster) == null) return;
            if (Active(HornetType) && InSirenCall() &&
                IsBody(attacker, "LoaderBody") &&
                attacker.characterMotor != null && !attacker.characterMotor.isGrounded &&
                IsAirborneFlyingEnemy(report.victimBody) &&
                Hooks.TryGetValue(attackerMaster, out HookState hook) &&
                hook.Confirmed && hook.TookFlight)
            {
                // Cualquier ataque/remate es válido; requisito: enganche real,
                // Loader aérea y objetivo efectivamente volador en Siren's Call.
                AddProgress(HornetType, "HornetBody", attackerMaster, attacker,
                    report.victimBody, "Loader: flying enemy after successful grapple");
            }

        }

        private static void AddProgress(string type, string unlockBody,
            CharacterMaster master, CharacterBody performer,
            CharacterBody victim, string detail)
        {
            var bindings = MissionRuntimeCatalog.GetObjectives(type);
            for (int i = 0; i < bindings.Count; i++)
            {
                MissionObjectiveRuntimeBinding binding = bindings[i];
                if (binding?.Mission == null || binding.Objective == null ||
                    !string.Equals(binding.BodyName, unlockBody, StringComparison.OrdinalIgnoreCase)) continue;
                NetworkUser user = MissionPlayerIdentity.GetNetworkUser(master);
                if (user == null || MissionProgressRegistry.IsMissionCompleted(
                    MissionProgressScope.PerPlayer, user, binding.MissionId)) continue;
                var context = new MissionEventContext
                {
                    PlayerMaster = master, PlayerBody = performer,
                    ActionBody = performer, TargetBody = victim,
                    EventType = type, IsFatalHit = victim != null
                };
                var result = MissionCompositionProgressEvaluator.AddProgress(
                    binding.MissionId, binding.Mission,
                    binding.RouteIndex, binding.ObjectiveIndex, context, 1d);
                GenericMissionDispatcher.HandleProgressResult(binding, result, detail,
                    user, victim);
            }
        }
    }
}
