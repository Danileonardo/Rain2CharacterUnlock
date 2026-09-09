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

    [BepInDependency(
        "com.rune580.riskofoptions",
        BepInDependency.DependencyFlags.SoftDependency
    )]

    public class Plugin :
        BaseUnityPlugin
    {
        public const string PluginGuid =
            "com.danileo.UniversalSurvivorUnlocks";


        public const string PluginName =
            "Universal Survivor Unlocks";


        public const string PluginVersion =
            "0.2.1";


        public static List<SurvivorInfo> Survivors
        {
            get;
            private set;
        }


        private void Awake()
        {
            // Logging.VerboseLogging=false por defecto:
            // mantiene la consola limpia sin perder warnings/errores.
            UsuLog.Initialize(
                Config
            );


            NetworkingAPI
                .RegisterMessageType<
                    HunkBanditShotResultMessage
                >();

            NetworkingAPI
                .RegisterMessageType<
                    SessionUnlockResultMessage
                >();

            NetworkingAPI
                .RegisterMessageType<
                    HunkRailgunnerShotResultMessage
                >();

            NetworkingAPI
                .RegisterMessageType<
                    SessionUnlockGrantMessage
                >();

            NetworkingAPI
                .RegisterMessageType<
                    SessionMissionSyncMessage
                >();

            NetworkingAPI
                .RegisterMessageType<
                    SessionMissionLobbyRequestMessage
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

            /*
             * 5G.2D-B
             *
             * Contextos runtime reutilizables para misiones:
             * TeleporterBoss / Elite / Umbra / Summon.
             */
            ContentRuntimeContextTracker.Initialize(
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

            KillEnemiesTracker.Initialize(
                Logger
            );

            SessionUnlockManager.Initialize(
                Logger
            );

            SessionMissionRegistry.Initialize(
                Logger
            );

            MissionRuntimeActivityPlan.Initialize(
                Logger
            );

            MissionRuntimeCatalog.Initialize(
                Logger
            );

            MissionPresetLibraryService.LogSummary(
                Logger
            );

            MissionRuntimeBudget.Initialize(
                Logger
            );

            MissionLogLimiter.Initialize(
                Logger
            );

            MissionProgressRegistry.Initialize(
                Logger
            );

            MissionStageRuntimeTracker.Initialize(
                Logger
            );

            GenericMissionDispatcher.Initialize(
                Logger
            );

            SessionMissionLobbySyncManager.Initialize(
                Logger
            );

            ChallengeCompletionRouter.Initialize(
                Logger
            );

            MissionRuntimeRefreshService.Initialize(
                Logger
            );

            MissionLibraryUIManager.Initialize(
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
             * Registro NUEVO de definiciones curadas.
             *
             * Fase 1: completamente pasivo. Sólo reconoce definitions cuyo
             * mod/body ya fue detectado por USU. No modifica localización,
             * providers, unlocks, misiones, JSON ni multiplayer.
             */
            SurvivorDefinitionRegistry
                .Initialize(
                    Survivors,
                    Logger
                );


            /*
             * Localización manual/curada de mods instalados.
             *
             * Los mods creadores ya ejecutaron Awake antes de este onLoad.
             * R2API conserva una traducción específica que ya exista, por lo
             * que Creator es-419/es-ES mantiene prioridad y USU sólo completa
             * los idiomas que el mod no registró.
             *
             * Se hace antes de construir Content Profiles para que la ficha de
             * USU capture desde el principio el mismo texto localizado que ve
             * la UI nativa del juego.
             */
            ModdedSurvivorLocalization
                .RegisterInstalledTranslations(
                    Survivors,
                    Logger
                );


            /*
             * 5G.2D-A
             *
             * Catálogo universal de contenido instalado. Esta capa es de
             * sólo lectura: no cambia providers, unlocks ni progreso.
             * Alimentará el navegador visual y el editor de misiones.
             */
            ContentCatalogService
                .Initialize(
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
             * 5G.1D-F
             *
             * BasePresetId es la autoridad de cualquier misión cuya
             * Source sea Preset. Si un preset oficial cambió desde la
             * última ejecución, actualizamos su snapshot persistido antes
             * de aplicar providers/runtime.
             */
            MissionAssignmentService
                .RefreshAssignedPresetSnapshots(
                    Logger
                );


            /*
             * Limpieza única de la asignación temporal utilizada durante
             * las pruebas Original <-> USU con Aurelion.
             *
             * No afecta futuras asignaciones voluntarias porque guarda un
             * marcador de migración en Survivors.json.
             */
            AurelionJhinTestCleanupMigration
                .Apply(
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


            /*
             * 5G.2D-C
             *
             * Construye la ficha contextual de cada survivor DESPUÉS de
             * aplicar el provider final. De este modo el navegador futuro
             * conoce el unlock runtime real y, a partir del personaje,
             * relaciona únicamente sus skills, skins, lore y requisitos.
             */
            SurvivorContentProfileService
                .Initialize(
                    Survivors,
                    Logger
                );


            /*
             * Auditoría TEMPORAL de cobertura de localización.
             * Se ejecuta después de construir los profiles para capturar
             * exactamente los textos/tokens visibles de cada survivor modded.
             * No registra traducciones ni modifica contenido.
             */
            LocalizationCoverageDiagnostics.Log(
                SurvivorContentProfileService.GetAll(),
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