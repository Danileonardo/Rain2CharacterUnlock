using System.Collections.Generic;

using BepInEx.Logging;

using RoR2;

using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Objetivo Mission V2 basado en activaciones reales de habilidad.
    /// G8 lo usa para ResourceMastery: cada activación de una skill de recurso
    /// compatible suma una unidad al contador Shared.
    /// </summary>
    public static class UseSkillObjectiveHandler
    {
        private const string ObjectiveType = "UseSkill";

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

            GameplayEventHub.SkillActivated += OnSkillActivated;

            UsuLog.Verbose(
                logger,
                "[MISSION V2] UseSkill handler inicializado."
            );
        }

        private static void OnSkillActivated(
            CharacterBody actionBody,
            GenericSkill skill
        )
        {
            if (
                !NetworkServer.active ||
                Run.instance == null ||
                !MissionProgressRegistry.IsRunActive ||
                actionBody == null ||
                skill == null ||
                !MissionRuntimeActivityPlan.IsTypeActive(ObjectiveType) ||
                !MissionRuntimeCatalog.HasObjectiveType(ObjectiveType)
            )
            {
                return;
            }

            CharacterMaster playerMaster =
                PlayerOwnerResolver.ResolveOwningPlayerMaster(
                    actionBody.master
                );

            CharacterBody playerBody =
                playerMaster?.GetBody();

            if (playerMaster == null || playerBody == null)
            {
                return;
            }

            MissionEventContext context =
                new MissionEventContext
                {
                    PlayerMaster = playerMaster,
                    PlayerBody = playerBody,
                    ActionBody = actionBody,
                    TargetBody = playerBody,
                    ActivatedSkill = skill,
                    EventType = ObjectiveType
                };

            IReadOnlyList<MissionObjectiveRuntimeBinding> bindings =
                MissionRuntimeCatalog.GetObjectives(ObjectiveType);

            for (int i = 0; i < bindings.Count; i++)
            {
                MissionObjectiveRuntimeBinding binding = bindings[i];

                if (
                    binding == null ||
                    binding.Mission == null ||
                    binding.Objective == null
                )
                {
                    continue;
                }

                MissionCompositionProgressResult result =
                    MissionCompositionProgressEvaluator.AddProgress(
                        binding.MissionId,
                        binding.Mission,
                        binding.RouteIndex,
                        binding.ObjectiveIndex,
                        context,
                        1d
                    );

                GenericMissionDispatcher.HandleProgressResult(
                    binding,
                    result,
                    result != null
                        ? $"Usos: {result.CurrentValue:0.##}/{result.RequiredValue:0.##}"
                        : ""
                );
            }
        }
    }
}
