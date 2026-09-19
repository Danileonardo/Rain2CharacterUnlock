using System;

using BepInEx.Logging;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    /// <summary>
    /// Punto único para eventos globales de gameplay que varios sistemas de USU
    /// consumen al mismo tiempo.
    ///
    /// El objetivo es evitar múltiples suscripciones directas al mismo hook de
    /// RoR2/MMHOOK sin cambiar la lógica de los trackers. Cada consumidor sigue
    /// aplicando sus propios filtros de autoridad, ActivityPlan y misión activa.
    /// </summary>
    public static class GameplayEventHub
    {
        private static ManualLogSource logger;
        private static bool initialized;

        private const float TenHzIntervalSeconds = 0.10f;
        private const float FiveHzIntervalSeconds = 0.20f;

        private static float nextTenHzTickTime;
        private static float nextFiveHzTickTime;

        public static event Action FixedUpdateTick;
        public static event Action TenHzTick;
        public static event Action FiveHzTick;
        public static event Action<DamageReport> CharacterDeath;
        public static event Action<CharacterMaster> InventoryChanged;
        public static event Action<ScrapperTransaction> ScrapperProcessed;

        public static void Initialize(ManualLogSource log)
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            logger = log;

            RoR2Application.onFixedUpdate += OnFixedUpdate;
            GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeathGlobal;
            On.RoR2.CharacterMaster.OnInventoryChanged += CharacterMaster_OnInventoryChanged;
            On.RoR2.ScrapperController.BeginScrapping_UniquePickup += ScrapperController_BeginScrapping;

            UsuLog.Verbose(
                logger,
                "[RUNTIME] GameplayEventHub inicializado."
            );
        }

        private static void OnFixedUpdate()
        {
            // Los consumidores que sólo necesitan polling periódico no deben
            // despertarse 50 veces por segundo. El hub conserva un tick de
            // precisión completa para aterrizajes/ventanas breves y ofrece
            // carriles de 10 Hz y 5 Hz para los escaneos Legacy.
            float now = UnityEngine.Time.unscaledTime;

            if (now >= nextTenHzTickTime)
            {
                nextTenHzTickTime = now + TenHzIntervalSeconds;
                TenHzTick?.Invoke();
            }

            if (now >= nextFiveHzTickTime)
            {
                nextFiveHzTickTime = now + FiveHzIntervalSeconds;
                FiveHzTick?.Invoke();
            }

            // Debe permanecer por tick físico: BombingRun, rachas aéreas y
            // la confirmación cliente de Lights Out dependen de contactos o
            // ventanas que pueden durar sólo un FixedUpdate.
            FixedUpdateTick?.Invoke();
        }

        private static void OnCharacterDeathGlobal(DamageReport report)
        {
            CharacterDeath?.Invoke(report);
        }

        private static void CharacterMaster_OnInventoryChanged(
            On.RoR2.CharacterMaster.orig_OnInventoryChanged orig,
            CharacterMaster self
        )
        {
            // Mantener el mismo contrato que los hooks anteriores: primero
            // ejecutamos RoR2/la cadena de otros mods y después notificamos a USU.
            orig(self);
            InventoryChanged?.Invoke(self);
        }

        private static void ScrapperController_BeginScrapping(
            On.RoR2.ScrapperController.orig_BeginScrapping_UniquePickup orig,
            ScrapperController self,
            UniquePickup pickupToTake
        )
        {
            // Sólo dos sistemas de USU necesitan observar la trituradora:
            // el objetivo V2 ScrapItems y el tracker Legacy de Tinkaton.
            // Si ninguno está activo, dejamos pasar el hook sin trabajo extra.
            bool shouldObserve =
                MissionRuntimeActivityPlan.IsTypeActive("ScrapItems") ||
                MissionRuntimeActivityPlan.IsTypeActive("ScrapItemBossFinisher");

            if (!shouldObserve)
            {
                orig(self, pickupToTake);
                return;
            }

            CharacterBody body = null;
            CharacterMaster master = null;
            ItemIndex itemIndex = ItemIndex.None;
            int before = 0;

            try
            {
                PickupDef pickupDef =
                    PickupCatalog.GetPickupDef(pickupToTake.pickupIndex);

                if (
                    pickupDef != null &&
                    pickupDef.itemIndex != ItemIndex.None &&
                    self != null &&
                    self.interactor != null
                )
                {
                    body = self.interactor.GetComponent<CharacterBody>();
                    master = body?.master;
                    itemIndex = pickupDef.itemIndex;

                    if (master != null && master.inventory != null)
                    {
                        before =
                            master.inventory.GetItemCountPermanent(itemIndex);
                    }
                }
            }
            catch (Exception ex)
            {
                logger?.LogWarning(
                    "[RUNTIME] No fue posible leer el estado previo de la trituradora | " +
                    ex.Message
                );
            }

            // El comportamiento original debe ejecutarse exactamente una vez.
            orig(self, pickupToTake);

            if (
                master == null ||
                master.inventory == null ||
                itemIndex == ItemIndex.None ||
                before <= 0
            )
            {
                return;
            }

            int after = master.inventory.GetItemCountPermanent(itemIndex);
            int converted = before - after;

            if (converted <= 0)
            {
                return;
            }

            ScrapperProcessed?.Invoke(
                new ScrapperTransaction(
                    body,
                    master,
                    itemIndex,
                    converted
                )
            );
        }
    }

    /// <summary>
    /// Resultado común de una operación real de trituradora observada por USU.
    /// AmountConverted representa objetos que desaparecieron efectivamente del
    /// inventario entre el estado previo y posterior a BeginScrapping.
    /// </summary>
    public sealed class ScrapperTransaction
    {
        public CharacterBody Body { get; }
        public CharacterMaster Master { get; }
        public ItemIndex ItemIndex { get; }
        public int AmountConverted { get; }

        public ScrapperTransaction(
            CharacterBody body,
            CharacterMaster master,
            ItemIndex itemIndex,
            int amountConverted
        )
        {
            Body = body;
            Master = master;
            ItemIndex = itemIndex;
            AmountConverted = amountConverted;
        }
    }
}
