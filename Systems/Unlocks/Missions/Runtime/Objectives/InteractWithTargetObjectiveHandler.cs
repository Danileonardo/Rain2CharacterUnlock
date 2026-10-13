using System;
using System.Collections.Generic;

using BepInEx.Logging;
using Newtonsoft.Json.Linq;
using RoR2;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Objetivo Mission v2 genérico para interacciones reales del jugador.
    ///
    /// No conoce Acolyte, Shrine of Descent ni ningún mod concreto. El objetivo
    /// se filtra por metadatos estables/configurables del objeto interactuado:
    /// - objectName;
    /// - body;
    /// - componentType.
    ///
    /// Si se configuran varios filtros se aplican como AND. Para evitar que un
    /// preset incompleto cuente cualquier cofre/terminal, al menos un filtro de
    /// destino debe estar configurado.
    /// </summary>
    public static class InteractWithTargetObjectiveHandler
    {
        private const string ObjectiveType = "InteractWithTarget";

        private static ManualLogSource logger;
        private static bool initialized;

        // Se conserva una cuenta por altar físico. Mezclar 1+2 usos
        // de distintos altares NO equivale a agotar uno solo.
        private sealed class SameTargetProgress
        {
            public readonly Dictionary<int, int> CountsByInstance =
                new Dictionary<int, int>();
            public int HighestCount;
        }

        private static readonly Dictionary<string, SameTargetProgress> SameTarget =
            new Dictionary<string, SameTargetProgress>(StringComparer.Ordinal);

        private static void ResetSameTarget(Run run) => SameTarget.Clear();

        public static void Initialize(ManualLogSource log)
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            logger = log;

            GameplayEventHub.InteractionPerformed += OnInteractionPerformed;
            GameplayEventHub.BloodShrinePurchased += OnBloodShrinePurchased;
            MissionRunLifecycle.RunStarted += ResetSameTarget;
            MissionRunLifecycle.RunDestroyed += ResetSameTarget;

            UsuLog.Verbose(
                logger,
                "[MISSION V2] InteractWithTarget handler inicializado."
            );
        }

        private static void OnInteractionPerformed(
            GameplayInteractionEvent interaction
        )
        {
            if (
                !NetworkServer.active ||
                Run.instance == null ||
                interaction == null ||
                interaction.PlayerMaster == null ||
                interaction.PlayerBody == null
            )
            {
                return;
            }

            if (
                !MissionRuntimeActivityPlan.IsTypeActive(ObjectiveType) ||
                !MissionRuntimeCatalog.HasObjectiveType(ObjectiveType)
            )
            {
                return;
            }

            if (
                MissionPlayerIdentity.GetNetworkUser(
                    interaction.PlayerMaster
                ) == null
            )
            {
                return;
            }

            IReadOnlyList<MissionObjectiveRuntimeBinding> bindings =
                MissionRuntimeCatalog.GetObjectives(ObjectiveType);

            for (int i = 0; i < bindings.Count; i++)
            {
                MissionObjectiveRuntimeBinding binding = bindings[i];
                MissionObjective objective = binding?.Objective;

                // Un Shrine of Blood usa el evento real de compra, no el
                // intento genérico de interacción: evita contarlo dos veces.
                if (objective?.Parameters?.Value<bool?>("bloodShrine") == true)
                {
                    continue;
                }

                if (
                    objective == null ||
                    !MatchesTarget(
                        objective.Parameters,
                        interaction
                    )
                )
                {
                    continue;
                }

                MissionEventContext context =
                    new MissionEventContext
                    {
                        PlayerMaster = interaction.PlayerMaster,
                        PlayerBody = interaction.PlayerBody,
                        ActionBody = interaction.PlayerBody,
                        TargetBody = interaction.TargetBody,
                        EventType = ObjectiveType
                    };

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
                    "Interacción: " +
                    (interaction.TargetObjectName ?? "<unknown>")
                );
            }
        }

        private static void OnBloodShrinePurchased(
            CharacterMaster master, CharacterBody body, int shrineInstanceId)
        {
            if (!NetworkServer.active || Run.instance == null ||
                master == null || body == null || shrineInstanceId == 0 ||
                MissionPlayerIdentity.GetNetworkUser(master) == null ||
                !MissionRuntimeActivityPlan.IsTypeActive(ObjectiveType) ||
                !MissionRuntimeCatalog.HasObjectiveType(ObjectiveType))
            {
                return;
            }

            IReadOnlyList<MissionObjectiveRuntimeBinding> bindings =
                MissionRuntimeCatalog.GetObjectives(ObjectiveType);

            for (int i = 0; i < bindings.Count; i++)
            {
                MissionObjectiveRuntimeBinding binding = bindings[i];
                MissionObjective objective = binding?.Objective;
                if (objective?.Parameters?.Value<bool?>("bloodShrine") != true ||
                    objective.Parameters.Value<bool?>("sameTargetInstance") != true)
                {
                    continue;
                }

                MissionProgressScope scope =
                    MissionProgressKeyBuilder.GetProgressScope(binding.Mission);
                NetworkUser user = MissionPlayerIdentity.GetNetworkUser(master);
                string identity = scope == MissionProgressScope.Shared
                    ? "shared" : user.GetInstanceID().ToString();
                string key = binding.MissionId + "|" +
                    binding.StorageObjectiveId + "|" + identity;

                SameTarget.TryGetValue(key, out SameTargetProgress state);
                if (state != null && state.HighestCount >= objective.Amount)
                {
                    continue;
                }

                int previousCount = 0;
                state?.CountsByInstance.TryGetValue(
                    shrineInstanceId, out previousCount);
                int count = previousCount + 1;
                int highest = Math.Max(count, state?.HighestCount ?? 0);

                var context = new MissionEventContext
                {
                    PlayerMaster = master,
                    PlayerBody = body,
                    ActionBody = body,
                    EventType = ObjectiveType
                };

                MissionCompositionProgressResult result =
                    MissionCompositionProgressEvaluator.SetProgress(
                        binding.MissionId, binding.Mission,
                        binding.RouteIndex, binding.ObjectiveIndex,
                        context, highest);

                if (result == null || !result.Accepted)
                {
                    continue;
                }

                if (state == null)
                {
                    state = new SameTargetProgress();
                    SameTarget[key] = state;
                }
                state.CountsByInstance[shrineInstanceId] = count;
                state.HighestCount = highest;

                GenericMissionDispatcher.HandleProgressResult(
                    binding, result,
                    "Santuario de sangre: " + highest +
                    "/" + objective.Amount);
            }
        }

        private static bool MatchesTarget(
            JObject parameters,
            GameplayInteractionEvent interaction
        )
        {
            if (interaction == null)
            {
                return false;
            }

            string expectedObject =
                ReadString(
                    parameters,
                    "objectName",
                    "targetObject",
                    "interactable"
                );

            string expectedBody =
                ReadString(
                    parameters,
                    "body",
                    "targetBody",
                    "bodyName"
                );

            string expectedComponent =
                ReadString(
                    parameters,
                    "componentType",
                    "component"
                );

            // Seguridad: un preset sin destino NO significa "cualquier
            // interacción". Debe configurarse al menos un criterio.
            if (
                string.IsNullOrWhiteSpace(expectedObject) &&
                string.IsNullOrWhiteSpace(expectedBody) &&
                string.IsNullOrWhiteSpace(expectedComponent)
            )
            {
                return false;
            }

            if (
                !string.IsNullOrWhiteSpace(expectedObject) &&
                !MatchesName(
                    expectedObject,
                    interaction.TargetObjectName,
                    parameters?.Value<string>("nameMatch")
                )
            )
            {
                return false;
            }

            if (
                !string.IsNullOrWhiteSpace(expectedBody) &&
                !string.Equals(
                    expectedBody.Trim(),
                    interaction.TargetBodyName ?? "",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return false;
            }

            if (
                !string.IsNullOrWhiteSpace(expectedComponent) &&
                !HasComponentType(
                    interaction.ComponentTypeNames,
                    expectedComponent
                )
            )
            {
                return false;
            }

            return true;
        }

        private static bool MatchesName(
            string expected,
            string actual,
            string mode
        )
        {
            expected = NormalizeObjectName(expected);
            actual = NormalizeObjectName(actual);

            if (
                string.IsNullOrWhiteSpace(expected) ||
                string.IsNullOrWhiteSpace(actual)
            )
            {
                return false;
            }

            string normalizedMode =
                string.IsNullOrWhiteSpace(mode)
                    ? "exact"
                    : mode.Trim().ToLowerInvariant();

            switch (normalizedMode)
            {
                case "contains":
                    return actual.IndexOf(
                        expected,
                        StringComparison.OrdinalIgnoreCase
                    ) >= 0;

                case "startswith":
                case "prefix":
                    return actual.StartsWith(
                        expected,
                        StringComparison.OrdinalIgnoreCase
                    );

                default:
                    return string.Equals(
                        expected,
                        actual,
                        StringComparison.OrdinalIgnoreCase
                    );
            }
        }

        private static bool HasComponentType(
            IReadOnlyList<string> types,
            string expected
        )
        {
            if (
                types == null ||
                string.IsNullOrWhiteSpace(expected)
            )
            {
                return false;
            }

            expected = expected.Trim();

            for (int i = 0; i < types.Count; i++)
            {
                string current = types[i];

                if (string.IsNullOrWhiteSpace(current))
                {
                    continue;
                }

                if (
                    string.Equals(
                        current,
                        expected,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    return true;
                }

                int lastDot = current.LastIndexOf('.');
                string shortName =
                    lastDot >= 0 && lastDot + 1 < current.Length
                        ? current.Substring(lastDot + 1)
                        : current;

                if (
                    string.Equals(
                        shortName,
                        expected,
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    return true;
                }
            }

            return false;
        }

        private static string ReadString(
            JObject parameters,
            params string[] keys
        )
        {
            if (parameters == null || keys == null)
            {
                return "";
            }

            for (int i = 0; i < keys.Length; i++)
            {
                string value = parameters.Value<string>(keys[i]);

                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }

            return "";
        }

        private static string NormalizeObjectName(
            string value
        )
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "";
            }

            string result = value.Trim();
            const string cloneSuffix = "(Clone)";

            if (
                result.EndsWith(
                    cloneSuffix,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                result = result.Substring(
                    0,
                    result.Length - cloneSuffix.Length
                ).Trim();
            }

            return result;
        }
    }
}
