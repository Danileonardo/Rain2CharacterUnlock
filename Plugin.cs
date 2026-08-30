using System.Collections.Generic;
using BepInEx;
using R2API.ContentManagement;
using R2API.Networking;
using RoR2;

namespace UniversalSurvivorUnlocks
{
    [BepInPlugin(
        PluginGuid,
        PluginName,
        PluginVersion
    )]

    [BepInDependency(
        R2APIContentManager.PluginGUID
    )]

    [BepInDependency(
    NetworkingAPI.PluginGUID
    )]

    public class Plugin :
        BaseUnityPlugin
    {
        public const string PluginGuid =
            "com.danileo.UniversalSurvivorUnlocks";


        public const string PluginName =
            "Universal Survivor Unlocks";


        public const string PluginVersion =
            "0.1.8";


        public static List<SurvivorInfo> Survivors
        {
            get;
            private set;
        }


        private void Awake()
        {
            NetworkingAPI
                .RegisterMessageType<
                    HunkBanditShotResultMessage
                >();


            NetworkingAPI
                .RegisterMessageType<
                    HunkRailgunnerShotResultMessage
                >();

            NetworkingAPI
                .RegisterMessageType<
                    SessionUnlockGrantMessage
                >();

            Logger.LogInfo(
                $"Universal Survivor Unlocks " +
                $"{PluginVersion} cargado."
            );


            // =====================================================
            // CONFIGURACIÓN
            // =====================================================

            SurvivorJsonManager
                .LoadForStartup(
                    Logger
                );


            // =====================================================
            // UNLOCKS CONOCIDOS DE ARRANQUES ANTERIORES
            // =====================================================

            SurvivorUnlockManager
                .RegisterConfiguredUnlockables(
                    Logger
                );


            // =====================================================
            // PROVIDER UNIVERSAL
            // =====================================================

            UniversalContentPackProvider
                .Initialize(
                    Logger
                );


            // =====================================================
            // VISUAL DE BLOQUEO
            // =====================================================

            SurvivorLockedPortraitManager
                .Initialize(
                    Logger
                );


            // =====================================================
            // FASE FINAL
            // =====================================================

            RoR2Application.onLoad +=
                OnGameLoaded;
        }


        private void OnGameLoaded()
        {
            Logger.LogInfo(
                "Risk of Rain 2 terminó de cargar."
            );

            StatusEffectScanner.Rebuild(
                Logger
            );

            StatusEffectTracker.Initialize(
                Logger
            );

            HealHealthTracker.Initialize(
                Logger
            );

            BossCriticalKillTracker.Initialize(
                Logger
            );

            HoldItemStackTracker.Initialize(
                Logger
            );

            BackstabBossKillTracker.Initialize(
                Logger
            );

            ExplosionKillTracker.Initialize(
                Logger
            );

            AirborneExplosionKillTracker.Initialize(
                Logger
            );

            PrecisionExecutionStreakTracker.Initialize(
                Logger
            );

            ScrapItemBossFinisherTracker.Initialize(
                Logger
            );

            SessionUnlockManager.Initialize(
                Logger
            );

            ReplayUnlockManager.Initialize(
                Logger
            );

            /*
             * NO hacemos Rebuild.
                         *
                         * Conservamos toda la información que
                         * recopilamos desde peerLoadInfos.
                         *
                         * Sólo cruzamos los BodyPrefabs contra
                         * el catálogo final como fallback.
                         */
            ModdedSurvivorRegistry
                .ReconcileCatalog(
                    Logger
                );


            Survivors =
                SurvivorDetector
                    .DetectSurvivors(
                        Logger
                    );


            /*
             * Persistimos en JSON las entradas
             * que fueron creadas en memoria.
             */
            SurvivorJsonManager
                .Sync(
                    Survivors,
                    Logger
                );


            /*
             * Aplicación final.
             */
            SurvivorUnlockManager
                .ApplyConfiguredUnlockables(
                    Survivors,
                    Logger
                );


            RoR2Application.onLoad -=
                OnGameLoaded;


            Logger.LogInfo(
                "Universal Survivor Unlocks terminó " +
                "su inicialización."
            );
        }
    }
}