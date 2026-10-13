using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

using BepInEx.Logging;

using RoR2;

using UnityEngine.Networking;


namespace UniversalSurvivorUnlocks
{
    /*
     * =============================================================
     * KILL OBJECTIVE HANDLER
     * =============================================================
     *
     * Handler genérico para objetivos Mission Schema v2 de tipo:
     *
     *     Kill
     *
     * ESCUCHA:
     *
     *     GlobalEventManager.onCharacterDeathGlobal
     *
     * SOPORTA MEDIANTE KillObjectiveEvaluator:
     *
     * - Any
     * - Enemy / AnyEnemy
     * - Elite
     * - Boss
     * - SpecificBody
     * - SpecificBoss
     *
     * También atribuye correctamente bajas realizadas por drones
     * y otros minions al jugador propietario.
     *
     * Este handler NO concede el desbloqueo directamente.
     * Sólo alimenta el progreso compuesto y entrega el resultado a
     * GenericMissionDispatcher.
     * =============================================================
     */
    public static class KillObjectiveHandler
    {
        private const string ObjectiveType =
            "Kill";


        private static ManualLogSource logger;

        private static bool initialized;

        // Distinct bosses are counted per owner, per objective, for the current run.
        private static readonly Dictionary<string, HashSet<BodyIndex>> DistinctBosses =
            new Dictionary<string, HashSet<BodyIndex>>();

        private static void ResetDistinctBosses(Run run)
        {
            DistinctBosses.Clear();
        }

        private static bool MeetsItemRequirement(MissionObjective objective, CharacterBody playerBody)
        {
            JObject p = objective?.Parameters;
            string required = p?.Value<string>("requiredItem");
            if (string.IsNullOrWhiteSpace(required)) return true;
            Inventory inventory = playerBody?.inventory;
            if (inventory == null) return false;
            ItemIndex item = ItemCatalog.FindItemIndex(required);
            if (item == ItemIndex.None) return false;
            int count = p.Value<int?>("requiredItemCount") ?? 1;
            return inventory.GetItemCountPermanent(item) >= Math.Max(1, count);
        }



        // =========================================================
        // INITIALIZE
        // =========================================================

        public static void Initialize(
            ManualLogSource log
        )
        {
            if (initialized)
            {
                return;
            }


            initialized =
                true;

            logger =
                log;


            MissionRunLifecycle.RunStarted += ResetDistinctBosses;
            MissionRunLifecycle.RunDestroyed += ResetDistinctBosses;

            GameplayEventHub
                .CharacterDeath +=
                OnCharacterDeathGlobal;


            UsuLog.Verbose(
                logger,
                "[MISSION V2] Kill handler inicializado."
            );
        }


        // =========================================================
        // CHARACTER DEATH
        // =========================================================

        private static void OnCharacterDeathGlobal(
            DamageReport damageReport
        )
        {
            if (
                !NetworkServer.active ||
                Run.instance == null ||
                !MissionProgressRegistry.IsRunActive ||
                damageReport == null
            )
            {
                return;
            }


            /*
             * Filtro barato.
             *
             * Si ninguna misión efectiva usa Kill, no resolvemos
             * propietarios ni recorremos bindings.
             */
            if (
                !MissionRuntimeActivityPlan
                    .IsTypeActive(
                        ObjectiveType
                    ) ||
                !MissionRuntimeCatalog
                    .HasObjectiveType(
                        ObjectiveType
                    )
            )
            {
                return;
            }


            CharacterBody victimBody =
                damageReport.victimBody;


            if (victimBody == null)
            {
                return;
            }


            BlastAttack activeBlast;


            bool isExplosiveDamage =
                ExplosionKillTracker
                    .TryGetActiveBlast(
                        out activeBlast
                    );


            CharacterMaster attackerMaster =
                damageReport.attackerMaster;


            if (
                attackerMaster == null &&
                damageReport.attackerBody != null
            )
            {
                attackerMaster =
                    damageReport
                        .attackerBody
                        .master;
            }


            /*
             * Algunos BlastAttack no rellenan attackerMaster en el
             * DamageReport. Reutilizamos el mismo fallback del tracker
             * explosivo antiguo para no perder la autoría del jugador.
             */
            if (
                attackerMaster == null &&
                activeBlast != null &&
                activeBlast.attacker != null
            )
            {
                CharacterBody blastAttackerBody =
                    activeBlast
                        .attacker
                        .GetComponent<CharacterBody>();


                if (blastAttackerBody != null)
                {
                    attackerMaster =
                        blastAttackerBody.master;
                }
            }


            CharacterMaster playerMaster =
                PlayerOwnerResolver
                    .ResolveOwningPlayerMaster(
                        attackerMaster
                    );


            if (playerMaster == null)
            {
                return;
            }


            CharacterBody playerBody =
                playerMaster.GetBody();


            if (playerBody == null)
            {
                return;
            }


            MissionEventContext context =
                new MissionEventContext
                {
                    PlayerMaster =
                        playerMaster,

                    PlayerBody =
                        playerBody,

                    ActionBody =
                        damageReport.attackerBody ??
                        (
                            activeBlast != null &&
                            activeBlast.attacker != null
                                ? activeBlast
                                    .attacker
                                    .GetComponent<CharacterBody>()
                                : null
                        ) ??
                        playerBody,

                    TargetBody =
                        victimBody,

                    DamageReport =
                        damageReport,

                    EventType =
                        "Kill",

                    IsFatalHit =
                        true,

                    IsExplosiveDamage =
                        isExplosiveDamage,

                    BlastAttack =
                        activeBlast
                };


            IReadOnlyList<
                MissionObjectiveRuntimeBinding
            > bindings =
                MissionRuntimeCatalog
                    .GetObjectives(
                        ObjectiveType
                    );


            for (
                int bindingIndex = 0;
                bindingIndex < bindings.Count;
                bindingIndex++
            )
            {
                MissionObjectiveRuntimeBinding binding =
                    bindings[
                        bindingIndex
                    ];


                if (
                    binding == null ||
                    binding.Mission == null ||
                    binding.Objective == null
                )
                {
                    continue;
                }


                /*
                 * Aquí se comprueba la categoría del objetivo:
                 * enemigo, élite, jefe o Body específico.
                 *
                 * Las Conditions de la ruta se comprueban después,
                 * dentro de MissionCompositionProgressEvaluator,
                 * justo antes de escribir el progreso.
                 */
                if (
                    !KillObjectiveEvaluator
                        .Matches(
                            binding.Objective,
                            context
                        )
                )
                {
                    continue;
                }


                if (!MeetsItemRequirement(binding.Objective, playerBody))
                {
                    continue;
                }

                // Objetivo dependiente: la muerte NO puede contar antes
                // de que el otro objetivo de la misma ruta esté completo.
                // Este parámetro es opcional para no alterar las misiones
                // oficiales existentes.
                if (!MeetsPriorObjectiveRequirement(binding, playerMaster))
                {
                    continue;
                }

                bool distinct = binding.Objective.Parameters?
                    .Value<bool?>("distinctBossBodies") == true;
                string distinctKey = null;
                HashSet<BodyIndex> seen = null;
                if (distinct)
                {
                    if (!victimBody.isBoss) continue;
                    distinctKey = playerMaster.GetInstanceID().ToString() + ":" +
                        binding.MissionId + ":" + binding.RouteIndex + ":" + binding.ObjectiveIndex;
                    if (!DistinctBosses.TryGetValue(distinctKey, out seen))
                    {
                        seen = new HashSet<BodyIndex>();
                        DistinctBosses[distinctKey] = seen;
                    }
                    if (seen.Contains(victimBody.bodyIndex)) continue;
                }

                MissionCompositionProgressResult result =
                    MissionCompositionProgressEvaluator
                        .AddProgress(
                            binding.MissionId,
                            binding.Mission,
                            binding.RouteIndex,
                            binding.ObjectiveIndex,
                            context,
                            1d
                        );


                // Remember a species only if its kill passed the composition evaluator.
                if (distinct && result != null)
                {
                    seen.Add(victimBody.bodyIndex);
                }

                GenericMissionDispatcher
                    .HandleProgressResult(
                        binding,
                        result,
                        BuildDetails(
                            victimBody,
                            result
                        )
                    );
            }
        }


        private static bool MeetsPriorObjectiveRequirement(
            MissionObjectiveRuntimeBinding binding,
            CharacterMaster playerMaster)
        {
            string priorId = binding?.Objective?.Parameters?
                .Value<string>("afterObjectiveId");
            if (string.IsNullOrWhiteSpace(priorId))
            {
                return true;
            }

            if (binding?.Route?.Objectives == null)
            {
                return false;
            }

            MissionProgressScope scope =
                MissionProgressKeyBuilder.GetProgressScope(binding.Mission);
            NetworkUser user = scope == MissionProgressScope.Shared
                ? null
                : MissionPlayerIdentity.GetNetworkUser(playerMaster);
            if (scope == MissionProgressScope.PerPlayer && user == null)
            {
                return false;
            }

            for (int i = 0; i < binding.Route.Objectives.Count; i++)
            {
                MissionObjective candidate = binding.Route.Objectives[i];
                if (candidate == null ||
                    !string.Equals(candidate.Id, priorId,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string storageId = MissionProgressKeyBuilder.GetStorageObjectiveId(
                    binding.Route, binding.RouteIndex, candidate, i);
                return MissionProgressRegistry.IsObjectiveCompleted(
                    scope, user, binding.MissionId, storageId);
            }

            return false;
        }

        // =========================================================
        // DETAILS
        // =========================================================

        private static string BuildDetails(
            CharacterBody victimBody,
            MissionCompositionProgressResult result
        )
        {
            string victimName =
                "<desconocido>";


            if (victimBody != null)
            {
                string catalogName =
                    BodyCatalog.GetBodyName(
                        victimBody.bodyIndex
                    );


                victimName =
                    !string.IsNullOrWhiteSpace(
                        catalogName
                    )
                        ? catalogName
                        : victimBody.name;
            }


            if (result == null)
            {
                return
                    $"Víctima: {victimName}";
            }


            return
                $"Víctima: {victimName} | " +
                $"Bajas: {result.CurrentValue:0.##}/" +
                $"{result.RequiredValue:0.##}";
        }
    }
}
