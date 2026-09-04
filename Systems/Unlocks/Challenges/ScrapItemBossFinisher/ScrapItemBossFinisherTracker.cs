using System;
using System.Collections.Generic;

using BepInEx.Logging;

using RoR2;

using UnityEngine;
using UnityEngine.Networking;


namespace UniversalSurvivorUnlocks
{
    public static class ScrapItemBossFinisherTracker
    {
        private static ManualLogSource logger;

        private static bool initialized;


        // =========================================================
        // PROGRESO INDIVIDUAL DE SCRAP
        // =========================================================
        //
        // IMPORTANTE:
        //
        // Cada CharacterMaster tiene SU contador.
        //
        // Jugador A = 3
        // Jugador B = 3
        //
        // NO significa 6.
        //
        // =========================================================

        private static readonly Dictionary<
            CharacterMaster,
            int
        > ScrapConvertedByPlayer =
            new Dictionary<
                CharacterMaster,
                int
            >();


        // =========================================================
        // EVENTO DE MUERTE CANDIDATA
        // =========================================================
        //
        // Enviamos:
        //
        // CharacterMaster = jugador que realizó el golpe mortal
        // CharacterBody   = cuerpo atacante
        // CharacterBody   = víctima
        // int             = Scrap convertido por ESE jugador
        // int             = DamageSource del golpe mortal
        //
        // El ServerAchievement decidirá si:
        //
        // - era MUL-T
        // - era Alloy Worship Unit
        // - tenía Justicia demoledora
        // - tenía 6 Scrap convertidos
        // - provenía de Bote explosivo
        //
        // =========================================================

        public static event Action<
            CharacterMaster,
            CharacterBody,
            CharacterBody,
            DamageReport,
            int,
            int
        > PlayerKillDetected;


        // =========================================================
        // INICIALIZAR
        // =========================================================

        public static void Initialize(
            ManualLogSource pluginLogger
        )
        {
            if (initialized)
            {
                return;
            }


            initialized =
                true;


            logger =
                pluginLogger;


            // =====================================================
            // TRITURADORA
            // =====================================================
            //
            // En las versiones actuales de RoR2:
            //
            // BeginScrapping utiliza UniquePickup.
            //
            // Medimos cuántos objetos desaparecieron
            // realmente del inventario.
            //
            // =====================================================

            On.RoR2.ScrapperController
                .BeginScrapping_UniquePickup +=
                OnBeginScrapping;


            // =====================================================
            // MUERTES
            // =====================================================

            GlobalEventManager
                .onCharacterDeathGlobal +=
                OnCharacterDeathGlobal;


            // =====================================================
            // RUN
            // =====================================================

            Run.onRunStartGlobal +=
                OnRunStart;


            Run.onRunDestroyGlobal +=
                OnRunEnd;


            logger?.LogInfo(
                "ScrapItemBossFinisherTracker inicializado."
            );
        }


        // =========================================================
        // INICIO DE RUN
        // =========================================================

        private static void OnRunStart(
            Run run
        )
        {
            ScrapConvertedByPlayer.Clear();


            if (MissionRuntimeActivityPlan.IsTypeActive("ScrapItemBossFinisher"))
            {
                logger?.LogInfo(
                    "[TINKATON] Nueva run | Contadores reiniciados."
                );
            }
        }


        // =========================================================
        // FIN DE RUN
        // =========================================================

        private static void OnRunEnd(
            Run run
        )
        {
            ScrapConvertedByPlayer.Clear();
        }


        // =========================================================
        // TRITURADORA
        // =========================================================

        private static void OnBeginScrapping(
            On.RoR2.ScrapperController
                .orig_BeginScrapping_UniquePickup orig,

            ScrapperController self,

            UniquePickup pickupToTake
        )
        {
            if (!MissionRuntimeActivityPlan.IsTypeActive("ScrapItemBossFinisher"))
            {
                orig(self, pickupToTake);
                return;
            }

            // =====================================================
            // CLIENTE
            // =====================================================
            //
            // Toda la misión se controla desde el servidor.
            //
            // En cliente simplemente dejamos funcionar
            // la trituradora normalmente.
            //
            // =====================================================

            if (!NetworkServer.active)
            {
                orig(
                    self,
                    pickupToTake
                );


                return;
            }


            CharacterBody playerBody =
                null;


            CharacterMaster playerMaster =
                null;


            ItemIndex itemIndex =
                ItemIndex.None;


            int countBefore =
                0;


            bool canTrack =
                false;


            // =====================================================
            // ESTADO ANTES DE TRITURAR
            // =====================================================

            try
            {
                PickupDef pickupDef =
                    PickupCatalog.GetPickupDef(
                        pickupToTake.pickupIndex
                    );


                if (
                    pickupDef != null &&
                    pickupDef.itemIndex !=
                        ItemIndex.None &&
                    self != null &&
                    self.interactor != null
                )
                {
                    playerBody =
                        self
                            .interactor
                            .GetComponent<
                                CharacterBody
                            >();


                    if (playerBody != null)
                    {
                        playerMaster =
                            playerBody.master;
                    }


                    if (
                        playerMaster != null &&
                        playerMaster.inventory != null &&
                        PlayerOwnerResolver
                            .IsPlayerMaster(
                                playerMaster
                            )
                    )
                    {
                        itemIndex =
                            pickupDef.itemIndex;


                        countBefore =
                            playerMaster
                                .inventory
                                .GetItemCountPermanent(
                                    itemIndex
                                );


                        canTrack =
                            countBefore > 0;
                    }
                }
            }
            catch (Exception exception)
            {
                logger?.LogWarning(
                    "[TINKATON] No fue posible leer " +
                    "el estado previo de la trituradora | " +
                    exception.Message
                );
            }


            // =====================================================
            // EJECUTAR COMPORTAMIENTO ORIGINAL
            // =====================================================
            //
            // IMPORTANTE:
            //
            // orig() se llama EXACTAMENTE una vez.
            //
            // =====================================================

            orig(
                self,
                pickupToTake
            );


            // =====================================================
            // NO ERA UNA OPERACIÓN RASTREABLE
            // =====================================================

            if (
                !canTrack ||
                playerMaster == null ||
                playerMaster.inventory == null ||
                itemIndex == ItemIndex.None
            )
            {
                return;
            }


            // =====================================================
            // ESTADO DESPUÉS DE TRITURAR
            // =====================================================

            int countAfter =
                playerMaster
                    .inventory
                    .GetItemCountPermanent(
                        itemIndex
                    );


            int amountConverted =
                countBefore -
                countAfter;


            /*
             * Sólo contamos objetos que realmente
             * desaparecieron del inventario.
             */
            if (amountConverted <= 0)
            {
                return;
            }


            // =====================================================
            // SUMAR PROGRESO DEL JUGADOR
            // =====================================================

            int previousTotal =
                GetScrapConverted(
                    playerMaster
                );


            int newTotal =
                previousTotal +
                amountConverted;


            ScrapConvertedByPlayer[
                playerMaster
            ] =
                newTotal;


            string playerName =
                GetPlayerName(
                    playerMaster
                );


            ItemDef itemDef =
                ItemCatalog.GetItemDef(
                    itemIndex
                );


            string itemName =
                itemDef != null
                    ? itemDef.name
                    : itemIndex.ToString();


            logger?.LogInfo(
                "[TINKATON] CHATARRA CONVERTIDA | " +
                $"Jugador: {playerName} | " +
                $"Objeto: {itemName} | " +
                $"Esta operación: {amountConverted} | " +
                $"Total run: {newTotal}"
            );
        }


        // =========================================================
        // MUERTE
        // =========================================================

        private static void OnCharacterDeathGlobal(
            DamageReport damageReport
        )
        {
            if (!MissionRuntimeActivityPlan.IsTypeActive("ScrapItemBossFinisher"))
            {
                return;
            }


            if (!NetworkServer.active)
            {
                return;
            }


            if (Run.instance == null)
            {
                return;
            }


            if (damageReport == null)
            {
                return;
            }


            CharacterBody attackerBody =
                damageReport.attackerBody;


            CharacterBody victimBody =
                damageReport.victimBody;


            if (
                attackerBody == null ||
                victimBody == null
            )
            {
                return;
            }


            // =====================================================
            // ATACANTE REAL
            // =====================================================

            CharacterMaster playerMaster =
                damageReport.attackerMaster;


            if (
                playerMaster == null &&
                attackerBody.master != null
            )
            {
                playerMaster =
                    attackerBody.master;
            }


            /*
             * Aquí NO resolvemos minions.
             *
             * La misión exige que el propio jugador
             * realice el golpe final.
             *
             * Una torreta o drone no debe completar
             * la misión en nombre de MUL-T.
             */
            if (
                playerMaster == null ||
                !PlayerOwnerResolver
                    .IsPlayerMaster(
                        playerMaster
                    )
            )
            {
                return;
            }


            int scrapConverted =
                GetScrapConverted(
                    playerMaster
                );


            // =====================================================
            // ORIGEN DEL DAÑO
            // =====================================================

            int damageSourceRaw =
                (int)
                damageReport
                    .damageInfo
                    .damageType
                    .damageSource;


            // =====================================================
            // AVISAR AL SERVER ACHIEVEMENT
            // =====================================================

            PlayerKillDetected?.Invoke(
                playerMaster,
                attackerBody,
                victimBody,
                damageReport,
                scrapConverted,
                damageSourceRaw
            );
        }


        // =========================================================
        // OBTENER SCRAP DE UN JUGADOR
        // =========================================================

        public static int GetScrapConverted(
            CharacterMaster playerMaster
        )
        {
            if (playerMaster == null)
            {
                return 0;
            }


            if (
                ScrapConvertedByPlayer
                    .TryGetValue(
                        playerMaster,
                        out int amount
                    )
            )
            {
                return amount;
            }


            return 0;
        }


        // =========================================================
        // NOMBRE DEL JUGADOR
        // =========================================================

        private static string GetPlayerName(
            CharacterMaster playerMaster
        )
        {
            if (playerMaster == null)
            {
                return "<desconocido>";
            }


            foreach (
                PlayerCharacterMasterController controller
                in PlayerCharacterMasterController.instances
            )
            {
                if (
                    controller == null ||
                    controller.master !=
                        playerMaster
                )
                {
                    continue;
                }


                string displayName =
                    controller.GetDisplayName();


                if (
                    !string.IsNullOrWhiteSpace(
                        displayName
                    )
                )
                {
                    return displayName;
                }
            }


            return playerMaster.name;
        }
    }
}