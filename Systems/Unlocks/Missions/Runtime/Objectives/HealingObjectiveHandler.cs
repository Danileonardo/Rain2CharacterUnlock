using System.Collections.Generic;

using BepInEx.Logging;

using RoR2;

using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Objetivo Mission V2 de curación acumulada.
    ///
    /// HealthComponent.Heal no expone directamente al sanador. Para no contar
    /// regeneración/items arbitrarios como una misión de soporte, atribuimos la
    /// curación únicamente cuando existe una activación reciente de una skill
    /// de curación perteneciente a un performer permitido por la Generated.
    ///
    /// La ventana es deliberadamente corta y sólo se usa para atribución; el
    /// progreso sigue siendo Shared y la curación contabilizada es la real.
    /// </summary>
    public static class HealingObjectiveHandler
    {
        private const string ObjectiveType = "Heal";
        private const float AttributionWindowSeconds = 8f;

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

            GameplayEventHub.HealingProcessed += OnHealingProcessed;

            UsuLog.Verbose(
                logger,
                "[MISSION V2] Heal handler inicializado."
            );
        }

        private static void OnHealingProcessed(
            GameplayHealingEvent healingEvent
        )
        {
            if (
                !NetworkServer.active ||
                Run.instance == null ||
                !MissionProgressRegistry.IsRunActive ||
                healingEvent == null ||
                !healingEvent.NonRegen ||
                healingEvent.ActualHealing <= 0f ||
                healingEvent.Body == null ||
                healingEvent.Body.teamComponent == null ||
                healingEvent.Body.teamComponent.teamIndex != TeamIndex.Player ||
                !MissionRuntimeActivityPlan.IsTypeActive(ObjectiveType) ||
                !MissionRuntimeCatalog.HasObjectiveType(ObjectiveType)
            )
            {
                return;
            }

            IReadOnlyList<MissionRecentSkillUse> recentUses =
                MissionSkillHistoryTracker.GetRecentUses(
                    AttributionWindowSeconds
                );

            if (recentUses == null || recentUses.Count == 0)
            {
                return;
            }

            IReadOnlyList<MissionObjectiveRuntimeBinding> bindings =
                MissionRuntimeCatalog.GetObjectives(ObjectiveType);

            for (int bindingIndex = 0;
                bindingIndex < bindings.Count;
                bindingIndex++)
            {
                MissionObjectiveRuntimeBinding binding =
                    bindings[bindingIndex];

                if (
                    binding == null ||
                    binding.Mission == null ||
                    binding.Objective == null
                )
                {
                    continue;
                }

                // Probar primero la skill compatible más reciente. En cuanto
                // una condición acepta la atribución, esta curación cuenta una
                // sola vez para la misión actual.
                for (int useIndex = 0;
                    useIndex < recentUses.Count;
                    useIndex++)
                {
                    MissionRecentSkillUse use =
                        recentUses[useIndex];

                    if (
                        use == null ||
                        use.PlayerMaster == null ||
                        use.PlayerBody == null ||
                        use.Skill == null
                    )
                    {
                        continue;
                    }

                    MissionEventContext context =
                        new MissionEventContext
                        {
                            PlayerMaster = use.PlayerMaster,
                            PlayerBody = use.PlayerBody,
                            ActionBody = use.PlayerBody,
                            TargetBody = healingEvent.Body,
                            ActivatedSkill = use.Skill,
                            EventType = ObjectiveType
                        };

                    MissionCompositionProgressResult result =
                        MissionCompositionProgressEvaluator.AddProgress(
                            binding.MissionId,
                            binding.Mission,
                            binding.RouteIndex,
                            binding.ObjectiveIndex,
                            context,
                            healingEvent.ActualHealing
                        );

                    if (result == null || !result.Accepted)
                    {
                        continue;
                    }

                    GenericMissionDispatcher.HandleProgressResult(
                        binding,
                        result,
                        $"Curación: {result.CurrentValue:0.##}/{result.RequiredValue:0.##}"
                    );

                    break;
                }
            }
        }
    }
}
