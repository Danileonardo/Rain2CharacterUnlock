using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Rastreador de la misión oficial HEL-P. Sólo escucha eventos cuando
    /// el proveedor activo incluye los objetivos HEL-P en el snapshot de run.
    /// Usa Harmony para la desguazadora de drones del DLC, ya que NO es el
    /// ScrapperController de objetos que observa GameplayEventHub.
    /// </summary>
    public static class HelPDroneMissionHandler
    {
        private const string Healing = "HelPScrapHealing";
        private const string Gunner = "HelPScrapGunner";
        private const string Repair = "HelPRepairEmergency";
        private const string Beam = "HelPEmergencyBeam";
        private static ManualLogSource logger;
        private static bool initialized;
        private static readonly HashSet<string> ScrappedDroneIds =
            new HashSet<string>(StringComparer.Ordinal);
        private static readonly HashSet<int> RepairedPlayers =
            new HashSet<int>();
        private static readonly HashSet<int> BeamUsedPlayers =
            new HashSet<int>();

        private sealed class ScrapState
        {
            public CharacterMaster Owner;
            public CharacterBody Body;
            public string ObjectiveType;
            public string DroneId;
        }

        public static void Initialize(ManualLogSource log)
        {
            if (initialized) return;
            initialized = true;
            logger = log;
            GameplayEventHub.InteractionPerformed += OnInteraction;
            GameplayEventHub.SkillActivated += OnSkillActivated;
            MissionRunLifecycle.RunStarted += Reset;
            MissionRunLifecycle.RunDestroyed += Reset;

            // No dependemos del DLC para compilar. Si no está instalado,
            // no se crea hook y no se concede progreso falso.
            Type scrapperType = typeof(Run).Assembly.GetType(
                "RoR2.DroneScrapperController", false);
            MethodInfo begin = scrapperType?.GetMethod(
                "BeginScrapping", BindingFlags.Instance |
                BindingFlags.Public | BindingFlags.NonPublic);
            if (begin != null)
            {
                try
                {
                    new Harmony("com.danileo.UniversalSurvivorUnlocks.hel-p-drone")
                        .Patch(begin,
                            prefix: new HarmonyMethod(typeof(HelPDroneMissionHandler),
                                nameof(BeginScrappingPrefix)),
                            postfix: new HarmonyMethod(typeof(HelPDroneMissionHandler),
                                nameof(BeginScrappingPostfix)));
                    logger?.LogInfo("[HEL-P DRONES] Hook de DroneScrapper registrado.");
                }
                catch (Exception ex)
                {
                    logger?.LogWarning("[HEL-P DRONES] No se pudo conectar DroneScrapper: " + ex.Message);
                }
            }
            else
            {
                logger?.LogWarning("[HEL-P DRONES] DroneScrapperController.BeginScrapping no disponible.");
            }
        }

        private static void Reset(Run run)
        {
            ScrappedDroneIds.Clear();
            RepairedPlayers.Clear();
            BeamUsedPlayers.Clear();
        }

        // Harmony recibe los argumentos del método original como __args;
        // evita depender del nombre del parámetro del nuevo DLC.
        private static void BeginScrappingPrefix(object[] __args, out ScrapState __state)
        {
            __state = null;
            if (!NetworkServer.active || Run.instance == null ||
                __args == null || __args.Length < 1 ||
                (!IsActive(Healing) && !IsActive(Gunner))) return;
            if (!(__args[0] is NetworkInstanceId networkId)) return;
            GameObject droneObj = NetworkServer.FindLocalObject(networkId);
            CharacterBody drone = droneObj?.GetComponent<CharacterBody>();
            if (drone == null || drone.master?.minionOwnership?.ownerMaster == null) return;

            string bodyName = BodyCatalog.GetBodyName(drone.bodyIndex) ?? "";
            string kind = IsHealingDrone(bodyName) ? Healing :
                IsGunnerDrone(bodyName) ? Gunner : null;
            if (kind == null || !IsActive(kind)) return;
            CharacterMaster owner = drone.master.minionOwnership.ownerMaster;
            if (MissionPlayerIdentity.GetNetworkUser(owner) == null) return;
            __state = new ScrapState
            {
                Owner = owner, Body = owner.GetBody(),
                ObjectiveType = kind,
                DroneId = networkId.ToString()
            };
        }

        private static void BeginScrappingPostfix(ScrapState __state)
        {
            if (__state == null || !NetworkServer.active || Run.instance == null) return;
            // La desguazadora ha aceptado el BeginScrapping y ejecutado
            // la implementación original sin excepción.
            if (!ScrappedDroneIds.Add(__state.DroneId)) return;
            Advance(__state.ObjectiveType, __state.Owner, __state.Body);
        }

        private static void OnInteraction(GameplayInteractionEvent ev)
        {
            if (!NetworkServer.active || Run.instance == null ||
                !IsActive(Repair) || ev?.PlayerMaster == null ||
                ev.PlayerBody == null ||
                MissionPlayerIdentity.GetNetworkUser(ev.PlayerMaster) == null) return;
            string name = ev.TargetObjectName ?? "";
            // Sólo el reparable del Dron de emergencia, nunca el propio dron
            // vivo o el reparable de un dron diferente.
            if (name.IndexOf("EmergencyDrone", StringComparison.OrdinalIgnoreCase) < 0 ||
                (name.IndexOf("Broken", StringComparison.OrdinalIgnoreCase) < 0 &&
                 name.IndexOf("Repair", StringComparison.OrdinalIgnoreCase) < 0))
                return;
            if (!RepairedPlayers.Add(ev.PlayerMaster.GetInstanceID())) return;
            Advance(Repair, ev.PlayerMaster, ev.PlayerBody);
        }

        private static void OnSkillActivated(CharacterBody drone, GenericSkill skill)
        {
            if (!NetworkServer.active || Run.instance == null ||
                !IsActive(Beam) || drone == null || skill == null ||
                drone.master?.minionOwnership?.ownerMaster == null) return;
            if (!IsEmergencyDrone(BodyCatalog.GetBodyName(drone.bodyIndex))) return;
            // Detectamos la activación de habilidad primaria del Dron de
            // emergencia; debe validarse en partida que sea curativa.
            if (drone.skillLocator == null ||
                drone.skillLocator.primary != skill) return;
            CharacterMaster owner = drone.master.minionOwnership.ownerMaster;
            if (MissionPlayerIdentity.GetNetworkUser(owner) == null ||
                !RepairedPlayers.Contains(owner.GetInstanceID()) ||
                !BeamUsedPlayers.Add(owner.GetInstanceID())) return;
            Advance(Beam, owner, owner.GetBody());
        }

        private static bool IsActive(string type) =>
            MissionRuntimeActivityPlan.IsTypeActive(type) &&
            MissionRuntimeCatalog.HasObjectiveType(type);

        private static bool IsHealingDrone(string name) =>
            string.Equals(name, "Drone2Body", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(name, "Drone2", StringComparison.OrdinalIgnoreCase);
        private static bool IsGunnerDrone(string name) =>
            string.Equals(name, "Drone1Body", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(name, "Drone1", StringComparison.OrdinalIgnoreCase);
        private static bool IsEmergencyDrone(string name) =>
            !string.IsNullOrEmpty(name) &&
            name.IndexOf("EmergencyDrone", StringComparison.OrdinalIgnoreCase) >= 0;

        private static void Advance(string type, CharacterMaster master, CharacterBody body)
        {
            var user = MissionPlayerIdentity.GetNetworkUser(master);
            if (user == null || !IsActive(type)) return;
            var bindings = MissionRuntimeCatalog.GetObjectives(type);
            for (int i = 0; i < bindings.Count; i++)
            {
                MissionObjectiveRuntimeBinding binding = bindings[i];
                if (binding == null || binding.BodyName != "RobUltronBody") continue;
                var context = new MissionEventContext
                {
                    PlayerMaster = master, PlayerBody = body,
                    ActionBody = body, EventType = type
                };
                var result = MissionCompositionProgressEvaluator.AddProgress(
                    binding.MissionId, binding.Mission,
                    binding.RouteIndex, binding.ObjectiveIndex,
                    context, 1d);
                GenericMissionDispatcher.HandleProgressResult(
                    binding, result, "Dron: " + type, user);
            }
        }
    }
}
