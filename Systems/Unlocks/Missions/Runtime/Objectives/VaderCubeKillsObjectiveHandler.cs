using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Logging;
using Newtonsoft.Json.Linq;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Contraorden 66: cuenta UNA baja si el enemigo esta dentro del campo
    /// RadialForce de un proyectil de Cubo Primordial activo. No infiere
    /// atraccion por movimiento ni confunde poseer el equipo con haberlo usado.
    /// Todos los eventos y las recompensas se resuelven en el servidor.
    /// </summary>
    public static class VaderCubeKillsObjectiveHandler
    {
        private const string ObjectiveType = "VaderCubeKills";
        private const float CubeLifetimeSafetySeconds = 11.5f;
        private const float DefaultCubeRadius = 30f;
        private static readonly List<CubeArea> ActiveCubes = new List<CubeArea>();
        private static bool initialized;
        private static bool warnedMissingForce;
        private static ManualLogSource logger;

        private sealed class CubeArea
        {
            internal ProjectileController Projectile;
            internal Component RadialForce;
            internal CharacterMaster CubeOwner;
            internal float SpawnedAt;
        }

        public static void Initialize(ManualLogSource log)
        {
            if (initialized) return;
            initialized = true;
            logger = log;
            On.RoR2.Projectile.ProjectileController.Start += OnProjectileStart;
            GameplayEventHub.CharacterDeath += OnEnemyDeath;
            MissionRunLifecycle.RunStarted += Reset;
            MissionRunLifecycle.RunDestroyed += Reset;
        }

        private static void Reset(Run unused)
        {
            ActiveCubes.Clear();
            warnedMissingForce = false;
        }

        private static bool Active => NetworkServer.active && Run.instance != null &&
            MissionProgressRegistry.IsRunActive &&
            MissionRuntimeActivityPlan.IsTypeActive(ObjectiveType) &&
            MissionRuntimeCatalog.HasObjectiveType(ObjectiveType);

        private static bool IsMercenary(CharacterBody body)
        {
            return body != null && body.master != null &&
                body.bodyIndex == BodyCatalog.FindBodyIndex("MercBody");
        }

        private static bool HoldingCube(CharacterBody body)
        {
            if (!IsMercenary(body) || body.inventory == null) return false;
            EquipmentIndex equipment = EquipmentCatalog.FindEquipmentIndex("Blackhole");
            return equipment != EquipmentIndex.None &&
                body.inventory.currentEquipmentIndex == equipment;
        }

        private static bool Qualifies(CharacterBody body, int requiredCrystals)
        {
            if (!HoldingCube(body)) return false;
            ItemIndex crystal = ItemCatalog.FindItemIndex("NearbyDamageBonus");
            return crystal != ItemIndex.None &&
                body.inventory.GetItemCountPermanent(crystal) >= requiredCrystals;
        }

        // Validamos el objeto real al nacer el proyectil, no solo el equipo
        // que lleva el jugador. El proyectil del Cubo se llama GravSphere.
        private static bool IsCubeProjectile(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            // La forma (Clone) es la instancia Unity; no aceptar prefabs
            // parecidos como GravSphereGhost u otros agujeros (bandas void).
            return name.Equals("GravSphere", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("GravSphere(Clone)", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("Blackhole", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("Blackhole(Clone)", StringComparison.OrdinalIgnoreCase);
        }

        private static Component FindRadialForce(GameObject projectile)
        {
            if (projectile == null) return null;
            foreach (Component component in projectile.GetComponentsInChildren<Component>(true))
                if (component != null &&
                    string.Equals(component.GetType().Name, "RadialForce", StringComparison.Ordinal))
                    return component;
            return null;
        }

        private static void OnProjectileStart(
            On.RoR2.Projectile.ProjectileController.orig_Start orig,
            ProjectileController projectile)
        {
            orig(projectile);
            if (!Active || projectile == null || projectile.owner == null ||
                !IsCubeProjectile(projectile.gameObject.name)) return;

            CharacterBody ownerBody = projectile.owner.GetComponent<CharacterBody>() ??
                projectile.owner.GetComponentInParent<CharacterBody>();
            if (!HoldingCube(ownerBody) ||
                MissionPlayerIdentity.GetNetworkUser(ownerBody.master) == null) return;

            Component force = FindRadialForce(projectile.gameObject);
            if (force == null)
            {
                if (!warnedMissingForce)
                {
                    warnedMissingForce = true;
                    logger?.LogWarning("[VADER] GravSphere sin RadialForce; " +
                        "las bajas no se contabilizan hasta confirmar el prefab del juego.");
                }
                return; // No falsificar alcance con una esfera arbitraria.
            }

            PruneCubes();
            ActiveCubes.Add(new CubeArea {
                Projectile = projectile,
                RadialForce = force,
                CubeOwner = ownerBody.master,
                SpawnedAt = Time.fixedTime
            });
            UsuLog.Verbose(logger, "[VADER] Cubo activo registrado | Owner: " +
                ownerBody.master.name + " | Prefab: " + projectile.gameObject.name);
        }

        private static bool IsLive(CubeArea cube)
        {
            return cube != null && cube.Projectile != null &&
                cube.RadialForce != null && cube.CubeOwner != null &&
                cube.Projectile.gameObject.activeInHierarchy &&
                cube.RadialForce.gameObject.activeInHierarchy &&
                cube.RadialForce is Behaviour &&
                ((Behaviour)cube.RadialForce).isActiveAndEnabled &&
                Time.fixedTime - cube.SpawnedAt <= CubeLifetimeSafetySeconds;
        }

        private static void PruneCubes()
        {
            for (int i = ActiveCubes.Count - 1; i >= 0; i--)
                if (!IsLive(ActiveCubes[i])) ActiveCubes.RemoveAt(i);
        }

        private static float ReadForceRadius(Component force)
        {
            // RoR2.RadialForce tiene un campo radius; reflexion mantiene
            // compatibilidad entre variantes de GameLibs sin referenciarlo.
            try
            {
                const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance;
                Type type = force.GetType();
                FieldInfo field = type.GetField("radius", flags);
                object value = field != null ? field.GetValue(force) : null;
                if (value == null)
                {
                    PropertyInfo prop = type.GetProperty("radius", flags);
                    value = prop != null ? prop.GetValue(force, null) : null;
                }
                if (value != null)
                {
                    float radius = Convert.ToSingle(value);
                    if (!float.IsNaN(radius) && !float.IsInfinity(radius) && radius > 0f)
                        return Mathf.Min(radius, 60f);
                }
            }
            catch (Exception ex)
            {
                UsuLog.Verbose(logger, "[VADER] RadialForce.radius no legible: " + ex.Message);
            }
            // El Cubo vanilla tiene un radio de 30m. Solo usamos este
            // respaldo cuando el RadialForce autentico ya fue identificado.
            return DefaultCubeRadius;
        }

        private static CubeArea FindCubeForDeath(CharacterBody victim, CharacterMaster killer)
        {
            CubeArea otherOwner = null;
            for (int i = 0; i < ActiveCubes.Count; i++)
            {
                CubeArea cube = ActiveCubes[i];
                CharacterBody ownerBody = cube.CubeOwner.GetBody();
                if (!HoldingCube(ownerBody) ||
                    MissionPlayerIdentity.GetNetworkUser(cube.CubeOwner) == null) continue;

                float radius = ReadForceRadius(cube.RadialForce);
                Vector3 delta = victim.corePosition - cube.RadialForce.transform.position;
                if (delta.sqrMagnitude > radius * radius) continue;
                if (cube.CubeOwner == killer) return cube;
                if (otherOwner == null) otherOwner = cube;
            }
            return otherOwner;
        }

        private static void OnEnemyDeath(DamageReport report)
        {
            if (!Active || report?.victimBody == null ||
                report.victimBody.teamComponent == null ||
                report.victimBody.teamComponent.teamIndex != TeamIndex.Monster) return;

            CharacterMaster killer = PlayerOwnerResolver.ResolveOwningPlayerMaster(
                report.attackerMaster ?? report.attackerBody?.master);
            CharacterBody killerBody = killer?.GetBody();
            NetworkUser killerUser = killer == null ? null :
                MissionPlayerIdentity.GetNetworkUser(killer);
            if (killerUser == null || !HoldingCube(killerBody)) return;

            PruneCubes();
            CubeArea cube = FindCubeForDeath(report.victimBody, killer);
            if (cube == null) return;

            foreach (MissionObjectiveRuntimeBinding binding in
                MissionRuntimeCatalog.GetObjectives(ObjectiveType))
            {
                if (binding?.Mission == null || binding.Objective == null ||
                    !string.Equals(binding.BodyName, "DarthVaderBody",
                        StringComparison.OrdinalIgnoreCase) ||
                    MissionProgressRegistry.IsMissionCompleted(
                        MissionProgressScope.PerPlayer, killerUser, binding.MissionId)) continue;

                int required = Math.Max(1,
                    binding.Objective.Parameters?.Value<int?>("requiredItemCount") ?? 10);
                // Tanto quien lanzo el Cubo como quien mata deben cumplir
                // las condiciones. Si son diferentes, ambos contribuyeron.
                if (!Qualifies(killerBody, required) ||
                    !Qualifies(cube.CubeOwner.GetBody(), required)) continue;

                var context = new MissionEventContext {
                    PlayerMaster = killer,
                    PlayerBody = killerBody,
                    ActionBody = killerBody,
                    TargetBody = report.victimBody,
                    EventType = ObjectiveType,
                    IsFatalHit = true
                };
                MissionCompositionProgressResult result =
                    MissionCompositionProgressEvaluator.AddProgress(
                        binding.MissionId, binding.Mission, binding.RouteIndex,
                        binding.ObjectiveIndex, context, 1d);

                // Si otro Mercenary activo el Cubo, se considera aporte
                // validado solo despues de que el evaluador acepto la baja.
                if (cube.CubeOwner != killer)
                    MissionContributionLedger.RecordValidatedVaderCubeOwner(
                        binding, result, cube.CubeOwner);

                GenericMissionDispatcher.HandleProgressResult(binding, result,
                    "Mercenary: muerte dentro del RadialForce del Cubo",
                    killerUser, report.victimBody);
            }
        }
    }
}
