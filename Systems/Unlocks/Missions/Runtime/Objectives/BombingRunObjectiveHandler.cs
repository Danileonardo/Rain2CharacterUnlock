using System;
using System.Collections.Generic;

using BepInEx.Logging;
using Newtonsoft.Json.Linq;
using RoR2;
using UnityEngine.Networking;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Objetivo reutilizable de "bombardeos": conseguir N bajas explosivas
    /// sin tocar el suelo, almacenar 1 ronda completada, aterrizar y repetir.
    /// El estado se separa por misión/objetivo/jugador para que el futuro
    /// editor pueda tener más de un BombingRun simultáneamente.
    /// </summary>
    public static class BombingRunObjectiveHandler
    {
        private const string ObjectiveType = "BombingRun";

        private sealed class ObjectiveState
        {
            public CharacterMaster Master;
            public int KillsInCurrentRun;
            public bool WaitingForLanding;
            public string LogKey;
        }

        private static readonly Dictionary<string, ObjectiveState> States =
            new Dictionary<string, ObjectiveState>(StringComparer.Ordinal);

        private static ManualLogSource logger;
        private static bool initialized;
        private static int fixedTick;

        public static void Initialize(ManualLogSource log)
        {
            if (initialized) return;
            initialized = true;
            logger = log;

            ExplosionKillTracker.LethalBlastDetected += OnLethalBlast;
            MissionStageRuntimeTracker.StageStarted += OnStageStarted;
            Run.onRunStartGlobal += OnRunStart;
            Run.onRunDestroyGlobal += OnRunEnd;
            RoR2Application.onFixedUpdate += OnFixedUpdate;

            logger?.LogInfo("[MISSION V2] BombingRun handler inicializado.");
        }

        private static void OnRunStart(Run run) => ClearStates();
        private static void OnRunEnd(Run run) => ClearStates();
        private static void OnStageStarted(MissionStageEventContext context) => ClearStates();

        private static void ClearStates()
        {
            foreach (ObjectiveState state in States.Values)
            {
                if (!string.IsNullOrWhiteSpace(state?.LogKey))
                {
                    MissionLogLimiter.ResetKey(state.LogKey);
                }
            }

            States.Clear();
            fixedTick = 0;
        }

        private static void OnLethalBlast(
            CharacterMaster playerMaster,
            DamageReport report,
            BlastAttack blast
        )
        {
            if (
                !NetworkServer.active ||
                Run.instance == null ||
                playerMaster == null ||
                report == null ||
                !MissionRuntimeActivityPlan.IsTypeActive(ObjectiveType) ||
                !MissionRuntimeCatalog.HasObjectiveType(ObjectiveType)
            )
            {
                return;
            }

            CharacterBody body = playerMaster.GetBody();
            if (body == null || body.characterMotor == null || body.characterMotor.isGrounded)
            {
                return;
            }

            IReadOnlyList<MissionObjectiveRuntimeBinding> bindings =
                MissionRuntimeCatalog.GetObjectives(ObjectiveType);

            for (int i = 0; i < bindings.Count; i++)
            {
                MissionObjectiveRuntimeBinding binding = bindings[i];
                if (binding?.Objective == null) continue;

                ObjectiveState state = GetState(binding, playerMaster);
                if (state.WaitingForLanding)
                {
                    continue;
                }

                int killsPerRun = ReadInt(binding.Objective.Parameters, "killsPerRun", 5);
                state.KillsInCurrentRun++;

                // Las rondas típicas son de 5, por lo que logueamos sólo
                // al completar la ronda; el detalle por baja desaparece.
                if (
                    state.KillsInCurrentRun >= killsPerRun &&
                    !SessionUnlockManager.AreAllLocalUsersUnlocked(binding.BodyName) &&
                    MissionLogLimiter.ShouldLogMilestone(
                        state.LogKey,
                        state.KillsInCurrentRun,
                        Math.Max(1, killsPerRun)
                    )
                )
                {
                    logger?.LogInfo(
                        $"[BombingRun] Bombardeo listo: {killsPerRun}/{killsPerRun} bajas"
                    );
                }

                if (state.KillsInCurrentRun < killsPerRun)
                {
                    continue;
                }

                state.KillsInCurrentRun = killsPerRun;
                state.WaitingForLanding = ReadBool(
                    binding.Objective.Parameters,
                    "requireLandingBetweenRuns",
                    true
                );

                MissionEventContext context = new MissionEventContext
                {
                    PlayerMaster = playerMaster,
                    PlayerBody = body,
                    ActionBody = report.attackerBody ?? body,
                    TargetBody = report.victimBody,
                    DamageReport = report,
                    EventType = ObjectiveType,
                    IsFatalHit = true,
                    IsExplosiveDamage = true,
                    BlastAttack = blast
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
                    result.MissionJustCompleted
                        ? "Bombardeo final completado"
                        : state.WaitingForLanding
                            ? "Bombardeo almacenado; aterriza para iniciar otro"
                            : "Bombardeo almacenado"
                );

                if (!state.WaitingForLanding)
                {
                    state.KillsInCurrentRun = 0;
                    MissionLogLimiter.ResetKey(state.LogKey);
                }
            }
        }

        private static void OnFixedUpdate()
        {
            if (
                !NetworkServer.active ||
                Run.instance == null ||
                States.Count == 0 ||
                !MissionRuntimeActivityPlan.IsTypeActive(ObjectiveType)
            )
            {
                return;
            }

            // Sólo 1 de cada 5 FixedUpdate y únicamente si existe una ronda
            // completada esperando aterrizaje.
            fixedTick++;
            if (fixedTick % 5 != 0)
            {
                return;
            }

            foreach (ObjectiveState state in States.Values)
            {
                if (state?.Master == null || !state.WaitingForLanding)
                {
                    continue;
                }

                CharacterBody body = state.Master.GetBody();
                if (body?.characterMotor == null || !body.characterMotor.isGrounded)
                {
                    continue;
                }

                state.WaitingForLanding = false;
                state.KillsInCurrentRun = 0;
                MissionLogLimiter.ResetKey(state.LogKey);
            }
        }

        private static ObjectiveState GetState(
            MissionObjectiveRuntimeBinding binding,
            CharacterMaster master
        )
        {
            string key =
                (binding.MissionId ?? "") + ":" +
                binding.RouteIndex + ":" +
                binding.ObjectiveIndex + ":" +
                master.GetInstanceID();

            if (!States.TryGetValue(key, out ObjectiveState state) || state == null)
            {
                state = new ObjectiveState
                {
                    Master = master,
                    LogKey = "bombing:" + key
                };

                States[key] = state;
            }

            return state;
        }

        private static int ReadInt(JObject parameters, string key, int fallback)
        {
            int value = parameters?.Value<int?>(key) ?? fallback;
            return value > 0 ? value : fallback;
        }

        private static bool ReadBool(JObject parameters, string key, bool fallback)
        {
            return parameters?.Value<bool?>(key) ?? fallback;
        }
    }
}
