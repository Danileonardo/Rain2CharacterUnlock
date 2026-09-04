using System;
using System.Collections.Generic;

using BepInEx.Logging;

using Newtonsoft.Json.Linq;

using RoR2;

using UnityEngine.Networking;


namespace UniversalSurvivorUnlocks
{
    /*
     * =============================================================
     * HOLD ITEM STACK OBJECTIVE HANDLER
     * =============================================================
     *
     * Reemplazo genérico FUTURO para el tracker especializado de
     * Energy Drink.
     *
     * IMPORTANTE:
     *
     * El tracker legacy NO se elimina todavía.
     *
     * Este handler sólo trabaja con Mission Schema v2.
     *
     * EVENTOS:
     *
     * 1. CharacterMaster.OnInventoryChanged
     *    -> sólo cuando cambia el inventario.
     *
     * 2. StageStarted
     *    -> una reconciliación pequeña por sector.
     *
     * No usa FixedUpdate.
     * No recorre el inventario completo.
     * Sólo consulta los ItemIndex requeridos por misiones activas.
     * =============================================================
     */
    public static class HoldItemStackObjectiveHandler
    {
        private const string ObjectiveType =
            "HoldItemStack";


        private static ManualLogSource logger;

        private static bool initialized;


        /*
         * Cache:
         *
         * item internal name
         *     -> ItemIndex
         *
         * ItemCatalog.FindItemIndex se realiza una vez por item
         * durante la run y no por cada cambio de inventario.
         */
        private static readonly Dictionary<
            string,
            ItemIndex
        > ItemIndexCache =
            new Dictionary<
                string,
                ItemIndex
            >(
                StringComparer.OrdinalIgnoreCase
            );


        private static readonly HashSet<string>
            WarnedMissingItems =
                new HashSet<string>(
                    StringComparer.OrdinalIgnoreCase
                );


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


            /*
             * Hook validado por el propio ecosistema de RoR2.
             *
             * Se ejecuta únicamente cuando CharacterMaster informa
             * que su inventario cambió.
             */
            On.RoR2.CharacterMaster
                .OnInventoryChanged +=
                CharacterMaster_OnInventoryChanged;


            /*
             * Si el jugador entra a un sector YA teniendo el item,
             * puede que no ocurra un InventoryChanged inmediatamente.
             *
             * Por eso hacemos una única reconciliación al entrar
             * en cada sector.
             */
            MissionStageRuntimeTracker
                .StageStarted +=
                OnStageStarted;


            Run.onRunStartGlobal +=
                OnRunStartGlobal;

            Run.onRunDestroyGlobal +=
                OnRunDestroyGlobal;


            logger?.LogInfo(
                "[MISSION V2] HoldItemStack handler inicializado."
            );
        }


        // =========================================================
        // RUN
        // =========================================================

        private static void OnRunStartGlobal(
            Run run
        )
        {
            ItemIndexCache.Clear();

            WarnedMissingItems.Clear();
        }


        private static void OnRunDestroyGlobal(
            Run run
        )
        {
            ItemIndexCache.Clear();

            WarnedMissingItems.Clear();
        }


        // =========================================================
        // INVENTORY EVENT
        // =========================================================

        private static void CharacterMaster_OnInventoryChanged(
            On.RoR2.CharacterMaster.orig_OnInventoryChanged orig,
            CharacterMaster self
        )
        {
            /*
             * Siempre respetamos primero el comportamiento vanilla.
             */
            orig(
                self
            );


            if (
                !NetworkServer.active ||
                Run.instance == null ||
                self == null
            )
            {
                return;
            }


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


            /*
             * Solamente masters pertenecientes a jugadores.
             */
            if (
                MissionPlayerIdentity
                    .GetNetworkUser(
                        self
                    ) == null
            )
            {
                return;
            }


            EvaluatePlayer(
                self
            );
        }


        // =========================================================
        // STAGE START RECONCILIATION
        // =========================================================

        private static void OnStageStarted(
            MissionStageEventContext context
        )
        {
            if (
                !NetworkServer.active ||
                Run.instance == null ||
                !MissionProgressRegistry.IsRunActive
            )
            {
                return;
            }


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


            /*
             * Esto NO es un scanner continuo.
             *
             * Ocurre una sola vez por sector y sólo sobre los
             * jugadores conectados.
             */
            EvaluateAllPlayers();
        }


        private static void EvaluateAllPlayers()
        {
            IReadOnlyList<
                MissionObjectiveRuntimeBinding
            > bindings =
                MissionRuntimeCatalog
                    .GetObjectives(
                        ObjectiveType
                    );


            /*
             * Primero resolvemos correctamente las misiones Shared.
             *
             * Para HoldItemStack la semántica por defecto es:
             *
             *     HighestEligiblePlayer
             *
             * NO sumamos inventarios de varios jugadores.
             *
             * Ejemplo:
             *
             * A = 1
             * B = 1
             *
             * objetivo 2
             *
             * resultado:
             *     1/2
             * NO 2/2.
             */
            for (
                int i = 0;
                i < bindings.Count;
                i++
            )
            {
                MissionObjectiveRuntimeBinding
                    binding =
                        bindings[i];


                if (
                    binding == null ||
                    binding.Mission == null
                )
                {
                    continue;
                }


                MissionProgressScope scope =
                    MissionProgressKeyBuilder
                        .GetProgressScope(
                            binding.Mission
                        );


                if (
                    scope !=
                        MissionProgressScope.Shared
                )
                {
                    continue;
                }


                EvaluateSharedBinding(
                    binding
                );
            }


            /*
             * PerPlayer.
             */
            foreach (
                PlayerCharacterMasterController controller
                in PlayerCharacterMasterController.instances
            )
            {
                CharacterMaster master =
                    controller?.master;


                if (master == null)
                {
                    continue;
                }


                EvaluatePlayer(
                    master,
                    onlyPerPlayer: true
                );
            }
        }


        // =========================================================
        // PLAYER
        // =========================================================

        private static void EvaluatePlayer(
            CharacterMaster master,
            bool onlyPerPlayer = false
        )
        {
            if (
                master == null ||
                master.inventory == null
            )
            {
                return;
            }


            MissionEventContext context =
                BuildContext(
                    master
                );


            IReadOnlyList<
                MissionObjectiveRuntimeBinding
            > bindings =
                MissionRuntimeCatalog
                    .GetObjectives(
                        ObjectiveType
                    );


            for (
                int i = 0;
                i < bindings.Count;
                i++
            )
            {
                MissionObjectiveRuntimeBinding
                    binding =
                        bindings[i];


                if (
                    binding == null ||
                    binding.Mission == null
                )
                {
                    continue;
                }


                MissionProgressScope scope =
                    MissionProgressKeyBuilder
                        .GetProgressScope(
                            binding.Mission
                        );


                if (
                    onlyPerPlayer &&
                    scope !=
                        MissionProgressScope.PerPlayer
                )
                {
                    continue;
                }


                /*
                 * Un InventoryChanged normal también puede actualizar
                 * Shared, pero siempre mediante HighestEligiblePlayer
                 * para no mezclar stacks de jugadores.
                 */
                if (
                    scope ==
                        MissionProgressScope.Shared
                )
                {
                    EvaluateSharedBinding(
                        binding
                    );

                    continue;
                }


                if (
                    !TryGetItemIndex(
                        binding,
                        out ItemIndex itemIndex
                    )
                )
                {
                    continue;
                }


                int currentCount =
                    master
                        .inventory
                        .GetItemCountPermanent(
                            itemIndex
                        );


                MissionCompositionProgressResult
                    result =
                        MissionCompositionProgressEvaluator
                            .SetProgress(
                                binding.MissionId,
                                binding.Mission,
                                binding.RouteIndex,
                                binding.ObjectiveIndex,
                                context,
                                currentCount
                            );


                GenericMissionDispatcher
                    .HandleProgressResult(
                        binding,
                        result,
                        $"Items: {currentCount}/{result.RequiredValue:0.##}"
                    );
            }
        }


        // =========================================================
        // SHARED
        // =========================================================

        private static void EvaluateSharedBinding(
            MissionObjectiveRuntimeBinding binding
        )
        {
            if (
                binding == null ||
                binding.Mission == null ||
                binding.Route == null
            )
            {
                return;
            }


            if (
                !TryGetItemIndex(
                    binding,
                    out ItemIndex itemIndex
                )
            )
            {
                return;
            }


            int highestCount =
                0;

            CharacterMaster bestMaster =
                null;

            bool foundEligiblePlayer =
                false;


            foreach (
                PlayerCharacterMasterController controller
                in PlayerCharacterMasterController.instances
            )
            {
                CharacterMaster master =
                    controller?.master;


                if (
                    master == null ||
                    master.inventory == null
                )
                {
                    continue;
                }


                MissionEventContext context =
                    BuildContext(
                        master
                    );


                /*
                 * Las conditions son por jugador.
                 *
                 * RequiredSurvivor AnyOf funciona aquí de manera
                 * natural.
                 */
                if (
                    !MissionConditionEvaluator
                        .AreSatisfied(
                            binding.Route.Conditions,
                            context
                        )
                )
                {
                    continue;
                }


                foundEligiblePlayer =
                    true;


                int currentCount =
                    master
                        .inventory
                        .GetItemCountPermanent(
                            itemIndex
                        );


                if (
                    bestMaster == null ||
                    currentCount >
                        highestCount
                )
                {
                    highestCount =
                        currentCount;

                    bestMaster =
                        master;
                }
            }


            /*
             * Si hay al menos un jugador elegible:
             * usamos al mejor de ellos como contexto.
             */
            if (
                foundEligiblePlayer &&
                bestMaster != null
            )
            {
                MissionCompositionProgressResult
                    result =
                        MissionCompositionProgressEvaluator
                            .SetProgress(
                                binding.MissionId,
                                binding.Mission,
                                binding.RouteIndex,
                                binding.ObjectiveIndex,
                                BuildContext(
                                    bestMaster
                                ),
                                highestCount
                            );


                GenericMissionDispatcher
                    .HandleProgressResult(
                        binding,
                        result,
                        $"Items shared/highest: {highestCount}/{result.RequiredValue:0.##}"
                    );


                return;
            }


            /*
             * Ningún jugador satisface actualmente las conditions.
             *
             * HoldItemStack representa ESTADO ACTUAL, no historial.
             *
             * Para Shared limpiamos el valor directamente.
             */
            MissionProgressResetScope resetScope =
                MissionProgressKeyBuilder
                    .GetResetScope(
                        binding.Objective
                    );


            MissionProgressRegistry
                .SetProgress(
                    MissionProgressScope.Shared,
                    null,
                    binding.MissionId,
                    binding.StorageObjectiveId,
                    0d,
                    resetScope
                );


            MissionProgressRegistry
                .SetObjectiveCompleted(
                    MissionProgressScope.Shared,
                    null,
                    binding.MissionId,
                    binding.StorageObjectiveId,
                    false,
                    resetScope
                );
        }


        // =========================================================
        // ITEM
        // =========================================================

        private static bool TryGetItemIndex(
            MissionObjectiveRuntimeBinding binding,
            out ItemIndex itemIndex
        )
        {
            itemIndex =
                ItemIndex.None;


            string itemName =
                ReadItemName(
                    binding?.Objective?.Parameters
                );


            if (
                string.IsNullOrWhiteSpace(
                    itemName
                )
            )
            {
                return false;
            }


            if (
                ItemIndexCache.TryGetValue(
                    itemName,
                    out itemIndex
                )
            )
            {
                return
                    itemIndex !=
                    ItemIndex.None;
            }


            itemIndex =
                ItemCatalog.FindItemIndex(
                    itemName
                );


            ItemIndexCache[
                itemName
            ] =
                itemIndex;


            if (
                itemIndex ==
                    ItemIndex.None
            )
            {
                if (
                    WarnedMissingItems.Add(
                        itemName
                    )
                )
                {
                    logger?.LogWarning(
                        "[MISSION V2] HoldItemStack no encontró ItemIndex | " +
                        $"Item: {itemName}"
                    );
                }


                return false;
            }


            return true;
        }


        private static string ReadItemName(
            JObject parameters
        )
        {
            if (parameters == null)
            {
                return "";
            }


            string[] keys =
            {
                "item",
                "itemName",
                "itemInternalName"
            };


            for (
                int i = 0;
                i < keys.Length;
                i++
            )
            {
                string value =
                    parameters
                        .Value<string>(
                            keys[i]
                        );


                if (
                    !string.IsNullOrWhiteSpace(
                        value
                    )
                )
                {
                    return value.Trim();
                }
            }


            return "";
        }


        private static MissionEventContext BuildContext(
            CharacterMaster master
        )
        {
            return new MissionEventContext
            {
                PlayerMaster =
                    master,

                PlayerBody =
                    master?.GetBody()
            };
        }
    }
}
